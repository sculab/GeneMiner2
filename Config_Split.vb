Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Button

Public Class Config_Split
    Private Sub Config_Split_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub



    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        waiting = True
        timer_id = 1
        Dim th1 As New Threading.Thread(AddressOf But1)
        th1.Start()
        Me.Hide()


    End Sub
    Public Sub But1()
        'Dim currentTime As DateTime = DateTime.Now
        'Dim currentTimeStamp As Long = (currentTime - New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
        'Dim formattedTime As String = currentTime.ToString("yyyy-MM-dd HH:mm")



        Dim SI_build_gb As New ProcessStartInfo()
        SI_build_gb.FileName = currentDirectory + "analysis\build_gb.exe" ' 替换为实际的命令行程序路径
        SI_build_gb.WorkingDirectory = currentDirectory + "temp\" ' 替换为实际的运行文件夹路径
        SI_build_gb.CreateNoWindow = True
        SI_build_gb.Arguments = "-input " + """" + current_file + """" + " -outdir " + """" + "out_gb" + """"
        Dim process_build_gb As Process = Process.Start(SI_build_gb)
        process_build_gb.WaitForExit()
        process_build_gb.Close()

        DeleteDir(root_path + "temp\org_seq")
        Dim SI_build_primer As New ProcessStartInfo()
        SI_build_primer.FileName = currentDirectory + "analysis\build_primer.exe" ' 替换为实际的命令行程序路径
        SI_build_primer.WorkingDirectory = currentDirectory + "temp\" ' 替换为实际的运行文件夹路径
        SI_build_primer.CreateNoWindow = True
        SI_build_primer.Arguments = "-input out_gb"
        SI_build_primer.Arguments += " -min_seq_length " + NumericUpDown2.Value.ToString + " -max_seq_length " + NumericUpDown3.Value.ToString
        SI_build_primer.Arguments += " -soft_boundary " + NumericUpDown4.Value.ToString
        SI_build_primer.Arguments += " -split_only True"
        SI_build_primer.Arguments += " -do_aln False"
        SI_build_primer.Arguments += " -out_dir " + """" + root_path + "temp" + """"
        Dim process_build_primer As Process = Process.Start(SI_build_primer)
        process_build_primer.WaitForExit()
        process_build_primer.Close()

        refresh_file()
    End Sub

    Private Sub Config_Split_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged

        Label4.Visible = True
        NumericUpDown4.Visible = True
        If language = "CH" Then
            Label2.Text = "基因最小长度"
            Label3.Text = "基因最大长度"
        Else
            Label2.Text = "Minimum Gene Length"
            Label3.Text = "Maximum Gene Length"
        End If
    End Sub
End Class