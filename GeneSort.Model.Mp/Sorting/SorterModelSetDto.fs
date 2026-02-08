namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting

[<MessagePackObject>]
type sortingModelSetDto = 
    { 
        [<Key(0)>] Id: Guid
        [<Key(1)>] sortingModelDtos: sortingModelDto[]
    }

module SortingModelSetDto =

    let toDomain (dto: sortingModelSetDto) : sortingModelSet =
        if dto.sortingModelDtos = null then
            failwith "SorterModels array cannot be null or empty"
        if Array.isEmpty dto.sortingModelDtos then
            failwith "SorterModels array cannot be null or empty"
        let sorterModels = 
            dto.sortingModelDtos 
            |> Array.map SortingModelDto.fromSortingModelDto
        sortingModelSet.create
                (UMX.tag<sortingModelSetID> dto.Id)
                (sorterModels)


    let fromDomain (domain: sortingModelSet) : sortingModelSetDto =
        { 
            Id = %domain.Id
            sortingModelDtos = domain.SortingModels |> Array.map SortingModelDto.toSortingModelDto 
        }


