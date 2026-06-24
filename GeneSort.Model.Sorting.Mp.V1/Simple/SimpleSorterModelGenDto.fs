namespace GeneSort.Model.Mp.Sorting.Sorter

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf4
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf6

[<MessagePackObject; 
  Union(0, typeof<msceRandGenDto>);
  Union(1, typeof<mssiRandGenDto>);
  Union(2, typeof<msrsRandGenDto>);
  Union(3, typeof<msuf4RandGenDto>);
  Union(4, typeof<msuf6RandGenDto>)>]

type simpleSorterModelGenDto =
    | MsceRandGen of msceRandGenDto
    | MssiRandGen of mssiRandGenDto
    | MsrsRandGen of msrsRandGenDto
    | Msuf4RandGen of msuf4RandGenDto
    | Msuf6RandGen of msuf6RandGenDto

module SimpleSorterModelGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (sorterModelGen: simpleSorterModelGen) : simpleSorterModelGenDto =
        match sorterModelGen with
        | simpleSorterModelGen.SmmMsceRandGen msceRandGen ->
            MsceRandGen (MsceRandGenDto.fromDomain msceRandGen)
        | simpleSorterModelGen.SmmMssiRandGen mssiRandGen ->
            MssiRandGen (MssiRandGenDto.fromDomain mssiRandGen)
        | simpleSorterModelGen.SmmMsrsRandGen msrsRandGen ->
            MsrsRandGen (MsrsRandGenDto.fromDomain msrsRandGen)
        | simpleSorterModelGen.SmmMsuf4RandGen msuf4RandGen ->
            Msuf4RandGen (Msuf4RandGenDto.fromDomain msuf4RandGen)
        | simpleSorterModelGen.SmmMsuf6RandGen msuf6RandGen ->
            Msuf6RandGen (Msuf6RandGenDto.fromDomain msuf6RandGen)


    let toDomain (dto: simpleSorterModelGenDto) : simpleSorterModelGen =
        try
            match dto with
            | MsceRandGen msceRandGenDto ->
                simpleSorterModelGen.SmmMsceRandGen (MsceRandGenDto.toDomain msceRandGenDto)
            | MssiRandGen mssiRandGenDto ->
                simpleSorterModelGen.SmmMssiRandGen (MssiRandGenDto.toDomain mssiRandGenDto |> Result.toOption |> Option.get )
            | MsrsRandGen msrsRandGenDto ->
                simpleSorterModelGen.SmmMsrsRandGen (MsrsRandGenDto.toDomain msrsRandGenDto)
            | Msuf4RandGen msuf4RandGenDto ->
                simpleSorterModelGen.SmmMsuf4RandGen (Msuf4RandGenDto.toDomain msuf4RandGenDto)
            | Msuf6RandGen msuf6RandGenDto ->
                simpleSorterModelGen.SmmMsuf6RandGen (Msuf6RandGenDto.toDomain msuf6RandGenDto)
        with
        | ex -> failwith $"Failed to convert sorterModelGenDto: {ex.Message}"