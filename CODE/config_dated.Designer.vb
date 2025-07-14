<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class config_dated
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
        DataGridView1 = New DataGridView()
        ColorDialog1 = New ColorDialog()
        Panel1 = New Panel()
        PictureBox1 = New PictureBox()
        FileToolStripMenuItem = New ToolStripMenuItem()
        载入树ToolStripMenuItem = New ToolStripMenuItem()
        McmctreeToolStripMenuItem = New ToolStripMenuItem()
        MenuStrip1 = New MenuStrip()
        FontDialog1 = New FontDialog()
        Timer1 = New Timer(components)
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        Panel1.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' DataGridView1
        ' 
        DataGridView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Location = New Point(1, 28)
        DataGridView1.MultiSelect = False
        DataGridView1.Name = "DataGridView1"
        DataGridView1.RowHeadersVisible = False
        DataGridView1.RowTemplate.Height = 23
        DataGridView1.Size = New Size(198, 501)
        DataGridView1.TabIndex = 0
        ' 
        ' Panel1
        ' 
        Panel1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel1.AutoScroll = True
        Panel1.BackColor = Color.White
        Panel1.Controls.Add(PictureBox1)
        Panel1.Location = New Point(205, 28)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(487, 501)
        Panel1.TabIndex = 1
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.White
        PictureBox1.BorderStyle = BorderStyle.FixedSingle
        PictureBox1.Location = New Point(0, 0)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(300, 300)
        PictureBox1.TabIndex = 0
        PictureBox1.TabStop = False
        ' 
        ' FileToolStripMenuItem
        ' 
        FileToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {载入树ToolStripMenuItem, McmctreeToolStripMenuItem})
        FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        FileToolStripMenuItem.Size = New Size(39, 21)
        FileToolStripMenuItem.Text = "File"
        ' 
        ' 载入树ToolStripMenuItem
        ' 
        载入树ToolStripMenuItem.Name = "载入树ToolStripMenuItem"
        载入树ToolStripMenuItem.Size = New Size(171, 22)
        载入树ToolStripMenuItem.Text = "Load Tree"
        ' 
        ' McmctreeToolStripMenuItem
        ' 
        McmctreeToolStripMenuItem.Name = "McmctreeToolStripMenuItem"
        McmctreeToolStripMenuItem.Size = New Size(171, 22)
        McmctreeToolStripMenuItem.Text = "Time Calibration"
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.ImageScalingSize = New Size(32, 32)
        MenuStrip1.Items.AddRange(New ToolStripItem() {FileToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(692, 25)
        MenuStrip1.TabIndex = 101
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FontDialog1
        ' 
        FontDialog1.Font = New Font("Times New Roman", 9F, FontStyle.Regular, GraphicsUnit.Point)
        FontDialog1.ShowColor = True
        ' 
        ' Timer1
        ' 
        Timer1.Enabled = True
        Timer1.Interval = 500
        ' 
        ' config_dated
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(692, 533)
        Controls.Add(DataGridView1)
        Controls.Add(Panel1)
        Controls.Add(MenuStrip1)
        Font = New Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point)
        Name = "config_dated"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Calibration"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        Panel1.ResumeLayout(False)
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents ColorDialog1 As ColorDialog
    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents NumericUpDown4 As NumericUpDown
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FontDialog1 As FontDialog
    Friend WithEvents 载入树ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Timer1 As Timer
    Friend WithEvents McmctreeToolStripMenuItem As ToolStripMenuItem
End Class
