<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_AGS
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
        TextBox1 = New TextBox()
        ListBox1 = New ListBox()
        Label1 = New Label()
        Button3 = New Button()
        ListBox3 = New ListBox()
        Button5 = New Button()
        SuspendLayout()
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(290, 163)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 46
        Button2.Text = "取消"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(209, 163)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 45
        Button1.Text = "确定"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(6, 5)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(150, 23)
        TextBox1.TabIndex = 48
        ' 
        ' ListBox1
        ' 
        ListBox1.FormattingEnabled = True
        ListBox1.ItemHeight = 17
        ListBox1.Location = New Point(6, 34)
        ListBox1.Name = "ListBox1"
        ListBox1.Size = New Size(150, 123)
        ListBox1.TabIndex = 49
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(209, 8)
        Label1.Name = "Label1"
        Label1.Size = New Size(63, 17)
        Label1.TabIndex = 52
        Label1.Text = "包含类群: "
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(162, 34)
        Button3.Name = "Button3"
        Button3.Size = New Size(41, 23)
        Button3.TabIndex = 54
        Button3.Text = ">>"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' ListBox3
        ' 
        ListBox3.FormattingEnabled = True
        ListBox3.ItemHeight = 17
        ListBox3.Location = New Point(209, 34)
        ListBox3.Name = "ListBox3"
        ListBox3.Size = New Size(156, 123)
        ListBox3.TabIndex = 55
        ' 
        ' Button5
        ' 
        Button5.Location = New Point(162, 63)
        Button5.Name = "Button5"
        Button5.Size = New Size(41, 23)
        Button5.TabIndex = 58
        Button5.Text = "<<"
        Button5.UseVisualStyleBackColor = True
        ' 
        ' Config_AGS
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(371, 200)
        ControlBox = False
        Controls.Add(Button5)
        Controls.Add(ListBox3)
        Controls.Add(Button3)
        Controls.Add(Label1)
        Controls.Add(ListBox1)
        Controls.Add(TextBox1)
        Controls.Add(Button2)
        Controls.Add(Button1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_AGS"
        StartPosition = FormStartPosition.CenterScreen
        Text = "下载AGS数据"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents ListBox2 As ListBox
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Button3 As Button
    Friend WithEvents ListBox3 As ListBox
    Friend WithEvents ListBox4 As ListBox
    Friend WithEvents Button4 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents Button6 As Button
End Class
