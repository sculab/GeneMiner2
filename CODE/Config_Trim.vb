Imports System.IO

Public Class Config_Trim
    Public Event ConfirmClicked()
    Public Event CancelClicked()
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ComboBox1.SelectedIndex = 1 Then
            For batch_i As Integer = 1 To seqsView.Count
                If form_main.DataGridView2.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    Dim folder_name As String = make_out_name(System.IO.Path.GetFileNameWithoutExtension(form_main.DataGridView2.Rows(batch_i - 1).Cells(2).Value.ToString), System.IO.Path.GetFileNameWithoutExtension(form_main.DataGridView2.Rows(batch_i - 1).Cells(3).Value.ToString))
                    folder_name = folder_name.Replace("-", "_").Replace(":", "_")
                    Dim temp_out_dir = (form_main.TextBox1.Text + "\" + batch_i.ToString + "_" + folder_name).Replace("\", "/")
                    If Directory.Exists(Path.Combine(temp_out_dir, "consensus")) = False Then
                        MsgBox("To use consensus results, you need to execute 'Generate Consensus' first.")
                        Exit Sub
                    End If
                End If
            Next
        End If

        Me.Hide()
        RaiseEvent ConfirmClicked()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        RaiseEvent CancelClicked()
    End Sub

    Private Sub ComboBox2_MouseHover(sender As Object, e As EventArgs) Handles ComboBox2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "All Fragments: This method involves combining all fragments that match the reference sequence to align the target sequence.
Longest Fragments: This approach uses only the longest matching fragments of the reference sequence to align the target sequence.
Trim Terminal: This method aligns the target sequence based solely on trimming the termini according to the reference sequence.
Longest Isoform: This method generates the longest transcript fragment based on the reference sequence. Suitable for transcriptomic data.

Longest Isoform uses Magic-BLAST to find splice junctions:
Boratyn G.M., Thierry-Mieg J., Thierry-Mieg D., Busby B., Madden TL. (2019) Magic-BLAST, an accurate RNA-seq aligner for long and short reads. BMC Bioinformatics 20: 405."
        Else
            TextBox1.Text = "All Fragments: 使用所有与参考序列匹配上的片段进行组合切齐。
Longest Fragments: 只使用与参考序列匹配最长的片段进行切齐。
Trim Terminal: 只使用参考序列切齐两端。
Longest Isoform: 切齐到与参考序列匹配的最长转录本片段，适用于转录组数据。

Longest Isoform 使用 Magic-BLAST 查找剪接位点:
Boratyn G.M., Thierry-Mieg J., Thierry-Mieg D., Busby B., Madden TL. (2019) Magic-BLAST, an accurate RNA-seq aligner for long and short reads. BMC Bioinformatics 20: 405."
        End If
    End Sub

    Private Sub Label1_MouseHover(sender As Object, e As EventArgs) Handles Label1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Results are kept if their length ratio to the reference sequence's median exceeds a set value; otherwise, they're removed. To prevent over-filtering, especially when sequencing and reference sources differ (e.g., genomic data vs. transcriptomic references), set this value to 0."
        Else
            TextBox1.Text = "如果结果序列的长度与参考序列的中值长度之比大于该值，则保留结果，反之则移除结果。如果测序数据和参考序列的来源不同，例如测序文件为基因组，参考序列来自转录组，请将该值设为 0，以免过度过滤。"
        End If

    End Sub

    Private Sub Label2_MouseHover(sender As Object, e As EventArgs) Handles Label2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Source of the sequences to be processed. Choose between using raw results or those that have undergone consistency processing."
        Else
            TextBox1.Text = "待处理的序列的来源。使用原始结果或者进行过一致性处理的结果。"
        End If
    End Sub
    Private Sub Config_Trim_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 0
        If ComboBox2.SelectedIndex < 0 Then
            ComboBox2.SelectedIndex = 2
        End If
        If language = "EN" Then
            TextBox1.Text = "Refine the results to align with the reference sequences. It is essential to modify parameters in accordance with the specific types of sequencing data and reference sequences utilized. The refined results will be stored in the 'blast' folder located in the output directory."
        Else
            TextBox1.Text = "基于参考序列对结果进行切齐，请根据测序数据和参考序列的类型调整参数。结果保存在输出目录的blast文件夹中。"
        End If
    End Sub

    Private Sub ComboBox1_MouseHover(sender As Object, e As EventArgs) Handles ComboBox1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Source of the sequences to be processed. Choose between using raw results or those that have undergone consistency processing."
        Else
            TextBox1.Text = "待处理的序列的来源。使用原始结果或者进行过一致性处理的结果。"
        End If
    End Sub
    Private Sub NumericUpDown1_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown1.GotFocus

    End Sub

    Private Sub NumericUpDown1_MouseEnter(sender As Object, e As EventArgs) Handles NumericUpDown1.MouseEnter
        If language = "EN" Then
            TextBox1.Text = "Results are kept if their length ratio to the reference sequence's median exceeds a set value; otherwise, they're removed. To prevent over-filtering, especially when sequencing and reference sources differ (e.g., genomic data vs. transcriptomic references), set this value to 0."
        Else
            TextBox1.Text = "如果结果序列的长度与参考序列的中值长度之比大于该值，则保留结果，反之则移除结果。如果测序数据和参考序列的来源不同，例如测序文件为基因组，参考序列来自转录组，请将该值设为 0，以免过度过滤。"
        End If
    End Sub
End Class