namespace GeneSort.Sorting.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open MessagePack
open System.Runtime.Intrinsics

[<MessagePackObject>]
type sortableUint8v512TestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] Blocks: simdSortBlockDto[]
}

module SortableUint8v512TestDto =

    // --- Helper for simd512SortBlock ---

    let private blockFromDomain (block: SortBlockUint8v512) : simdSortBlockDto =
        let raw = 
            block.Vectors 
            |> Array.map (fun v -> 
                let buf = Array.zeroCreate<byte> 64
                v.CopyTo(buf.AsSpan())
                buf)
        { RawVectors = raw; SortableCount = block.SortableCount }

    let private blockToDomain (dto: simdSortBlockDto) : SortBlockUint8v512 =
        let vecs = 
            dto.RawVectors 
            |> Array.map (fun bytes -> Vector512.Create<byte>(ReadOnlySpan(bytes)))
        
        SortBlockUint8v512.createFromVectors vecs dto.SortableCount

    // --- Main DTO Logic ---

    let fromDomain (test: sortableUint8v512Test) : sortableUint8v512TestDto =
        { Id = %test.Id
          SortingWidth = %test.SortingWidth
          Blocks = test.SimdSortBlocks |> Array.map blockFromDomain }

    let toDomain (dto: sortableUint8v512TestDto) : sortableUint8v512Test =
        let id = UMX.tag<sorterTestId> dto.Id
        let sw = UMX.tag<sortingWidth> dto.SortingWidth
        let blocks = dto.Blocks |> Array.map blockToDomain
        sortableUint8v512Test.create id sw blocks