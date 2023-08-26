Imports System.IO
Module Module_Function
	Public Function ReadSettings(filePath As String) As Dictionary(Of String, String)
		Dim settings As New Dictionary(Of String, String)()

		If File.Exists(filePath) Then
			Dim lines As String() = File.ReadAllLines(filePath)

			For Each line As String In lines
				Dim parts As String() = line.Split("="c)
				If parts.Length = 2 Then
					Dim key As String = parts(0).Trim()
					Dim value As String = parts(1).Trim()
					settings(key) = value
				End If
			Next
		End If

		Return settings
	End Function

	' 保存设置到文件
	Public Sub SaveSettings(filePath As String, settings As Dictionary(Of String, String))
		Dim lines As New List(Of String)()

		For Each kvp As KeyValuePair(Of String, String) In settings
			Dim line As String = $"{kvp.Key}={kvp.Value}"
			lines.Add(line)
		Next

		File.WriteAllLines(filePath, lines)
	End Sub

	Public Function new_line(ByVal is_LF As Boolean) As String
		If is_LF Then
			Return vbLf
		Else
			Return vbCrLf
		End If
	End Function

	Public Function Check_Mixed_AA(ByVal seq As String) As Boolean
		Dim mixed_AA() As String = {"R", "Y", "M", "K", "S", "W", "H", "B", "V", "D", "N"}
		For Each i As String In mixed_AA
			If seq.ToUpper.Contains(i) Then
				Return False
			End If
		Next
		Return True
	End Function
	Public Sub safe_copy(source, target)
		If File.Exists(source) Then
			File.Copy(source, target, True)
			File.Delete(source)
		End If
	End Sub
	Public Sub DeleteDir(ByVal aimPath As String)
		If (aimPath(aimPath.Length - 1) <> Path.DirectorySeparatorChar) Then
			aimPath += Path.DirectorySeparatorChar
		End If  '判断待删除的目录是否存在,不存在则退出.  
		If (Not Directory.Exists(aimPath)) Then Exit Sub ' 
		Dim fileList() As String = Directory.GetFileSystemEntries(aimPath)  ' 遍历所有的文件和目录  
		For Each FileName As String In fileList
			If (Directory.Exists(FileName)) Then  ' 先当作目录处理如果存在这个目录就递归
				DeleteDir(aimPath + Path.GetFileName(FileName))
			Else  ' 否则直接Delete文件  
				Try
					File.Delete(aimPath + Path.GetFileName(FileName))
				Catch ex As Exception
				End Try
			End If
		Next  '删除文件夹  
	End Sub
	Public Sub format_path()
		Select Case TargetOS
			Case "linux"
				path_char = "/"
			Case "win32", "macos"
				path_char = "\"
			Case Else
				path_char = "\"
		End Select
		root_path = (Application.StartupPath + path_char).Replace(path_char + path_char, path_char)
		Dec_Sym = CInt("0").ToString("F1").Replace("0", "")
		If Dec_Sym <> "." Then
			MsgBox("Notice: We will use dat (.) as decimal quotation instead of comma (,). We recommand to change your system's number format to English! ")
		End If
	End Sub

	Public Sub MergeFiles(ByVal firstFilePath As String, ByVal secondFilePath As String)
		Try
			Using firstFileWriter As New StreamWriter(firstFilePath, True) ' "True" appends to the file
				Using secondFileReader As New StreamReader(secondFilePath)
					Dim line As String = ""
					While (InlineAssignHelper(line, secondFileReader.ReadLine())) IsNot Nothing
						firstFileWriter.WriteLine(line)
					End While
				End Using
			End Using
			Console.WriteLine("Files merged successfully.")
		Catch ex As Exception
			Console.WriteLine("An error occurred: " & ex.Message)
		End Try
	End Sub

	Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
		target = value
		Return value
	End Function


	Public Sub refresh_file()
		Dim newrow(7) As String
		Dim seq_count As Integer = 0
		refsView.AllowNew = True
		Dim fileList() As String = Directory.GetFileSystemEntries(root_path + "temp\org_seq")
		For Each FileName As String In fileList
			If FileName.EndsWith(".fasta") Then
				seq_count += 1
				refsView.AddNew()
				Dim ref_count As Integer = 0
				Dim ref_length As Integer = 0
				Dim sr As New StreamReader(FileName)
				Dim line As String = sr.ReadLine
				Do
					If line.StartsWith(">") Then
						ref_count += 1
					Else
						ref_length += line.Length
					End If
					line = sr.ReadLine
				Loop Until line Is Nothing
				sr.Close()
				newrow(0) = seq_count
				newrow(1) = System.IO.Path.GetFileNameWithoutExtension(FileName)
				newrow(2) = ref_count
				newrow(3) = CInt(ref_length / ref_count)
				newrow(4) = ""
				newrow(5) = ""
				newrow(6) = ""
				newrow(7) = ""
				refsView.Item(seq_count - 1).Row.ItemArray = newrow
			End If
		Next
		refsView.AllowNew = False
		refsView.AllowEdit = True
		timer_id = 2
	End Sub
End Module
