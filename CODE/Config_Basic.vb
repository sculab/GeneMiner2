Public Class Config_Basic
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        NumericUpDown6.Enabled = CheckBox1.Checked Xor False
        NumericUpDown7.Enabled = CheckBox1.Checked Xor False
        NumericUpDown5.Enabled = CheckBox1.Checked Xor True
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        NumericUpDown3.Enabled = CheckBox3.Checked Xor False
    End Sub

    Public Event ConfirmClicked()
    Public Event CancelClicked()

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If CheckBox4.Checked Then
            no_window = "1"
        Else
            no_window = "0"
        End If
        k1 = NumericUpDown1.Value.ToString
        k2 = NumericUpDown5.Value.ToString
        Me.Hide()
        RaiseEvent ConfirmClicked()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        RaiseEvent CancelClicked()
    End Sub

    Private Sub Config_Basic_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If ComboBox1.SelectedIndex < 0 Then
            ComboBox1.SelectedIndex = 0
        End If
    End Sub
    Private Sub NumericUpDown3_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown3.GotFocus
        If language = "EN" Then
            TextBox1.Text = "Activating this option will limit the analysis to a portion of the sequencing data (M reads). When analyzing high-copy number genes, such as organellar genes, it is unnecessary to utilize the entire dataset. Engaging this option can significantly enhance the speed of the analysis. However, in cases of low sequencing depth or when dealing with low-copy number genes, it is advisable to deactivate this option."
        Else
            TextBox1.Text = "启用此选项将只使用测序数据的一部分（单位为M reads）。在分析高拷贝数基因（如细胞器基因）时，无需使用整个数据集。启用此选项可以显著提高分析速度。但在测序深度低或处理低拷贝数基因的情况下，建议关闭此选项。"
        End If
    End Sub

    Private Sub NumericUpDown1_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown1.GotFocus
        If language = "EN" Then
            TextBox1.Text = "The Filter K Value (kf) depends on three factors: acquisition rate (p) of gene-specific reads, read length (r), and variation degree (v) between reference and target sequences. Typically, with p at 0.99, r at 150, and v at 0.1, kf is around 31, our default setting. If p is 0.95 and v increases to 0.2 (r stays at 150), kf drops to 17, the proposed minimum threshold."
        Else
            TextBox1.Text = "过滤K值（kf）的设定取决于三个因素：目标基因相关reads的获取率（p）、reads的长度（r）以及参考与目标序列之间的变异程度（v）。通常，当p为0.99，r为150，v为0.1时，kf约为31，这是我们的默认设置。如果p为0.95且v增加到0.2（r保持在150），kf降至17，这是建议的最低阈值。"
        End If
    End Sub

    Private Sub CheckBox2_MouseHover(sender As Object, e As EventArgs) Handles CheckBox2.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Enabling this option stores the majority of the data in memory instead of computing it in real-time. This approach can nearly double the processing speed compared to when the option is disabled. However, it is important to note that the RAM usage will also increase substantially, almost doubling in capacity."
        Else
            TextBox1.Text = "启用此选项将大部分数据存储在内存中，而不是实时计算。相对于禁用该选项可以使处理速度几乎加倍，但RAM的使用量也会加倍。"
        End If
    End Sub



    Private Sub CheckBox1_MouseHover(sender As Object, e As EventArgs) Handles CheckBox1.MouseHover
        If language = "EN" Then
            TextBox1.Text = "Selecting this option enables the program to automatically estimate the largest feasible assembly K-mer (ka) value based on the degree of overlap between reads and the reference sequence. A larger ka value can enhance the accuracy of the results, but may compromise their completeness. Activating this option will notably decrease the speed of the assembly process."
        Else
            TextBox1.Text = "选择此选项允许程序根据reads与参考序列之间的重叠程度自动估算最大可行的组装K-mer（ka）值。较大的ka值可以提高结果的准确性，但可能会影响结果完整性。激活此选项将显著降低组装过程的速度。"
        End If
    End Sub

    Private Sub NumericUpDown2_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown2.GotFocus
        If language = "EN" Then
            TextBox1.Text = "Sets the interval of base numbers in the filtering process. Increasing this value can speed up the filtering process but may also reduce the recovery rate of reads. In cases of low sequencing depth or significant differences between the reference and target sequences, it is recommended to reduce this value to 1; otherwise, keep the default setting."
        Else
            TextBox1.Text = "设置过滤过程的间隔碱基数。增加此值可以加速过滤过程，但也会降低reads的恢复率。在测序深度很低或参考序列与目标序列差异较大的情况下，建议将此值减少到1，否则保留默认设置。"
        End If
    End Sub

    Private Sub NumericUpDown4_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown4.GotFocus
        If language = "EN" Then
            TextBox1.Text = "When the software processes a large amount of filtering data, it initiates a secondary filtering process aimed at enhancing the efficiency and accuracy of sequence assembly. GeneMiner increases the filtering K value (kf) by 2 each time when the product of read length and read count divided by the average reference length exceeds this value, with an upper limit of 63."
        Else
            TextBox1.Text = "当软件处理大量的过滤数据时，它会启动二次过滤过程。这个过程旨在提高序列组装的效率和精度。当读取长度和读取次数的乘积除以参考平均长度超过此值时，GeneMiner会每次将过滤K值（kf）增加2，上限为63。"
        End If
    End Sub

    Private Sub NumericUpDown9_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown9.GotFocus
        If language = "EN" Then
            TextBox1.Text = "When the software processes a large amount of filtering data, it initiates a secondary filtering process aimed at enhancing the efficiency and accuracy of sequence assembly. GeneMiner increases the filtering K value (kf) by 2 each time when the size of the filtering file exceeds this value (measured in MB), with an upper limit of 63."
        Else
            TextBox1.Text = "当软件处理大量的过滤数据时，它会启动二次过滤过程。这个过程旨在提高序列组装的效率和精度。当过滤文件的大小超过此值时（以MB计量），GeneMiner会每次将过滤K值（kf）增加2，上限为63。"
        End If
    End Sub

    Private Sub NumericUpDown6_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown6.GotFocus
        If language = "EN" Then
            TextBox1.Text = "Sets the starting value for the automatic estimation process of the assembly K-mer (ka)."
        Else
            TextBox1.Text = "设定自动估算组装K-mer（ka）过程的起始值。"
        End If
    End Sub

    Private Sub NumericUpDown7_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown7.GotFocus
        If language = "EN" Then
            TextBox1.Text = "Sets the terminal value for the automatic estimation process of the assembly K-mer (ka)."
        Else
            TextBox1.Text = "设定自动估算组装K-mer（ka）过程的终止值。"
        End If
    End Sub



    Private Sub NumericUpDown5_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown5.GotFocus
        If language = "EN" Then
            TextBox1.Text = "Utilize a fixed assembly K-mer (ka) value for sequence assembly."
        Else
            TextBox1.Text = "使用固定的K-mer（ka）值进行序列组装。"
        End If
    End Sub

    Private Sub NumericUpDown8_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown8.GotFocus
        If language = "EN" Then
            TextBox1.Text = "Within the generated Kmers from reads, only Kmers with a count exceeding this specified value will be used for sequence assembly."
        Else
            TextBox1.Text = "在从reads生成的Kmers中，只有计数超过此指定值的Kmers将用于序列组装。"
        End If
    End Sub

    Private Sub ComboBox1_GotFocus(sender As Object, e As EventArgs) Handles ComboBox1.GotFocus
        If language = "EN" Then
            TextBox1.Text = "In GeneMiner, the soft boundary is the maximum extension outside sequence edges during target sequence recovery. 'Auto' limits this to half the read length. Setting it to '0' disables it, while 'Unlimited' imposes no boundary length restriction."
        Else
            TextBox1.Text = "在GeneMiner中，软边界是目标序列恢复期间序列边缘之外的最大扩展。'自动'将此限制为reads长度的一半。将其设置为'0'将禁用它，而'无限'不会施加边界长度限制。"
        End If
    End Sub

    Private Sub NumericUpDown10_GotFocus(sender As Object, e As EventArgs) Handles NumericUpDown10.GotFocus
        If language = "EN" Then
            TextBox1.Text = "This option specifies the maximum number of distinct candidate sequences retained during assembly, which typically requires no modification. For target sequences exceeding 5k in length, a higher value may be set."
        Else
            TextBox1.Text = "此选项指定在组装期间保留的候选序列的最大数量，通常无需修改。对于长度超过5k的目标序列，可以设置更高的值。"
        End If
    End Sub

End Class