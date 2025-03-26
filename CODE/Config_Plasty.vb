Imports System.IO
Imports Microsoft.VisualBasic.Devices

Public Class Config_Plasty
    Private Sub Config_Plasty_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        NumericUpDown2.Value = Math.Min(New ComputerInfo().AvailablePhysicalMemory / (1024 * 1024 * 1024), 4)
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        'Select Case ComboBox1.SelectedIndex
        '    Case "0"
        '        TextBox1.Text = "120000-200000"
        '    Case "1"
        '        TextBox1.Text = "12000-22000"
        '    Case Else
        '        TextBox1.Text = "120000-200000"
        'End Select


    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Hide()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If cpg_assemble_mode = -1 Then
            timer_id = 8
            Me.Hide()
            Exit Sub
        End If
        If ComboBox1.SelectedIndex = 2 And TextBox3.Text = "" Then
            MsgBox("Need chloroplast sequence for mito_plant mode!")
            Exit Sub
        End If
        DeleteDir(currentDirectory + "temp\NOVOPlasty")
        Directory.CreateDirectory(currentDirectory + "temp\NOVOPlasty")
        waiting = True
        timer_id = 1
        'If reads_length = 0 Then
        '    reads_length = GetReadLength()
        'End If
        Dim sr As New StreamReader(currentDirectory + "\analysis\NOVO_config.txt")
        Dim config_text As String = sr.ReadToEnd
        Dim sw As New StreamWriter(currentDirectory + "temp\NOVO_config.txt")
        config_text = config_text.Replace("$batch_file$", "batch:batch_file.txt")
        config_text = config_text.Replace("$type$", ComboBox1.Text)
        config_text = config_text.Replace("$range$", TextBox1.Text)
        config_text = config_text.Replace("$k-mer$", NumericUpDown1.Value.ToString)
        config_text = config_text.Replace("$mem$", NumericUpDown2.Value.ToString)
        config_text = config_text.Replace("$read_length$", NumericUpDown4.Value.ToString)
        If NumericUpDown3.Value = 0 Then
            config_text = config_text.Replace("$insert_size$", "")
        Else
            config_text = config_text.Replace("$insert_size$", NumericUpDown3.Value.ToString)
        End If

        config_text = config_text.Replace("$ref$", TextBox2.Text)
        config_text = config_text.Replace("$chlo$", TextBox3.Text)
        config_text = config_text.Replace("$out$", ".\NOVOPlasty\")
        sw.Write(config_text)
        sw.Close()
        sr.Close()
        Dim th1 As New Threading.Thread(AddressOf But4)
        th1.Start()
        Me.Hide()
    End Sub
    Public Sub But4()
        If CheckBox1.Checked = False Then
            For i As Integer = 1 To seqsView.Count
                If form_main.DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    Dim SI_build_fq As New ProcessStartInfo With {
                        .FileName = currentDirectory + "analysis\build_fq.exe",
                        .WorkingDirectory = currentDirectory + "analysis\",
                        .CreateNoWindow = False,
                        .Arguments = "-i1 " + """" + form_main.DataGridView2.Rows(i - 1).Cells(2).Value.ToString + """"
                    }
                    SI_build_fq.Arguments += " -i2 " + """" + form_main.DataGridView2.Rows(i - 1).Cells(3).Value.ToString + """"
                    SI_build_fq.Arguments += " -o " + """" + currentDirectory + "temp" + """"
                    SI_build_fq.Arguments += " -o1 " + "Project" + form_main.DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".1"
                    SI_build_fq.Arguments += " -o2 " + "Project" + form_main.DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".2"
                    SI_build_fq.Arguments += " -skip 0"
                    If form_config_basic.CheckBox3.Checked Then
                        SI_build_fq.Arguments += " -m_reads " + form_config_basic.NumericUpDown3.Value.ToString

                    End If

                    Dim process_build_fq As Process = Process.Start(SI_build_fq)
                    process_build_fq.WaitForExit()
                    process_build_fq.Close()
                End If
            Next
        End If

        Dim SI_build_plasty As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\NOVOPlasty4.3.4.exe",
            .WorkingDirectory = currentDirectory + "temp\",
            .CreateNoWindow = False,
            .Arguments = "-c NOVO_config.txt"
        }
        Dim process_build_plasty As Process = Process.Start(SI_build_plasty)
        process_build_plasty.WaitForExit()
        process_build_plasty.Close()

        Dim assemble_file As String = ""
        If File.Exists(currentDirectory + "temp\NOVOPlasty\Circularized_assembly_1_Project1.fasta") Then
            assemble_file = currentDirectory + "temp\NOVOPlasty\Circularized_assembly_1_Project1.fasta"
        End If
        If File.Exists(currentDirectory + "temp\NOVOPlasty\Option_1_Project1.fasta") Then
            Dim SI_check_option As New ProcessStartInfo With {
                .FileName = currentDirectory + "analysis\check_option_blast.exe",
                .WorkingDirectory = currentDirectory + "temp\",
                .CreateNoWindow = False,
                .Arguments = "-i " + """" + currentDirectory + "temp\NOVOPlasty" + """" + " -r " + """" + TextBox2.Text + """" + " -o " + "best.fasta"
            }
            Dim process_check_option As Process = New Process With {
                .StartInfo = SI_check_option
            }
            process_check_option.Start()
            process_check_option.WaitForExit()
            process_check_option.Close()
            If File.Exists(currentDirectory + "temp\NOVOPlasty\best.fasta") Then
                assemble_file = currentDirectory + "temp\NOVOPlasty\best.fasta"
            End If
        End If
        If File.Exists(assemble_file) Then
            Select Case cpg_assemble_mode
                Case 0, 1, 2
                    If File.Exists(currentDirectory + "temp\output_log.txt") Then
                        File.Delete(currentDirectory + "temp\output_log.txt")
                    End If
                    If File.Exists(currentDirectory + "temp\output.gb") Then
                        File.Delete(currentDirectory + "temp\output.gb")
                    End If
                    If File.Exists(currentDirectory + "temp\output.fasta") Then
                        File.Delete(currentDirectory + "temp\output.fasta")
                    End If
                    Directory.CreateDirectory(out_dir + "\Organelle\")
                    If cpg_assemble_mode <> 0 Then
                        File.Copy(assemble_file, out_dir + "\Organelle\" + TextBox5.Text + ".fasta", True)
                        Dim result0 As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        ' 根据用户的选择执行相应的操作
                        If result0 = DialogResult.Yes Then
                            Process.Start("explorer.exe", """" + out_dir.Replace("/", "\") + "\Organelle" + """")
                        End If
                        Exit Sub
                    End If
                    do_PGA(currentDirectory + "temp\ref_gb.gb", assemble_file, currentDirectory + "temp")

                    If File.Exists(currentDirectory + "temp\output.gb") Then
                        safe_copy(currentDirectory + "temp\output.gb", out_dir + "\Organelle\" + TextBox5.Text + ".gb", True)
                        safe_copy(currentDirectory + "temp\output.fasta", out_dir + "\Organelle\" + TextBox5.Text + ".fasta", True)
                        safe_copy(currentDirectory + "temp\warning.log", out_dir + "\Organelle\warning.log", True)
                    Else
                        MsgBox("Error in annotate.")
                        Dim result1 As DialogResult = MessageBox.Show("Unable to annotate. Would you like to view the temp file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        ' 根据用户的选择执行相应的操作
                        If result1 = DialogResult.Yes Then
                            Process.Start("explorer.exe", """" + (currentDirectory + "temp\NOVOPlasty").Replace("/", "\") + """")
                        End If
                        Exit Sub
                    End If
                    If File.Exists(currentDirectory + "temp\output_log.txt") Then
                        File.Copy(currentDirectory + "temp\output_log.txt", out_dir + "\Organelle\" + TextBox5.Text + "_warning.txt", True)
                    End If
                    Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    ' 根据用户的选择执行相应的操作
                    If result = DialogResult.Yes Then
                        Process.Start("explorer.exe", """" + out_dir.Replace("/", "\") + "\Organelle" + """")
                    End If
                Case Else

            End Select
        Else
            Dim result As DialogResult = MessageBox.Show("Unable to obtain the complete genome. Would you like to view the temp file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            ' 根据用户的选择执行相应的操作
            If result = DialogResult.Yes Then
                Process.Start("explorer.exe", """" + (currentDirectory + "temp\NOVOPlasty").Replace("/", "\") + """")
            End If
        End If
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
            .FileName = "",
            .Multiselect = True,
            .DefaultExt = ".fas",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            TextBox3.Text = opendialog.FileName
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
            .FileName = "",
            .Multiselect = True,
            .DefaultExt = ".fas",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            TextBox2.Text = opendialog.FileName
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        TextBox2.Text = ""
    End Sub

End Class