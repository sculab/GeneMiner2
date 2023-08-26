Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Button
Imports Microsoft.VisualBasic.Devices

Public Class Config_Plasty
    Private Sub Config_Plasty_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 0
        Dim computerInfo As New ComputerInfo()
        Dim totalPhysicalMemory As Long = computerInfo.TotalPhysicalMemory
        NumericUpDown2.Value = CInt(totalPhysicalMemory / (1024.0 * 1024.0 * 1024.0) - 1)

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Select Case ComboBox1.SelectedIndex
            Case "0"
                TextBox1.Text = "120000-200000"
            Case "1"
                TextBox1.Text = "12000-22000"
            Case Else
                TextBox1.Text = "120000-200000"
        End Select


    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Hide()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If ComboBox1.SelectedIndex = 2 And TextBox3.Text = "" Then
            MsgBox("Need chloroplast sequence for mito_plant mode!")
            Exit Sub
        End If
        waiting = True
        timer_id = 1
        Dim sr As New StreamReader(currentDirectory + "\analysis\NOVO_config.txt")
        Dim config_text As String = sr.ReadToEnd
        Dim sw As New StreamWriter(currentDirectory + "temp\NOVO_config.txt")
        config_text = config_text.Replace("$batch_file$", "batch:" + currentDirectory + "temp\batch_file.txt")
        config_text = config_text.Replace("$type$", ComboBox1.Text)
        config_text = config_text.Replace("$range$", TextBox1.Text)
        config_text = config_text.Replace("$k-mer$", NumericUpDown1.Value.ToString)
        config_text = config_text.Replace("$mem$", NumericUpDown2.Value.ToString)
        config_text = config_text.Replace("$read_length$", NumericUpDown4.Value.ToString)
        config_text = config_text.Replace("$insert_size$", NumericUpDown3.Value.ToString)
        config_text = config_text.Replace("$ref$", TextBox2.Text)
        config_text = config_text.Replace("$chlo$", TextBox3.Text)
        config_text = config_text.Replace("$out$", form_main.TextBox1.Text + "\".Replace("\\", "\"))
        sw.Write(config_text)
        sw.Close()
        sr.Close()
        Dim th1 As New Threading.Thread(AddressOf But4)
        th1.Start()
        Me.Hide()
    End Sub
    Public Sub But4()
        For i As Integer = 1 To seqsView.Count
            If form_main.DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim SI_build_fq As New ProcessStartInfo()
                SI_build_fq.FileName = currentDirectory + "analysis\build_fq.exe" ' 替换为实际的命令行程序路径
                SI_build_fq.WorkingDirectory = currentDirectory + "analysis\" ' 替换为实际的运行文件夹路径
                SI_build_fq.CreateNoWindow = False
                SI_build_fq.Arguments = "-i1 " + """" + form_main.DataGridView2.Rows(i - 1).Cells(2).Value.ToString + """"
                SI_build_fq.Arguments += " -i2 " + """" + form_main.DataGridView2.Rows(i - 1).Cells(3).Value.ToString + """"
                SI_build_fq.Arguments += " -o " + """" + currentDirectory + "temp" + """"
                SI_build_fq.Arguments += " -o1 " + "Project" + form_main.DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".1"
                SI_build_fq.Arguments += " -o2 " + "Project" + form_main.DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".2"
                If form_main.CheckBox3.Checked Then
                    SI_build_fq.Arguments += " -m_reads " + form_main.NumericUpDown3.Value.ToString

                End If

                Dim process_build_fq As Process = Process.Start(SI_build_fq)
                process_build_fq.WaitForExit()
                process_build_fq.Close()
            End If
        Next

        Dim SI_build_plasty As New ProcessStartInfo()
        SI_build_plasty.FileName = currentDirectory + "analysis\NOVOPlasty4.3.1.exe" ' 替换为实际的命令行程序路径
        SI_build_plasty.WorkingDirectory = currentDirectory + "analysis\" ' 替换为实际的运行文件夹路径
        SI_build_plasty.CreateNoWindow = False
        SI_build_plasty.Arguments = "-c " + """" + currentDirectory + "temp\NOVO_config.txt" + """"
        Dim process_build_plasty As Process = Process.Start(SI_build_plasty)
        process_build_plasty.WaitForExit()
        process_build_plasty.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim opendialog As New FolderBrowserDialog
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            TextBox3.Text = opendialog.SelectedPath
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim opendialog As New FolderBrowserDialog
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            TextBox2.Text = opendialog.SelectedPath
        End If
    End Sub
End Class