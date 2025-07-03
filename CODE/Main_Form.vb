Imports System.Collections.Concurrent
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.Devices


Public Class Main_Form

    Public Sub initialize_data()
        Dim Column_Select As New DataGridViewCheckBoxColumn With {
            .HeaderText = "Select"
        }
        DataGridView1.Columns.Insert(0, Column_Select)
        DataGridView1.AllowUserToAddRows = False

        Dim ref_table As New System.Data.DataTable With {
            .TableName = "Refs Table"
        }
        Dim Column_ID As New System.Data.DataColumn("ID", System.Type.GetType("System.Int32"))
        Dim Column_Name As New System.Data.DataColumn("Name")
        Dim Column_Filter As New System.Data.DataColumn("Ref. Count")
        Dim Column_RF As New System.Data.DataColumn("Ref. Length")
        Dim Column_Reads As New System.Data.DataColumn("Reads")
        Dim Column_State As New System.Data.DataColumn("Ass. State/Count")
        Dim Column_Align As New System.Data.DataColumn("Ass. Length")
        Dim Column_Diff As New System.Data.DataColumn("Max. Diff.")
        ref_table.Columns.Add(Column_ID)
        ref_table.Columns.Add(Column_Name)
        ref_table.Columns.Add(Column_Filter)
        ref_table.Columns.Add(Column_RF)
        ref_table.Columns.Add(Column_Reads)
        ref_table.Columns.Add(Column_State)
        ref_table.Columns.Add(Column_Align)
        ref_table.Columns.Add(Column_Diff)
        mydata_Dataset.Tables.Add(ref_table)

        refsView = mydata_Dataset.Tables("Refs Table").DefaultView
        refsView.AllowNew = False
        refsView.AllowDelete = False
        refsView.AllowEdit = False


        Dim Column_Select1 As New DataGridViewCheckBoxColumn With {
            .HeaderText = "Select"
        }
        DataGridView2.Columns.Insert(0, Column_Select1)
        DataGridView2.AllowUserToAddRows = False

        Dim data_table As New System.Data.DataTable With {
            .TableName = "Data Table"
        }
        Dim Column_ID1 As New System.Data.DataColumn("ID", System.Type.GetType("System.Int32"))
        Dim Column_1 As New System.Data.DataColumn("Data 1")
        Dim Column_2 As New System.Data.DataColumn("Data 2")
        data_table.Columns.Add(Column_ID1)
        data_table.Columns.Add(Column_1)
        data_table.Columns.Add(Column_2)

        mydata_Dataset.Tables.Add(data_table)

        seqsView = mydata_Dataset.Tables("Data Table").DefaultView
        seqsView.AllowNew = False
        seqsView.AllowDelete = False
        seqsView.AllowEdit = False

        Dim Column_Select2 As New DataGridViewCheckBoxColumn With {
            .HeaderText = "Select"
        }
        form_config_tree.DataGridView1.Columns.Insert(0, Column_Select2)
        Dim taxon_table As New System.Data.DataTable
        form_config_tree.DataGridView1.AllowUserToAddRows = False
        taxon_table.TableName = "Taxon Table"
        Dim Column_Taxon_Name As New System.Data.DataColumn("Name")
        taxon_table.Columns.Add(Column_Taxon_Name)
        mydata_Dataset.Tables.Add(taxon_table)
        taxonView = mydata_Dataset.Tables("Taxon Table").DefaultView
        taxonView.AllowNew = False
        taxonView.AllowDelete = False
        taxonView.AllowEdit = False

    End Sub
    Public Sub do_filter(ByVal options() As String)
        'options = (0:kf,1:kr,2:q1,3:q2,4:ref,5:out_dir,6:lkd,7:plasty,8:refilter,9:no_window,10:thread)
        Dim SI_filter As New ProcessStartInfo()
        Dim read_count_path As String = options(5) + "\ref_reads_count_dict.txt"

        If options(8) = 1 Then
            If Not File.Exists(read_count_path) Then
                If Not MenuClicked.StartsWith("batch_") Then
                    MsgBox("Please do filter first!", MsgBoxStyle.Information, "Information")
                End If
                Exit Sub
            End If

            SI_filter.FileName = currentDirectory + "analysis\main_refilter_new.exe"
            SI_filter.WorkingDirectory = currentDirectory + "temp\"
            SI_filter.CreateNoWindow = (options(9) = 1)
            SI_filter.Arguments = "-r " + """" + options(4) + """"
            SI_filter.Arguments += " -qd " + """" + options(5) + "\filtered_pe" + """"
            SI_filter.Arguments += " -o " + """" + options(5) + "\filtered" + """"
            SI_filter.Arguments += " -kf " + options(0)
            SI_filter.Arguments += " --log-file " + """" + options(5) + "\log.txt" + """"
            SI_filter.Arguments += " --max-depth " + form_config_basic.NumericUpDown4.Value.ToString
            SI_filter.Arguments += " --max-size " + form_config_basic.NumericUpDown9.Value.ToString
            SI_filter.Arguments += " -p " + options(10)
        Else
            SI_filter.FileName = currentDirectory + "analysis\MainFilterNew.exe"
            SI_filter.WorkingDirectory = currentDirectory + "temp\"
            SI_filter.CreateNoWindow = (options(9) = 1)
            SI_filter.Arguments = "-r " + """" + options(4) + """"
            SI_filter.Arguments += " -q1" + options(2) + " -q2" + options(3)
            SI_filter.Arguments += " -o " + """" + options(5) + """"
            SI_filter.Arguments += " -kf " + options(0)
            SI_filter.Arguments += " -s " + form_config_basic.NumericUpDown2.Value.ToString
            SI_filter.Arguments += If(form_config_basic.CheckBox2.Checked, " -gr", "")
            SI_filter.Arguments += If(options(7) = 1, " -m 1 -lb", " -subdir filtered_pe -m 4 -lb")
            SI_filter.Arguments += If(Len(options(6)) > 0, " -lkd " + """" + Path.GetFullPath(options(5) + "\" + options(6)) + """", "")
            SI_filter.Arguments += If(form_config_basic.CheckBox3.Checked, " -m_reads " + form_config_basic.NumericUpDown3.Value.ToString, "")
        End If

        Dim process_filter As Process = Process.Start(SI_filter)
        process_filter.WaitForExit()
        process_filter.Close()

        If options(8) = 0 AndAlso MenuClicked = "filter" AndAlso Directory.Exists(options(5) + "\filtered_pe") Then
            FileIO.FileSystem.CreateDirectory(options(5) + "\filtered")

            Dim bufferSize = 1024 * 1024
            Dim buffer = New Byte(bufferSize - 1) {}
            Dim extName = If(options(2).EndsWith(".fasta"), ".fasta", ".fq")

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    Dim name1 = options(5) + "\filtered_pe\" + refsView.Item(i - 1).Item(1).ToString + "_1" + extName
                    Dim name2 = options(5) + "\filtered_pe\" + refsView.Item(i - 1).Item(1).ToString + "_2" + extName
                    Dim target = New FileStream(options(5) + "\filtered\" + refsView.Item(i - 1).Item(1).ToString + extName, IO.FileMode.Create, FileAccess.Write)

                    If File.Exists(name1) Then
                        Dim s1 = New FileStream(name1, IO.FileMode.Open, FileAccess.Read, FileShare.Read)
                        Dim readSize As Integer = s1.Read(buffer, 0, bufferSize)
                        While readSize > 0
                            target.Write(buffer, 0, readSize)
                            readSize = s1.Read(buffer, 0, bufferSize)
                        End While
                        s1.Close()

                        If File.Exists(name2) Then
                            Dim s2 = New FileStream(name2, IO.FileMode.Open, FileAccess.Read, FileShare.Read)
                            readSize = s2.Read(buffer, 0, bufferSize)
                            While readSize > 0
                                target.Write(buffer, 0, readSize)
                                readSize = s2.Read(buffer, 0, bufferSize)
                            End While
                            s2.Close()
                        End If
                    End If
                    target.Close()
                End If
            Next
        End If

        If MenuClicked.StartsWith("batch_") Then
            If Not File.Exists(read_count_path) AndAlso options(7) <> 1 Then
                RichTextBox1.AppendText("Error in Filter: " + options(5).Split("/")(UBound(options(5).Split("/"))) + vbCrLf)
            End If
        Else
            If File.Exists(read_count_path) AndAlso options(7) <> 1 Then
                ref_filter_result(read_count_path)
            Else
                MsgBox("Could not find the filter result, please check options and try again!", MsgBoxStyle.Information, "Information")
            End If
        End If
    End Sub
    Public Sub ref_filter_result(ByVal filePath As String)
        Try
            Dim count_dict As New Dictionary(Of String, Integer)

            ' 读取文件内容并将内容存入字典
            Using sr As New StreamReader(filePath)
                While Not sr.EndOfStream
                    Dim line As String = sr.ReadLine().ToLower
                    Dim parts As String() = line.ToLower().Split(","c)

                    If parts.Length >= 2 Then
                        Dim key As String = parts(0)
                        Dim value As Integer

                        If Integer.TryParse(parts(1), value) Then
                            If count_dict.ContainsKey(key) Then
                                count_dict(key) = value
                            Else
                                count_dict.Add(key, value)
                            End If

                        End If
                    End If
                End While
            End Using
            SyncLock refsView
                For i As Integer = 1 To refsView.Count
                    If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                        If count_dict.ContainsKey(refsView.Item(i - 1).Item(1).ToString.ToLower) Then
                            DataGridView1.Rows(i - 1).Cells(5).Value = CInt(count_dict(refsView.Item(i - 1).Item(1).ToString.ToLower))
                        Else
                            DataGridView1.Rows(i - 1).Cells(5).Value = 0
                        End If
                    End If
                Next
            End SyncLock
        Catch ex As Exception
            MsgBox(ex.ToString, MsgBoxStyle.Information, "Information")
        End Try

    End Sub
    Public Sub do_assemble(ByVal options() As String)
        'options = (0:kf,1:ka,2:q1,3:q2,4:ref,5:out_dir,6:lkd,7:rl,8:refilter,9:no_window,10:thread)

        Dim SI_assembler As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\main_assembler.exe",
            .WorkingDirectory = currentDirectory + "temp\",
            .CreateNoWindow = (options(9) = 1),
            .Arguments = "-r " + """" + options(4) + """"
        }
        SI_assembler.Arguments += " -o " + """" + options(5) + """"
        If form_config_basic.CheckBox1.Checked Then
            SI_assembler.Arguments += " -ka 0"
        Else
            SI_assembler.Arguments += " -ka " + options(1)
        End If
        SI_assembler.Arguments += " -k_min " + form_config_basic.NumericUpDown6.Value.ToString
        SI_assembler.Arguments += " -k_max " + form_config_basic.NumericUpDown7.Value.ToString
        SI_assembler.Arguments += " -limit_count " + form_config_basic.NumericUpDown8.Value.ToString
        SI_assembler.Arguments += " -iteration " + form_config_basic.NumericUpDown10.Value.ToString
        SI_assembler.Arguments += " -p " + options(10)
        SI_assembler.Arguments += " -sb " + sb
        Dim process_filter As Process = Process.Start(SI_assembler)
        process_filter.WaitForExit()
        process_filter.Close()

        Dim filePath As String = options(5) + "\result_dict.txt"
        If MenuClicked <> "batch_auto_assemble" Then
            If File.Exists(filePath) Then
                ref_assemble_result(filePath)
            Else
                MsgBox("Could not find assemble result, please check option and try again!", MsgBoxStyle.Information, "Information")
            End If
        Else
            If File.Exists(filePath) = False Then
                RichTextBox1.AppendText("Error in Assemble: " + options(5).Split("/")(UBound(options(5).Split("/"))) + vbCrLf)
            End If
        End If

    End Sub

    Public Sub ref_assemble_result(ByVal filePath As String)
        Try
            Dim result_dict As New Dictionary(Of String, String)
            Dim my_out_dir As String = Path.GetDirectoryName(filePath)
            ' 读取文件内容并将内容存入字典
            Using sr As New StreamReader(filePath)
                While Not sr.EndOfStream
                    Dim line As String = sr.ReadLine()
                    Dim parts As String() = line.Split(","c)

                    If parts.Length >= 3 Then
                        Dim key As String = parts(0)
                        If result_dict.ContainsKey(key) Then
                            result_dict(key) = parts(1) + "," + parts(2)
                        Else
                            result_dict.Add(key, parts(1) + "," + parts(2))
                        End If
                    End If
                End While
            End Using
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    If result_dict.ContainsKey(refsView.Item(i - 1).Item(1).ToString) Then
                        DataGridView1.Rows(i - 1).Cells(6).Value = result_dict(refsView.Item(i - 1).Item(1).ToString).Split(","c)(0)
                        If DataGridView1.Rows(i - 1).Cells(6).Value <> "success" Then
                            'DataGridView1.Rows(i - 1).Cells(6).Value = "failed"
                            DataGridView1.Rows(i - 1).Cells(7).Value = 0
                            'DataGridView1.Rows(i - 1).Cells(8).Value = "failed"
                        End If
                        If File.Exists(my_out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                            Try
                                Dim sr As New StreamReader(my_out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                sr.ReadLine()
                                DataGridView1.Rows(i - 1).Cells(7).Value = sr.ReadLine().Length
                                sr.Close()
                                'DataGridView1.Rows(i - 1).Cells(8).Value = (CInt(result_dict(refsView.Item(i - 1).Item(1).ToString).Split(","c)(1)) * reads_length / CInt(DataGridView1.Rows(i - 1).Cells(7).Value)).ToString("F0")

                            Catch ex As Exception
                                MsgBox(ex.ToString, MsgBoxStyle.Information, "Information")
                                If File.Exists(my_out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                                    File.Delete(my_out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                End If
                                DataGridView1.Rows(i - 1).Cells(6).Value = "failed"
                                DataGridView1.Rows(i - 1).Cells(7).Value = 0
                                'DataGridView1.Rows(i - 1).Cells(8).Value = "failed"
                            End Try

                        End If
                    End If
                End If
            Next
        Catch ex As Exception
            MsgBox(ex.ToString, MsgBoxStyle.Information, "Information")
        End Try

    End Sub
    Dim init_output_folder As Boolean = False
    Private Sub Main_Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        currentDirectory = Application.StartupPath
        initialize_data()
        If TargetOS = "macos" Then
            TextBox1.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "GeneMiner")
            Directory.CreateDirectory(TextBox1.Text)
        Else
            TextBox1.Text = currentDirectory + "results"
        End If


        NumericUpDown10.Maximum = Math.Max(System.Environment.ProcessorCount - 2, 1)
        NumericUpDown10.Value = Math.Max(NumericUpDown10.Maximum / 2, 1)
        AddHandler form_config_basic.ConfirmClicked, AddressOf Basic_ConfirmClickedHandler
        AddHandler form_config_basic.CancelClicked, AddressOf SubCancel
        AddHandler form_config_trim.ConfirmClicked, AddressOf Trim_ConfirmClickedHandler
        AddHandler form_config_trim.CancelClicked, AddressOf SubCancel
        AddHandler form_config_tree.ConfirmClicked, AddressOf Tree_ConfirmClickedHandler
        AddHandler form_config_tree.CancelClicked, AddressOf SubCancel
        AddHandler form_config_combine.ConfirmClicked, AddressOf Combine_ConfirmClickedHandler
        AddHandler form_config_combine.CancelClicked, AddressOf SubCancel
        AddHandler form_config_consensus.ConfirmClicked, AddressOf Consensus_ConfirmClickedHandler
        AddHandler form_config_consensus.CancelClicked, AddressOf SubCancel
        init_output_folder = True
    End Sub


    Private Sub SubCancel()
    End Sub
    Private Sub Main_Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        e.Cancel = True
        settings("language") = language
        SaveSettings(root_path + "analysis\" + "setting.ini", settings)
        End
    End Sub

    Public Sub refresh_DG1()
        DataGridView1.DataSource = refsView
        refsView.AllowNew = False
        refsView.AllowEdit = True

        DataGridView1.Columns(1).ReadOnly = True
        DataGridView1.Columns(2).ReadOnly = True
        DataGridView1.Columns(3).ReadOnly = True
        DataGridView1.Columns(4).ReadOnly = True
        DataGridView1.Columns(5).ReadOnly = True
        DataGridView1.Columns(6).ReadOnly = True
        DataGridView1.Columns(7).ReadOnly = True
        DataGridView1.Columns(8).ReadOnly = True

        DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(4).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(5).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(6).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(7).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(8).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(0).Width = 50
        DataGridView1.Columns(1).Width = 50
        DataGridView1.Columns(2).Width = 160
        DataGridView1.Columns(3).Width = 80
        DataGridView1.Columns(4).Width = 80
        DataGridView1.Columns(5).Width = 80
        DataGridView1.Columns(6).Width = 160
        DataGridView1.Columns(7).Width = 80
        DataGridView1.Columns(8).Width = 80
        DataGridView1.RefreshEdit()
        GC.Collect()
    End Sub

    Public Sub refresh_DG2()
        DataGridView2.DataSource = seqsView
        seqsView.AllowNew = False
        seqsView.AllowEdit = True

        DataGridView2.Columns(1).ReadOnly = True
        DataGridView2.Columns(2).ReadOnly = True
        DataGridView2.Columns(3).ReadOnly = True

        DataGridView2.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView2.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView2.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView2.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView2.Columns(0).Width = 50
        DataGridView2.Columns(1).Width = 50
        DataGridView2.Columns(2).Width = 400
        DataGridView2.Columns(3).Width = 400
        DataGridView2.RefreshEdit()
        GC.Collect()
    End Sub
    Public Sub refresh_DG3()
        form_config_tree.DataGridView1.DataSource = taxonView
        taxonView.AllowNew = False
        taxonView.AllowEdit = True
        form_config_tree.DataGridView1.Columns(1).ReadOnly = True
        form_config_tree.DataGridView1.Columns(0).Width = 50
        form_config_tree.DataGridView1.Columns(1).Width = 350
        form_config_tree.DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
        form_config_tree.DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
        form_config_tree.DataGridView1.RefreshEdit()
        GC.Collect()
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Select Case timer_id
            Case 0
                ProgressBar1.Value = PB_value
                If init_output_folder Then
                    Timer1.Enabled = False
                    If TargetOS = "macos" Then
                        MsgBox("Output directory: GeneMiner folder on desktop.", MsgBoxStyle.Information, "Information")
                        form_config_basic.CheckBox4.Checked = True
                    End If
                    init_output_folder = False
                    Timer1.Enabled = True
                End If
            Case 1
                ProgressBar1.Value = PB_value
                If ProgressBar1.Value = 100 Then
                    PB_value = 0
                    timer_id = 0
                End If

            Case 2
                Timer1.Enabled = False
                refresh_DG1()
                DataGridView1.Update()
                For i As Integer = 0 To refsView.Count - 1
                    DataGridView1.Rows(i).Cells(0).Value = True
                Next

                data_loaded = True
                timer_id = 0
                Timer1.Enabled = True

            Case 3
                Timer1.Enabled = False
                refresh_DG2()
                DataGridView2.Update()
                For i As Integer = 0 To seqsView.Count - 1
                    DataGridView2.Rows(i).Cells(0).Value = True
                Next
                form_config_tree.DataGridView1.DataSource = taxonView
                taxonView.AllowNew = False
                taxonView.AllowEdit = True
                form_config_tree.DataGridView1.Columns(1).ReadOnly = True
                form_config_tree.DataGridView1.RefreshEdit()
                form_config_tree.DataGridView1.Columns(0).Width = 50
                form_config_tree.DataGridView1.Columns(1).Width = 350
                form_config_tree.DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
                form_config_tree.DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
                form_config_tree.DataGridView1.RefreshEdit()

                GC.Collect()
                timer_id = 0
                data_loaded = True
                Timer1.Enabled = True

            Case 4
                If PB_value = -1 Then
                    PB_value = 0
                    timer_id = 0
                Else
                    ProgressBar1.Value = PB_value
                End If
            Case 5
                form_config_split.Show()
                timer_id = 0
            Case 6
                Timer1.Enabled = False
                Dim opendialog As New SaveFileDialog With {
                    .Filter = "FastQ File (*.fq)|*.fq;*.FQ",
                    .FileName = "",
                    .DefaultExt = ".fq",
                    .CheckFileExists = False,
                    .CheckPathExists = True
                }
                Dim resultdialog As DialogResult = opendialog.ShowDialog()
                If resultdialog = DialogResult.OK Then
                    For i As Integer = 1 To seqsView.Count
                        If File.Exists(currentDirectory + "\results\Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".1.fq") And File.Exists(currentDirectory + "\results\Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".2.fq") Then

                            My.Computer.FileSystem.MoveFile(currentDirectory + "\results\Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".1.fq", opendialog.FileName.Substring(0, opendialog.FileName.Length - 3) + "_" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".1.fq", True)
                            My.Computer.FileSystem.MoveFile(currentDirectory + "\results\Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".2.fq", opendialog.FileName.Substring(0, opendialog.FileName.Length - 3) + "_" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".2.fq", True)

                        End If
                    Next
                End If
                Timer1.Enabled = True
                timer_id = 0
            Case 7
                form_config_plasty.Show()
                timer_id = 0
            Case 8 '批量细胞器拼接
                Timer1.Enabled = False
                DataGridView1.EndEdit()
                Dim th1 As New Thread(AddressOf batch_assemble_organelle)
                If cpg_down_mode = 4 Then
                    th1.Start("cp")
                ElseIf cpg_down_mode = 5 Then
                    th1.Start("mito_plant")
                ElseIf cpg_down_mode = 9 Then
                    th1.Start("mito")
                End If
                timer_id = 0
                Timer1.Enabled = True
            Case 9
                Timer1.Enabled = False
                refresh_DG1()
                data_loaded = True
                form_config_plasty.Show()
                timer_id = 0
                Timer1.Enabled = True
            Case 10
                form_config_dated.Show()
                timer_id = 0
        End Select
    End Sub
    Public Function check_batch_folder()
        Dim is_ready As Boolean = True
        For batch_i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                If My.Computer.FileSystem.DirectoryExists((TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")) = False Then
                    is_ready = False
                    Exit For
                End If
            End If
        Next
        Return is_ready
    End Function


    Public Sub batch_assemble_organelle(ByVal database_type As String)
        batch_task(True, False, False, True, database_type)
        MsgBox("Analysis completed! Please check Organelle folder in output.", MsgBoxStyle.Information, "Information")
    End Sub
    Private Sub 测序文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 测序文件ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "FastQ File(*.fq;*.fq.gz)|*.fq;*.fastq;*.FQ;*.fq.gz;*.gz|Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
            .FileName = "",
            .Multiselect = True,
            .DefaultExt = ".fq",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            For Each file_name As String In opendialog.FileNames
                Dim RegCHZN As New Regex("[\u4e00-\u9fa5]")
                Dim m As Match = RegCHZN.Match(Path.GetFileNameWithoutExtension(file_name))
                If m.Success Then
                    MsgBox("测序文件的文件名中不得含有中文（亚洲语言字符）！" + Chr(13) + "Sequencing file names must not include Asian language characters.", MsgBoxStyle.Information, "Information")
                    Exit Sub
                End If
            Next

            Dim paired As Boolean = False

            If opendialog.FileNames.Length > 1 Then
                paired = (MessageBox.Show("Are these paired sequencing files?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes)
            End If

            If paired Then
                If opendialog.FileNames.Length Mod 2 = 0 Then
                    data_loaded = False
                    Dim sortedFileNames As String() = opendialog.FileNames.OrderBy(Function(path) path).ToArray()
                    For i As Integer = 1 To opendialog.FileNames.Length \ 2
                        Dim newrow(2) As String
                        seqsView.AllowNew = True
                        seqsView.AddNew()
                        newrow(0) = seqsView.Count
                        newrow(1) = sortedFileNames((i - 1) * 2)
                        newrow(2) = sortedFileNames((i - 1) * 2 + 1)

                        Dim newrow_taxon(0) As String
                        taxonView.AllowNew = True
                        taxonView.AddNew()
                        newrow_taxon(0) = taxonView.Count.ToString + "_" + make_out_name(Path.GetFileNameWithoutExtension(sortedFileNames((i - 1) * 2)), Path.GetFileNameWithoutExtension(sortedFileNames((i - 1) * 2 + 1)))

                        seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow
                        taxonView.Item(taxonView.Count - 1).Row.ItemArray = newrow_taxon
                    Next
                    timer_id = 3
                Else
                    MsgBox("The files are not in pairs (the number of files cannot be divided by 2 evenly).", MsgBoxStyle.Information, "Information")
                End If
            Else
                data_loaded = False
                Dim sortedFileNames As String() = opendialog.FileNames.OrderBy(Function(path) path).ToArray()
                For i As Integer = 1 To opendialog.FileNames.Length
                    Dim newrow(2) As String
                    seqsView.AllowNew = True
                    seqsView.AddNew()
                    newrow(0) = seqsView.Count
                    newrow(1) = sortedFileNames(i - 1)
                    newrow(2) = sortedFileNames(i - 1)

                    Dim newrow_taxon(0) As String
                    taxonView.AllowNew = True
                    taxonView.AddNew()
                    newrow_taxon(0) = taxonView.Count.ToString + "_" + make_out_name(Path.GetFileNameWithoutExtension(sortedFileNames(i - 1)), Path.GetFileNameWithoutExtension(sortedFileNames(i - 1)))

                    seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow
                    taxonView.Item(taxonView.Count - 1).Row.ItemArray = newrow_taxon
                Next
                timer_id = 3
            End If
        End If
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub DataGridView1_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles DataGridView1.DataBindingComplete
        'If data_loaded = False And refsView.Count > 0 Then
        '    For i As Integer = 1 To refsView.Count
        '        DataGridView1.Rows(i - 1).Cells(0).Value = True
        '    Next
        'End If
    End Sub

    Private Sub DataGridView2_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView2.CellContentClick

    End Sub

    Private Sub DataGridView2_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles DataGridView2.DataBindingComplete
        'If data_loaded = False And seqsView.Count > 0 Then
        '    For i As Integer = 1 To seqsView.Count
        '        DataGridView2.Rows(i - 1).Cells(0).Value = True
        '    Next
        'End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim opendialog As New FolderBrowserDialog
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            Dim RegCHZN As New Regex("[\u4e00-\u9fa5]")
            Dim m As Match = RegCHZN.Match(opendialog.SelectedPath)
            If m.Success Then
                MsgBox("结果所在路径不得含有中文（亚洲语言字符）！" + Chr(13) + "The path for the results should not include Asian language characters.", MsgBoxStyle.Information, "Information")
                Exit Sub
            End If
            If opendialog.SelectedPath.EndsWith(":\") Then
                MsgBox("Please do not save to the root directory!", MsgBoxStyle.Information, "Information")
            Else
                If Directory.GetFileSystemEntries(opendialog.SelectedPath).Length > 0 Then
                    Dim result As DialogResult = MessageBox.Show("The folder is not empty, its contents may be deleted. Are you sure to use this folder?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    ' 根据用户的选择执行相应的操作
                    If result = DialogResult.Yes Then
                        TextBox1.Text = opendialog.SelectedPath
                    End If
                Else
                    TextBox1.Text = opendialog.SelectedPath
                End If
            End If
        End If
    End Sub


    Private Sub 拼接ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 拼接ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            MenuClicked = "assemble"
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = False
            form_config_basic.GroupBox2.Enabled = False
            form_config_basic.GroupBox3.Enabled = False
            form_config_basic.GroupBox4.Enabled = True
            form_config_basic.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If

    End Sub
    Private Sub menu_assemble()
        Dim refs_count As Integer = 0
        Dim seqs_count As Integer = 0
        Dim has_assemble As Boolean = False
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = TextBox1.Text.Replace("\", "/")
        q1 = ""
        q2 = ""
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)


        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                If File.Exists(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                    has_assemble = True
                End If
            End If
        Next
        If has_assemble Then
            'Dim result As DialogResult = MessageBox.Show("Reassemble All Selected Genes?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            '' 根据用户的选择执行相应的操作
            'If result = DialogResult.Yes Then
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    If File.Exists(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                        File.Delete(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                    End If
                End If
            Next
            'End If
        End If

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
                q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next

        If seqs_count >= 1 And refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_assemble)
            Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "0", no_window, current_thread}
            th1.Start(my_options)
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub


    Private Sub 迭代ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 迭代ToolStripMenuItem.Click

    End Sub
    Public Sub do_iteration(ByVal times As Integer)
        For x As Integer = 1 To times
            PB_value = x / times * 100
            DataGridView1.EndEdit()
            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = (TextBox1.Text + "/iteration").Replace("\", "/")

            Directory.CreateDirectory(out_dir)
            q1 = ""
            q2 = ""
            'DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    If x = 1 Then
                        refs_count += 1
                    Else
                        If File.Exists(TextBox1.Text + "\iteration\contigs_all\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                            refs_count += 1
                            '叠加参考序列
                            CombineFiles(ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", TextBox1.Text + "\iteration\contigs_all\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                        End If
                    End If
                End If
            Next
            DeleteDir(out_dir)
            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    Else
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                    End If
                End If
            Next
            If seqs_count >= 1 And refs_count >= 1 Then
                'options = (0:kf,1:kr,2:q1,3:q2,4:ref,5:out_dir,6:lkd,7:rl,8:refilter,9:no_window,10:thread)
                Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "0", no_window, Math.Min(current_thread, filter_thread)}
                do_filer_assemble(my_options)
            End If
        Next
        PB_value = -1
    End Sub
    Public Sub do_filer_assemble(ByVal options() As String)
        'options = (0:kf,1:kr,2:q1,3:q2,4:ref,5:out_dir,6:lkd,7:rl,8:refilter,9:no_window,10:thread)
        options(8) = "0"
        do_filter(options)
        options(10) = current_thread
        options(8) = "1"
        do_filter(options)
        do_assemble(options)
    End Sub

    Private Sub 进一步过滤ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 进一步过滤ToolStripMenuItem.Click
        MenuClicked = "refilter"
        form_config_basic.CheckBox3.Checked = False
        form_config_basic.CheckBox4.Enabled = False
        form_config_basic.GroupBox2.Enabled = False
        form_config_basic.GroupBox3.Enabled = True
        form_config_basic.GroupBox4.Enabled = False
        form_config_basic.Show()
    End Sub
    Private Sub menu_refilter()
        DataGridView1.EndEdit()
        Dim refs_count As Integer = 0
        Dim seqs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = TextBox1.Text.Replace("\", "/")
        q1 = ""
        q2 = ""
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)

        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
                q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next

        If seqs_count >= 1 And refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_filter)
            Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "1", no_window, current_thread}
            th1.Start(my_options)
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub 从头过滤ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 从头过滤ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            MenuClicked = "filter"
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = False
            form_config_basic.GroupBox2.Enabled = True
            form_config_basic.GroupBox3.Enabled = False
            form_config_basic.GroupBox4.Enabled = False
            form_config_basic.NumericUpDown1.Value = If(form_config_calculate.kf = 0, 31, form_config_calculate.kf)
            form_config_basic.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If

    End Sub
    Private Sub menu_filter()
        If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 Then
            Dim result As DialogResult = MessageBox.Show("Clear the output directory? If you are optimizing for previous results, please select 'NO'!", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                DeleteDir(TextBox1.Text)
                Directory.CreateDirectory(TextBox1.Text)
            End If
        End If

        Dim refs_count As Integer = 0
        Dim seqs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = TextBox1.Text.Replace("\", "/")
        q1 = ""
        q2 = ""
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        If File.Exists(out_dir + "\kmer_dict_k" + k1.ToString + ".dict") Then
            File.Delete(out_dir + "\kmer_dict_k" + k1.ToString + ".dict")
        End If

        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
                q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next

        If seqs_count >= 1 And refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_filter)
            Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "0", no_window, Math.Min(current_thread, filter_thread)}
            th1.Start(my_options)
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Function check_data_count()
        Dim seqs_count As Integer = 0
        Dim refs_count As Integer = 0
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
            End If
        Next

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
            End If
        Next

        Return {refs_count, seqs_count}
    End Function
    Public Sub load_refs(ByVal file_names() As String)
        Dim count As Integer = 1
        If file_names(0).ToLower.EndsWith(".gb") Then
            Dim sw As New StreamWriter(root_path + "temp\temp.gb")
            For Each file_name As String In file_names
                PB_value = 100 * count / file_names.Length
                Dim sr As New StreamReader(file_name)
                sw.Write(sr.ReadToEnd)
                sr.Close()
                Interlocked.Add(count, 1)
            Next
            sw.Close()
            current_file = root_path + "temp\temp.gb"
            form_config_split.Show()
            refs_type = "gb"
        Else
            DeleteDir(root_path + "temp\org_seq")
            Directory.CreateDirectory(root_path + "temp\org_seq")
            For Each FileName As String In file_names
                PB_value = 100 * count / file_names.Length
                safe_copy(FileName, root_path + "temp\org_seq\" + Path.GetFileNameWithoutExtension(FileName).Replace(" ", "_").Replace(".", "_").Replace("-", "_") + ".fasta")
                Interlocked.Add(count, 1)
            Next
            refs_type = "fasta"
            refresh_file()
            timer_id = 2
        End If
        PB_value = 0
    End Sub

    Private Sub 载入参考序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入参考序列ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa|GenBank File|*.gb",
            .FileName = "",
            .Multiselect = True,
            .DefaultExt = ".fas",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            current_file = opendialog.FileName
            mydata_Dataset.Tables("Refs Table").Clear()
            DataGridView1.DataSource = Nothing
            data_loaded = False
            If UBound(opendialog.FileNames) >= 1 Then
                Dim th1 As New Thread(AddressOf load_refs)
                th1.Start(opendialog.FileNames)
            Else
                current_file = opendialog.FileName
                Dim result As DialogResult = MessageBox.Show("Importing as gene list? If importing as file list, select 'No'", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    If opendialog.FileName.ToLower.EndsWith(".gb") Then
                        form_config_split.Show()
                        refs_type = "gb"
                    Else
                        DeleteDir(root_path + "temp\org_seq")
                        Directory.CreateDirectory(root_path + "temp\org_seq")
                        safe_copy(current_file, root_path + "temp\org_seq\" + Path.GetFileNameWithoutExtension(current_file).Replace(" ", "_").Replace(".", "_").Replace("-", "_") + ".fasta", True)
                        refs_type = "fasta"
                        refresh_file()
                        timer_id = 2
                    End If
                Else
                    DeleteDir(root_path + "temp\org_seq")
                    Directory.CreateDirectory(root_path + "temp\org_seq")

                    Dim SI_split_file As New ProcessStartInfo With {
                        .FileName = currentDirectory + "analysis\split_file.exe",
                        .WorkingDirectory = currentDirectory + "analysis\",
                        .CreateNoWindow = True,
                        .Arguments = "-i " + """" + current_file + """"
                    }
                    SI_split_file.Arguments += " -o " + """" + root_path + "temp\org_seq" + """"
                    If opendialog.FileName.ToLower.EndsWith(".gb") Then
                        SI_split_file.Arguments += " -f genbank"
                        refs_type = "gb"
                    Else
                        SI_split_file.Arguments += " -f fasta"
                        refs_type = "fasta"
                    End If
                    Dim process_split_file As Process = Process.Start(SI_split_file)
                    process_split_file.WaitForExit()
                    process_split_file.Close()
                    refresh_file()
                    timer_id = 2
                End If

            End If

        End If
    End Sub

    Private Sub 导出ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导出ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog With {
            .Filter = "CSV File (*.csv)|*.csv;*.CSV",
            .FileName = "",
            .DefaultExt = ".csv",
            .CheckFileExists = False,
            .CheckPathExists = False
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            If opendialog.FileName.ToLower.EndsWith(".csv") Then
                save_datagrid1(opendialog.FileName)
                MsgBox("Export Complete!", MsgBoxStyle.Information, "Information")
            End If
        End If
    End Sub

    Public Sub save_datagrid1(ByVal file_path As String)
        DataGridView1.EndEdit()
        Dim dw As New StreamWriter(file_path, False)
        Dim state_line As String = "Selected,ID,Name"
        For j As Integer = 3 To DataGridView1.ColumnCount - 1
            state_line += "," + DataGridView1.Columns(j).HeaderText
        Next
        dw.WriteLine(state_line)
        For i As Integer = 1 To refsView.Count
            state_line = DataGridView1.Rows(i - 1).Cells(0).Value.ToString + "," + i.ToString
            For j As Integer = 2 To DataGridView1.ColumnCount - 1
                state_line += "," + refsView.Item(i - 1).Item(j - 1)
            Next
            dw.WriteLine(state_line)
        Next
        dw.Close()
    End Sub
    Private Sub 全自动ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全自动ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = False
            form_config_basic.GroupBox2.Enabled = True
            form_config_basic.GroupBox3.Enabled = True
            form_config_basic.GroupBox4.Enabled = True
            form_config_basic.NumericUpDown1.Value = If(form_config_calculate.kf = 0, 31, form_config_calculate.kf)
            MenuClicked = "auto_assemble"
            form_config_basic.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If

    End Sub
    Private Sub menu_auto_assemble()
        Directory.CreateDirectory(TextBox1.Text)
        If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 Then
            Dim result As DialogResult = MessageBox.Show("Clear the output directory?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                DeleteDir(TextBox1.Text)
                Directory.CreateDirectory(TextBox1.Text)
            End If
        End If
        Dim refs_count As Integer = 0
        Dim seqs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = TextBox1.Text.Replace("\", "/")
        q1 = ""
        q2 = ""
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        If File.Exists(out_dir + "\kmer_dict_k" + k1.ToString + ".dict") Then
            File.Delete(out_dir + "\kmer_dict_k" + k1.ToString + ".dict")
        End If
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                If File.Exists(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                    File.Delete(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                End If
            End If
        Next

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
                q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next

        If seqs_count >= 1 And refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_filer_assemble)
            Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "0", no_window, Math.Min(current_thread, filter_thread)}

            th1.Start(my_options)
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub 导出参考序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导出参考序列ToolStripMenuItem.Click
        Dim opendialog As New FolderBrowserDialog
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", opendialog.SelectedPath + "\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                End If
            Next
            MsgBox("Export Complete!", MsgBoxStyle.Information, "Information")

        End If
    End Sub

    Private Sub 构建质体基因组ToolStripMenuItem_Click(sender As Object, e As EventArgs)


    End Sub

    Private Sub 导出测序文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导出测序文件ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            Dim opendialog As New FolderBrowserDialog
            Dim resultdialog As DialogResult = opendialog.ShowDialog()
            If resultdialog = DialogResult.OK Then
                If opendialog.SelectedPath.EndsWith(":\") Then
                    MsgBox("Please do not save to the root directory!", MsgBoxStyle.Information, "Information")
                Else

                    Dim my_input As String = InputBox("Number of Reads to SKIP (Million)", "SKIP", 0)
                    Dim reads_skip, reads_keep As Integer
                    If Not Integer.TryParse(my_input, reads_skip) Then
                        Exit Sub
                    End If
                    my_input = InputBox("Number of Reads to KEEP (Million)", "KEEP", 10)
                    If Not Integer.TryParse(my_input, reads_keep) Then
                        Exit Sub
                    End If
                    Dim th1 As New Threading.Thread(AddressOf export_seq)
                    th1.Start({reads_skip.ToString, reads_keep.ToString, opendialog.SelectedPath})
                End If

            End If

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If

    End Sub
    Public Sub export_seq(ByVal para() As String)
        For i As Integer = 1 To seqsView.Count
            If form_main.DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(i - 1).Cells(3).Value.ToString))

                Dim SI_build_fq As New ProcessStartInfo With {
                    .FileName = currentDirectory + "analysis\build_fq.exe",
                    .WorkingDirectory = currentDirectory + "analysis\",
                    .CreateNoWindow = False,
                    .Arguments = "-i1 " + """" + form_main.DataGridView2.Rows(i - 1).Cells(2).Value.ToString + """"
                }
                SI_build_fq.Arguments += " -i2 " + """" + form_main.DataGridView2.Rows(i - 1).Cells(3).Value.ToString + """"
                SI_build_fq.Arguments += " -o " + """" + para(2) + """"
                SI_build_fq.Arguments += " -o1 " + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + "_" + folder_name + ".1"
                SI_build_fq.Arguments += " -o2 " + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + "_" + folder_name + ".2"
                SI_build_fq.Arguments += " -skip " + para(0).ToString
                SI_build_fq.Arguments += " -m_reads " + para(1).ToString
                Dim process_build_fq As Process = Process.Start(SI_build_fq)
                process_build_fq.WaitForExit()
                process_build_fq.Close()
            End If
        Next
        MsgBox("Export Complete!", MsgBoxStyle.Information, "Information")

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If TextBox1.Text <> "" Then
            Process.Start("explorer.exe", TextBox1.Text)
        End If
    End Sub

    Private Sub 过滤ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 过滤ToolStripMenuItem.Click

    End Sub


    Public Sub do_align()
        Directory.CreateDirectory(TextBox1.Text + "\aligned\")
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                 Interlocked.Add(count, 1)
                                                                 PB_value = count / refsView.Count * 100
                                                                 If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                     'If File.Exists(TextBox1.Text + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                                                                     Try
                                                                         Dim in_path As String = ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta"
                                                                         Dim out_path As String = TextBox1.Text + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta"
                                                                         If align_app = "muscle" Then
                                                                             do_muscle_align(in_path, out_path)
                                                                         Else
                                                                             do_mafft_align(in_path, out_path)
                                                                         End If
                                                                     Catch ex As Exception
                                                                         File.AppendAllText(TextBox1.Text + "\log.txt", "Could not do alignment for " & DataGridView1.Rows(i - 1).Cells(2).ToString & Environment.NewLine)
                                                                     End Try
                                                                 End If
                                                             End Sub)
        PB_value = -1
        Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        ' 根据用户的选择执行相应的操作
        If result = DialogResult.Yes Then
            Process.Start("explorer.exe", """" + out_dir.Replace("/", "\") + "\aligned" + """")
        End If
    End Sub

    Private Sub 清空数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 清空数据ToolStripMenuItem.Click
        mydata_Dataset.Tables("Data Table").Clear()
        'DataGridView2.DataSource = Nothing

        mydata_Dataset.Tables("Taxon Table").Clear()

    End Sub

    Private Sub 迭代ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 迭代ToolStripMenuItem1.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = False
            form_config_basic.GroupBox2.Enabled = True
            form_config_basic.GroupBox3.Enabled = True
            form_config_basic.GroupBox4.Enabled = True
            MenuClicked = "iteration"
            form_config_basic.Show()
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub menu_iteration()
        Dim refs_count As Integer = 0
        Dim seqs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = (TextBox1.Text + "/iteration").Replace("\", "/")
        DeleteDir(out_dir)
        Directory.CreateDirectory(out_dir)
        q1 = ""
        q2 = ""
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)

        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                If File.Exists(TextBox1.Text + "\contigs_all\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                    refs_count += 1
                    safe_copy(TextBox1.Text + "\contigs_all\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                End If
            End If
        Next

        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                seqs_count += 1
                q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next
        If seqs_count >= 1 And refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_filer_assemble)
            Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "0", no_window, Math.Min(current_thread, filter_thread)}
            th1.Start(my_options)
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub 重新拼接ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = (TextBox1.Text + "/iteration").Replace("\", "/")
            DeleteDir(out_dir + "/results")
            q1 = ""
            q2 = ""
            DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    If File.Exists(TextBox1.Text + "\contigs_all\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                        refs_count += 1
                        safe_copy(TextBox1.Text + "\contigs_all\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                    End If
                End If
            Next

            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    q1 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    If DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                    Else
                        q2 += " " + """" + DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                    End If
                End If
            Next
            If seqs_count >= 1 And refs_count >= 1 Then
                Dim th1 As New Thread(AddressOf do_assemble)
                Dim my_options() As String = {k1, k2, q1, q2, ref_dir, out_dir, "kmer_dict_k" + k1.ToString + ".dict", 0, "0", no_window, current_thread}
                th1.Start(my_options)
            Else
                MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
            End If
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Public Sub do_muscle_align(ByVal in_path As String, ByVal out_path As String)
        Dim startInfo As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\muscle_wrapper.exe",
            .WorkingDirectory = currentDirectory + "analysis\",
            .CreateNoWindow = True,
            .Arguments = "-i " + """" + in_path + """" + " -o " + """" + out_path + """"
        }
        Dim process As Process = Process.Start(startInfo)
        process.WaitForExit()
        process.Close()
    End Sub

    Public Sub do_mafft_align(ByVal in_path As String, ByVal out_path As String, Optional method As String = "--auto ")
        Dim SI_mafft As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\mafft-win\mafft.bat",
            .WorkingDirectory = currentDirectory + "analysis\mafft-win\",
            .CreateNoWindow = True,
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .Arguments = method + "--inputorder " + """" + in_path + """" + ">" + """" + out_path + """"
        }
        Dim process_mafft As Process = New Process With {
            .StartInfo = SI_mafft
        }
        AddHandler process_mafft.OutputDataReceived, Sub(sender, e)
                                                         If Not String.IsNullOrEmpty(e.Data) Then
                                                             '处理输出数据
                                                             Console.WriteLine(e.Data)
                                                         End If
                                                     End Sub
        AddHandler process_mafft.ErrorDataReceived, Sub(sender, e)
                                                        If Not String.IsNullOrEmpty(e.Data) Then
                                                            '处理错误数据
                                                            Console.WriteLine("ERROR: " + e.Data)
                                                        End If
                                                    End Sub
        process_mafft.Start()
        process_mafft.BeginOutputReadLine()
        process_mafft.BeginErrorReadLine()
        process_mafft.WaitForExit()
        process_mafft.Close()
    End Sub


    Public Sub do_cut()
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        If My.Computer.FileSystem.DirectoryExists(out_dir + "\results") Then
            If Directory.GetFileSystemEntries(out_dir + "\results").Length > 0 Then
                For i As Integer = 1 To refsView.Count
                    If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" And File.Exists(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                        CombineFiles(ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                    End If
                Next
            End If
        End If
        timer_id = 4
        PB_value = 0
        Dim th1 As New Thread(AddressOf run_cut)
        th1.Start()

    End Sub

    Public Sub run_cut(ByVal pross_id As Integer)
        Directory.CreateDirectory(out_dir + "\aligned\")
        Directory.CreateDirectory(out_dir + "\trimed\")
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                 Interlocked.Add(count, 1)
                                                                 PB_value = count / refsView.Count * 100
                                                                 If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                     If File.Exists(out_dir + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                                                                         Try
                                                                             Dim in_path As String = ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta"
                                                                             Dim out_path As String = out_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta"
                                                                             If align_app = "muscle" Then
                                                                                 do_muscle_align(in_path, out_path)
                                                                             Else
                                                                                 do_mafft_align(in_path, out_path)
                                                                             End If
                                                                             If TargetOS = "macos" Then
                                                                                 Thread.Sleep(100)
                                                                             End If

                                                                             Dim SI_trimed As New ProcessStartInfo With {
                                                                                 .FileName = currentDirectory + "analysis\trimal.exe",
                                                                                 .WorkingDirectory = currentDirectory + "analysis\",
                                                                                 .CreateNoWindow = True,
                                                                                 .Arguments = "-in " + """" + out_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """"
                                                                             }
                                                                             SI_trimed.Arguments += " -out " + """" + out_dir + "\trimed\" + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """"
                                                                             SI_trimed.Arguments += " -automated1"
                                                                             Dim process_trimed As Process = Process.Start(SI_trimed)
                                                                             process_trimed.WaitForExit()
                                                                             process_trimed.Close()
                                                                         Catch ex As Exception
                                                                             File.AppendAllText(TextBox1.Text + "\log.txt", "Could not do cut for " & DataGridView1.Rows(i - 1).Cells(2).ToString & Environment.NewLine)
                                                                         End Try


                                                                     End If
                                                                 End If
                                                             End Sub)
        PB_value = -1
    End Sub

    'Private Sub 刷新数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 刷新数据ToolStripMenuItem.Click
    '    out_dir = TextBox1.Text
    '    Dim filePath As String = TextBox1.Text + "\ref_reads_count_dict.txt"
    '    If File.Exists(filePath) Then
    '        ref_filter_result(filePath)
    '    End If
    '    filePath = TextBox1.Text + "\result_dict.txt"
    '    If File.Exists(filePath) Then
    '        ref_assemble_result(filePath)
    '    End If
    'End Sub

    Private Sub 过滤拼接ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 过滤拼接ToolStripMenuItem.Click
        Directory.CreateDirectory(TextBox1.Text)
        If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 And DebugToolStripMenuItem.Checked = False Then
            Dim result As DialogResult = MessageBox.Show("The result folder is not empty. Are you sure to proceed?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.No Then
                Exit Sub
            End If
        End If
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            MenuClicked = "batch_auto_assemble"
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = True
            form_config_basic.GroupBox2.Enabled = True
            form_config_basic.GroupBox3.Enabled = True
            form_config_basic.GroupBox4.Enabled = True
            form_config_basic.NumericUpDown1.Value = If(form_config_calculate.kf = 0, 31, form_config_calculate.kf)
            form_config_basic.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub menu_batch_auto_assemble()
        Dim refs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)

        timer_id = 4
        PB_value = 0
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        If refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf batch_filter_assemble)
            th1.Start()
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub batch_filter_assemble()
        batch_task(True, True, True, False)
        MsgBox("Analysis completed!", MsgBoxStyle.Information, "Information")
    End Sub


    'Private Sub 过滤拼接切齐ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 过滤拼接切齐ToolStripMenuItem1.Click
    '    If TextBox1.Text <> "" Then
    '        DataGridView1.EndEdit()
    '        Dim refs_count As Integer = 0
    '        timer_id = 4
    '        PB_value = 0
    '        For i As Integer = 1 To refsView.Count
    '            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
    '                refs_count += 1
    '            End If
    '        Next
    '        If refs_count >= 1 Then
    '            timer_id = 4
    '            PB_value = 0
    '            Dim th1 As New Thread(AddressOf batch_trim)
    '            th1.Start()
    '        Else
    '            MsgBox("Please select at least one reference and one sequencing data!")
    '        End If
    '    Else
    '        MsgBox("Please select an output folder!")
    '    End If
    'End Sub
    Public Sub pre_align()
        Dim tmp_aligns As String = (currentDirectory + "temp\temp_aligns\").Replace("\", "/")
        DeleteDir(tmp_aligns)
        Directory.CreateDirectory(tmp_aligns)
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        '拷贝到临时文件夹
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next

        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        '生成排序的临时文件
        Parallel.For(0, refsView.Count, parallelOptions, Sub(i)
                                                             Interlocked.Add(count, 1)
                                                             PB_value = count / refsView.Count * 100
                                                             If DataGridView1.Rows(i).Cells(0).FormattedValue.ToString = "True" Then
                                                                 Try
                                                                     Dim in_path As String = ref_dir + DataGridView1.Rows(i).Cells(2).Value.ToString + ".fasta"
                                                                     Dim out_path As String = tmp_aligns + DataGridView1.Rows(i).Cells(2).Value.ToString + ".fasta"
                                                                     If align_app = "muscle" Then
                                                                         do_muscle_align(in_path, out_path)
                                                                     Else
                                                                         do_mafft_align(in_path, out_path)
                                                                     End If

                                                                 Catch ex As Exception
                                                                     File.AppendAllText(TextBox1.Text + "\log.txt", "Could not do muscle for " & DataGridView1.Rows(i).Cells(2).ToString & Environment.NewLine)
                                                                 End Try

                                                             End If
                                                         End Sub)
    End Sub

    Private Sub 合并结果ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        MenuClicked = "combine"
        form_config_combine.Show()

    End Sub

    Public Sub do_combine(ByVal do_align As Boolean)
        Dim combine_res_dir As String = TextBox1.Text + "\combined_results\"
        Dim combine_trimed_dir As String = TextBox1.Text + "\combined_trimed\"
        Dim source_folder As String = "results"
        If form_config_combine.CheckBox2.Checked Then
            source_folder = "blast"
        End If
        DeleteDir(combine_res_dir)
        DeleteDir(combine_trimed_dir)
        Directory.CreateDirectory(combine_res_dir)
        If do_align Then
            Directory.CreateDirectory(combine_res_dir + "\aligned\")
            Directory.CreateDirectory(combine_trimed_dir)
        End If

        Dim count As Integer = 0
        Dim result_count_dict As New ConcurrentDictionary(Of String, Integer)
        Dim result_length_dict As New ConcurrentDictionary(Of String, Integer)
        Dim max_distance() As Single
        Dim passed_genes() As Boolean
        ReDim max_distance(refsView.Count - 1)
        ReDim passed_genes(refsView.Count - 1)
        For i As Integer = 0 To refsView.Count - 1
            passed_genes(i) = True
            max_distance(i) = -1
            result_count_dict.TryAdd(refsView.Item(i).Item(1).ToString.ToLower, 0)
            result_length_dict.TryAdd(refsView.Item(i).Item(1).ToString.ToLower, 0)
        Next

        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                 Dim sw_res As New StreamWriter(combine_res_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", False, utf8WithoutBom)
                                                                 For batch_i As Integer = 1 To seqsView.Count
                                                                     Interlocked.Add(count, 1)
                                                                     PB_value = count / refsView.Count / seqsView.Count * 100
                                                                     If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                         Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                                                                         folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                                                                         Dim temp_out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                                                                         Dim result_path As String = Path.Combine(temp_out_dir, source_folder, refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                         Dim sr_line As String = ""
                                                                         If File.Exists(result_path) Then
                                                                             Dim sr_res As New StreamReader(result_path)
                                                                             sr_line = sr_res.ReadLine
                                                                             sr_line = sr_res.ReadLine?.Trim
                                                                             If sr_line IsNot Nothing AndAlso sr_line <> "" Then
                                                                                 sw_res.WriteLine(">" + batch_i.ToString + "_" + folder_name)
                                                                                 sw_res.WriteLine(sr_line)
                                                                                 result_length_dict(refsView.Item(i - 1).Item(1).ToString.ToLower) += sr_line.Length
                                                                                 result_count_dict(refsView.Item(i - 1).Item(1).ToString.ToLower) += 1
                                                                             End If
                                                                             sr_res.Close()
                                                                         End If
                                                                     End If
                                                                 Next
                                                                 sw_res.Close()
                                                                 If result_count_dict(refsView.Item(i - 1).Item(1).ToString.ToLower) = 0 Then
                                                                     safe_delete(combine_res_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                 End If
                                                                 If do_align Then
                                                                     If form_config_combine.ComboBox1.Text = "muscle" Then
                                                                         do_muscle_align(combine_res_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                     Else
                                                                         do_mafft_align(combine_res_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                     End If

                                                                     Dim passed As Boolean = File.Exists(combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")

                                                                     If passed AndAlso form_config_combine.CheckBox3.Checked Then
                                                                         Dim SI_fix As New ProcessStartInfo With {
                                                                             .FileName = currentDirectory + "analysis\fix_alignment.exe",
                                                                             .WorkingDirectory = currentDirectory + "analysis\",
                                                                             .CreateNoWindow = True,
                                                                             .Arguments = "-f " + """" + combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """" + " -n " + CStr(form_config_combine.NumericUpDown1.Value) + " -p " + form_config_combine.TextBox2.Text
                                                                         }
                                                                         Dim process_fix As Process = Process.Start(SI_fix)
                                                                         process_fix.WaitForExit()
                                                                         process_fix.Close()

                                                                         passed = File.Exists(combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                     End If

                                                                     If passed Then
                                                                         max_distance(i - 1) = CalculateMaxDifference(combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")

                                                                         Dim SI_trimed As New ProcessStartInfo With {
                                                                             .FileName = currentDirectory + "analysis\trimal.exe",
                                                                             .WorkingDirectory = currentDirectory + "analysis\",
                                                                             .CreateNoWindow = True,
                                                                             .Arguments = "-in " + """" + combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """"
                                                                         }
                                                                         SI_trimed.Arguments += " -out " + """" + combine_trimed_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """"
                                                                         SI_trimed.Arguments += " -automated1"
                                                                         Dim process_trimed As Process = Process.Start(SI_trimed)
                                                                         process_trimed.WaitForExit()
                                                                         process_trimed.Close()

                                                                         If form_config_combine.CheckBox1.Checked Then
                                                                             replace_gap2missing(combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                             replace_gap2missing(combine_trimed_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                         End If
                                                                     End If

                                                                     passed_genes(i - 1) = passed
                                                                 End If
                                                             End Sub)

        Dim filter_count_dict As New Dictionary(Of String, Integer)
        For batch_i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                Dim temp_out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                Dim filter_file As String = Path.Combine(temp_out_dir, "ref_reads_count_dict.txt")
                If File.Exists(filter_file) Then
                    Using sr As New StreamReader(filter_file)
                        While Not sr.EndOfStream
                            Dim line As String = sr.ReadLine().ToLower
                            Dim parts As String() = line.Split(","c)
                            If parts.Length >= 2 Then
                                Dim key As String = parts(0)
                                Dim value As Integer
                                If Integer.TryParse(parts(1), value) Then
                                    If filter_count_dict.ContainsKey(key) Then
                                        filter_count_dict(key) += value
                                    Else
                                        filter_count_dict.Add(key, value)
                                    End If
                                End If
                            End If
                        End While
                    End Using
                End If
            End If
        Next
        For i As Integer = 1 To refsView.Count
            If do_align Then
                If File.Exists(combine_res_dir + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                    refsView.Item(i - 1).Item(7) = max_distance(i - 1)
                ElseIf max_distance(i - 1) > 0 Then
                    refsView.Item(i - 1).Item(7) = max_distance(i - 1)
                Else
                    refsView.Item(i - 1).Item(7) = ""
                End If
            End If
            If seqsView.Count > 0 Then
                If filter_count_dict.ContainsKey(refsView.Item(i - 1).Item(1).ToString.ToLower) Then
                    refsView.Item(i - 1).Item(4) = CInt(filter_count_dict(refsView.Item(i - 1).Item(1).ToString.ToLower) / seqsView.Count)
                End If
                If result_count_dict.ContainsKey(refsView.Item(i - 1).Item(1).ToString.ToLower) Then
                    refsView.Item(i - 1).Item(5) = result_count_dict(refsView.Item(i - 1).Item(1).ToString.ToLower)
                End If
                If result_length_dict.ContainsKey(refsView.Item(i - 1).Item(1).ToString.ToLower) Then
                    If IsNumeric(refsView.Item(i - 1).Item(5)) AndAlso CInt(refsView.Item(i - 1).Item(5)) > 0 Then
                        refsView.Item(i - 1).Item(6) = CInt(result_length_dict(refsView.Item(i - 1).Item(1).ToString.ToLower) / CInt(refsView.Item(i - 1).Item(5)))
                    Else
                        refsView.Item(i - 1).Item(6) = ""
                    End If
                End If
            End If
        Next
        For i As Integer = 1 To refsView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = passed_genes(i - 1)
        Next

        PB_value = -1
        If form_config_combine.CheckBox1.Checked Then
            combine_file_horizontal(TextBox1.Text + "\combined_results\aligned", ".fasta", TextBox1.Text + "\combined_results.fasta", "?")
            If TargetOS = "macos" Then
                Thread.Sleep(100)
            End If
            combine_file_horizontal(TextBox1.Text + "\combined_trimed", ".fasta", TextBox1.Text + "\combined_trimed.fasta", "?")
        Else
            combine_file_horizontal(TextBox1.Text + "\combined_results\aligned", ".fasta", TextBox1.Text + "\combined_results.fasta", "-")
            If TargetOS = "macos" Then
                Thread.Sleep(100)
            End If
            combine_file_horizontal(TextBox1.Text + "\combined_trimed", ".fasta", TextBox1.Text + "\combined_trimed.fasta", "-")
        End If

        DataGridView1.RefreshEdit()
        MsgBox("Analysis completed! Please check 'combined_*' files and folders in output.", MsgBoxStyle.Information, "Information")
    End Sub

    Public Function FilterAndSaveFile(ByVal fileName As String, ByVal lineNumbers As List(Of Integer)) As Integer
        Dim result = ReadFasta(fileName)
        Dim sequences As List(Of String) = result.Item1
        Dim headers As List(Of String) = result.Item2
        If sequences.Count > 0 Then
            Dim filteredLines As New List(Of String)

            For Each lineNumber In lineNumbers
                filteredLines.Add(headers(lineNumber))
                filteredLines.Add(sequences(lineNumber))
            Next
            File.WriteAllLines(fileName, filteredLines)
        Else
            safe_delete(fileName)
        End If
        Return Math.Min(sequences.Count, lineNumbers.Count)
    End Function

    Public Sub replace_gap2missing(ByVal fileName As String)
        If File.Exists(fileName) Then
            Dim lines As List(Of String) = File.ReadAllLines(fileName).ToList()
            For i As Integer = 0 To lines.Count - 1
                If Not lines(i).StartsWith(">") Then
                    lines(i) = lines(i).Replace("-", "?")
                End If
            Next
            File.WriteAllLines(fileName, lines)
        End If
    End Sub

    Public Sub combine_file_horizontal(ByVal From_dir As String, ByVal exts As String, ByVal new_name As String, ByVal missingchar As String)
        If Not (From_dir Is Nothing) Then
            Dim startInfo As New ProcessStartInfo With {
                .FileName = currentDirectory + "analysis\merge_seq.exe",
                .WorkingDirectory = currentDirectory + "analysis\",
                .CreateNoWindow = True,
                .Arguments = "-input " + """" + From_dir + """"
            }
            startInfo.Arguments += " -output " + """" + new_name + """"
            startInfo.Arguments += " -exts " + exts
            startInfo.Arguments += " -missing " + missingchar
            Dim process As Process = Process.Start(startInfo)
            process.WaitForExit()
            process.Close()
        End If
        info_text = ""
        PB_value = 0
    End Sub
    Public Sub combine_file_vertical(ByVal From_dir As String, ByVal exts As String, ByVal new_name As String)
        If Not (From_dir Is Nothing) Then
            Dim sw As New StreamWriter(new_name)
            Dim mFileInfo As FileInfo
            Dim mDirInfo As New DirectoryInfo(From_dir)
            For Each mFileInfo In mDirInfo.GetFiles()
                If exts.ToUpper().Split(",").Contains(mFileInfo.Extension.ToUpper()) Then
                    Try
                        Dim sr As New StreamReader(mFileInfo.FullName)
                        sw.Write(sr.ReadToEnd)
                        sr.Close()
                    Catch ex As Exception
                        MsgBox(mFileInfo.FullName, MsgBoxStyle.Information, "Information")
                    End Try
                End If
            Next
            sw.Close()
        End If
        info_text = ""
        PB_value = 0
    End Sub

    Private Sub NumericUpDown10_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown10.ValueChanged
        current_thread = NumericUpDown10.Value
    End Sub


    Private Sub 合并比对ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 合并比对ToolStripMenuItem.Click
        MenuClicked = "combine_trim"
        form_config_combine.Show()
    End Sub

    Private Sub NumericUpDown10_TextChanged(sender As Object, e As EventArgs) Handles NumericUpDown10.TextChanged
        current_thread = NumericUpDown10.Value
    End Sub

    Private Sub 手动提取ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            'If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 Then
            '    Dim result As DialogResult = MessageBox.Show("Clear the output directory?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            '    If result = DialogResult.Yes Then
            '        DeleteDir(TextBox1.Text)
            '        Directory.CreateDirectory(TextBox1.Text)
            '    End If
            'End If

            Dim refs_count As Integer = 0
            Dim seqs_count As Integer = 0

            'DeleteDir(currentDirectory + "temp\seeds")
            'Directory.CreateDirectory(currentDirectory + "temp\seeds")

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    refs_count += 1
                    If refs_count = 1 Then
                        safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", currentDirectory + "temp\seed.fasta", True)
                    Else
                        MsgBox("In this mode, you can only choose one reference sequence as a seed file.", MsgBoxStyle.Information, "Information")
                        Exit Sub
                    End If
                End If
            Next
            Dim sw As New StreamWriter(currentDirectory + "temp\batch_file.txt")
            For i As Integer = 1 To seqsView.Count
                If DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    seqs_count += 1
                    sw.WriteLine("Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString)
                    sw.WriteLine("seed.fasta")
                    sw.WriteLine("Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".1.fq")
                    sw.WriteLine("Project" + DataGridView2.Rows(i - 1).Cells(1).FormattedValue.ToString + ".2.fq")
                End If
            Next
            sw.Close()
            If seqs_count >= 1 And refs_count >= 1 Then
                form_config_plasty.ComboBox1.Enabled = True
                form_config_plasty.TextBox3.Text = ""
                form_config_plasty.TextBox2.Text = ""
                cpg_assemble_mode = -1
                form_config_plasty.Show()
            Else
                MsgBox("Please select one reference as seed and at least one sequencing data!", MsgBoxStyle.Information, "Information")
            End If
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub ToolStripMenuItem3_Click_1(sender As Object, e As EventArgs) Handles ToolStripMenuItem3.Click
        If check_batch_folder() = False Then
            MsgBox("To assemble the plant mitochondrial genome, it is necessary to already possess the chloroplast genome first.", MsgBoxStyle.Information, "Information")
            Exit Sub
        End If
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        MenuClicked = "batch_plant_mito"
        form_config_basic.CheckBox3.Checked = True
        form_config_basic.GroupBox2.Enabled = True
        form_config_basic.GroupBox3.Enabled = False
        form_config_basic.GroupBox4.Enabled = False
        form_config_basic.NumericUpDown1.Value = 17
        form_config_basic.Show()

    End Sub
    Private Sub menu_batch_plant_mito()
        cpg_down_mode = 5
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub

    Private Sub ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 And DebugToolStripMenuItem.Checked = False Then
            Dim result As DialogResult = MessageBox.Show("The result folder is not empty. Are you sure to proceed?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.No Then
                Exit Sub
            End If
        End If
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        MenuClicked = "batch_plant_cp"
        form_config_basic.CheckBox3.Checked = True
        form_config_basic.GroupBox2.Enabled = True
        form_config_basic.GroupBox3.Enabled = False
        form_config_basic.GroupBox4.Enabled = False
        form_config_basic.NumericUpDown1.Value = 17
        form_config_basic.Show()
    End Sub
    Private Sub menu_batch_plant_cp()
        cpg_down_mode = 4
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub

    Private Sub 全选ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 全选ToolStripMenuItem1.Click
        For i As Integer = 1 To refsView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = True
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 清空ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 清空ToolStripMenuItem1.Click
        For i As Integer = 1 To refsView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = False
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 反选ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 反选ToolStripMenuItem.Click
        Dim sel_count As Integer = 0
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).Value = True Then
                DataGridView1.Rows(i - 1).Cells(0).Value = False
            Else
                DataGridView1.Rows(i - 1).Cells(0).Value = True
                sel_count += 1
            End If
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 失败的项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 失败的项ToolStripMenuItem.Click
        Dim sel_count As Integer = 0
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(6).Value <> "success" Then
                DataGridView1.Rows(i - 1).Cells(0).Value = True
                sel_count += 1
            Else
                DataGridView1.Rows(i - 1).Cells(0).Value = False
            End If
        Next
        MsgBox(sel_count.ToString + " were selected!", MsgBoxStyle.Information, "Information")
        DataGridView1.RefreshEdit()
    End Sub


    Public Sub do_trim(ByVal terminalonly As Boolean)
        Directory.CreateDirectory(TextBox1.Text + "\trimed\")
        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                 Interlocked.Add(count, 1)
                                                                 PB_value = count / refsView.Count * 100
                                                                 If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                     If File.Exists(TextBox1.Text + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                                                                         Try
                                                                             Dim SI_trimed As New ProcessStartInfo With {
                                                                                 .FileName = currentDirectory + "analysis\trimal.exe",
                                                                                 .WorkingDirectory = currentDirectory + "analysis\",
                                                                                 .CreateNoWindow = True,
                                                                                 .Arguments = "-in " + """" + TextBox1.Text + "\aligned\" + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """"
                                                                             }
                                                                             SI_trimed.Arguments += " -out " + """" + TextBox1.Text + "\trimed\" + refsView.Item(i - 1).Item(1).ToString + ".fasta" + """"
                                                                             SI_trimed.Arguments += " -automated1"
                                                                             If terminalonly Then
                                                                                 SI_trimed.Arguments += " -terminalonly"
                                                                             End If
                                                                             Dim process_trimed As Process = Process.Start(SI_trimed)
                                                                             process_trimed.WaitForExit()
                                                                             process_trimed.Close()
                                                                         Catch ex As Exception
                                                                             File.AppendAllText(TextBox1.Text + "\log.txt", "Could not do trim for " & DataGridView1.Rows(i - 1).Cells(2).ToString & Environment.NewLine)
                                                                         End Try

                                                                     End If
                                                                 End If
                                                             End Sub)
        PB_value = -1
        'MsgBox("Analysis completed!")
        Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        ' 根据用户的选择执行相应的操作
        If result = DialogResult.Yes Then
            Process.Start("explorer.exe", """" + out_dir.Replace("/", "\") + "\trimed" + """")
        End If
    End Sub

    Private Sub 重构ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 重构ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            MenuClicked = "consensus"
            form_config_consensus.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub do_consensus(ByVal options() As String)
        Dim source_folder As String = options(1)

        If My.Computer.FileSystem.DirectoryExists(Path.Combine(out_dir, source_folder)) Then
            Dim consensus_dir As String = Path.Combine(out_dir, "consensus")
            Directory.CreateDirectory(consensus_dir)
            Dim count As Integer = 0
            Dim parallelOptions As New ParallelOptions With {
                .MaxDegreeOfParallelism = current_thread
            }
            If TargetOS = "macos" Then
                parallelOptions.MaxDegreeOfParallelism = 1
            End If
            Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                     'For i As Integer = 1 To refsView.Count
                                                                     Interlocked.Add(count, 1)
                                                                     PB_value = count / refsView.Count * 100
                                                                     If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                         Dim in_path_fasta As String = Path.Combine(out_dir, source_folder, refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                         Dim in_path_fq As String = Path.Combine(out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                         Dim out_path As String = consensus_dir
                                                                         Dim out_path_file As String = Path.Combine(consensus_dir, refsView.Item(i - 1).Item(1).ToString + ".sam")
                                                                         If File.Exists(in_path_fasta) AndAlso File.Exists(in_path_fq) Then
                                                                             Try
                                                                                 Dim SI_minimap2 As New ProcessStartInfo With {
                                                                                     .FileName = Path.Combine(currentDirectory, "analysis", "minimap2.exe"),
                                                                                     .WorkingDirectory = Path.Combine(currentDirectory, "analysis"),
                                                                                     .CreateNoWindow = True,
                                                                                     .UseShellExecute = False,
                                                                                     .RedirectStandardOutput = True,
                                                                                     .Arguments = "-a --sam-hit-only -t 1 -x sr " + """" + in_path_fasta + """" + " " + """" + in_path_fq + """"
                                                                                 }

                                                                                 Using process_minimap2 As Process = Process.Start(SI_minimap2)
                                                                                     Using reader As StreamReader = process_minimap2.StandardOutput
                                                                                         Dim my_result As String = reader.ReadToEnd()
                                                                                         File.WriteAllText(out_path_file, my_result)
                                                                                     End Using
                                                                                     process_minimap2.WaitForExit()
                                                                                 End Using
                                                                                 If File.Exists(out_path_file) Then
                                                                                     Dim SI_consensus As New ProcessStartInfo With {
                                                                                         .FileName = Path.Combine(currentDirectory, "analysis", "build_consensus.exe"),
                                                                                         .WorkingDirectory = out_path,
                                                                                         .CreateNoWindow = True,
                                                                                         .Arguments = "-i " + """" + refsView.Item(i - 1).Item(1).ToString + ".sam" + """" + " -c " + options(0) + " -o " + """" + out_path + """" + " -s 0"
                                                                                     }
                                                                                     Dim process_consensus As Process = Process.Start(SI_consensus)
                                                                                     process_consensus.WaitForExit()
                                                                                     process_consensus.Close()
                                                                                 End If
                                                                             Catch ex As Exception
                                                                                 File.AppendAllText(TextBox1.Text + "\log.txt", "Could not make consensus for " & DataGridView1.Rows(i - 1).Cells(2).ToString & Environment.NewLine)
                                                                             End Try



                                                                         End If
                                                                     End If
                                                                 End Sub)
            PB_value = -1
            Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            ' 根据用户的选择执行相应的操作
            If result = DialogResult.Yes Then
                Process.Start("explorer.exe", """" + consensus_dir + """")
            End If
        End If
    End Sub

    Private Sub 重构序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 重构序列ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            MenuClicked = "batch_consensus"
            form_config_consensus.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If


    End Sub

    Public Sub batch_consensus(ByVal options() As String)
        Dim source_folder As String = options(1)
        For batch_i As Integer = 1 To seqsView.Count
            PB_value = batch_i / seqsView.Count * 100
            If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")

                Dim consensus_dir As String = Path.Combine(out_dir, "consensus")
                Directory.CreateDirectory(consensus_dir)
                Dim count As Integer = 0
                Dim parallelOptions As New ParallelOptions With {
                    .MaxDegreeOfParallelism = current_thread
                }
                If TargetOS = "macos" Then
                    parallelOptions.MaxDegreeOfParallelism = 1
                End If
                Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                         If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                             Dim in_path_fasta As String = Path.Combine(out_dir, source_folder, refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                             Dim in_path_fq As String = Path.Combine(out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                             Dim out_path As String = consensus_dir
                                                                             Dim out_path_file As String = Path.Combine(consensus_dir, refsView.Item(i - 1).Item(1).ToString + ".sam")

                                                                             If File.Exists(in_path_fasta) AndAlso File.Exists(in_path_fq) Then
                                                                                 Try
                                                                                     Dim SI_minimap2 As New ProcessStartInfo With {
                                                                                         .FileName = Path.Combine(currentDirectory, "analysis", "minimap2.exe"),
                                                                                         .WorkingDirectory = Path.Combine(currentDirectory, "analysis"),
                                                                                         .CreateNoWindow = True,
                                                                                         .UseShellExecute = False,
                                                                                         .RedirectStandardOutput = True,
                                                                                         .Arguments = "-a --sam-hit-only -t 1 -x sr " + """" + in_path_fasta + """" + " " + """" + in_path_fq + """"
                                                                                     }

                                                                                     Using process_minimap2 As Process = Process.Start(SI_minimap2)
                                                                                         Using reader As StreamReader = process_minimap2.StandardOutput
                                                                                             Dim my_result As String = reader.ReadToEnd()
                                                                                             File.WriteAllText(out_path_file, my_result)
                                                                                         End Using
                                                                                         process_minimap2.WaitForExit()
                                                                                     End Using
                                                                                     If TargetOS = "macos" Then
                                                                                         Thread.Sleep(100)
                                                                                     End If
                                                                                     If File.Exists(out_path_file) Then
                                                                                         Dim SI_consensus As New ProcessStartInfo With {
                                                                                             .FileName = Path.Combine(currentDirectory, "analysis", "build_consensus.exe"),
                                                                                             .WorkingDirectory = out_path,
                                                                                             .CreateNoWindow = True,
                                                                                             .Arguments = "-i " + """" + refsView.Item(i - 1).Item(1).ToString + ".sam" + """" + " -c " + options(0) + " -o " + """" + out_path + """" + " -s 0"
                                                                                         }
                                                                                         Dim process_consensus As Process = Process.Start(SI_consensus)
                                                                                         process_consensus.WaitForExit()
                                                                                         process_consensus.Close()
                                                                                         File.Delete(out_path_file)
                                                                                     End If
                                                                                 Catch ex As Exception
                                                                                     File.AppendAllText(TextBox1.Text + "\log.txt", "Could not do muscle for " & DataGridView1.Rows(i - 1).Cells(2).ToString & " in " & folder_name & Environment.NewLine)
                                                                                 End Try

                                                                             End If
                                                                         End If
                                                                     End Sub)

            End If
        Next

        If options(2) = "1" Then
            Dim supercontigs_dir As String = TextBox1.Text + "\supercontigs\"
            Directory.CreateDirectory(supercontigs_dir)
            Dim sw_namelist As New StreamWriter(Path.Combine(TextBox1.Text, "namelist.txt"), False, utf8WithoutBom)
            For batch_i As Integer = 1 To seqsView.Count
                PB_value = batch_i / seqsView.Count * 100
                If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                    Dim temp_out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                    folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                    Dim sw_res As New StreamWriter(Path.Combine(supercontigs_dir, folder_name + ".degenerated.fasta"), False, utf8WithoutBom)
                    sw_namelist.WriteLine(folder_name)
                    For i As Integer = 1 To refsView.Count
                        If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                            Dim consensus_path As String = temp_out_dir + "\consensus\" + refsView.Item(i - 1).Item(1).ToString + ".fasta"
                            Dim sr_line As String = ""
                            If File.Exists(consensus_path) Then
                                Dim sr_res As New StreamReader(consensus_path)
                                sr_line = sr_res.ReadLine
                                sr_line = sr_res.ReadLine
                                If sr_line IsNot Nothing Then
                                    If sr_line <> "" Then
                                        sw_res.WriteLine(">" + batch_i.ToString + " " + folder_name + "-" + refsView.Item(i - 1).Item(1).ToString)
                                        sw_res.WriteLine(sr_line)
                                    End If
                                End If
                                sr_res.Close()
                            End If
                        End If
                    Next
                    sw_res.Close()
                End If
            Next
            sw_namelist.Close()

            Dim combine_consensus_dir As String = TextBox1.Text + "\consensus\"
            Directory.CreateDirectory(combine_consensus_dir)
            For i As Integer = 1 To refsView.Count
                PB_value = i / refsView.Count * 100
                Dim sw_res As New StreamWriter(Path.Combine(combine_consensus_dir, refsView.Item(i - 1).Item(1).ToString + ".fasta"), False, utf8WithoutBom)
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    For batch_i As Integer = 1 To seqsView.Count
                        If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                            Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                            Dim temp_out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                            folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                            Dim consensus_path As String = temp_out_dir + "\consensus\" + refsView.Item(i - 1).Item(1).ToString + ".fasta"
                            Dim sr_line As String = ""
                            If File.Exists(consensus_path) Then
                                Dim sr_res As New StreamReader(consensus_path)
                                sr_line = sr_res.ReadLine
                                sr_line = sr_res.ReadLine
                                If sr_line IsNot Nothing Then
                                    If sr_line <> "" Then
                                        sw_res.WriteLine(">" + folder_name + "-" + refsView.Item(i - 1).Item(1).ToString)
                                        sw_res.WriteLine(sr_line)
                                    End If
                                End If
                                sr_res.Close()
                            End If
                        End If
                    Next
                End If
                sw_res.Close()
            Next

            Dim sw_tg As New StreamWriter(Path.Combine(TextBox1.Text, "TargetSequences.fasta"), False, utf8WithoutBom)
            For i As Integer = 1 To refsView.Count
                Dim ref_file As String = Path.Combine(currentDirectory, "temp", "org_seq", refsView.Item(i - 1).Item(1).ToString + ".fasta")
                Using sr As New StreamReader(ref_file)
                    While Not sr.EndOfStream
                        Dim line As String = sr.ReadLine()
                        If line.StartsWith(">") Then
                            line = line.Replace(refsView.Item(i - 1).Item(1).ToString, "").Replace("-", "_").Replace(" ", "_")
                            Do While line.EndsWith("_")
                                line = line.Substring(0, line.Length - 1)
                            Loop
                            line = line + "-" + refsView.Item(i - 1).Item(1).ToString
                        End If
                        sw_tg.WriteLine(line)
                    End While
                End Using
            Next
            sw_tg.Close()
        End If

        PB_value = -1
        MsgBox("Analysis completed!", MsgBoxStyle.Information, "Information")
    End Sub

    Private Sub PPDToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PPDToolStripMenuItem.Click
        If File.Exists(Path.Combine(TextBox1.Text, "TargetSequences.fasta")) And File.Exists(Path.Combine(TextBox1.Text, "namelist.txt")) Then

            If File.Exists(Path.Combine(TextBox1.Text, "outgroup.txt")) Then
                Dim result As DialogResult = MessageBox.Show("Need to choose a different outgroup? If you want to start the analysis right away, click 'No'!", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.Yes Then
                    Dim my_input As String = InputBox("Input ID of Outgroup", "ID of Outgroup", seqsView.Count)
                    Dim og_id As Integer
                    If Not Integer.TryParse(my_input, og_id) Then
                        Exit Sub
                    End If

                    Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(og_id - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(og_id - 1).Cells(3).Value.ToString))
                    folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                    Dim sw_og As New StreamWriter(Path.Combine(TextBox1.Text, "outgroup.txt"), False, utf8WithoutBom)
                    sw_og.WriteLine(folder_name)
                    sw_og.Close()
                End If
            Else
                Dim my_input As String = InputBox("Input ID of Outgroup", "ID of Outgroup", seqsView.Count)
                Dim og_id As Integer
                If Not Integer.TryParse(my_input, og_id) Then
                    Exit Sub
                End If
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(og_id - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(og_id - 1).Cells(3).Value.ToString))
                folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                Dim sw_og As New StreamWriter(Path.Combine(TextBox1.Text, "outgroup.txt"), False, utf8WithoutBom)
                sw_og.WriteLine(folder_name)
                sw_og.Close()
            End If

            If My.Computer.FileSystem.DirectoryExists(Path.Combine(TextBox1.Text, "PPD")) Then
                '    If Directory.GetFileSystemEntries(Path.Combine(TextBox1.Text, "PPD")).Length > 0 Then
                '        Dim result As DialogResult = MessageBox.Show("Clear the PPD output directory? If you are optimizing for previous results, please select 'NO'!", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                '        If result = DialogResult.Yes Then
                DeleteDir(Path.Combine(TextBox1.Text, "PPD"))
                '        End If
                '    End If
            End If
            RichTextBox1.Text = "Cite PPD:" + vbCrLf + "Zhou, W., Soghigian, J., Xiang, Q. 2022, A New Pipeline for Removing Paralogs in Target Enrichment Data. Systematic Biology, syab044. DOI: /10.1093/sysbio/syab044"
            Dim th1 As New Thread(AddressOf run_ppd)
            th1.Start(TextBox1.Text)
        Else
            MsgBox("You should Generate Consensus first." + vbCrLf + "您需要先进行一致性重构.", MsgBoxStyle.Information, "Information")
        End If

    End Sub
    Public Sub run_ppd(ByVal out_path As String)
        Dim SI_PPD As New ProcessStartInfo With {
            .FileName = Path.Combine(currentDirectory, "analysis", "PPD.exe"),
            .WorkingDirectory = Path.Combine(currentDirectory, "analysis"),
            .CreateNoWindow = False,
            .Arguments = "-ifa " + """" + Path.Combine(out_path, "supercontigs") + """"
        }
        SI_PPD.Arguments += " -ina " + """" + Path.Combine(out_path, "namelist.txt") + """"
        SI_PPD.Arguments += " -iref " + """" + Path.Combine(out_path, "TargetSequences.fasta") + """"
        SI_PPD.Arguments += " -io " + """" + Path.Combine(out_path, "outgroup.txt") + """"
        SI_PPD.Arguments += " -o " + """" + Path.Combine(out_path, "PPD") + """"
        SI_PPD.Arguments += " -th " + current_thread.ToString

        If align_app = "muscle" Then
            SI_PPD.Arguments += " -aln muscle"
        Else
            SI_PPD.Arguments += " -aln mafft"
        End If
        Dim process_PPD As Process = Process.Start(SI_PPD)
        process_PPD.WaitForExit()
        process_PPD.Close()
        Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        ' 根据用户的选择执行相应的操作
        If result = DialogResult.Yes Then
            Process.Start("explorer.exe", """" + Path.Combine(out_path, "PPD", "result", "supercontig", "s8_rm_paralogs", "Final_kept_genes") + """")
        End If
    End Sub


    Private Sub 下载ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 下载ToolStripMenuItem.Click
        'form_config_ags.Show()

        cpg_down_mode = 6
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub

    Private Sub 下载叶绿体基因组ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 下载叶绿体基因组ToolStripMenuItem.Click
        cpg_down_mode = 0
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = True
        form_config_cp.Show()
    End Sub

    Private Sub 下载植物线粒体ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 下载植物线粒体ToolStripMenuItem.Click
        cpg_down_mode = 2
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = True
        form_config_cp.Show()
    End Sub

    Private Sub 植物叶绿体基因组ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 植物叶绿体基因组ToolStripMenuItem.Click
        If check_data_count(1) = 0 Then
            MsgBox("Please select at least one sequencing data!", MsgBoxStyle.Information, "Information")
            Exit Sub
        End If
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        MenuClicked = "plant_cp"
        form_config_basic.CheckBox3.Checked = True
        form_config_basic.GroupBox2.Enabled = True
        form_config_basic.GroupBox3.Enabled = False
        form_config_basic.GroupBox4.Enabled = False
        form_config_basic.NumericUpDown1.Value = 17
        form_config_basic.Show()
    End Sub
    Private Sub menu_plant_cp()
        cpg_down_mode = 1
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub

    Private Sub 植物线粒体基因组ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 植物线粒体基因组ToolStripMenuItem.Click
        If check_data_count(1) = 0 Then
            MsgBox("Please select at least one sequencing data!", MsgBoxStyle.Information, "Information")
            Exit Sub
        End If
        Dim result As DialogResult = MessageBox.Show("To assemble the plant mitochondrial genome, it is necessary to already possess the chloroplast genome of the plant. Click 'Yes' to choose the chloroplast genome.", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        ' 根据用户的选择执行相应的操作
        If result = DialogResult.Yes Then
            Dim opendialog As New OpenFileDialog With {
                .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
                .FileName = "",
                .DefaultExt = ".fas",
                .CheckFileExists = True,
                .CheckPathExists = True
            }
            Dim resultdialog As DialogResult = opendialog.ShowDialog()
            If resultdialog = DialogResult.OK Then
                File.Copy(opendialog.FileName, currentDirectory + "temp\cpg.fasta", True)
                DataGridView1.EndEdit()
                DataGridView2.EndEdit()
                DataGridView1.Refresh()
                DataGridView2.Refresh()
                MenuClicked = "plant_mito"
                form_config_basic.CheckBox3.Checked = True
                form_config_basic.GroupBox2.Enabled = True
                form_config_basic.GroupBox3.Enabled = False
                form_config_basic.GroupBox4.Enabled = False
                form_config_basic.NumericUpDown1.Value = 17
                form_config_basic.Show()
            End If
        End If
    End Sub
    Private Sub menu_plant_mito()
        cpg_down_mode = 3
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub

    Private Sub 哺乳动物线粒体基因组ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 哺乳动物线粒体基因组ToolStripMenuItem.Click
        cpg_down_mode = 7
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = True
        form_config_cp.Show()
    End Sub

    Private Sub 哺乳动物线粒体基因组ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 哺乳动物线粒体基因组ToolStripMenuItem1.Click
        If check_data_count(1) = 0 Then
            MsgBox("Please select at least one sequencing data!", MsgBoxStyle.Information, "Information")
            Exit Sub
        End If
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        MenuClicked = "animal_mito"
        form_config_basic.CheckBox3.Checked = True
        form_config_basic.GroupBox2.Enabled = True
        form_config_basic.GroupBox3.Enabled = False
        form_config_basic.GroupBox4.Enabled = False
        form_config_basic.NumericUpDown1.Value = 17
        form_config_basic.Show()
    End Sub
    Private Sub menu_animal_mito()
        cpg_down_mode = 8
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub
    Private Sub 哺乳动物线粒体基因组ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles 哺乳动物线粒体基因组ToolStripMenuItem2.Click
        If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 And DebugToolStripMenuItem.Checked = False Then
            Dim result As DialogResult = MessageBox.Show("The result folder is not empty. Are you sure to proceed?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.No Then
                Exit Sub
            End If
        End If
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        MenuClicked = "batch_animal_mito"
        form_config_basic.CheckBox3.Checked = True
        form_config_basic.GroupBox2.Enabled = True
        form_config_basic.GroupBox3.Enabled = False
        form_config_basic.GroupBox4.Enabled = False
        form_config_basic.NumericUpDown1.Value = 17
        form_config_basic.Show()


    End Sub

    Private Sub menu_batch_animal_mito()
        cpg_down_mode = 9
        out_dir = TextBox1.Text.Replace("\", "/")
        form_config_cp.CheckBox1.Visible = False
        form_config_cp.Show()
    End Sub



    Private Sub RichTextBox1_DoubleClick(sender As Object, e As EventArgs) Handles RichTextBox1.DoubleClick
        If File.Exists(TextBox1.Text + "\log.txt") Then
            Using sr As New StreamReader(TextBox1.Text + "\log.txt")
                RichTextBox1.Text = sr.ReadToEnd
            End Using
        End If
    End Sub


    Private Sub 多拷贝检测ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 多拷贝检测ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            out_dir = TextBox1.Text
            timer_id = 4
            PB_value = 0
            Dim th1 As New Thread(AddressOf check_paralogs)
            th1.Start()
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub check_paralogs()
        If My.Computer.FileSystem.DirectoryExists(Path.Combine(out_dir, "results")) Then
            Dim paralogs_dir As String = Path.Combine(out_dir, "multicopy")
            DeleteDir(paralogs_dir)
            Directory.CreateDirectory(paralogs_dir)
            Dim count As Integer = 0
            Dim parallelOptions As New ParallelOptions With {
                .MaxDegreeOfParallelism = current_thread
            }
            If TargetOS = "macos" Then
                parallelOptions.MaxDegreeOfParallelism = 1
            End If
            Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                     'For i As Integer = 1 To refsView.Count
                                                                     Interlocked.Add(count, 1)
                                                                     PB_value = count / refsView.Count * 100
                                                                     If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                         Dim in_path_fasta As String = Path.Combine(out_dir, "results", refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                         Dim in_path_fq As String = Path.Combine(out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                         Dim out_path As String = paralogs_dir
                                                                         Dim out_path_file As String = Path.Combine(paralogs_dir, refsView.Item(i - 1).Item(1).ToString + "_ref.fasta")
                                                                         Try
                                                                             If File.Exists(in_path_fasta) AndAlso File.Exists(in_path_fq) Then
                                                                                 safe_copy(in_path_fasta, out_path_file, True)
                                                                                 Dim SI_build_barcode As New ProcessStartInfo With {
                                                                                     .FileName = Path.Combine(currentDirectory, "analysis", "build_barcode.exe"),
                                                                                     .WorkingDirectory = Path.Combine(currentDirectory, "temp"),
                                                                                     .CreateNoWindow = True,
                                                                                     .Arguments = "-i " + """" + in_path_fq + """" + " -o " + """" + out_path + """" + " -p " + current_thread.ToString + " -c 0.25,0.3,0.35,0.4,0.45,0.5,0.55,0.6,0.65,0.7,0.75 -l 4 -m 1"
                                                                                 }
                                                                                 Dim process_build_barcode As Process = Process.Start(SI_build_barcode)
                                                                                 process_build_barcode.WaitForExit()
                                                                                 process_build_barcode.Close()
                                                                             End If
                                                                         Catch ex As Exception
                                                                             File.AppendAllText(TextBox1.Text + "\log.txt", "Could not check multicopy for " & DataGridView1.Rows(i - 1).Cells(2).ToString & Environment.NewLine)
                                                                         End Try

                                                                     End If
                                                                 End Sub)
            PB_value = -1
            Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            ' 根据用户的选择执行相应的操作
            If result = DialogResult.Yes Then
                Process.Start("explorer.exe", """" + paralogs_dir + """")
            End If
        End If
    End Sub

    Private Sub 旁系同源检测ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 旁系同源检测ToolStripMenuItem.Click

        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        timer_id = 4
        PB_value = 0
        Dim th1 As New Thread(AddressOf batch_multicopy)
        th1.Start()
    End Sub

    Public Sub batch_multicopy()
        For batch_i As Integer = 1 To seqsView.Count
            PB_value = batch_i / seqsView.Count * 100
            If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")

                Dim paralogs_dir As String = Path.Combine(out_dir, "multicopy")
                safe_delete(paralogs_dir)
                Directory.CreateDirectory(paralogs_dir)
                Dim count As Integer = 0
                Dim parallelOptions As New ParallelOptions With {
                    .MaxDegreeOfParallelism = current_thread
                }
                If TargetOS = "macos" Then
                    parallelOptions.MaxDegreeOfParallelism = 1
                End If
                Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                         If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then

                                                                             Dim in_path_fasta As String = Path.Combine(out_dir, "results", refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                             Dim in_path_fq As String = Path.Combine(out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                             Dim out_path As String = paralogs_dir
                                                                             Dim out_path_file As String = Path.Combine(paralogs_dir, refsView.Item(i - 1).Item(1).ToString + "_ref.fasta")

                                                                             If File.Exists(in_path_fasta) AndAlso File.Exists(in_path_fq) Then
                                                                                 Try
                                                                                     safe_copy(in_path_fasta, out_path_file, True)
                                                                                     Dim SI_build_barcode As New ProcessStartInfo With {
                                                                                         .FileName = Path.Combine(currentDirectory, "analysis", "build_barcode.exe"),
                                                                                         .WorkingDirectory = Path.Combine(currentDirectory, "temp"),
                                                                                         .CreateNoWindow = True,
                                                                                         .Arguments = "-i " + """" + in_path_fq + """" + " -o " + """" + out_path + """" + " -p " + current_thread.ToString + " -c 0.25,0.3,0.35,0.4,0.45,0.5,0.55,0.6,0.65,0.7,0.75 -l 4 -m 1"
                                                                                     }
                                                                                     Dim process_build_barcode As Process = Process.Start(SI_build_barcode)
                                                                                     process_build_barcode.WaitForExit()
                                                                                     process_build_barcode.Close()
                                                                                 Catch ex As Exception
                                                                                     File.AppendAllText(TextBox1.Text + "\log.txt", "Could not check multicopy for " & DataGridView1.Rows(i - 1).Cells(2).ToString & " in " & folder_name & Environment.NewLine)
                                                                                 End Try

                                                                             End If
                                                                         End If
                                                                     End Sub)
                RichTextBox1.AppendText("Multicopy in " + folder_name + ":" + vbCrLf)
                For i As Integer = 1 To refsView.Count
                    If File.Exists(Path.Combine(paralogs_dir, refsView.Item(i - 1).Item(1).ToString + ".fasta")) Then
                        RichTextBox1.AppendText(refsView.Item(i - 1).Item(1).ToString + vbCrLf)
                    End If
                Next

            End If
        Next
        PB_value = -1
        MsgBox("Analysis completed! Please check multicopy folder in output.", MsgBoxStyle.Information, "Information")
    End Sub

    Private Sub 全选ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全选ToolStripMenuItem.Click
        For i As Integer = 1 To seqsView.Count
            DataGridView2.Rows(i - 1).Cells(0).Value = True
        Next
        DataGridView2.RefreshEdit()
    End Sub

    Private Sub 反选ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 反选ToolStripMenuItem1.Click
        Dim sel_count As Integer = 0
        For i As Integer = 1 To seqsView.Count
            If DataGridView2.Rows(i - 1).Cells(0).Value = True Then
                DataGridView2.Rows(i - 1).Cells(0).Value = False
            Else
                DataGridView2.Rows(i - 1).Cells(0).Value = True
                sel_count += 1
            End If
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 全不选ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全不选ToolStripMenuItem.Click
        For i As Integer = 1 To seqsView.Count
            DataGridView2.Rows(i - 1).Cells(0).Value = False
        Next
        DataGridView2.RefreshEdit()
    End Sub

    Private Sub 统计结果ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 统计结果ToolStripMenuItem.Click
        'Dim SI_build_summary As New ProcessStartInfo()
        'SI_build_summary.FileName = Path.Combine(currentDirectory, "analysis", "build_summary.exe")
        'SI_build_summary.WorkingDirectory = Path.Combine(currentDirectory, "temp")
        'SI_build_summary.CreateNoWindow = True
        'SI_build_summary.Arguments = "-b " + """" + TextBox1.Text + """"
        'Dim process_build_summary As Process = Process.Start(SI_build_summary)
        'process_build_summary.WaitForExit()
        'process_build_summary.Close()
        Dim th1 As New Thread(AddressOf build_summary)
        th1.Start()
    End Sub
    Public Sub build_summary()
        Dim count As Integer = 0
        PB_value = 0
        Dim ref_len_dict As New Dictionary(Of String, Integer)
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                '统计参考序列中值
                Dim ref_file As String = Path.Combine(currentDirectory + "temp", "org_seq", refsView.Item(i - 1).Item(1).ToString + ".fasta")
                If File.Exists(ref_file) Then
                    ref_len_dict.Add(refsView.Item(i - 1).Item(1).ToString, CInt(CalculateMedianSequenceLength(ref_file)))
                End If
            End If
        Next
        Try
            Using writer As StreamWriter = New StreamWriter(TextBox1.Text + "\summary.csv", False, System.Text.Encoding.UTF8)
                Dim header As New List(Of String) From {
                "Sample Name", "Gene Name", "Reference Median Length", "Reads Count", "Result Availability", "Length of Result Sequence", "Length of Trimed Sequence", "Multicopy Presence"
             }
                writer.WriteLine(String.Join(",", header))

                For batch_i As Integer = 1 To seqsView.Count
                    Interlocked.Add(count, 1)
                    PB_value = count / seqsView.Count * 100
                    If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                        Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                        Dim my_out_dir As String = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                        For i As Integer = 1 To refsView.Count
                            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                Dim result_list As New List(Of String)
                                '0原始数据1参考序列2参考序列中值3reads数量4是否有结果5结果长度6是否存在多拷贝
                                result_list.Add(folder_name)
                                result_list.Add(refsView.Item(i - 1).Item(1).ToString)
                                result_list.Add(ref_len_dict(refsView.Item(i - 1).Item(1).ToString))
                                '统计reads
                                If File.Exists(Path.Combine(my_out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")) Then
                                    result_list.Add(CInt(CountLinesInFile(Path.Combine(my_out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")) / 4).ToString)
                                Else
                                    result_list.Add("0")
                                End If
                                '统计序列长度（中值）
                                If File.Exists(Path.Combine(my_out_dir, "results", refsView.Item(i - 1).Item(1).ToString + ".fasta")) Then
                                    Dim tmp_len As Integer = CalculateMedianSequenceLength(Path.Combine(my_out_dir, "results", refsView.Item(i - 1).Item(1).ToString + ".fasta"))
                                    If tmp_len = 0 Then
                                        result_list.Add("0")
                                        File.Delete(Path.Combine(my_out_dir, "results", refsView.Item(i - 1).Item(1).ToString + ".fasta"))
                                    Else
                                        result_list.Add("1")
                                    End If
                                    result_list.Add(tmp_len.ToString)
                                Else
                                    result_list.Add("0")
                                    result_list.Add("0")
                                End If
                                '统计序列长度（中值）
                                If File.Exists(Path.Combine(my_out_dir, "blast", refsView.Item(i - 1).Item(1).ToString + ".fasta")) Then
                                    Dim tmp_len As Integer = CalculateMedianSequenceLength(Path.Combine(my_out_dir, "blast", refsView.Item(i - 1).Item(1).ToString + ".fasta"))
                                    result_list.Add(tmp_len.ToString)
                                Else
                                    result_list.Add("0")
                                End If
                                If Directory.Exists(Path.Combine(my_out_dir, "multicopy")) Then
                                    If File.Exists(Path.Combine(my_out_dir, "multicopy", refsView.Item(i - 1).Item(1).ToString + ".fasta")) Then
                                        result_list.Add("1")
                                    Else
                                        result_list.Add("0")
                                    End If
                                Else
                                    result_list.Add("N/A")
                                End If
                                writer.WriteLine(String.Join(",", result_list))
                            End If

                        Next
                    End If
                Next
            End Using
            PB_value = 0
            MsgBox("Please check the 'summary.csv' in the output folder.", MsgBoxStyle.Information, "Information")
        Catch ex As Exception
            MsgBox(ex.ToString, MsgBoxStyle.Information, "Information")
            PB_value = 0

        End Try


    End Sub
    ' 计算FASTA文件中序列长度的中值
    Function CalculateMedianSequenceLength(filePath As String) As Double
        Dim sequenceLengths As New List(Of Integer)
        Dim currentSequenceLength As Integer = 0
        Dim inSequence As Boolean = False

        Using reader As StreamReader = New StreamReader(filePath)
            Dim line As String

            While (reader.Peek() >= 0)
                line = reader.ReadLine()

                If line.StartsWith(">") Then
                    If inSequence Then
                        sequenceLengths.Add(currentSequenceLength)
                        currentSequenceLength = 0
                    End If
                    inSequence = True
                ElseIf inSequence Then
                    currentSequenceLength += line.Length
                End If
            End While

            ' 添加最后一个序列的长度
            If inSequence Then
                sequenceLengths.Add(currentSequenceLength)
            End If
        End Using

        ' 计算中值
        If sequenceLengths.Count = 0 Then
            Return 0
        Else
            sequenceLengths.Sort()
            Dim middleIndex As Integer = sequenceLengths.Count \ 2

            If sequenceLengths.Count Mod 2 = 0 Then
                ' 偶数个元素
                Return (sequenceLengths(middleIndex - 1) + sequenceLengths(middleIndex)) / 2.0
            Else
                ' 奇数个元素
                Return sequenceLengths(middleIndex)
            End If
        End If
    End Function
    Function CountLinesInFile(filePath As String) As Integer
        Dim lineCount As Integer = 0
        Using reader As StreamReader = New StreamReader(filePath)
            While reader.ReadLine() IsNot Nothing
                lineCount += 1
            End While
        End Using
        Return lineCount
    End Function



    Private Sub 用迭代覆盖ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 用迭代覆盖ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            If refsView.Count > 0 Then
                DataGridView1.EndEdit()
                DataGridView2.EndEdit()
                DataGridView1.Refresh()
                DataGridView2.Refresh()
                Dim th1 As New Thread(AddressOf cover_with_iteration)
                th1.Start()
            End If
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If

    End Sub
    Private Sub cover_with_iteration()
        For i As Integer = 1 To refsView.Count
            PB_value = i / refsView.Count * 100
            Dim gene_name As String = refsView.Item(i - 1).Item(1).ToString
            If File.Exists(TextBox1.Text + "\iteration\contigs_all\" + gene_name + ".fasta") Then
                safe_copy(TextBox1.Text + "\iteration\contigs_all\" + gene_name + ".fasta", TextBox1.Text + "\contigs_all\" + gene_name + ".fasta", True)
            End If
            If File.Exists(TextBox1.Text + "\iteration\results\" + gene_name + ".fasta") Then
                safe_copy(TextBox1.Text + "\iteration\results\" + gene_name + ".fasta", TextBox1.Text + "\results\" + gene_name + ".fasta", True)
            End If
            If File.Exists(TextBox1.Text + "\iteration\filtered\" + gene_name + ".fq") Then
                safe_copy(TextBox1.Text + "\iteration\filtered\" + gene_name + ".fq", TextBox1.Text + "\filtered\" + gene_name + ".fq", True)
            End If
            If File.Exists(TextBox1.Text + "\iteration\large_files\" + gene_name + ".fq") Then
                safe_copy(TextBox1.Text + "\iteration\large_files\" + gene_name + ".fq", TextBox1.Text + "\large_files\" + gene_name + ".fq", True)
            End If
        Next
        DeleteDir(TextBox1.Text + "\iteration")
        PB_value = 0
    End Sub

    Private Sub ToolStripMenuItem4_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem4.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            MenuClicked = "batch_re_assemble"
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = True
            form_config_basic.GroupBox2.Enabled = False
            form_config_basic.GroupBox3.Enabled = False
            form_config_basic.GroupBox4.Enabled = True
            form_config_basic.NumericUpDown1.Value = If(form_config_calculate.kf = 0, 31, form_config_calculate.kf)
            form_config_basic.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub menu_batch_re_assemble()
        Dim refs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)

        timer_id = 4
        PB_value = 0
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        If refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf batch_re_assemble)
            th1.Start()
        Else
            MsgBox("Please select at least one reference And one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub batch_re_assemble()
        batch_task(False, False, True, False)
        MsgBox("Analysis completed! Please check results folder.", MsgBoxStyle.Information, "Information")
    End Sub

    Private Sub 拆分fq文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 拆分fq文件ToolStripMenuItem.Click
        timer_id = 4
        Dim th1 As New Thread(AddressOf do_SplitFastqFile)
        th1.Start()
    End Sub

    Public Sub do_SplitFastqFile()
        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If

        For i As Integer = 1 To refsView.Count
            Interlocked.Add(count, 1)
            PB_value = count / refsView.Count * 100
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then

                Dim inputFilePath As String = Path.Combine(TextBox1.Text, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                If File.Exists(inputFilePath) Then
                    SplitFastqFile(inputFilePath)

                End If
            End If
        Next

        For batch_i As Integer = 1 To seqsView.Count
            PB_value = batch_i / seqsView.Count * 100
            If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                Dim folder_name_tmp As String = batch_i.ToString + "_" + folder_name
                Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                         If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                             Dim inputFilePath As String = Path.Combine(TextBox1.Text, folder_name_tmp, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                             If File.Exists(inputFilePath) Then
                                                                                 SplitFastqFile(inputFilePath)
                                                                             End If
                                                                         End If
                                                                     End Sub)
                safe_delete(Path.Combine(TextBox1.Text, folder_name + ".1.fq"))
                safe_delete(Path.Combine(TextBox1.Text, folder_name + ".2.fq"))
                For i As Integer = 1 To refsView.Count
                    If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                        Dim inputfq As String = Path.Combine(TextBox1.Text, folder_name_tmp, "filtered", refsView.Item(i - 1).Item(1).ToString)
                        If File.Exists(inputfq + ".1.fq") Then
                            CombineFiles(Path.Combine(TextBox1.Text, folder_name + ".1.fq"), inputfq + ".1.fq")
                        End If
                        If File.Exists(inputfq + ".2.fq") Then
                            CombineFiles(Path.Combine(TextBox1.Text, folder_name + ".2.fq"), inputfq + ".2.fq")
                        End If
                    End If
                Next
            End If
        Next
        PB_value = -1


        MsgBox("Analysis completed! Please check filtered folder in output.", MsgBoxStyle.Information, "Information")
    End Sub
    Public Sub SplitFastqFile(inputFilePath As String)
        Dim directoryPath As String = Path.GetDirectoryName(inputFilePath)
        Dim baseFileName As String = Path.GetFileNameWithoutExtension(inputFilePath)
        Dim outputFilePath1 As String = Path.Combine(directoryPath, baseFileName & ".1.fq")
        Dim outputFilePath2 As String = Path.Combine(directoryPath, baseFileName & ".2.fq")

        Using reader As StreamReader = New StreamReader(inputFilePath)
            Using writer1 As StreamWriter = New StreamWriter(outputFilePath1)
                Using writer2 As StreamWriter = New StreamWriter(outputFilePath2)

                    Dim line As String
                    Dim readNumber As Integer = 1

                    While (Not reader.EndOfStream)
                        line = reader.ReadLine()
                        If readNumber Mod 2 <> 0 Then
                            WriteRead(writer1, line, reader)
                        Else
                            WriteRead(writer2, line, reader)
                        End If

                        readNumber += 1
                    End While

                End Using
            End Using
        End Using
    End Sub
    Private Sub WriteRead(writer As StreamWriter, firstLine As String, reader As StreamReader)
        writer.WriteLine(firstLine)
        For i As Integer = 1 To 3
            writer.WriteLine(reader.ReadLine())
        Next
    End Sub

    Public Sub do_orthofinder(ByVal genome_dir As String)
        Dim SI_orthofinder As New ProcessStartInfo With {
            .FileName = Path.Combine(currentDirectory, "analysis", "orthofinder.exe"),
            .WorkingDirectory = Path.Combine(currentDirectory, "analysis"),
            .CreateNoWindow = False,
            .Arguments = " -d -og -f " + """" + genome_dir + """"
        }
        Dim process_orthofinder As Process = Process.Start(SI_orthofinder)
        process_orthofinder.WaitForExit()
        process_orthofinder.Close()
        Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you Like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        ' 根据用户的选择执行相应的操作
        If result = DialogResult.Yes Then
            Process.Start("explorer.exe", """" + genome_dir + """")
        End If
    End Sub

    Private Sub 获取单拷贝基因ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 获取单拷贝基因ToolStripMenuItem.Click
        Dim refs_count As Integer = 0
        Dim genome_dir As String = (TextBox1.Text + "\OrthoFinder")
        DeleteDir(genome_dir)
        Directory.CreateDirectory(genome_dir)
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", genome_dir + "\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        RichTextBox1.Text = "Cite OrthoFinder 2.5.5: " + vbCrLf + "Emms, D.M. and Kelly, S. (2019) OrthoFinder: phylogenetic orthology inference for comparative genomics. Genome Biology 20:238" + vbCrLf

        If refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf do_orthofinder)
            th1.Start(genome_dir)
        Else
            MsgBox("Please select at least one genome data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub ToolStripMenuItem5_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem5.Click
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = True
            form_config_basic.GroupBox2.Enabled = True
            form_config_basic.GroupBox3.Enabled = True
            form_config_basic.GroupBox4.Enabled = True
            MenuClicked = "muti_iteration"
            form_config_basic.Show()
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub menu_muti_iteration()
        If TextBox1.Text <> "" Then
            Dim my_input As String = InputBox("Please enter the number of iterations:", "Iterations", 1)
            Dim iterations_times As Integer
            If Not Integer.TryParse(my_input, iterations_times) Then
                Exit Sub
            End If


            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                End If
            Next

            timer_id = 4
            PB_value = 0
            Dim th1 As New Threading.Thread(AddressOf do_iteration)
            th1.Start(CInt(iterations_times))
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub 序列切片ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 序列切片ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
            .FileName = "",
            .Multiselect = False,
            .DefaultExt = ".fas",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            current_file = opendialog.FileName
            mydata_Dataset.Tables("Refs Table").Clear()
            DataGridView1.DataSource = Nothing
            data_loaded = False
            current_file = opendialog.FileName
            refs_type = "fasta"
            DeleteDir(root_path + "temp\org_seq")
            Dim SI_split_genes As New ProcessStartInfo With {
                .FileName = currentDirectory + "analysis\split_genes.exe",
                .WorkingDirectory = currentDirectory + "temp\",
                .CreateNoWindow = False,
                .Arguments = "-input " + """" + current_file + """"
            }
            Dim my_input As String = InputBox("Enter the length of the input slice and the overlap length, separated by ',':", "Input", "300,150")
            SI_split_genes.Arguments += " -min_seq_length " + my_input.Split(",")(1) + " -max_seq_length " + my_input.Split(",")(0)
            SI_split_genes.Arguments += " -intron_only False"
            SI_split_genes.Arguments += " -out_dir " + """" + root_path + "temp" + """"
            Dim process_split_genes As Process = Process.Start(SI_split_genes)
            process_split_genes.WaitForExit()
            process_split_genes.Close()
            refresh_file()
            timer_id = 2
        End If
    End Sub

    Private Sub EnglishToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EnglishToolStripMenuItem.Click
        If language = "EN" Then
            to_ch()
        Else
            to_en()
        End If
        settings("language") = language
    End Sub

    Private Sub 按物种合并ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 按物种合并ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
            .FileName = "",
            .DefaultExt = ".fasta",
            .CheckFileExists = False,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                End If
            Next
            Dim result As DialogResult = MessageBox.Show("Use '?' for 'Yes' and '-' for 'No' to mark missing samples.", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            ' 根据用户的选择执行相应的操作
            If result = DialogResult.Yes Then
                combine_file_horizontal(ref_dir, ".fasta", opendialog.FileName, "?")
            Else
                combine_file_horizontal(ref_dir, ".fasta", opendialog.FileName, "-")
            End If
            MsgBox("Analysis complete!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub 合并文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 合并文件ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa",
            .FileName = "",
            .DefaultExt = ".fasta",
            .CheckFileExists = False,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                End If
            Next
            combine_file_vertical(ref_dir, ".fasta", opendialog.FileName)
            MsgBox("Analysis complete!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub 比对ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 比对ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            Dim refs_count As Integer = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = TextBox1.Text
            DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    refs_count += 1
                    safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                End If
            Next
            If refs_count >= 1 Then
                If My.Computer.FileSystem.DirectoryExists(TextBox1.Text + "\results") Then
                    If Directory.GetFileSystemEntries(TextBox1.Text + "\results").Length > 0 Then
                        For i As Integer = 1 To refsView.Count
                            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" And File.Exists(TextBox1.Text + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                                CombineFiles(ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", TextBox1.Text + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta")
                            End If
                        Next
                    End If
                End If
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf do_align)
                th1.Start()
            Else
                MsgBox("Please select at least one reference!", MsgBoxStyle.Information, "Information")
            End If
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub 切齐ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 切齐ToolStripMenuItem.Click
        If TextBox1.Text <> "" Then
            Dim refs_count As Integer = 0
            ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
            out_dir = TextBox1.Text
            DeleteDir(ref_dir)
            Directory.CreateDirectory(ref_dir)

            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    refs_count += 1
                End If
            Next
            If refs_count >= 1 Then
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf do_trim)
                Dim result As DialogResult = MessageBox.Show("Trim the entire alignment? If you only want to trim terminal, please select 'No'.?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                ' 根据用户的选择执行相应的操作
                If result = DialogResult.Yes Then
                    th1.Start(False)
                Else
                    th1.Start(True)
                End If

            Else
                MsgBox("Please select at least one reference!", MsgBoxStyle.Information, "Information")
            End If
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub 对齐参考ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 对齐参考ToolStripMenuItem.Click

        If TextBox1.Text <> "" Then
            MenuClicked = "trim_with_ref"
            form_config_trim.Show()
        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub menu_align_blast()
        Dim refs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        out_dir = TextBox1.Text
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)

        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        If refs_count >= 1 Then
            Dim count As Integer = 0
            Dim parallelOptions As New ParallelOptions With {
                .MaxDegreeOfParallelism = current_thread
            }
            If TargetOS = "macos" Then
                parallelOptions.MaxDegreeOfParallelism = 1
            End If
            If Not File.Exists(Path.Combine(TextBox1.Text, "blast_db")) Then
                Directory.CreateDirectory(Path.Combine(TextBox1.Text, "blast_db"))
            End If
            DeleteDir(Path.Combine(TextBox1.Text, "blast"))
            Directory.CreateDirectory(Path.Combine(TextBox1.Text, "blast"))
            Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                     Interlocked.Add(count, 1)
                                                                     PB_value = count / refsView.Count * 100
                                                                     If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" And File.Exists(TextBox1.Text + "\results\" + refsView.Item(i - 1).Item(1).ToString + ".fasta") Then
                                                                         build_blast_db(refsView.Item(i - 1).Item(1).ToString)
                                                                         do_trim_blast(refsView.Item(i - 1).Item(1).ToString, Path.Combine(TextBox1.Text, "results"), Path.Combine(TextBox1.Text, "blast"))
                                                                     End If
                                                                 End Sub)
            PB_value = -1
            Dim filePath As String = TextBox1.Text + "\result_dict.txt"
            If File.Exists(filePath) Then
                ref_assemble_result(filePath)
            End If
            Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            ' 根据用户的选择执行相应的操作
            If result = DialogResult.Yes Then
                Process.Start("explorer.exe", """" + TextBox1.Text + "\blast" + """")
            End If
        Else
            MsgBox("Please select at least one reference!", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub 对齐参考ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 对齐参考ToolStripMenuItem1.Click
        MenuClicked = "batch_trim_with_ref"
        form_config_trim.Show()
    End Sub
    Public Sub menu_batch_align_blast()
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        If Not File.Exists(Path.Combine(TextBox1.Text, "blast_db")) Then
            Directory.CreateDirectory(Path.Combine(TextBox1.Text, "blast_db"))
        End If
        Dim source_dir As String = "results"
        Select Case form_config_trim.ComboBox1.SelectedIndex
            Case 0
                source_dir = "results"
            Case 1
                source_dir = "consensus"
        End Select
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Dim ticked_samples = Enumerable.Range(1, seqsView.Count).Where(Function(i) DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True").ToList
        Dim success_count As Integer = 0
        Dim total_count As Integer = (ticked_samples.Count + 1) * refsView.Count

        Parallel.For(0, refsView.Count, parallelOptions, Sub(i)
                                                             Interlocked.Add(success_count, 1)
                                                             PB_value = success_count / total_count * 100
                                                             If DataGridView1.Rows(i).Cells(0).FormattedValue.ToString = "True" Then
                                                                 build_blast_db(refsView.Item(i).Item(1).ToString)
                                                             End If
                                                         End Sub)

        Parallel.ForEach(ticked_samples, parallelOptions, Sub(batch_i)
                                                              Try
                                                                  Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                                                                  folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                                                                  Dim temp_out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                                                                  DeleteDir(Path.Combine(temp_out_dir, "blast"))
                                                                  Directory.CreateDirectory(Path.Combine(temp_out_dir, "blast"))
                                                                  For i As Integer = 1 To refsView.Count
                                                                      Interlocked.Add(success_count, 1)
                                                                      PB_value = success_count / total_count * 100
                                                                      If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                          do_trim_blast(refsView.Item(i - 1).Item(1).ToString, Path.Combine(temp_out_dir, source_dir), Path.Combine(temp_out_dir, "blast"))
                                                                      End If
                                                                  Next
                                                              Catch ex As Exception
                                                              End Try
                                                          End Sub)
        PB_value = -1
        MsgBox("Analysis Complete! All results has been trimed.", MsgBoxStyle.Information, "Information")
    End Sub


    Private Sub 参考数量ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 参考数量ToolStripMenuItem.Click
        Dim sel_count As Integer = 0
        Dim my_input As String = InputBox("Select count of reference below:", "Count", "1")
        If IsNumeric(my_input) Then
            For i As Integer = 1 To refsView.Count
                If IsNumeric(DataGridView1.Rows(i - 1).Cells(3).Value) Then
                    If CSng(DataGridView1.Rows(i - 1).Cells(3).Value) <= CSng(my_input) Then
                        'sel_count += 1
                        DataGridView1.Rows(i - 1).Cells(0).Value = True
                    End If
                End If
            Next
        End If


        'MsgBox(sel_count.ToString + " were selected!", MsgBoxStyle.Information, "Information")
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 构建系统发育树ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 构建系统发育树ToolStripMenuItem.Click
        MenuClicked = "build_tree"
        form_config_tree.Label2.Visible = False
        form_config_tree.TextBox2.Visible = False
        form_config_tree.DataGridView1.Visible = True
        form_config_tree.GroupBox2.Enabled = True
        form_config_tree.CheckBox2.Visible = True
        form_config_tree.Show()
    End Sub
    Private Sub Tree_ConfirmClickedHandler()
        Select Case MenuClicked
            Case "build_tree"
                If form_config_tree.RadioButton1.Checked Then
                    Dim fasta_file As String
                    If form_config_tree.RadioButton3.Checked Then
                        fasta_file = Path.Combine(TextBox1.Text, "combined_results.fasta")
                    Else
                        fasta_file = Path.Combine(TextBox1.Text, "combined_trimed.fasta")
                    End If
                    Dim th1 As New Thread(AddressOf MakeConcatenationTree)
                    th1.Start(fasta_file)

                Else
                    Dim fasta_folder As String
                    If form_config_tree.RadioButton3.Checked Then
                        fasta_folder = Path.Combine(TextBox1.Text, "combined_results", "aligned")
                    Else
                        fasta_folder = Path.Combine(TextBox1.Text, "combined_trimed")
                    End If
                    Dim th1 As New Thread(AddressOf Make_Coalescent_Tree)
                    th1.Start(fasta_folder)
                End If
            Case "build_tree_refs"
                ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
                DeleteDir(ref_dir)
                Directory.CreateDirectory(ref_dir)
                For i As Integer = 1 To refsView.Count
                    If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                        safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
                    End If
                Next
                If form_config_tree.RadioButton1.Checked Then
                    combine_file_horizontal(ref_dir, ".fasta", Path.Combine(TextBox1.Text, "combined_refs.fasta"), "-")
                    Dim th1 As New Thread(AddressOf MakeConcatenationTree)
                    th1.Start(Path.Combine(TextBox1.Text, "combined_refs.fasta"))
                Else
                    Dim th1 As New Thread(AddressOf Make_Coalescent_Tree)
                    th1.Start(ref_dir)
                End If
        End Select
    End Sub
    Public Sub MakeConcatenationTree(ByVal fasta_file As String)

        If File.Exists(fasta_file) Then
            Dim random_folder As String = GenerateRandomString(8)
            safe_copy(fasta_file, Path.Combine(currentDirectory, "temp", random_folder + ".fasta"))
            Dim my_args() As String = {"0", "..\temp\" + random_folder + ".fasta",
                "..\temp\" + random_folder + "_coa.tree",
                "..\temp\" + random_folder,
                "null",
                form_config_tree.NumericUpDown1.Value.ToString,
                current_thread.ToString(),
                form_config_tree.SelectedTreeProgram()
                }
            If form_config_tree.CheckBox1.Checked Then
                my_args(4) = """" + Path.Combine(TextBox1.Text, "og.txt") + """"
            End If

            do_build_tree(my_args)

            If File.Exists(Path.Combine(currentDirectory, "temp", random_folder + "_coa.tree")) Then
                Dim temp_tree As String = File.ReadAllText(Path.Combine(currentDirectory, "temp", random_folder + "_coa.tree")).Replace("):", ")1:")
                Using sw As New StreamWriter(Path.Combine(TextBox1.Text, "Concatenation.tree"))
                    sw.Write(temp_tree)
                End Using
                'safe_copy(Path.Combine(currentDirectory, "temp", random_folder + "_coa.tree"), Path.Combine(TextBox1.Text, "Concatenation.tree"))
                'safe_copy(Path.Combine(currentDirectory, "temp", random_folder, "bootstrap.trees"), Path.Combine(TextBox1.Text, "bootstrap.trees"))
                DeleteDir(Path.Combine(currentDirectory, "temp", random_folder))
                File.Delete(Path.Combine(currentDirectory, "temp", random_folder + "_coa.tree"))
                File.Delete(Path.Combine(currentDirectory, "temp", random_folder + ".fasta"))
                If form_config_tree.CheckBox1.Checked Then
                    'do_newick({TextBox1.Text, "-rootbyoutgroup Concatenation.tree -labels og.txt -output rooted.tree"})
                    'do_newick({TextBox1.Text, "-ladderize rooted.tree -output Concatenation_rooted.tree"})
                    'File.Delete(Path.Combine(TextBox1.Text, "rooted.tree"))
                    File.Delete(Path.Combine(TextBox1.Text, "og.txt"))
                End If

                If MenuClicked = "build_tree" And form_config_tree.CheckBox2.Checked Then
                    form_config_dated.tree_path = Path.Combine(form_main.TextBox1.Text, "Concatenation.tree")

                    timer_id = 10
                Else
                    MsgBox("Analysis Complete! Please check the 'Concatenation.tree' in output folder.", MsgBoxStyle.Information, "Information")
                End If
            Else
                MsgBox("Analysis Failed!", MsgBoxStyle.Information, "Information")
            End If
        Else
            MsgBox("You should Combine the results first!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub Make_Coalescent_Tree(ByVal fasta_folder As String)
        If Directory.Exists(fasta_folder) Then
            timer_id = 4
            Dim count As Integer = 0
            PB_value = 0
            Dim parallelOptions As New ParallelOptions()
            If File.Exists(Path.Combine(TextBox1.Text, "combined_genes.trees")) Then
                File.Delete(Path.Combine(TextBox1.Text, "combined_genes.trees"))
            End If
            parallelOptions.MaxDegreeOfParallelism = current_thread
            If TargetOS = "macos" Then
                parallelOptions.MaxDegreeOfParallelism = 1
            End If
            Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                     Interlocked.Add(count, 1)
                                                                     PB_value = count / refsView.Count * 100
                                                                     If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                         Dim fasta_file As String = Path.Combine(fasta_folder, refsView.Item(i - 1).Item(1).ToString + ".fasta")
                                                                         Dim tree_file As String = Path.Combine(fasta_folder, refsView.Item(i - 1).Item(1).ToString + ".tree")
                                                                         If File.Exists(fasta_file) Then
                                                                             Dim my_args() As String = {"1", """" + fasta_file + """",
                                                                             """" + tree_file + """",
                                                                             "no",
                                                                             "null",
                                                                             "0",
                                                                             "1",
                                                                             form_config_tree.SelectedTreeProgram()
                                                                             }
                                                                             do_build_tree(my_args)
                                                                         End If
                                                                     End If
                                                                 End Sub)
            PB_value = -1
            For i As Integer = 1 To refsView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    Dim tree_file As String = Path.Combine(fasta_folder, refsView.Item(i - 1).Item(1).ToString + ".tree")
                    If File.Exists(tree_file) Then
                        Dim fileContent As String = File.ReadAllText(tree_file)
                        Dim commaCount As Integer = fileContent.Count(Function(c) c = ",")
                        ' 检查逗号数量是否大于等于3物种数量大于等于4
                        If commaCount >= 3 Then
                            CombineFiles(Path.Combine(TextBox1.Text, "combined_genes.trees"), tree_file)
                        End If
                    End If
                End If
            Next

            Dim SI_astral As New ProcessStartInfo With {
                .FileName = currentDirectory + "analysis\astral.exe",
                .WorkingDirectory = currentDirectory + "analysis\",
                .CreateNoWindow = True,
                .Arguments = "-i " + """" + Path.Combine(TextBox1.Text, "combined_genes.trees") + """"
            }
            SI_astral.Arguments += " -o " + """" + Path.Combine(TextBox1.Text, "Coalescent.tree") + """"
            Dim process_filter As Process = Process.Start(SI_astral)
            process_filter.WaitForExit()
            process_filter.Close()
            If form_config_tree.CheckBox1.Checked Then
                do_newick({TextBox1.Text, "-rootbyoutgroup Coalescent.tree -labels og.txt -output Coalescent.tree"})
                'File.Delete(Path.Combine(TextBox1.Text, "rooted.tree"))
                File.Delete(Path.Combine(TextBox1.Text, "og.txt"))
            End If
            do_newick({TextBox1.Text, "-ladderize Coalescent.tree -output Coalescent.tree"})
            Dim temp_tree As String = File.ReadAllText(Path.Combine(TextBox1.Text, "Coalescent.tree")).Replace("):", ")1:")
            Using sw As New StreamWriter(Path.Combine(TextBox1.Text, "Coalescent.tree"))
                sw.Write(temp_tree)
            End Using
            If MenuClicked = "build_tree" And form_config_tree.CheckBox2.Checked Then
                form_config_dated.tree_path = Path.Combine(form_main.TextBox1.Text, "Coalescent.tree")
                timer_id = 10
            Else
                MsgBox("Analysis Complete! Please check the 'Coalescent.tree' and 'combined_genes.trees' in output folder.", MsgBoxStyle.Information, "Information")
            End If
        Else
            MsgBox("You should Combine the results first!", MsgBoxStyle.Information, "Information")
        End If
    End Sub


    Public Sub do_build_tree(ByVal args() As String)
        Dim SI_build_tree As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\build_tree.exe",
            .WorkingDirectory = currentDirectory + "analysis\"
            }
        If args(0) = "1" Then
            SI_build_tree.CreateNoWindow = True
        Else
            SI_build_tree.CreateNoWindow = False
        End If
        SI_build_tree.Arguments = "-input " + args(1)
        SI_build_tree.Arguments += " -output " + args(2)
        SI_build_tree.Arguments += " -bootstrap_output_dir " + args(3)
        SI_build_tree.Arguments += " -outgroup " + args(4)
        SI_build_tree.Arguments += " -num_bootstraps " + args(5)
        SI_build_tree.Arguments += " -num_processes " + args(6)
        SI_build_tree.Arguments += " -program " + args(7)
        Dim process_filter As Process = Process.Start(SI_build_tree)
        process_filter.WaitForExit()
        process_filter.Close()
    End Sub
    Private Sub Trim_ConfirmClickedHandler()
        Select Case MenuClicked
            Case "batch_trim_with_ref"
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf menu_batch_align_blast)
                th1.Start()
            Case "trim_with_ref"
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf menu_align_blast)
                th1.Start()
            Case Else
        End Select
    End Sub
    Private Sub Consensus_ConfirmClickedHandler()
        Select Case MenuClicked
            Case "consensus"
                out_dir = TextBox1.Text
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf do_consensus)
                Dim my_options(1) As String
                If form_config_consensus.ComboBox1.SelectedIndex = 0 Then
                    my_options(1) = "results"
                Else
                    my_options(1) = "best_refs"
                End If
                my_options(0) = form_config_consensus.NumericUpDown1.Value / 100
                th1.Start(my_options)
            Case "batch_consensus"
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf batch_consensus)
                Dim my_options(2) As String
                If form_config_consensus.ComboBox1.SelectedIndex = 0 Then
                    my_options(1) = "results"
                    my_options(2) = "1"
                Else
                    my_options(1) = "best_refs"
                    my_options(2) = "0"

                End If

                my_options(0) = form_config_consensus.NumericUpDown1.Value / 100
                th1.Start(my_options)
            Case Else
        End Select
    End Sub
    Private Sub Combine_ConfirmClickedHandler()
        Select Case MenuClicked
            Case "combine_trim"
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf do_combine)
                th1.Start(form_config_combine.CheckBox4.Checked)
            Case "combine"
                timer_id = 4
                PB_value = 0
                Dim th1 As New Thread(AddressOf do_combine)
                th1.Start(True)
            Case Else
        End Select
    End Sub
    Private Sub Basic_ConfirmClickedHandler()
        Select Case MenuClicked
            Case "filter"
                menu_filter()
            Case "refilter"
                menu_refilter()
            Case "assemble"
                menu_assemble()
            Case "batch_auto_assemble"
                menu_batch_auto_assemble()
            Case "batch_filter"
                menu_batch_filter()
            Case "auto_assemble"
                menu_auto_assemble()
            Case "iteration"
                menu_iteration()
            Case "muti_iteration"
                menu_muti_iteration()
            Case "plant_cp"
                form_config_plasty.NumericUpDown1.Value = 31
                RichTextBox1.Text = "Cite NOVOPlasty 4.3.4:" + vbCrLf + "Dierckxsens N., Mardulyn P. and Smits G. (2016) NOVOPlasty: De novo assembly of organelle genomes from whole genome data. Nucleic Acids Research, doi: 10.1093/nar/gkw955" + vbCrLf
                RichTextBox1.AppendText("Cite PGA:" + vbCrLf + "Qu X-J, Moore MJ, Li D-Z, Yi T-S. 2019. PGA: a software package for rapid, accurate, and flexible batch annotation of plastomes. Plant Methods 15:50" + vbCrLf)
                menu_plant_cp()
            Case "plant_mito"
                form_config_plasty.NumericUpDown1.Value = 63
                RichTextBox1.Text = "Cite NOVOPlasty 4.3.4:" + vbCrLf + "Dierckxsens N., Mardulyn P. and Smits G. (2016) NOVOPlasty: De novo assembly of organelle genomes from whole genome data. Nucleic Acids Research, doi: 10.1093/nar/gkw955" + vbCrLf
                menu_plant_mito()
            Case "animal_mito"
                form_config_plasty.NumericUpDown1.Value = 31
                RichTextBox1.Text = "Cite NOVOPlasty 4.3.4:" + vbCrLf + "Dierckxsens N., Mardulyn P. and Smits G. (2016) NOVOPlasty: De novo assembly of organelle genomes from whole genome data. Nucleic Acids Research, doi: 10.1093/nar/gkw955" + vbCrLf
                menu_animal_mito()
            Case "batch_plant_cp"
                form_config_plasty.NumericUpDown1.Value = 31
                RichTextBox1.Text = "Cite NOVOPlasty 4.3.4:" + vbCrLf + "Dierckxsens N., Mardulyn P. and Smits G. (2016) NOVOPlasty: De novo assembly of organelle genomes from whole genome data. Nucleic Acids Research, doi: 10.1093/nar/gkw955" + vbCrLf
                RichTextBox1.AppendText("Cite PGA:" + vbCrLf + "Qu X-J, Moore MJ, Li D-Z, Yi T-S. 2019. PGA: a software package for rapid, accurate, and flexible batch annotation of plastomes. Plant Methods 15:50" + vbCrLf)
                menu_batch_plant_cp()
            Case "batch_plant_mito"
                form_config_plasty.NumericUpDown1.Value = 63
                RichTextBox1.Text = "Cite NOVOPlasty 4.3.4:" + vbCrLf + "Dierckxsens N., Mardulyn P. and Smits G. (2016) NOVOPlasty: De novo assembly of organelle genomes from whole genome data. Nucleic Acids Research, doi: 10.1093/nar/gkw955" + vbCrLf
                menu_batch_plant_mito()
            Case "batch_animal_mito"
                form_config_plasty.NumericUpDown1.Value = 31
                RichTextBox1.Text = "Cite NOVOPlasty 4.3.4:" + vbCrLf + "Dierckxsens N., Mardulyn P. and Smits G. (2016) NOVOPlasty: De novo assembly of organelle genomes from whole genome data. Nucleic Acids Research, doi: 10.1093/nar/gkw955" + vbCrLf
                menu_batch_animal_mito()
            Case "batch_re_assemble"
                menu_batch_re_assemble()
            Case "null"
            Case Else
        End Select
        Select Case form_config_basic.ComboBox1.SelectedIndex
            Case 0
                sb = -1
            Case 1
                sb = 0
            Case 2
                sb = 10000
        End Select
    End Sub

    Private Sub 参考序列建树ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 参考序列建树ToolStripMenuItem.Click
        MenuClicked = "build_tree_refs"
        form_config_tree.Label2.Visible = True
        form_config_tree.TextBox2.Visible = True
        form_config_tree.DataGridView1.Visible = False
        form_config_tree.CheckBox2.Visible = False
        form_config_tree.GroupBox2.Enabled = False
        form_config_tree.Show()
    End Sub


    Private Sub 修订树时间ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 修订树时间ToolStripMenuItem.Click
        MenuClicked = "dated_tree"
        form_config_dated.Show()
    End Sub

    Private Sub 整理树格式ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 整理树格式ToolStripMenuItem.Click
        Dim my_form As New Tool_TipName
        my_form.Show()
    End Sub

    Function CalculateMaxDifference(fastaFile As String) As Double
        Dim maxDifference As Double = 0
        Dim sequences As List(Of String) = ReadFasta(fastaFile).Item1

        For i As Integer = 0 To sequences.Count - 2
            For j As Integer = i + 1 To sequences.Count - 1
                Dim mismatch As Integer = 0
                Dim seq1 As String = sequences(i).ToUpperInvariant
                Dim seq2 As String = sequences(j).ToUpperInvariant

                For k As Integer = 0 To seq1.Length - 1
                    If seq1(k) <> "-" AndAlso seq2(k) <> "-" AndAlso seq1(k) <> seq2(k) Then
                        mismatch += 1
                    End If
                Next

                If mismatch = 0 Then
                    Continue For
                End If

                seq1 = seq1.Replace("-", "")
                seq2 = seq2.Replace("-", "")

                If seq1.Length > 0 AndAlso seq2.Length > 0 Then
                    Dim difference = mismatch / Math.Min(seq1.Length, seq2.Length)

                    If difference > maxDifference Then
                        maxDifference = difference
                    End If
                End If
            Next
        Next

        Return maxDifference
    End Function

    Function ReadFasta(filePath As String) As (List(Of String), List(Of String))
        Dim sequences As New List(Of String)
        Dim headers As New List(Of String)
        Dim currentSequence As New Text.StringBuilder()

        Dim lines As String() = File.ReadAllLines(filePath)
        For Each line As String In lines
            If line.StartsWith(">") Then
                If currentSequence.Length > 0 Then
                    sequences.Add(currentSequence.ToString())
                    currentSequence.Clear()
                End If
                headers.Add(line.Trim())
            Else
                currentSequence.Append(line.Trim())
            End If
        Next

        If currentSequence.Length > 0 Then
            sequences.Add(currentSequence.ToString())
        End If

        Return (sequences, headers)
    End Function

    Private Sub 最大差异度ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 最大差异度ToolStripMenuItem.Click
        Dim sel_count As Integer = 0
        Dim my_input As String = InputBox("Difference equal or below:", "Count", "0.1")
        If IsNumeric(my_input) Then
            For i As Integer = 1 To refsView.Count
                If IsNumeric(DataGridView1.Rows(i - 1).Cells(8).Value) Then
                    If CSng(DataGridView1.Rows(i - 1).Cells(8).Value) <= CSng(my_input) Then
                        'sel_count += 1
                        DataGridView1.Rows(i - 1).Cells(0).Value = True
                    End If
                End If
            Next
        End If
        'MsgBox(sel_count.ToString + " were selected!", MsgBoxStyle.Information, "Information")
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 导入列表信息ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导入列表信息ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "CSV File (*.csv)|*.csv;*.CSV|ALL Files(*.*)|*.*",
            .FileName = "",
            .Multiselect = False,
            .DefaultExt = ".csv",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            load_datagrid1(opendialog.FileName)

        End If
    End Sub

    Private Sub 过滤ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 过滤ToolStripMenuItem1.Click
        Directory.CreateDirectory(TextBox1.Text)
        If Directory.GetFileSystemEntries(TextBox1.Text).Length > 0 And DebugToolStripMenuItem.Checked = False Then
            Dim result As DialogResult = MessageBox.Show("The result folder is not empty. Are you sure to proceed?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.No Then
                Exit Sub
            End If
        End If
        If TextBox1.Text <> "" Then
            DataGridView1.EndEdit()
            DataGridView2.EndEdit()
            DataGridView1.Refresh()
            DataGridView2.Refresh()
            MenuClicked = "batch_filter"
            form_config_basic.CheckBox3.Checked = False
            form_config_basic.CheckBox4.Enabled = True
            form_config_basic.GroupBox2.Enabled = True
            form_config_basic.GroupBox3.Enabled = True
            form_config_basic.GroupBox4.Enabled = False
            form_config_basic.NumericUpDown1.Value = If(form_config_calculate.kf = 0, 31, form_config_calculate.kf)
            form_config_basic.Show()

        Else
            MsgBox("Please select an output folder!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Private Sub menu_batch_filter()
        Dim refs_count As Integer = 0
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)

        timer_id = 4
        PB_value = 0
        For i As Integer = 1 To refsView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                refs_count += 1
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", ref_dir + refsView.Item(i - 1).Item(1).ToString + ".fasta", True)
            End If
        Next
        If refs_count >= 1 Then
            Dim th1 As New Thread(AddressOf batch_filter)
            th1.Start()
        Else
            MsgBox("Please select at least one reference and one sequencing data!", MsgBoxStyle.Information, "Information")
        End If
    End Sub
    Public Sub batch_filter()
        batch_task(True, True, False, False)
        MsgBox("Analysis completed!", MsgBoxStyle.Information, "Information")
    End Sub

    Public Sub save_datagrid2(ByVal file_path As String)
        DataGridView2.EndEdit()
        Dim dw As New StreamWriter(file_path, False)
        Dim state_line As String = "Selected,ID"
        For j As Integer = 2 To DataGridView2.ColumnCount - 1
            state_line += "," + DataGridView2.Columns(j).HeaderText
        Next
        dw.WriteLine(state_line)
        For i As Integer = 1 To seqsView.Count
            state_line = DataGridView2.Rows(i - 1).Cells(0).Value.ToString + "," + i.ToString
            For j As Integer = 2 To DataGridView2.ColumnCount - 1
                state_line += "," + seqsView.Item(i - 1).Item(j - 1)
            Next
            dw.WriteLine(state_line)
        Next
        dw.Close()
    End Sub

    Private Sub 保存项目文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 保存项目文件ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog With {
            .Filter = "GeneMiner Project File (*.geneminer)|*.geneminer;*.GENEMINER",
            .FileName = "",
            .DefaultExt = ".geneminer",
            .CheckFileExists = False,
            .CheckPathExists = False
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            Dim files_save_path As String = Path.Combine(Path.GetDirectoryName(opendialog.FileName), Path.GetFileNameWithoutExtension(opendialog.FileName) + "_files")
            Directory.CreateDirectory(files_save_path)
            save_datagrid1(Path.Combine(files_save_path, "datagrid1.csv"))
            save_datagrid2(Path.Combine(files_save_path, "datagrid2.csv"))

            For i As Integer = 1 To refsView.Count
                safe_copy(currentDirectory + "temp\org_seq\" + refsView.Item(i - 1).Item(1).ToString + ".fasta", Path.Combine(files_save_path, "refs", refsView.Item(i - 1).Item(1).ToString + ".fasta"), True)
            Next
            Using sw As New StreamWriter(opendialog.FileName)
                sw.WriteLine("OUTPATH=" + TextBox1.Text)
                sw.WriteLine("THREADS=" + NumericUpDown10.Value.ToString)
            End Using
        End If
    End Sub
    Public Sub load_datagrid1(ByVal file_path As String)
        DataGridView1.EndEdit()
        mydata_Dataset.Tables("Refs Table").Clear()
        Dim selectrow As New List(Of Boolean)()
        Using sr As New StreamReader(file_path)
            sr.ReadLine()
            While (Not sr.EndOfStream)
                Dim newrow() As String = sr.ReadLine().Split(",")
                refsView.AllowNew = True
                refsView.AddNew()
                refsView.Item(refsView.Count - 1).Row.ItemArray = newrow.Skip(1).ToArray()
                selectrow.Add(Boolean.Parse(newrow(0)))
            End While
        End Using
        refresh_DG1()
        DataGridView1.Update()

        For i As Integer = 0 To refsView.Count - 1
            DataGridView1.Rows(i).Cells(0).Value = selectrow(i)
        Next
    End Sub
    Public Sub load_datagrid2(ByVal file_path As String)
        DataGridView2.EndEdit()
        form_config_tree.DataGridView1.EndEdit()
        mydata_Dataset.Tables("Data Table").Clear()
        mydata_Dataset.Tables("Taxon Table").Clear()

        Dim selectrow As New List(Of Boolean)()
        Using sr As New StreamReader(file_path)
            sr.ReadLine()
            While (Not sr.EndOfStream)
                Dim newrow() As String = sr.ReadLine().Split(",")
                seqsView.AllowNew = True
                seqsView.AddNew()
                seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow.Skip(1).ToArray()
                selectrow.Add(Boolean.Parse(newrow(0)))

                Dim newrow_taxon(0) As String
                taxonView.AllowNew = True
                taxonView.AddNew()
                newrow_taxon(0) = newrow(1) + "_" + make_out_name(Path.GetFileNameWithoutExtension(newrow(2)), Path.GetFileNameWithoutExtension(newrow(3)))
                taxonView.Item(taxonView.Count - 1).Row.ItemArray = newrow_taxon

            End While
        End Using
        refresh_DG2()
        refresh_DG3()
        DataGridView2.Update()
        For i As Integer = 0 To seqsView.Count - 1
            DataGridView2.Rows(i).Cells(0).Value = selectrow(i)
        Next


    End Sub

    Public Sub load_datagrid3(ByVal file_path As String)
        form_config_tree.DataGridView1.EndEdit()
        mydata_Dataset.Tables("Taxon Table").Clear()
        Dim selectrow As New List(Of Boolean)()
        Using sr As New StreamReader(file_path)
            sr.ReadLine()
            While (Not sr.EndOfStream)
                Dim newrow() As String = sr.ReadLine().Split(",")
                seqsView.AllowNew = True
                seqsView.AddNew()
                seqsView.Item(seqsView.Count - 1).Row.ItemArray = newrow.Skip(1).ToArray()
                selectrow.Add(Boolean.Parse(newrow(0)))
            End While
        End Using
        refresh_DG3()
        form_config_tree.DataGridView1.Update()
        For i As Integer = 0 To taxonView.Count - 1
            form_config_tree.DataGridView1.Rows(i).Cells(0).Value = selectrow(i)
        Next
    End Sub

    Private Sub 载入项目文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入项目文件ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "GeneMiner Project File (*.geneminer)|*.geneminer;*.GENEMINER",
            .FileName = "",
            .DefaultExt = ".geneminer",
            .Multiselect = False,
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            Dim files_save_path As String = Path.Combine(Path.GetDirectoryName(opendialog.FileName), Path.GetFileNameWithoutExtension(opendialog.FileName) + "_files")
            If Directory.Exists(files_save_path) Then
                Directory.CreateDirectory(files_save_path)
                load_datagrid1(Path.Combine(files_save_path, "datagrid1.csv"))
                load_datagrid2(Path.Combine(files_save_path, "datagrid2.csv"))
                DeleteDir(Path.Combine(currentDirectory, "temp", "org_seq"))
                Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", "org_seq"))
                For i As Integer = 1 To refsView.Count
                    safe_copy(Path.Combine(files_save_path, "refs", refsView.Item(i - 1).Item(1).ToString + ".fasta"), Path.Combine(currentDirectory, "temp", "org_seq", refsView.Item(i - 1).Item(1).ToString + ".fasta"), True)
                Next
                Using sr As New StreamReader(opendialog.FileName)
                    While Not sr.EndOfStream
                        'sw.WriteLine("OUTPATH=" + TextBox1.Text)
                        'sw.WriteLine("THREADS=" + NumericUpDown10.Value.ToString)
                        Dim line As String = sr.ReadLine()
                        If line.StartsWith("OUTPATH") Then
                            TextBox1.Text = line.Split("=")(1)
                        End If
                        If line.StartsWith("THREADS") Then
                            NumericUpDown10.Value = CInt(line.Split("=")(1))
                        End If
                    End While
                End Using
            Else
                MsgBox("Could not find Path.GetFileNameWithoutExtension(opendialog.FileName)" + "_files")
            End If
        End If
    End Sub

    Private Sub 获取最佳参考序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 获取最佳参考序列ToolStripMenuItem.Click
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()
        out_dir = TextBox1.Text
        If Directory.Exists(Path.Combine(out_dir, "best_refs")) = False Then
            Directory.CreateDirectory(Path.Combine(out_dir, "best_refs"))
        End If
        Dim th1 As New Thread(AddressOf make_best_ref)
        th1.Start()
    End Sub

    Private Sub 获取最佳参考序列ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 获取最佳参考序列ToolStripMenuItem1.Click
        DataGridView1.EndEdit()
        DataGridView2.EndEdit()
        DataGridView1.Refresh()
        DataGridView2.Refresh()

        timer_id = 4
        PB_value = 0
        Dim th1 As New Thread(AddressOf batch_make_best_ref)
        th1.Start()
    End Sub
    Public Sub make_best_ref()
        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Parallel.For(1, refsView.Count + 1, parallelOptions, Sub(i)
                                                                 Interlocked.Add(count, 1)
                                                                 PB_value = count / refsView.Count * 100
                                                                 Dim in_path_fq As String = Path.Combine(out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                 If File.Exists(in_path_fq) Then
                                                                     Dim random_folder As String = Path.Combine(currentDirectory, "temp", GenerateRandomString(8))
                                                                     Directory.CreateDirectory(random_folder)
                                                                     SplitFastaFile(Path.Combine(currentDirectory, "temp", "org_seq", refsView.Item(i - 1).Item(1).ToString + ".fasta"), random_folder)
                                                                     'options = (0:kf,1:kr,2:q1,3:q2,4:ref,5:out_dir,6:lkd,7:rl,8:refilter,9:no_window,10:thread,11,random_folder,ref_name )
                                                                     Dim my_options() As String = {31, 31, in_path_fq, in_path_fq, random_folder, random_folder, "kmer_dict.dict", 0, "-1", "1", current_thread, random_folder, refsView.Item(i - 1).Item(1).ToString}
                                                                     find_best_ref(my_options)
                                                                 End If
                                                             End Sub)
        PB_value = 0
    End Sub
    Public Sub batch_make_best_ref()
        Dim count As Integer = 0
        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = current_thread
        }
        If TargetOS = "macos" Then
            parallelOptions.MaxDegreeOfParallelism = 1
        End If
        Parallel.For(1, seqsView.Count + 1, parallelOptions, Sub(batch_i)
                                                                 Try
                                                                     If DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                                                         Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                                                                         folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                                                                         Dim temp_out_dir = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                                                                         If Directory.Exists(Path.Combine(temp_out_dir, "best_refs")) = False Then
                                                                             Directory.CreateDirectory(Path.Combine(temp_out_dir, "best_refs"))
                                                                         End If
                                                                         For i As Integer = 1 To refsView.Count
                                                                             Interlocked.Add(count, 1)
                                                                             PB_value = count / seqsView.Count / refsView.Count * 100
                                                                             Dim in_path_fq As String = Path.Combine(temp_out_dir, "filtered", refsView.Item(i - 1).Item(1).ToString + ".fq")
                                                                             If File.Exists(in_path_fq) Then
                                                                                 Dim random_folder As String = Path.Combine(currentDirectory, "temp", GenerateRandomString(8))
                                                                                 Directory.CreateDirectory(random_folder)
                                                                                 SplitFastaFile(Path.Combine(currentDirectory, "temp", "org_seq", refsView.Item(i - 1).Item(1).ToString + ".fasta"), random_folder)
                                                                                 'options = (0:kf,1:kr,2:q1,3:q2,4:ref,5:out_dir,6:lkd,7:rl,8:refilter,9:no_window,10:thread,11,random_folder,ref_name )
                                                                                 Dim my_options() As String = {0, 0, in_path_fq, in_path_fq, random_folder, random_folder, "kmer_dict.dict", 0, "-1", "1", current_thread, random_folder, refsView.Item(i - 1).Item(1).ToString, temp_out_dir}
                                                                                 find_best_ref(my_options)
                                                                             End If
                                                                         Next
                                                                     End If
                                                                 Catch ex As Exception
                                                                 End Try
                                                             End Sub)
        PB_value = -1
        MsgBox("Analysis completed! Please check multicopy folder in output.", MsgBoxStyle.Information, "Information")
    End Sub

    Public Sub find_best_ref(ByVal options() As String)
        Dim SI_filter As New ProcessStartInfo()
        Dim filePath As String = options(5) + "\ref_reads_count_dict.txt"
        SI_filter.FileName = currentDirectory + "analysis\MainFilterNew.exe"
        SI_filter.WorkingDirectory = currentDirectory + "temp\"
        SI_filter.CreateNoWindow = (options(9) = 1)
        SI_filter.Arguments = "-r " + """" + options(4) + """"
        SI_filter.Arguments += " -q1 " + """" + options(2) + """" + " -q2 " + """" + options(3) + """"
        SI_filter.Arguments += " -o " + """" + options(5) + """"
        SI_filter.Arguments += " -kf " + form_config_basic.NumericUpDown1.Value.ToString
        SI_filter.Arguments += " -s " + form_config_basic.NumericUpDown2.Value.ToString
        SI_filter.Arguments += If(form_config_basic.CheckBox2.Checked, " -gr", "")
        SI_filter.Arguments += " -m 3"
        Dim process_filter As Process = Process.Start(SI_filter)
        process_filter.WaitForExit()
        process_filter.Close()
        Dim best_ref As String = ""
        If File.Exists(filePath) Then
            best_ref = GetMaxValueRecord(filePath)
        End If
        If best_ref <> "" Then
            safe_copy(Path.Combine(options(11), best_ref + ".fasta"), Path.Combine(options(13), "best_refs", options(12) + ".fasta"))
        End If
        DeleteDir(options(11))
    End Sub

    Public Sub batch_task(filter As Boolean, refilter As Boolean, assemble As Boolean, plasty As Boolean, Optional ByVal database_type As String = "")
        If File.Exists(TextBox1.Text + "\kmer_dict_k" + k1.ToString + ".dict") Then
            File.Delete(TextBox1.Text + "\kmer_dict_k" + k1.ToString + ".dict")
        End If

        Dim memory_used As Double = 0.1

        If filter Then
            memory_used = make_ref_dict(TextBox1.Text, ref_dir, TextBox1.Text, "kmer_dict_k" + k1.ToString + ".dict")
        End If

        If plasty Then
            memory_used = Math.Max(memory_used, form_config_plasty.NumericUpDown2.Value)
        End If

        Dim allow_thread As Integer = Math.Max(current_thread \ 4, 1)
        Dim avail_thread As Integer = Math.Max((New ComputerInfo().AvailablePhysicalMemory / 1024 / 1024 / 1024 - 4) / memory_used, 1)
        Dim concurrency As Integer = Math.Min(Math.Min(allow_thread, avail_thread), filter_thread)
        Dim parallel_options As New ParallelOptions With {
            .MaxDegreeOfParallelism = concurrency
        }
        If TargetOS = "macos" Then
            parallel_options.MaxDegreeOfParallelism = 1
        End If

        Dim allocatable_tasks = concurrency
        Dim task_threads() As Integer = Enumerable.Repeat(0, concurrency).ToArray
        Dim total_threads As Integer = current_thread

        While total_threads > 0
            Dim threads_per_task As Integer = total_threads \ allocatable_tasks
            If threads_per_task = 0 Then
                task_threads(0) += total_threads
                total_threads = 0
                Exit While
            End If
            For i = 0 To allocatable_tasks - 1
                task_threads(i) += threads_per_task
                total_threads -= threads_per_task
            Next
            allocatable_tasks \= 2
        End While

        Dim task_id As Integer = 0
        Dim task_dict As New Dictionary(Of Integer, Integer)
        Dim ticked_samples = Enumerable.Range(1, seqsView.Count).Where(Function(i) DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True").ToList

        Dim finish_count As Integer = 1
        PB_value = finish_count / (ticked_samples.Count + 1) * 100

        Parallel.ForEach(ticked_samples, parallel_options,
                         Sub(batch_i As Integer)
                             Dim folder_name As String = make_out_name(Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), Path.GetFileNameWithoutExtension(DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                             Dim out_dir As String = (TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                             Directory.CreateDirectory(out_dir)

                             Dim q1 As String = " " + """" + DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                             Dim q2 As String = " " + """" + DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"

                             If plasty Then
                                 If database_type = "mito_plant" AndAlso Not File.Exists(out_dir + "\Organelle\Gennome_cp.fasta") Then
                                     RichTextBox1.BeginInvoke(Sub() RichTextBox1.AppendText("The chloroplast genome was not found in the " + folder_name + vbCrLf))
                                     Exit Sub
                                 End If

                                 If q1 = q2 Then
                                     RichTextBox1.BeginInvoke(Sub() RichTextBox1.AppendText("It requires paired reads to assemble organelle genomes for " + folder_name + vbCrLf))
                                     Exit Sub
                                 End If
                             End If

                             Dim thread_count As Integer

                             SyncLock seqsView
                                 Dim current_thread_id = Environment.CurrentManagedThreadId
                                 If task_dict.ContainsKey(current_thread_id) Then
                                     thread_count = task_dict(current_thread_id)
                                 Else
                                     thread_count = task_threads(task_id)
                                     task_dict(current_thread_id) = thread_count
                                     task_id = Math.Min(task_id + 1, concurrency - 1)
                                 End If
                             End SyncLock

                             If filter Then
                                 Dim filter_dict_path = "..\kmer_dict_k" + k1.ToString + ".dict"
                                 Dim filter_out_dir = out_dir

                                 If plasty Then
                                     filter_dict_path = "..\" + filter_dict_path
                                     filter_out_dir = Path.Combine(out_dir, "NOVOPlasty")

                                     If My.Computer.FileSystem.DirectoryExists(filter_out_dir) Then
                                         DeleteDir(filter_out_dir)
                                     End If

                                     Directory.CreateDirectory(filter_out_dir)
                                 End If

                                 do_filter({k1, k2, q1, q2, ref_dir, filter_out_dir, filter_dict_path, If(plasty, 1, 0), 0, no_window, 1})
                             End If

                             If refilter Then
                                 If My.Computer.FileSystem.DirectoryExists(Path.Combine(out_dir, "filtered")) Then
                                     DeleteDir(Path.Combine(out_dir, "filtered"))
                                 End If

                                 do_filter({k1, k2, "", "", ref_dir, out_dir, "", 0, 1, no_window, thread_count})

                                 ' 如果两步同时进行，删除第一步过滤的临时文件
                                 If filter AndAlso (My.Computer.FileSystem.DirectoryExists(Path.Combine(out_dir, "filtered_pe")) AndAlso
                                                    My.Computer.FileSystem.DirectoryExists(Path.Combine(out_dir, "filtered"))) Then
                                     DeleteDir(Path.Combine(out_dir, "filtered_pe"))
                                 End If
                             End If

                             If assemble Then
                                 If My.Computer.FileSystem.DirectoryExists(Path.Combine(out_dir, "results")) Then
                                     DeleteDir(Path.Combine(out_dir, "results"))
                                 End If

                                 do_assemble({k1, k2, q1, q2, ref_dir, out_dir, "", 0, 0, no_window, thread_count})
                             End If

                             If plasty Then
                                 Dim plasty_dir = Path.Combine(out_dir, "NOVOPlasty")
                                 Dim count_file As String = plasty_dir + "\ref_reads_count_dict.txt"
                                 If File.Exists(count_file) Then
                                     Dim best_ref As String = ""
                                     Dim max_value As Integer = 0
                                     Using sr As New StreamReader(count_file)
                                         While Not sr.EndOfStream
                                             Dim line As String = sr.ReadLine()
                                             Dim parts As String() = line.Split(","c)

                                             If parts.Length >= 2 Then
                                                 If max_value < CInt(parts(1)) Then
                                                     max_value = CInt(parts(1))
                                                     best_ref = parts(0)
                                                 End If
                                             End If
                                         End While
                                     End Using

                                     If best_ref <> "" Then
                                         Dim best_gb As String = best_ref.Split("#")(1).Replace(".fasta", "")
                                         File.Copy(get_genome_data(database_type, "gb", best_gb).Result, plasty_dir + "\ref_gb.gb", True)
                                         File.Copy(ref_dir + best_ref + ".fasta", plasty_dir + "\" + best_ref + ".fasta", True)
                                         File.Move(plasty_dir + "\filtered\all_1.fq", plasty_dir + "\Project1.1.fq", True)
                                         File.Move(plasty_dir + "\filtered\all_2.fq", plasty_dir + "\Project1.2.fq", True)

                                         Try
                                             Directory.Delete(plasty_dir + "\filtered")
                                         Catch ex As IOException
                                         End Try

                                         If File.Exists(plasty_dir + "\best.fasta") Then
                                             File.Delete(plasty_dir + "\best.fasta")
                                         End If

                                         If File.Exists(plasty_dir + "\Circularized_assembly_1_Project1.fasta") Then
                                             File.Delete(plasty_dir + "\Circularized_assembly_1_Project1.fasta")
                                         End If

                                         If File.Exists(plasty_dir + "\Option_1_Project1.fasta") Then
                                             File.Delete(plasty_dir + "\Option_1_Project1.fasta")
                                         End If

                                         Using sw1 As New StreamWriter(plasty_dir + "\batch_file.txt")
                                             sw1.WriteLine("Project1")
                                             sw1.WriteLine(best_ref + ".fasta")
                                             sw1.WriteLine("Project1.1.fq")
                                             sw1.WriteLine("Project1.2.fq")
                                         End Using

                                         Dim config_text As String
                                         Using sr As New StreamReader(currentDirectory + "\analysis\NOVO_config.txt")
                                             config_text = sr.ReadToEnd
                                         End Using

                                         config_text = config_text.Replace("$batch_file$", "batch:batch_file.txt")
                                         config_text = config_text.Replace("$type$", form_config_plasty.ComboBox1.Text)
                                         config_text = config_text.Replace("$range$", form_config_plasty.TextBox1.Text)
                                         config_text = config_text.Replace("$k-mer$", form_config_plasty.NumericUpDown1.Value.ToString)
                                         config_text = config_text.Replace("$mem$", form_config_plasty.NumericUpDown2.Value.ToString)
                                         config_text = config_text.Replace("$read_length$", GetReadLength(plasty_dir + "\Project1.1.fq").ToString)
                                         config_text = config_text.Replace("$insert_size$", If(form_config_plasty.NumericUpDown3.Value = 0, "", form_config_plasty.NumericUpDown3.Value.ToString))
                                         config_text = config_text.Replace("$ref$", best_ref + ".fasta")
                                         config_text = config_text.Replace("$chlo$", form_config_plasty.TextBox3.Text)
                                         config_text = config_text.Replace("$out$", ".\")
                                         Using sw As New StreamWriter(plasty_dir + "\NOVO_config.txt")
                                             sw.Write(config_text)
                                         End Using

                                         Dim process_build_plasty As Process = Process.Start(New ProcessStartInfo With {
                                             .FileName = currentDirectory + "analysis\NOVOPlasty4.3.4.exe",
                                             .WorkingDirectory = plasty_dir,
                                             .CreateNoWindow = False,
                                             .Arguments = "-c NOVO_config.txt"
                                         })
                                         process_build_plasty.WaitForExit()
                                         process_build_plasty.Close()

                                         If Not DebugToolStripMenuItem.Checked Then
                                             If File.Exists(plasty_dir + "\Project1.1.fq") Then
                                                 File.Delete(plasty_dir + "\Project1.1.fq")
                                             End If
                                             If File.Exists(plasty_dir + "\Project1.2.fq") Then
                                                 File.Delete(plasty_dir + "\Project1.2.fq")
                                             End If
                                         End If

                                         If TargetOS = "macos" Then
                                             Thread.Sleep(100)
                                         End If

                                         Dim assemble_file As String = plasty_dir + "\Circularized_assembly_1_Project1.fasta"
                                         If File.Exists(plasty_dir + "\Option_1_Project1.fasta") Then
                                             Dim process_check_option As Process = Process.Start(New ProcessStartInfo With {
                                                 .FileName = currentDirectory + "analysis\check_option_blast.exe",
                                                 .WorkingDirectory = currentDirectory + "temp\",
                                                 .CreateNoWindow = False,
                                                 .Arguments = "-i " + """" + plasty_dir + """" + " -r " + """" + plasty_dir + "\" + best_ref + ".fasta" + """" + " -o " + """" + plasty_dir + "\" + "best.fasta" + """"
                                             })
                                             process_check_option.Start()
                                             process_check_option.WaitForExit()
                                             process_check_option.Close()
                                             If File.Exists(plasty_dir + "\best.fasta") Then
                                                 assemble_file = plasty_dir + "\best.fasta"
                                             End If
                                         End If

                                         If File.Exists(assemble_file) Then
                                             Directory.CreateDirectory(out_dir + "\Organelle")
                                             If database_type <> "cp" Then
                                                 Dim lines As List(Of String) = File.ReadAllLines(assemble_file).ToList()
                                                 If lines.Count > 0 Then
                                                     lines(0) = ">" + folder_name
                                                 End If
                                                 File.WriteAllLines(out_dir + "\Organelle\" + folder_name + ".fasta", lines)
                                             Else
                                                 do_PGA(plasty_dir + "\ref_gb.gb", assemble_file, plasty_dir)
                                                 If File.Exists(plasty_dir + "\output.gb") Then
                                                     Dim lines As List(Of String) = File.ReadAllLines(plasty_dir + "\output.fasta").ToList()
                                                     If lines.Count > 0 Then
                                                         lines(0) = ">" + folder_name
                                                     End If
                                                     File.WriteAllLines(out_dir + "\Organelle\" + folder_name + ".fasta", lines)

                                                     lines = File.ReadAllLines(plasty_dir + "\output.gb").ToList()
                                                     If lines.Count > 12 Then
                                                         For i As Integer = 0 To 6
                                                             lines(i) = lines(i).Replace("my_target", folder_name)
                                                         Next
                                                     End If
                                                     File.WriteAllLines(out_dir + "\Organelle\" + folder_name + ".gb", lines)
                                                 Else
                                                     Dim lines As List(Of String) = File.ReadAllLines(assemble_file).ToList()
                                                     If lines.Count > 0 Then
                                                         lines(0) = ">" + folder_name
                                                     End If
                                                     File.WriteAllLines(out_dir + "\Organelle\" + folder_name + ".fasta", lines)
                                                     RichTextBox1.BeginInvoke(Sub() RichTextBox1.AppendText("The organelle genome of the " + folder_name + " lacks annotation" + vbCrLf))
                                                 End If
                                                 If File.Exists(plasty_dir + "\warning.log") Then
                                                     File.Copy(plasty_dir + "\warning.log", out_dir + "\Organelle\" + folder_name + "_warning.log", True)
                                                 End If
                                             End If
                                         Else
                                             RichTextBox1.BeginInvoke(Sub() RichTextBox1.AppendText("The organelle genome of the " + folder_name + " is not circularized" + vbCrLf))
                                         End If
                                     Else
                                         RichTextBox1.BeginInvoke(Sub() RichTextBox1.AppendText("No filtered organelle reads for the " + folder_name + vbCrLf))
                                     End If
                                 End If
                             End If

                             If File.Exists(out_dir + "\log.txt") Then
                                 Using LogFileReader As New StreamReader(out_dir + "\log.txt")
                                     Dim line As String = ""
                                     While InlineAssignHelper(line, LogFileReader.ReadLine()) IsNot Nothing
                                         If line.ToLower.StartsWith("error") Then
                                             RichTextBox1.BeginInvoke(Sub() RichTextBox1.AppendText("Error: " + folder_name + vbCrLf))
                                         End If
                                     End While
                                 End Using
                             End If

                             Interlocked.Add(finish_count, 1)
                             PB_value = finish_count / (ticked_samples.Count + 1) * 100
                         End Sub)

        PB_value = 0
    End Sub

    Private Sub 计算参数ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 计算参数ToolStripMenuItem.Click
        form_config_calculate.Show()
    End Sub

    Public Sub build_blast_db(ByVal gene_name As String)
        Dim db_dir As String = Path.Combine(TextBox1.Text, "blast_db")
        Dim ref_path As String = Path.Combine(currentDirectory, "temp", "temp_refs", gene_name + ".fasta")

        Dim SI_makeblastdb As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\makeblastdb.exe",
            .WorkingDirectory = db_dir,
            .CreateNoWindow = True
        }
        SI_makeblastdb.ArgumentList.Add("-in")
        SI_makeblastdb.ArgumentList.Add("""" + ref_path + """")
        SI_makeblastdb.ArgumentList.Add("-out")
        SI_makeblastdb.ArgumentList.Add(gene_name)
        SI_makeblastdb.ArgumentList.Add("-dbtype")
        SI_makeblastdb.ArgumentList.Add("nucl")

        If Not SI_makeblastdb.EnvironmentVariables.ContainsKey("BLAST_USAGE_REPORT") Then
            SI_makeblastdb.EnvironmentVariables.Add("BLAST_USAGE_REPORT", "0")
        End If

        If Not SI_makeblastdb.EnvironmentVariables.ContainsKey("DO_NOT_TRACK") Then
            SI_makeblastdb.EnvironmentVariables.Add("DO_NOT_TRACK", "1")
        End If

        Dim process_makeblastdb As New Process With {
            .StartInfo = SI_makeblastdb
        }

        process_makeblastdb.Start()
        process_makeblastdb.WaitForExit()
        process_makeblastdb.Close()
    End Sub

    Public Sub do_trim_blast(ByVal gene_name As String, ByVal source_dir As String, ByVal output_dir As String)
        Dim db_path As String = Path.Combine(TextBox1.Text, "blast_db", gene_name)
        Dim in_path As String = Path.Combine(source_dir, gene_name + ".fasta")
        Dim out_path As String = Path.Combine(output_dir, gene_name + ".fasta")
        Dim ref_path As String = Path.Combine(currentDirectory, "temp", "temp_refs", gene_name + ".fasta")

        Dim SI_build_trimed As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\build_trimed.exe",
            .WorkingDirectory = currentDirectory + "analysis",
            .CreateNoWindow = True,
            .Arguments = "-i " + """" + in_path + """"
        }
        SI_build_trimed.Arguments += " -r " + """" + ref_path + """"
        SI_build_trimed.Arguments += " -o " + """" + out_path + """"
        SI_build_trimed.Arguments += " -b " + """" + db_path + """"
        SI_build_trimed.Arguments += " -m " + form_config_trim.ComboBox2.SelectedIndex.ToString
        SI_build_trimed.Arguments += " -p " + form_config_trim.NumericUpDown1.Value.ToString

        Dim process_build_trimed As New Process With {
            .StartInfo = SI_build_trimed
        }

        process_build_trimed.Start()
        process_build_trimed.WaitForExit()
        process_build_trimed.Close()
    End Sub
End Class
