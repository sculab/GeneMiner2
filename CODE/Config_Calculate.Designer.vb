<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Calculate
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
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

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        TextBox_Diff = New TextBox()
        Label1 = New Label()
        TableLayoutPanel1 = New TableLayoutPanel()
        TextBox_RefLen = New TextBox()
        Label13 = New Label()
        Label11 = New Label()
        TextBox_ReadLen = New TextBox()
        Label2 = New Label()
        TextBox_Depth = New TextBox()
        Label3 = New Label()
        ComboBox_Ref = New ComboBox()
        Label4 = New Label()
        ComboBox_Seq = New ComboBox()
        TableLayoutPanel2 = New TableLayoutPanel()
        Label12 = New Label()
        TextBox_Kf = New TextBox()
        Label5 = New Label()
        Label6 = New Label()
        TextBox_StepSize = New TextBox()
        TextBox_TrimThr = New TextBox()
        TextBox_ErrorLimit = New TextBox()
        Label9 = New Label()
        TextBox_CombineThr = New TextBox()
        Label10 = New Label()
        TextBox_TrimMode = New TextBox()
        Label8 = New Label()
        TextBox_Boundary = New TextBox()
        Label7 = New Label()
        GroupBox1 = New GroupBox()
        GroupBox2 = New GroupBox()
        Button_Calculate = New Button()
        Button_Apply = New Button()
        Button_Close = New Button()
        Label14 = New Label()
        TextBox_SearchDepth = New TextBox()
        TableLayoutPanel1.SuspendLayout()
        TableLayoutPanel2.SuspendLayout()
        GroupBox1.SuspendLayout()
        GroupBox2.SuspendLayout()
        SuspendLayout()
        ' 
        ' TextBox_Diff
        ' 
        TextBox_Diff.Location = New Point(271, 3)
        TextBox_Diff.Name = "TextBox_Diff"
        TextBox_Diff.Size = New Size(65, 23)
        TextBox_Diff.TabIndex = 3
        TextBox_Diff.Text = "0.1"
        ' 
        ' Label1
        ' 
        Label1.Anchor = AnchorStyles.Right
        Label1.AutoSize = True
        Label1.Location = New Point(197, 6)
        Label1.Name = "Label1"
        Label1.Size = New Size(68, 17)
        Label1.TabIndex = 9
        Label1.Text = "最大差异度"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.AutoSize = True
        TableLayoutPanel1.ColumnCount = 4
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel1.Controls.Add(TextBox_RefLen, 3, 1)
        TableLayoutPanel1.Controls.Add(Label13, 2, 1)
        TableLayoutPanel1.Controls.Add(Label11, 0, 0)
        TableLayoutPanel1.Controls.Add(TextBox_ReadLen, 1, 0)
        TableLayoutPanel1.Controls.Add(Label2, 0, 1)
        TableLayoutPanel1.Controls.Add(TextBox_Depth, 1, 1)
        TableLayoutPanel1.Controls.Add(Label1, 2, 0)
        TableLayoutPanel1.Controls.Add(TextBox_Diff, 3, 0)
        TableLayoutPanel1.Controls.Add(Label3, 2, 2)
        TableLayoutPanel1.Controls.Add(ComboBox_Ref, 3, 2)
        TableLayoutPanel1.Controls.Add(Label4, 0, 2)
        TableLayoutPanel1.Controls.Add(ComboBox_Seq, 1, 2)
        TableLayoutPanel1.Location = New Point(6, 22)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 3
        TableLayoutPanel1.RowStyles.Add(New RowStyle())
        TableLayoutPanel1.RowStyles.Add(New RowStyle())
        TableLayoutPanel1.RowStyles.Add(New RowStyle())
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        TableLayoutPanel1.Size = New Size(367, 89)
        TableLayoutPanel1.TabIndex = 11
        ' 
        ' TextBox_RefLen
        ' 
        TextBox_RefLen.Location = New Point(271, 32)
        TextBox_RefLen.Name = "TextBox_RefLen"
        TextBox_RefLen.Size = New Size(65, 23)
        TextBox_RefLen.TabIndex = 4
        TextBox_RefLen.Text = "1000"
        ' 
        ' Label13
        ' 
        Label13.Anchor = AnchorStyles.Right
        Label13.AutoSize = True
        Label13.Location = New Point(185, 35)
        Label13.Name = "Label13"
        Label13.Size = New Size(80, 17)
        Label13.TabIndex = 18
        Label13.Text = "平均基因长度"
        Label13.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label11
        ' 
        Label11.Anchor = AnchorStyles.Right
        Label11.AutoSize = True
        Label11.Location = New Point(27, 6)
        Label11.Name = "Label11"
        Label11.Size = New Size(56, 17)
        Label11.TabIndex = 17
        Label11.Text = "测序读长"
        Label11.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_ReadLen
        ' 
        TextBox_ReadLen.Location = New Point(89, 3)
        TextBox_ReadLen.Name = "TextBox_ReadLen"
        TextBox_ReadLen.Size = New Size(65, 23)
        TextBox_ReadLen.TabIndex = 1
        TextBox_ReadLen.Text = "150"
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Right
        Label2.AutoSize = True
        Label2.Location = New Point(3, 35)
        Label2.Name = "Label2"
        Label2.Size = New Size(80, 17)
        Label2.TabIndex = 11
        Label2.Text = "平均测序深度"
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_Depth
        ' 
        TextBox_Depth.Location = New Point(89, 32)
        TextBox_Depth.Name = "TextBox_Depth"
        TextBox_Depth.Size = New Size(65, 23)
        TextBox_Depth.TabIndex = 2
        TextBox_Depth.Text = "10"
        ' 
        ' Label3
        ' 
        Label3.Anchor = AnchorStyles.Right
        Label3.AutoSize = True
        Label3.Location = New Point(209, 65)
        Label3.Name = "Label3"
        Label3.Size = New Size(56, 17)
        Label3.TabIndex = 13
        Label3.Text = "参考类型"
        Label3.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ComboBox_Ref
        ' 
        ComboBox_Ref.Anchor = AnchorStyles.Left
        ComboBox_Ref.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox_Ref.FormattingEnabled = True
        ComboBox_Ref.Items.AddRange(New Object() {"Genome", "Transcriptome"})
        ComboBox_Ref.Location = New Point(271, 61)
        ComboBox_Ref.Name = "ComboBox_Ref"
        ComboBox_Ref.Size = New Size(90, 25)
        ComboBox_Ref.TabIndex = 6
        ' 
        ' Label4
        ' 
        Label4.Anchor = AnchorStyles.Right
        Label4.AutoSize = True
        Label4.Location = New Point(27, 65)
        Label4.Name = "Label4"
        Label4.Size = New Size(56, 17)
        Label4.TabIndex = 15
        Label4.Text = "样本类型"
        Label4.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ComboBox_Seq
        ' 
        ComboBox_Seq.Anchor = AnchorStyles.Left
        ComboBox_Seq.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox_Seq.FormattingEnabled = True
        ComboBox_Seq.Items.AddRange(New Object() {"Genome", "Transcriptome"})
        ComboBox_Seq.Location = New Point(89, 61)
        ComboBox_Seq.Name = "ComboBox_Seq"
        ComboBox_Seq.Size = New Size(90, 25)
        ComboBox_Seq.TabIndex = 5
        ' 
        ' TableLayoutPanel2
        ' 
        TableLayoutPanel2.ColumnCount = 4
        TableLayoutPanel2.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel2.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel2.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel2.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel2.Controls.Add(TextBox_SearchDepth, 3, 1)
        TableLayoutPanel2.Controls.Add(Label14, 2, 1)
        TableLayoutPanel2.Controls.Add(Label12, 0, 3)
        TableLayoutPanel2.Controls.Add(TextBox_Kf, 1, 0)
        TableLayoutPanel2.Controls.Add(Label5, 0, 0)
        TableLayoutPanel2.Controls.Add(Label6, 0, 1)
        TableLayoutPanel2.Controls.Add(TextBox_StepSize, 1, 1)
        TableLayoutPanel2.Controls.Add(TextBox_TrimThr, 1, 2)
        TableLayoutPanel2.Controls.Add(TextBox_ErrorLimit, 3, 0)
        TableLayoutPanel2.Controls.Add(Label9, 0, 2)
        TableLayoutPanel2.Controls.Add(TextBox_CombineThr, 1, 3)
        TableLayoutPanel2.Controls.Add(Label10, 2, 3)
        TableLayoutPanel2.Controls.Add(TextBox_TrimMode, 3, 3)
        TableLayoutPanel2.Controls.Add(Label8, 2, 2)
        TableLayoutPanel2.Controls.Add(TextBox_Boundary, 3, 2)
        TableLayoutPanel2.Controls.Add(Label7, 2, 0)
        TableLayoutPanel2.Location = New Point(15, 22)
        TableLayoutPanel2.Name = "TableLayoutPanel2"
        TableLayoutPanel2.RowCount = 4
        TableLayoutPanel2.RowStyles.Add(New RowStyle())
        TableLayoutPanel2.RowStyles.Add(New RowStyle())
        TableLayoutPanel2.RowStyles.Add(New RowStyle())
        TableLayoutPanel2.RowStyles.Add(New RowStyle())
        TableLayoutPanel2.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        TableLayoutPanel2.Size = New Size(364, 116)
        TableLayoutPanel2.TabIndex = 12
        ' 
        ' Label12
        ' 
        Label12.Anchor = AnchorStyles.Right
        Label12.AutoSize = True
        Label12.Location = New Point(3, 93)
        Label12.Name = "Label12"
        Label12.Size = New Size(80, 17)
        Label12.TabIndex = 23
        Label12.Text = "清理差异阈值"
        Label12.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_Kf
        ' 
        TextBox_Kf.Enabled = False
        TextBox_Kf.Location = New Point(89, 3)
        TextBox_Kf.Name = "TextBox_Kf"
        TextBox_Kf.Size = New Size(65, 23)
        TextBox_Kf.TabIndex = 8
        ' 
        ' Label5
        ' 
        Label5.Anchor = AnchorStyles.Right
        Label5.AutoSize = True
        Label5.Location = New Point(31, 6)
        Label5.Name = "Label5"
        Label5.Size = New Size(52, 17)
        Label5.TabIndex = 0
        Label5.Text = "过滤K值"
        Label5.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label6
        ' 
        Label6.Anchor = AnchorStyles.Right
        Label6.AutoSize = True
        Label6.Location = New Point(27, 35)
        Label6.Name = "Label6"
        Label6.Size = New Size(56, 17)
        Label6.TabIndex = 6
        Label6.Text = "过滤步长"
        Label6.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_StepSize
        ' 
        TextBox_StepSize.Enabled = False
        TextBox_StepSize.Location = New Point(89, 32)
        TextBox_StepSize.Name = "TextBox_StepSize"
        TextBox_StepSize.Size = New Size(65, 23)
        TextBox_StepSize.TabIndex = 9
        ' 
        ' TextBox_TrimThr
        ' 
        TextBox_TrimThr.Enabled = False
        TextBox_TrimThr.Location = New Point(89, 61)
        TextBox_TrimThr.Name = "TextBox_TrimThr"
        TextBox_TrimThr.Size = New Size(65, 23)
        TextBox_TrimThr.TabIndex = 10
        ' 
        ' TextBox_ErrorLimit
        ' 
        TextBox_ErrorLimit.Anchor = AnchorStyles.Left
        TextBox_ErrorLimit.Enabled = False
        TextBox_ErrorLimit.Location = New Point(222, 3)
        TextBox_ErrorLimit.Name = "TextBox_ErrorLimit"
        TextBox_ErrorLimit.Size = New Size(65, 23)
        TextBox_ErrorLimit.TabIndex = 12
        ' 
        ' Label9
        ' 
        Label9.Anchor = AnchorStyles.Right
        Label9.AutoSize = True
        Label9.Location = New Point(3, 64)
        Label9.Name = "Label9"
        Label9.Size = New Size(80, 17)
        Label9.TabIndex = 4
        Label9.Text = "保留长度阈值"
        Label9.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_CombineThr
        ' 
        TextBox_CombineThr.Enabled = False
        TextBox_CombineThr.Location = New Point(89, 90)
        TextBox_CombineThr.Name = "TextBox_CombineThr"
        TextBox_CombineThr.Size = New Size(65, 23)
        TextBox_CombineThr.TabIndex = 11
        ' 
        ' Label10
        ' 
        Label10.Anchor = AnchorStyles.Right
        Label10.AutoSize = True
        Label10.Location = New Point(160, 93)
        Label10.Name = "Label10"
        Label10.Size = New Size(56, 17)
        Label10.TabIndex = 5
        Label10.Text = "切齐方式"
        Label10.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_TrimMode
        ' 
        TextBox_TrimMode.Anchor = AnchorStyles.Left
        TextBox_TrimMode.Enabled = False
        TextBox_TrimMode.Location = New Point(222, 90)
        TextBox_TrimMode.Name = "TextBox_TrimMode"
        TextBox_TrimMode.Size = New Size(140, 23)
        TextBox_TrimMode.TabIndex = 14
        ' 
        ' Label8
        ' 
        Label8.Anchor = AnchorStyles.Right
        Label8.AutoSize = True
        Label8.Location = New Point(160, 64)
        Label8.Name = "Label8"
        Label8.Size = New Size(56, 17)
        Label8.TabIndex = 3
        Label8.Text = "边界长度"
        Label8.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_Boundary
        ' 
        TextBox_Boundary.Anchor = AnchorStyles.Left
        TextBox_Boundary.Enabled = False
        TextBox_Boundary.Location = New Point(222, 61)
        TextBox_Boundary.Name = "TextBox_Boundary"
        TextBox_Boundary.Size = New Size(140, 23)
        TextBox_Boundary.TabIndex = 13
        ' 
        ' Label7
        ' 
        Label7.Anchor = AnchorStyles.Right
        Label7.AutoSize = True
        Label7.Location = New Point(160, 6)
        Label7.Name = "Label7"
        Label7.Size = New Size(56, 17)
        Label7.TabIndex = 2
        Label7.Text = "错误阈值"
        Label7.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' GroupBox1
        ' 
        GroupBox1.AutoSize = True
        GroupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink
        GroupBox1.Controls.Add(TableLayoutPanel2)
        GroupBox1.Location = New Point(12, 194)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(385, 160)
        GroupBox1.TabIndex = 20
        GroupBox1.TabStop = False
        GroupBox1.Text = "计算结果"
        ' 
        ' GroupBox2
        ' 
        GroupBox2.AutoSize = True
        GroupBox2.AutoSizeMode = AutoSizeMode.GrowAndShrink
        GroupBox2.Controls.Add(TableLayoutPanel1)
        GroupBox2.Location = New Point(14, 12)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(379, 133)
        GroupBox2.TabIndex = 20
        GroupBox2.TabStop = False
        GroupBox2.Text = "输入数据"
        ' 
        ' Button_Calculate
        ' 
        Button_Calculate.Location = New Point(168, 157)
        Button_Calculate.Name = "Button_Calculate"
        Button_Calculate.Size = New Size(75, 30)
        Button_Calculate.TabIndex = 7
        Button_Calculate.Text = "计算"
        Button_Calculate.UseVisualStyleBackColor = True
        ' 
        ' Button_Apply
        ' 
        Button_Apply.Location = New Point(124, 366)
        Button_Apply.Name = "Button_Apply"
        Button_Apply.Size = New Size(75, 30)
        Button_Apply.TabIndex = 15
        Button_Apply.Text = "应用设置"
        Button_Apply.UseVisualStyleBackColor = True
        ' 
        ' Button_Close
        ' 
        Button_Close.Location = New Point(210, 366)
        Button_Close.Name = "Button_Close"
        Button_Close.Size = New Size(75, 30)
        Button_Close.TabIndex = 16
        Button_Close.Text = "关闭"
        Button_Close.UseVisualStyleBackColor = True
        ' 
        ' Label14
        ' 
        Label14.Anchor = AnchorStyles.Right
        Label14.AutoSize = True
        Label14.Location = New Point(160, 35)
        Label14.Name = "Label14"
        Label14.Size = New Size(56, 17)
        Label14.TabIndex = 21
        Label14.Text = "搜索深度"
        Label14.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TextBox_SearchDepth
        ' 
        TextBox_SearchDepth.Anchor = AnchorStyles.Left
        TextBox_SearchDepth.Enabled = False
        TextBox_SearchDepth.Location = New Point(222, 32)
        TextBox_SearchDepth.Name = "TextBox_SearchDepth"
        TextBox_SearchDepth.Size = New Size(65, 23)
        TextBox_SearchDepth.TabIndex = 21
        ' 
        ' Config_Calculate
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSize = True
        ClientSize = New Size(409, 411)
        ControlBox = False
        Controls.Add(Button_Calculate)
        Controls.Add(Button_Close)
        Controls.Add(Button_Apply)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        Name = "Config_Calculate"
        StartPosition = FormStartPosition.CenterScreen
        Text = "计算参数"
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        TableLayoutPanel2.ResumeLayout(False)
        TableLayoutPanel2.PerformLayout()
        GroupBox1.ResumeLayout(False)
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TextBox_Diff As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents TextBox_Depth As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents Label5 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents TextBox_TrimThr As TextBox
    Friend WithEvents TextBox_StepSize As TextBox
    Friend WithEvents TextBox_Kf As TextBox
    Friend WithEvents TextBox_Boundary As TextBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Label3 As Label
    Friend WithEvents ComboBox_Ref As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents ComboBox_Seq As ComboBox
    Friend WithEvents TextBox_ErrorLimit As TextBox
    Friend WithEvents TextBox_TrimMode As TextBox
    Friend WithEvents Button_Calculate As Button
    Friend WithEvents Button_Apply As Button
    Friend WithEvents Button_Close As Button
    Friend WithEvents Label11 As Label
    Friend WithEvents TextBox_ReadLen As TextBox
    Friend WithEvents Label12 As Label
    Friend WithEvents TextBox_CombineThr As TextBox
    Friend WithEvents TextBox_RefLen As TextBox
    Friend WithEvents Label13 As Label
    Friend WithEvents TextBox_SearchDepth As TextBox
    Friend WithEvents Label14 As Label
End Class
