namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable


[<MessagePackObject>]
type sortableTestsModelSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModels: sorterTestModelDto[]
}

module SortableTestsModelSetDto =

    let fromDomain (set: sortableTestsModelSet) : sortableTestsModelSetDto =
        { Id = %set.Id; SorterTestModels = set.SorterTestModels |> Array.map SorterTestModelDto.toDomain }

    let toDomain (dto: sortableTestsModelSetDto) : sortableTestsModelSet =
        let models = dto.SorterTestModels |> Array.map SorterTestModelDto.fromDomain
        sortableTestsModelSet.create (UMX.tag<sorterTestModelSetID> dto.Id) models