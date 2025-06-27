from Bio.SeqIO.FastaIO import SimpleFastaParser
import argparse
import os

# We try to find all biconnected components as clusters.
# Simply speaking, a site must be non-gap in at least three species
# to give phylogenetic information. A bridge edge implies only two
# species with good sequence similarity at a given locus. Therefore,
# we remove those edges and group similar species together to remove
# possibly incorrect alignments.
def find_bridges(adj):
    n        = len(adj)
    dfn      = [-1] * n
    low      = [-1] * n
    time     = 0
    bridges  = set()

    def dfs(u, parent):
        nonlocal time
        time += 1
        dfn[u] = low[u] = time
        for v in adj[u]:
            if dfn[v] == -1:            # Tree edge
                dfs(v, u)
                low[u] = min(low[u], low[v])
                if low[v] > dfn[u]:     # Bridge edge
                    bridges.add((u, v) if u < v else (v, u))
            elif v != parent:           # Back edge or cross edge
                low[u] = min(low[u], dfn[v])

    for s in range(n):
        if dfn[s] == -1:
            dfs(s, -1)

    return bridges

def find_parent(p, x):
    while p[x] != x:
        x, p[x] = p[x], p[p[x]]
    return x

def merge_sets(p, r, x, y):
    px = find_parent(p, x)
    py = find_parent(p, y)

    if px == py:
        return

    if r[px] < r[py]:
        p[px] = py
    elif r[px] > r[py]:
        p[py] = px
    else:
        p[py] = px
        r[px] += 1

def find_bcc(adj):
    bridges = find_bridges(adj)
    n       = len(adj)
    parent  = list(range(n))
    rank    = [0] * n

    for u in range(n):
        for v in adj[u]:
            if u < v and (u, v) not in bridges:
                merge_sets(parent, rank, u, v)

    comps_dict = {}

    for v in range(n):
        comps_dict.setdefault(find_parent(parent, v), []).append(v)

    return sorted(comps_dict.values(), key=lambda x: len(x), reverse=True)

def clean_file(gene_path, min_number, max_difference):
    with open(gene_path, 'r') as f:
        seq_list = list(SimpleFastaParser(f))

    seq_count = len(seq_list)

    if seq_count <= 1:
        os.remove(gene_path)
        return

    adjacency_list  = [set() for _ in range(seq_count)]
    identity_thresh = 1 - max_difference
    min_number      = max(min_number, 2)

    for i in range(seq_count - 1):
        for j in range(i + 1, seq_count):
            seq_i = seq_list[i][1].upper()
            seq_j = seq_list[j][1].upper()

            nuc_cmp = [ci == cj
                       for ci, cj in zip(seq_i, seq_j)
                       if ci not in '-?' and cj not in '-?']

            identity_len = sum(nuc_cmp)
            overlap_len  = len(nuc_cmp)

            # We need at least 7 overlapping bases because 4^-7 < 1e-5.
            # While we cannot afford to do multiple comparison correction here,
            # it ensures very few spurious edges.
            identity_pct = identity_len / overlap_len if overlap_len > 6 else 0

            if identity_pct >= identity_thresh:
                adjacency_list[i].add(j)
                adjacency_list[j].add(i)

    bcc_list = []
    out_mask = [True] * seq_count

    for bcc in find_bcc(adjacency_list):
        if len(bcc) < min_number:
            for i in bcc:
                out_mask[i] = False
        else:
            bcc_list.append(bcc)

    if not bcc_list:
        os.remove(gene_path)
        return

    gap_seq = '-' * len(seq_list[0][1])
    out_lst = [''] * seq_count

    for bcc in bcc_list:
        for i, _ in enumerate(out_lst):
            if out_mask[i]:
                out_lst[i] += seq_list[i][1] if i in bcc else gap_seq

    with open(gene_path, 'w') as f:
        f.writelines(f'>{name}\n{out_lst[i]}\n' for i, (name, seq) in enumerate(seq_list) if out_mask[i])

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Clean up an alignment")
    parser.add_argument("-f", required=True, help="File to process")
    parser.add_argument("-n", default=1, help="Minimum number of sequences", type=int)
    parser.add_argument("-p", default=1, help="Maximum fraction of differences", type=float)

    args = parser.parse_args()

    clean_file(args.f, args.n, args.p)
