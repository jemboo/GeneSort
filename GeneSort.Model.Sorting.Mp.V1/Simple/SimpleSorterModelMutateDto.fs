
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple

open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf4
open GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf6

type simpleSorterModelMutateDto =
    | MsceRandMutate of msceRandMutateDto
    | MssiRandMutate of mssiRandMutateDto
    | MsrsRandMutate of msrsRandMutateDto
    | Msuf4RandMutate of msuf4RandMutateDto
    | Msuf6RandMutate of msuf6RandMutateDto

module SimpleSorterModelMutateDto =

    let fromDomain (sorterModelMutator: simpleSorterModelMutator) : simpleSorterModelMutateDto =
        match sorterModelMutator with
        | simpleSorterModelMutator.SmmMsceRandMutate msceRandMutate ->
            MsceRandMutate (MsceRandMutateDto.fromDomain msceRandMutate)
        | simpleSorterModelMutator.SmmMssiRandMutate mssiRandMutate ->
            MssiRandMutate (MssiRandMutateDto.fromDomain mssiRandMutate)
        | simpleSorterModelMutator.SmmMsrsRandMutate msrsRandMutate ->
            MsrsRandMutate (MsrsRandMutateDto.fromDomain msrsRandMutate)
        | simpleSorterModelMutator.SmmMsuf4RandMutate msuf4RandMutate ->
            Msuf4RandMutate (Msuf4RandMutateDto.fromDomain msuf4RandMutate)
        | simpleSorterModelMutator.SmmMsuf6RandMutate msuf6RandMutate ->
            Msuf6RandMutate (Msuf6RandMutateDto.fromDomain msuf6RandMutate)

    let toDomain (dto: simpleSorterModelMutateDto) : simpleSorterModelMutator =
        try
            match dto with
            | MsceRandMutate msceRandMutateDto ->
                simpleSorterModelMutator.SmmMsceRandMutate (MsceRandMutateDto.toDomain msceRandMutateDto)
            | MssiRandMutate mssiRandGenDto ->
                simpleSorterModelMutator.SmmMssiRandMutate (MssiRandMutateDto.toDomain mssiRandGenDto)
            | MsrsRandMutate msrsRandGenDto ->
                simpleSorterModelMutator.SmmMsrsRandMutate (MsrsRandMutateDto.toDomain msrsRandGenDto)
            | Msuf4RandMutate msuf4RandGenDto ->
                simpleSorterModelMutator.SmmMsuf4RandMutate (Msuf4RandMutateDto.toDomain msuf4RandGenDto)
            | Msuf6RandMutate msuf6RandGenDto ->
                simpleSorterModelMutator.SmmMsuf6RandMutate (Msuf6RandMutateDto.toDomain msuf6RandGenDto)
        with
        | ex -> failwith $"Failed to convert sorterModelMutatorDto: {ex.Message}"