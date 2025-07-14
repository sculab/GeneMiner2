Imports System.IO

Public Class Config_CP
    Public GenusDictionary As New Dictionary(Of String, String)
    'Public combinedArray As New List(Of String)()
    Public select_class As Boolean = False

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        ' 获取用户输入的文本
        Dim userInput As String = TextBox1.Text.ToLower
        If userInput.Length > 1 And select_class = False Then
            ' 清空ComboBox的选项
            ListBox1.Items.Clear()

            ' 根据用户输入刷新ComboBox的选项
            Dim myHashSet As New HashSet(Of String)()
            For Each item As String In GenusDictionary.Keys
                If CheckBox2.Checked = False Then
                    For Each i As String In item.Split(";")
                        If i.ToLower().StartsWith(userInput.ToLower) Then
                            myHashSet.Add(GenusDictionary(item))
                            Exit For
                        End If
                    Next
                Else
                    If item.ToLower().StartsWith(userInput.ToLower) Then
                        myHashSet.Add(GenusDictionary(item))
                    End If
                End If

            Next
            Dim sortedStrings As List(Of String) = myHashSet.ToList()
            sortedStrings.Sort()
            For Each item As String In sortedStrings
                ListBox1.Items.Add(item)
            Next
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ListBox3.Items.Count >= 1 Then
            waiting = True
            timer_id = 1
            mydata_Dataset.Tables("Refs Table").Clear()
            form_main.DataGridView1.DataSource = Nothing
            data_loaded = False
            Select Case cpg_down_mode
                Case 0 '获取叶绿体序列
                    If CheckBox1.Checked Then
                        refs_type = "fasta"
                        Dim th1 As New Threading.Thread(AddressOf get_fasta)
                        th1.Start("cp")
                    Else
                        refs_type = "gb"
                        Dim th1 As New Threading.Thread(AddressOf get_gb)
                        th1.Start("cp")
                    End If

                Case 1 '拼接叶绿体
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf assemble_genome)
                    th1.Start("cp")

                Case 2 '获取植物线粒体
                    If CheckBox1.Checked Then
                        refs_type = "fasta"
                        Dim th1 As New Threading.Thread(AddressOf get_fasta)
                        th1.Start("mito_plant")
                    Else
                        refs_type = "gb"
                        Dim th1 As New Threading.Thread(AddressOf get_gb)
                        th1.Start("mito_plant")
                    End If
                Case 3 '拼接植物线粒体
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf assemble_genome)
                    th1.Start("mito_plant")

                Case 4 '批量叶绿体
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf assemble_genome)
                    th1.Start("cp")
                Case 5 '批量植物线粒体
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf assemble_genome)
                    th1.Start("mito_plant")
                Case 6 'AGS353
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf get_AGS353)
                    th1.Start("AGS353")
                Case 7 '获取哺乳线粒体
                    If CheckBox1.Checked Then
                        refs_type = "fasta"
                        Dim th1 As New Threading.Thread(AddressOf get_fasta)
                        th1.Start("mito")
                    Else
                        refs_type = "gb"
                        Dim th1 As New Threading.Thread(AddressOf get_gb)
                        th1.Start("mito")
                    End If
                Case 8 '拼接哺乳线粒体
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf assemble_genome)
                    th1.Start("mito")
                Case 9 '批量哺乳线粒体
                    refs_type = "fasta"
                    Dim th1 As New Threading.Thread(AddressOf assemble_genome)
                    th1.Start("mito")
            End Select

            Me.Hide()
        Else
            MsgBox("At least one genus must be selected!")
        End If

    End Sub
    Public Sub get_gb(ByVal database_type As String)
        DeleteDir(root_path + "temp\org_seq")
        Directory.CreateDirectory(root_path + "temp\org_seq")
        Dim sw As New StreamWriter(root_path + "temp\temp.gb")
        Dim info_list As String = currentDirectory + "\analysis\info_list_" + database_type + ".tsv"
        Dim total_line As String = GetLineCount(info_list)
        Dim line_count As Integer = 0
        Using reader As New StreamReader(info_list)

            While Not reader.EndOfStream
                Dim line_list() As String = reader.ReadLine().Split(vbTab)

                If line_list.Length > 4 Then
                    If ListBox3.Items.Contains(line_list(3).Split(" ")(0)) Then
                        Dim my_gb_file As String = get_genome_data(database_type, "gb", line_list(1)).Result
                        If my_gb_file <> "" Then
                            Dim sr As New StreamReader(my_gb_file)
                            sw.Write(sr.ReadToEnd)
                            sr.Close()
                        End If
                    End If

                End If
                line_count += 1
                PB_value = line_count / total_line * 100
            End While
        End Using
        sw.Close()
        PB_value = 0
        current_file = root_path + "temp\temp.gb"
        timer_id = 5
    End Sub
    Public Sub get_fasta(ByVal database_type As String)
        DeleteDir(root_path + "temp\org_seq")
        Directory.CreateDirectory(root_path + "temp\org_seq")
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        out_dir = form_main.TextBox1.Text.Replace("\", "/")
        Dim info_list As String = currentDirectory + "\analysis\info_list_" + database_type + ".tsv"
        Dim total_line As String = GetLineCount(info_list)
        Dim line_count As Integer = 0
        Using reader As New StreamReader(currentDirectory + "\analysis\info_list_" + database_type + ".tsv")
            While Not reader.EndOfStream
                Dim line_list() As String = reader.ReadLine().Split(vbTab)
                If line_list.Length > 4 Then
                    If ListBox3.Items.Contains(line_list(3).Split(" ")(0)) Then
                        Dim my_gb_file As String = get_genome_data(database_type, "fasta", line_list(1)).Result
                        If my_gb_file <> "" Then
                            File.Copy(my_gb_file, root_path + "temp\org_seq\" + line_list(3).Replace(" ", "_").Replace(".", "").Replace("'", "") + "#" + line_list(1) + ".fasta", True)
                            safe_copy(my_gb_file, ref_dir + line_list(3).Replace(" ", "_").Replace(".", "").Replace("'", "") + "#" + line_list(1) + ".fasta", True)
                        End If
                    End If
                End If
                line_count += 1
                PB_value = line_count / total_line * 100
            End While
        End Using
        PB_value = 0
        refresh_file()
        timer_id = 2
    End Sub
    Public Sub get_AGS353(ByVal database_type As String)
        DeleteDir(root_path + "temp\org_seq")
        Directory.CreateDirectory(root_path + "temp\org_seq")
        DeleteDir(root_path + "temp\AGS353")
        Directory.CreateDirectory(root_path + "temp\AGS353")
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        out_dir = form_main.TextBox1.Text.Replace("\", "/")
        Dim info_list As String = currentDirectory + "\analysis\info_list_" + database_type + ".tsv"
        Dim total_line As String = GetLineCount(info_list)
        Dim line_count As Integer = 0
        Using reader As New StreamReader(currentDirectory + "\analysis\info_list_" + database_type + ".tsv")
            While Not reader.EndOfStream
                Dim line_list() As String = reader.ReadLine().Split(vbTab)
                If line_list.Length >= 4 Then
                    If ListBox3.Items.Contains(line_list(2)) Then
                        Dim my_gb_file As String = get_genome_data(database_type, "fasta", line_list(1)).Result
                        If my_gb_file <> "" Then
                            File.Copy(my_gb_file, root_path + "temp\AGS353\" + line_list(1) + ".fasta", True)


                            'File.Copy(my_gb_file, root_path + "temp\org_seq\" + line_list(3).Replace(" ", "_").Replace(".", "") + "#" + line_list(1) + ".fasta", True)
                            'safe_copy(my_gb_file, ref_dir + line_list(3).Replace(" ", "_").Replace(".", "") + "#" + line_list(1) + ".fasta", True)
                        End If
                    End If
                End If
                line_count += 1
                PB_value = line_count / total_line * 100
            End While
        End Using

        Dim SI_build_database As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\build_database.exe",
            .WorkingDirectory = currentDirectory + "analysis\",
            .CreateNoWindow = False,
            .Arguments = "-i " + """" + currentDirectory + "temp\AGS353" + """"
        }
        SI_build_database.Arguments += " -o " + """" + currentDirectory + "temp\org_seq" + """"
        'SI_build_database.Arguments += " -exclude " + exclude_txt
        Dim process_build_gb As Process = Process.Start(SI_build_database)
        process_build_gb.WaitForExit()
        process_build_gb.Close()

        PB_value = 0
        refresh_file()
        timer_id = 2
    End Sub
    Public Sub assemble_genome(ByVal database_type As String)
        DeleteDir(root_path + "temp\org_seq")
        Directory.CreateDirectory(root_path + "temp\org_seq")
        ref_dir = (currentDirectory + "temp\temp_refs\").Replace("\", "/")
        DeleteDir(ref_dir)
        Directory.CreateDirectory(ref_dir)
        out_dir = form_main.TextBox1.Text.Replace("\", "/")
        Dim my_list As New List(Of String)
        Using reader As New StreamReader(currentDirectory + "\analysis\info_list_" + database_type + ".tsv")
            While Not reader.EndOfStream
                Dim line_list() As String = reader.ReadLine().Split(vbTab)
                If line_list.Length > 4 Then
                    If ListBox3.Items.Contains(line_list(3).Split(" ")(0)) Then
                        my_list.Add(line_list(3) + vbTab + line_list(1))

                    End If
                End If
            End While
        End Using
        If my_list.Count = 0 Then
            MsgBox("Could not find reference!")
            Exit Sub
        End If
        For i As Integer = 1 To my_list.Count
            PB_value = i / my_list.Count * 100
            Dim my_gb_file As String = get_genome_data(database_type, "fasta", my_list(i - 1).Split(vbTab)(1)).Result
            If my_gb_file <> "" Then
                File.Copy(my_gb_file, root_path + "temp\org_seq\" + my_list(i - 1).Split(vbTab)(0).Replace(" ", "_").Replace(".", "").Replace("'", "") + "#" + my_list(i - 1).Split(vbTab)(1) + ".fasta", True)
                safe_copy(my_gb_file, ref_dir + my_list(i - 1).Split(vbTab)(0).Replace(" ", "_").Replace(".", "").Replace("'", "") + "#" + my_list(i - 1).Split(vbTab)(1) + ".fasta", True)
            End If
        Next
        Dim length_range() As Integer = refresh_file()
        PB_value = 0
        If cpg_down_mode = 4 Or cpg_down_mode = 5 Or cpg_down_mode = 9 Then
            cpg_assemble_mode = -1
            form_config_plasty.TextBox1.Text = (CInt(length_range(0) * 0.8 / 1000) * 1000).ToString + "-" + (CInt(length_range(1) * 1.2 / 1000) * 1000).ToString
            form_config_plasty.TextBox5.Text = "Gennome_" + database_type
            form_config_plasty.ComboBox1.Enabled = False
            form_config_plasty.Button2.Enabled = False
            If cpg_down_mode = 4 Then
                form_config_plasty.ComboBox1.SelectedIndex = 0
                form_config_plasty.TextBox3.Text = ""
            ElseIf cpg_down_mode = 9 Then
                form_config_plasty.ComboBox1.SelectedIndex = 1
                form_config_plasty.TextBox3.Text = ""
            Else
                form_config_plasty.ComboBox1.SelectedIndex = 2
                form_config_plasty.TextBox3.Text = "..\Organelle\Gennome_cp.fasta"
            End If
            form_config_plasty.TextBox2.Text = "batch"
            form_config_plasty.CheckBox1.Visible = False
            timer_id = 9
            Exit Sub
        End If
        timer_id = 2
        q1 = ""
        q2 = ""
        For i As Integer = 1 To seqsView.Count
            If form_main.DataGridView2.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                q1 += " " + """" + form_main.DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                If form_main.DataGridView2.Rows(i - 1).Cells(3).FormattedValue.ToString = "" Then
                    q2 += " " + """" + form_main.DataGridView2.Rows(i - 1).Cells(2).Value.ToString.Replace("\", "/") + """"
                Else
                    q2 += " " + """" + form_main.DataGridView2.Rows(i - 1).Cells(3).Value.ToString.Replace("\", "/") + """"
                End If
            End If
        Next
        If File.Exists(out_dir + "\ref_reads_count_dict.txt") Then
            File.Delete(out_dir + "\ref_reads_count_dict.txt")
        End If
        If File.Exists(out_dir + "\kmer_dict_k" + k1.ToString + ".dict") Then
            File.Delete(out_dir + "\kmer_dict_k" + k1.ToString + ".dict")
        End If

        Dim SI_filter As New ProcessStartInfo()
        Dim count_file As String = out_dir + "\ref_reads_count_dict.txt"
        If File.Exists(count_file) Then
            File.Delete(count_file)
        End If
        SI_filter.FileName = currentDirectory + "analysis\MainFilterNew.exe"
        SI_filter.WorkingDirectory = currentDirectory + "temp\"
        SI_filter.CreateNoWindow = False
        SI_filter.Arguments = "-r " + """" + ref_dir + """"
        SI_filter.Arguments += " -q1" + q1 + " -q2" + q2
        SI_filter.Arguments += " -o " + """" + out_dir + """"
        SI_filter.Arguments += " -kf " + k1
        SI_filter.Arguments += " -s " + form_config_basic.NumericUpDown2.Value.ToString
        SI_filter.Arguments += If(form_config_basic.CheckBox2.Checked, " -gr", "")
        SI_filter.Arguments += If(form_config_basic.CheckBox3.Checked, " -m_reads " + form_config_basic.NumericUpDown3.Value.ToString, "")
        SI_filter.Arguments += " -lb -m 1"
        Dim process_filter As Process = Process.Start(SI_filter)
        process_filter.WaitForExit()
        process_filter.Close()
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
                File.Copy(get_genome_data(database_type, "gb", best_gb).Result, currentDirectory + "temp\ref_gb.gb", True)

                File.Copy(ref_dir + best_ref + ".fasta", currentDirectory + "temp\" + best_ref + ".fasta", True)
                File.Move(out_dir + "\filtered\all_1.fq", currentDirectory + "temp\Project1.1.fq", True)
                File.Move(out_dir + "\filtered\all_2.fq", currentDirectory + "temp\Project1.2.fq", True)
                Dim sw As New StreamWriter(currentDirectory + "temp\batch_file.txt")
                sw.WriteLine("Project1")
                sw.WriteLine(best_ref + ".fasta")
                sw.WriteLine("Project1.1.fq")
                sw.WriteLine("Project1.2.fq")
                sw.Close()
                Select Case database_type
                    Case "mito_plant"
                        form_config_plasty.ComboBox1.SelectedIndex = 2
                        form_config_plasty.ComboBox1.Enabled = False
                        form_config_plasty.TextBox2.Text = best_ref + ".fasta"
                        form_config_plasty.Button2.Enabled = False
                        form_config_plasty.CheckBox1.Visible = True
                        form_config_plasty.TextBox3.Text = "cpg.fasta"
                        cpg_assemble_mode = 2
                    Case "cp"
                        form_config_plasty.ComboBox1.SelectedIndex = 0
                        form_config_plasty.ComboBox1.Enabled = False
                        form_config_plasty.Button2.Enabled = False
                        form_config_plasty.TextBox3.Text = ""
                        form_config_plasty.CheckBox1.Visible = True
                        form_config_plasty.TextBox2.Text = best_ref + ".fasta"
                        cpg_assemble_mode = 0
                    Case "mito"
                        form_config_plasty.ComboBox1.SelectedIndex = 1
                        form_config_plasty.ComboBox1.Enabled = False
                        form_config_plasty.Button2.Enabled = False
                        form_config_plasty.TextBox3.Text = ""
                        form_config_plasty.CheckBox1.Visible = True
                        form_config_plasty.TextBox2.Text = best_ref + ".fasta"
                        cpg_assemble_mode = 1
                End Select
                form_config_plasty.TextBox1.Text = (CInt(length_range(0) * 0.8 / 1000) * 1000).ToString + "-" + (CInt(length_range(1) * 1.2 / 1000) * 1000).ToString

                form_config_plasty.TextBox5.Text = "Gennome_" + database_type
                timer_id = 7
            Else
                MsgBox("Faild to get seed!")
            End If
        Else
            MsgBox("Faild to get seed!")
        End If
    End Sub


    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        For Each selectedItem As Object In ListBox1.SelectedItems
            If Not ListBox3.Items.Contains(selectedItem) Then
                ListBox3.Items.Add(selectedItem)
            End If
        Next
    End Sub


    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If ListBox3.SelectedIndex >= 0 Then
            ListBox3.Items.RemoveAt(ListBox3.SelectedIndex)
        End If

    End Sub

    Private Sub Config_CP_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Config_CP_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Me.Visible Then
            make_genus_dict()
        End If
    End Sub
    Public Sub make_genus_dict()
        GenusDictionary.Clear()

        Select Case cpg_down_mode
            Case 0, 1, 4
                Using reader As New StreamReader(currentDirectory + "\analysis\genus_list_cp.txt")
                    While Not reader.EndOfStream
                        Dim my_line As String = reader.ReadLine()
                        If CheckBox2.Checked = False Then
                            GenusDictionary(my_line.Split(vbTab)(1)) = my_line.Split(vbTab)(0)
                        Else
                            GenusDictionary(my_line.Split(vbTab)(0)) = my_line.Split(vbTab)(0)
                        End If
                    End While
                End Using
            Case 2, 3, 5
                Using reader As New StreamReader(currentDirectory + "\analysis\genus_list_mito_plant.txt")
                    While Not reader.EndOfStream
                        Dim my_line As String = reader.ReadLine()
                        If CheckBox2.Checked = False Then
                            GenusDictionary(my_line.Split(vbTab)(1)) = my_line.Split(vbTab)(0)
                        Else
                            GenusDictionary(my_line.Split(vbTab)(0)) = my_line.Split(vbTab)(0)
                        End If
                    End While
                End Using
            Case 6
                Using reader As New StreamReader(currentDirectory + "\analysis\genus_list_AGS353.txt")
                    While Not reader.EndOfStream
                        Dim my_line As String = reader.ReadLine()
                        If CheckBox2.Checked = False Then
                            GenusDictionary(my_line.Split(vbTab)(1)) = my_line.Split(vbTab)(0)
                        Else
                            GenusDictionary(my_line.Split(vbTab)(0)) = my_line.Split(vbTab)(0)
                        End If
                    End While
                End Using
            Case 7, 8, 9
                Using reader As New StreamReader(currentDirectory + "\analysis\genus_list_mito.txt")
                    While Not reader.EndOfStream
                        Dim my_line As String = reader.ReadLine()
                        If CheckBox2.Checked = False Then
                            GenusDictionary(my_line.Split(vbTab)(1)) = my_line.Split(vbTab)(0)
                        Else
                            GenusDictionary(my_line.Split(vbTab)(0)) = my_line.Split(vbTab)(0)
                        End If
                    End While
                End Using
        End Select
        ListBox1.Items.Clear()
        ListBox3.Items.Clear()
        TextBox1.Text = ""
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If Me.Visible Then
            make_genus_dict()

        End If
    End Sub

    Private Sub 全选ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全选ToolStripMenuItem.Click
        Dim tempItems As New List(Of Object)
        For Each item In ListBox1.Items
            tempItems.Add(item)
        Next

        ' 遍历临时列表，并选中ListBox中的每一项
        For Each item In tempItems
            ListBox1.SetSelected(ListBox1.Items.IndexOf(item), True)
        Next
    End Sub

    Private Sub 清空ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 清空ToolStripMenuItem.Click
        ListBox3.Items.Clear()
    End Sub


End Class