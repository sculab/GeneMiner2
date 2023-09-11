import numpy as np
cimport numpy as np

def calculate_overlap(np.ndarray[np.uint32_t, ndim=1] long_dna_array, np.ndarray[np.uint32_t, ndim=2] short_dnas_array, int min_overlap, int step, int max_overlap_limit):
    cdef int max_overlap = 0
    cdef int offset, i, j, overlap, num_short_dnas

    num_short_dnas = short_dnas_array.shape[0]

    for offset in range(1 - short_dnas_array.shape[1], len(long_dna_array), step):
        for j in range(num_short_dnas):
            overlap = 0
            for i in range(short_dnas_array.shape[1]):
                if 0 <= i + offset < len(long_dna_array) and long_dna_array[i + offset] == short_dnas_array[j, i]:
                    overlap += 1
                    if overlap >= min_overlap:
                        max_overlap = max(max_overlap, overlap)
                        if max_overlap >= max_overlap_limit:
                            return max_overlap
                else:
                    overlap = 0

    return max_overlap if max_overlap > min_overlap else 0

def calculate_overlap_wrapper(args):
    return calculate_overlap(*args)
