Imports System.IO

Module Module_Tree
    Public ShowScale As Boolean = False
    Public StartTreeView As Boolean = False
    Public TransparentBG As Boolean = False
    Public area_lower As Integer = 5
    Public keep_at_least As Integer = 1
    Public Show_area_names As Boolean = False
    Public Show_area_pies As Boolean = True
    Public pie_radii As Single = 16
    Public Display_taxon_names As Boolean = True
    Public Display_Null_distribution As Boolean = True
    Public Display_taxon_dis As Boolean = False
    Public Display_taxon_pie As Boolean = False
    Public Display_circle As Boolean = False
    Public Circle_size As Integer = 8
    Public Circle_color As System.Drawing.Color = Color.White
    Public Tree_font As System.Drawing.Font
    Public Label_font As System.Drawing.Font
    Public ID_font As System.Drawing.Font
    Public ID_color As System.Drawing.Color = Color.DarkBlue
    Public Display_node_frequency As Boolean = True
    Public Low_frequency As Single = 0.01
    Public Hide_pie As Single = 1
    Public Display_lines As Boolean = True
    Public frequency_h As Integer = 4
    Public frequency_v As Integer = 4
    Public Display_node_ID As Boolean = True
    Public node_h As Integer = -8
    Public node_v As Integer = -8
    Public Taxon_separation As Integer = 24
    Public Branch_length As Integer = 32
    Public Border_separation As Integer = 1
    Public Line_width As Integer = 1
    Public File_zoom As Integer = 4
    Public taxon_pie_radii As Integer = 2
    Public dorefresh As Boolean = False

    Public time_view As New DataView
    Public dtView As New DataView
    Public tree_filename As String
    Public taxon_num As Integer
    Public final_tree As String = ""
    Public error_no As Integer
    Public mrbayes_tree As Boolean = False

    Public tree_mcmctree As String
    Public ToplogyOnly As Boolean = True
    Public Selected_node As Integer = -1
    Public tree_view_title As String = ""
    Public tree_show_with_value As String = ""
    Public RangeStr As String = ""

    Public Sub ConvertFastaToPhylip(ByVal fastaFile As String, ByVal phylipFile As String)
        Dim sequences As New Dictionary(Of String, String)
        Dim currentHeader As String = String.Empty
        Dim maxLength As Integer = 0

        For Each line As String In File.ReadAllLines(fastaFile)
            If line.StartsWith(">") Then
                currentHeader = line.Substring(1).Trim()
                sequences(currentHeader) = String.Empty
            Else
                Dim sequenceLine = line.Trim()
                sequences(currentHeader) &= sequenceLine
                maxLength = Math.Max(maxLength, sequenceLine.Length)
            End If
        Next
        Using writer As New StreamWriter(phylipFile)
            writer.WriteLine(sequences.Count & " " & maxLength)
            For Each pair In sequences
                writer.WriteLine(pair.Key & "  " & pair.Value)
            Next
        End Using
    End Sub

End Module
