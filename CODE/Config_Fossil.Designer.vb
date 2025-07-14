<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_Fossil
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
        TextBox1 = New TextBox()
        TextBox2 = New TextBox()
        Label2 = New Label()
        Button2 = New Button()
        Button1 = New Button()
        Label3 = New Label()
        Label4 = New Label()
        ComboBox1 = New ComboBox()
        ComboBox2 = New ComboBox()
        TextBox3 = New TextBox()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 76)
        Label1.Name = "Label1"
        Label1.Size = New Size(128, 17)
        Label1.TabIndex = 0
        Label1.Text = "节点时间上限（大）："
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(156, 73)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(85, 23)
        TextBox1.TabIndex = 3
        ' 
        ' TextBox2
        ' 
        TextBox2.Location = New Point(156, 12)
        TextBox2.Name = "TextBox2"
        TextBox2.Size = New Size(85, 23)
        TextBox2.TabIndex = 1
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(12, 15)
        Label2.Name = "Label2"
        Label2.Size = New Size(128, 17)
        Label2.TabIndex = 2
        Label2.Text = "节点时间下限（小）："
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(439, 133)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 46
        Button2.Text = "取消"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(358, 133)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 45
        Button1.Text = "确定"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(12, 44)
        Label3.Name = "Label3"
        Label3.Size = New Size(131, 17)
        Label3.TabIndex = 47
        Label3.Text = "Lower tail probability"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(12, 105)
        Label4.Name = "Label4"
        Label4.Size = New Size(133, 17)
        Label4.TabIndex = 50
        Label4.Text = "Upper tail probability"
        ' 
        ' ComboBox1
        ' 
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"0.025", "1e-300"})
        ComboBox1.Location = New Point(156, 102)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(85, 25)
        ComboBox1.TabIndex = 51
        ' 
        ' ComboBox2
        ' 
        ComboBox2.FormattingEnabled = True
        ComboBox2.Items.AddRange(New Object() {"0.025", "1e-300"})
        ComboBox2.Location = New Point(156, 41)
        ComboBox2.Name = "ComboBox2"
        ComboBox2.Size = New Size(85, 25)
        ComboBox2.TabIndex = 52
        ' 
        ' TextBox3
        ' 
        TextBox3.Location = New Point(247, 12)
        TextBox3.Multiline = True
        TextBox3.Name = "TextBox3"
        TextBox3.ReadOnly = True
        TextBox3.ScrollBars = ScrollBars.Vertical
        TextBox3.Size = New Size(267, 115)
        TextBox3.TabIndex = 53
        ' 
        ' Config_Fossil
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(525, 194)
        ControlBox = False
        Controls.Add(TextBox3)
        Controls.Add(ComboBox2)
        Controls.Add(ComboBox1)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(Button2)
        Controls.Add(Button1)
        Controls.Add(TextBox2)
        Controls.Add(Label2)
        Controls.Add(TextBox1)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_Fossil"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Calibration Points"
        TopMost = True
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents ComboBox2 As ComboBox
    Friend WithEvents TextBox3 As TextBox
End Class
