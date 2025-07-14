
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Math
Imports System.Threading

Public Class Config_Dated

    Public show_my_tree As String = ""
    Dim data_type As Integer = 0
    Dim current_state_mode = 0
    Dim Poly_Node(,) As String
    Dim Node_Relationship(,) As String
    Dim Poly_Node_row(,) As Single
    Dim Poly_terminal_xy(,) As Single
    Dim Poly_Node_col(,) As Single
    Dim Distribution() As String
    Dim TaxonName() As String
    Dim TaxonTime(,) As String
    Dim Tree_Export_Char() As String
    Dim NumofNode As Integer
    Dim NumofTaxon As Integer
    Dim RangeLength As Integer
    Dim max_level As Integer
    Dim max_taxon_name As Integer
    Dim begin_draw As Boolean = False
    Dim draw_result As Boolean = False
    '结果列表
    Dim Global_Text As String = ""
    Dim Result_list() As String
    Dim result_ID As Integer = -1
    Dim selected_nodes(0) As Integer

    Dim D_nodes(0) As Integer
    Dim E_nodes(0) As Integer
    Dim G_nodes(0) As Integer
    Dim R_nodes(0) As Integer
    Dim V_nodes(0) As Integer
    '区域饼图
    Dim Current_AreaS(,) As String
    Dim Current_AreaP(,) As Single
    Dim max_dis_value As Single = -1 / 0
    Dim min_dis_value As Single = 1 / 0
    '色卡
    Dim Color_S() As String
    Dim Color_B() As Brush

    Dim Color_S_node() As String
    Dim Color_B_node() As Brush
    '圆参数
    Dim pie_step As Single = 0.01
    Dim taxon_array() As String
    Dim has_length As Boolean = False
    '平滑度
    Dim smooth_x As Integer = 1

    Public tree_path As String

    Dim time_Dataset As New DataSet
    Dim Loading As Boolean = True
    Dim TimeView As Integer = 0
    Public current_state As Integer

    Public file_names() As String
    Public tree_filename As String
    Dim files_complete As Boolean = True

    Dim outgroup As Boolean = True
    Dim B_Tree_File As String = ""
    Dim P_Tree_File As String = ""
    Dim final_tree_file As String = ""
    Dim Write_Tree_Info As Boolean = False
    Public Taxon_Dataset As New DataSet
    Dim Node_Dataset As New DataSet
    Dim Tree_Num_B As Integer = 0
    Dim Tree_Num_P As Integer = 0
    Dim sdec_id As Integer = 0
    Dim sdec_count As Integer = 0
    Dim taxon_line As String
    Dim num_r1() As Single
    Dim num_r2() As Single
    Dim node(,) As String
    Dim clade_p(,) As Single
    Dim tree_char() As String



    Public Function Swap_tree(ByVal Treeline As String, ByVal gotnode As Integer) As String
        Dim tree_char() As String
        ReDim tree_char(NumofTaxon * 7)
        Dim char_id As Integer = 0
        Dim l_c As Integer = 0
        Dim r_c As Integer = 0
        Dim tx As Integer = 0
        Dim last_symb As Boolean = True
        For Each i As Char In Treeline
            Select Case i
                Case "("
                    char_id += 1
                    tree_char(char_id) = i
                    last_symb = True

                Case ")"
                    char_id += 1
                    tree_char(char_id) = i
                    last_symb = True
                Case ","
                    char_id += 1
                    tree_char(char_id) = i
                    last_symb = True
                Case Else
                    If last_symb Then
                        char_id += 1
                        tree_char(char_id) = i
                        last_symb = False
                    Else
                        tree_char(char_id) += i
                    End If
            End Select
        Next
        Dim start_index, end_index As Integer

        For i As Integer = 1 To char_id
            If tree_char(i) = ")" And gotnode <> 0 Then
                gotnode -= 1
                If gotnode = 0 Then
                    end_index = i
                End If
            End If
        Next
        gotnode = Selected_node + 1
        l_c = 0
        r_c = 0
        For i As Integer = 1 To end_index
            Select Case tree_char(end_index - i + 1)
                Case "("
                    l_c += 1
                Case ")"
                    r_c += 1
                Case Else
            End Select
            If l_c = r_c Then
                start_index = end_index - i + 1
                Exit For
            End If
        Next
        l_c = 0
        r_c = 0
        For i As Integer = 1 To start_index
            Select Case tree_char(i)
                Case ")"
                    r_c += 1
                Case Else
            End Select
        Next



        l_c = 0
        r_c = 0

        Dim new_clade As String = ""
        Dim new_tree As String = ""
        Dim has_value As Boolean = False
        Dim tree_value(0) As String
        Dim tree_index(0) As Integer
        For i As Integer = 1 To end_index - start_index - 1
            Select Case tree_char(end_index - i)
                Case "("
                    l_c += 1
                    new_clade = new_clade + ")"
                    If UBound(tree_value) > 0 Then
                        new_clade = new_clade + tree_value(UBound(tree_value))
                        ReDim Preserve tree_value(tree_value.Length - 2)
                    End If
                    ReDim Preserve tree_index(tree_index.Length - 2)
                Case ")"
                    r_c += 1
                    new_clade = new_clade + "("
                    ReDim Preserve tree_index(tree_index.Length)
                    tree_index(UBound(tree_index)) = Selected_node - r_c

                Case ","
                    new_clade = new_clade + ","
                Case Else
                    If tree_char(end_index - i - 1) = ")" Then
                        ReDim Preserve tree_value(tree_value.Length)
                        tree_value(UBound(tree_value)) = tree_char(end_index - i)
                    Else
                        new_clade = new_clade + tree_char(end_index - i)
                    End If
            End Select
        Next
        For i As Integer = 1 To start_index
            new_tree = new_tree + tree_char(i)
        Next
        new_tree = new_tree + new_clade
        For i As Integer = end_index To char_id
            new_tree = new_tree + tree_char(i)
        Next

        Dim node_line_old() As String
        ReDim node_line_old(NumofNode - 1)
        For i As Integer = 0 To NumofNode - 1
            Dim temp_arry() As String = Poly_Node(i, 3).Split(",")
            Array.Sort(temp_arry)
            Poly_Node(i, 3) = temp_arry(0)
            For j As Integer = 1 To UBound(temp_arry)
                Poly_Node(i, 3) += "," + temp_arry(j)
            Next
            node_line_old(i) = Poly_Node(i, 3)
        Next
        Treeline = new_tree
        Read_Poly_Tree(Treeline)
        Dim node_line_new() As String
        ReDim node_line_new(NumofNode - 1)
        For i As Integer = 0 To NumofNode - 1
            Dim temp_arry() As String = Poly_Node(i, 3).Split(",")
            Array.Sort(temp_arry)
            Poly_Node(i, 3) = temp_arry(0)
            For j As Integer = 1 To UBound(temp_arry)
                Poly_Node(i, 3) += "," + temp_arry(j)
            Next
            node_line_new(i) = Poly_Node(i, 3)
        Next

        Return new_tree
    End Function
    Public Sub Read_Poly_Tree(ByVal Treeline As String)
        If Treeline.EndsWith(";") = False Then
            Treeline += ";"
        End If
        ReDim Poly_Node(NumofNode - 1, 10) '0 root,1 末端, 2 子节点, 3 全部链, 4 左侧个数, 5 右侧个数, 6 支持率,7 枝长, 8 总长, 9 祖先节点, 10 级别
        ReDim Poly_Node_row(NumofNode - 1, 2)
        ReDim Poly_Node_col(NumofNode - 1, 2)
        ReDim Poly_terminal_xy(NumofTaxon - 1, 2)
        ReDim TaxonTime(NumofTaxon - 1, 2)
        has_length = False
        If Treeline.Contains(":") Then
            has_length = True
        End If
        ReDim taxon_array(NumofTaxon - 1)
        For i As Integer = 0 To NumofNode - 1
            Poly_Node(i, 0) = 0
            Poly_Node(i, 1) = ""
            Poly_Node(i, 2) = ""
            Poly_Node(i, 3) = ""
            Poly_Node(i, 6) = "0.00"
            Poly_Node(i, 7) = "0"
            Poly_Node(i, 8) = "0"
            Poly_Node(i, 10) = "0"
        Next
        Dim tree_char() As String
        ReDim tree_char(NumofTaxon * 7)
        Dim char_id As Integer = 0
        Dim l_c As Integer = 0
        Dim r_c As Integer = 0
        Dim tx As Integer = 0
        Dim last_symb As Boolean = True
        For Each i As Char In Treeline
            Select Case i
                Case "("
                    char_id += 1
                    tree_char(char_id) = i
                    last_symb = True

                Case ")"
                    char_id += 1
                    tree_char(char_id) = i
                    last_symb = True
                Case ","
                    char_id += 1
                    tree_char(char_id) = i
                    last_symb = True
                Case Else
                    If last_symb Then
                        char_id += 1
                        tree_char(char_id) = i
                        last_symb = False
                    Else
                        tree_char(char_id) += i
                    End If
            End Select
        Next
        ReDim Tree_Export_Char(char_id)
        For i As Integer = 1 To char_id
            Tree_Export_Char(i) = tree_char(i)
        Next
        If has_length Then
            For i As Integer = 1 To char_id
                If Tree_Export_Char(i).Contains(":") Then
                    If Tree_Export_Char(i - 1) <> ")" Then
                        '物种
                        TaxonTime(CInt(tree_char(i).Split(New Char() {":"c})(0)) - 1, 0) = tree_char(i).Split(New Char() {":"c})(1)
                        tree_char(i) = tree_char(i).Split(New Char() {":"c})(0)
                    End If
                End If
            Next
        End If

        Dim point_1, point_2 As Integer
        point_1 = 0
        point_2 = 0
        Dim Temp_node(,) As String
        ReDim Temp_node(NumofNode, 6) '0 节点位置,1 末端, 2 子节点, 4 左侧个数, 5 右侧个数, 6 支持率
        For i As Integer = 0 To NumofNode - 1
            Temp_node(i, 0) = ""
            Temp_node(i, 1) = ""
            Temp_node(i, 2) = ""
            Temp_node(i, 4) = "32768"
            Temp_node(i, 5) = "0"
            Temp_node(i, 6) = "1"
        Next
        For i As Integer = 1 To char_id
            Select Case tree_char(i)
                Case "("
                    l_c += 1
                    Temp_node(point_1, 0) = i
                    point_1 += 1
                Case ")"
                    r_c += 1
                    Poly_Node(point_2, 1) = Temp_node(point_1 - 1, 1)
                    Poly_Node(point_2, 2) = Temp_node(point_1 - 1, 2)
                    Poly_Node(point_2, 4) = Temp_node(point_1 - 1, 4)
                    Poly_Node(point_2, 5) = Temp_node(point_1 - 1, 5)
                    If Poly_Node(point_2, 2) = "" Then
                        Poly_Node(point_2, 10) = 1
                    Else
                        For Each k As String In Poly_Node(point_2, 2).Split(",")
                            If k <> "" Then
                                If CInt(Poly_Node(CInt(k), 10)) >= CInt(Poly_Node(point_2, 10)) Then
                                    Poly_Node(point_2, 10) = CInt(Poly_Node(k, 10)) + 1
                                End If
                            End If
                        Next
                    End If
                    For j As Integer = Temp_node(point_1 - 1, 0) To i
                        If tree_char(j) <> "(" And tree_char(j) <> ")" Then
                            If tree_char(j) <> "," Then
                                If tree_char(j - 1) <> ")" Then
                                    Poly_Node(point_2, 3) += tree_char(j)
                                End If
                            Else
                                Poly_Node(point_2, 3) += tree_char(j)
                            End If
                        End If
                    Next
                    If point_1 > 1 Then
                        Temp_node(point_1 - 2, 2) = point_2.ToString + "," + Temp_node(point_1 - 2, 2)
                        Temp_node(point_1 - 2, 4) = Min(Val(Temp_node(point_1 - 2, 4)), (Val(Poly_Node(point_2, 5)) + Val(Poly_Node(point_2, 4))) / 2)
                        Temp_node(point_1 - 2, 5) = Max(Val(Temp_node(point_1 - 2, 5)), (Val(Poly_Node(point_2, 5)) + Val(Poly_Node(point_2, 4))) / 2)
                    End If
                    point_2 += 1
                    point_1 -= 1
                    Temp_node(point_1, 0) = ""
                    Temp_node(point_1, 1) = ""
                    Temp_node(point_1, 2) = ""
                    Temp_node(point_1, 4) = "32768"
                    Temp_node(point_1, 5) = "0"
                Case ","

                Case Else
                    If tree_char(i - 1) = ")" Then
                        '读取支持率
                        If has_length And tree_char(i).Contains(":") Then
                            If Val(tree_char(i).Split(New Char() {":"c})(0)) > 1 Then
                                Poly_Node(point_2 - 1, 6) = (Val(tree_char(i)) / 100).ToString("F2")
                            Else
                                Poly_Node(point_2 - 1, 6) = Val(tree_char(i)).ToString("F2")
                            End If
                            Poly_Node(point_2 - 1, 7) = tree_char(i).Split(New Char() {":"c})(1)
                        Else
                            If Val(tree_char(i)) > 1 Then
                                Poly_Node(point_2 - 1, 6) = (Val(tree_char(i)) / 100).ToString("F2")
                            Else
                                Poly_Node(point_2 - 1, 6) = Val(tree_char(i)).ToString("F2")
                            End If
                        End If

                    Else
                        taxon_array(tx) = tree_char(i)
                        tx += 1
                        Temp_node(point_1 - 1, 1) += tree_char(i) + ","
                        Temp_node(point_1 - 1, 4) = Min(Val(Temp_node(point_1 - 1, 4)), tx)
                        Temp_node(point_1 - 1, 5) = Max(Val(Temp_node(point_1 - 1, 4)), tx)
                    End If
            End Select
        Next
        If has_length Then
            make_chain(NumofNode - 1)
        End If
        time_Dataset.Tables("Time Table").Clear()
        time_view.AllowNew = True
        maxtime = 0
        For i As Integer = 0 To NumofTaxon - 1
            If maxtime < Val(TaxonTime(i, 1)) Then
                maxtime = Val(TaxonTime(i, 1))
            End If
        Next
        Dim temp_Item_Array As Integer = 0

        For i As Integer = 0 To NumofNode - 1
            If (temp_Item_Array = 0 And time_view.Count = 1) = False Then
                time_view.AddNew()
            End If
            Dim newrow(2) As String
            newrow(0) = time_view.Count + NumofTaxon
            newrow(1) = Poly_Node(i, 6)
            'If CheckBox8.Checked Then
            '    newrow(2) = ((maxtime - Val(Poly_Node(i, 8))) * root_time).ToString("F8")
            '    TaxonTime(i, 2) = ((maxtime - Val(TaxonTime(i, 1))) * root_time).ToString("F8")
            'Else
            newrow(2) = "" '(Val(Poly_Node(i, 8)) * root_time).ToString("F8")
            TaxonTime(i, 2) = (Val(TaxonTime(i, 1)) * root_time).ToString("F8")
            'End If
            time_view.Item(temp_Item_Array).Row.ItemArray = newrow
            temp_Item_Array += 1

            If Poly_Node(i, 2) <> "" Then
                Dim anc_node() As String = Poly_Node(i, 2).Split(New Char() {","c})

                For Each j As String In anc_node
                    If j <> "" Then
                        Poly_Node(CInt(j), 9) = i.ToString
                        Poly_Node(i, 0) = Math.Max(Val(Poly_Node(i, 0)), Poly_Node(CInt(j), 0) + 1)
                    End If
                Next
            Else
                Poly_Node(i, 0) = 1
            End If
        Next
        time_view.AllowNew = False
        make_tree_xy()

        If maxtime <= 0 Then
            has_length = False
        End If
    End Sub
    Public Sub make_tree_xy()
        For i As Integer = 0 To NumofNode - 1
            Dim anc_node() As String = Poly_Node(i, 2).Split(New Char() {","c})
            If Poly_Node(i, 2) <> "" Then
                For Each j As String In anc_node
                    If j <> "" Then
                        max_level = Math.Max(Val(Poly_Node(i, 0)), max_level)
                    End If
                Next
                For Each j As String In anc_node
                    If j <> "" Then
                        '竖线及节点中心坐标
                        Poly_Node_col(CInt(j), 0) = (Val(Poly_Node(CInt(j), 4)) + Val(Poly_Node(CInt(j), 5))) / 2
                        If ToplogyOnly = False And has_length Then
                            Poly_Node_col(CInt(j), 1) = (maxtime - Val(Poly_Node(CInt(j), 8))) * max_level / maxtime
                            Poly_Node_col(CInt(j), 2) = (maxtime - Val(Poly_Node(i, 8))) * max_level / maxtime

                        Else
                            Poly_Node_col(CInt(j), 1) = Val(Poly_Node(CInt(j), 0))
                            Poly_Node_col(CInt(j), 2) = Val(Poly_Node(i, 0))
                        End If

                    End If
                Next
            End If
            '横线
            If ToplogyOnly = False And has_length Then
                Poly_Node_row(i, 0) = (maxtime - Val(Poly_Node(i, 8))) * max_level / maxtime
            Else
                Poly_Node_row(i, 0) = Val(Poly_Node(i, 0))
            End If
            Poly_Node_row(i, 1) = Val(Poly_Node(i, 4))
            Poly_Node_row(i, 2) = Val(Poly_Node(i, 5))
            If Poly_Node(i, 1) <> "" Then
                Dim anc_terminal() As String = Poly_Node(i, 1).Split(New Char() {","c})
                For Each j As String In anc_terminal
                    If j <> "" Then
                        '末端分支横线
                        Poly_terminal_xy(CInt(j) - 1, 0) = (Array.IndexOf(taxon_array, j) + 1)
                        If ToplogyOnly = False And has_length Then
                            Poly_terminal_xy(CInt(j) - 1, 1) = (maxtime - TaxonTime(CInt(j) - 1, 1)) * max_level / maxtime
                            Poly_terminal_xy(CInt(j) - 1, 2) = (maxtime - Val(Poly_Node(i, 8))) * max_level / maxtime
                        Else
                            Poly_terminal_xy(CInt(j) - 1, 1) = 0
                            Poly_terminal_xy(CInt(j) - 1, 2) = Val(Poly_Node(i, 0))
                        End If

                    End If
                Next
            End If
        Next
        '根节点位置
        Poly_Node_col(NumofNode - 1, 0) = (Val(Poly_Node(NumofNode - 1, 4)) + Val(Poly_Node(NumofNode - 1, 5))) / 2
        If ToplogyOnly = False And has_length Then
            Poly_Node_col(NumofNode - 1, 1) = (maxtime - Val(Poly_Node(NumofNode - 1, 8))) * max_level / maxtime
            Poly_Node_col(NumofNode - 1, 2) = (maxtime - Val(Poly_Node(NumofNode - 1, 8))) * max_level / maxtime
        Else
            Poly_Node_col(NumofNode - 1, 1) = Val(Poly_Node(NumofNode - 1, 0))
            Poly_Node_col(NumofNode - 1, 2) = Val(Poly_Node(NumofNode - 1, 0))
        End If
    End Sub
    Public Sub make_chain(ByVal n As Integer)
        If Poly_Node(n, 2) <> "" Then
            Dim anc_node() As String = Poly_Node(n, 2).Split(New Char() {","c})
            For Each j As String In anc_node
                If j <> "" Then
                    Poly_Node(CInt(j), 8) = (Val(Poly_Node(CInt(j), 7)) + Val(Poly_Node(n, 8))).ToString
                    make_chain(CInt(j))
                End If
            Next
        End If
        If Poly_Node(n, 1) <> "" Then
            Dim anc_node() As String = Poly_Node(n, 1).Split(New Char() {","c})
            For Each j As String In anc_node
                If j <> "" Then
                    TaxonTime(CInt(j) - 1, 1) = (Val(TaxonTime(CInt(j) - 1, 0)) + Val(Poly_Node(n, 8))).ToString
                End If
            Next
        End If
    End Sub
    Private Sub TreeView_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
        e.Cancel = True
        Me.Hide()
    End Sub

    Private Sub config_dated_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        data_type = 0
        System.Threading.Thread.CurrentThread.CurrentCulture = ci
        CheckForIllegalCrossThreadCalls = False
        Loading = True
        selected_nodes(0) = -1
        D_nodes(0) = -1
        R_nodes(0) = -1
        E_nodes(0) = -1
        G_nodes(0) = -1
        V_nodes(0) = -1
        Dim taxon_table As New System.Data.DataTable With {
            .TableName = "Time Table"
        }
        Dim Column_ID As New System.Data.DataColumn("ID")
        Dim Column_Length As New System.Data.DataColumn("SV")
        Dim Column_Time As New System.Data.DataColumn("Calibration")
        taxon_table.Columns.Add(Column_ID)
        taxon_table.Columns.Add(Column_Length)
        taxon_table.Columns.Add(Column_Time)

        time_Dataset.Tables.Add(taxon_table)

        time_view = time_Dataset.Tables("Time Table").DefaultView
        time_view.AllowNew = False
        time_view.AllowDelete = False
        time_view.AllowEdit = True
        DataGridView1.DataSource = time_view
        DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(0).ReadOnly = True
        DataGridView1.Columns(0).Width = 30
        DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(1).Width = 50
        DataGridView1.Columns(1).ReadOnly = True
        DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(2).Width = 90
        DataGridView1.Columns(2).ReadOnly = True

        Tree_font = FontDialog1.Font
        Label_font = FontDialog1.Font
        ID_font = FontDialog1.Font

        Dim name_table As New System.Data.DataTable With {
            .TableName = "Tree Table"
        }
        Dim Column_TaxID As New System.Data.DataColumn("ID")
        Dim Column_Name As New System.Data.DataColumn("Name")
        Dim Column_State As New System.Data.DataColumn("State")
        name_table.Columns.Add(Column_TaxID)
        name_table.Columns.Add(Column_Name)
        name_table.Columns.Add(Column_State)
        time_Dataset.Tables.Add(name_table)
        dtView = time_Dataset.Tables("Tree Table").DefaultView
        dtView.AllowNew = False
        dtView.AllowDelete = False
        dtView.AllowEdit = False

        AddHandler form_config_fossil.ConfirmClicked, AddressOf Fossil_ConfirmClickedHandler

    End Sub
    Public Sub Fossil_ConfirmClickedHandler()
        'DataGridView1.EndEdit()
        DataGridView1.DataSource = time_view
        DataGridView1.Refresh()

        PictureBox1.Width = (max_level + 2) * Branch_length + 2 * Border_separation + (max_taxon_name + RangeStr.Length + 8) * Label_font.SizeInPoints
        PictureBox1.Height = (NumofTaxon + 5) * Taxon_separation + Border_separation + Label_font.Height + Tree_font.Height 'y
        Bitmap_Tree = New Bitmap(CInt(PictureBox1.Width), CInt(PictureBox1.Height))
        draw_tree(Graphics.FromImage(Bitmap_Tree))
        PictureBox1.Refresh()

    End Sub
    Public Sub draw_loaded_tree()
        show_my_tree = tree_show_with_value.Replace(";", "")
        load_my_tree()
        Loading = False
        PictureBox1.Width = (max_level + 2) * Branch_length + 2 * Border_separation + (max_taxon_name + RangeStr.Length + 8) * Label_font.SizeInPoints
        PictureBox1.Height = (NumofTaxon + 5) * Taxon_separation + Border_separation + Label_font.Height + Tree_font.Height 'y
        Bitmap_Tree = New Bitmap(CInt(PictureBox1.Width), CInt(PictureBox1.Height))
        draw_tree(Graphics.FromImage(Bitmap_Tree))
        PictureBox1.Refresh()
    End Sub
    Public Sub load_my_tree()
        NumofTaxon = show_my_tree.Length - show_my_tree.Replace(",", "").Length + 1
        NumofNode = show_my_tree.Length - show_my_tree.Replace("(", "").Length

        ReDim TaxonName(NumofTaxon - 1)
        ReDim Distribution(NumofTaxon - 1)
        For i As Integer = 0 To NumofTaxon - 1
            TaxonName(i) = dtView.Item(i).Item(1).ToString
            Distribution(i) = dtView.Item(i).Item(current_state).ToString
            max_taxon_name = Math.Max(max_taxon_name, TaxonName(i).Length)
        Next

        ReDim Current_AreaS(NumofNode - 1, 32)
        ReDim Current_AreaP(NumofNode - 1, 32)
        ReDim Color_S(0)
        ReDim Color_B(0)
        Color_S(0) = "*"
        Color_B(0) = Brushes.Black

        For i As Integer = 0 To RangeLength - 1
            ReDim Preserve Color_S(UBound(Color_S) + 1)
            ReDim Preserve Color_B(UBound(Color_S))
            Color_S(UBound(Color_S)) = ChrW(65 + i)
        Next
        Color_S_node = Color_S.Clone
        Color_B_node = Color_B.Clone
        For Each i As String In Distribution
            If Array.IndexOf(Color_S, sort_area(i)) < 0 Then
                ReDim Preserve Color_S(UBound(Color_S) + 1)
                ReDim Preserve Color_B(UBound(Color_S))
                Color_S(UBound(Color_S)) = sort_area(i)
            End If
        Next
        load_color()
        Read_Poly_Tree(show_my_tree)
    End Sub
    Public Sub SortNum(ByRef input_array() As Object)
        For i As Integer = 0 To UBound(input_array)
            For j As Integer = i + 1 To UBound(input_array)
                If IsNumeric(input_array(i)) And IsNumeric(input_array(j)) Then
                    If CInt(input_array(i)) > CInt(input_array(j)) Then
                        Dim t As Integer = CInt(input_array(i))
                        input_array(i) = input_array(j)
                        input_array(j) = t.ToString
                    End If
                End If
                If IsNumeric(input_array(j)) And IsNumeric(input_array(i)) = False Then
                    If input_array(i) <> "*" And input_array(i) <> "" And input_array(i) <> "\" Then
                        Dim t As String = input_array(i)
                        input_array(i) = input_array(j)
                        input_array(j) = t.ToString
                    End If
                End If
                If IsNumeric(input_array(j)) = False And IsNumeric(input_array(i)) = False Then
                    If input_array(i) > input_array(j) Then
                        Dim t As String = input_array(i)
                        input_array(i) = input_array(j)
                        input_array(j) = t
                    End If
                End If
            Next
        Next
    End Sub
    Public Sub load_color()

        If data_type = 1 Then
            SortNum(Color_S)
        End If
        For i As Integer = 0 To Color_S.Length - 1
            Color_B(i) = Int2Brushes(Distributiton_to_Integer(Color_S(i)))
        Next
        If data_type = 1 Then
            SortNum(Color_S_node)
        End If
        For i As Integer = 0 To Color_S_node.Length - 1
            Color_B_node(i) = Int2Brushes(Distributiton_to_Integer(Color_S_node(i)))
        Next
        Array.Sort(Color_S, Color_B)
        Array.Sort(Color_S_node, Color_B_node)


    End Sub
    Dim PROB_list(,) As Single
    Dim reconstr_path As String

    Public Sub cale_relation()
        ReDim Node_Relationship(NumofNode, 2) '0,1 末端, 2 子节点, 3 全部链, 4 左侧个数, 5 右侧个数, 6 支持率
        For i As Integer = 0 To NumofNode - 1
            Node_Relationship(i, 0) = Get_node(i)
            Node_Relationship(i, 1) = Poly_Node(i, 1)
            For Each j As String In Node_Relationship(i, 0).Split(New Char() {","c})
                If j <> "" Then
                    If Result_list(CInt(j)) = "" Then
                        If Poly_Node(CInt(j), 1) <> "" Then
                            Node_Relationship(i, 1) = Node_Relationship(i, 1) + Poly_Node(CInt(j), 1)
                        End If
                    ElseIf Result_list(CInt(j)).Split(New Char() {" "c})(1) = "" Or Result_list(CInt(j)).Split(New Char() {" "c})(1) = "/" Then
                        If Poly_Node(CInt(j), 1) <> "" Then
                            Node_Relationship(i, 1) = Node_Relationship(i, 1) + Poly_Node(CInt(j), 1)
                        End If
                    Else
                        Node_Relationship(i, 2) = Node_Relationship(i, 2) + j + ","
                    End If
                End If
            Next
        Next
    End Sub
    Public Function Get_node(ByVal n As Integer) As String
        Dim node As String = ""
        If Poly_Node(n, 2) <> "" Then
            Dim temp_ter() As String = Poly_Node(n, 2).Split(New Char() {","c})
            For Each k As String In temp_ter
                If k <> "" Then
                    If Result_list(CInt(k)) = "" Then
                        node = node + k + "," + Get_node(CInt(k))
                    ElseIf Result_list(CInt(k)).Split(New Char() {" "c})(1) = "" Or Result_list(CInt(k)).Split(New Char() {" "c})(1) = "/" Then
                        node = node + k + "," + Get_node(CInt(k))
                    Else
                        node = node + k + ","
                    End If
                End If
            Next
        End If
        Return node
    End Function
    Public Sub Load_result()
        ReDim Current_AreaS(NumofNode - 1, 32)
        ReDim Current_AreaP(NumofNode - 1, 32)
        ReDim Color_S(0)
        ReDim Color_B(0)
        Color_S(0) = "*"
        Color_B(0) = Brushes.Black
        If data_type = 0 Then
            For i As Integer = 0 To RangeLength - 1
                ReDim Preserve Color_S(UBound(Color_S) + 1)
                ReDim Preserve Color_B(UBound(Color_S))
                Color_S(UBound(Color_S)) = ChrW(65 + i)
            Next
            'For Each i As String In Distribution
            '    If Array.IndexOf(Color_S, sort_area(i)) < 0 Then
            '        ReDim Preserve Color_S(UBound(Color_S) + 1)
            '        ReDim Preserve Color_B(UBound(Color_S))
            '        Color_S(UBound(Color_S)) = sort_area(i)
            '    End If
            'Next
        End If

        For i As Integer = NumofNode * (result_ID - 1) To NumofNode * result_ID - 1
            Dim Templist() As String = Result_list(i).Split(New Char() {" "c})
            Dim Temp_array As Integer = 0
            Dim Temp_sum As Single = 0
            For j As Integer = 1 To UBound(Templist) Step 2
                If j <= 64 Then
                    If Templist(j + 1) >= area_lower Or Temp_array < keep_at_least Then
                        If Templist(j) = "" Then
                            Templist(j) = "/"
                        End If
                        Current_AreaS(CInt(i Mod NumofNode), CInt((j - 1) / 2)) = Templist(j)
                        Current_AreaP(CInt(i Mod NumofNode), CInt((j - 1) / 2)) = Templist(j + 1)
                        Temp_array += 1
                        Temp_sum += Templist(j + 1)
                        If Array.IndexOf(Color_S, Templist(j)) < 0 Then
                            ReDim Preserve Color_S(UBound(Color_S) + 1)
                            ReDim Preserve Color_B(UBound(Color_S))
                            Color_S(UBound(Color_S)) = Templist(j)
                        End If
                    End If
                Else
                    Exit For
                End If

            Next
            Current_AreaP(i Mod NumofNode, Temp_array) = 100 - Temp_sum
            Current_AreaS(i Mod NumofNode, Temp_array) = "*"
        Next
        Color_S_node = Color_S.Clone
        Color_B_node = Color_B.Clone
        For Each i As String In Distribution
            If Array.IndexOf(Color_S, sort_area(i)) < 0 Then
                ReDim Preserve Color_S(UBound(Color_S) + 1)
                ReDim Preserve Color_B(UBound(Color_S))
                Color_S(UBound(Color_S)) = sort_area(i)
            End If
        Next
        load_color()

    End Sub

    Public Sub draw_tree(ByVal TempGrap As Object)
        make_tree_xy()
        Dim Poly_terminal_xy_draw(,) As Single
        Dim Poly_Node_row_draw(,) As Single
        Dim Poly_Node_col_draw(,) As Single
        ReDim Poly_Node_row_draw(NumofNode - 1, 2)
        ReDim Poly_Node_col_draw(NumofNode - 1, 2)
        ReDim Poly_terminal_xy_draw(NumofTaxon - 1, 2)
        Dim Tree_pen As New System.Drawing.Pen(System.Drawing.Color.Black, Line_width)
        Dim Select_pen As New System.Drawing.Pen(System.Drawing.Color.Red, Line_width * 2)
        Dim Line_pen As New System.Drawing.Pen(System.Drawing.Color.LightGray, Max(1, CInt(Line_width / 2)))
        Dim startpoint As Integer = (max_level + 2) * Branch_length + Border_separation 'x

        For i As Integer = 0 To NumofTaxon - 1
            Poly_terminal_xy_draw(i, 0) = Poly_terminal_xy(i, 0) * Taxon_separation + Taxon_separation + Label_font.Height 'y
            Poly_terminal_xy_draw(i, 1) = Poly_terminal_xy(i, 1) * Branch_length 'x1
            Poly_terminal_xy_draw(i, 2) = Poly_terminal_xy(i, 2) * Branch_length 'x2
        Next
        For i As Integer = 0 To NumofNode - 1
            Poly_Node_row_draw(i, 0) = Poly_Node_row(i, 0) * Branch_length 'x
            Poly_Node_row_draw(i, 1) = Poly_Node_row(i, 1) * Taxon_separation + Taxon_separation + Label_font.Height 'y1
            Poly_Node_row_draw(i, 2) = Poly_Node_row(i, 2) * Taxon_separation + Taxon_separation + Label_font.Height  'y2
            Poly_Node_col_draw(i, 0) = Poly_Node_col(i, 0) * Taxon_separation + Taxon_separation + Label_font.Height  'y
            Poly_Node_col_draw(i, 1) = Poly_Node_col(i, 1) * Branch_length 'x1
            Poly_Node_col_draw(i, 2) = Poly_Node_col(i, 2) * Branch_length 'x2

        Next
        '背景透明
        If TransparentBG = False And savingpic Then
            TempGrap.FillRectangle(Brushes.White, 0, 0, PictureBox1.Width * File_zoom, PictureBox1.Height * File_zoom)
        ElseIf savingpic = False Then
            TempGrap.FillRectangle(Brushes.White, 0, 0, PictureBox1.Width * File_zoom, PictureBox1.Height * File_zoom)
        End If
        '时间轴
        If has_length And ToplogyOnly = False And ShowScale Then
            TempGrap.DrawLine(Line_pen, startpoint - Poly_Node_col_draw(NumofNode - 1, 1), (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height, startpoint, (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height)
            TempGrap.DrawLine(Line_pen, startpoint - Poly_Node_col_draw(NumofNode - 1, 1), Border_separation + Label_font.Height, startpoint, Border_separation + Label_font.Height)

            TempGrap.DrawString((maxtime * root_time).ToString("F0"), Label_font, Brushes.Black, startpoint - Poly_Node_col_draw(NumofNode - 1, 1), (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height)
            TempGrap.DrawString("0", Label_font, Brushes.Black, startpoint, (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height)
            For i As Integer = 0 To maxtime * root_time * smooth_x Step NumericUpDown1.Value * smooth_x
                Dim steptime As Integer = i * Poly_Node_col_draw(NumofNode - 1, 1) / maxtime / root_time / smooth_x
                TempGrap.DrawLine(Line_pen, startpoint - steptime, (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height, startpoint - steptime, Border_separation + Label_font.Height)
                TempGrap.DrawString((i / smooth_x).ToString("F0"), Label_font, Brushes.Black, startpoint - steptime, (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height)
            Next
            TempGrap.DrawLine(Line_pen, startpoint - Poly_Node_col_draw(NumofNode - 1, 1), (NumofTaxon + 2) * Taxon_separation + Border_separation + Label_font.Height, startpoint - Poly_Node_col_draw(NumofNode - 1, 1), Border_separation + Label_font.Height)
        End If
        '绘制树干
        If Display_lines Then
            For i As Integer = 0 To NumofNode - 1
                If i <> Selected_node Then
                    TempGrap.DrawLine(Tree_pen, startpoint - CInt(Poly_Node_row_draw(i, 0)), CInt(Poly_Node_row_draw(i, 1)), startpoint - CInt(Poly_Node_row_draw(i, 0)), CInt(Poly_Node_row_draw(i, 2)))
                    TempGrap.DrawLine(Tree_pen, startpoint - CInt(Poly_Node_col_draw(i, 1)), CInt(Poly_Node_col_draw(i, 0)), startpoint - CInt(Poly_Node_col_draw(i, 2)), CInt(Poly_Node_col_draw(i, 0)))
                Else
                    TempGrap.DrawLine(Select_pen, startpoint - CInt(Poly_Node_row_draw(i, 0)), CInt(Poly_Node_row_draw(i, 1)), startpoint - CInt(Poly_Node_row_draw(i, 0)), CInt(Poly_Node_row_draw(i, 2)))
                    TempGrap.DrawLine(Select_pen, startpoint - CInt(Poly_Node_col_draw(i, 1)), CInt(Poly_Node_col_draw(i, 0)), startpoint - CInt(Poly_Node_col_draw(i, 2)), CInt(Poly_Node_col_draw(i, 0)))
                End If
            Next

            For i As Integer = 0 To NumofTaxon - 1
                TempGrap.DrawLine(Tree_pen, startpoint - CInt(Poly_terminal_xy_draw(i, 1)), CInt(Poly_terminal_xy_draw(i, 0)), startpoint - CInt(Poly_terminal_xy_draw(i, 2)), CInt(Poly_terminal_xy_draw(i, 0)))
            Next
        End If

        TempGrap.SmoothingMode = SmoothingMode.AntiAlias

        For i As Integer = 0 To NumofTaxon - 1
            '物种名
            Dim Temp_str As String = ""
            If Display_taxon_names Then
                Temp_str = TaxonName(i).Replace("_", " ")
            End If
            If Display_taxon_dis Then
                Temp_str = "(" + Distribution(i) + ") " + Temp_str
            End If

            TempGrap.DrawString(Temp_str, Tree_font, Brushes.Black, startpoint - CInt(Poly_terminal_xy_draw(i, 1)) + pie_radii, CInt(Poly_terminal_xy_draw(i, 0)) - Tree_font.GetHeight / 2)
        Next
        For i As Integer = 0 To NumofNode - 1
            '支持率
            If Display_node_frequency Then
                If CSng(Poly_Node(i, 6)) >= Low_frequency Then
                    TempGrap.DrawString((CSng(Poly_Node(i, 6)) * 100).ToString("F0"), Label_font, Brushes.Black, startpoint - CInt(Poly_Node_col_draw(i, 1)) + frequency_h, CInt(Poly_Node_col_draw(i, 0)) + frequency_v)
                End If
            End If
            'node ID
            If Display_node_ID Then
                'If CSng(Poly_Node(i, 6)) >= Hide_pie Then
                If time_view.Item(i).Item(2).ToString <> "" Then
                    TempGrap.DrawString((i + NumofTaxon + 1).ToString + " " + time_view.Item(i).Item(2).ToString, ID_font, New SolidBrush(ID_color), startpoint - CInt(Poly_Node_col_draw(i, 1)) + node_h, CInt(Poly_Node_col_draw(i, 0)) + node_v)
                Else
                    TempGrap.DrawString((i + NumofTaxon + 1).ToString, ID_font, New SolidBrush(ID_color), startpoint - CInt(Poly_Node_col_draw(i, 1)) + node_h, CInt(Poly_Node_col_draw(i, 0)) + node_v)

                End If
                'End If
            End If
        Next

    End Sub
    Private Sub PictureBox1_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDoubleClick
        Dim click_node As Boolean = False
        Selected_node = -1
        If StartTreeView Then
            For i As Integer = 0 To NumofNode - 1
                If (Poly_Node_col(i, 0) + 1) * Taxon_separation + Label_font.Height + pie_radii > e.Y And e.Y > (Poly_Node_col(i, 0) + 1) * Taxon_separation + Label_font.Height - pie_radii Then
                    If (max_level + 2) * Branch_length + Border_separation - Poly_Node_col(i, 1) * Branch_length + pie_radii > e.X And
                e.X > (max_level + 2) * Branch_length + Border_separation - Poly_Node_col(i, 1) * Branch_length - pie_radii Then
                        click_node = True
                        Selected_node = i
                        Exit For
                    End If
                End If
            Next

            PictureBox1.Width = (max_level + 2) * Branch_length + 2 * Border_separation + (max_taxon_name + RangeStr.Length + 8) * Label_font.SizeInPoints
            PictureBox1.Height = (NumofTaxon + 5) * Taxon_separation + Border_separation + Label_font.Height + Tree_font.Height 'y
            Bitmap_Tree = New Bitmap(CInt(PictureBox1.Width), CInt(PictureBox1.Height))
            draw_tree(Graphics.FromImage(Bitmap_Tree))
            PictureBox1.Refresh()
            If Selected_node >= 0 Then
                DataGridView1.ClearSelection()
                ' 选中对应的行
                DataGridView1.Rows(Selected_node).Selected = True
                form_config_fossil.Show()
            End If

        End If
    End Sub
    Dim Bitmap_Tree As Bitmap
    Dim Bitmap_Legend As Bitmap

    Private Sub PictureBox1_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBox1.Paint
        If (StartTreeView Or draw_result) And Bitmap_Tree IsNot Nothing Then
            e.Graphics.DrawImage(Bitmap_Tree, 0, 0)
        End If
    End Sub

    Public Sub gray_bmp(ByVal MyBitmap As Bitmap)
        Dim t, tt As Integer
        Dim b As Integer, c As Color
        With MyBitmap
            For t = 0 To .Width - 1
                For tt = 0 To .Height - 1
                    c = .GetPixel(t, tt)
                    b = c.R * 0.3 + c.G * 0.5 + c.B * 0.2
                    .SetPixel(t, tt, Color.FromArgb(b, b, b))
                Next
            Next
        End With
    End Sub

    Dim RASP_Event(,) As Integer
    Public Function sort_area(ByVal input_area As String) As String
        If data_type = 1 Then
            Return input_area
        End If
        Dim temp() As Char = input_area
        Array.Sort(temp)
        Return temp
    End Function



    Dim savingpic As Boolean = False
    Dim maxtime As Single = 0
    Dim root_time As Single = 1
    Dim unit_time As Single = 1
    Dim draw_area(,) As Single
    Dim draw_pie(,) As String
    Dim draw_area_para(3) As Single
    Dim start_draw_area As Boolean = False

    Public Function node_line(ByVal n As Integer) As String
        Dim line As String = n.ToString + ","
        Dim anc_node() As String = Poly_Node(n - NumofTaxon - 1, 2).Split(New Char() {","c})
        For Each j As String In anc_node
            If j <> "" Then
                line = line + node_line(j + NumofTaxon + 1)
            End If

        Next
        Return line
    End Function

    Dim Table_font As Font = New Font("Tahoma", 10, FontStyle.Regular)
    Dim swaping As Boolean = False
    Private Sub 载入树ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入树ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Trees dataset|*.trees;*.TREES;*.trees.txt;*.t;*.T;*.tre;*.TRE;*.tree;*.TREE;*.nex;*.NEX|Tree (*.tre)|*.tre;*.TRE|Mrbayes Tree Data (*.t)|*.t;*.T|ALL Files(*.*)|*.*",
            .FileName = "",
            .Multiselect = False,
            .DefaultExt = ".trees",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then

            tree_path = opendialog.FileName

            time_Dataset.Tables("Time Table").Clear()
            time_Dataset.Tables("Tree Table").Clear()
            Tree_Num_P = 0
            Dim l_Tree As New Thread(AddressOf load_trees) With {
                .CurrentCulture = ci
            }
            l_Tree.Start()
        End If
    End Sub

    Public Function load_trees() As Integer
        Try
            Dim line As String = ""
            Dim rt As StreamReader
            'Dim B_WT As StreamWriter
            'Dim P_WT As StreamWriter
            Dim have_translate As Boolean = False

            tree_filename = tree_path.Substring(tree_path.Replace("/", "\").LastIndexOf("\") + 1)
            If tree_filename.Contains(".") Then
                tree_filename = tree_filename.Substring(0, tree_filename.LastIndexOf("."))
            End If

            'Dim name_wt As StreamWriter


            rt = New StreamReader(tree_path)
            'If B_Tree_File = "" Then
            '    B_Tree_File = "temp" + path_char + format_time(Date.Now.ToString) + "_B.tre"
            '    P_Tree_File = "temp" + path_char + format_time(Date.Now.ToString) + "_P.tre"

            '    B_WT = New StreamWriter(root_path + B_Tree_File, False)
            '    P_WT = New StreamWriter(root_path + P_Tree_File, False)
            'Else
            '    B_WT = New StreamWriter(root_path + B_Tree_File, True)
            '    P_WT = New StreamWriter(root_path + P_Tree_File, True)
            'End If
            'name_wt = New StreamWriter(root_path + "temp\gene_name.txt", True)
            Dim name_count As Integer = 0

            'Dim wt_clean_num As New StreamWriter(root_path + "temp" + path_char + "clean_num.trees", True)
            'Dim wt_clean_num_P As New StreamWriter(root_path + "temp" + path_char + "clean_num_p.trees", True)
            line = rt.ReadLine

            Dim f_t_name(,) As String
            ReDim f_t_name(dtView.Count - 1, 1)
            Dim skip_trees As Integer = 0
            Do While line Is Nothing = False
                line = CleanLine(line)
                If line.Replace("	", "").ToUpper.StartsWith("TRANSLATE") Then
                    line = rt.ReadLine.Replace("	", " ").Replace(",", "")
                    Dim name_num As Integer = 0
                    Do

                        If line.Length > 0 Then
                            line = CleanLine(line)
                            Dim TRANSLATE() As String = line.Replace(";", "").Split(New Char() {" "c})
                            f_t_name(name_num, 0) = TRANSLATE(0)
                            f_t_name(name_num, 1) = TRANSLATE(1).Replace("'", "")
                            name_num = name_num + 1
                        End If
                        line = rt.ReadLine.Replace("	", " ").Replace(",", "")
                    Loop Until line.Contains(";")
                    If line.Replace("	", "").Replace(" ", "").Length > 1 Then
                        line = CleanLine(line)
                        Dim TRANSLATE() As String = line.Replace(";", "").Split(New Char() {" "c})
                        f_t_name(name_num, 0) = TRANSLATE(0)
                        f_t_name(name_num, 1) = TRANSLATE(1).Replace("'", "")
                        name_num = name_num + 1
                    End If
                End If

                If line.Replace("	", "").ToUpper.StartsWith("TREE") Or line.Replace("	", "").ToUpper.StartsWith("(") Then
                    Do While line.Contains(";") = False
                        Dim next_tree_line As String = rt.ReadLine
                        If next_tree_line <> "" Then
                            line = line + next_tree_line
                        End If
                    Loop
                    Dim tree_Temp As String = line.Substring(line.IndexOf("("), line.Length - line.IndexOf("(")).Replace("'", "")
                    Dim tree_complete As String = ""
                    Dim tree_sdec As String = ""
                    Dim is_sym As Boolean = False
                    Dim is_kh As Boolean = False
                    Dim is_sym1 As Boolean = False
                    For k As Integer = 0 To tree_Temp.Length - 1
                        Dim tree_chr As Char = tree_Temp(k)
                        If tree_chr = "[" Then
                            is_sym1 = True
                            is_sym = True
                        End If
                        If tree_chr = "]" Then
                            is_sym1 = False
                        End If
                        If tree_chr = ":" Then
                            is_sym = True
                            is_kh = False
                        End If
                        If (tree_chr = "," Or tree_chr = "(" Or tree_chr = ")") And is_sym1 = False Then
                            is_sym = False
                            is_kh = False
                        End If
                        If tree_chr = ")" And is_sym1 = False Then
                            tree_complete &= tree_chr.ToString
                            tree_sdec &= tree_chr.ToString
                            is_sym = True
                            is_kh = True
                        End If
                        If is_sym = False Then
                            tree_complete &= tree_chr.ToString
                        End If
                        If is_sym1 = False And tree_chr <> "]" And is_kh = False Then
                            tree_sdec &= tree_chr.ToString
                        End If
                    Next
                    If dtView.Count <= 1 Then
                        taxon_line = tree_complete
                        Make_Taxon_List()
                    End If
                    Dim outgroup_str As String = ""
                    Dim isbase_three As Boolean = True
                    If tree_complete.Replace("(", "").Length - tree_complete.Replace(",", "").Length > 0 Then
                        MsgBox("It must be a rooted tree and without polytomies.")
                        Return 1
                    End If
                    If tree_complete.EndsWith(";") = False Then
                        tree_complete = tree_complete + ";"
                    End If

                    If taxon_line <> "" Then
                        For i As Integer = 1 To dtView.Count
                            If dtView.Item(i - 1).Item(0).ToString <> "" And dtView.Item(i - 1).Item(1).ToString <> "" Then
                                tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ",", "($%#" + dtView.Item(i - 1).Item(0).ToString + "$%#,")
                                tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ")", ",$%#" + dtView.Item(i - 1).Item(0).ToString + "$%#)")
                                tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ",", ",$%#" + dtView.Item(i - 1).Item(0).ToString + "$%#,")
                                tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ":", "($%#" + dtView.Item(i - 1).Item(0).ToString + "$%#:")
                                tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ":", ",$%#" + dtView.Item(i - 1).Item(0).ToString + "$%#:")
                            End If
                        Next
                        tree_complete = tree_complete.Replace("$%#", "")
                    End If
                    If dtView.Count > 0 Then
                        If f_t_name.Length = dtView.Count * 2 Then
                            For i As Integer = 1 To dtView.Count
                                If f_t_name(i - 1, 0) <> "" And f_t_name(i - 1, 1) <> "" Then
                                    tree_complete = tree_complete.Replace("(" + f_t_name(i - 1, 0) + ",", "($%*" + f_t_name(i - 1, 1) + "$%*,")
                                    tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ")", ",$%*" + f_t_name(i - 1, 1) + "$%*)")
                                    tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ",", ",$%*" + f_t_name(i - 1, 1) + "$%*,")
                                    tree_complete = tree_complete.Replace("(" + f_t_name(i - 1, 0) + ":", "($%*" + f_t_name(i - 1, 1) + "$%*:")
                                    tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ":", ",$%*" + f_t_name(i - 1, 1) + "$%*:")
                                End If
                            Next
                        End If
                        For i As Integer = 1 To dtView.Count
                            tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ",", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ")", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ",", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ":", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ":", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                        Next
                        For i As Integer = 1 To dtView.Count
                            tree_complete = tree_complete.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "(" + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)", "," + dtView.Item(i - 1).Item(0).ToString + ")")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "," + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_complete = tree_complete.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "(" + dtView.Item(i - 1).Item(0).ToString + ":")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "," + dtView.Item(i - 1).Item(0).ToString + ":")
                        Next
                    End If
                    tree_complete = tree_complete.Replace(" ", "")
                    If tree_sdec.Replace("(", "").Length - tree_sdec.Replace(",", "").Length > 1 Then
                        MsgBox("The structure is either an unrooted tree or comprises multiple polytomies")
                        Return 1
                    ElseIf tree_sdec.Replace("(", "").Length - tree_sdec.Replace(",", "").Length = 1 Then
                        Dim tree_poly() As Char = tree_sdec
                        ReDim tree_char(taxon_num * 5)
                        tree_sdec = ""
                        Dim char_id As Integer = 0
                        Dim l_c As Integer = 0
                        Dim r_c As Integer = 0
                        Dim dh As Integer = 0
                        Dim last_symb As Boolean = True
                        For i As Integer = 0 To tree_poly.Length - 1
                            Select Case tree_poly(i)
                                Case "("
                                    char_id += 1
                                    tree_char(char_id) = tree_poly(i)
                                    last_symb = True

                                Case ")"
                                    char_id += 1
                                    tree_char(char_id) = tree_poly(i)
                                    last_symb = True
                                Case ","
                                    char_id += 1
                                    tree_char(char_id) = tree_poly(i)
                                    last_symb = True
                                Case Else
                                    If last_symb Then
                                        char_id += 1
                                        tree_char(char_id) = tree_poly(i)
                                        last_symb = False
                                    Else
                                        tree_char(char_id) += tree_poly(i)
                                    End If
                            End Select
                        Next
                        Dim three_clade_id(2) As Integer
                        three_clade_id(0) = 0
                        three_clade_id(1) = 0
                        three_clade_id(2) = 0
                        For i As Integer = 1 To tree_char.Length - 1
                            If tree_char(i) = "(" Then
                                l_c = l_c + 1
                            End If
                            If tree_char(i) = ")" Then
                                three_clade_id(2) = i
                            End If
                            If tree_char(i) = "," Then
                                dh = dh + 1
                            End If
                            If dh = l_c + 1 Then
                                If three_clade_id(1) = 0 Then
                                    dh = 0
                                    l_c = 0
                                End If
                                three_clade_id(1) = i
                            End If
                        Next
                        If dh <> l_c Then
                            isbase_three = False
                        End If
                        dh = 0
                        l_c = 0
                        For i As Integer = 0 To three_clade_id(2) - 1
                            If tree_char(three_clade_id(2) - i) = ")" Then
                                r_c = r_c + 1
                            End If
                            If tree_char(three_clade_id(2) - i) = "," Then
                                dh = dh + 1
                            End If
                            If dh = r_c + 1 Then
                                If three_clade_id(0) = 0 Then
                                    dh = 0
                                    r_c = 0
                                End If
                                three_clade_id(0) = three_clade_id(2) - i
                            End If
                        Next
                        If dh <> r_c Then
                            isbase_three = False
                        End If
                        dh = 0
                        r_c = 0
                        For i As Integer = 0 To three_clade_id(2) - 1
                            Select Case tree_char(i)
                                Case "("
                                    l_c += 1
                                Case ","
                                    dh = dh + 1
                                Case ")"
                                    r_c += 1
                                Case Else

                            End Select
                            If i = three_clade_id(0) Then
                                If l_c <> dh Or dh <> r_c + 1 Then
                                    isbase_three = False
                                End If
                            End If
                            If i = three_clade_id(1) Then
                                If l_c <> dh - 1 Or dh - 1 <> r_c + 1 Then
                                    isbase_three = False
                                End If
                            End If
                        Next
                        If isbase_three Then
                            If three_clade_id(2) - three_clade_id(1) <= three_clade_id(0) - 1 Then
                                If three_clade_id(2) - three_clade_id(1) <= three_clade_id(1) - three_clade_id(0) Then
                                    tree_sdec = "("
                                    For i As Integer = 1 To three_clade_id(1) - 1
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += "):0,"
                                    For i As Integer = three_clade_id(1) + 1 To three_clade_id(2)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += ";"
                                Else
                                    tree_sdec = "("
                                    For i As Integer = three_clade_id(0) To three_clade_id(1) - 1
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += ","
                                    For i As Integer = 1 To three_clade_id(0)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    For i As Integer = three_clade_id(1) + 1 To three_clade_id(2)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += ");"
                                End If
                            Else
                                If three_clade_id(0) - 1 <= three_clade_id(1) - three_clade_id(0) Then
                                    tree_sdec = ""
                                    For i As Integer = 1 To three_clade_id(0)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += "("
                                    For i As Integer = three_clade_id(0) + 1 To three_clade_id(2)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += ");"
                                Else
                                    tree_sdec = "("
                                    For i As Integer = three_clade_id(0) + 1 To three_clade_id(1) - 1
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += ","
                                    For i As Integer = 1 To three_clade_id(0)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    For i As Integer = three_clade_id(1) + 1 To three_clade_id(2)
                                        tree_sdec = tree_sdec + tree_char(i)
                                    Next
                                    tree_sdec += ");"
                                End If
                            End If
                        Else
                            For i As Integer = 1 To three_clade_id(2)
                                tree_sdec = tree_sdec + tree_char(i)
                            Next
                        End If
                    End If

                    If tree_sdec.EndsWith(";") = False Then
                        tree_sdec = tree_sdec + ";"
                    End If

                    If taxon_line <> "" Then
                        For i As Integer = 1 To dtView.Count
                            If dtView.Item(i - 1).Item(0).ToString <> "" And dtView.Item(i - 1).Item(1).ToString <> "" Then
                                tree_sdec = tree_sdec.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ",", "($%#" + dtView.Item(i - 1).Item(0).ToString + "$%#,")
                                tree_sdec = tree_sdec.Replace("," + dtView.Item(i - 1).Item(1).ToString + ")", ",$%#" + dtView.Item(i - 1).Item(0).ToString + "$%#)")
                                tree_sdec = tree_sdec.Replace("," + dtView.Item(i - 1).Item(1).ToString + ",", ",$%#" + dtView.Item(i - 1).Item(0).ToString + "$%#,")
                                tree_sdec = tree_sdec.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ":", "($%#" + dtView.Item(i - 1).Item(0).ToString + "$%#:")
                                tree_sdec = tree_sdec.Replace("," + dtView.Item(i - 1).Item(1).ToString + ":", ",$%#" + dtView.Item(i - 1).Item(0).ToString + "$%#:")
                            End If
                        Next
                        tree_sdec = tree_sdec.Replace("$%#", "")
                    End If
                    If dtView.Count > 0 Then
                        If f_t_name.Length = dtView.Count * 2 Then
                            For i As Integer = 1 To dtView.Count
                                If f_t_name(i - 1, 0) <> "" And f_t_name(i - 1, 1) <> "" Then
                                    tree_sdec = tree_sdec.Replace("(" + f_t_name(i - 1, 0) + ",", "($%*" + f_t_name(i - 1, 1) + "$%*,")
                                    tree_sdec = tree_sdec.Replace("," + f_t_name(i - 1, 0) + ")", ",$%*" + f_t_name(i - 1, 1) + "$%*)")
                                    tree_sdec = tree_sdec.Replace("," + f_t_name(i - 1, 0) + ",", ",$%*" + f_t_name(i - 1, 1) + "$%*,")
                                    tree_sdec = tree_sdec.Replace("(" + f_t_name(i - 1, 0) + ":", "($%*" + f_t_name(i - 1, 1) + "$%*:")
                                    tree_sdec = tree_sdec.Replace("," + f_t_name(i - 1, 0) + ":", ",$%*" + f_t_name(i - 1, 1) + "$%*:")
                                End If
                            Next
                        End If
                        For i As Integer = 1 To dtView.Count
                            tree_sdec = tree_sdec.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ",", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_sdec = tree_sdec.Replace("," + dtView.Item(i - 1).Item(1).ToString + ")", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)")
                            tree_sdec = tree_sdec.Replace("," + dtView.Item(i - 1).Item(1).ToString + ",", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_sdec = tree_sdec.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ":", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                            tree_sdec = tree_sdec.Replace("," + dtView.Item(i - 1).Item(1).ToString + ":", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                        Next
                        For i As Integer = 1 To dtView.Count
                            tree_sdec = tree_sdec.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "(" + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_sdec = tree_sdec.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)", "," + dtView.Item(i - 1).Item(0).ToString + ")")
                            tree_sdec = tree_sdec.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "," + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_sdec = tree_sdec.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "(" + dtView.Item(i - 1).Item(0).ToString + ":")
                            tree_sdec = tree_sdec.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "," + dtView.Item(i - 1).Item(0).ToString + ":")
                        Next
                    End If
                    tree_sdec = tree_sdec.Replace(" ", "")

                    If tree_complete.Replace("(", "").Length <> tree_complete.Replace(")", "").Length Then
                        MsgBox("Error 10. missing parentheses in tree! Please check you tree file!")
                        Try
                            rt.Close()
                            'B_WT.Close()
                            'P_WT.Close()
                            'name_wt.Close()
                        Catch ex As Exception

                        End Try
                        Return 1
                    End If
                    If tree_sdec.Replace("):", "").Length = tree_sdec.Length - 2 Then
                        tree_sdec = tree_sdec.Replace("):0", ")")
                    End If
                    If tree_complete.Replace("(", "").Length = tree_complete.Replace(",", "").Length Then
                        'wt_clean_num.WriteLine(tree_sdec)
                        'wt_clean_num_P.WriteLine(tree_sdec)
                        'B_WT.WriteLine(tree_complete)
                        tree_mcmctree = tree_complete
                        'P_WT.WriteLine(tree_complete)
                        Tree_Num_B += 1
                    Else
                        'wt_clean_num_P.WriteLine(tree_sdec)
                        'P_WT.WriteLine(tree_complete)
                    End If
                    Tree_Num_P += 1
                    name_count += 1
                    'name_wt.WriteLine(tree_filename + "_" + name_count.ToString)

                End If
                line = rt.ReadLine()
            Loop


            rt.Close()
            'B_WT.Close()
            'P_WT.Close()
            'name_wt.Close()
            'wt_clean_num.Close()
            'wt_clean_num_P.Close()
            If Tree_Num_P = 1 Then
                load_final_trees()
            End If


            Return 0
        Catch ex As Exception
            files_complete = True
            MsgBox(ex.ToString)
            MsgBox("Error 6. Cannot format the trees!")
            Return 1
        End Try
    End Function
    Public Sub Make_Taxon_List()
        If Me.InvokeRequired Then
            Me.Invoke(New MethodInvoker(AddressOf Make_Taxon_List))
        Else
            Dim Temp_list() As String = taxon_line.Replace("(", ",").Replace(")", ",").Replace(";", "").Replace("'", "").Replace("""", "").Split(New Char() {","c})
            Dim temp_Item_Array As Integer = 0
            For Each i As String In Temp_list
                If i <> "" Then
                    dtView.AllowNew = True
                    If (temp_Item_Array = 0 And dtView.Count = 1) = False Then
                        dtView.AddNew()
                    End If
                    dtView.AllowEdit = True
                    Dim newrow(2) As String
                    newrow(0) = dtView.Count
                    newrow(1) = i
                    newrow(2) = ""
                    dtView.Item(temp_Item_Array).Row.ItemArray = newrow
                    temp_Item_Array += 1
                End If
            Next
            taxon_num = dtView.Count
            dtView.AllowNew = False
        End If
    End Sub

    Private Function CleanLine(ByVal line As String) As String
        Dim i As Integer = 0
        While i < line.Length AndAlso (line(i) = " "c OrElse line(i) = vbTab)
            i += 1
        End While
        Return line.Substring(i)
    End Function

    Public Function make_final_trees() As Integer
        Dim has_length As Boolean
        tree_show_with_value = final_tree
        Using rt As New StreamReader(tree_path)
            Try
                Dim line As String = ""
                line = rt.ReadLine
                Dim f_t_name(,) As String
                ReDim f_t_name(dtView.Count, 1)
                Dim Temp_pv As Integer = 0
                Do While line Is Nothing = False
                    line = CleanLine(line)
                    If line.Replace("	", "").ToUpper.StartsWith("TRANSLATE") Then
                        line = rt.ReadLine.Replace("	", " ").Replace(",", "")
                        Dim name_num As Integer = 0
                        Do
                            line = CleanLine(line)
                            Dim TRANSLATE() As String = line.Replace(";", "").Split(New Char() {" "c})
                            f_t_name(name_num, 0) = TRANSLATE(0)
                            f_t_name(name_num, 1) = TRANSLATE(1).Replace("'", "")
                            name_num = name_num + 1
                            line = rt.ReadLine.Replace("	", " ").Replace(",", "")
                        Loop Until line.Contains(";")
                        If line.Replace("	", "").Replace(" ", "").Length > 1 Then
                            line = CleanLine(line)
                            Dim TRANSLATE() As String = line.Replace(";", "").Split(New Char() {" "c})
                            f_t_name(name_num, 0) = TRANSLATE(0)
                            f_t_name(name_num, 1) = TRANSLATE(1).Replace("'", "")
                            name_num = name_num + 1
                            line = rt.ReadLine.Replace("	", " ")
                        End If
                    End If
                    If line.Replace("	", "").Replace(" ", "").ToUpper.StartsWith("TREE") Or line.Replace("	", "").Replace(" ", "").ToUpper.StartsWith("(") Then
                        Do While line.Contains(";") = False
                            Dim next_tree_line As String = rt.ReadLine
                            If next_tree_line <> "" Then
                                line = line + next_tree_line
                            End If
                        Loop
                        Dim tree_Temp1 As String = line.Substring(line.IndexOf("("), line.Length - line.IndexOf("(")).Replace(" ", "")
                        Dim tree_Temp As String = ""
                        Dim tree_complete As String = ""
                        Dim is_sym As Boolean = False
                        Dim Temp_line As String = ""
                        For Each tree_chr As Char In tree_Temp1
                            If tree_chr = "[" Then
                                is_sym = True
                            End If
                            If tree_chr = "]" Then
                                is_sym = False
                                If Temp_line.IndexOf("posterior=") >= 0 Then
                                    Temp_line = Temp_line.Remove(0, Temp_line.IndexOf("posterior="))
                                    If Temp_line.Contains(",") Then
                                        Temp_line = Temp_line.Substring(Temp_line.IndexOf("=") + 1, Temp_line.IndexOf(",") - Temp_line.IndexOf("=") - 1)
                                    Else
                                        Temp_line = Temp_line.Replace("posterior=", "")
                                    End If
                                    tree_Temp = tree_Temp + Val(Temp_line).ToString("F4")
                                End If
                                Try
                                    If Temp_line.IndexOf("label=") >= 0 Then
                                        Temp_line = Temp_line.Remove(0, Temp_line.IndexOf("label="))
                                        If Temp_line.Contains(",") Then
                                            Temp_line = Temp_line.Substring(Temp_line.IndexOf("=") + 1, Temp_line.IndexOf(",") - Temp_line.IndexOf("=") - 1)
                                        Else
                                            Temp_line = Temp_line.Replace("label=", "")
                                        End If
                                        tree_Temp = tree_Temp + Val(Temp_line).ToString("F4")
                                    End If
                                Catch ex As Exception

                                End Try
                            End If
                            If is_sym Then
                                Temp_line = Temp_line + tree_chr
                            End If
                            If is_sym = False And tree_chr <> "]" Then
                                tree_Temp = tree_Temp + tree_chr.ToString
                            End If

                        Next

                        If tree_Temp.IndexOf(":") > 0 Then
                            has_length = True
                        End If


                        For Each tree_chr As Char In tree_Temp
                            If tree_chr = ":" Then
                                is_sym = has_length Xor True
                            End If
                            If tree_chr = "," Or tree_chr = "(" Or tree_chr = ")" Then
                                is_sym = False
                            End If
                            If is_sym = False Then
                                tree_complete = tree_complete + tree_chr.ToString
                            End If
                        Next

                        If tree_complete.Replace("(", "").Length - tree_complete.Replace(",", "").Length = 1 And mrbayes_tree Then
                            Dim tree_poly() As Char = tree_complete
                            tree_complete = ""
                            Dim l_c As Integer = 0
                            Dim dh As Integer = 0
                            Dim adddh As Boolean = True
                            For i As Integer = 0 To tree_poly.Length - 1

                                If tree_poly(i) = "(" Then
                                    l_c = l_c + 1
                                End If
                                If tree_poly(i) = "," Then
                                    dh = dh + 1
                                End If
                                If dh = l_c + 1 And adddh Then
                                    If has_length Then
                                        tree_complete = "(" + tree_complete + ")1:0,"
                                    Else
                                        tree_complete = "(" + tree_complete + ")1,"
                                    End If
                                    i = i + 1
                                    adddh = False
                                End If
                                tree_complete = tree_complete + tree_poly(i)
                            Next
                        End If
                        '定义外类群]
                        tree_complete = tree_complete.Replace("'", "").Replace("""", "")
                        For i As Integer = 1 To dtView.Count
                            If f_t_name(i - 1, 0) <> "" And f_t_name(i - 1, 1) <> "" Then
                                tree_complete = tree_complete.Replace("(" + f_t_name(i - 1, 0) + ",", "($%*" + f_t_name(i - 1, 1) + "$%*,")
                                tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ")", ",$%*" + f_t_name(i - 1, 1) + "$%*)")
                                tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ",", ",$%*" + f_t_name(i - 1, 1) + "$%*,")
                                tree_complete = tree_complete.Replace("(" + f_t_name(i - 1, 0) + ":", "($%*" + f_t_name(i - 1, 1) + "$%*:")
                                tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ":", ",$%*" + f_t_name(i - 1, 1) + "$%*:")
                            End If
                        Next
                        'Dim wt As New StreamWriter(root_path + "temp" + path_char + "v_tree1.tre", False)
                        'wt.WriteLine(tree_complete.Replace("$%*", ""))
                        'wt.Close()
                        If has_length Then
                            For i As Integer = 1 To dtView.Count
                                tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ":", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                                tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ":", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                            Next
                        End If
                        For i As Integer = 1 To dtView.Count
                            tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ",", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ")", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ",", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                        Next
                        If has_length Then
                            For i As Integer = 1 To dtView.Count
                                tree_complete = tree_complete.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "(" + dtView.Item(i - 1).Item(0).ToString + ":")
                                tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "," + dtView.Item(i - 1).Item(0).ToString + ":")
                            Next
                        End If
                        For i As Integer = 1 To dtView.Count
                            tree_complete = tree_complete.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "(" + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)", "," + dtView.Item(i - 1).Item(0).ToString + ")")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "," + dtView.Item(i - 1).Item(0).ToString + ",")
                        Next
                        tree_complete = tree_complete.Replace(" ", "")
                        tree_show_with_value = tree_complete
                        'Dim wt1 As New StreamWriter(root_path + "temp" + path_char + "v_tree2.tre", False)
                        'wt1.WriteLine(tree_complete)
                        'wt1.Close()
                    End If
                    line = rt.ReadLine()
                Loop
            Catch ex As Exception
                'Me.Invoke(RT1, New Object() {"Cannot load node frequency, but feel free to continue your work!" + vbCrLf})
                tree_show_with_value = final_tree
            End Try
        End Using
        File.Delete(tree_path)
        StartTreeView = True
        start_load_tree = True
        Return 0
    End Function
    Public Function load_final_trees() As Integer
        Try
            File.Copy(tree_path, root_path + "temp" + path_char + "T_Tre.tre", True)

            Dim CopyFileInfo As New FileInfo(root_path + "temp" + path_char + "T_Tre.tre") With {
                .Attributes = FileAttributes.Normal
            }

            tree_path = root_path + "temp" + path_char + "T_Tre.tre"
            Dim line As String = ""
            final_tree = ""
            Using rt As New StreamReader(tree_path)
                line = rt.ReadLine
                Dim f_t_name(,) As String
                ReDim f_t_name(dtView.Count - 1, 1)
                Do While line Is Nothing = False

                    Do
                        If line.StartsWith("	") Or line.StartsWith(" ") Then
                            line = line.Remove(0, 1)
                        Else
                            Exit Do
                        End If
                    Loop


                    If line.Replace("	", "").Replace(" ", "").ToUpper.StartsWith("TRANSLATE") Then
                        line = rt.ReadLine.Replace("	", " ").Replace(",", "")
                        Dim name_num As Integer = 0
                        Do

                            If line.Length > 0 Then
                                Do
                                    If line.StartsWith("	") Or line.StartsWith(" ") Then
                                        line = line.Remove(0, 1)
                                    Else
                                        Exit Do
                                    End If
                                Loop
                                Dim TRANSLATE() As String = line.Replace(";", "").Split(New Char() {" "c})
                                f_t_name(name_num, 0) = TRANSLATE(0)
                                f_t_name(name_num, 1) = TRANSLATE(1).Replace("'", "")
                                name_num = name_num + 1
                            End If
                            line = rt.ReadLine.Replace("	", " ").Replace(",", "")
                        Loop Until line.Contains(";")
                        If line.Replace("	", "").Replace(" ", "").Length > 1 Then
                            Do
                                If line.StartsWith("	") Or line.StartsWith(" ") Then
                                    line = line.Remove(0, 1)
                                Else
                                    Exit Do
                                End If
                            Loop
                            Dim TRANSLATE() As String = line.Replace(";", "").Split(New Char() {" "c})
                            f_t_name(name_num, 0) = TRANSLATE(0)
                            f_t_name(name_num, 1) = TRANSLATE(1).Replace("'", "")
                            name_num = name_num + 1
                            line = rt.ReadLine.Replace("	", " ")
                        End If
                    End If

                    If line.Replace("	", "").Replace(" ", "").ToUpper.StartsWith("TREE") Or (line.Replace("	", "").Replace(" ", "").StartsWith("(") And line.Replace("	", "").Replace(" ", "").EndsWith(";")) Then
                        Do While line.Contains(";") = False
                            Dim next_tree_line As String = rt.ReadLine
                            If next_tree_line <> "" Then
                                line = line + next_tree_line
                            End If
                        Loop
                        Dim tree_Temp As String = line.Substring(line.IndexOf("("), line.Length - line.IndexOf("("))
                        Dim tree_Temp1 As String = ""
                        Dim tree_complete As String = ""
                        Dim is_sym As Boolean = False
                        For Each tree_chr As Char In tree_Temp
                            If tree_chr = "[" Then
                                is_sym = True
                            End If
                            If tree_chr = "]" Then
                                is_sym = False
                            End If
                            If is_sym = False And tree_chr <> "]" Then
                                tree_Temp1 = tree_Temp1 + tree_chr.ToString
                            End If
                        Next

                        tree_Temp = tree_Temp1
                        tree_Temp1 = ""
                        For Each tree_chr As Char In tree_Temp
                            If tree_chr = ")" Then
                                is_sym = True
                                tree_Temp1 = tree_Temp1 + tree_chr.ToString
                                tree_chr = ""
                            End If
                            If tree_chr = "," Or tree_chr = "(" Or tree_chr = ")" Or tree_chr = ";" Then
                                is_sym = False
                            End If
                            If is_sym = False Then
                                tree_Temp1 = tree_Temp1 + tree_chr.ToString
                            End If
                        Next
                        For Each tree_chr As Char In tree_Temp1
                            If tree_chr = ":" Then
                                is_sym = True
                            End If
                            If tree_chr = "," Or tree_chr = "(" Or tree_chr = ")" Then
                                is_sym = False
                            End If
                            If is_sym = False And tree_chr.ToString <> " " Then
                                tree_complete = tree_complete + tree_chr.ToString
                            End If
                        Next

                        tree_complete = tree_complete.Replace("""", "").Replace("'", "")
                        For i As Integer = 1 To dtView.Count
                            If f_t_name(i - 1, 0) <> "" And f_t_name(i - 1, 1) <> "" Then
                                tree_complete = tree_complete.Replace("(" + f_t_name(i - 1, 0) + ",", "($%*" + f_t_name(i - 1, 1) + "$%*,")
                                tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ")", ",$%*" + f_t_name(i - 1, 1) + "$%*)")
                                tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ",", ",$%*" + f_t_name(i - 1, 1) + "$%*,")
                                tree_complete = tree_complete.Replace("(" + f_t_name(i - 1, 0) + ":", "($%*" + f_t_name(i - 1, 1) + "$%*:")
                                tree_complete = tree_complete.Replace("," + f_t_name(i - 1, 0) + ":", ",$%*" + f_t_name(i - 1, 1) + "$%*:")
                            End If
                        Next
                        For i As Integer = 1 To dtView.Count
                            tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ",", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ")", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ",", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,")
                            tree_complete = tree_complete.Replace("(" + dtView.Item(i - 1).Item(1).ToString + ":", "($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                            tree_complete = tree_complete.Replace("," + dtView.Item(i - 1).Item(1).ToString + ":", ",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:")
                        Next
                        For i As Integer = 1 To dtView.Count
                            tree_complete = tree_complete.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "(" + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*)", "," + dtView.Item(i - 1).Item(0).ToString + ")")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*,", "," + dtView.Item(i - 1).Item(0).ToString + ",")
                            tree_complete = tree_complete.Replace("($%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "(" + dtView.Item(i - 1).Item(0).ToString + ":")
                            tree_complete = tree_complete.Replace(",$%*" + dtView.Item(i - 1).Item(1).ToString + "$%*:", "," + dtView.Item(i - 1).Item(0).ToString + ":")
                        Next
                        tree_complete = tree_complete.Replace(" ", "")
                        If tree_complete.Replace("(", "").Length <> tree_complete.Replace(")", "").Length Then
                            MsgBox("Error 10. missing parentheses in tree! Please check you tree file!")
                            Try
                                rt.Close()
                            Catch ex As Exception

                            End Try
                            Return 1
                        End If
                        final_tree = tree_complete
                    End If
                    line = rt.ReadLine()
                Loop


                Dim outgroup_str As String = ""
                If final_tree.Replace("(", "").Length - final_tree.Replace(",", "").Length = 1 And mrbayes_tree Then
                    Dim tree_poly() As Char = final_tree
                    final_tree = ""
                    Dim l_c As Integer = 0
                    Dim dh As Integer = 0
                    Dim adddh As Boolean = True
                    For i As Integer = 0 To tree_poly.Length - 1

                        If tree_poly(i) = "(" Then
                            l_c = l_c + 1
                        End If
                        If tree_poly(i) = "," Then
                            dh = dh + 1
                        End If
                        If dh = l_c + 1 And adddh Then
                            final_tree = "(" + final_tree + "),"
                            i = i + 1
                            adddh = False
                        End If
                        If adddh = False Then
                            outgroup_str = outgroup_str + tree_poly(i)
                        End If
                        final_tree = final_tree + tree_poly(i)
                    Next
                    outgroup_str = outgroup_str.Replace("(", "").Replace(")", "").Replace(";", "")
                End If

                If final_tree = "" Then
                    Return 1
                End If
            End Using
            Dim check_tree() As String
            check_tree = final_tree.Replace(";", "").Replace(",", "|").Replace("(", "|").Replace(")", "|").Replace("||", "|").Split(New Char() {"|"c})
            For Each Tempstr As String In check_tree
                If IsNumeric(Tempstr) = False And Tempstr <> "" Then
                    MsgBox("Cannot find " + Tempstr + " in your trees file!")
                    Return 1
                End If
            Next
        Catch ex As Exception
            MsgBox(ex.ToString)
            MsgBox("Error 2! Cannot format the tree!")
            Return 1
        End Try
        Try
            make_final_trees()
        Catch ex As Exception
            MsgBox("Error 7! Wrong condensed tree!")
            Return 1
        End Try
        Return 0
    End Function

    Public Function format_time(ByVal time As String) As String
        Dim temp_time As String = ""
        For Each i As Char In time
            If IsNumeric(i) Then
                temp_time = temp_time + i.ToString
            End If
        Next
        Return temp_time
    End Function

    Dim start_load_tree As Boolean = False
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If start_load_tree Then
            draw_loaded_tree()
            start_load_tree = False
        End If
    End Sub

    Private Sub PictureBox1_DragDrop(sender As Object, e As DragEventArgs) Handles PictureBox1.DragDrop

    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick
        Dim rowIndex As Integer = -1
        If Not IsNothing(DataGridView1.CurrentCell) Then
            rowIndex = DataGridView1.CurrentCell.RowIndex
            ' 如果没有选中的单元格，检查是否有选中的行
        ElseIf DataGridView1.SelectedRows.Count > 0 Then
            rowIndex = DataGridView1.SelectedRows(0).Index
        End If
        If rowIndex >= 0 Then
            Selected_node = rowIndex
            PictureBox1.Width = (max_level + 2) * Branch_length + 2 * Border_separation + (max_taxon_name + RangeStr.Length + 8) * Label_font.SizeInPoints
            PictureBox1.Height = (NumofTaxon + 5) * Taxon_separation + Border_separation + Label_font.Height + Tree_font.Height 'y
            Bitmap_Tree = New Bitmap(CInt(PictureBox1.Width), CInt(PictureBox1.Height))
            draw_tree(Graphics.FromImage(Bitmap_Tree))
            PictureBox1.Refresh()
            form_config_fossil.Show()

        End If

    End Sub


    Private Sub McmctreeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles McmctreeToolStripMenuItem.Click
        If tree_mcmctree <> "" Then
            Dim temp_tree As String = ""
            Dim temp_node_id As Integer = -1
            Dim fossil_count As Integer = 0
            For Each my_char As Char In tree_mcmctree
                temp_tree += my_char
                If my_char = ")" Then
                    temp_node_id += 1
                    Dim temp_date As String = time_view.Item(temp_node_id).Item(2).ToString.Replace("(", "$%*($%*").Replace(")", "$%*)$%*").Replace(",", "$%*,$%*")
                    If temp_date.Contains(",") Then
                        temp_tree += "'" + temp_date + "'"
                        fossil_count += 1
                    End If
                End If
            Next
            If fossil_count = 0 Then
                MsgBox("Date must be specified for at least one branch")
                Exit Sub
            End If
            For i As Integer = 1 To dtView.Count
                If dtView.Item(i - 1).Item(0) <> "" And dtView.Item(i - 1).Item(1) <> "" Then
                    temp_tree = temp_tree.Replace("(" + dtView.Item(i - 1).Item(0) + ",", "($%*" + dtView.Item(i - 1).Item(1) + "$%*,")
                    temp_tree = temp_tree.Replace("," + dtView.Item(i - 1).Item(0) + ")", ",$%*" + dtView.Item(i - 1).Item(1) + "$%*)")
                    temp_tree = temp_tree.Replace("," + dtView.Item(i - 1).Item(0) + ",", ",$%*" + dtView.Item(i - 1).Item(1) + "$%*,")
                    temp_tree = temp_tree.Replace("(" + dtView.Item(i - 1).Item(0) + ":", "($%*" + dtView.Item(i - 1).Item(1) + "$%*:")
                    temp_tree = temp_tree.Replace("," + dtView.Item(i - 1).Item(0) + ":", ",$%*" + dtView.Item(i - 1).Item(1) + "$%*:")
                End If
            Next
            temp_tree = temp_tree.Replace("$%*", "")
            DeleteDir(Path.Combine(form_main.TextBox1.Text, "mcmctree"))
            Directory.CreateDirectory(Path.Combine(form_main.TextBox1.Text, "mcmctree"))
            Using sw As New StreamWriter(Path.Combine(form_main.TextBox1.Text, "mcmctree", "MCMC_intree.tree"))
                sw.WriteLine(NumofTaxon.ToString + " 1")
                sw.WriteLine(temp_tree)
            End Using
            If MenuClicked = "build_tree" Then
                If form_config_tree.RadioButton3.Checked Then
                    form_config_mcmc.TextBox2.Text = Path.Combine(form_main.TextBox1.Text, "combined_results.fasta")
                Else
                    form_config_mcmc.TextBox2.Text = Path.Combine(form_main.TextBox1.Text, "combined_trimed.fasta")
                End If
            Else
                form_config_mcmc.TextBox2.Text = ""
            End If
            form_config_mcmc.Show()
        End If

    End Sub

    Private Sub config_dated_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        If Me.Visible Then
            If MenuClicked = "build_tree" Then
                Selected_node = -1
                time_Dataset.Tables("Time Table").Clear()
                time_Dataset.Tables("Tree Table").Clear()
                Tree_Num_P = 0
                Dim l_Tree As New Thread(AddressOf load_trees) With {
                    .CurrentCulture = ci
                }
                l_Tree.Start()
            End If
        End If
    End Sub
End Class