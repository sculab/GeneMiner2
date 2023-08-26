Module Module_Var
#Const TargetOS = "win32"
#If TargetOS = "linux" Then
    Public TargetOS As String = "linux"
#ElseIf TargetOS = "macos" Then
    Public TargetOS As String = "macos"
#ElseIf TargetOS = "win32" Then
    Public TargetOS As String = "win32"
#End If
    Public settings As Dictionary(Of String, String)
    Public currentDirectory As String
    Public refsView As New DataView
    Public seqsView As New DataView
    Public ci As Globalization.CultureInfo = New Globalization.CultureInfo("en-us")
    Public path_char As String
    Public root_path As String
    Public lib_path As String
    Public Dec_Sym As String
    Public timer_id As Integer = 0
    Public waiting As Boolean = False
    Public current_file As String
    Public mydata_Dataset As New DataSet
    'Public form_config_stand As New Config_Stand
    'Public form_config_clean As New Config_Clean
    'Public form_config_mix As New Config_Mix
    'Public form_config_data As New Config_Data
    Public form_config_align As New Config_Align
    Public form_config_plasty As New Config_Plasty
    Public form_config_ags As New Config_AGS
    Public form_config_split As New Config_Split
    'Public form_config_combine As New Config_Combine
    Public form_main As New Main_Form
    Public PB_value As Integer = 0
    Public info_text As String = ""
    Public language As String = "CH"
    Public exe_mode As String = "pro"
    Public fasta_seq() As String
    Public add_data As Boolean = False
    Public data_type As String = "fas"

    Public data_loaded As Boolean = False
    Public reads_length As Integer = 0
    Public ref_dir, out_dir, q1, q2, k1, k2 As String
End Module
