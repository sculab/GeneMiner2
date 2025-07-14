<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Split
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
        Button2 = New Button()
        Button1 = New Button()
        NumericUpDown4 = New NumericUpDown()
        Label4 = New Label()
        NumericUpDown3 = New NumericUpDown()
        Label3 = New Label()
        NumericUpDown2 = New NumericUpDown()
        Label2 = New Label()
        CheckBox1 = New CheckBox()
        NumericUpDown1 = New NumericUpDown()
        Label1 = New Label()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(216, 152)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 44
        Button2.Text = "取消"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(135, 152)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 43
        Button1.Text = "确定"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.Location = New Point(204, 69)
        NumericUpDown4.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(87, 23)
        NumericUpDown4.TabIndex = 31
        NumericUpDown4.Value = New Decimal(New Integer() {1000, 0, 0, 0})
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(13, 71)
        Label4.Name = "Label4"
        Label4.Size = New Size(80, 17)
        Label4.TabIndex = 30
        Label4.Text = "扩展边界左侧"
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Location = New Point(204, 40)
        NumericUpDown3.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        NumericUpDown3.Minimum = New Decimal(New Integer() {100, 0, 0, 0})
        NumericUpDown3.Name = "NumericUpDown3"
        NumericUpDown3.Size = New Size(87, 23)
        NumericUpDown3.TabIndex = 29
        NumericUpDown3.Value = New Decimal(New Integer() {5000, 0, 0, 0})
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(13, 42)
        Label3.Name = "Label3"
        Label3.Size = New Size(80, 17)
        Label3.TabIndex = 28
        Label3.Text = "基因最大长度"
        ' 
        ' NumericUpDown2
        ' 
        NumericUpDown2.Location = New Point(204, 11)
        NumericUpDown2.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NumericUpDown2.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        NumericUpDown2.Name = "NumericUpDown2"
        NumericUpDown2.Size = New Size(87, 23)
        NumericUpDown2.TabIndex = 27
        NumericUpDown2.Value = New Decimal(New Integer() {200, 0, 0, 0})
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(13, 13)
        Label2.Name = "Label2"
        Label2.Size = New Size(80, 17)
        Label2.TabIndex = 26
        Label2.Text = "基因最小长度"
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Location = New Point(13, 127)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(99, 21)
        CheckBox1.TabIndex = 45
        CheckBox1.Text = "去除外显子区"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(204, 98)
        NumericUpDown1.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(87, 23)
        NumericUpDown1.TabIndex = 47
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(13, 100)
        Label1.Name = "Label1"
        Label1.Size = New Size(80, 17)
        Label1.TabIndex = 46
        Label1.Text = "扩展边界右侧"
        ' 
        ' Config_Split
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(302, 213)
        ControlBox = False
        Controls.Add(NumericUpDown1)
        Controls.Add(Label1)
        Controls.Add(CheckBox1)
        Controls.Add(Button2)
        Controls.Add(Button1)
        Controls.Add(NumericUpDown4)
        Controls.Add(Label4)
        Controls.Add(NumericUpDown3)
        Controls.Add(Label3)
        Controls.Add(NumericUpDown2)
        Controls.Add(Label2)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_Split"
        StartPosition = FormStartPosition.CenterScreen
        Text = "分割序列"
        TopMost = True
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents NumericUpDown4 As NumericUpDown
    Friend WithEvents Label4 As Label
    Friend WithEvents NumericUpDown3 As NumericUpDown
    Friend WithEvents Label3 As Label
    Friend WithEvents NumericUpDown2 As NumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label1 As Label
End Class
