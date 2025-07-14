Imports System.IO
Imports System.Threading
Public Class Config_MCMC
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If File.Exists(TextBox2.Text) Then
            If CSng(TextBox4.Text) > 0 Then
                Dim th1 As New Thread(AddressOf do_mcmctree)
                th1.Start()
                Me.Hide()
            Else
                MsgBox("The variance of the mutation rate must be greater than 0.")
            End If
        Else
            MsgBox("Sequence file must be provided.")
        End If


    End Sub

    Public Sub do_mcmctree()
        Dim rgene_a As Single = CSng(TextBox3.Text) * CSng(TextBox3.Text) / CSng(TextBox4.Text)
        Dim rgene_b As Single = CSng(TextBox3.Text) / CSng(TextBox4.Text)
        If TextBox2.Text.ToLower.EndsWith(".phy") = False Then
            ConvertFastaToPhylip(TextBox2.Text, Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_infile.phy"))
        End If
        safe_delete(Path.Combine(form_main.TextBox1.Text, "mcmctree", "FigTree.tre"))
        Dim repeat_times As Integer = 1
        If CheckBox2.Checked Then
            repeat_times = 2
        End If

        Dim parallelOptions As New ParallelOptions With {
            .MaxDegreeOfParallelism = repeat_times
        }
        Parallel.For(1, repeat_times + 1, parallelOptions, Sub(i)
                                                               If i = 2 Then
                                                                   Thread.Sleep(100)
                                                               End If
                                                               Using sr As New StreamReader(Path.Combine(root_path, "analysis", "mcmc_config.ctl"))
                                                                   Using sw As New StreamWriter(Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_config_" + i.ToString + ".txt"))
                                                                       Dim temp_config As String = sr.ReadToEnd
                                                                       temp_config = temp_config.Replace("{seed}", TextBox1.Text)
                                                                       temp_config = temp_config.Replace("{rootage}", NumericUpDown1.Value.ToString)
                                                                       temp_config = temp_config.Replace("{model}", ComboBox1.SelectedIndex.ToString)
                                                                       temp_config = temp_config.Replace("{alpha}", ComboBox2.SelectedIndex.ToString)
                                                                       temp_config = temp_config.Replace("{rgene_a}", rgene_a.ToString)
                                                                       temp_config = temp_config.Replace("{rgene_b}", rgene_b.ToString)
                                                                       temp_config = temp_config.Replace("{burnin}", NumericUpDown2.Value.ToString)
                                                                       temp_config = temp_config.Replace("{sqmpfreq}", NumericUpDown3.Value.ToString)
                                                                       temp_config = temp_config.Replace("{nsample}", NumericUpDown4.Value.ToString)
                                                                       temp_config = temp_config.Replace("{cleandata}", ComboBox3.SelectedIndex.ToString)
                                                                       temp_config = temp_config.Replace("{r}", "_" + i.ToString)
                                                                       sw.Write(temp_config)
                                                                   End Using
                                                               End Using

                                                               Dim SI_mcmctree As New ProcessStartInfo With {
                                                                   .FileName = currentDirectory + "analysis\mcmctree.exe",
                                                                   .WorkingDirectory = Path.Combine(form_main.TextBox1.Text, "mcmctree"), ' 替换为实际的运行文件夹
                                                                   .CreateNoWindow = False,
                                                                   .Arguments = "MCMC_config_" + i.ToString + ".txt"
                                                               }
                                                               Dim process_filter As Process = Process.Start(SI_mcmctree)
                                                               process_filter.WaitForExit()
                                                               process_filter.Close()
                                                               If File.Exists(Path.Combine(form_main.TextBox1.Text, "mcmctree", "FigTree.tre")) Then
                                                                   Using sr As New StreamReader(Path.Combine(form_main.TextBox1.Text, "mcmctree", "FigTree.tre"))
                                                                       Using sw As New StreamWriter(Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_dated_" + i.ToString + ".tree"))
                                                                           sw.WriteLine(sr.ReadLine())
                                                                           sw.WriteLine(sr.ReadLine())
                                                                           sw.WriteLine(sr.ReadLine())
                                                                           Dim dated_tree As String = sr.ReadLine().Substring(11)
                                                                           Dim temp_tree As String = ""
                                                                           Dim temp_node_id As Integer = -1
                                                                           Dim fossil_count As Integer = 0
                                                                           For Each my_char As Char In dated_tree
                                                                               temp_tree += my_char
                                                                               If my_char = ")" Then
                                                                                   temp_node_id += 1
                                                                                   Dim temp_sv As String = time_view.Item(temp_node_id).Item(1).ToString
                                                                                   temp_tree += " [&support_value=" + temp_sv + "]"
                                                                               End If
                                                                           Next
                                                                           temp_tree = temp_tree.Replace("] [&", ",")
                                                                           sw.WriteLine("UTREE 1 = " + temp_tree)
                                                                           sw.Write(sr.ReadToEnd)
                                                                       End Using

                                                                   End Using
                                                                   safe_delete(Path.Combine(form_main.TextBox1.Text, "mcmctree", "FigTree.tre"))
                                                                   safe_delete(Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_config_" + i.ToString + ".txt"))

                                                               End If
                                                           End Sub)

        If form_config_mcmc.CheckBox1.Checked Then
            safe_delete(Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_infile.phy"))
            safe_delete(Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_intree.tree"))
            safe_delete(Path.Combine(form_main.TextBox1.Text, "mcmctree", "SeedUsed"))
        End If
        Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        ' 根据用户的选择执行相应的操作
        If result = DialogResult.Yes Then
            Process.Start("explorer.exe", """" + Path.Combine(form_main.TextBox1.Text, "mcmctree") + """")
        End If
    End Sub
    Private Sub Config_MCMC_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 4
        ComboBox2.SelectedIndex = 1
        ComboBox3.SelectedIndex = 1
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa|PHYLIP File|*.phy",
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


    Private Sub Label1_MouseHover(sender As Object, e As EventArgs) Handles Label1.MouseHover
        If language = "EN" Then
            TextBox5.Text = "-1 generates a random seed randomly, other values specify a seed."
        Else
            TextBox5.Text = "-1为随机生成随机数种子，其余为指定种子。"
        End If
    End Sub

    Private Sub Label2_MouseHover(sender As Object, e As EventArgs) Handles Label2.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Sequence file, supports fasta and phylip formats."
        Else
            TextBox5.Text = "序列文件，支持fasta格式和phylip格式。"
        End If
    End Sub

    Private Sub Label3_MouseHover(sender As Object, e As EventArgs) Handles Label3.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Upper bound calibration for the tree root, effective only when root time calibration is not set in the tree file."
        Else
            TextBox5.Text = "树根部的上界校准，仅当在树文件没有设置根上时间校准时候起作用。"
        End If
    End Sub

    Private Sub Label14_MouseHover(sender As Object, e As EventArgs) Handles Label14.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Include or exclude gaps and missing data."
        Else
            TextBox5.Text = "包含或者不包含gap和missing"
        End If
    End Sub

    Private Sub Label4_MouseHover(sender As Object, e As EventArgs) Handles Label4.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Evolutionary models. JC69 model suitable for closely related species, while HKY85 model suitable for distantly related species."
        Else
            TextBox5.Text = "进化模型。其中JC69模型适用于近缘物种，HKY85模型适用远缘物种。"
        End If
    End Sub


    Private Sub Label13_MouseHover(sender As Object, e As EventArgs) Handles Label13.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Parameters of the gamma distribution followed by the evolutionary rate at the site. If set to Auto, the prior distribution is assigned by alpha_gamma; otherwise, the evolutionary rate is consistent across the entire sequence."
        Else
            TextBox5.Text = "位点上进化速率所服从gamma分布的参数。设为Auto由alpha_gamma赋予先验分布，否则整条序列上进化速率一致。"
        End If
    End Sub

    Private Sub Label5_MouseHover(sender As Object, e As EventArgs) Handles Label5.MouseHover
        If language = "EN" Then
            TextBox5.Text = "The prior value of the mean number of mutations, expressed in mutations per 10^8 years."
        Else
            TextBox5.Text = "突变次数的均值的先验值，单位为次/10^8年"
        End If
    End Sub

    Private Sub Label6_MouseHover(sender As Object, e As EventArgs) Handles Label6.MouseHover
        If language = "EN" Then
            TextBox5.Text = "The prior value of the variance of the number of mutations, expressed in mutations per 10^8 years."
        Else
            TextBox5.Text = "突变次数的方差的先验值，单位为次/10^8年"
        End If
    End Sub

    Private Sub Label10_MouseHover(sender As Object, e As EventArgs) Handles Label10.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Iterations not included in the final statistical results. Total iterations = Burn-in + Sample Frequence * Number of sample."
        Else
            TextBox5.Text = "不计入最终统计结果的迭代。总迭代数=舍弃迭代+抽样频率*抽样次数"
        End If
    End Sub

    Private Sub Label11_MouseHover(sender As Object, e As EventArgs) Handles Label11.MouseHover
        If language = "EN" Then
            TextBox5.Text = "The frequency of sampling. Total iterations = Burn-in + Sample Frequence * Number of sample."
        Else
            TextBox5.Text = "抽样的频率。总迭代数=舍弃迭代+抽样频率*抽样次数"
        End If
    End Sub

    Private Sub Label12_MouseHover(sender As Object, e As EventArgs) Handles Label12.MouseHover
        If language = "EN" Then
            TextBox5.Text = "The number of sampling instances. Total iterations = Burn-in + Sample Frequence * Number of sample."
        Else
            TextBox5.Text = "抽样的次数。总迭代数=舍弃迭代+抽样频率*抽样次数"
        End If
    End Sub

    Private Sub CheckBox1_MouseHover(sender As Object, e As EventArgs) Handles CheckBox1.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Delete all generated temporary files."
        Else
            TextBox5.Text = "删除所有产生的临时文件。"
        End If
    End Sub

    Private Sub CheckBox2_MouseHover(sender As Object, e As EventArgs) Handles CheckBox2.MouseHover
        If language = "EN" Then
            TextBox5.Text = "Selecting this option analyzes the same data twice to ensure convergence."
        Else
            TextBox5.Text = "选中该选项将同一个数据分析两次，以确保收敛。"
        End If
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Hide()
    End Sub

End Class