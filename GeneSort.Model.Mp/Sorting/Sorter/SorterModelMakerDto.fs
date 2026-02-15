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

[<MessagePackObject; 
  Union(0, typeof<msceRandGenDto>);
  Union(1, typeof<mssiRandGenDto>);
  Union(2, typeof<msrsRandGenDto>);
  Union(3, typeof<msuf4RandGenDto>);
  Union(4, typeof<msuf6RandGenDto>)>]

type sorterModelMakerDto =
    | MsceRandGen of msceRandGenDto
    | MssiRandGen of mssiRandGenDto
    | MsrsRandGen of msrsRandGenDto
    | Msuf4RandGen of msuf4RandGenDto
    | Msuf6RandGen of msuf6RandGenDto

module SorterModelMakerDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelMakerDto (sorterModelMaker: sorterModelMaker) : sorterModelMakerDto =
        match sorterModelMaker with
        | sorterModelMaker.SmmMsceRandGen msceRandGen ->
            MsceRandGen (MsceRandGenDto.toMsceRandGenDto msceRandGen)
        | sorterModelMaker.SmmMssiRandGen mssiRandGen ->
            MssiRandGen (MssiRandGenDto.toMssiRandGenDto mssiRandGen)
        | sorterModelMaker.SmmMsrsRandGen msrsRandGen ->
            MsrsRandGen (MsrsRandGenDto.toMsrsRandGenDto msrsRandGen)
        | sorterModelMaker.SmmMsuf4RandGen msuf4RandGen ->
            Msuf4RandGen (Msuf4RandGenDto.toMsuf4RandGenDto msuf4RandGen)
        | sorterModelMaker.SmmMsuf6RandGen msuf6RandGen ->
            Msuf6RandGen (Msuf6RandGenDto.toMsuf6RandGenDto msuf6RandGen)

    let fromSorterModelMakerDto (dto: sorterModelMakerDto) : sorterModelMaker =
        try
            match dto with
            | MsceRandGen msceRandGenDto ->
                sorterModelMaker.SmmMsceRandGen (MsceRandGenDto.fromMsceRandGenDto msceRandGenDto)
            | MssiRandGen mssiRandGenDto ->
                sorterModelMaker.SmmMssiRandGen (MssiRandGenDto.fromMssiRandGenDto mssiRandGenDto |> Result.toOption |> Option.get )
            | MsrsRandGen msrsRandGenDto ->
                sorterModelMaker.SmmMsrsRandGen (MsrsRandGenDto.fromMsrsRandGenDto msrsRandGenDto)
            | Msuf4RandGen msuf4RandGenDto ->
                sorterModelMaker.SmmMsuf4RandGen (Msuf4RandGenDto.fromMsuf4RandGenDto msuf4RandGenDto)
            | Msuf6RandGen msuf6RandGenDto ->
                sorterModelMaker.SmmMsuf6RandGen (Msuf6RandGenDto.fromMsuf6RandGenDto msuf6RandGenDto)
        with
        | ex -> failwith $"Failed to convert SorterModelMakerDto: {ex.Message}"