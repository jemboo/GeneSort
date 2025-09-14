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

[<MessagePackObject; Union(0, typeof<msceRandGenDto>);
  Union(1, typeof<MsceRandMutateDto>); Union(2, typeof<mssiRandGenDto>);
  Union(3, typeof<mssiRandMutateDto>); Union(4, typeof<msrsRandGenDto>);
  Union(5, typeof<msrsRandMutateDto>); Union(6, typeof<msuf4RandGenDto>);
  Union(7, typeof<msuf4RandMutateDto>); Union(8, typeof<msuf6RandGenDto>);
  Union(9, typeof<msuf6RandMutateDto>)>]
type SorterModelMakerDto =
    | MsceRandGen of msceRandGenDto
    | MsceRandMutate of MsceRandMutateDto
    | MssiRandGen of mssiRandGenDto
    | MssiRandMutate of mssiRandMutateDto
    | MsrsRandGen of msrsRandGenDto
    | MsrsRandMutate of msrsRandMutateDto
    | Msuf4RandGen of msuf4RandGenDto
    | Msuf4RandMutate of msuf4RandMutateDto
    | Msuf6RandGen of msuf6RandGenDto
    | Msuf6RandMutate of msuf6RandMutateDto

module SorterModelMakerDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelMakerDto (sorterModelMaker: sorterModelMaker) : SorterModelMakerDto =
        match sorterModelMaker with
        | sorterModelMaker.SmmMsceRandGen msceRandGen ->
            MsceRandGen (MsceRandGenDto.toMsceRandGenDto msceRandGen)
        | sorterModelMaker.SmmMsceRandMutate msceRandMutate ->
            MsceRandMutate (MsceRandMutateDto.toMsceRandMutateDto msceRandMutate)
        | sorterModelMaker.SmmMssiRandGen mssiRandGen ->
            MssiRandGen (MssiRandGenDto.toMssiRandGenDto mssiRandGen)
        | sorterModelMaker.SmmMssiRandMutate mssiRandMutate ->
            MssiRandMutate (MssiRandMutateDto.toMssiRandMutateDto mssiRandMutate)
        | sorterModelMaker.SmmMsrsRandGen msrsRandGen ->
            MsrsRandGen (MsrsRandGenDto.toMsrsRandGenDto msrsRandGen)
        | sorterModelMaker.SmmMsrsRandMutate msrsRandMutate ->
            MsrsRandMutate (MsrsRandMutateDto.toMsrsRandMutateDto msrsRandMutate)
        | sorterModelMaker.SmmMsuf4RandGen msuf4RandGen ->
            Msuf4RandGen (Msuf4RandGenDto.toMsuf4RandGenDto msuf4RandGen)
        | sorterModelMaker.SmmMsuf4RandMutate msuf4RandMutate ->
            Msuf4RandMutate (Msuf4RandMutateDto.toMsuf4RandMutateDto msuf4RandMutate)
        | sorterModelMaker.SmmMsuf6RandGen msuf6RandGen ->
            Msuf6RandGen (Msuf6RandGenDto.toMsuf6RandGenDto msuf6RandGen)
        | sorterModelMaker.SmmMsuf6RandMutate msuf6RandMutate ->
            Msuf6RandMutate (Msuf6RandMutateDto.toMsuf6RandMutateDto msuf6RandMutate)

    let fromSorterModelMakerDto (dto: SorterModelMakerDto) : sorterModelMaker =
        try
            match dto with
            | MsceRandGen msceRandGenDto ->
                sorterModelMaker.SmmMsceRandGen (MsceRandGenDto.fromMsceRandGenDto msceRandGenDto)
            | MsceRandMutate msceRandMutateDto ->
                sorterModelMaker.SmmMsceRandMutate (MsceRandMutateDto.fromMsceRandMutateDto msceRandMutateDto |> Result.toOption |> Option.get )
            | MssiRandGen mssiRandGenDto ->
                sorterModelMaker.SmmMssiRandGen (MssiRandGenDto.fromMssiRandGenDto mssiRandGenDto |> Result.toOption |> Option.get )
            | MssiRandMutate mssiRandMutateDto ->
                sorterModelMaker.SmmMssiRandMutate (MssiRandMutateDto.fromMssiRandMutateDto mssiRandMutateDto |> Result.toOption |> Option.get )
            | MsrsRandGen msrsRandGenDto ->
                sorterModelMaker.SmmMsrsRandGen (MsrsRandGenDto.fromMsrsRandGenDto msrsRandGenDto)
            | MsrsRandMutate msrsRandMutateDto ->
                sorterModelMaker.SmmMsrsRandMutate (MsrsRandMutateDto.fromMsrsRandMutateDto msrsRandMutateDto)
            | Msuf4RandGen msuf4RandGenDto ->
                sorterModelMaker.SmmMsuf4RandGen (Msuf4RandGenDto.fromMsuf4RandGenDto msuf4RandGenDto)
            | Msuf4RandMutate msuf4RandMutateDto ->
                sorterModelMaker.SmmMsuf4RandMutate (Msuf4RandMutateDto.fromMsuf4RandMutateDto msuf4RandMutateDto)
            | Msuf6RandGen msuf6RandGenDto ->
                sorterModelMaker.SmmMsuf6RandGen (Msuf6RandGenDto.fromMsuf6RandGenDto msuf6RandGenDto)
            | Msuf6RandMutate msuf6RandMutateDto ->
                sorterModelMaker.SmmMsuf6RandMutate (Msuf6RandMutateDto.fromMsuf6RandMutateDto msuf6RandMutateDto)
        with
        | ex -> failwith $"Failed to convert SorterModelMakerDto: {ex.Message}"