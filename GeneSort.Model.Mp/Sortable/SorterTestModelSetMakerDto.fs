namespace GeneSort.Model.Mp.Sortable

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Core.Mp
open GeneSort.Sorter



[<MessagePackObject>]
type SorterTestModelSetMakerDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModelGenDto: SorterTestModelGenDto
    [<Key(2)>] FirstIndex: int
    [<Key(3)>] Count: int
}

module SorterTestModelSetMakerDto =
    let fromDomain (maker: SorterTestModelSetMaker) : SorterTestModelSetMakerDto =
        { Id = %maker.Id
          SorterTestModelGenDto = SorterTestModelGenDto.toDtoSorterTestModelGen maker.SorterTestModelGen
          FirstIndex = int maker.FirstIndex
          Count = int maker.Count }

    let toDomain (dto: SorterTestModelSetMakerDto) (msasORandGen: MsasORandGen) : SorterTestModelSetMaker =
        if dto.FirstIndex < 0 then
            invalidArg "FirstIndex" "First index must be non-negative."
        if dto.Count < 0 then
            invalidArg "Count" "Count must be non-negative."
        let sorterTestModelGen = SorterTestModelGenDto.fromDtoSorterTestModelGen dto.SorterTestModelGenDto
        SorterTestModelSetMaker.create sorterTestModelGen (UMX.tag<sorterTestModelCount> dto.FirstIndex) (UMX.tag<sorterTestModelCount> dto.Count)