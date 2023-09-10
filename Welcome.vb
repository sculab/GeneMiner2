Imports System.IO
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.Timers

Public Class Welcome
    Dim total_file As Integer = 2
    Dim current_file As Single = 0
    Private Sub Welcome_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        System.Threading.Thread.CurrentThread.CurrentCulture = ci
        Timer1.Enabled = True
        form_main.Text = form_main.Text + " ver." + version
        form_main.Show()
        format_path()
        Dim th1 As New Thread(AddressOf load_main)
        th1.Start()
    End Sub
    Public Sub load_main()
        Try
            My.Computer.FileSystem.CreateDirectory(root_path + "results")
            If File.Exists(root_path + "temp") Then
                Directory.Delete(root_path + "temp", True)
            End If
            My.Computer.FileSystem.CreateDirectory(root_path + "temp")
        Catch ex As Exception

        End Try

        current_file = total_file

        Dim filePath As String = root_path + "main\" + "setting.ini"
        settings = ReadSettings(filePath)

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