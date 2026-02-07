namespace GeneSort.Component.Mp.Sortable

open System
open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sortable
open MessagePack
open System.Runtime.Intrinsics


/// DTO for the full SIMD-optimized test suite
[<MessagePackObject>]
type sortableUint8v256TestDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SortingWidth: int
    [<Key(2)>] Blocks: simdSortBlockDto[]
}

module SortableUint8v256TestDto =

    let private blockFromDomain (block: SortBlockUint8v256) : simdSortBlockDto =
        let raw = 
            block.Vectors 
            |> Array.map (fun v -> 
                let buf = Array.zeroCreate<byte> 32
                v.CopyTo(buf.AsSpan())
                buf)
        { RawVectors = raw; SortableCount = block.SortableCount }

    let private blockToDomain (dto: simdSortBlockDto) : SortBlockUint8v256 =
        let vecs = 
            dto.RawVectors 
            |> Array.map (fun bytes -> Vector256.Create<byte>(bytes))
        
        SortBlockUint8v256.createFromVectors vecs dto.SortableCount

    let fromDomain (test: sortableUint8v256Test) : sortableUint8v256TestDto =
        { Id = %test.Id
          SortingWidth = %test.SortingWidth
          Blocks = test.SimdSortBlocks |> Array.map blockFromDomain }

    let toDomain (dto: sortableUint8v256TestDto) : sortableUint8v256Test =
        let id = UMX.tag<sorterTestId> dto.Id
        let sw = UMX.tag<sortingWidth> dto.SortingWidth
        let blocks = dto.Blocks |> Array.map blockToDomain
        sortableUint8v256Test.create id sw blocks