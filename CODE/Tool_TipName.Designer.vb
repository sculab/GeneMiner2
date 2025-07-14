<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Tool_TipName
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
        DataGridView1 = New DataGridView()
        MenuStrip1 = New MenuStrip()
        FileToolStripMenuItem = New ToolStripMenuItem()
        LoadTreeToolStripMenuItem = New ToolStripMenuItem()
        LoadNameTableToolStripMenuItem = New ToolStripMenuItem()
        SaveTreeToolStripMenuItem = New ToolStripMenuItem()
        SaveNameTableToolStripMenuItem = New ToolStripMenuItem()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' DataGridView1
        ' 
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Location = New Point(2, 28)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.RowTemplate.Height = 25
        DataGridView1.Size = New Size(798, 420)
        DataGridView1.TabIndex = 0
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {FileToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(800, 25)
        MenuStrip1.TabIndex = 1
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FileToolStripMenuItem
        ' 
        FileToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {LoadTreeToolStripMenuItem, LoadNameTableToolStripMenuItem, SaveTreeToolStripMenuItem, SaveNameTableToolStripMenuItem})
        FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        FileToolStripMenuItem.Size = New Size(39, 21)
        FileToolStripMenuItem.Text = "File"
        ' 
        ' LoadTreeToolStripMenuItem
        ' 
        LoadTreeToolStripMenuItem.Name = "LoadTreeToolStripMenuItem"
        LoadTreeToolStripMenuItem.Size = New Size(180, 22)
        LoadTreeToolStripMenuItem.Text = "Load Tree"
        ' 
        ' LoadNameTableToolStripMenuItem
        ' 
        LoadNameTableToolStripMenuItem.Name = "LoadNameTableToolStripMenuItem"
        LoadNameTableToolStripMenuItem.Size = New Size(180, 22)
        LoadNameTableToolStripMenuItem.Text = "Load Name Table"
        ' 
        ' SaveTreeToolStripMenuItem
        ' 
        SaveTreeToolStripMenuItem.Name = "SaveTreeToolStripMenuItem"
        SaveTreeToolStripMenuItem.Size = New Size(180, 22)
        SaveTreeToolStripMenuItem.Text = "Save Tree"
        ' 
        ' SaveNameTableToolStripMenuItem
        ' 
        SaveNameTableToolStripMenuItem.Name = "SaveNameTableToolStripMenuItem"
        SaveNameTableToolStripMenuItem.Size = New Size(180, 22)
        SaveNameTableToolStripMenuItem.Text = "Save Name Table"
        ' 
        ' Tool_TipName
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(DataGridView1)
        Controls.Add(MenuStrip1)
        MainMenuStrip = MenuStrip1
        Name = "Tool_TipName"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Tip Name"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LoadTreeToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LoadNameTableToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SaveTreeToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SaveNameTableToolStripMenuItem As ToolStripMenuItem
End Class
