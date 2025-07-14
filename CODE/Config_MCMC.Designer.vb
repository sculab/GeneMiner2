<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_MCMC
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Config_MCMC))
        Label1 = New Label()
        TextBox1 = New TextBox()
        Label2 = New Label()
        TextBox2 = New TextBox()
        Label3 = New Label()
        NumericUpDown1 = New NumericUpDown()
        Label4 = New Label()
        ComboBox1 = New ComboBox()
        Label5 = New Label()
        TextBox3 = New TextBox()
        TextBox4 = New TextBox()
        Label6 = New Label()
        Button1 = New Button()
        Button2 = New Button()
        Button3 = New Button()
        Label7 = New Label()
        Label8 = New Label()
        Label9 = New Label()
        TextBox5 = New TextBox()
        NumericUpDown2 = New NumericUpDown()
        NumericUpDown3 = New NumericUpDown()
        NumericUpDown4 = New NumericUpDown()
        Label10 = New Label()
        Label11 = New Label()
        Label12 = New Label()
        Label13 = New Label()
        ComboBox2 = New ComboBox()
        ComboBox3 = New ComboBox()
        Label14 = New Label()
        CheckBox1 = New CheckBox()
        CheckBox2 = New CheckBox()
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
        Label1.Size = New Size(37, 17)
        Label1.TabIndex = 0
        Label1.Text = "Seed"
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(159, 12)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(99, 23)
        TextBox1.TabIndex = 1
        TextBox1.Text = "-1"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(12, 44)
        Label2.Name = "Label2"
        Label2.Size = New Size(87, 17)
        Label2.TabIndex = 2
        Label2.Text = "Sequence File"
        ' 
        ' TextBox2
        ' 
        TextBox2.Location = New Point(159, 41)
        TextBox2.Name = "TextBox2"
        TextBox2.ReadOnly = True
        TextBox2.Size = New Size(99, 23)
        TextBox2.TabIndex = 3
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(12, 75)
        Label3.Name = "Label3"
        Label3.Size = New Size(63, 17)
        Label3.TabIndex = 4
        Label3.Text = "Root Age"
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(159, 73)
        NumericUpDown1.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(99, 23)
        NumericUpDown1.TabIndex = 5
        NumericUpDown1.Value = New Decimal(New Integer() {100, 0, 0, 0})
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(12, 136)
        Label4.Name = "Label4"
        Label4.Size = New Size(46, 17)
        Label4.TabIndex = 6
        Label4.Text = "Model"
        ' 
        ' ComboBox1
        ' 
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"JC96", "K80", "F81", "F84", "HKY85"})
        ComboBox1.Location = New Point(159, 133)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(99, 25)
        ComboBox1.TabIndex = 7
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(12, 199)
        Label5.Name = "Label5"
        Label5.Size = New Size(127, 17)
        Label5.TabIndex = 8
        Label5.Text = "Mean Mutation Rate"
        ' 
        ' TextBox3
        ' 
        TextBox3.Location = New Point(159, 196)
        TextBox3.Name = "TextBox3"
        TextBox3.Size = New Size(99, 23)
        TextBox3.TabIndex = 9
        TextBox3.Text = "1"
        ' 
        ' TextBox4
        ' 
        TextBox4.Location = New Point(159, 225)
        TextBox4.Name = "TextBox4"
        TextBox4.Size = New Size(99, 23)
        TextBox4.TabIndex = 10
        TextBox4.Text = "0.1"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(12, 228)
        Label6.Name = "Label6"
        Label6.Size = New Size(144, 17)
        Label6.TabIndex = 11
        Label6.Text = "Mutation Rate Variance"
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(264, 37)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 12
        Button1.Text = "Browse"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(585, 311)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 46
        Button2.Text = "Cancel"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(504, 311)
        Button3.Name = "Button3"
        Button3.Size = New Size(75, 30)
        Button3.TabIndex = 45
        Button3.Text = "OK"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(264, 228)
        Label7.Name = "Label7"
        Label7.Size = New Size(78, 17)
        Label7.TabIndex = 47
        Label7.Text = "/10^8 years"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(264, 199)
        Label8.Name = "Label8"
        Label8.Size = New Size(78, 17)
        Label8.TabIndex = 48
        Label8.Text = "/10^8 years"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Location = New Point(264, 75)
        Label9.Name = "Label9"
        Label9.Size = New Size(108, 17)
        Label9.TabIndex = 49
        Label9.Text = "10^6 years (Myr)"
        ' 
        ' TextBox5
        ' 
        TextBox5.Location = New Point(378, 12)
        TextBox5.Multiline = True
        TextBox5.Name = "TextBox5"
        TextBox5.ReadOnly = True
        TextBox5.Size = New Size(282, 267)
        TextBox5.TabIndex = 50
        TextBox5.Text = resources.GetString("TextBox5.Text")
        ' 
        ' NumericUpDown2
        ' 
        NumericUpDown2.Location = New Point(159, 255)
        NumericUpDown2.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        NumericUpDown2.Name = "NumericUpDown2"
        NumericUpDown2.Size = New Size(99, 23)
        NumericUpDown2.TabIndex = 51
        NumericUpDown2.Value = New Decimal(New Integer() {2000, 0, 0, 0})
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Location = New Point(159, 284)
        NumericUpDown3.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        NumericUpDown3.Name = "NumericUpDown3"
        NumericUpDown3.Size = New Size(99, 23)
        NumericUpDown3.TabIndex = 52
        NumericUpDown3.Value = New Decimal(New Integer() {10, 0, 0, 0})
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.Location = New Point(159, 316)
        NumericUpDown4.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(99, 23)
        NumericUpDown4.TabIndex = 53
        NumericUpDown4.Value = New Decimal(New Integer() {20000, 0, 0, 0})
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Location = New Point(12, 257)
        Label10.Name = "Label10"
        Label10.Size = New Size(50, 17)
        Label10.TabIndex = 54
        Label10.Text = "Burn-in"
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.Location = New Point(12, 286)
        Label11.Name = "Label11"
        Label11.Size = New Size(115, 17)
        Label11.TabIndex = 55
        Label11.Text = "Sample Frequence"
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.Location = New Point(12, 318)
        Label12.Name = "Label12"
        Label12.Size = New Size(119, 17)
        Label12.TabIndex = 56
        Label12.Text = "Number of Sample"
        ' 
        ' Label13
        ' 
        Label13.AutoSize = True
        Label13.Location = New Point(12, 167)
        Label13.Name = "Label13"
        Label13.Size = New Size(41, 17)
        Label13.TabIndex = 57
        Label13.Text = "Alpha"
        ' 
        ' ComboBox2
        ' 
        ComboBox2.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox2.FormattingEnabled = True
        ComboBox2.Items.AddRange(New Object() {"Consistent", "Auto"})
        ComboBox2.Location = New Point(159, 164)
        ComboBox2.Name = "ComboBox2"
        ComboBox2.Size = New Size(99, 25)
        ComboBox2.TabIndex = 58
        ' 
        ' ComboBox3
        ' 
        ComboBox3.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox3.FormattingEnabled = True
        ComboBox3.Items.AddRange(New Object() {"with ambiguity", "no ambiguity"})
        ComboBox3.Location = New Point(159, 102)
        ComboBox3.Name = "ComboBox3"
        ComboBox3.Size = New Size(99, 25)
        ComboBox3.TabIndex = 60
        ' 
        ' Label14
        ' 
        Label14.AutoSize = True
        Label14.Location = New Point(12, 105)
        Label14.Name = "Label14"
        Label14.Size = New Size(71, 17)
        Label14.TabIndex = 59
        Label14.Text = "Clean Data"
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Checked = True
        CheckBox1.CheckState = CheckState.Checked
        CheckBox1.Location = New Point(378, 285)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(156, 21)
        CheckBox1.TabIndex = 61
        CheckBox1.Text = "Delete temporary files"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' CheckBox2
        ' 
        CheckBox2.AutoSize = True
        CheckBox2.Checked = True
        CheckBox2.CheckState = CheckState.Checked
        CheckBox2.Location = New Point(540, 285)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(118, 21)
        CheckBox2.TabIndex = 62
        CheckBox2.Text = "Repeat Analysis"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' Config_MCMC
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(672, 372)
        ControlBox = False
        Controls.Add(CheckBox2)
        Controls.Add(CheckBox1)
        Controls.Add(ComboBox3)
        Controls.Add(Label14)
        Controls.Add(ComboBox2)
        Controls.Add(Label13)
        Controls.Add(Label12)
        Controls.Add(Label11)
        Controls.Add(Label10)
        Controls.Add(NumericUpDown4)
        Controls.Add(NumericUpDown3)
        Controls.Add(NumericUpDown2)
        Controls.Add(TextBox5)
        Controls.Add(Label9)
        Controls.Add(Label8)
        Controls.Add(Label7)
        Controls.Add(Button2)
        Controls.Add(Button3)
        Controls.Add(Button1)
        Controls.Add(Label6)
        Controls.Add(TextBox4)
        Controls.Add(TextBox3)
        Controls.Add(Label5)
        Controls.Add(ComboBox1)
        Controls.Add(Label4)
        Controls.Add(NumericUpDown1)
        Controls.Add(Label3)
        Controls.Add(TextBox2)
        Controls.Add(Label2)
        Controls.Add(TextBox1)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_MCMC"
        StartPosition = FormStartPosition.CenterScreen
        Text = "MCMCTREE"
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label4 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label5 As Label
    Friend WithEvents TextBox3 As TextBox
    Friend WithEvents TextBox4 As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents TextBox5 As TextBox
    Friend WithEvents NumericUpDown2 As NumericUpDown
    Friend WithEvents NumericUpDown3 As NumericUpDown
    Friend WithEvents NumericUpDown4 As NumericUpDown
    Friend WithEvents Label10 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents Label13 As Label
    Friend WithEvents ComboBox2 As ComboBox
    Friend WithEvents ComboBox3 As ComboBox
    Friend WithEvents Label14 As Label
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents CheckBox2 As CheckBox
End Class
