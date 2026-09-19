namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open System.Runtime.Intrinsics

/// DTO for an individual bit-packed SIMD block
type sortBlockBitv512Dto = {
    /// Each Vector512<uint64> becomes an 8-element uint64 array
    RawVectors: uint64[][] 
    SortableCount: int
}

type sortableBitv512TestDto = {
    Id: Guid
    SortingWidth: int
    Blocks: sortBlockBitv512Dto[]
}

module SortableBitv512TestDto =

    // --- Helper for sortBlockBitv512 ---

    let private blockFromDomain (block: sortBlockBitv512) : sortBlockBitv512Dto =
        let raw = 
            block.Vectors 
            |> Array.map (fun v -> 
                let buf = Array.zeroCreate<uint64> 8
                let localV = v // Local copy to avoid FS0406 struct-field access issues
                localV.CopyTo(buf.AsSpan())
                buf)
        { RawVectors = raw; SortableCount = block.SortableCount }

    let private blockToDomain (dto: sortBlockBitv512Dto) : sortBlockBitv512 =
        let vecs = 
            dto.RawVectors 
            |> Array.map (fun u64s -> 
                if u64s.Length <> 8 then
                    invalidArg "dto.RawVectors" "Each bit-vector must contain exactly 8 uint64 elements."
                Vector512.Create<uint64>(ReadOnlySpan(u64s)))
        
        sortBlockBitv512.createFromVectors vecs dto.SortableCount

    // --- Main DTO Logic ---

    let fromDomain (test: sortableBitv512Test) : sortableBitv512TestDto =
        { Id = %test.Id
          SortingWidth = %test.SortingWidth
          Blocks = test.SimdSortBlocks |> Array.map blockFromDomain }

    let toDomain (dto: sortableBitv512TestDto) : sortableBitv512Test =
        let id = UMX.tag<sortableTestId> dto.Id
        let sw = UMX.tag<sortingWidth> dto.SortingWidth
        let blocks = dto.Blocks |> Array.map blockToDomain
        sortableBitv512Test.create id sw blocks