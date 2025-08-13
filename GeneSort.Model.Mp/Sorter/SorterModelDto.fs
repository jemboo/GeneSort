namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
open GeneSort.Model.Mp.Sorter.Ce
open GeneSort.Model.Mp.Sorter.Si
open GeneSort.Model.Mp.Sorter.Rs
open GeneSort.Model.Mp.Sorter.Uf4
open GeneSort.Model.Mp.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
[<Union(0, typeof<MsceDto>); Union(1, typeof<MssiDto>); Union(2, typeof<MsrsDto>); Union(3, typeof<Msuf4Dto>); Union(4, typeof<Msuf6Dto>)>]
type SorterModelDto =
    | Msce of MsceDto
    | Mssi of MssiDto
    | Msrs of MsrsDto
    | Msuf4 of Msuf4Dto
    | Msuf6 of Msuf6Dto

module SorterModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelDto (sorterModel: SorterModel) : SorterModelDto =
        match sorterModel with
        | SorterModel.Msce msce -> Msce (MsceDto.toMsceDto msce)
        | SorterModel.Mssi mssi -> Mssi (MssiDto.toMssiDto mssi)
        | SorterModel.Msrs msrs -> Msrs (MsrsDto.toMsrsDto msrs)
        | SorterModel.Msuf4 msuf4 -> Msuf4 (Msuf4Dto.toMsuf4Dto msuf4)
        | SorterModel.Msuf6 msuf6 -> Msuf6 (Msuf6Dto.toMsuf6Dto msuf6)

    let fromSorterModelDto (dto: SorterModelDto) : SorterModel =
        try
            match dto with
            | Msce msceDto -> SorterModel.Msce (MsceDto.toMsce msceDto |> Result.toOption |> Option.get)
            | Mssi mssiDto -> SorterModel.Mssi (MssiDto.toMssi mssiDto |> Result.toOption |> Option.get)
            | Msrs msrsDto -> SorterModel.Msrs (MsrsDto.toMsrs msrsDto |> Result.toOption |> Option.get)
            | Msuf4 msuf4Dto -> SorterModel.Msuf4 (Msuf4Dto.fromMsuf4Dto msuf4Dto |> Result.toOption |> Option.get)
            | Msuf6 msuf6Dto -> SorterModel.Msuf6 (Msuf6Dto.fromMsuf6Dto msuf6Dto |> Result.toOption |> Option.get)
        with
        | ex -> failwith $"Failed to convert SorterModelDto: {ex.Message}"