<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Plasty
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
        Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(Config_Plasty))
        Label1 = New Label()
        ComboBox1 = New ComboBox()
        Label2 = New Label()
        TextBox1 = New TextBox()
        NumericUpDown1 = New NumericUpDown()
        Label3 = New Label()
        Label4 = New Label()
        NumericUpDown2 = New NumericUpDown()
        Label5 = New Label()
        Label6 = New Label()
        TextBox2 = New TextBox()
        Button1 = New Button()
        TextBox3 = New TextBox()
        Button2 = New Button()
        TextBox4 = New TextBox()
        Button3 = New Button()
        Button4 = New Button()
        NumericUpDown3 = New NumericUpDown()
        Label7 = New Label()
        Label8 = New Label()
        NumericUpDown4 = New NumericUpDown()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 15)
        Label1.Name = "Label1"
        Label1.Size = New Size(124, 17)
        Label1.TabIndex = 0
        Label1.Text = "类型(叶绿体，线粒体)"
        ' 
        ' ComboBox1
        ' 
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"chloro", "mito", "mito_plant"})
        ComboBox1.Location = New Point(164, 12)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(141, 25)
        ComboBox1.TabIndex = 1
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(12, 46)
        Label2.Name = "Label2"
        Label2.Size = New Size(92, 17)
        Label2.TabIndex = 2
        Label2.Text = "基因组大小范围"
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(164, 43)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(141, 23)
        TextBox1.TabIndex = 3
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(103, 79)
        NumericUpDown1.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        NumericUpDown1.Minimum = New Decimal(New Integer() {19, 0, 0, 0})
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(54, 23)
        NumericUpDown1.TabIndex = 4
        NumericUpDown1.Value = New Decimal(New Integer() {31, 0, 0, 0})
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(12, 81)
        Label3.Name = "Label3"
        Label3.Size = New Size(44, 17)
        Label3.TabIndex = 5
        Label3.Text = "K-mer"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(12, 110)
        Label4.Name = "Label4"
        Label4.Size = New Size(80, 17)
        Label4.TabIndex = 6
        Label4.Text = "最大允许内存"
        ' 
        ' NumericUpDown2
        ' 
        NumericUpDown2.Location = New Point(103, 108)
        NumericUpDown2.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        NumericUpDown2.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        NumericUpDown2.Name = "NumericUpDown2"
        NumericUpDown2.Size = New Size(54, 23)
        NumericUpDown2.TabIndex = 7
        NumericUpDown2.Value = New Decimal(New Integer() {31, 0, 0, 0})
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(12, 143)
        Label5.Name = "Label5"
        Label5.Size = New Size(136, 17)
        Label5.TabIndex = 8
        Label5.Text = "叶绿体序列(拼接线粒体)"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(164, 143)
        Label6.Name = "Label6"
        Label6.Size = New Size(88, 17)
        Label6.TabIndex = 9
        Label6.Text = "参考序列(可选)"
        ' 
        ' TextBox2
        ' 
        TextBox2.Location = New Point(164, 163)
        TextBox2.Name = "TextBox2"
        TextBox2.ReadOnly = True
        TextBox2.Size = New Size(141, 23)
        TextBox2.TabIndex = 10
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(230, 192)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 11
        Button1.Text = "浏览"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' TextBox3
        ' 
        TextBox3.Location = New Point(13, 163)
        TextBox3.Name = "TextBox3"
        TextBox3.ReadOnly = True
        TextBox3.Size = New Size(144, 23)
        TextBox3.TabIndex = 12
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(82, 192)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 13
        Button2.Text = "浏览"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' TextBox4
        ' 
        TextBox4.Location = New Point(12, 228)
        TextBox4.Multiline = True
        TextBox4.Name = "TextBox4"
        TextBox4.ReadOnly = True
        TextBox4.Size = New Size(292, 134)
        TextBox4.TabIndex = 14
        TextBox4.Text = resources.GetString("TextBox4.Text")
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(230, 368)
        Button3.Name = "Button3"
        Button3.Size = New Size(75, 30)
        Button3.TabIndex = 48
        Button3.Text = "取消"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' Button4
        ' 
        Button4.Location = New Point(149, 368)
        Button4.Name = "Button4"
        Button4.Size = New Size(75, 30)
        Button4.TabIndex = 47
        Button4.Text = "确定"
        Button4.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Location = New Point(251, 108)
        NumericUpDown3.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NumericUpDown3.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        NumericUpDown3.Name = "NumericUpDown3"
        NumericUpDown3.Size = New Size(54, 23)
        NumericUpDown3.TabIndex = 52
        NumericUpDown3.Value = New Decimal(New Integer() {500, 0, 0, 0})
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(164, 110)
        Label7.Name = "Label7"
        Label7.Size = New Size(56, 17)
        Label7.TabIndex = 51
        Label7.Text = "插入大小"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(164, 81)
        Label8.Name = "Label8"
        Label8.Size = New Size(56, 17)
        Label8.TabIndex = 50
        Label8.Text = "读长大小"
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.Location = New Point(251, 79)
        NumericUpDown4.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NumericUpDown4.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(54, 23)
        NumericUpDown4.TabIndex = 49
        NumericUpDown4.Value = New Decimal(New Integer() {150, 0, 0, 0})
        ' 
        ' Config_Plasty
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(317, 408)
        ControlBox = False
        Controls.Add(NumericUpDown3)
        Controls.Add(Label7)
        Controls.Add(Label8)
        Controls.Add(NumericUpDown4)
        Controls.Add(Button3)
        Controls.Add(Button4)
        Controls.Add(TextBox4)
        Controls.Add(Button2)
        Controls.Add(TextBox3)
        Controls.Add(Button1)
        Controls.Add(TextBox2)
        Controls.Add(Label6)
        Controls.Add(Label5)
        Controls.Add(NumericUpDown2)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(NumericUpDown1)
        Controls.Add(TextBox1)
        Controls.Add(Label2)
        Controls.Add(ComboBox1)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.SizableToolWindow
        Name = "Config_Plasty"
        StartPosition = FormStartPosition.CenterScreen
        Text = "细胞器基因组"
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents NumericUpDown2 As NumericUpDown
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents TextBox3 As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents TextBox4 As TextBox
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents NumericUpDown3 As NumericUpDown
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents NumericUpDown4 As NumericUpDown
End Class
