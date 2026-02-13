namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable


[<MessagePackObject>]
type sortableTestModelSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModels: sorterTestModelDto[]
}

module SortableTestModelSetDto =

    let fromDomain (set: sortableTestModelSet) : sortableTestModelSetDto =
        { Id = %set.Id; SorterTestModels = set.SorterTestModels |> Array.map SorterTestModelDto.toDomain }

    let toDomain (dto: sortableTestModelSetDto) : sortableTestModelSet =
        let models = dto.SorterTestModels |> Array.map SorterTestModelDto.fromDomain
        sortableTestModelSet.create (UMX.tag<sorterTestModelSetID> dto.Id) models