namespace GeneSort.Model.Mp.Sortable

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Core.Mp
open GeneSort.Sorter


[<MessagePackObject>]
type SorterTestModelSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModels: SorterTestModelDto[]
}

module SorterTestModelSetDto =

    let toDtoSorterTestModelSet (set: SorterTestModelSet) : SorterTestModelSetDto =
        { Id = %set.Id; SorterTestModels = set.SorterTestModels |> Array.map SorterTestModelDto.toDto }

    let fromDtoSorterTestModelSet (dto: SorterTestModelSetDto) : SorterTestModelSet =
        let models = dto.SorterTestModels |> Array.map SorterTestModelDto.fromDto
        SorterTestModelSet.create (UMX.tag<sorterTestModelSetID> dto.Id) models