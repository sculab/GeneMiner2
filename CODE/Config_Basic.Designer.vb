<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Basic
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Config_Basic))
        CheckBox4 = New CheckBox()
        GroupBox2 = New GroupBox()
        NumericUpDown3 = New NumericUpDown()
        CheckBox3 = New CheckBox()
        Label1 = New Label()
        NumericUpDown1 = New NumericUpDown()
        Label2 = New Label()
        CheckBox2 = New CheckBox()
        NumericUpDown2 = New NumericUpDown()
        GroupBox4 = New GroupBox()
        NumericUpDown10 = New NumericUpDown()
        Label9 = New Label()
        ComboBox1 = New ComboBox()
        Label3 = New Label()
        NumericUpDown8 = New NumericUpDown()
        Label5 = New Label()
        Label7 = New Label()
        NumericUpDown5 = New NumericUpDown()
        Label6 = New Label()
        CheckBox1 = New CheckBox()
        NumericUpDown7 = New NumericUpDown()
        NumericUpDown6 = New NumericUpDown()
        GroupBox3 = New GroupBox()
        NumericUpDown4 = New NumericUpDown()
        NumericUpDown9 = New NumericUpDown()
        Label8 = New Label()
        Label4 = New Label()
        Button2 = New Button()
        Button1 = New Button()
        TextBox1 = New TextBox()
        GroupBox2.SuspendLayout()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox4.SuspendLayout()
        CType(NumericUpDown10, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown8, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown5, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown7, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown6, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox3.SuspendLayout()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown9, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' CheckBox4
        ' 
        CheckBox4.AutoSize = True
        CheckBox4.Location = New Point(20, 269)
        CheckBox4.Name = "CheckBox4"
        CheckBox4.Size = New Size(111, 21)
        CheckBox4.TabIndex = 21
        CheckBox4.Text = "隐藏命令行窗口"
        CheckBox4.UseVisualStyleBackColor = True
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(NumericUpDown3)
        GroupBox2.Controls.Add(CheckBox3)
        GroupBox2.Controls.Add(Label1)
        GroupBox2.Controls.Add(NumericUpDown1)
        GroupBox2.Controls.Add(Label2)
        GroupBox2.Controls.Add(CheckBox2)
        GroupBox2.Controls.Add(NumericUpDown2)
        GroupBox2.Location = New Point(12, 12)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(175, 149)
        GroupBox2.TabIndex = 6
        GroupBox2.TabStop = False
        GroupBox2.Text = "过滤"
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Location = New Point(111, 20)
        NumericUpDown3.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NumericUpDown3.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        NumericUpDown3.Name = "NumericUpDown3"
        NumericUpDown3.Size = New Size(54, 23)
        NumericUpDown3.TabIndex = 22
        NumericUpDown3.Value = New Decimal(New Integer() {10, 0, 0, 0})
        ' 
        ' CheckBox3
        ' 
        CheckBox3.AutoSize = True
        CheckBox3.Location = New Point(8, 21)
        CheckBox3.Name = "CheckBox3"
        CheckBox3.Size = New Size(100, 21)
        CheckBox3.TabIndex = 21
        CheckBox3.Text = "读长/文件(M)"
        CheckBox3.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(8, 55)
        Label1.Name = "Label1"
        Label1.Size = New Size(64, 17)
        Label1.TabIndex = 2
        Label1.Text = "过滤K值："
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(111, 52)
        NumericUpDown1.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        NumericUpDown1.Minimum = New Decimal(New Integer() {16, 0, 0, 0})
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(54, 23)
        NumericUpDown1.TabIndex = 3
        NumericUpDown1.Value = New Decimal(New Integer() {31, 0, 0, 0})
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(8, 86)
        Label2.Name = "Label2"
        Label2.Size = New Size(68, 17)
        Label2.TabIndex = 4
        Label2.Text = "过滤步长："
        ' 
        ' CheckBox2
        ' 
        CheckBox2.AutoSize = True
        CheckBox2.Checked = True
        CheckBox2.CheckState = CheckState.Checked
        CheckBox2.Location = New Point(8, 117)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(119, 21)
        CheckBox2.TabIndex = 18
        CheckBox2.Text = "高速(高内存占用)"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown2
        ' 
        NumericUpDown2.Location = New Point(111, 83)
        NumericUpDown2.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        NumericUpDown2.Name = "NumericUpDown2"
        NumericUpDown2.Size = New Size(54, 23)
        NumericUpDown2.TabIndex = 5
        NumericUpDown2.Value = New Decimal(New Integer() {4, 0, 0, 0})
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(NumericUpDown10)
        GroupBox4.Controls.Add(Label9)
        GroupBox4.Controls.Add(ComboBox1)
        GroupBox4.Controls.Add(Label3)
        GroupBox4.Controls.Add(NumericUpDown8)
        GroupBox4.Controls.Add(Label5)
        GroupBox4.Controls.Add(Label7)
        GroupBox4.Controls.Add(NumericUpDown5)
        GroupBox4.Controls.Add(Label6)
        GroupBox4.Controls.Add(CheckBox1)
        GroupBox4.Controls.Add(NumericUpDown7)
        GroupBox4.Controls.Add(NumericUpDown6)
        GroupBox4.Location = New Point(193, 12)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(168, 245)
        GroupBox4.TabIndex = 8
        GroupBox4.TabStop = False
        GroupBox4.Text = "拼接"
        ' 
        ' NumericUpDown10
        ' 
        NumericUpDown10.Location = New Point(106, 177)
        NumericUpDown10.Maximum = New Decimal(New Integer() {9999, 0, 0, 0})
        NumericUpDown10.Minimum = New Decimal(New Integer() {1024, 0, 0, 0})
        NumericUpDown10.Name = "NumericUpDown10"
        NumericUpDown10.Size = New Size(54, 23)
        NumericUpDown10.TabIndex = 22
        NumericUpDown10.Value = New Decimal(New Integer() {4096, 0, 0, 0})
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Location = New Point(8, 179)
        Label9.Name = "Label9"
        Label9.Size = New Size(68, 17)
        Label9.TabIndex = 21
        Label9.Text = "搜索深度："
        ' 
        ' ComboBox1
        ' 
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"Auto", "0", "Unlimited"})
        ComboBox1.Location = New Point(80, 145)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(80, 25)
        ComboBox1.TabIndex = 20
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(8, 148)
        Label3.Name = "Label3"
        Label3.Size = New Size(44, 17)
        Label3.TabIndex = 18
        Label3.Text = "边界："
        ' 
        ' NumericUpDown8
        ' 
        NumericUpDown8.Location = New Point(106, 114)
        NumericUpDown8.Name = "NumericUpDown8"
        NumericUpDown8.Size = New Size(54, 23)
        NumericUpDown8.TabIndex = 17
        NumericUpDown8.Value = New Decimal(New Integer() {2, 0, 0, 0})
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(8, 86)
        Label5.Name = "Label5"
        Label5.Size = New Size(88, 17)
        Label5.TabIndex = 10
        Label5.Text = "固定拼接K值："
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(8, 117)
        Label7.Name = "Label7"
        Label7.Size = New Size(68, 17)
        Label7.TabIndex = 16
        Label7.Text = "错误阈值："
        ' 
        ' NumericUpDown5
        ' 
        NumericUpDown5.Enabled = False
        NumericUpDown5.Location = New Point(106, 83)
        NumericUpDown5.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        NumericUpDown5.Minimum = New Decimal(New Integer() {19, 0, 0, 0})
        NumericUpDown5.Name = "NumericUpDown5"
        NumericUpDown5.Size = New Size(54, 23)
        NumericUpDown5.TabIndex = 11
        NumericUpDown5.Value = New Decimal(New Integer() {39, 0, 0, 0})
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(80, 55)
        Label6.Name = "Label6"
        Label6.Size = New Size(22, 17)
        Label6.TabIndex = 15
        Label6.Text = "->"
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Checked = True
        CheckBox1.CheckState = CheckState.Checked
        CheckBox1.Location = New Point(8, 20)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(119, 21)
        CheckBox1.TabIndex = 12
        CheckBox1.Text = "自动估算拼接K值"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown7
        ' 
        NumericUpDown7.Location = New Point(106, 52)
        NumericUpDown7.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        NumericUpDown7.Minimum = New Decimal(New Integer() {19, 0, 0, 0})
        NumericUpDown7.Name = "NumericUpDown7"
        NumericUpDown7.Size = New Size(54, 23)
        NumericUpDown7.TabIndex = 14
        NumericUpDown7.Value = New Decimal(New Integer() {51, 0, 0, 0})
        ' 
        ' NumericUpDown6
        ' 
        NumericUpDown6.Location = New Point(20, 52)
        NumericUpDown6.Maximum = New Decimal(New Integer() {300, 0, 0, 0})
        NumericUpDown6.Minimum = New Decimal(New Integer() {19, 0, 0, 0})
        NumericUpDown6.Name = "NumericUpDown6"
        NumericUpDown6.Size = New Size(54, 23)
        NumericUpDown6.TabIndex = 13
        NumericUpDown6.Value = New Decimal(New Integer() {21, 0, 0, 0})
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(NumericUpDown4)
        GroupBox3.Controls.Add(NumericUpDown9)
        GroupBox3.Controls.Add(Label8)
        GroupBox3.Controls.Add(Label4)
        GroupBox3.Location = New Point(12, 167)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(175, 90)
        GroupBox3.TabIndex = 10
        GroupBox3.TabStop = False
        GroupBox3.Text = "进一步过滤"
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.Location = New Point(111, 22)
        NumericUpDown4.Maximum = New Decimal(New Integer() {2048, 0, 0, 0})
        NumericUpDown4.Minimum = New Decimal(New Integer() {64, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(54, 23)
        NumericUpDown4.TabIndex = 9
        NumericUpDown4.Value = New Decimal(New Integer() {768, 0, 0, 0})
        ' 
        ' NumericUpDown9
        ' 
        NumericUpDown9.Location = New Point(111, 53)
        NumericUpDown9.Maximum = New Decimal(New Integer() {64, 0, 0, 0})
        NumericUpDown9.Minimum = New Decimal(New Integer() {4, 0, 0, 0})
        NumericUpDown9.Name = "NumericUpDown9"
        NumericUpDown9.Size = New Size(54, 23)
        NumericUpDown9.TabIndex = 20
        NumericUpDown9.Value = New Decimal(New Integer() {6, 0, 0, 0})
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(8, 55)
        Label8.Name = "Label8"
        Label8.Size = New Size(92, 17)
        Label8.TabIndex = 19
        Label8.Text = "文件大小限制："
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(8, 24)
        Label4.Name = "Label4"
        Label4.Size = New Size(68, 17)
        Label4.TabIndex = 8
        Label4.Text = "深度限制："
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(555, 263)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 62
        Button2.Text = "取消"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(474, 263)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 61
        Button1.Text = "确定"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(367, 21)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.ScrollBars = ScrollBars.Vertical
        TextBox1.Size = New Size(263, 236)
        TextBox1.TabIndex = 63
        TextBox1.Text = resources.GetString("TextBox1.Text")
        ' 
        ' Config_Basic
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(642, 324)
        ControlBox = False
        Controls.Add(TextBox1)
        Controls.Add(CheckBox4)
        Controls.Add(Button2)
        Controls.Add(Button1)
        Controls.Add(GroupBox3)
        Controls.Add(GroupBox4)
        Controls.Add(GroupBox2)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_Basic"
        StartPosition = FormStartPosition.CenterScreen
        Text = "基础设定"
        TopMost = True
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).EndInit()
        GroupBox4.ResumeLayout(False)
        GroupBox4.PerformLayout()
        CType(NumericUpDown10, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown8, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown5, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown7, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown6, ComponentModel.ISupportInitialize).EndInit()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown9, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents NumericUpDown3 As NumericUpDown
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents Label1 As Label
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents NumericUpDown2 As NumericUpDown
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents NumericUpDown8 As NumericUpDown
    Friend WithEvents Label5 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents NumericUpDown5 As NumericUpDown
    Friend WithEvents Label6 As Label
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents NumericUpDown7 As NumericUpDown
    Friend WithEvents NumericUpDown6 As NumericUpDown
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents NumericUpDown4 As NumericUpDown
    Friend WithEvents NumericUpDown9 As NumericUpDown
    Friend WithEvents Label8 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents Label3 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents CheckBox4 As CheckBox
    Friend WithEvents NumericUpDown10 As NumericUpDown
    Friend WithEvents Label9 As Label
    Friend WithEvents TextBox1 As TextBox
End Class
