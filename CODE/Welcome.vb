Imports System.Environment
Imports System.IO
Imports System.Threading

Public Class Welcome
    Dim total_file As Integer = 2
    Dim current_file As Single = 0
    Private Sub Welcome_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        System.Threading.Thread.CurrentThread.CurrentCulture = ci
        Timer1.Enabled = True
        format_path()
        load_main()
        Dim th1 As New Thread(AddressOf load_url)
        th1.Start()
        form_main.Show()
    End Sub
    Public Sub load_url()
        database_url = TestUrlsAsync(database_url.Split(";")).Result
    End Sub

    Public Sub load_main()
        Directory.CreateDirectory(root_path + "results")
        Directory.CreateDirectory(root_path + "temp")

        settings = ReadSettings(root_path + "analysis\" + "setting.ini")

        Dim guessedOS As String = "win64"
        Dim ostype As String = GetEnvironmentVariable("OSTYPE")
        If ostype IsNot Nothing Then
            If ostype.StartsWith("bsd") OrElse ostype.StartsWith("linux") Then
                guessedOS = "linux"
            ElseIf ostype.StartsWith("darwin") Then
                guessedOS = "macos"
            End If
        End If

        ' 读取 language 和 mode 设置
        TargetOS = settings.GetValueOrDefault("os", guessedOS)
        language = settings.GetValueOrDefault("language", "EN")
        exe_mode = settings.GetValueOrDefault("mode", "basic")
        database_url = "https://bbpt.scu.edu.cn/database/;http://life-bioinfo.tpddns.cn:8445/database/"
        filter_thread = CInt(settings.GetValueOrDefault("filter_thread", "2"))
        align_app = settings.GetValueOrDefault("align_app", "muscle")

        If language = "CH" Then
            to_ch()
        Else
            to_en()
        End If

        Select Case exe_mode.ToLower
            Case "basic"
                'form_main.长读条码ToolStripMenuItem.Visible = False
                form_main.DebugToolStripMenuItem.Visible = False
                DeleteTemp(Path.Combine(root_path, "temp"))
            Case "advanced", "debug"
                'form_main.长读条码ToolStripMenuItem.Visible = True
                form_main.DebugToolStripMenuItem.Visible = True
                form_main.迭代ToolStripMenuItem.Visible = True
            Case "hiv"
                'form_main.长读条码ToolStripMenuItem.Visible = True
                form_main.DebugToolStripMenuItem.Visible = False
                DeleteTemp(Path.Combine(root_path, "temp"))
            Case Else
                'form_main.长读条码ToolStripMenuItem.Visible = False
                form_main.DebugToolStripMenuItem.Visible = False
                DeleteTemp(Path.Combine(root_path, "temp"))
        End Select
        current_file = total_file

    End Sub
    Private Sub Welcome_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing

    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If current_file < total_file Then
            ProgressBar1.Value = Math.Min(current_file / total_file * 100, 100)
        Else
            Timer1.Enabled = False
            Me.Hide()
        End If
    End Sub
    Public Sub DeleteTemp(ByVal aimPath As String)
        If (aimPath(aimPath.Length - 1) <> Path.DirectorySeparatorChar) Then
            aimPath += Path.DirectorySeparatorChar
        End If  '判断待删除的目录是否存在,不存在则退出.  
        If (Not Directory.Exists(aimPath)) Then Exit Sub ' 
        Dim fileList() As String = Directory.GetFileSystemEntries(aimPath)  ' 遍历所有的文件和目录  
        total_file = Math.Max(fileList.Length, total_file)
        For Each FileName As String In fileList
            If (Directory.Exists(FileName)) Then  ' 先当作目录处理如果存在这个目录就递归
                DeleteDir(aimPath + Path.GetFileName(FileName))
            Else  ' 否则直接Delete文件  
                Try
                    File.Delete(aimPath + Path.GetFileName(FileName))
                    current_file += 1
                Catch ex As Exception
                End Try
            End If
        Next  '删除文件夹  
    End Sub

    Private Sub ProgressBar1_Click(sender As Object, e As EventArgs) Handles ProgressBar1.Click


    End Sub
End Class