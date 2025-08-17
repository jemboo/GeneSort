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

[<MessagePackObject; Union(0, typeof<MsceRandGenDto>);
  Union(1, typeof<MsceRandMutateDto>); Union(2, typeof<MssiRandGenDto>);
  Union(3, typeof<MssiRandMutateDto>); Union(4, typeof<MsrsRandGenDto>);
  Union(5, typeof<MsrsRandMutateDto>); Union(6, typeof<Msuf4RandGenDto>);
  Union(7, typeof<Msuf4RandMutateDto>); Union(8, typeof<Msuf6RandGenDto>);
  Union(9, typeof<Msuf6RandMutateDto>)>]
type SorterModelMakerDto =
    | MsceRandGen of MsceRandGenDto
    | MsceRandMutate of MsceRandMutateDto
    | MssiRandGen of MssiRandGenDto
    | MssiRandMutate of MssiRandMutateDto
    | MsrsRandGen of MsrsRandGenDto
    | MsrsRandMutate of MsrsRandMutateDto
    | Msuf4RandGen of Msuf4RandGenDto
    | Msuf4RandMutate of Msuf4RandMutateDto
    | Msuf6RandGen of Msuf6RandGenDto
    | Msuf6RandMutate of Msuf6RandMutateDto

module SorterModelMakerDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelMakerDto (sorterModelMaker: SorterModelMaker) : SorterModelMakerDto =
        match sorterModelMaker with
        | SorterModelMaker.SmmMsceRandGen msceRandGen ->
            MsceRandGen (MsceRandGenDto.toMsceRandGenDto msceRandGen)
        | SorterModelMaker.SmmMsceRandMutate msceRandMutate ->
            MsceRandMutate (MsceRandMutateDto.toMsceRandMutateDto msceRandMutate)
        | SorterModelMaker.SmmMssiRandGen mssiRandGen ->
            MssiRandGen (MssiRandGenDto.toMssiRandGenDto mssiRandGen)
        | SorterModelMaker.SmmMssiRandMutate mssiRandMutate ->
            MssiRandMutate (MssiRandMutateDto.toMssiRandMutateDto mssiRandMutate)
        | SorterModelMaker.SmmMsrsRandGen msrsRandGen ->
            MsrsRandGen (MsrsRandGenDto.toMsrsRandGenDto msrsRandGen)
        | SorterModelMaker.SmmMsrsRandMutate msrsRandMutate ->
            MsrsRandMutate (MsrsRandMutateDto.toMsrsRandMutateDto msrsRandMutate)
        | SorterModelMaker.SmmMsuf4RandGen msuf4RandGen ->
            Msuf4RandGen (Msuf4RandGenDto.toMsuf4RandGenDto msuf4RandGen)
        | SorterModelMaker.SmmMsuf4RandMutate msuf4RandMutate ->
            Msuf4RandMutate (Msuf4RandMutateDto.toMsuf4RandMutateDto msuf4RandMutate)
        | SorterModelMaker.SmmMsuf6RandGen msuf6RandGen ->
            Msuf6RandGen (Msuf6RandGenDto.toMsuf6RandGenDto msuf6RandGen)
        | SorterModelMaker.SmmMsuf6RandMutate msuf6RandMutate ->
            Msuf6RandMutate (Msuf6RandMutateDto.toMsuf6RandMutateDto msuf6RandMutate)

    let fromSorterModelMakerDto (dto: SorterModelMakerDto) : SorterModelMaker =
        try
            match dto with
            | MsceRandGen msceRandGenDto ->
                SorterModelMaker.SmmMsceRandGen (MsceRandGenDto.fromMsceRandGenDto msceRandGenDto)
            | MsceRandMutate msceRandMutateDto ->
                SorterModelMaker.SmmMsceRandMutate (MsceRandMutateDto.fromMsceRandMutateDto msceRandMutateDto |> Result.toOption |> Option.get )
            | MssiRandGen mssiRandGenDto ->
                SorterModelMaker.SmmMssiRandGen (MssiRandGenDto.fromMssiRandGenDto mssiRandGenDto |> Result.toOption |> Option.get )
            | MssiRandMutate mssiRandMutateDto ->
                SorterModelMaker.SmmMssiRandMutate (MssiRandMutateDto.fromMssiRandMutateDto mssiRandMutateDto |> Result.toOption |> Option.get )
            | MsrsRandGen msrsRandGenDto ->
                SorterModelMaker.SmmMsrsRandGen (MsrsRandGenDto.fromMsrsRandGenDto msrsRandGenDto)
            | MsrsRandMutate msrsRandMutateDto ->
                SorterModelMaker.SmmMsrsRandMutate (MsrsRandMutateDto.fromMsrsRandMutateDto msrsRandMutateDto)
            | Msuf4RandGen msuf4RandGenDto ->
                SorterModelMaker.SmmMsuf4RandGen (Msuf4RandGenDto.fromMsuf4RandGenDto msuf4RandGenDto)
            | Msuf4RandMutate msuf4RandMutateDto ->
                SorterModelMaker.SmmMsuf4RandMutate (Msuf4RandMutateDto.fromMsuf4RandMutateDto msuf4RandMutateDto)
            | Msuf6RandGen msuf6RandGenDto ->
                SorterModelMaker.SmmMsuf6RandGen (Msuf6RandGenDto.fromMsuf6RandGenDto msuf6RandGenDto)
            | Msuf6RandMutate msuf6RandMutateDto ->
                SorterModelMaker.SmmMsuf6RandMutate (Msuf6RandMutateDto.fromMsuf6RandMutateDto msuf6RandMutateDto)
        with
        | ex -> failwith $"Failed to convert SorterModelMakerDto: {ex.Message}"