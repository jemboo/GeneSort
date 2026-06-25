namespace GeneSort.Model.Mp.Sorting.Mp.V1

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple

[<MessagePackObject>]
type UnknownDto() = class end

[<MessagePackObject>]
[<Union(0, typeof<simpleSorterModelDto>); Union(1, typeof<UnknownDto>)>]
type sorterModelDto =
    | Simple of simpleSorterModelDto
    | Unknown of UnknownDto

module SorterModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (model: sorterModel) : sorterModelDto =
        match model with
        | sorterModel.Simple sms -> Simple (SimpleSorterModelDto.fromDomain sms)
        | sorterModel.Unknown -> Unknown (UnknownDto())

    let toDomain (dto: sorterModelDto) : sorterModel =
        try
            match dto with
            | Simple simpleDto -> sorterModel.Simple (SimpleSorterModelDto.toDomain simpleDto)
            | Unknown _ -> sorterModel.Unknown
        with
        | ex -> failwith $"Failed to convert SorterModelDto: {ex.Message}"