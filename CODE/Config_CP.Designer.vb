<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Config_CP
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
        components = New ComponentModel.Container()
        Button5 = New Button()
        ListBox3 = New ListBox()
        ContextMenuStrip1 = New ContextMenuStrip(components)
        清空ToolStripMenuItem = New ToolStripMenuItem()
        Button3 = New Button()
        Label1 = New Label()
        ListBox1 = New ListBox()
        ContextMenuStrip2 = New ContextMenuStrip(components)
        全选ToolStripMenuItem = New ToolStripMenuItem()
        TextBox1 = New TextBox()
        Button2 = New Button()
        Button1 = New Button()
        CheckBox1 = New CheckBox()
        CheckBox2 = New CheckBox()
        ContextMenuStrip1.SuspendLayout()
        ContextMenuStrip2.SuspendLayout()
        SuspendLayout()
        ' 
        ' Button5
        ' 
        Button5.Location = New Point(208, 62)
        Button5.Name = "Button5"
        Button5.Size = New Size(41, 23)
        Button5.TabIndex = 66
        Button5.Text = "<<"
        Button5.UseVisualStyleBackColor = True
        ' 
        ' ListBox3
        ' 
        ListBox3.ContextMenuStrip = ContextMenuStrip1
        ListBox3.FormattingEnabled = True
        ListBox3.ItemHeight = 17
        ListBox3.Location = New Point(255, 33)
        ListBox3.Name = "ListBox3"
        ListBox3.SelectionMode = SelectionMode.MultiExtended
        ListBox3.Size = New Size(197, 174)
        ListBox3.TabIndex = 65
        ' 
        ' ContextMenuStrip1
        ' 
        ContextMenuStrip1.Items.AddRange(New ToolStripItem() {清空ToolStripMenuItem})
        ContextMenuStrip1.Name = "ContextMenuStrip1"
        ContextMenuStrip1.Size = New Size(101, 26)
        ' 
        ' 清空ToolStripMenuItem
        ' 
        清空ToolStripMenuItem.Name = "清空ToolStripMenuItem"
        清空ToolStripMenuItem.Size = New Size(100, 22)
        清空ToolStripMenuItem.Text = "清空"
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(208, 33)
        Button3.Name = "Button3"
        Button3.Size = New Size(41, 23)
        Button3.TabIndex = 64
        Button3.Text = ">>"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(4, 7)
        Label1.Name = "Label1"
        Label1.Size = New Size(63, 17)
        Label1.TabIndex = 63
        Label1.Text = "包含类群: "
        ' 
        ' ListBox1
        ' 
        ListBox1.ContextMenuStrip = ContextMenuStrip2
        ListBox1.FormattingEnabled = True
        ListBox1.ItemHeight = 17
        ListBox1.Location = New Point(5, 33)
        ListBox1.Name = "ListBox1"
        ListBox1.SelectionMode = SelectionMode.MultiExtended
        ListBox1.Size = New Size(197, 174)
        ListBox1.TabIndex = 62
        ' 
        ' ContextMenuStrip2
        ' 
        ContextMenuStrip2.Items.AddRange(New ToolStripItem() {全选ToolStripMenuItem})
        ContextMenuStrip2.Name = "ContextMenuStrip2"
        ContextMenuStrip2.Size = New Size(101, 26)
        ' 
        ' 全选ToolStripMenuItem
        ' 
        全选ToolStripMenuItem.Name = "全选ToolStripMenuItem"
        全选ToolStripMenuItem.Size = New Size(100, 22)
        全选ToolStripMenuItem.Text = "全选"
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(73, 4)
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(129, 23)
        TextBox1.TabIndex = 61
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(377, 216)
        Button2.Name = "Button2"
        Button2.Size = New Size(75, 30)
        Button2.TabIndex = 60
        Button2.Text = "取消"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(296, 216)
        Button1.Name = "Button1"
        Button1.Size = New Size(75, 30)
        Button1.TabIndex = 59
        Button1.Text = "下载序列"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Location = New Point(5, 222)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(123, 21)
        CheckBox1.TabIndex = 67
        CheckBox1.Text = "作为单个基因下载"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' CheckBox2
        ' 
        CheckBox2.AutoSize = True
        CheckBox2.Checked = True
        CheckBox2.CheckState = CheckState.Checked
        CheckBox2.Location = New Point(255, 4)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(111, 21)
        CheckBox2.TabIndex = 68
        CheckBox2.Text = "不在属以上搜索"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' Config_CP
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(459, 277)
        ControlBox = False
        Controls.Add(CheckBox2)
        Controls.Add(CheckBox1)
        Controls.Add(Button5)
        Controls.Add(ListBox3)
        Controls.Add(Button3)
        Controls.Add(Label1)
        Controls.Add(ListBox1)
        Controls.Add(TextBox1)
        Controls.Add(Button2)
        Controls.Add(Button1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "Config_CP"
        StartPosition = FormStartPosition.CenterScreen
        Text = "下载数据"
        TopMost = True
        ContextMenuStrip1.ResumeLayout(False)
        ContextMenuStrip2.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Button5 As Button
    Friend WithEvents ListBox3 As ListBox
    Friend WithEvents Button3 As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button2 As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents 清空ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ContextMenuStrip2 As ContextMenuStrip
    Friend WithEvents 全选ToolStripMenuItem As ToolStripMenuItem
End Class
