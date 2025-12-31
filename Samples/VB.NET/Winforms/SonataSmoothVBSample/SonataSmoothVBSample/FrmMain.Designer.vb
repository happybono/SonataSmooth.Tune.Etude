<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmMain))
        Me.label4 = New System.Windows.Forms.Label()
        Me.label2 = New System.Windows.Forms.Label()
        Me.label1 = New System.Windows.Forms.Label()
        Me.cbxDeriv = New System.Windows.Forms.ComboBox()
        Me.btnExcelExport = New System.Windows.Forms.Button()
        Me.cbxBoundaryMode = New System.Windows.Forms.ComboBox()
        Me.updRadius = New System.Windows.Forms.NumericUpDown()
        Me.updPolyOrder = New System.Windows.Forms.NumericUpDown()
        Me.slblStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblDerivOrder = New System.Windows.Forms.Label()
        Me.statusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.chkSG = New System.Windows.Forms.CheckBox()
        Me.chkGauss = New System.Windows.Forms.CheckBox()
        Me.chkBinoMedian = New System.Windows.Forms.CheckBox()
        Me.chkBinoAvg = New System.Windows.Forms.CheckBox()
        Me.gbSmoothingMethods = New System.Windows.Forms.GroupBox()
        Me.chkGaussMed = New System.Windows.Forms.CheckBox()
        Me.chkRectAvg = New System.Windows.Forms.CheckBox()
        Me.txtRefined = New System.Windows.Forms.TextBox()
        Me.txtInit = New System.Windows.Forms.TextBox()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.lblSigmaValue = New System.Windows.Forms.Label()
        Me.cbxSigmaValue = New System.Windows.Forms.ComboBox()
        Me.lblSigmaKernelWidth = New System.Windows.Forms.Label()
        Me.cbxAlpha = New System.Windows.Forms.ComboBox()
        Me.lblAlpha = New System.Windows.Forms.Label()
        Me.btnCSVExport = New System.Windows.Forms.Button()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.gbParameters = New System.Windows.Forms.GroupBox()
        Me.gbInitData = New System.Windows.Forms.GroupBox()
        Me.gbRefData = New System.Windows.Forms.GroupBox()
        CType(Me.updRadius, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.updPolyOrder, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.statusStrip1.SuspendLayout()
        Me.gbSmoothingMethods.SuspendLayout()
        Me.gbParameters.SuspendLayout()
        Me.gbInitData.SuspendLayout()
        Me.gbRefData.SuspendLayout()
        Me.SuspendLayout()
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.label4.Location = New System.Drawing.Point(18, 64)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(132, 19)
        Me.label4.TabIndex = 27
        Me.label4.Text = "Boundary Handling :"
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.label2.Location = New System.Drawing.Point(18, 31)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(90, 19)
        Me.label2.TabIndex = 25
        Me.label2.Text = "Kernel Radius"
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.label1.Location = New System.Drawing.Point(18, 97)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(113, 19)
        Me.label1.TabIndex = 24
        Me.label1.Text = "Polynomial Order"
        '
        'cbxDeriv
        '
        Me.cbxDeriv.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxDeriv.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.cbxDeriv.FormattingEnabled = True
        Me.cbxDeriv.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"})
        Me.cbxDeriv.Location = New System.Drawing.Point(166, 161)
        Me.cbxDeriv.Name = "cbxDeriv"
        Me.cbxDeriv.Size = New System.Drawing.Size(121, 25)
        Me.cbxDeriv.TabIndex = 23
        '
        'btnExcelExport
        '
        Me.btnExcelExport.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.btnExcelExport.Location = New System.Drawing.Point(582, 548)
        Me.btnExcelExport.Name = "btnExcelExport"
        Me.btnExcelExport.Size = New System.Drawing.Size(140, 32)
        Me.btnExcelExport.TabIndex = 22
        Me.btnExcelExport.Text = "Export to Excel"
        Me.btnExcelExport.UseVisualStyleBackColor = True
        '
        'cbxBoundaryMode
        '
        Me.cbxBoundaryMode.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.cbxBoundaryMode.FormattingEnabled = True
        Me.cbxBoundaryMode.Items.AddRange(New Object() {"Symmetric", "Adaptive", "Replicate", "ZeroPad"})
        Me.cbxBoundaryMode.Location = New System.Drawing.Point(166, 62)
        Me.cbxBoundaryMode.Name = "cbxBoundaryMode"
        Me.cbxBoundaryMode.Size = New System.Drawing.Size(121, 25)
        Me.cbxBoundaryMode.TabIndex = 21
        '
        'updRadius
        '
        Me.updRadius.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.updRadius.Location = New System.Drawing.Point(166, 29)
        Me.updRadius.Maximum = New Decimal(New Integer() {13, 0, 0, 0})
        Me.updRadius.Minimum = New Decimal(New Integer() {10, 0, 0, -2147483648})
        Me.updRadius.Name = "updRadius"
        Me.updRadius.Size = New System.Drawing.Size(120, 25)
        Me.updRadius.TabIndex = 20
        Me.updRadius.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'updPolyOrder
        '
        Me.updPolyOrder.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.updPolyOrder.Location = New System.Drawing.Point(166, 95)
        Me.updPolyOrder.Maximum = New Decimal(New Integer() {13, 0, 0, 0})
        Me.updPolyOrder.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updPolyOrder.Name = "updPolyOrder"
        Me.updPolyOrder.Size = New System.Drawing.Size(120, 25)
        Me.updPolyOrder.TabIndex = 19
        Me.updPolyOrder.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'slblStatus
        '
        Me.slblStatus.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.75!)
        Me.slblStatus.ForeColor = System.Drawing.Color.White
        Me.slblStatus.Name = "slblStatus"
        Me.slblStatus.Size = New System.Drawing.Size(44, 17)
        Me.slblStatus.Text = "Ready"
        '
        'lblDerivOrder
        '
        Me.lblDerivOrder.AutoSize = True
        Me.lblDerivOrder.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.lblDerivOrder.Location = New System.Drawing.Point(18, 163)
        Me.lblDerivOrder.Name = "lblDerivOrder"
        Me.lblDerivOrder.Size = New System.Drawing.Size(114, 19)
        Me.lblDerivOrder.TabIndex = 26
        Me.lblDerivOrder.Text = "Derivative Order :"
        '
        'statusStrip1
        '
        Me.statusStrip1.BackColor = System.Drawing.Color.Crimson
        Me.statusStrip1.ImageScalingSize = New System.Drawing.Size(32, 32)
        Me.statusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.slblStatus})
        Me.statusStrip1.Location = New System.Drawing.Point(0, 601)
        Me.statusStrip1.Name = "statusStrip1"
        Me.statusStrip1.Size = New System.Drawing.Size(899, 22)
        Me.statusStrip1.TabIndex = 18
        Me.statusStrip1.Text = "statusStrip1"
        '
        'chkSG
        '
        Me.chkSG.AutoSize = True
        Me.chkSG.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.chkSG.Location = New System.Drawing.Point(56, 176)
        Me.chkSG.Name = "chkSG"
        Me.chkSG.Size = New System.Drawing.Size(151, 23)
        Me.chkSG.TabIndex = 4
        Me.chkSG.Text = "Savitzky-Golay Filter"
        Me.chkSG.UseVisualStyleBackColor = True
        '
        'chkGauss
        '
        Me.chkGauss.AutoSize = True
        Me.chkGauss.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.chkGauss.Location = New System.Drawing.Point(56, 150)
        Me.chkGauss.Name = "chkGauss"
        Me.chkGauss.Size = New System.Drawing.Size(116, 23)
        Me.chkGauss.TabIndex = 3
        Me.chkGauss.Text = "Gaussian Filter"
        Me.chkGauss.UseVisualStyleBackColor = True
        '
        'chkBinoMedian
        '
        Me.chkBinoMedian.AutoSize = True
        Me.chkBinoMedian.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.chkBinoMedian.Location = New System.Drawing.Point(56, 98)
        Me.chkBinoMedian.Name = "chkBinoMedian"
        Me.chkBinoMedian.Size = New System.Drawing.Size(128, 23)
        Me.chkBinoMedian.TabIndex = 2
        Me.chkBinoMedian.Text = "Binomial Median"
        Me.chkBinoMedian.UseVisualStyleBackColor = True
        '
        'chkBinoAvg
        '
        Me.chkBinoAvg.AutoSize = True
        Me.chkBinoAvg.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.chkBinoAvg.Location = New System.Drawing.Point(56, 72)
        Me.chkBinoAvg.Name = "chkBinoAvg"
        Me.chkBinoAvg.Size = New System.Drawing.Size(133, 23)
        Me.chkBinoAvg.TabIndex = 1
        Me.chkBinoAvg.Text = "Binomial Average"
        Me.chkBinoAvg.UseVisualStyleBackColor = True
        '
        'gbSmoothingMethods
        '
        Me.gbSmoothingMethods.Controls.Add(Me.chkGaussMed)
        Me.gbSmoothingMethods.Controls.Add(Me.chkSG)
        Me.gbSmoothingMethods.Controls.Add(Me.chkGauss)
        Me.gbSmoothingMethods.Controls.Add(Me.chkBinoMedian)
        Me.gbSmoothingMethods.Controls.Add(Me.chkBinoAvg)
        Me.gbSmoothingMethods.Controls.Add(Me.chkRectAvg)
        Me.gbSmoothingMethods.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 12.0!, System.Drawing.FontStyle.Bold)
        Me.gbSmoothingMethods.Location = New System.Drawing.Point(38, 61)
        Me.gbSmoothingMethods.Name = "gbSmoothingMethods"
        Me.gbSmoothingMethods.Size = New System.Drawing.Size(304, 229)
        Me.gbSmoothingMethods.TabIndex = 17
        Me.gbSmoothingMethods.TabStop = False
        Me.gbSmoothingMethods.Text = "Smoothing Methods"
        '
        'chkGaussMed
        '
        Me.chkGaussMed.AutoSize = True
        Me.chkGaussMed.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.chkGaussMed.Location = New System.Drawing.Point(56, 124)
        Me.chkGaussMed.Name = "chkGaussMed"
        Me.chkGaussMed.Size = New System.Drawing.Size(192, 23)
        Me.chkGaussMed.TabIndex = 51
        Me.chkGaussMed.Text = "Gaussian Weighted Median"
        Me.chkGaussMed.UseVisualStyleBackColor = True
        '
        'chkRectAvg
        '
        Me.chkRectAvg.AutoSize = True
        Me.chkRectAvg.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.chkRectAvg.Location = New System.Drawing.Point(56, 46)
        Me.chkRectAvg.Name = "chkRectAvg"
        Me.chkRectAvg.Size = New System.Drawing.Size(154, 23)
        Me.chkRectAvg.TabIndex = 0
        Me.chkRectAvg.Text = "Rectangular Average"
        Me.chkRectAvg.UseVisualStyleBackColor = True
        '
        'txtRefined
        '
        Me.txtRefined.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtRefined.Location = New System.Drawing.Point(6, 28)
        Me.txtRefined.Multiline = True
        Me.txtRefined.Name = "txtRefined"
        Me.txtRefined.ReadOnly = True
        Me.txtRefined.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtRefined.Size = New System.Drawing.Size(490, 190)
        Me.txtRefined.TabIndex = 16
        '
        'txtInit
        '
        Me.txtInit.Font = New System.Drawing.Font("Segoe UI Variable Display", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtInit.Location = New System.Drawing.Point(6, 28)
        Me.txtInit.Multiline = True
        Me.txtInit.Name = "txtInit"
        Me.txtInit.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtInit.Size = New System.Drawing.Size(490, 190)
        Me.txtInit.TabIndex = 15
        '
        'btnStart
        '
        Me.btnStart.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.btnStart.Location = New System.Drawing.Point(38, 548)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(304, 32)
        Me.btnStart.TabIndex = 14
        Me.btnStart.Text = "Start Smoothing"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'lblSigmaValue
        '
        Me.lblSigmaValue.AutoSize = True
        Me.lblSigmaValue.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.lblSigmaValue.Location = New System.Drawing.Point(18, 196)
        Me.lblSigmaValue.Name = "lblSigmaValue"
        Me.lblSigmaValue.Size = New System.Drawing.Size(89, 19)
        Me.lblSigmaValue.TabIndex = 47
        Me.lblSigmaValue.Text = "Sigma Value :"
        '
        'cbxSigmaValue
        '
        Me.cbxSigmaValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxSigmaValue.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.cbxSigmaValue.FormattingEnabled = True
        Me.cbxSigmaValue.Items.AddRange(New Object() {"1.0", "2.0", "3.0", "4.0", "5.0", "6.0", "7.0", "8.0", "9.0", "10.0", "11.0", "12.0", "13.0", "14.0", "15.0", "16.0", "17.0", "18.0"})
        Me.cbxSigmaValue.Location = New System.Drawing.Point(197, 194)
        Me.cbxSigmaValue.Name = "cbxSigmaValue"
        Me.cbxSigmaValue.Size = New System.Drawing.Size(89, 25)
        Me.cbxSigmaValue.TabIndex = 46
        '
        'lblSigmaKernelWidth
        '
        Me.lblSigmaKernelWidth.AutoSize = True
        Me.lblSigmaKernelWidth.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.lblSigmaKernelWidth.Location = New System.Drawing.Point(164, 197)
        Me.lblSigmaKernelWidth.Name = "lblSigmaKernelWidth"
        Me.lblSigmaKernelWidth.Size = New System.Drawing.Size(28, 19)
        Me.lblSigmaKernelWidth.TabIndex = 45
        Me.lblSigmaKernelWidth.Text = "w /"
        '
        'cbxAlpha
        '
        Me.cbxAlpha.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxAlpha.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.cbxAlpha.FormattingEnabled = True
        Me.cbxAlpha.Items.AddRange(New Object() {"0.1", "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0"})
        Me.cbxAlpha.Location = New System.Drawing.Point(166, 128)
        Me.cbxAlpha.Name = "cbxAlpha"
        Me.cbxAlpha.Size = New System.Drawing.Size(120, 25)
        Me.cbxAlpha.TabIndex = 49
        '
        'lblAlpha
        '
        Me.lblAlpha.AutoSize = True
        Me.lblAlpha.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.lblAlpha.Location = New System.Drawing.Point(18, 130)
        Me.lblAlpha.Name = "lblAlpha"
        Me.lblAlpha.Size = New System.Drawing.Size(87, 19)
        Me.lblAlpha.TabIndex = 48
        Me.lblAlpha.Text = "Alpha Blend :"
        '
        'btnCSVExport
        '
        Me.btnCSVExport.Font = New System.Drawing.Font("Segoe UI Variable Display", 10.0!)
        Me.btnCSVExport.Location = New System.Drawing.Point(728, 548)
        Me.btnCSVExport.Name = "btnCSVExport"
        Me.btnCSVExport.Size = New System.Drawing.Size(140, 32)
        Me.btnCSVExport.TabIndex = 50
        Me.btnCSVExport.Text = "Export to CSV"
        Me.btnCSVExport.UseVisualStyleBackColor = True
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 17.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.Location = New System.Drawing.Point(31, 14)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(290, 31)
        Me.lblTitle.TabIndex = 51
        Me.lblTitle.Text = "SonataSmooth.Tune.Etude"
        '
        'gbParameters
        '
        Me.gbParameters.Controls.Add(Me.label2)
        Me.gbParameters.Controls.Add(Me.lblDerivOrder)
        Me.gbParameters.Controls.Add(Me.updPolyOrder)
        Me.gbParameters.Controls.Add(Me.cbxAlpha)
        Me.gbParameters.Controls.Add(Me.updRadius)
        Me.gbParameters.Controls.Add(Me.lblAlpha)
        Me.gbParameters.Controls.Add(Me.cbxBoundaryMode)
        Me.gbParameters.Controls.Add(Me.lblSigmaValue)
        Me.gbParameters.Controls.Add(Me.cbxSigmaValue)
        Me.gbParameters.Controls.Add(Me.cbxDeriv)
        Me.gbParameters.Controls.Add(Me.lblSigmaKernelWidth)
        Me.gbParameters.Controls.Add(Me.label1)
        Me.gbParameters.Controls.Add(Me.label4)
        Me.gbParameters.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 12.0!, System.Drawing.FontStyle.Bold)
        Me.gbParameters.Location = New System.Drawing.Point(38, 303)
        Me.gbParameters.Name = "gbParameters"
        Me.gbParameters.Size = New System.Drawing.Size(304, 229)
        Me.gbParameters.TabIndex = 52
        Me.gbParameters.TabStop = False
        Me.gbParameters.Text = "Smoothing Parameters"
        '
        'gbInitData
        '
        Me.gbInitData.Controls.Add(Me.txtInit)
        Me.gbInitData.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gbInitData.Location = New System.Drawing.Point(366, 61)
        Me.gbInitData.Name = "gbInitData"
        Me.gbInitData.Size = New System.Drawing.Size(502, 229)
        Me.gbInitData.TabIndex = 53
        Me.gbInitData.TabStop = False
        Me.gbInitData.Text = "Initial Data"
        '
        'gbRefData
        '
        Me.gbRefData.Controls.Add(Me.txtRefined)
        Me.gbRefData.Font = New System.Drawing.Font("Segoe UI Variable Display Semib", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gbRefData.Location = New System.Drawing.Point(366, 303)
        Me.gbRefData.Name = "gbRefData"
        Me.gbRefData.Size = New System.Drawing.Size(502, 229)
        Me.gbRefData.TabIndex = 54
        Me.gbRefData.TabStop = False
        Me.gbRefData.Text = "Refined Data"
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(899, 623)
        Me.Controls.Add(Me.gbRefData)
        Me.Controls.Add(Me.gbInitData)
        Me.Controls.Add(Me.gbParameters)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.btnCSVExport)
        Me.Controls.Add(Me.statusStrip1)
        Me.Controls.Add(Me.gbSmoothingMethods)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.btnExcelExport)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "FrmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SonataSmooth.Tune.Etude"
        CType(Me.updRadius, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.updPolyOrder, System.ComponentModel.ISupportInitialize).EndInit()
        Me.statusStrip1.ResumeLayout(False)
        Me.statusStrip1.PerformLayout()
        Me.gbSmoothingMethods.ResumeLayout(False)
        Me.gbSmoothingMethods.PerformLayout()
        Me.gbParameters.ResumeLayout(False)
        Me.gbParameters.PerformLayout()
        Me.gbInitData.ResumeLayout(False)
        Me.gbInitData.PerformLayout()
        Me.gbRefData.ResumeLayout(False)
        Me.gbRefData.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents label4 As Label
    Private WithEvents label2 As Label
    Private WithEvents label1 As Label
    Private WithEvents cbxDeriv As ComboBox
    Private WithEvents btnExcelExport As Button
    Private WithEvents cbxBoundaryMode As ComboBox
    Private WithEvents updRadius As NumericUpDown
    Private WithEvents updPolyOrder As NumericUpDown
    Private WithEvents slblStatus As ToolStripStatusLabel
    Private WithEvents lblDerivOrder As Label
    Private WithEvents statusStrip1 As StatusStrip
    Private WithEvents chkSG As CheckBox
    Private WithEvents chkGauss As CheckBox
    Private WithEvents chkBinoMedian As CheckBox
    Private WithEvents chkBinoAvg As CheckBox
    Private WithEvents gbSmoothingMethods As GroupBox
    Private WithEvents chkRectAvg As CheckBox
    Private WithEvents txtRefined As TextBox
    Private WithEvents txtInit As TextBox
    Private WithEvents btnStart As Button
    Private WithEvents lblSigmaValue As Label
    Private WithEvents cbxSigmaValue As ComboBox
    Private WithEvents lblSigmaKernelWidth As Label
    Private WithEvents cbxAlpha As ComboBox
    Private WithEvents lblAlpha As Label
    Private WithEvents btnCSVExport As Button
    Private WithEvents chkGaussMed As CheckBox
    Private WithEvents lblTitle As Label
    Friend WithEvents gbParameters As GroupBox
    Private WithEvents gbInitData As GroupBox
    Private WithEvents gbRefData As GroupBox
End Class
