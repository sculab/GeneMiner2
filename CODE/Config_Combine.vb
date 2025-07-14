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
        If TargetOS = "win64" Then
            ComboBox1.SelectedIndex = 0
        Else
            ComboBox1.SelectedIndex = 1
            If TargetOS = "macos" Then
                ComboBox1.Enabled = False
            End If
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
            TextBox1.Text = "Selecting this option enables automatic cleanup. The program calculates the difference of sequence overlaps and clusters sequences with a pairwise difference lower than the 'Maximum Difference' threshold. If a cluster is larger than or equal to the specified 'Number of sequences', the sequences are added to the alignment."
        Else
            TextBox1.Text = "勾选表示执行自动清理。程序将计算序列重叠区域的差异率，然后对差异率不超过所设定的‘最大差异’的序列进行聚类。如果一组重叠序列的数量超过所设定的‘序列数量’，则将这组序列加入多序列比对中，否则删除。"
        End If
    End Sub

    Private Sub Label1_MouseHover(sender As Object, e As EventArgs) Handles Label1.MouseHover, TextBox2.GotFocus, TextBox2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "The maximum mismatch between two overlapping sequences is not allowed to exceed this value."
        Else
            TextBox1.Text = "设定值表示在两条重叠序列之间最多允许的不匹配碱基比例。"
        End If
    End Sub

    Private Sub Label2_MouseHover(sender As Object, e As EventArgs) Handles Label2.MouseHover, NumericUpDown1.GotFocus, NumericUpDown1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Each cluster of overlapping sequences is required to have at least this number of sequences. Otherwise, the cluster will be removed."
        Else
            TextBox1.Text = "设定值表示保留一组具有重叠区域的序列所需要的序列数。如果序列数低于设定值，则会被删除。"
        End If
    End Sub

    Private Sub Label3_MouseHover(sender As Object, e As EventArgs) Handles Label3.MouseHover
        If language = "EN" Then
            TextBox1.Text = "The parameters for MAFFT are set to '-auto', while the parameters for MUSCLE are configured as '-align'."
        Else
            TextBox1.Text = "MAFFT的参数为-auto，muscle的参数为-align"
        End If
    End Sub

End Class