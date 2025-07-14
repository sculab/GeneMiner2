<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Combine
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Config_Combine))
        CheckBox1 = New CheckBox()
        CheckBox3 = New CheckBox()
        TextBox1 = New TextBox()
        Label1 = New Label()
        Label2 = New Label()
        NumericUpDown1 = New NumericUpDown()
        TextBox2 = New TextBox()
        Button2 = New Button()
        Button1 = New Button()
        GroupBox1 = New GroupBox()
        ComboBox1 = New ComboBox()
        Label3 = New Label()
        CheckBox4 = New CheckBox()
        CheckBox2 = New CheckBox()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox1.SuspendLayout()
        SuspendLayout()
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Checked = True
        CheckBox1.CheckState = CheckState.Checked
        CheckBox1.Location = New Point(6, 49)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(173, 21)
        CheckBox1.TabIndex = 0
        CheckBox1.Text = "使用missing(?)代替gap(-) "
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' CheckBox3
        ' 
        CheckBox3.AutoSize = True
        CheckBox3.Checked = True
        CheckBox3.CheckState = CheckState.Checked
        CheckBox3.Location = New Point(6, 76)
        CheckBox3.Name = "CheckBox3"
        CheckBox3.Size = New Size(99, 21)
        CheckBox3.TabIndex = 2
        CheckBox3.Text = "自动清理序列"
        CheckBox3.UseVisualStyleBackColor = True
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(249, 10)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.ScrollBars = ScrollBars.Vertical
        TextBox1.Size = New Size(310, 178)
        TextBox1.TabIndex = 3
        TextBox1.Text = resources.GetString("TextBox1.Text")
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(23, 106)
        Label1.Name = "Label1"
        Label1.Size = New Size(80, 17)
        Label1.TabIndex = 4
        Label1.Text = "最大差异大于"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(23, 135)
        Label2.Name = "Label2"
        Label2.Size = New Size(80, 17)
        Label2.TabIndex = 6
        Label2.Text = "序列数量小于"
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(160, 133)
        NumericUpDown1.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
        NumericUpDown1.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(65, 23)
        NumericUpDown1.TabIndex = 7
        NumericUpDown1.Value = New Decimal(New Integer() {1, 0, 0, 0})
        ' 
        ' TextBox2
        ' 
        TextBox2.Location = New Point(160, 103)
        TextBox2.Name = "TextBox2"
        TextBox2.Size = New Size(65, 23)
        TextBox2.TabIndex = 8
        TextBox2.Text = "0.1"
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(484, 194)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 48
        Button2.Text = "Cancel"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(403, 194)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 47
        Button1.Text = "OK"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(ComboBox1)
        GroupBox1.Controls.Add(Label3)
        GroupBox1.Controls.Add(CheckBox1)
        GroupBox1.Controls.Add(CheckBox3)
        GroupBox1.Controls.Add(TextBox2)
        GroupBox1.Controls.Add(Label1)
        GroupBox1.Controls.Add(NumericUpDown1)
        GroupBox1.Controls.Add(Label2)
        GroupBox1.Location = New Point(12, 66)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(231, 167)
        GroupBox1.TabIndex = 49
        GroupBox1.TabStop = False
        ' 
        ' ComboBox1
        ' 
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"mafft", "muscle"})
        ComboBox1.Location = New Point(160, 16)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(65, 25)
        ComboBox1.TabIndex = 10
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(6, 19)
        Label3.Name = "Label3"
        Label3.Size = New Size(56, 17)
        Label3.TabIndex = 9
        Label3.Text = "比对程序"
        ' 
        ' CheckBox4
        ' 
        CheckBox4.AutoSize = True
        CheckBox4.Checked = True
        CheckBox4.CheckState = CheckState.Checked
        CheckBox4.Location = New Point(18, 39)
        CheckBox4.Name = "CheckBox4"
        CheckBox4.Size = New Size(111, 21)
        CheckBox4.TabIndex = 50
        CheckBox4.Text = "比对并切齐结果"
        CheckBox4.UseVisualStyleBackColor = True
        ' 
        ' CheckBox2
        ' 
        CheckBox2.AutoSize = True
        CheckBox2.Checked = True
        CheckBox2.CheckState = CheckState.Checked
        CheckBox2.Location = New Point(18, 12)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(183, 21)
        CheckBox2.TabIndex = 1
        CheckBox2.Text = "使用基于参考序列切齐的结果"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' Config_Combine
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(571, 266)
        ControlBox = False
        Controls.Add(CheckBox4)
        Controls.Add(GroupBox1)
        Controls.Add(CheckBox2)
        Controls.Add(Button2)
        Controls.Add(Button1)
        Controls.Add(TextBox1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_Combine"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Combine"
        TopMost = True
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents CheckBox4 As CheckBox
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents CheckBox2 As CheckBox
End Class
