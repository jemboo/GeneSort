namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX
open GeneSort.Sorter

[<MessagePackObject>]
type sorterModelSetDto = 
    { 
        [<Key(0)>] Id: Guid
        [<Key(1)>] CeLength: int
        [<Key(2)>] SorterModels: sorterModelDto[]
    }

module SorterModelSetDto =

    let toDomain (dto: sorterModelSetDto) : sorterModelSet =
        if dto.SorterModels = null then
            failwith "SorterModels array cannot be null or empty"
        if Array.isEmpty dto.SorterModels then
            failwith "SorterModels array cannot be null or empty"
        let sorterModels = 
            dto.SorterModels 
            |> Array.map SorterModelDto.fromSorterModelDto
        sorterModelSet.create
                (UMX.tag<sorterModelSetID> dto.Id)
                (dto.CeLength |> UMX.tag<ceLength>)
                (sorterModels)


    let fromDomain (domain: sorterModelSet) : sorterModelSetDto =
        { 
            Id = %domain.Id
            CeLength = %domain.CeLength
            SorterModels = domain.SorterModels |> Array.map SorterModelDto.toSorterModelDto }


