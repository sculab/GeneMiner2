Imports System.IO
Imports System.Text.Json
Public Class Config_AGS
    Public combinedArray As New List(Of String)()
    Public select_class As Boolean = False
    Private Sub Config_AGS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 读取JSON文件内容
        Dim jsonText As String = File.ReadAllText(currentDirectory + "\analysis\classification.json")

        ' 解析JSON
        Dim jsonDocument As JsonDocument = JsonDocument.Parse(jsonText)

        ' 获取根元素
        Dim root As JsonElement = jsonDocument.RootElement

        ' 获取"Order"，"Family"和"Genus"字段内容
        Dim orderArray As JsonElement = root.GetProperty("Order")
        Dim familyArray As JsonElement = root.GetProperty("Family")
        Dim genusArray As JsonElement = root.GetProperty("Genus")

        ' 将内容存储到同一个数组中


        If orderArray.ValueKind = JsonValueKind.Array Then
            For Each item As JsonElement In orderArray.EnumerateArray()
                combinedArray.Add(item.GetString())
            Next
        End If

        If familyArray.ValueKind = JsonValueKind.Array Then
            For Each item As JsonElement In familyArray.EnumerateArray()
                combinedArray.Add(item.GetString())
            Next
        End If

        If genusArray.ValueKind = JsonValueKind.Array Then
            For Each item As JsonElement In genusArray.EnumerateArray()
                combinedArray.Add(item.GetString())
            Next
        End If


    End Sub


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
            For Each item As String In combinedArray
                If item.ToLower().StartsWith(userInput) Then
                    ListBox1.Items.Add(item)
                End If
            Next

        End If
    End Sub

    'Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
    '    select_class = True
    '    TextBox1.Text = ListBox1.Text
    '    select_class = False
    'End Sub

    'Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs)
    '    ' 获取用户输入的文本
    '    Dim userInput As String = TextBox2.Text.ToLower
    '    If userInput.Length > 1 And select_class = False Then
    '        ' 清空ComboBox的选项
    '        ListBox2.Items.Clear()

    '        ' 根据用户输入刷新ComboBox的选项
    '        For Each item As String In combinedArray
    '            If item.ToLower().StartsWith(userInput) Then
    '                ListBox2.Items.Add(item)
    '            End If
    '        Next

    '    End If
    'End Sub

    'Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox2.SelectedIndexChanged
    '    select_class = True
    '    TextBox2.Text = ListBox2.Text
    '    select_class = False
    'End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ListBox3.Items.Count >= 1 Then
            waiting = True
            timer_id = 1
            mydata_Dataset.Tables("Refs Table").Clear()
            form_main.DataGridView1.DataSource = Nothing
            data_loaded = False
            refs_type = "353"
            DeleteDir(currentDirectory + "temp\ags_seq")
            Dim th1 As New Threading.Thread(AddressOf But1)
            th1.Start()
            Me.Hide()
        Else
            MsgBox("At least one organism must be selected!")
        End If

    End Sub
    Public Sub But1()
        DeleteDir(currentDirectory + "temp\ags_seq\353gene")
        Dim include_txt As String = ""
        For Each i As String In ListBox3.Items
            include_txt += " " + i
        Next


        Dim SI_build_database As New ProcessStartInfo()
        SI_build_database.FileName = currentDirectory + "analysis\build_database.exe" ' 替换为实际的命令行程序路径
        SI_build_database.WorkingDirectory = currentDirectory + "analysis\" ' 替换为实际的运行文件夹路径
        SI_build_database.CreateNoWindow = False
        SI_build_database.Arguments = "-i " + """" + currentDirectory + "temp\ags_seq" + """"
        SI_build_database.Arguments += " -o " + """" + currentDirectory + "temp\ags_seq" + """"
        SI_build_database.Arguments += " -c " + include_txt
        SI_build_database.Arguments += " -t 4"
        'SI_build_database.Arguments += " -exclude " + exclude_txt
        Dim process_build_gb As Process = Process.Start(SI_build_database)
        process_build_gb.WaitForExit()
        process_build_gb.Close()
        If Directory.Exists(currentDirectory + "temp\ags_seq\353gene") Then
            DeleteDir(root_path + "temp\org_seq")
            My.Computer.FileSystem.CreateDirectory(root_path + "temp\org_seq")
            For Each FileName As String In Directory.GetFiles(currentDirectory + "temp\ags_seq\353gene", "*.fasta")
                File.Copy(FileName, root_path + "temp\org_seq\" + System.IO.Path.GetFileNameWithoutExtension(FileName) + ".fasta", True)
            Next
            refresh_file()
        Else
            MsgBox("Faild to download references!")
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex >= 0 Then
            Dim selectedItem As Object = ListBox1.SelectedItem
            If Not ListBox3.Items.Contains(selectedItem) Then
                ListBox3.Items.Add(selectedItem)
            End If
        End If
    End Sub


    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If ListBox3.SelectedIndex >= 0 Then
            ListBox3.Items.RemoveAt(ListBox3.SelectedIndex)
        End If

    End Sub

End Class