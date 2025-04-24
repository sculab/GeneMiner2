Imports System.IO

Public Class Config_Combine


    ' 假设这是你要执行的操作
    Public Event ConfirmClicked()
    Public Event CancelClicked()

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If CheckBox2.Checked Then
            For batch_i As Integer = 1 To seqsView.Count
                If form_main.DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    Dim folder_name As String = make_out_name(System.IO.Path.GetFileNameWithoutExtension(form_main.DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), System.IO.Path.GetFileNameWithoutExtension(form_main.DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                    folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                    Dim temp_out_dir = (form_main.TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                    If Directory.Exists(Path.Combine(temp_out_dir, "blast")) = False Then
                        MsgBox("To use trimed results, you need to execute 'Trim With Reference' first.")
                        Exit Sub
                    End If
                End If
            Next
        End If

        Hide()
        RaiseEvent ConfirmClicked()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Hide()
        RaiseEvent CancelClicked()
    End Sub

    Private Sub Config_Combine_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If TargetOS = "macos" Then
            ComboBox1.SelectedIndex = 1
            ComboBox1.Enabled = False
        Else
            ComboBox1.SelectedIndex = 0
        End If


    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        GroupBox1.Enabled = CheckBox4.Checked

    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        TextBox2.Enabled = CheckBox3.Checked
        NumericUpDown1.Enabled = CheckBox3.Checked
    End Sub

    Private Sub CheckBox2_MouseHover(sender As Object, e As EventArgs) Handles CheckBox2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Selecting this option indicates the use of results processed based on reference alignment, which requires prior execution of 'Trim With Reference'. Deselecting this option indicates processing based on the original results."
        Else
            TextBox1.Text = "勾选表示使用基于参考切齐的结果进行处理，需要先执行'基于参考切齐'；不勾选表示基于原始的结果进行处理。"
        End If
    End Sub

    Private Sub CheckBox4_MouseHover(sender As Object, e As EventArgs) Handles CheckBox4.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Selecting the option initiates sequence alignment using the specified multiple sequence alignment program, followed by cleanup with the designated options. Deselecting the option will simply merge the sequences."
        Else
            TextBox1.Text = "勾选表示使用指定的多序列比对程序进行序列比对并使用指定的选项进行清理，不勾选表示仅仅合并序列。"
        End If
    End Sub


    Private Sub ComboBox1_MouseHover(sender As Object, e As EventArgs) Handles ComboBox1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "The parameters for MAFFT are set to '-auto', while the parameters for MUSCLE are configured as '-align'."
        Else
            TextBox1.Text = "MAFFT的参数为-auto，muscle的参数为-align"
        End If
    End Sub
    Private Sub CheckBox1_MouseHover(sender As Object, e As EventArgs) Handles CheckBox1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Selecting this option replaces all gaps ('-') in the alignment with missing data symbols ('?'). If unchecked, gaps ('-') will be retained. This option should be selected in cases of incomplete sequences."
        Else
            TextBox1.Text = "勾选表示将排序中所有的gap(-)都替换为missing(?)，否则保持gap(-)。如果序列不完整，则应当勾选该选项。"
        End If
    End Sub

    Private Sub CheckBox3_MouseHover(sender As Object, e As EventArgs) Handles CheckBox3.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Selecting this option enables automatic cleanup. The program calculates the difference rate between each pair of sequences (the number of base differences divided by the length of the shorter sequence in the pair). It then extracts the largest subset of sequence pairs from the collection, ensuring that their pairwise difference rates do not exceed the 'Maximum Difference' threshold. If the number of sequences in this largest subset is greater than or equal to the 'Number of sequences' specified, this subset is used as the result. Otherwise, the alignment will be removed."
        Else
            TextBox1.Text = "勾选表示执行自动清理。程序将计算两两序列间的差异率（碱基差异的数量除以两条序列中较短序列的长度），然后从序列集合中获取两两比对的差异率不超过所设定的‘最大差异’的最大子集，如果最大子集的序列数量大于等于所设定的‘序列数量’，则用该子集作为结果，否则删除该基因。"
        End If
    End Sub

    Private Sub Label1_MouseHover(sender As Object, e As EventArgs) Handles Label1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Retrieve the largest subset from the sequence collection where the pairwise comparison difference rate does not exceed the set value."
        Else
            TextBox1.Text = "从序列集合中获取两两比对的差异率不超过所设定的值的最大子集"
        End If
    End Sub

    Private Sub Label2_MouseHover(sender As Object, e As EventArgs) Handles Label2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "If the number of sequences in the largest subset is greater than or equal to the set value, then the subset is used as the result; otherwise, the alignment is deleted."
        Else
            TextBox1.Text = "如果最大子集的序列数量大于等于所设定的值，则用该序列子集中作为结果，否则删除该基因。"
        End If
    End Sub

    Private Sub Label3_MouseHover(sender As Object, e As EventArgs) Handles Label3.MouseHover
        If language = "EN" Then
            TextBox1.Text = "The parameters for MAFFT are set to '-auto', while the parameters for MUSCLE are configured as '-align'."
        Else
            TextBox1.Text = "MAFFT的参数为-auto，muscle的参数为-align"
        End If
    End Sub

    Private Sub TextBox2_MouseHover(sender As Object, e As EventArgs) Handles TextBox2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Retrieve the largest subset from the sequence collection where the pairwise comparison difference rate does not exceed the set value."
        Else
            TextBox1.Text = "从序列集合中获取两两比对的差异率不超过所设定的值的最大子集"
        End If
    End Sub

    Private Sub NumericUpDown1_MouseHover(sender As Object, e As EventArgs) Handles NumericUpDown1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "If the number of sequences in the largest subset is greater than or equal to the set value, then the subset is used as the result; otherwise, the alignment is deleted."
        Else
            TextBox1.Text = "如果最大子集的序列数量大于等于所设定的值，则用该序列子集中作为结果，否则删除序列。"
        End If
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged

    End Sub
End Class