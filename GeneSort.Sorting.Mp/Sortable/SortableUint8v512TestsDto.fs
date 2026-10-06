namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open System.Runtime.Intrinsics

type sortableUint8v512TestsDto = {
    Id: Guid
    SortingWidth: int
    Blocks: simdSortBlockDto[]
}

module SortableUint8v512TestsDto =

    // --- Helper for simd512SortBlock ---

    let private blockFromDomain (block: sortBlockUint8v512) : simdSortBlockDto =
        let raw = 
            block.Vectors 
            |> Array.map (fun v -> 
                let buf = Array.zeroCreate<byte> 64
                v.CopyTo(buf.AsSpan())
                buf)
        { RawVectors = raw; SortableCount = block.SortableCount }

    let private blockToDomain (dto: simdSortBlockDto) : sortBlockUint8v512 =
        let vecs = 
            dto.RawVectors 
            |> Array.map (fun bytes -> Vector512.Create<byte>(ReadOnlySpan(bytes)))
        
        sortBlockUint8v512.createFromVectors vecs dto.SortableCount

    // --- Main DTO Logic ---

    let fromDomain (test: sortableUint8v512Tests) : sortableUint8v512TestsDto =
        { Id = %test.Id
          SortingWidth = %test.SortingWidth
          Blocks = test.SimdSortBlocks |> Array.map blockFromDomain }

    let toDomain (dto: sortableUint8v512TestsDto) : sortableUint8v512Tests =
        let id = UMX.tag<sortableTestsId> dto.Id
        let sw = UMX.tag<sortingWidth> dto.SortingWidth
        let blocks = dto.Blocks |> Array.map blockToDomain
        sortableUint8v512Tests.create id sw blocks