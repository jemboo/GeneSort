namespace GeneSort.Sorting.Mp.Sortable

/// DTO for an individual SIMD block
type simdSortBlockDto = {
    /// Vectors flattened: each Vector256<uint8> becomes a 32-byte array
    RawVectors: byte[][] 
    SortableCount: int
}