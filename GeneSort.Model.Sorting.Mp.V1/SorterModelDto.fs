namespace GeneSort.Model.Mp.Sorting.Sorter
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf4
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf6
open GeneSort.Model.Sorting.V1.Simple


[<MessagePackObject>]
[<Union(0, typeof<msceDto>); Union(1, typeof<mssiDto>); Union(2, typeof<msrsDto>); Union(3, typeof<msuf4Dto>); Union(4, typeof<msuf6Dto>)>]
type sorterModelDto =
    | Msce of msceDto
    | Mssi of mssiDto
    | Msrs of msrsDto
    | Msuf4 of msuf4Dto
    | Msuf6 of msuf6Dto

module SorterModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (ssm: simpleSorterModel) : sorterModelDto =
        match ssm with
        | simpleSorterModel.Msce msce -> Msce (MsceDto.fromDomain msce)
        | simpleSorterModel.Mssi mssi -> Mssi (MssiDto.fromDomain mssi)
        | simpleSorterModel.Msrs msrs -> Msrs (MsrsDto.fromDomain msrs)
        | simpleSorterModel.Msuf4 msuf4 -> Msuf4 (Msuf4Dto.fromDomain msuf4)
        | simpleSorterModel.Msuf6 msuf6 -> Msuf6 (Msuf6Dto.fromDomain msuf6)


    let toDomain (dto: sorterModelDto) : simpleSorterModel =
        try
            match dto with
            | Msce msceDto -> simpleSorterModel.Msce (MsceDto.toDomain msceDto |> Result.toOption |> Option.get)
            | Mssi mssiDto -> simpleSorterModel.Mssi (MssiDto.toDomain mssiDto |> Result.toOption |> Option.get)
            | Msrs msrsDto -> simpleSorterModel.Msrs (MsrsDto.toDomain msrsDto |> Result.toOption |> Option.get)
            | Msuf4 msuf4Dto -> simpleSorterModel.Msuf4 (Msuf4Dto.toDomain msuf4Dto |> Result.toOption |> Option.get)
            | Msuf6 msuf6Dto -> simpleSorterModel.Msuf6 (Msuf6Dto.toDomain msuf6Dto |> Result.toOption |> Option.get)
        with
        | ex -> failwith $"Failed to convert SorterModelDto: {ex.Message}"