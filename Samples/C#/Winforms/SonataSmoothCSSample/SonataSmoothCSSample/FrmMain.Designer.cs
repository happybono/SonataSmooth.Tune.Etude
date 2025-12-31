namespace SonataSmooth.Tune.Etude
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.lblBoundaryMode = new System.Windows.Forms.Label();
            this.lblKernelRadius = new System.Windows.Forms.Label();
            this.lblPolyOrder = new System.Windows.Forms.Label();
            this.cbxDeriv = new System.Windows.Forms.ComboBox();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.cbxBoundaryMode = new System.Windows.Forms.ComboBox();
            this.updRadius = new System.Windows.Forms.NumericUpDown();
            this.updPolyOrder = new System.Windows.Forms.NumericUpDown();
            this.chkGauss = new System.Windows.Forms.CheckBox();
            this.chkBinoMedian = new System.Windows.Forms.CheckBox();
            this.chkBinoAvg = new System.Windows.Forms.CheckBox();
            this.chkRectAvg = new System.Windows.Forms.CheckBox();
            this.lblDerivOrder = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.slblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbSmoothingMethods = new System.Windows.Forms.GroupBox();
            this.chkGaussMed = new System.Windows.Forms.CheckBox();
            this.chkSG = new System.Windows.Forms.CheckBox();
            this.txtInit = new System.Windows.Forms.TextBox();
            this.txtRefined = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblSigmaKernelWidth = new System.Windows.Forms.Label();
            this.cbxSigmaValue = new System.Windows.Forms.ComboBox();
            this.lblSigmaValue = new System.Windows.Forms.Label();
            this.btnExportCSV = new System.Windows.Forms.Button();
            this.lblAlpha = new System.Windows.Forms.Label();
            this.cbxAlpha = new System.Windows.Forms.ComboBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.gbParameters = new System.Windows.Forms.GroupBox();
            this.gbInitData = new System.Windows.Forms.GroupBox();
            this.gbRefData = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.updRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updPolyOrder)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.gbSmoothingMethods.SuspendLayout();
            this.gbParameters.SuspendLayout();
            this.gbInitData.SuspendLayout();
            this.gbRefData.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBoundaryMode
            // 
            this.lblBoundaryMode.AutoSize = true;
            this.lblBoundaryMode.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblBoundaryMode.Location = new System.Drawing.Point(18, 64);
            this.lblBoundaryMode.Name = "lblBoundaryMode";
            this.lblBoundaryMode.Size = new System.Drawing.Size(132, 19);
            this.lblBoundaryMode.TabIndex = 41;
            this.lblBoundaryMode.Text = "Boundary Handling :";
            // 
            // lblKernelRadius
            // 
            this.lblKernelRadius.AutoSize = true;
            this.lblKernelRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblKernelRadius.Location = new System.Drawing.Point(18, 31);
            this.lblKernelRadius.Name = "lblKernelRadius";
            this.lblKernelRadius.Size = new System.Drawing.Size(90, 19);
            this.lblKernelRadius.TabIndex = 39;
            this.lblKernelRadius.Text = "Kernel Radius";
            // 
            // lblPolyOrder
            // 
            this.lblPolyOrder.AutoSize = true;
            this.lblPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblPolyOrder.Location = new System.Drawing.Point(18, 97);
            this.lblPolyOrder.Name = "lblPolyOrder";
            this.lblPolyOrder.Size = new System.Drawing.Size(113, 19);
            this.lblPolyOrder.TabIndex = 38;
            this.lblPolyOrder.Text = "Polynomial Order";
            // 
            // cbxDeriv
            // 
            this.cbxDeriv.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDeriv.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.cbxDeriv.FormattingEnabled = true;
            this.cbxDeriv.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cbxDeriv.Location = new System.Drawing.Point(166, 161);
            this.cbxDeriv.Name = "cbxDeriv";
            this.cbxDeriv.Size = new System.Drawing.Size(120, 25);
            this.cbxDeriv.TabIndex = 37;
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.btnExportExcel.Location = new System.Drawing.Point(582, 548);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(140, 32);
            this.btnExportExcel.TabIndex = 36;
            this.btnExportExcel.Text = "Export to Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // cbxBoundaryMode
            // 
            this.cbxBoundaryMode.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.cbxBoundaryMode.FormattingEnabled = true;
            this.cbxBoundaryMode.Items.AddRange(new object[] {
            "Symmetric",
            "Adaptive",
            "Replicate",
            "ZeroPad"});
            this.cbxBoundaryMode.Location = new System.Drawing.Point(166, 62);
            this.cbxBoundaryMode.Name = "cbxBoundaryMode";
            this.cbxBoundaryMode.Size = new System.Drawing.Size(120, 25);
            this.cbxBoundaryMode.TabIndex = 35;
            this.cbxBoundaryMode.SelectedIndexChanged += new System.EventHandler(this.cbxBoundaryMode_SelectedIndexChanged);
            // 
            // updRadius
            // 
            this.updRadius.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.updRadius.Location = new System.Drawing.Point(166, 29);
            this.updRadius.Maximum = new decimal(new int[] {
            13,
            0,
            0,
            0});
            this.updRadius.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.updRadius.Name = "updRadius";
            this.updRadius.Size = new System.Drawing.Size(120, 25);
            this.updRadius.TabIndex = 34;
            this.updRadius.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // updPolyOrder
            // 
            this.updPolyOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.updPolyOrder.Location = new System.Drawing.Point(166, 95);
            this.updPolyOrder.Maximum = new decimal(new int[] {
            13,
            0,
            0,
            0});
            this.updPolyOrder.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updPolyOrder.Name = "updPolyOrder";
            this.updPolyOrder.Size = new System.Drawing.Size(120, 25);
            this.updPolyOrder.TabIndex = 33;
            this.updPolyOrder.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.updPolyOrder.ValueChanged += new System.EventHandler(this.updPolyOrder_ValueChanged);
            // 
            // chkGauss
            // 
            this.chkGauss.AutoSize = true;
            this.chkGauss.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.chkGauss.Location = new System.Drawing.Point(56, 150);
            this.chkGauss.Name = "chkGauss";
            this.chkGauss.Size = new System.Drawing.Size(116, 23);
            this.chkGauss.TabIndex = 3;
            this.chkGauss.Text = "Gaussian Filter";
            this.chkGauss.UseVisualStyleBackColor = true;
            // 
            // chkBinoMedian
            // 
            this.chkBinoMedian.AutoSize = true;
            this.chkBinoMedian.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.chkBinoMedian.Location = new System.Drawing.Point(56, 98);
            this.chkBinoMedian.Name = "chkBinoMedian";
            this.chkBinoMedian.Size = new System.Drawing.Size(128, 23);
            this.chkBinoMedian.TabIndex = 2;
            this.chkBinoMedian.Text = "Binomial Median";
            this.chkBinoMedian.UseVisualStyleBackColor = true;
            // 
            // chkBinoAvg
            // 
            this.chkBinoAvg.AutoSize = true;
            this.chkBinoAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.chkBinoAvg.Location = new System.Drawing.Point(56, 72);
            this.chkBinoAvg.Name = "chkBinoAvg";
            this.chkBinoAvg.Size = new System.Drawing.Size(133, 23);
            this.chkBinoAvg.TabIndex = 1;
            this.chkBinoAvg.Text = "Binomial Average";
            this.chkBinoAvg.UseVisualStyleBackColor = true;
            // 
            // chkRectAvg
            // 
            this.chkRectAvg.AutoSize = true;
            this.chkRectAvg.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.chkRectAvg.Location = new System.Drawing.Point(56, 46);
            this.chkRectAvg.Name = "chkRectAvg";
            this.chkRectAvg.Size = new System.Drawing.Size(154, 23);
            this.chkRectAvg.TabIndex = 0;
            this.chkRectAvg.Text = "Rectangular Average";
            this.chkRectAvg.UseVisualStyleBackColor = true;
            // 
            // lblDerivOrder
            // 
            this.lblDerivOrder.AutoSize = true;
            this.lblDerivOrder.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblDerivOrder.Location = new System.Drawing.Point(18, 163);
            this.lblDerivOrder.Name = "lblDerivOrder";
            this.lblDerivOrder.Size = new System.Drawing.Size(114, 19);
            this.lblDerivOrder.TabIndex = 40;
            this.lblDerivOrder.Text = "Derivative Order :";
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.BackColor = System.Drawing.Color.Crimson;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 599);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(899, 24);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 32;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // slblStatus
            // 
            this.slblStatus.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.slblStatus.ForeColor = System.Drawing.Color.White;
            this.slblStatus.Name = "slblStatus";
            this.slblStatus.Size = new System.Drawing.Size(44, 19);
            this.slblStatus.Text = "Ready";
            // 
            // gbSmoothingMethods
            // 
            this.gbSmoothingMethods.Controls.Add(this.chkGaussMed);
            this.gbSmoothingMethods.Controls.Add(this.chkSG);
            this.gbSmoothingMethods.Controls.Add(this.chkGauss);
            this.gbSmoothingMethods.Controls.Add(this.chkBinoMedian);
            this.gbSmoothingMethods.Controls.Add(this.chkBinoAvg);
            this.gbSmoothingMethods.Controls.Add(this.chkRectAvg);
            this.gbSmoothingMethods.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold);
            this.gbSmoothingMethods.Location = new System.Drawing.Point(38, 61);
            this.gbSmoothingMethods.Name = "gbSmoothingMethods";
            this.gbSmoothingMethods.Size = new System.Drawing.Size(304, 229);
            this.gbSmoothingMethods.TabIndex = 31;
            this.gbSmoothingMethods.TabStop = false;
            this.gbSmoothingMethods.Text = "Smoothing Methods";
            // 
            // chkGaussMed
            // 
            this.chkGaussMed.AutoSize = true;
            this.chkGaussMed.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.chkGaussMed.Location = new System.Drawing.Point(56, 124);
            this.chkGaussMed.Name = "chkGaussMed";
            this.chkGaussMed.Size = new System.Drawing.Size(192, 23);
            this.chkGaussMed.TabIndex = 5;
            this.chkGaussMed.Text = "Gaussian Weighted Median";
            this.chkGaussMed.UseVisualStyleBackColor = true;
            // 
            // chkSG
            // 
            this.chkSG.AutoSize = true;
            this.chkSG.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.chkSG.Location = new System.Drawing.Point(56, 176);
            this.chkSG.Name = "chkSG";
            this.chkSG.Size = new System.Drawing.Size(151, 23);
            this.chkSG.TabIndex = 4;
            this.chkSG.Text = "Savitzky-Golay Filter";
            this.chkSG.UseVisualStyleBackColor = true;
            // 
            // txtInit
            // 
            this.txtInit.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.txtInit.Location = new System.Drawing.Point(6, 28);
            this.txtInit.Multiline = true;
            this.txtInit.Name = "txtInit";
            this.txtInit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInit.Size = new System.Drawing.Size(490, 190);
            this.txtInit.TabIndex = 29;
            // 
            // txtRefined
            // 
            this.txtRefined.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.txtRefined.Location = new System.Drawing.Point(6, 28);
            this.txtRefined.Multiline = true;
            this.txtRefined.Name = "txtRefined";
            this.txtRefined.ReadOnly = true;
            this.txtRefined.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRefined.Size = new System.Drawing.Size(490, 190);
            this.txtRefined.TabIndex = 30;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.btnStart.Location = new System.Drawing.Point(38, 548);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(304, 32);
            this.btnStart.TabIndex = 28;
            this.btnStart.Text = "Start Smoothing";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblSigmaKernelWidth
            // 
            this.lblSigmaKernelWidth.AutoSize = true;
            this.lblSigmaKernelWidth.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblSigmaKernelWidth.Location = new System.Drawing.Point(164, 197);
            this.lblSigmaKernelWidth.Name = "lblSigmaKernelWidth";
            this.lblSigmaKernelWidth.Size = new System.Drawing.Size(28, 19);
            this.lblSigmaKernelWidth.TabIndex = 42;
            this.lblSigmaKernelWidth.Text = "w /";
            // 
            // cbxSigmaValue
            // 
            this.cbxSigmaValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSigmaValue.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.cbxSigmaValue.FormattingEnabled = true;
            this.cbxSigmaValue.Items.AddRange(new object[] {
            "1.0",
            "2.0",
            "3.0",
            "4.0",
            "5.0",
            "6.0",
            "7.0",
            "8.0",
            "9.0",
            "10.0",
            "11.0",
            "12.0",
            "13.0",
            "14.0",
            "15.0",
            "16.0",
            "17.0",
            "18.0"});
            this.cbxSigmaValue.Location = new System.Drawing.Point(197, 194);
            this.cbxSigmaValue.Name = "cbxSigmaValue";
            this.cbxSigmaValue.Size = new System.Drawing.Size(89, 25);
            this.cbxSigmaValue.TabIndex = 43;
            // 
            // lblSigmaValue
            // 
            this.lblSigmaValue.AutoSize = true;
            this.lblSigmaValue.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblSigmaValue.Location = new System.Drawing.Point(18, 196);
            this.lblSigmaValue.Name = "lblSigmaValue";
            this.lblSigmaValue.Size = new System.Drawing.Size(89, 19);
            this.lblSigmaValue.TabIndex = 44;
            this.lblSigmaValue.Text = "Sigma Value :";
            // 
            // btnExportCSV
            // 
            this.btnExportCSV.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.btnExportCSV.Location = new System.Drawing.Point(728, 548);
            this.btnExportCSV.Name = "btnExportCSV";
            this.btnExportCSV.Size = new System.Drawing.Size(140, 32);
            this.btnExportCSV.TabIndex = 45;
            this.btnExportCSV.Text = "Export to CSV";
            this.btnExportCSV.UseVisualStyleBackColor = true;
            this.btnExportCSV.Click += new System.EventHandler(this.btnExportCSV_Click);
            // 
            // lblAlpha
            // 
            this.lblAlpha.AutoSize = true;
            this.lblAlpha.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.lblAlpha.Location = new System.Drawing.Point(18, 130);
            this.lblAlpha.Name = "lblAlpha";
            this.lblAlpha.Size = new System.Drawing.Size(87, 19);
            this.lblAlpha.TabIndex = 46;
            this.lblAlpha.Text = "Alpha Blend :";
            // 
            // cbxAlpha
            // 
            this.cbxAlpha.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAlpha.Font = new System.Drawing.Font("Segoe UI Variable Display", 10F);
            this.cbxAlpha.FormattingEnabled = true;
            this.cbxAlpha.Items.AddRange(new object[] {
            "0.1",
            "0.2",
            "0.3",
            "0.4",
            "0.5",
            "0.6",
            "0.7",
            "0.8",
            "0.9",
            "1.0"});
            this.cbxAlpha.Location = new System.Drawing.Point(166, 128);
            this.cbxAlpha.Name = "cbxAlpha";
            this.cbxAlpha.Size = new System.Drawing.Size(120, 25);
            this.cbxAlpha.TabIndex = 47;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 17F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(31, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(290, 31);
            this.lblTitle.TabIndex = 48;
            this.lblTitle.Text = "SonataSmooth.Tune.Etude";
            // 
            // gbParameters
            // 
            this.gbParameters.Controls.Add(this.lblPolyOrder);
            this.gbParameters.Controls.Add(this.lblDerivOrder);
            this.gbParameters.Controls.Add(this.cbxAlpha);
            this.gbParameters.Controls.Add(this.lblAlpha);
            this.gbParameters.Controls.Add(this.lblKernelRadius);
            this.gbParameters.Controls.Add(this.lblSigmaValue);
            this.gbParameters.Controls.Add(this.lblBoundaryMode);
            this.gbParameters.Controls.Add(this.cbxSigmaValue);
            this.gbParameters.Controls.Add(this.lblSigmaKernelWidth);
            this.gbParameters.Controls.Add(this.updPolyOrder);
            this.gbParameters.Controls.Add(this.updRadius);
            this.gbParameters.Controls.Add(this.cbxBoundaryMode);
            this.gbParameters.Controls.Add(this.cbxDeriv);
            this.gbParameters.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F);
            this.gbParameters.Location = new System.Drawing.Point(38, 303);
            this.gbParameters.Name = "gbParameters";
            this.gbParameters.Size = new System.Drawing.Size(304, 229);
            this.gbParameters.TabIndex = 51;
            this.gbParameters.TabStop = false;
            this.gbParameters.Text = "Smoothing Parameters";
            // 
            // gbInitData
            // 
            this.gbInitData.Controls.Add(this.txtInit);
            this.gbInitData.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbInitData.Location = new System.Drawing.Point(366, 61);
            this.gbInitData.Name = "gbInitData";
            this.gbInitData.Size = new System.Drawing.Size(502, 229);
            this.gbInitData.TabIndex = 52;
            this.gbInitData.TabStop = false;
            this.gbInitData.Text = "Initial Data";
            // 
            // gbRefData
            // 
            this.gbRefData.Controls.Add(this.txtRefined);
            this.gbRefData.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbRefData.Location = new System.Drawing.Point(366, 303);
            this.gbRefData.Name = "gbRefData";
            this.gbRefData.Size = new System.Drawing.Size(502, 229);
            this.gbRefData.TabIndex = 53;
            this.gbRefData.TabStop = false;
            this.gbRefData.Text = "Refined Data";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(899, 623);
            this.Controls.Add(this.gbRefData);
            this.Controls.Add(this.gbInitData);
            this.Controls.Add(this.gbParameters);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnExportCSV);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gbSmoothingMethods);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SonataSmooth.Tune.Etude";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.updRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updPolyOrder)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbSmoothingMethods.ResumeLayout(false);
            this.gbSmoothingMethods.PerformLayout();
            this.gbParameters.ResumeLayout(false);
            this.gbParameters.PerformLayout();
            this.gbInitData.ResumeLayout(false);
            this.gbInitData.PerformLayout();
            this.gbRefData.ResumeLayout(false);
            this.gbRefData.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBoundaryMode;
        private System.Windows.Forms.Label lblKernelRadius;
        private System.Windows.Forms.Label lblPolyOrder;
        private System.Windows.Forms.ComboBox cbxDeriv;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.ComboBox cbxBoundaryMode;
        private System.Windows.Forms.NumericUpDown updRadius;
        private System.Windows.Forms.NumericUpDown updPolyOrder;
        private System.Windows.Forms.CheckBox chkGauss;
        private System.Windows.Forms.CheckBox chkBinoMedian;
        private System.Windows.Forms.CheckBox chkBinoAvg;
        private System.Windows.Forms.CheckBox chkRectAvg;
        private System.Windows.Forms.Label lblDerivOrder;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel slblStatus;
        private System.Windows.Forms.GroupBox gbSmoothingMethods;
        private System.Windows.Forms.CheckBox chkSG;
        private System.Windows.Forms.TextBox txtInit;
        private System.Windows.Forms.TextBox txtRefined;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.CheckBox chkGaussMed;
        private System.Windows.Forms.Label lblSigmaKernelWidth;
        private System.Windows.Forms.ComboBox cbxSigmaValue;
        private System.Windows.Forms.Label lblSigmaValue;
        private System.Windows.Forms.Button btnExportCSV;
        private System.Windows.Forms.Label lblAlpha;
        private System.Windows.Forms.ComboBox cbxAlpha;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox gbParameters;
        private System.Windows.Forms.GroupBox gbInitData;
        private System.Windows.Forms.GroupBox gbRefData;
    }
}