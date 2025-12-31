using SonataSmooth.Tune;
using SonataSmooth.Tune.Export;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SonataSmooth.Tune.Etude
{
    public partial class FrmMain : Form
    {
        // Cache for asymmetric SG derivative coefficient vectors keyed by (left, right, polyOrder, derivative, deltaBits)
        private static readonly Dictionary<Tuple<int, int, int, int, long>, double[]> _sgAsymDerivCoeffCache =
            new Dictionary<Tuple<int, int, int, int, long>, double[]>();
        private static readonly object _sgAsymDerivCoeffCacheLock = new object();

        private bool _inBoundaryModeChange;
        private bool _inDerivChange;
        private bool _uiInitialized;

        public FrmMain()
        {
            InitializeComponent();
            // All UI initialization moved to Form1_Load
            cbxSigmaValue.SelectedIndexChanged += (s, e) => UpdateSigmaKernelWidthLabel();

            // Set default for cbxAlpha to 1.0
            if (cbxAlpha != null && cbxAlpha.Items.Count > 0)
            {
                int idx = cbxAlpha.Items.IndexOf("1.0");
                cbxAlpha.SelectedIndex = idx >= 0 ? idx : cbxAlpha.Items.Count - 1;
            }
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (_uiInitialized) return;

            chkRectAvg.Checked = true;
            chkBinoAvg.Checked = true;
            chkBinoMedian.Checked = true;
            chkGaussMed.Checked = true;
            chkGaussMed.Checked = true;
            chkGauss.Checked = true;
            chkSG.Checked = true;

            // Remove 32K limit so both sections can be displayed for large outputs
            txtInit.MaxLength = 0;        // 0 = no limit
            txtInit.Multiline = true;     // ensure multiline (just in case)
            txtInit.WordWrap = true;

            txtRefined.MaxLength = 0;         // 0 = no limit
            txtRefined.Multiline = true;      // ensure multiline (just in case)
            txtRefined.WordWrap = true;

            // Ensure boundary mode combo population
            if (cbxBoundaryMode.Items.Count == 0)
                cbxBoundaryMode.Items.AddRange(new object[] { "Symmetric", "Adaptive", "Replicate", "ZeroPad" });
            if (cbxBoundaryMode.SelectedIndex < 0)
                cbxBoundaryMode.SelectedIndex = 0;

            // Derivative combo setup
            if (cbxDeriv != null)
                cbxDeriv.DropDownStyle = ComboBoxStyle.DropDownList;

            // Keep derivative options synchronized with poly-order
            updPolyOrder.ValueChanged += updPolyOrder_ValueChanged;
            SyncDerivativeOptionsWithPolyOrder();

            // Set default sigma value to 6.0 if not present, and update label
            bool foundDefaultSigma = false;
            for (int i = 0; i < cbxSigmaValue.Items.Count; i++)
            {
                if (double.TryParse(cbxSigmaValue.Items[i].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val) && Math.Abs(val - 6.0) < 1e-6)
                {
                    cbxSigmaValue.SelectedIndex = i;
                    foundDefaultSigma = true;
                    break;
                }
            }
            if (!foundDefaultSigma)
            {
                cbxSigmaValue.Items.Add("6.0");
                cbxSigmaValue.SelectedIndex = cbxSigmaValue.Items.Count - 1;
            }
            UpdateSigmaKernelWidthLabel();

            // Multi-selection checkboxes
            chkRectAvg.CheckedChanged += AlgoCheckBox_CheckedChanged;
            chkBinoAvg.CheckedChanged += AlgoCheckBox_CheckedChanged;
            chkBinoMedian.CheckedChanged += AlgoCheckBox_CheckedChanged;
            chkGaussMed.CheckedChanged += AlgoCheckBox_CheckedChanged;
            chkGauss.CheckedChanged += AlgoCheckBox_CheckedChanged;
            chkSG.CheckedChanged += AlgoCheckBox_CheckedChanged;

            // Sigma controls initial state
            UpdateSigmaControlsEnabled();

            // Alpha controls initial state
            UpdateAlphaControlsEnabled();

            // Derivative controls initial state
            UpdateDerivativeControlsEnabled();

            _uiInitialized = true;
        }

        private double? GetSelectedSigmaFactor()
        {
            if (cbxSigmaValue.SelectedItem != null &&
                double.TryParse(cbxSigmaValue.SelectedItem.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var sigma))
            {
                return sigma;
            }
            // Return null to use API default (6.0)
            return null;
        }

        private void UpdateSigmaControlsEnabled()
        {
            bool enableSigma = chkGaussMed.Checked || chkGauss.Checked;
            lblSigmaValue.Enabled = enableSigma;
            lblSigmaKernelWidth.Enabled = enableSigma;
            cbxSigmaValue.Enabled = enableSigma;
        }

        private void UpdateSigmaKernelWidthLabel()
        {
            var sigma = GetSelectedSigmaFactor() ?? 6.0;
            lblSigmaKernelWidth.Text = $"w / ";
        }

        private void SyncDerivativeOptionsWithPolyOrder()
        {
            if (cbxDeriv == null) return;

            int maxD = Math.Max(0, (int)updPolyOrder.Value);
            int current = GetSelectedDerivativeOrder();

            cbxDeriv.BeginUpdate();
            try
            {
                cbxDeriv.Items.Clear();
                for (int i = 0; i <= maxD; i++)
                    cbxDeriv.Items.Add(i.ToString(CultureInfo.InvariantCulture));

                int newSel = Math.Max(0, Math.Min(maxD, current));
                cbxDeriv.SelectedIndex = newSel;
            }
            finally
            {
                cbxDeriv.EndUpdate();
            }
        }

        private void updPolyOrder_ValueChanged(object sender, EventArgs e)
        {
            SyncDerivativeOptionsWithPolyOrder();
        }

        private void AlgoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSigmaControlsEnabled();
            UpdateAlphaControlsEnabled();
            UpdateDerivativeControlsEnabled();
        }

        private void UpdateDerivativeControlsEnabled()
        {
            // Only enable derivative order controls if Savitzky-Golay is checked
            bool enableDeriv = chkSG.Checked;
            cbxDeriv.Enabled = enableDeriv;
            lblDerivOrder.Enabled = enableDeriv; // label3 is the "Derivative Order :" label
        }

        private void UpdateAlphaControlsEnabled()
        {
            // Enable cbxAlpha if any supported filter is checked
            bool enableAlpha =
                chkBinoAvg.Checked ||
                chkBinoMedian.Checked ||
                chkGaussMed.Checked ||
                chkGauss.Checked;

            cbxAlpha.Enabled = enableAlpha;
            lblAlpha.Enabled = enableAlpha;
        }

        private double GetSelectedAlpha()
        {
            if (cbxAlpha.SelectedItem != null &&
                double.TryParse(cbxAlpha.SelectedItem.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var alpha))
            {
                return alpha;
            }
            // Default to 1.0 if not set or parse fails
            return 1.0;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                slblStatus.Text = "Processing...";

                double alpha = GetSelectedAlpha();

                var (values, warn) = ParseInputSeries(txtInit.Text);
                if (values.Length == 0)
                    throw new ArgumentException("No input values. Enter numbers separated by whitespace.");
                if (!string.IsNullOrEmpty(warn))
                    MessageBox.Show(warn, "Input Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                bool doRect = chkRectAvg.Checked;
                bool doAvg = chkBinoAvg.Checked;
                bool doMed = chkBinoMedian.Checked;
                bool doGaussMed = chkGaussMed.Checked;
                bool doGauss = chkGauss.Checked;
                bool doSG = chkSG.Checked;

                if (!(doRect || doAvg || doMed || doGaussMed || doGauss || doSG))
                    throw new InvalidOperationException("Select at least one smoothing method.");

                int radius = (int)updRadius.Value;
                int polyOrder = (int)updPolyOrder.Value;
                var boundary = ParseBoundaryMode(cbxBoundaryMode.SelectedItem as string);

                int derivOrder = GetSelectedDerivativeOrder();
                double delta = 1.0;
                double? sigmaFactor = GetSelectedSigmaFactor();

                if (doSG && derivOrder > polyOrder)
                    throw new ArgumentException($"Savitzky–Golay polynomial order (polyOrder={polyOrder}) must be >= derivative order (d={derivOrder}).");

                var validation = ScoreConformanceChecker.Validate(values.Length, radius, polyOrder, doSG);
                if (!validation.Success)
                {
                    MessageBox.Show(validation.Error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    slblStatus.Text = "Error";
                    return;
                }

                // Pass sigmaFactor to smoothing methods
                var (rect, binom, median, gaussMed, gauss, sgRaw) = SmoothingConductor.ApplySmoothing(
                    input: values,
                    r: radius,
                    polyOrder: polyOrder,
                    boundaryMode: boundary,
                    doRect: doRect,
                    doAvg: doAvg,
                    doMed: doMed,
                    doGaussMed: doGaussMed,
                    doGauss: doGauss,
                    doSG: doSG && derivOrder == 0,
                    alpha: alpha,
                    sigmaFactor: sigmaFactor
                );

                double[] sg = sgRaw;
                string sgLabel = "Savitzky-Golay";
                if (doSG && derivOrder > 0)
                {
                    sg = SmoothingConductor.ApplySGDerivative(values, radius, polyOrder, derivOrder, delta, boundary);
                    sgLabel = $"Savitzky-Golay d={derivOrder}";
                }

                var sb = new StringBuilder();
                AppendSection(sb, "Rectangular Average", doRect, rect);
                AppendSection(sb, "Binomial Average", doAvg, binom);
                AppendSection(sb, "Binomial Median", doMed, median);
                AppendSection(sb, "Gaussian Weighted Median", doGaussMed, gaussMed);
                AppendSection(sb, "Gaussian", doGauss, gauss);
                AppendSection(sb, sgLabel, doSG, sg);

                txtRefined.Text = sb.ToString();
                slblStatus.Text = "Completed";
            }
            catch (Exception ex)
            {
                slblStatus.Text = "Error";
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            await Task.CompletedTask;
        }

        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                slblStatus.Text = "Preparing Excel...";

                var (values, warn) = ParseInputSeries(txtInit.Text);
                if (values.Length == 0)
                    throw new ArgumentException("No input values. Enter numbers separated by whitespace.");
                if (!string.IsNullOrEmpty(warn))
                    MessageBox.Show(warn, "Input Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                bool doRect = chkRectAvg.Checked;
                bool doAvg = chkBinoAvg.Checked;
                bool doMed = chkBinoMedian.Checked;
                bool doGaussMed = chkGaussMed.Checked;
                bool doGauss = chkGauss.Checked;
                bool doSG = chkSG.Checked;

                if (!(doRect || doAvg || doMed || doGauss || doSG))
                    throw new InvalidOperationException("Select at least one smoothing method.");

                int radius = (int)updRadius.Value;
                int polyOrder = (int)updPolyOrder.Value;
                var boundary = ParseBoundaryMode(cbxBoundaryMode.SelectedItem as string);
                int derivOrder = GetSelectedDerivativeOrder();
                if (derivOrder < 0) derivOrder = 0;

                if (derivOrder > 0 && !doSG)
                    doSG = true;

                if (doSG && derivOrder > polyOrder)
                    throw new ArgumentException($"Savitzky–Golay polynomial order (polyOrder={polyOrder}) must be >= derivative order (d={derivOrder}).");

                var validation = ScoreConformanceChecker.Validate(values.Length, radius, polyOrder, doSG);
                if (!validation.Success)
                {
                    MessageBox.Show(validation.Error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    slblStatus.Text = "Error";
                    return;
                }

                var flags = new SmoothingNotation
                {
                    Rectangular = doRect,
                    BinomialAverage = doAvg,
                    BinomialMedian = doMed,
                    GaussianMedian = doGaussMed,
                    Gaussian = doGauss,
                    SavitzkyGolay = doSG
                };

                double alpha = GetSelectedAlpha();
                double? sigmaFactor = GetSelectedSigmaFactor();

                // Prompt user for save path
                string filePath = null;
                using (var sfd = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Save Excel File",
                    FileName = "SonataSmoothExport.xlsx"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                        filePath = sfd.FileName;
                    else
                    {
                        slblStatus.Text = "Excel export canceled";
                        return;
                    }
                }

                var req = new ExcelScoreRequest
                {
                    DatasetTitle = "SonataSmooth Test",
                    InitialData = values,
                    Radius = radius,
                    PolyOrder = polyOrder,
                    BoundaryMode = boundary,
                    Flags = flags,
                    DerivOrder = derivOrder,
                    Alpha = alpha,
                    OpenAfterExport = false,
                    SigmaFactor = sigmaFactor,
                    SavePath = filePath
                };

                if (req.DerivOrder > 0 && !req.Flags.SavitzkyGolay)
                    req.Flags.SavitzkyGolay = true;
                if (req.DerivOrder > 0)
                    req.DatasetTitle = $"{req.DatasetTitle} (SG d={req.DerivOrder})";

                var progress = new Progress<int>(p => slblStatus.Text = $"Excel {p}%");
                await ExcelScoreWriter.ExportAsync(req, progress);

                slblStatus.Text = "Excel export complete";
            }
            catch (Exception ex)
            {
                slblStatus.Text = "Error";
                MessageBox.Show(ex.Message, "Excel export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (double[] values, string warn)
        ParseInputSeries(string input)
        {
            // Treat commas, spaces, tabs, and line breaks all as delimiters
            var tokens = Regex.Split(input ?? string.Empty, @"[\s,]+")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            var list = new List<double>();
            var bads = new List<string>();
            foreach (var t in tokens)
            {
                if (double.TryParse(t, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v) ||
                    double.TryParse(t, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out v))
                {
                    list.Add(v);
                }
                else
                {
                    bads.Add(t);
                }
            }

            string warn = bads.Count > 0
                ? $"The following tokens could not be parsed as numbers and were ignored: {string.Join(", ", bads)}"
                : null;

            return (list.ToArray(), warn);
        }

        private BoundaryMode ParseBoundaryMode(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BoundaryMode.Symmetric;

            if (Enum.TryParse(name, true, out BoundaryMode mode))
                return mode;

            switch (name.Trim())
            {
                case "Symmetric": return BoundaryMode.Symmetric;
                case "Adaptive": return BoundaryMode.Adaptive;
                case "Replicate": return BoundaryMode.Replicate;
                case "ZeroPad": return BoundaryMode.ZeroPad;
                default: return BoundaryMode.Symmetric;
            }
        }

        private static void AppendSection(StringBuilder sb, string label, bool enabled, double[] data)
        {
            if (!enabled || data == null) return;
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine($"[{label}]");
            sb.AppendLine(string.Join(" ", data.Select(v => v.ToString("G", CultureInfo.InvariantCulture))));
        }

        private int GetSelectedDerivativeOrder()
        {
            try
            {
                if (cbxDeriv == null) return 0;

                var si = cbxDeriv.SelectedItem;
                if (si != null)
                {
                    if (si is int intVal) return Math.Max(0, intVal);
                    var s = si.ToString();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        var digits = new string(s.Where(char.IsDigit).ToArray());
                        if (int.TryParse(digits.Length > 0 ? digits : s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d1))
                            return Math.Max(0, d1);
                    }
                }

                var text = cbxDeriv.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var digits = new string(text.Where(char.IsDigit).ToArray());
                    if (int.TryParse(digits.Length > 0 ? digits : text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d2))
                        return Math.Max(0, d2);
                }

                if (cbxDeriv.SelectedIndex >= 0 && cbxDeriv.SelectedIndex < cbxDeriv.Items.Count)
                {
                    var itemStr = cbxDeriv.Items[cbxDeriv.SelectedIndex]?.ToString();
                    if (int.TryParse(itemStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var d3))
                        return Math.Max(0, d3);
                }
            }
            catch
            {
                // Ignore parsing errors, return 0
            }
            return 0;
        }

        private void cbxBoundaryMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inBoundaryModeChange) return;
            try
            {
                _inBoundaryModeChange = true;

                if (cbxBoundaryMode.Items.Count == 0)
                    cbxBoundaryMode.Items.AddRange(new object[] { "Symmetric", "Adaptive", "Replicate", "ZeroPad" });

                string text = cbxBoundaryMode.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(text))
                    text = cbxBoundaryMode.Text;

                int idx = -1;
                for (int i = 0; i < cbxBoundaryMode.Items.Count; i++)
                {
                    var it = cbxBoundaryMode.Items[i] as string;
                    if (string.Equals(it, text, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx < 0) idx = 0;

                if (cbxBoundaryMode.SelectedIndex != idx)
                {
                    cbxBoundaryMode.SelectedIndex = idx;
                    return;
                }

                cbxBoundaryMode.DropDownStyle = ComboBoxStyle.DropDownList;
                slblStatus.Text = $"Boundary : {cbxBoundaryMode.SelectedItem}";
            }
            finally
            {
                _inBoundaryModeChange = false;
            }
        }

        private void cbxDeriv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inDerivChange) return;
            try
            {
                _inDerivChange = true;

                if (cbxDeriv == null) return;

                if (!cbxDeriv.Items.Contains("0"))
                    cbxDeriv.Items.Insert(0, "0");
                cbxDeriv.DropDownStyle = ComboBoxStyle.DropDownList;

                int d = 0;
                string text = cbxDeriv.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(text))
                    text = cbxDeriv.Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var digits = new string(text.Where(char.IsDigit).ToArray());
                    if (!int.TryParse(digits.Length > 0 ? digits : text, out d))
                        d = 0;
                }

                string dStr = d.ToString(CultureInfo.InvariantCulture);
                int idx = -1;
                for (int i = 0; i < cbxDeriv.Items.Count; i++)
                {
                    if (string.Equals(cbxDeriv.Items[i] as string, dStr, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx < 0)
                {
                    int insertAt = 0;
                    for (int i = 0; i < cbxDeriv.Items.Count; i++)
                    {
                        if (int.TryParse(cbxDeriv.Items[i] as string, out var val) && val < d)
                            insertAt = i + 1;
                    }
                    cbxDeriv.Items.Insert(insertAt, dStr);
                    idx = insertAt;
                }

                if (cbxDeriv.SelectedIndex != idx)
                {
                    cbxDeriv.SelectedIndex = idx;
                    return;
                }

                if (!chkSG.Checked && d > 0)
                {
                    slblStatus.Text = $"Derivative d={d} (enable SG to apply).";
                }
                else
                {
                    int p = (int)updPolyOrder.Value;
                    if (chkSG.Checked && d > p)
                        slblStatus.Text = $"Derivative d={d}; increase PolyOrder to ≥ {d}.";
                    else
                        slblStatus.Text = $"Derivative d={d}";
                }
            }
            finally
            {
                _inDerivChange = false;
            }
        }

        private async void btnExportCSV_Click(object sender, EventArgs e)
        {
            try
            {
                slblStatus.Text = "Preparing CSV...";

                var (values, warn) = ParseInputSeries(txtInit.Text);
                if (values.Length == 0)
                    throw new ArgumentException("No input values. Enter numbers separated by whitespace.");
                if (!string.IsNullOrEmpty(warn))
                    MessageBox.Show(warn, "Input Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                bool doRect = chkRectAvg.Checked;
                bool doAvg = chkBinoAvg.Checked;
                bool doMed = chkBinoMedian.Checked;
                bool doGaussMed = chkGaussMed.Checked;
                bool doGauss = chkGauss.Checked;
                bool doSG = chkSG.Checked;

                if (!(doRect || doAvg || doMed || doGauss || doGaussMed || doSG))
                    throw new InvalidOperationException("Select at least one smoothing method.");

                int radius = (int)updRadius.Value;
                int polyOrder = (int)updPolyOrder.Value;
                var boundary = ParseBoundaryMode(cbxBoundaryMode.SelectedItem as string);
                int derivOrder = GetSelectedDerivativeOrder();
                if (derivOrder < 0) derivOrder = 0;
                double alpha = GetSelectedAlpha();
                double? sigmaFactor = GetSelectedSigmaFactor();

                if (derivOrder > 0 && !doSG)
                    doSG = true;

                if (doSG && derivOrder > polyOrder)
                    throw new ArgumentException($"Savitzky–Golay polynomial order (polyOrder={polyOrder}) must be >= derivative order (d={derivOrder}).");

                var validation = ScoreConformanceChecker.Validate(values.Length, radius, polyOrder, doSG);
                if (!validation.Success)
                {
                    MessageBox.Show(validation.Error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    slblStatus.Text = "Error";
                    return;
                }

                var flags = new SmoothingNotation
                {
                    Rectangular = doRect,
                    BinomialAverage = doAvg,
                    BinomialMedian = doMed,
                    GaussianMedian = doGaussMed,
                    Gaussian = doGauss,
                    SavitzkyGolay = doSG
                };

                string filePath = null;
                using (var sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    Title = "Save CSV File",
                    FileName = "SonataSmoothExport.csv"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                        filePath = sfd.FileName;
                    else
                    {
                        slblStatus.Text = "CSV export canceled";
                        return;
                    }
                }

                var req = new CsvScoreRequest
                {
                    Title = "SonataSmooth Test",
                    InitialData = values,
                    Radius = radius,
                    PolyOrder = polyOrder,
                    BoundaryMode = boundary,
                    Flags = flags,
                    DerivOrder = derivOrder,
                    Alpha = alpha,
                    SigmaFactor = sigmaFactor,
                    BaseFilePath = Path.GetFileNameWithoutExtension(filePath),
                    SavePath = filePath
                };

                var progress = new Progress<int>(p => slblStatus.Text = $"CSV {p}%");
                await CsvScoreWriter.ExportAsync(req, progress, System.Threading.CancellationToken.None);

                slblStatus.Text = "CSV export complete";
            }
            catch (Exception ex)
            {
                slblStatus.Text = "Error";
                MessageBox.Show(ex.Message, "CSV export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}