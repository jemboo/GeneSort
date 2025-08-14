namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX

[<MessagePackObject>]
type SorterModelSetDto = 
    { 
        [<Key(0)>] Id: Guid
        [<Key(1)>] SorterModels: SorterModelDto[]
    }

module SorterModelSetDto =

    let toDomain (dto: SorterModelSetDto) : SorterModelSet =
        if dto.SorterModels = null then
            failwith "SorterModels array cannot be null or empty"
        if Array.isEmpty dto.SorterModels then
            failwith "SorterModels array cannot be null or empty"
        let sorterModels = 
            dto.SorterModels 
            |> Array.map SorterModelDto.fromSorterModelDto
        { SorterModelSet.Id = UMX.tag<sorterModelSetID> dto.Id
          SorterModels = sorterModels }

    let fromDomain (domain: SorterModelSet) : SorterModelSetDto =
        { Id = %domain.Id
          SorterModels = domain.SorterModels |> Array.map SorterModelDto.toSorterModelDto }


