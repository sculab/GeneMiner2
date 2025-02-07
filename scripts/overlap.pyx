import numpy as np
cimport numpy as np

def calculate_overlap(np.ndarray[np.uint32_t, ndim=1] long_dna_array, np.ndarray[np.uint32_t, ndim=2] short_dnas_array, int min_overlap, int step, int max_overlap_limit):
    cdef int max_overlap = min_overlap
    cdef int offset, start_i, end_i, i, j, overlap, num_short_dnas, len_short_dna, len_long_dna

    num_short_dnas = short_dnas_array.shape[0]
    len_short_dna = short_dnas_array.shape[1]
    len_long_dna = len(long_dna_array)

    for j in range(num_short_dnas):
        for offset in range(1 - len_short_dna, len_long_dna - min_overlap, step):
            start_i = 0
            end_i = len_short_dna

            if offset < 0:
                start_i = -offset

            if offset + len_short_dna > len_long_dna:
                end_i = len_long_dna - offset

            if end_i - start_i <= max_overlap:
                continue

            overlap = 0

            for i in range(start_i, end_i):
                if long_dna_array[i + offset] == short_dnas_array[j, i]:
                    overlap += 1
                    if overlap > max_overlap:
                        max_overlap = overlap
                        if max_overlap >= max_overlap_limit:
                            return max_overlap
                else:
                    overlap = 0

    return max_overlap if max_overlap > min_overlap else 0
