namespace GeneSort.Model.Sorting.Mp.V1.Simple.Ce

open FSharp.UMX
open GeneSort.Core.Mp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Ce

type msceRandMutateDto = 
    { rngFactoryDto: rngFactoryDto
      indelRatesDto: indelRatesDto
      excludeSelfCe: bool
      mutVariant: string }

module MsceRandMutateDto =
    
    let fromDomain (msceRm: msceRandMutate) : msceRandMutateDto =
        { 
          rngFactoryDto = msceRm.RngFactory |> RngFactoryDto.fromDomain
          indelRatesDto = IndelRatesDto.fromDomain msceRm.IndelRates
          excludeSelfCe = %msceRm.ExcludeSelfCe 
          mutVariant = msceRm.MutatorVariant |> MutatorVariant.toString 
        }
    
    let toDomain (dto: msceRandMutateDto) : msceRandMutate =
        msceRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (dto.indelRatesDto |> IndelRatesDto.toDomain)
            (dto.excludeSelfCe |> UMX.tag)
            (dto.mutVariant |> MutatorVariant.fromString)

