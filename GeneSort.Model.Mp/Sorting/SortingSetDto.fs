namespace GeneSort.Model.Mp.Sorting

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting

[<MessagePackObject>]
type sortingSetDto = 
    { 
        [<Key(0)>] Id: Guid
        [<Key(1)>] sortingDtos: sortingDto[]
    }

module SortingSetDto =

    let toDomain (dto: sortingSetDto) : sortingSet =
        if dto.sortingDtos = null then
            failwith "SorterModels array cannot be null or empty"
        if Array.isEmpty dto.sortingDtos then
            failwith "SorterModels array cannot be null or empty"
        let sorterModels = 
            dto.sortingDtos 
            |> Array.map SortingDto.toDomain
        sortingSet.create
                (UMX.tag<sortingSetId> dto.Id)
                (sorterModels)


    let fromDomain (domain: sortingSet) : sortingSetDto =
        { 
            Id = %domain.Id
            sortingDtos = domain.Sortings |> Array.map SortingDto.fromDomain 
        }


