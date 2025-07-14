Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
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
    Public Sub clean_fasta_file(ByVal inputFilePath As String, ByVal outputFilePath As String)
        Try
            Using inputFile As New StreamReader(inputFilePath)
                Using outputFile As New StreamWriter(outputFilePath)
                    Dim line As String
                    While (Not inputFile.EndOfStream)
                        line = inputFile.ReadLine()
                        If line.StartsWith(">") Then
                            ' 替换非文件夹名称中允许的字符为下划线
                            line = ReplaceInvalidCharacters(line.Substring(1))
                            ' 写入到输出文件
                            outputFile.WriteLine(">" & line)
                        Else
                            ' 直接写入到输出文件
                            outputFile.WriteLine(line)
                        End If
                    End While
                End Using
            End Using
            Console.WriteLine("文件处理完成。")
        Catch ex As Exception
            Console.WriteLine("发生错误：" & ex.Message)
        End Try
    End Sub

    Private Function ReplaceInvalidCharacters(ByVal inputString As String) As String
        ' 定义不允许在文件夹名称中出现的字符
        Dim invalidChars As String = Path.GetInvalidFileNameChars()

        ' 遍历输入字符串，替换非法字符为下划线
        For Each invalidChar As Char In invalidChars
            inputString = inputString.Replace(invalidChar, "_")
        Next

        Return inputString
    End Function
    Public Sub safe_copy(ByVal source As String, ByVal target As String, Optional ByVal overwrite As Boolean = True)
        If File.Exists(source) Then
            Dim directoryPath As String = Path.GetDirectoryName(target)
            If Not Directory.Exists(directoryPath) Then
                ' 如果目录不存在，则创建目录树
                Directory.CreateDirectory(directoryPath)
            End If
            File.Copy(source, target, overwrite)
            Dim myFileInfo As FileInfo = New FileInfo(target) With {
                .IsReadOnly = False
            }
        End If
    End Sub

    Public Sub DeleteDir(ByVal aimPath As String)
        If (aimPath(aimPath.Length - 1) <> Path.DirectorySeparatorChar) Then
            aimPath += Path.DirectorySeparatorChar
        End If
        If (Not Directory.Exists(aimPath)) Then Exit Sub
        Try
            Directory.Delete(aimPath, True) ' 第二个参数设置为 True 表示递归删除目录及其内容
        Catch ex As Exception
            ' 处理删除失败的情况
        End Try
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
        Dim RegCHZN As New Regex("[\u4e00-\u9fa5]")
        Dim m As Match = RegCHZN.Match(root_path)
        If m.Success Then
            MsgBox("程序所在路径不得含有中文（亚洲语言字符）！" + Chr(13) + "The installation path should not contain Asian language characters.")
            End
        End If
        Dec_Sym = CInt("0").ToString("F1").Replace("0", "")
        If Dec_Sym <> "." Then
            MsgBox("Notice: We will use dot (.) as decimal notation instead of comma (,). We recommend to change your system's number format to English! ")
        End If
        Dim driveInfo As New DriveInfo(Path.GetPathRoot(root_path))

        ' 判断是否为可移动类型
        If driveInfo.DriveType = DriveType.Removable Then
            MsgBox("请勿在可移动磁盘上运行程序！" + Chr(13) + "Please do not run the program on a removable disk!")
            End
        End If
    End Sub
    Public Sub CombineFiles(ByVal FilePath As String, ByVal FilePath2add As String)
        Try
            Using firstFileWriter As New StreamWriter(FilePath, True) ' "True" appends to the file
                Using secondFileReader As New StreamReader(FilePath2add)
                    Dim line As String = ""
                    While (InlineAssignHelper(line, secondFileReader.ReadLine())) IsNot Nothing
                        firstFileWriter.WriteLine(line)
                    End While
                End Using
                'firstFileWriter.Write(vbCrLf)
            End Using
            Console.WriteLine("Files Combined Successfully.")
        Catch ex As Exception
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
    End Sub
    Public Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function
    Public Function refresh_file()
        Dim length_min As Integer = 1000000000.0
        Dim length_max As Integer = 0
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
                newrow(0) = refsView.Count
                newrow(1) = System.IO.Path.GetFileNameWithoutExtension(FileName)
                newrow(2) = ref_count
                newrow(3) = CInt(ref_length / ref_count)
                newrow(4) = ""
                newrow(5) = ""
                newrow(6) = ""
                newrow(7) = ""
                refsView.Item(refsView.Count - 1).Row.ItemArray = newrow
                If length_min > CInt(ref_length / ref_count) Then
                    length_min = CInt(ref_length / ref_count)
                End If
                If length_max < CInt(ref_length / ref_count) Then
                    length_max = CInt(ref_length / ref_count)
                End If
            End If
        Next
        refsView.AllowNew = False
        refsView.AllowEdit = True

        Return {length_min, length_max}
    End Function
    Public Function GetLineCount(filePath As String) As Integer
        Try
            Using reader As New StreamReader(filePath)
                Dim lineCount As Integer = 0
                While Not reader.EndOfStream
                    reader.ReadLine()
                    lineCount += 1
                End While
                Return lineCount
            End Using
        Catch ex As Exception
            MessageBox.Show($"无法打开文件: {ex.Message}")
            Return -1 ' 如果发生错误，返回-1表示失败
        End Try
    End Function

    Private ReadOnly client As HttpClient = New HttpClient()
    Private Async Function GetResponseTime(url As String) As Task(Of Long)
        Dim httpClient As New HttpClient()
        Dim stopwatch As New Stopwatch()

        Try
            stopwatch.Start()
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(url)
            ' 不需要做任何事情，只是获取响应时间
        Catch ex As HttpRequestException
            ' 处理异常
            MessageBox.Show($"无法获取 {url} 的响应时间：{ex.Message}")
        Finally
            stopwatch.Stop()
        End Try

        Return stopwatch.ElapsedMilliseconds
    End Function
    Public Async Function get_genome_data(ByVal database_type As String, ByVal file_type As String, ByVal gb_id As String) As Task(Of String)
        Dim data_folder As String = currentDirectory & "database\" & database_type & "_" & file_type & "\" & gb_id.Substring(0, Math.Min(2, gb_id.Length)) & "\" & gb_id.Substring(0, Math.Min(5, gb_id.Length)) & "\"
        Dim file_path As String = data_folder & gb_id & "." & file_type

        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 1 ' 设置最大重试次数为1
        Dim httpClient As New HttpClient With {
            .Timeout = TimeSpan.FromSeconds(10)
        }

        While retryCount <= maxRetries
            Try
                If Not File.Exists(file_path) Then
                    Dim source_file As String = database_url.Split(";")(0) & database_type & "_" & file_type & "/" & gb_id.Substring(0, Math.Min(2, gb_id.Length)) & "/" & gb_id.Substring(0, Math.Min(5, gb_id.Length)) & "/" & gb_id & "." & file_type
                    Directory.CreateDirectory(data_folder)

                    Dim response = Await httpClient.GetAsync(source_file)
                    response.EnsureSuccessStatusCode()

                    Dim content = Await response.Content.ReadAsByteArrayAsync()
                    File.WriteAllBytes(file_path, content)
                    form_main.RichTextBox1.AppendText("Get: " + gb_id + vbCrLf)
                Else
                    form_main.RichTextBox1.AppendText("Get: " + gb_id + vbCrLf)
                End If
                Return file_path
            Catch ex As Exception
                ' 如果发生异常，等待一段时间后重试
                form_main.RichTextBox1.AppendText("Failed: " + gb_id + vbCrLf)
                retryCount += 1
                If retryCount <= maxRetries Then
                    Thread.Sleep(TimeSpan.FromSeconds(1)) ' 等待5秒后重试
                Else
                    Return ""
                End If
            End Try
        End While
        Return ""
    End Function


    Public Async Function TestUrlsAsync(ByVal urls() As String) As Task(Of String)
        Dim fastestUrl As String = urls(0)
        Dim minResponseTime As Long = Long.MaxValue
        Dim httpClient As HttpClient = New HttpClient With {
            .Timeout = TimeSpan.FromSeconds(10) ' 设置超时时间为10秒
            }

        For Each url As String In urls
            Try
                Dim stopwatch As Stopwatch = Stopwatch.StartNew()

                ' 使用ConfigureAwait(False)来避免死锁
                Dim response As HttpResponseMessage = Await httpClient.GetAsync(url + "test.fasta").ConfigureAwait(False)
                If response.IsSuccessStatusCode Then
                    stopwatch.Stop()
                    Dim responseTime As Long = stopwatch.ElapsedMilliseconds

                    ' 检查是否是最快的URL
                    If responseTime < minResponseTime Then
                        minResponseTime = responseTime
                        fastestUrl = url
                    End If
                End If
            Catch ex As Exception
                ' 出错时忽略该URL
                Console.WriteLine("Error accessing URL: " & url & " - " & ex.Message)
            End Try
        Next

        Return fastestUrl
    End Function

    Public Sub SplitFastaFile(ref_file As String, outputDir As String)
        If Not Directory.Exists(outputDir) Then
            Directory.CreateDirectory(outputDir)
        End If

        Dim sequenceCounter As Integer = 1
        Dim currentSequence As New List(Of String)
        Dim sequenceStarted As Boolean = False

        For Each line As String In File.ReadLines(ref_file)
            If line.StartsWith(">") Then
                If sequenceStarted Then
                    SaveSequence(currentSequence, outputDir, sequenceCounter)
                    sequenceCounter += 1
                    currentSequence.Clear()
                End If
                sequenceStarted = True
            End If

            If sequenceStarted Then
                currentSequence.Add(line)
            End If
        Next

        ' Save the last sequence
        If sequenceStarted Then
            SaveSequence(currentSequence, outputDir, sequenceCounter)
        End If
    End Sub

    Private Sub SaveSequence(sequence As List(Of String), outputDir As String, sequenceNumber As Integer)
        Dim filePath As String = Path.Combine(outputDir, $"ref_{sequenceNumber}.fasta")
        File.WriteAllLines(filePath, sequence)
    End Sub

    Public Function GetMaxValueRecord(inputPath As String) As String
        Dim maxRecord As String = String.Empty
        Dim maxValue As Integer = Integer.MinValue

        For Each line As String In File.ReadLines(inputPath)
            Dim parts As String() = line.Split(","c)
            If parts.Length >= 2 Then
                Dim value As Integer
                If Integer.TryParse(parts(1), value) Then
                    If value > maxValue Then
                        maxValue = value
                        maxRecord = parts(0)
                    End If
                End If
            End If
        Next
        Return maxRecord
    End Function

    Function GenerateRandomString(ByVal length As Integer) As String
        Dim allowedChars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
        Dim randomBytes(length - 1) As Byte
        Dim rng As RandomNumberGenerator = RandomNumberGenerator.Create()
        rng.GetBytes(randomBytes)
        Dim result As New StringBuilder(length)

        For Each randomByte In randomBytes
            Dim index As Integer = randomByte Mod allowedChars.Length
            result.Append(allowedChars(index))
        Next

        Return result.ToString()
    End Function
    Public Sub do_PGA(ByVal gb_file As String, ByVal taregt_file As String, ByVal output_dir As String)
        Dim random_folder As String = GenerateRandomString(8)
        Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", random_folder, "target"))
        Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", random_folder, "reference"))
        Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", random_folder, "result"))
        safe_copy(gb_file, Path.Combine(currentDirectory, "temp", random_folder, "reference", "my_ref.gb"))
        safe_copy(taregt_file, Path.Combine(currentDirectory, "temp", random_folder, "target", "my_target.fasta"))

        Dim SI_PGA As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\PGA.exe",
            .WorkingDirectory = currentDirectory + "analysis",
            .CreateNoWindow = False,
            .Arguments = "-r ..\temp\" + random_folder + "\reference -t ..\temp\" + random_folder + "\target -o  ..\temp\" + random_folder + "\result"
        }
        Dim process_PGA As Process = New Process With {
            .StartInfo = SI_PGA
        }
        process_PGA.Start()
        process_PGA.WaitForExit()
        process_PGA.Close()
        safe_copy(Path.Combine(currentDirectory, "temp", random_folder, "result", "my_target.gb"), Path.Combine(output_dir, "output.gb"))
        safe_copy(Path.Combine(currentDirectory, "temp", random_folder, "result", "warning.log"), Path.Combine(output_dir, "warning.log"))
        safe_copy(Path.Combine(currentDirectory, "temp", random_folder, "target", "my_target.fasta"), Path.Combine(output_dir, "output.fasta"))
        Directory.Delete(Path.Combine(currentDirectory, "temp", random_folder), True)
    End Sub

    Public Function make_ref_dict(ByVal wd As String, ByVal rd As String, ByVal output_dir As String, ByVal lkd As String) As Double
        Dim SI_filter As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\MainFilterNew.exe",
            .WorkingDirectory = wd,
            .CreateNoWindow = False,
            .Arguments = "-r " + """" + rd + """"
        }
        SI_filter.Arguments += " -o " + """" + output_dir + """"
        SI_filter.Arguments += " -kf " + k1.ToString
        SI_filter.Arguments += " -s " + form_config_basic.NumericUpDown2.Value.ToString
        SI_filter.Arguments += If(form_config_basic.CheckBox2.Checked, " -gr", "")
        SI_filter.Arguments += " -lkd " + """" + output_dir + "\" + lkd + """"
        SI_filter.Arguments += " -m 2"
        Dim process_filter As Process = Process.Start(SI_filter)

        ' 用于存储观察到的最大内存使用量
        Dim peakMemoryUsage As Double
        If TargetOS = "macos" Then
            process_filter.WaitForExit()
            peakMemoryUsage = 8
        Else
            Dim peakMemoryBytes As Long = 512 * 1024 * 1024
            ' 循环检查进程是否仍在运行，并更新峰值内存使用量
            While Not process_filter.HasExited
                ' 读取当前的工作集大小，并更新峰值内存使用量
                peakMemoryBytes = Math.Max(peakMemoryBytes, process_filter.PeakWorkingSet64)
                Threading.Thread.Sleep(500)
            End While
            peakMemoryUsage = Math.Ceiling(CDbl(peakMemoryBytes) / (1024.0 * 1024.0 * 1024.0))
        End If

        process_filter.Close()
        Return peakMemoryUsage
    End Function

    Public Function GetReadLength(filePath As String) As Integer
        Try
            Using reader As New StreamReader(filePath)
                Dim line As String
                Dim maxLength As Integer = 0
                Dim lineNumber As Integer = 0

                While Not reader.EndOfStream And lineNumber < 400
                    line = reader.ReadLine()
                    lineNumber += 1

                    ' Check if the line number modulo 4 equals 2
                    If lineNumber Mod 4 = 2 Then
                        ' Update maxLength if this line is longer
                        maxLength = Math.Max(maxLength, line.Length)
                    End If
                End While

                Return maxLength
            End Using
        Catch ex As IOException
            Console.WriteLine("IO错误: " & ex.Message)
        Catch ex As Exception
            Console.WriteLine("一般错误: " & ex.Message)
        End Try
        Return 0 ' Return 0 if an error occurs or the conditions are not met
    End Function
    Public Function safe_delete(ByVal file_path As String)
        Try
            If File.Exists(file_path) Then
                File.Delete(file_path)
            End If
            Return 0
        Catch ex As Exception
            Return 1
        End Try

    End Function
    Public Sub do_newick(ByVal args() As String)
        Dim SI_newick As New ProcessStartInfo With {
            .FileName = currentDirectory + "analysis\newick.exe",
            .WorkingDirectory = args(0), ' 替换为实际的运行文件夹
            .CreateNoWindow = False,
            .Arguments = args(1)
        }
        Dim process_filter As Process = Process.Start(SI_newick)
        process_filter.WaitForExit()
        process_filter.Close()
    End Sub

    Public Function make_out_name(ByVal fq_1 As String, ByVal fq_2 As String)
        Dim out_name As String = ""
        For i As Integer = 1 To fq_1.Length
            If fq_1.Substring(i - 1, 1) = fq_2.Substring(i - 1, 1) Then
                out_name += fq_1.Substring(i - 1, 1)
            End If
        Next
        out_name = out_name.Replace(".fasta", "").Replace(".fastq", "").Replace(".fq", "").Replace(".gz", "").Replace(".fas", "").Replace("  ", " ").Replace(".", "_").Replace(" ", "_").Replace("-", "_")
        Do While out_name.EndsWith("_")
            out_name = out_name.Substring(0, out_name.Length - 1)
        Loop
        Return out_name
    End Function
End Module
