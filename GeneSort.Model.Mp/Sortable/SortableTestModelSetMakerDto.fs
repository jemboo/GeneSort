namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable



[<MessagePackObject>]
type sortableTestModelSetMakerDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModelGenDto: sortableTestModelGenDto
    [<Key(2)>] FirstIndex: int
    [<Key(3)>] Count: int
}

module SortableTestModelSetMakerDto =

    let fromDomain (maker: sortableTestModelSetMaker) : sortableTestModelSetMakerDto =
        { Id = %maker.Id
          SorterTestModelGenDto = SortableTestModelGenDto.fromDomain maker.SorterTestModelGen
          FirstIndex = int maker.FirstIndex
          Count = int maker.Count }

    let toDomain (dto: sortableTestModelSetMakerDto) : sortableTestModelSetMaker =
        if dto.FirstIndex < 0 then
            invalidArg "FirstIndex" "First index must be non-negative."
        if dto.Count < 0 then
            invalidArg "Count" "Count must be non-negative."
        let sorterTestModelGen = SortableTestModelGenDto.toDomain dto.SorterTestModelGenDto
        sortableTestModelSetMaker.create sorterTestModelGen (UMX.tag<sorterTestModelCount> dto.FirstIndex) (UMX.tag<sorterTestModelCount> dto.Count)