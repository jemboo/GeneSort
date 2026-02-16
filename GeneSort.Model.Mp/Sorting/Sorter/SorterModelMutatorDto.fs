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

    let fromDomain (sorterModelMutator: sorterModelMutator) : sorterModelMutatorDto =
        match sorterModelMutator with
        | sorterModelMutator.SmmMsceRandMutate msceRandMutate ->
            MsceRandMutate (MsceRandMutateDto.fromDomain msceRandMutate)
        | sorterModelMutator.SmmMssiRandMutate mssiRandMutate ->
            MssiRandMutate (MssiRandMutateDto.fromDomain mssiRandMutate)
        | sorterModelMutator.SmmMsrsRandMutate msrsRandMutate->
            MsrsRandMutate (MsrsRandMutateDto.fromDomain msrsRandMutate)
        | sorterModelMutator.SmmMsuf4RandMutate msuf4RandMutate ->
            Msuf4RandMutate (Msuf4RandMutateDto.fromDomain msuf4RandMutate)
        | sorterModelMutator.SmmMsuf6RandMutate msuf6RandMutate ->
            Msuf6RandMutate (Msuf6RandMutateDto.fromDomain msuf6RandMutate)


    let toDomain (dto: sorterModelMutatorDto) : sorterModelMutator =
        try
            match dto with
            | MsceRandMutate msceRandMutateDto ->
                sorterModelMutator.SmmMsceRandMutate (MsceRandMutateDto.toDomain msceRandMutateDto)
            | MssiRandMutate mssiRandGenDto ->
                sorterModelMutator.SmmMssiRandMutate (MssiRandMutateDto.toDomain mssiRandGenDto |> Result.toOption |> Option.get )
            | MsrsRandMutate msrsRandGenDto ->
                sorterModelMutator.SmmMsrsRandMutate (MsrsRandMutateDto.toDomain msrsRandGenDto)
            | Msuf4RandMutate msuf4RandGenDto ->
                sorterModelMutator.SmmMsuf4RandMutate (Msuf4RandMutateDto.toDomain msuf4RandGenDto)
            | Msuf6RandMutate msuf6RandGenDto ->
                sorterModelMutator.SmmMsuf6RandMutate (Msuf6RandMutateDto.toDomain msuf6RandGenDto)

        with
        | ex -> failwith $"Failed to convert SorterModelMakerDto: {ex.Message}"