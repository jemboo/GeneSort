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

type sorterModelGenDto =
    | MsceRandGen of msceRandGenDto
    | MssiRandGen of mssiRandGenDto
    | MsrsRandGen of msrsRandGenDto
    | Msuf4RandGen of msuf4RandGenDto
    | Msuf6RandGen of msuf6RandGenDto

module SorterModelGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sorterModelMaker: sorterModelGen) : sorterModelGenDto =
        match sorterModelMaker with
        | sorterModelGen.SmmMsceRandGen msceRandGen ->
            MsceRandGen (MsceRandGenDto.fromDomain msceRandGen)
        | sorterModelGen.SmmMssiRandGen mssiRandGen ->
            MssiRandGen (MssiRandGenDto.fromDomain mssiRandGen)
        | sorterModelGen.SmmMsrsRandGen msrsRandGen ->
            MsrsRandGen (MsrsRandGenDto.fromDomain msrsRandGen)
        | sorterModelGen.SmmMsuf4RandGen msuf4RandGen ->
            Msuf4RandGen (Msuf4RandGenDto.fromDomain msuf4RandGen)
        | sorterModelGen.SmmMsuf6RandGen msuf6RandGen ->
            Msuf6RandGen (Msuf6RandGenDto.fromDomain msuf6RandGen)


    let toDomain (dto: sorterModelGenDto) : sorterModelGen =
        try
            match dto with
            | MsceRandGen msceRandGenDto ->
                sorterModelGen.SmmMsceRandGen (MsceRandGenDto.toDomain msceRandGenDto)
            | MssiRandGen mssiRandGenDto ->
                sorterModelGen.SmmMssiRandGen (MssiRandGenDto.toDomain mssiRandGenDto |> Result.toOption |> Option.get )
            | MsrsRandGen msrsRandGenDto ->
                sorterModelGen.SmmMsrsRandGen (MsrsRandGenDto.toDomain msrsRandGenDto)
            | Msuf4RandGen msuf4RandGenDto ->
                sorterModelGen.SmmMsuf4RandGen (Msuf4RandGenDto.toDomain msuf4RandGenDto)
            | Msuf6RandGen msuf6RandGenDto ->
                sorterModelGen.SmmMsuf6RandGen (Msuf6RandGenDto.toDomain msuf6RandGenDto)
        with
        | ex -> failwith $"Failed to convert SorterModelMakerDto: {ex.Message}"