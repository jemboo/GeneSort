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
  Union(0, typeof<msceRandMutateDto>);
  Union(1, typeof<mssiRandMutateDto>);
  Union(2, typeof<msrsRandMutateDto>);
  Union(3, typeof<msuf4RandMutateDto>);
  Union(4, typeof<msuf6RandMutateDto>)>]

type sorterModelMutatorDto =
    | MsceRandMutate of msceRandMutateDto
    | MssiRandMutate of mssiRandMutateDto
    | MsrsRandMutate of msrsRandMutateDto
    | Msuf4RandMutate of msuf4RandMutateDto
    | Msuf6RandMutate of msuf6RandMutateDto

module SorterModelMutatorDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelMutatorDto (sorterModelMutator: sorterModelMutator) : sorterModelMutatorDto =
        match sorterModelMutator with
        | sorterModelMutator.SmmMsceRandMutate msceRandMutate ->
            MsceRandMutate (MsceRandMutateDto.toMsceRandMutateDto msceRandMutate)
        | sorterModelMutator.SmmMssiRandMutate mssiRandMutate ->
            MssiRandMutate (MssiRandMutateDto.toMssiRandMutateDto mssiRandMutate)
        | sorterModelMutator.SmmMsrsRandMutate msrsRandMutate->
            MsrsRandMutate (MsrsRandMutateDto.toMsrsRandMutateDto msrsRandMutate)
        | sorterModelMutator.SmmMsuf4RandMutate msuf4RandMutate ->
            Msuf4RandMutate (Msuf4RandMutateDto.toMsuf4RandMutateDto msuf4RandMutate)
        | sorterModelMutator.SmmMsuf6RandMutate msuf6RandMutate ->
            Msuf6RandMutate (Msuf6RandMutateDto.toMsuf6RandMutateDto msuf6RandMutate)

    let fromSorterModelMutatorDto (dto: sorterModelMutatorDto) : sorterModelMutator =
        try
            match dto with
            | MsceRandMutate msceRandMutateDto ->
                sorterModelMutator.SmmMsceRandMutate (MsceRandMutateDto.fromMsceRandMutateDto msceRandMutateDto)
            | MssiRandMutate mssiRandGenDto ->
                sorterModelMutator.SmmMssiRandMutate (MssiRandMutateDto.fromMssiRandMutateDto mssiRandGenDto |> Result.toOption |> Option.get )
            | MsrsRandMutate msrsRandGenDto ->
                sorterModelMutator.SmmMsrsRandMutate (MsrsRandMutateDto.fromMsrsRandMutateDto msrsRandGenDto)
            | Msuf4RandMutate msuf4RandGenDto ->
                sorterModelMutator.SmmMsuf4RandMutate (Msuf4RandMutateDto.fromMsuf4RandMutateDto msuf4RandGenDto)
            | Msuf6RandMutate msuf6RandGenDto ->
                sorterModelMutator.SmmMsuf6RandMutate (Msuf6RandMutateDto.fromMsuf6RandMutateDto msuf6RandGenDto)

        with
        | ex -> failwith $"Failed to convert SorterModelMakerDto: {ex.Message}"