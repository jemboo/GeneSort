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

    let fromDomain (set: sorterTestModelSet) : SorterTestModelSetDto =
        { Id = %set.Id; SorterTestModels = set.SorterTestModels |> Array.map SorterTestModelDto.toDomain }

    let toDomain (dto: SorterTestModelSetDto) : sorterTestModelSet =
        let models = dto.SorterTestModels |> Array.map SorterTestModelDto.fromDomain
        sorterTestModelSet.create (UMX.tag<sorterTestModelSetID> dto.Id) models