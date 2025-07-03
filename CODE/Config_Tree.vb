Imports System.IO

Public Class Config_Tree
    Public Event ConfirmClicked()
    Public Event CancelClicked()
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        DataGridView1.EndEdit()
        DataGridView1.Refresh()
        If CheckBox1.Checked Then
            Dim og_count As Integer = 0
            Select Case MenuClicked
                Case "build_tree_refs"
                    Using ogwriter As StreamWriter = New StreamWriter(Path.Combine(form_main.TextBox1.Text, "og.txt"))
                        If TextBox2.Text <> "" Then
                            For batch_i As Integer = 1 To TextBox2.Text.Split(Chr(13)).Length
                                og_count += 1
                                ogwriter.WriteLine(TextBox2.Text.Split(Chr(13))(batch_i - 1))
                            Next
                        End If
                    End Using
                Case "build_tree"
                    If taxonView.Count > 0 Then
                        Using ogwriter As StreamWriter = New StreamWriter(Path.Combine(form_main.TextBox1.Text, "og.txt"))
                            For batch_i As Integer = 1 To seqsView.Count
                                If DataGridView1.Rows(batch_i - 1).Cells(0).FormattedValue.ToString = "True" Then
                                    og_count += 1
                                    ogwriter.WriteLine(DataGridView1.Rows(batch_i - 1).Cells(1).Value.ToString)
                                End If
                            Next
                        End Using
                    End If
            End Select
            If og_count = 0 Then
                MsgBox("You must designate outgroup for rooted tree.")
                Exit Sub
            End If
        End If
        Me.Hide()
        RaiseEvent ConfirmClicked()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        RaiseEvent CancelClicked()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        DataGridView1.ReadOnly = CheckBox1.Checked Xor True
        Textbox2.ReadOnly = CheckBox1.Checked Xor True
        DataGridView1.Enabled = CheckBox1.Checked
        TextBox2.Enabled = CheckBox1.Checked
    End Sub

    Private Sub Config_Tree_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 0
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged

    End Sub

    Public Function SelectedTreeProgram() As String
        Select Case ComboBox1.SelectedIndex
            Case 0
                Return "fasttree"
            Case 1
                Return "iqtree"
            Case Else
                Return "fasttree"
        End Select
    End Function
End Class