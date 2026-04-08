try:
    from cython import uchar as NativeChar, int as NativeInt
except ImportError:
    NativeChar = NativeInt = int

def collect_runs_stats(read, kmer_dict, kmer_size):
    # Return values: [0:4]=best length; [4:8]=run count; [8:12]=hit count; [12]=k-mer count
    i: NativeInt
    mask_bin = (1 << (kmer_size << 1)) - 1
    kmer_cnt: NativeInt = len(read) - kmer_size + 1
    results: NativeInt[13] = [0] * 13

    if kmer_cnt > 0:
        curr_dir: NativeInt = 0
        curr_len: NativeInt = 0
        read_int = int(read, 4)
        results[12] = kmer_cnt

        for i in range(0, kmer_cnt):
            kmer = read_int & mask_bin
            orient: NativeInt = 0
            read_int >>= 2

            if kmer in kmer_dict:
                orient = kmer_dict[kmer]

            if orient != curr_dir:
                if curr_len > results[curr_dir]:
                    results[curr_dir] = curr_len

                results[curr_dir + 4] += 1
                curr_dir = orient
                curr_len = 0

            if curr_dir != 0:
                curr_len += 1
                results[curr_dir + 8] += 1

        if curr_len > results[curr_dir]:
            results[curr_dir] = curr_len

        results[curr_dir + 4] += 1
    else:
        results[12] = 0

    return results

def filter_read(read, kmer_dict, kmer_size):
    if len(read) < kmer_size:
        return False

    mask_bin = (1 << (kmer_size << 1)) - 1
    read_int = int(read, 4)

    for i in range(0, len(read) - kmer_size + 1):
        if ((read_int >> (2 * i)) & mask_bin) in kmer_dict:
            return True

    return False

def parse_record(record: bytes, has_phr: bool, seq_buf: bytearray, phr_buf: bytearray, seq_len_r: int):
    i: NativeInt = 0
    j: NativeInt = 0
    k: NativeInt
    last_chunk: NativeChar = 0
    seq_len: NativeInt = seq_len_r

    while i < seq_len:
        delta: NativeChar = record[j] & 0x7f
        chunk: NativeChar = delta ^ last_chunk
        rep_num: NativeChar = ((chunk >> 5) & 3) + 1
        for k in range(rep_num):
            seq_buf[i + k] = (chunk & 31) + 64
        i += rep_num
        last_chunk = chunk
        if record[j] >> 7:
            chunk = delta ^ last_chunk
            rep_num = ((chunk >> 5) & 3) + 1
            for k in range(rep_num):
                seq_buf[i + k] = (chunk & 31) + 64
            i += rep_num
            last_chunk = chunk
        j += 1

    if not has_phr:
        return

    i = 0

    while i < seq_len:
        chunk = record[j]
        if chunk & 0x80:
            chunk2: NativeChar = record[j + 1]
            rep_num = ((chunk & 0x7f) | (chunk2 & 0x80)) + 1
            for k in range(rep_num):
                phr_buf[i + k] = chunk2 & 0x7f
            i += rep_num
            j += 2
        else:
            phr_buf[i] = chunk
            i += 1
            j += 1
