Public Class Config_Calculate
    Dim max_diff As Double, gene_len As Integer, read_depth As Double, read_len As Integer
    Public kf As Integer = 0
    Dim error_limit As Integer, search_depth As Integer, soft_boundary As String, step_size As Integer
    Dim combine_thr As Double, trim_mode As String, trim_thr As Integer

    Private Sub Button_Apply_Click(sender As Object, e As EventArgs) Handles Button_Apply.Click
        If kf = 0 Then
            MsgBox("Please calculate the parameters first.", MsgBoxStyle.Information, "Information")
            Return
        End If

        form_config_basic.NumericUpDown1.Value = kf
        form_config_basic.NumericUpDown2.Value = step_size
        form_config_basic.NumericUpDown8.Value = error_limit
        form_config_basic.NumericUpDown10.Value = search_depth
        form_config_basic.ComboBox1.SelectedIndex = form_config_basic.ComboBox1.Items.IndexOf(soft_boundary)
        form_config_combine.TextBox2.Text = Math.Round(combine_thr, 3).ToString
        form_config_trim.NumericUpDown1.Value = trim_thr
        form_config_trim.ComboBox2.SelectedIndex = form_config_trim.ComboBox2.Items.IndexOf(trim_mode)
        Hide()
    End Sub

    Private Sub Button_Calculate_Click(sender As Object, e As EventArgs) Handles Button_Calculate.Click
        kf = 0
        TextBox_Kf.Clear()
        TextBox_StepSize.Clear()
        TextBox_ErrorLimit.Clear()
        TextBox_SearchDepth.Clear()
        TextBox_Boundary.Clear()
        TextBox_CombineThr.Clear()
        TextBox_TrimThr.Clear()
        TextBox_TrimMode.Clear()

        If (Not Double.TryParse(TextBox_Diff.Text, max_diff)) OrElse (max_diff < 0 Or max_diff > 0.3) Then
            MsgBox("Max difference must be between 0 and 0.3.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        End If

        If (Not Double.TryParse(TextBox_Depth.Text, read_depth)) OrElse read_depth < 1 Then
            MsgBox("Read depth must be higher than 1.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        End If

        If (Not Integer.TryParse(TextBox_ReadLen.Text, read_len)) OrElse (read_len < 40 Or read_len > 1000) Then
            MsgBox("Read length must be between 40 and 1000.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        End If

        If (Not Integer.TryParse(TextBox_RefLen.Text, gene_len)) OrElse gene_len < read_len Then
            MsgBox("Gene length must be larger than read length.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        End If

        If ComboBox_Ref.SelectedIndex < 0 Then
            MsgBox("Please select a reference type.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        End If

        If ComboBox_Seq.SelectedIndex < 0 Then
            MsgBox("Please select a sample type.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        End If

        combine_thr = max_diff

        If ComboBox_Seq.SelectedIndex = 0 Then
            If ComboBox_Ref.SelectedIndex = 0 Then
                trim_mode = "Trim Terminal"
            Else
                trim_mode = "All Fragments"
            End If
        Else
            trim_mode = "Longest Isoform"
        End If

        If ComboBox_Seq.SelectedIndex = 1 Then
            trim_thr = 0
        ElseIf ComboBox_Ref.SelectedIndex = 1 Then
            ' Fitting a poisson distribution is too much work here
            ' Just use a discount factor on the read depth and call it a day
            trim_thr = If(read_depth <= 15, 0, CInt(Math.Floor(Math.Min(read_depth * 2 / 3, 25) / 5) * 5))
        Else
            trim_thr = If(read_depth <= 15, 0, CInt(Math.Floor(Math.Min(read_depth, 50) / 5) * 5))
        End If

        For k As Integer = 39 To 17 Step -1
            If 1.0 - Math.Pow(1.0 - Math.Pow(1.0 - max_diff, k), read_len - k + 1) > 0.985 Then
                kf = k
                Exit For
            End If
        Next

        If kf = 0 Then
            For k As Integer = 39 To 17 Step -1
                If 1.0 - Math.Pow(1.0 - Math.Pow(1.0 - max_diff, k), read_len - k + 1) > 0.945 Then
                    kf = k
                    Exit For
                End If
            Next
        End If

        If kf = 0 Then
            MsgBox("Unable to find a k-mer size because sequence variation is too large.", MsgBoxStyle.Exclamation, "Invalid value")
            Return
        ElseIf kf >= 35 Then
            step_size = kf - 32 + 1
            kf = 32
        ElseIf kf >= 31 Then
            step_size = kf - 31 + 1
            kf = 31
        Else
            step_size = 1
        End If

        If read_depth > 45 Then
            error_limit = 3
            soft_boundary = "Auto"
        ElseIf read_depth > 15 Then
            error_limit = 2
            soft_boundary = "Auto"
        Else
            error_limit = 1
            soft_boundary = "Unlimited"

            If read_depth <= 5 Then
                kf -= 2 * (5 - read_depth)
                kf = Math.Max(kf, 17)
            End If
        End If

        If gene_len >= 6000 Then
            search_depth = 9999
        ElseIf gene_len >= 4800 Then
            search_depth = 8192
        Else
            search_depth = 4096
        End If

        TextBox_Kf.Text = kf.ToString
        TextBox_StepSize.Text = step_size.ToString
        TextBox_ErrorLimit.Text = error_limit.ToString
        TextBox_SearchDepth.Text = search_depth.ToString
        TextBox_Boundary.Text = soft_boundary
        TextBox_CombineThr.Text = Math.Round(combine_thr, 3).ToString
        TextBox_TrimThr.Text = trim_thr.ToString
        TextBox_TrimMode.Text = trim_mode

        If gene_len >= 10000 Then
            MsgBox("Please note that assembling genes beyond 10kbp might be inefficient.", MsgBoxStyle.Information, "Information")
        End If

        If read_depth >= 200 Then
            MsgBox("Please consider limiting reads per file because read depth is too high.", MsgBoxStyle.Information, "Information")
        End If
    End Sub

    Private Sub Button_Close_Click(sender As Object, e As EventArgs) Handles Button_Close.Click
        Hide()
    End Sub

End Class