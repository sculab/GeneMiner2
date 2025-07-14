Imports System.IO
Public Class Tool_TipName
    Public mytree_Dataset As New DataSet
    Public mytree_View As New DataView
    Private Sub LoadTreeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadTreeToolStripMenuItem.Click
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
            safe_delete(Path.Combine(root_path, "temp", "temp_labels.txt"))
            safe_delete(Path.Combine(root_path, "temp", "temp_org.tree"))
            safe_copy(opendialog.FileName, Path.Combine(root_path, "temp", "temp_org.tree"))
            do_newick({Path.Combine(root_path, "temp"), "-getlabels temp_org.tree -output temp_labels.txt"})
            If File.Exists(Path.Combine(root_path, "temp", "temp_labels.txt")) Then
                mytree_Dataset.Tables("Name Table").Clear()
                Using sr As New StreamReader(Path.Combine(root_path, "temp", "temp_labels.txt"))
                    While (Not sr.EndOfStream)
                        Dim newrow(2) As String
                        mytree_View.AllowNew = True
                        mytree_View.AddNew()
                        newrow(0) = mytree_View.Count
                        newrow(1) = sr.ReadLine()
                        newrow(2) = ""
                        mytree_View.Item(mytree_View.Count - 1).Row.ItemArray = newrow
                    End While
                End Using
                DataGridView1.RefreshEdit()

            End If

        End If


    End Sub


    Private Sub Tool_TipName_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim data_table As New System.Data.DataTable With {
            .TableName = "Name Table"
        }
        Dim Column_ID1 As New System.Data.DataColumn("ID", System.Type.GetType("System.Int32"))
        Dim Column_1 As New System.Data.DataColumn("Label_1")
        Dim Column_2 As New System.Data.DataColumn("Label_2")
        data_table.Columns.Add(Column_ID1)
        data_table.Columns.Add(Column_1)
        data_table.Columns.Add(Column_2)

        mytree_Dataset.Tables.Add(data_table)

        mytree_View = mytree_Dataset.Tables("Name Table").DefaultView
        mytree_View.AllowNew = False
        mytree_View.AllowDelete = False
        mytree_View.AllowEdit = True

        Dim Column_Select1 As New DataGridViewCheckBoxColumn With {
            .HeaderText = "Select"
        }
        DataGridView1.Columns.Insert(0, Column_Select1)
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.DataSource = mytree_View
        DataGridView1.Columns(1).ReadOnly = True
        DataGridView1.Columns(2).ReadOnly = True
        DataGridView1.Columns(3).ReadOnly = False

        DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
        DataGridView1.Columns(0).Width = 50
        DataGridView1.Columns(1).Width = 50
        DataGridView1.Columns(2).Width = 200
        DataGridView1.Columns(3).Width = 200
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub SaveNameTableToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveNameTableToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog With {
            .Filter = "CSV File (*.csv)|*.csv;*.CSV",
            .FileName = "",
            .DefaultExt = ".csv",
            .CheckFileExists = False,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            If opendialog.FileName.ToLower.EndsWith(".csv") Then
                Dim dw As New StreamWriter(opendialog.FileName, False)
                Dim state_line As String = "ID,Label_1"
                For j As Integer = 3 To DataGridView1.ColumnCount - 1
                    state_line += "," + DataGridView1.Columns(j).HeaderText
                Next
                dw.WriteLine(state_line)
                For i As Integer = 1 To mytree_View.Count
                    state_line = i.ToString
                    For j As Integer = 2 To DataGridView1.ColumnCount - 1
                        state_line += "," + mytree_View.Item(i - 1).Item(j - 1)
                    Next
                    dw.WriteLine(state_line)
                Next
                dw.Close()
            End If
        End If
    End Sub

    Private Sub LoadNameTableToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadNameTableToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "CSV File (*.csv)|*.csv;*.CSV|ALL Files(*.*)|*.*",
            .FileName = "",
            .Multiselect = False,
            .DefaultExt = ".csv",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            DataGridView1.EndEdit()
            mytree_Dataset.Tables("Name Table").Clear()
            'DataGridView1.DataSource = Nothing
            Using sr As New StreamReader(opendialog.FileName)
                sr.ReadLine()
                While (Not sr.EndOfStream)
                    Dim newrow() As String = sr.ReadLine().Split(",")
                    ReDim Preserve newrow(2)
                    mytree_View.AllowNew = True
                    mytree_View.AddNew()
                    mytree_View.Item(mytree_View.Count - 1).Row.ItemArray = newrow
                End While
            End Using
            'DataGridView1.DataSource = mytree_View
            DataGridView1.RefreshEdit()
        End If
    End Sub

    Private Sub SaveTreeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveTreeToolStripMenuItem.Click
        Using sr As New StreamReader(Path.Combine(root_path, "temp", "temp_org.tree"))
            Dim tree_complete As String = sr.ReadToEnd
            For i As Integer = 1 To mytree_View.Count
                tree_complete = tree_complete.Replace("(" + mytree_View.Item(i - 1).Item(1).ToString + ",", "($%#" + mytree_View.Item(i - 1).Item(2).ToString + "$%#,")
                tree_complete = tree_complete.Replace("," + mytree_View.Item(i - 1).Item(1).ToString + ")", ",$%#" + mytree_View.Item(i - 1).Item(2).ToString + "$%#)")
                tree_complete = tree_complete.Replace("," + mytree_View.Item(i - 1).Item(1).ToString + ",", ",$%#" + mytree_View.Item(i - 1).Item(2).ToString + "$%#,")
                tree_complete = tree_complete.Replace("(" + mytree_View.Item(i - 1).Item(1).ToString + ":", "($%#" + mytree_View.Item(i - 1).Item(2).ToString + "$%#:")
                tree_complete = tree_complete.Replace("," + mytree_View.Item(i - 1).Item(1).ToString + ":", ",$%#" + mytree_View.Item(i - 1).Item(2).ToString + "$%#:")
            Next
            tree_complete = tree_complete.Replace("$%#", "")
            Using sw As New StreamWriter(Path.Combine(root_path, "temp", "relabeled_tree.tree"))
                sw.Write(tree_complete)
            End Using
        End Using

        Dim opendialog As New SaveFileDialog With {
            .Filter = "Tree File (*.tree)|*.tree;*.TREE",
            .FileName = "",
            .DefaultExt = ".tree",
            .CheckFileExists = False,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            safe_copy(Path.Combine(root_path, "temp", "relabeled_tree.tree"), opendialog.FileName)
        End If
    End Sub
End Class