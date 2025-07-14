<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Trim
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
        Label1 = New Label()
        NumericUpDown1 = New NumericUpDown()
        Button2 = New Button()
        Button1 = New Button()
        TextBox1 = New TextBox()
        Label2 = New Label()
        ComboBox1 = New ComboBox()
        Label3 = New Label()
        ComboBox2 = New ComboBox()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 39)
        Label1.Name = "Label1"
        Label1.Size = New Size(99, 17)
        Label1.TabIndex = 1
        Label1.Text = "保留长度阈值(%)"
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(224, 37)
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(66, 23)
        NumericUpDown1.TabIndex = 2
        NumericUpDown1.Value = New Decimal(New Integer() {50, 0, 0, 0})
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(215, 216)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 46
        Button2.Text = "取消"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(134, 216)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 45
        Button1.Text = "确定"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(12, 101)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.ScrollBars = ScrollBars.Vertical
        TextBox1.Size = New Size(278, 109)
        TextBox1.TabIndex = 47
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(12, 9)
        Label2.Name = "Label2"
        Label2.Size = New Size(56, 17)
        Label2.TabIndex = 48
        Label2.Text = "来源序列"
        ' 
        ' ComboBox1
        ' 
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"Original Results", "Consensus Results"})
        ComboBox1.Location = New Point(169, 6)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(121, 25)
        ComboBox1.TabIndex = 49
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(12, 73)
        Label3.Name = "Label3"
        Label3.Size = New Size(68, 17)
        Label3.TabIndex = 50
        Label3.Text = "切齐的方式"
        ' 
        ' ComboBox2
        ' 
        ComboBox2.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox2.FormattingEnabled = True
        ComboBox2.Items.AddRange(New Object() {"All Fragments", "Longest Fragments", "Trim Terminal", "Longest Isoform"})
        ComboBox2.Location = New Point(169, 70)
        ComboBox2.Name = "ComboBox2"
        ComboBox2.Size = New Size(121, 25)
        ComboBox2.TabIndex = 51
        ' 
        ' Config_Trim
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(302, 277)
        ControlBox = False
        Controls.Add(ComboBox2)
        Controls.Add(Label3)
        Controls.Add(ComboBox1)
        Controls.Add(Label2)
        Controls.Add(TextBox1)
        Controls.Add(Button2)
        Controls.Add(Button1)
        Controls.Add(NumericUpDown1)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_Trim"
        StartPosition = FormStartPosition.CenterScreen
        Text = "切齐序列"
        TopMost = True
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents Label1 As Label
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents ComboBox2 As ComboBox
End Class
