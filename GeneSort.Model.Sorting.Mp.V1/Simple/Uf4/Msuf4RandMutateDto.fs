
namespace GeneSort.Model.Sorting.Mp.V1.Simple.Uf4

open System
open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1
open GeneSort.Core.Mp
open GeneSort.Model.Mp.Sorter.Uf4

type msuf4RandMutateDto =
    { id: Guid
      rngFactoryDto: rngFactoryDto
      uf4MutationRatesArrayDtos: uf4MutationRatesDto 
      mutVariant: string }

module Msuf4RandMutateDto =

    let fromDomain (msuf4Rm: msuf4RandMutate) : msuf4RandMutateDto =
        { id = %msuf4Rm.Id
          rngFactoryDto = msuf4Rm.RngFactory |> RngFactoryDto.fromDomain
          uf4MutationRatesArrayDtos = Uf4MutationRatesDto.fromDomain msuf4Rm.Uf4MutationRates 
          mutVariant = msuf4Rm.MutatorVariant |> MutatorVariant.toString 
        }

    let toDomain (dto: msuf4RandMutateDto) : msuf4RandMutate =
        msuf4RandMutate.create 
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (dto.uf4MutationRatesArrayDtos |> Uf4MutationRatesDto.toDomain)
            (dto.mutVariant |> MutatorVariant.fromString)
