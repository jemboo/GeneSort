namespace GeneSort.Sorting.Mp.Sortable

open System
open System.Runtime.Intrinsics
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

/// DTO for an individual bit-packed SIMD block
type sortBlockBitv512Dto = {
    /// Each Vector512<uint64> becomes an 8-element uint64 array
    RawVectors: uint64[][] 
    SortableCount: int
}

type sortableBitv512TestsDto = {
    Id: Guid
    ChannelCount: int
    Blocks: sortBlockBitv512Dto[]
}

module SortableBitv512TestsDto =

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

    let fromDomain (test: sortableBitv512Tests) : sortableBitv512TestsDto =
        { Id = %test.Id
          ChannelCount = %test.ChannelCount
          Blocks = test.SimdSortBlocks |> Array.map blockFromDomain }

    let toDomain (dto: sortableBitv512TestsDto) : sortableBitv512Tests =
        let id = UMX.tag<sortableTestsId> dto.Id
        let sw = UMX.tag<channelCount> dto.ChannelCount
        let blocks = dto.Blocks |> Array.map blockToDomain
        sortableBitv512Tests.create id sw blocks