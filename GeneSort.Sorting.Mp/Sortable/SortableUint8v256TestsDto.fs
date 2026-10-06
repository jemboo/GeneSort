namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open System.Runtime.Intrinsics

/// DTO for the full SIMD-optimized test suite
type sortableUint8v256TestsDto = {
    Id: Guid
    SortingWidth: int
    Blocks: simdSortBlockDto[]
}

module SortableUint8v256TestsDto =

    let private blockFromDomain (block: sortBlockUint8v256) : simdSortBlockDto =
        let raw = 
            block.Vectors 
            |> Array.map (fun v -> 
                let buf = Array.zeroCreate<byte> 32
                v.CopyTo(buf.AsSpan())
                buf)
        { RawVectors = raw; SortableCount = block.SortableCount }

    let private blockToDomain (dto: simdSortBlockDto) : sortBlockUint8v256 =
        let vecs = 
            dto.RawVectors 
            |> Array.map (fun bytes -> Vector256.Create<byte>(bytes))
        
        sortBlockUint8v256.createFromVectors vecs dto.SortableCount

    let fromDomain (test: sortableUint8v256Tests) : sortableUint8v256TestsDto =
        { Id = %test.Id
          SortingWidth = %test.SortingWidth
          Blocks = test.SimdSortBlocks |> Array.map blockFromDomain }

    let toDomain (dto: sortableUint8v256TestsDto) : sortableUint8v256Tests =
        let id = UMX.tag<sortableTestsId> dto.Id
        let sw = UMX.tag<sortingWidth> dto.SortingWidth
        let blocks = dto.Blocks |> Array.map blockToDomain
        sortableUint8v256Tests.create id sw blocks