namespace GeneSort.Model.Mp.Sorting.Sorter

open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting.Sorter.Ce
open GeneSort.Model.Mp.Sorting.Sorter.Si
open GeneSort.Model.Mp.Sorting.Sorter.Rs
open GeneSort.Model.Mp.Sorting.Sorter.Uf4
open GeneSort.Model.Mp.Sorting.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

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

    let fromDomain (sorterModel: sorterModel) : sorterModelDto =
        match sorterModel with
        | sorterModel.Msce msce -> Msce (MsceDto.fromDomain msce)
        | sorterModel.Mssi mssi -> Mssi (MssiDto.fromDomain mssi)
        | sorterModel.Msrs msrs -> Msrs (MsrsDto.fromDomain msrs)
        | sorterModel.Msuf4 msuf4 -> Msuf4 (Msuf4Dto.fromDomain msuf4)
        | sorterModel.Msuf6 msuf6 -> Msuf6 (Msuf6Dto.fromDomain msuf6)


    let toDomain (dto: sorterModelDto) : sorterModel =
        try
            match dto with
            | Msce msceDto -> sorterModel.Msce (MsceDto.toDomain msceDto |> Result.toOption |> Option.get)
            | Mssi mssiDto -> sorterModel.Mssi (MssiDto.toDomain mssiDto |> Result.toOption |> Option.get)
            | Msrs msrsDto -> sorterModel.Msrs (MsrsDto.toDomain msrsDto |> Result.toOption |> Option.get)
            | Msuf4 msuf4Dto -> sorterModel.Msuf4 (Msuf4Dto.toDomain msuf4Dto |> Result.toOption |> Option.get)
            | Msuf6 msuf6Dto -> sorterModel.Msuf6 (Msuf6Dto.toDomain msuf6Dto |> Result.toOption |> Option.get)
        with
        | ex -> failwith $"Failed to convert SorterModelDto: {ex.Message}"