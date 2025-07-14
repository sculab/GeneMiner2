Public Class Config_Fossil
    Public Event ConfirmClicked()
    Public Event CancelClicked()
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox1.Text <> "" And TextBox2.Text <> "" Then
            If IsNumeric(TextBox1.Text) And IsNumeric(TextBox2.Text) Then
                If CSng(TextBox1.Text) >= CSng(TextBox2.Text) Then
                    time_view.Item(Selected_node).Item(2) = "B(" + TextBox2.Text + "," + TextBox1.Text + "," + ComboBox2.Text + "," + ComboBox1.Text + ")"
                    Me.Hide()
                    RaiseEvent ConfirmClicked()
                Else
                    MsgBox("Please enter correct number!")
                End If
            Else
                MsgBox("Please enter correct number!")
            End If
        ElseIf TextBox1.Text = "" And TextBox2.Text = "" Then
            time_view.Item(Selected_node).Item(2) = ""
            Me.Hide()
            RaiseEvent ConfirmClicked()
        ElseIf TextBox1.Text <> "" Then
            time_view.Item(Selected_node).Item(2) = "U(" + TextBox1.Text + "," + ComboBox1.Text + ")"
            Me.Hide()
            RaiseEvent ConfirmClicked()
        ElseIf TextBox2.Text <> "" Then
            time_view.Item(Selected_node).Item(2) = "L(" + TextBox2.Text + ",0.1,0.5," + ComboBox2.Text + ")"
            Me.Hide()
            RaiseEvent ConfirmClicked()
        Else

            MsgBox("Please enter correct number!")
        End If


    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        RaiseEvent CancelClicked()
    End Sub

    Private Sub Config_Fossil_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 0
        ComboBox2.SelectedIndex = 0
        If language = "EN" Then
            TextBox3.Text = "Set soft boundary calibration for fossil settings. By default, there is a 0.025 probability that it is lower than the lower limit of the time node, a 0.025 probability that it is higher than the upper limit of the time node, and a 0.95 probability that it is between the lower and upper limits. To set hard boundaries, the boundary probability can be adjusted to 1e-300, but do not set it to zero or negative values."
        Else
            TextBox3.Text = "设值化石的设置软边界校准。默认以0.025的概率小于时间节点下限, 0.025的概率大于时间节点上限，以0.95的概率在下限到上限之间。如果要设置硬边界，可以将边界概率调整为1e-300，不要设为0或负值。"
        End If
    End Sub

    Private Sub Config_Fossil_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then
            If Selected_node >= 0 Then
                Dim temp_date = time_view.Item(Selected_node).Item(2).ToString
                temp_date = temp_date.Replace("(", "").Replace(")", "")
                If temp_date <> "" Then
                    Select Case temp_date.Substring(0, 1)
                        Case "B"
                            temp_date = temp_date.Substring(1)
                            TextBox1.Text = temp_date.Split(",")(1)
                            TextBox2.Text = temp_date.Split(",")(0)
                            ComboBox1.Text = temp_date.Split(",")(3)
                            ComboBox2.Text = temp_date.Split(",")(2)
                        Case "L"
                            temp_date = temp_date.Substring(1)
                            TextBox2.Text = temp_date.Split(",")(0)
                            TextBox1.Text = ""
                            ComboBox2.Text = temp_date.Split(",")(3)
                            ComboBox1.Text = ""
                        Case "U"
                            temp_date = temp_date.Substring(1)
                            TextBox1.Text = temp_date.Split(",")(0)
                            TextBox2.Text = ""
                            ComboBox1.Text = temp_date.Split(",")(1)
                            ComboBox2.Text = ""
                        Case Else
                            TextBox1.Text = ""
                            TextBox2.Text = ""
                            ComboBox1.Text = "0.025"
                            ComboBox2.Text = "0.025"
                    End Select

                Else
                    TextBox1.Text = ""
                    TextBox2.Text = ""
                    ComboBox1.Text = "0.025"
                    ComboBox2.Text = "0.025"
                End If
            End If
        End If

    End Sub
End Class