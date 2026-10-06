namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable



[<MessagePackObject>]
type sortableTestsModelSetGenDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModelGenDto: sortableTestsModelGenDto
    [<Key(2)>] FirstIndex: int
    [<Key(3)>] Count: int
}

module SortableTestsModelSetGenDto =

    let fromDomain (gen: sortableTestsModelSetGen) : sortableTestsModelSetGenDto =
        { Id = %gen.Id
          SorterTestModelGenDto = SortableTestsModelGenDto.fromDomain gen.SorterTestModelGen
          FirstIndex = int gen.FirstIndex
          Count = int gen.Count }

    let toDomain (dto: sortableTestsModelSetGenDto) : sortableTestsModelSetGen =
        if dto.FirstIndex < 0 then
            invalidArg "FirstIndex" "First index must be non-negative."
        if dto.Count < 0 then
            invalidArg "Count" "Count must be non-negative."
        let sorterTestModelGen = SortableTestsModelGenDto.toDomain dto.SorterTestModelGenDto
        sortableTestsModelSetGen.create sorterTestModelGen (UMX.tag<sorterTestModelCount> dto.FirstIndex) (UMX.tag<sorterTestModelCount> dto.Count)