namespace GeneSort.Model.Mp.Sorting.Mp.V1

open GeneSort.Model.Sorting.V1
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple

type UnknownDto = UnknownDto

type sorterModelDto =
    | Simple of simpleSorterModelDto
    | Unknown of UnknownDto

module SorterModelDto =

    let fromDomain (model: sorterModel) : sorterModelDto =
        match model with
        | sorterModel.Simple sms -> Simple (SimpleSorterModelDto.fromDomain sms)
        | sorterModel.Unknown -> Unknown UnknownDto

    let toDomain (dto: sorterModelDto) : sorterModel =
        try
            match dto with
            | Simple simpleDto -> sorterModel.Simple (SimpleSorterModelDto.toDomain simpleDto)
            | Unknown _ -> sorterModel.Unknown
        with
        | ex -> failwith $"Failed to convert SorterModelDto: {ex.Message}"