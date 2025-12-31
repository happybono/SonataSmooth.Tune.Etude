Imports System.Globalization
Imports System.Text
Imports SonataSmooth.Tune
Imports SonataSmooth.Tune.Export

Partial Public Class FrmMain
    ' Cache for asymmetric SG derivative coefficient vectors keyed by (left, right, polyOrder, derivative, deltaBits)
    Private Shared ReadOnly _sgAsymDerivCoeffCache As New Dictionary(Of Tuple(Of Integer, Integer, Integer, Integer, Long), Double())()
    Private Shared ReadOnly _sgAsymDerivCoeffCacheLock As New Object()

    Private _inBoundaryModeChange As Boolean
    Private _inDerivChange As Boolean
    Private _uiInitialized As Boolean

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _uiInitialized Then Return

        chkRectAvg.Checked = True
        chkBinoAvg.Checked = True
        chkBinoMedian.Checked = True
        chkGaussMed.Checked = True
        chkGauss.Checked = True
        chkSG.Checked = True

        ' Remove 32K limit so both sections can be displayed for large outputs
        txtInit.MaxLength = 0
        txtInit.Multiline = True
        txtInit.WordWrap = True

        txtRefined.MaxLength = 0
        txtRefined.Multiline = True
        txtRefined.WordWrap = True

        ' Ensure boundary mode combo population
        If cbxBoundaryMode.Items.Count = 0 Then
            cbxBoundaryMode.Items.AddRange(New Object() {"Symmetric", "Adaptive", "Replicate", "ZeroPad"})
        End If
        If cbxBoundaryMode.SelectedIndex < 0 Then
            cbxBoundaryMode.SelectedIndex = 0
        End If

        ' Derivative combo setup
        If cbxDeriv IsNot Nothing Then
            cbxDeriv.DropDownStyle = ComboBoxStyle.DropDownList
        End If

        ' Keep derivative options synchronized with poly-order
        AddHandler updPolyOrder.ValueChanged, AddressOf updPolyOrder_ValueChanged
        SyncDerivativeOptionsWithPolyOrder()

        ' Multi-selection checkboxes
        AddHandler chkRectAvg.CheckedChanged, AddressOf AlgoCheckBox_CheckedChanged
        AddHandler chkBinoAvg.CheckedChanged, AddressOf AlgoCheckBox_CheckedChanged
        AddHandler chkBinoMedian.CheckedChanged, AddressOf AlgoCheckBox_CheckedChanged
        AddHandler chkGaussMed.CheckedChanged, AddressOf AlgoCheckBox_CheckedChanged
        AddHandler chkGauss.CheckedChanged, AddressOf AlgoCheckBox_CheckedChanged
        AddHandler chkSG.CheckedChanged, AddressOf AlgoCheckBox_CheckedChanged

        ' Set default for cbxAlpha to 1.0
        If cbxAlpha IsNot Nothing AndAlso cbxAlpha.Items.Count > 0 Then
            Dim idx = cbxAlpha.Items.IndexOf("1.0")
            cbxAlpha.SelectedIndex = If(idx >= 0, idx, cbxAlpha.Items.Count - 1)
        End If

        ' Set default for cbxSigmaValue to 6.0 if not present
        Dim foundDefaultSigma = False
        For i = 0 To cbxSigmaValue.Items.Count - 1
            Dim val As Double
            If Double.TryParse(cbxSigmaValue.Items(i).ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, val) AndAlso Math.Abs(val - 6.0) < 0.000001 Then
                cbxSigmaValue.SelectedIndex = i
                foundDefaultSigma = True
                Exit For
            End If
        Next
        If Not foundDefaultSigma Then
            cbxSigmaValue.Items.Add("6.0")
            cbxSigmaValue.SelectedIndex = cbxSigmaValue.Items.Count - 1
        End If

        UpdateSigmaControlsEnabled()
        UpdateAlphaControlsEnabled()

        _uiInitialized = True
    End Sub

    Private Sub SyncDerivativeOptionsWithPolyOrder()
        If cbxDeriv Is Nothing Then Return
        Dim maxD = Math.Max(0, CInt(updPolyOrder.Value))
        Dim current = GetSelectedDerivativeOrder()

        cbxDeriv.BeginUpdate()
        Try
            cbxDeriv.Items.Clear()
            For i = 0 To maxD
                cbxDeriv.Items.Add(i.ToString(CultureInfo.InvariantCulture))
            Next
            Dim newSel = Math.Max(0, Math.Min(maxD, current))
            cbxDeriv.SelectedIndex = newSel
        Finally
            cbxDeriv.EndUpdate()
        End Try
    End Sub

    Private Sub updPolyOrder_ValueChanged(sender As Object, e As EventArgs)
        SyncDerivativeOptionsWithPolyOrder()
    End Sub

    Private Sub AlgoCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        UpdateSigmaControlsEnabled()
        UpdateAlphaControlsEnabled()
        UpdateDerivativeControlsEnabled()
    End Sub

    Private Sub cbxSigmaValue_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbxSigmaValue.SelectedIndexChanged
        Dim sigma = GetSelectedSigmaFactor().GetValueOrDefault(6.0)
        lblSigmaKernelWidth.Text = "w / "
    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Try
            slblStatus.Text = "Processing..."

            Dim parsed = ParseInputSeries(txtInit.Text)
            Dim values = parsed.values
            Dim warn = parsed.warn

            If values.Length = 0 Then
                Throw New ArgumentException("No input values. Enter numbers separated by whitespace.")
            End If
            If Not String.IsNullOrEmpty(warn) Then
                MessageBox.Show(warn, "Input Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            Dim doRect = chkRectAvg.Checked
            Dim doAvg = chkBinoAvg.Checked
            Dim doMed = chkBinoMedian.Checked
            Dim doGaussMed = chkGaussMed.Checked
            Dim doGauss = chkGauss.Checked
            Dim doSG = chkSG.Checked

            If Not (doRect OrElse doAvg OrElse doMed OrElse doGauss OrElse doSG) Then
                Throw New InvalidOperationException("Select at least one smoothing method.")
            End If

            Dim radius = CInt(updRadius.Value)
            Dim polyOrder = CInt(updPolyOrder.Value)
            Dim boundary = ParseBoundaryMode(TryCast(cbxBoundaryMode.SelectedItem, String))

            Dim derivOrder = GetSelectedDerivativeOrder()
            Dim delta = 1.0

            If doSG AndAlso derivOrder > polyOrder Then
                Throw New ArgumentException($"Savitzky–Golay polynomial order (polyOrder={polyOrder}) must be >= derivative order (d={derivOrder}).")
            End If

            Dim validation = ScoreConformanceChecker.Validate(values.Length, radius, polyOrder, doSG)
            If Not validation.Success Then
                MessageBox.Show(validation.Error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                slblStatus.Text = "Error"
                Return
            End If

            Dim alpha = GetSelectedAlpha()
            Dim sigmaFactor = GetSelectedSigmaFactor()

            ' Pass alpha and sigmaFactor to ApplySmoothing
            Dim result = SmoothingConductor.ApplySmoothing(
                input:=values,
                r:=radius,
                polyOrder:=polyOrder,
                boundaryMode:=boundary,
                doRect:=doRect,
                doAvg:=doAvg,
                doMed:=doMed,
                doGaussMed:=doGaussMed,
                doGauss:=doGauss,
                doSG:=doSG AndAlso derivOrder = 0,
                alpha:=alpha,
                sigmaFactor:=sigmaFactor
            )

            Dim sg = result.SG
            Dim sgLabel = "Savitzky-Golay"
            If doSG AndAlso derivOrder > 0 Then
                sg = SmoothingConductor.ApplySGDerivative(values, radius, polyOrder, derivOrder, delta, boundary)
                sgLabel = $"Savitzky-Golay d={derivOrder}"
            End If

            Dim sb As New StringBuilder()
            AppendSection(sb, "Rectangular Average", doRect, result.Rect)
            AppendSection(sb, "Binomial Average", doAvg, result.Binom)
            AppendSection(sb, "Binomial Median", doMed, result.Median)
            AppendSection(sb, "Gaussian Weighted Median", doGaussMed, result.GaussMed)
            AppendSection(sb, "Gaussian", doGauss, result.Gauss)
            AppendSection(sb, sgLabel, doSG, sg)

            txtRefined.Text = sb.ToString()
            slblStatus.Text = "Completed"
        Catch ex As Exception
            slblStatus.Text = "Error"
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Async Sub btnExcelExport_Click(sender As Object, e As EventArgs) Handles btnExcelExport.Click
        Try
            slblStatus.Text = "Preparing Excel..."

            Dim parsed = ParseInputSeries(txtInit.Text)
            Dim values = parsed.values
            Dim warn = parsed.warn

            If values.Length = 0 Then
                Throw New ArgumentException("No input values. Enter numbers separated by whitespace.")
            End If
            If Not String.IsNullOrEmpty(warn) Then
                MessageBox.Show(warn, "Input Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            Dim doRect = chkRectAvg.Checked
            Dim doAvg = chkBinoAvg.Checked
            Dim doMed = chkBinoMedian.Checked
            Dim doGauss = chkGauss.Checked
            Dim doGaussMed = chkGaussMed.Checked
            Dim doSG = chkSG.Checked

            If Not (doRect OrElse doAvg OrElse doMed OrElse doGauss OrElse doSG) Then
                Throw New InvalidOperationException("Select at least one smoothing method.")
            End If

            Dim radius = CInt(updRadius.Value)
            Dim polyOrder = CInt(updPolyOrder.Value)
            Dim boundary = ParseBoundaryMode(TryCast(cbxBoundaryMode.SelectedItem, String))
            Dim derivOrder = GetSelectedDerivativeOrder()
            If derivOrder < 0 Then derivOrder = 0

            If derivOrder > 0 AndAlso Not doSG Then
                doSG = True
            End If

            If doSG AndAlso derivOrder > polyOrder Then
                Throw New ArgumentException($"Savitzky–Golay polynomial order (polyOrder={polyOrder}) must be >= derivative order (d={derivOrder}).")
            End If

            Dim validation = ScoreConformanceChecker.Validate(values.Length, radius, polyOrder, doSG)
            If Not validation.Success Then
                MessageBox.Show(validation.Error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                slblStatus.Text = "Error"
                Return
            End If

            Dim flags = New SmoothingNotation() With {
                .Rectangular = doRect,
                .BinomialAverage = doAvg,
                .BinomialMedian = doMed,
                .GaussianMedian = doGaussMed,
                .Gaussian = doGauss,
                .SavitzkyGolay = doSG
            }

            Dim alpha = GetSelectedAlpha()
            Dim sigmaFactor = GetSelectedSigmaFactor()

            Dim req = New ExcelScoreRequest() With {
                .DatasetTitle = "SonataSmooth Test",
                .InitialData = values,
                .Radius = radius,
                .PolyOrder = polyOrder,
                .BoundaryMode = boundary,
                .Flags = flags,
                .DerivOrder = derivOrder,
                .Alpha = alpha,
                .OpenAfterExport = True,
                .SigmaFactor = sigmaFactor
            }

            If req.DerivOrder > 0 AndAlso Not req.Flags.SavitzkyGolay Then
                req.Flags.SavitzkyGolay = True
            End If
            If req.DerivOrder > 0 Then
                req.DatasetTitle = $"{req.DatasetTitle} (SG d={req.DerivOrder})"
            End If

            Dim progress = New Progress(Of Integer)(Sub(p) slblStatus.Text = $"Excel {p}%")
            Await ExcelScoreWriter.ExportAsync(req, progress)

            slblStatus.Text = "Excel export complete"
        Catch ex As Exception
            slblStatus.Text = "Error"
            MessageBox.Show(ex.Message, "Excel export error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ParseInputSeries(input As String) As (values As Double(), warn As String)
        ' Treat commas, spaces, tabs, and line breaks all as delimiters
        Dim tokens = System.Text.RegularExpressions.Regex.Split(If(input, String.Empty), "[\s,]+").
        Where(Function(t) Not String.IsNullOrWhiteSpace(t)).ToArray()
        Dim list As New List(Of Double)()
        Dim bads As New List(Of String)()
        For Each t In tokens
            Dim v As Double
            If Double.TryParse(t, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, v) OrElse
           Double.TryParse(t, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, v) Then
                list.Add(v)
            Else
                bads.Add(t)
            End If
        Next
        Dim warn As String = If(bads.Count > 0,
        $"The following tokens could not be parsed as numbers and were ignored: {String.Join(", ", bads)}",
        Nothing)
        Return (list.ToArray(), warn)
    End Function

    Private Function ParseBoundaryMode(name As String) As BoundaryMode
        If String.IsNullOrWhiteSpace(name) Then Return BoundaryMode.Symmetric
        Dim mode As BoundaryMode
        If [Enum].TryParse(name, True, mode) Then
            Return mode
        End If
        Select Case name.Trim()
            Case "Symmetric" : Return BoundaryMode.Symmetric
            Case "Adaptive" : Return BoundaryMode.Adaptive
            Case "Replicate" : Return BoundaryMode.Replicate
            Case "ZeroPad" : Return BoundaryMode.ZeroPad
            Case Else : Return BoundaryMode.Symmetric
        End Select
    End Function

    Private Shared Sub AppendSection(sb As StringBuilder, label As String, enabled As Boolean, data As Double())
        If Not enabled OrElse data Is Nothing Then Return
        If sb.Length > 0 Then sb.AppendLine()
        sb.AppendLine($"[{label}]")
        sb.AppendLine(String.Join(" ", data.Select(Function(v) v.ToString("G", CultureInfo.InvariantCulture))))
    End Sub
    Private Function GetSelectedAlpha() As Double
        If cbxAlpha.SelectedItem IsNot Nothing Then
            Dim alpha As Double
            If Double.TryParse(cbxAlpha.SelectedItem.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, alpha) Then
                Return alpha
            End If
        End If
        ' Default to 1.0 if not set or parse fails
        Return 1.0
    End Function

    Private Function GetSelectedSigmaFactor() As Double?
        If cbxSigmaValue.SelectedItem IsNot Nothing Then
            Dim sigma As Double
            If Double.TryParse(cbxSigmaValue.SelectedItem.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, sigma) Then
                Return sigma
            End If
        End If
        ' Return Nothing to use API default (6.0)
        Return Nothing
    End Function

    Private Sub UpdateSigmaControlsEnabled()
        ' Enable sigma controls if Gaussian or Gaussian Median is checked
        Dim enableSigma = chkGaussMed.Checked OrElse chkGauss.Checked
        lblSigmaValue.Enabled = enableSigma
        lblSigmaKernelWidth.Enabled = enableSigma
        cbxSigmaValue.Enabled = enableSigma
    End Sub

    Private Sub UpdateAlphaControlsEnabled()
        ' Enable alpha controls if any of the supported filters are checked
        Dim enableAlpha = chkBinoAvg.Checked OrElse chkBinoMedian.Checked OrElse chkGaussMed.Checked OrElse chkGauss.Checked
        cbxAlpha.Enabled = enableAlpha
        lblAlpha.Enabled = enableAlpha
    End Sub

    Private Sub UpdateDerivativeControlsEnabled()
        ' Enable derivative controls if SG is checked
        Dim enableDeriv = chkSG.Checked
        cbxDeriv.Enabled = enableDeriv
        lblDerivOrder.Enabled = enableDeriv
    End Sub

    Private Function GetSelectedDerivativeOrder() As Integer
        Try
            If cbxDeriv Is Nothing Then Return 0

            Dim si = cbxDeriv.SelectedItem
            If si IsNot Nothing Then
                If TypeOf si Is Integer Then
                    Return Math.Max(0, CInt(si))
                End If
                Dim s = si.ToString()
                If Not String.IsNullOrWhiteSpace(s) Then
                    Dim digits = New String(s.Where(AddressOf Char.IsDigit).ToArray())
                    Dim d As Integer
                    If Integer.TryParse(If(digits.Length > 0, digits, s), NumberStyles.Integer, CultureInfo.InvariantCulture, d) Then
                        Return Math.Max(0, d)
                    End If
                End If
            End If

            Dim txt = cbxDeriv.Text
            If Not String.IsNullOrWhiteSpace(txt) Then
                Dim digits = New String(txt.Where(AddressOf Char.IsDigit).ToArray())
                Dim d As Integer
                If Integer.TryParse(If(digits.Length > 0, digits, txt), NumberStyles.Integer, CultureInfo.InvariantCulture, d) Then
                    Return Math.Max(0, d)
                End If
            End If

            If cbxDeriv.SelectedIndex >= 0 AndAlso cbxDeriv.SelectedIndex < cbxDeriv.Items.Count Then
                Dim itemStr = TryCast(cbxDeriv.Items(cbxDeriv.SelectedIndex), String)
                Dim d As Integer
                If Integer.TryParse(itemStr, NumberStyles.Integer, CultureInfo.InvariantCulture, d) Then
                    Return Math.Max(0, d)
                End If
            End If
        Catch
            ' Ignore parsing failures
        End Try
        Return 0
    End Function

    Private Sub cbxBoundaryMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbxBoundaryMode.SelectedIndexChanged
        If _inBoundaryModeChange Then Return
        Try
            _inBoundaryModeChange = True

            If cbxBoundaryMode.Items.Count = 0 Then
                cbxBoundaryMode.Items.AddRange(New Object() {"Symmetric", "Adaptive", "Replicate", "ZeroPad"})
            End If

            Dim text = TryCast(cbxBoundaryMode.SelectedItem, String)
            If String.IsNullOrWhiteSpace(text) Then
                text = cbxBoundaryMode.Text
            End If

            Dim idx = -1
            For i = 0 To cbxBoundaryMode.Items.Count - 1
                Dim it = TryCast(cbxBoundaryMode.Items(i), String)
                If String.Equals(it, text, StringComparison.OrdinalIgnoreCase) Then
                    idx = i
                    Exit For
                End If
            Next
            If idx < 0 Then idx = 0

            If cbxBoundaryMode.SelectedIndex <> idx Then
                cbxBoundaryMode.SelectedIndex = idx
                Return
            End If

            cbxBoundaryMode.DropDownStyle = ComboBoxStyle.DropDownList
            slblStatus.Text = $"Boundary: {cbxBoundaryMode.SelectedItem}"
        Finally
            _inBoundaryModeChange = False
        End Try
    End Sub

    Private Sub cbxDeriv_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbxDeriv.SelectedIndexChanged
        If _inDerivChange Then Return
        Try
            _inDerivChange = True
            If cbxDeriv Is Nothing Then Return

            If Not cbxDeriv.Items.Contains("0") Then
                cbxDeriv.Items.Insert(0, "0")
            End If
            cbxDeriv.DropDownStyle = ComboBoxStyle.DropDownList

            Dim d = 0
            Dim text = TryCast(cbxDeriv.SelectedItem, String)
            If String.IsNullOrWhiteSpace(text) Then
                text = cbxDeriv.Text
            End If
            If Not String.IsNullOrWhiteSpace(text) Then
                Dim digits = New String(text.Where(AddressOf Char.IsDigit).ToArray())
                If Not Integer.TryParse(If(digits.Length > 0, digits, text), d) Then
                    d = 0
                End If
            End If

            Dim dStr = d.ToString(CultureInfo.InvariantCulture)
            Dim idx = -1
            For i = 0 To cbxDeriv.Items.Count - 1
                If String.Equals(TryCast(cbxDeriv.Items(i), String), dStr, StringComparison.OrdinalIgnoreCase) Then
                    idx = i
                    Exit For
                End If
            Next

            If idx < 0 Then
                Dim insertAt = 0
                For i = 0 To cbxDeriv.Items.Count - 1
                    Dim val As Integer
                    If Integer.TryParse(TryCast(cbxDeriv.Items(i), String), val) AndAlso val < d Then
                        insertAt = i + 1
                    End If
                Next
                cbxDeriv.Items.Insert(insertAt, dStr)
                idx = insertAt
            End If

            If cbxDeriv.SelectedIndex <> idx Then
                cbxDeriv.SelectedIndex = idx
                Return
            End If

            If Not chkSG.Checked AndAlso d > 0 Then
                slblStatus.Text = $"Derivative d={d} (enable SG to apply)."
            Else
                Dim p = CInt(updPolyOrder.Value)
                If chkSG.Checked AndAlso d > p Then
                    slblStatus.Text = $"Derivative d={d}; increase PolyOrder to ≥ {d}."
                Else
                    slblStatus.Text = $"Derivative d={d}"
                End If
            End If
        Finally
            _inDerivChange = False
        End Try
    End Sub

    Private Async Sub btnCSVExport_Click(sender As Object, e As EventArgs) Handles btnCSVExport.Click
        Try
            slblStatus.Text = "Preparing CSV..."

            Dim parsed = ParseInputSeries(txtInit.Text)
            Dim values = parsed.values
            Dim warn = parsed.warn

            If values.Length = 0 Then
                Throw New ArgumentException("No input values. Enter numbers separated by whitespace.")
            End If
            If Not String.IsNullOrEmpty(warn) Then
                MessageBox.Show(warn, "Input Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            Dim doRect = chkRectAvg.Checked
            Dim doAvg = chkBinoAvg.Checked
            Dim doMed = chkBinoMedian.Checked
            Dim doGaussMed = chkGaussMed.Checked
            Dim doGauss = chkGauss.Checked
            Dim doSG = chkSG.Checked

            If Not (doRect OrElse doAvg OrElse doMed OrElse doGauss OrElse doSG) Then
                Throw New InvalidOperationException("Select at least one smoothing method.")
            End If

            Dim radius = CInt(updRadius.Value)
            Dim polyOrder = CInt(updPolyOrder.Value)
            Dim boundary = ParseBoundaryMode(TryCast(cbxBoundaryMode.SelectedItem, String))
            Dim derivOrder = GetSelectedDerivativeOrder()
            If derivOrder < 0 Then derivOrder = 0
            Dim alpha = GetSelectedAlpha()
            Dim sigmaFactor = GetSelectedSigmaFactor()

            If derivOrder > 0 AndAlso Not doSG Then
                doSG = True
            End If

            If doSG AndAlso derivOrder > polyOrder Then
                Throw New ArgumentException($"Savitzky–Golay polynomial order (polyOrder={polyOrder}) must be >= derivative order (d={derivOrder}).")
            End If

            Dim validation = ScoreConformanceChecker.Validate(values.Length, radius, polyOrder, doSG)
            If Not validation.Success Then
                MessageBox.Show(validation.Error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                slblStatus.Text = "Error"
                Return
            End If

            Dim flags = New SmoothingNotation() With {
            .Rectangular = doRect,
            .BinomialAverage = doAvg,
            .BinomialMedian = doMed,
            .GaussianMedian = doGaussMed,
            .Gaussian = doGauss,
            .SavitzkyGolay = doSG
        }

            Dim filePath As String = Nothing
            Using sfd As New SaveFileDialog() With {
            .Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            .Title = "Save CSV File",
            .FileName = "SonataSmoothExport.csv"
        }
                If sfd.ShowDialog() = DialogResult.OK Then
                    filePath = sfd.FileName
                Else
                    slblStatus.Text = "CSV export canceled"
                    Return
                End If
            End Using

            Dim req = New CsvScoreRequest() With {
            .Title = "SonataSmooth Test",
            .InitialData = values,
            .Radius = radius,
            .PolyOrder = polyOrder,
            .BoundaryMode = boundary,
            .Flags = flags,
            .DerivOrder = derivOrder,
            .Alpha = alpha,
            .SigmaFactor = sigmaFactor,
            .BaseFilePath = IO.Path.GetFileNameWithoutExtension(filePath),
            .SavePath = filePath
        }

            Dim progress = New Progress(Of Integer)(Sub(p) slblStatus.Text = $"CSV {p}%")
            Await CsvScoreWriter.ExportAsync(req, progress, Threading.CancellationToken.None)

            slblStatus.Text = "CSV export complete"
        Catch ex As Exception
            slblStatus.Text = "Error"
            MessageBox.Show(ex.Message, "CSV export error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class