
namespace GeneSort.Model.Sorting.Mp.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1

type msrsRandMutateDto = 
    { rngFactoryDto: rngFactoryDto
      opsActionRates: opsActionRatesDto 
      mutVariant: string }

module MsrsRandMutateDto =

    let fromDomain (msrsRm: msrsRandMutate) : msrsRandMutateDto =
        { 
          rngFactoryDto = msrsRm.RngFactory |> RngFactoryDto.fromDomain
          opsActionRates = OpsActionRatesDto.fromDomain msrsRm.OpsActionRates 
          mutVariant = msrsRm.MutatorVariant |> MutatorVariant.toString 
         }

    let toDomain (dto: msrsRandMutateDto) : msrsRandMutate =
        msrsRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (OpsActionRatesDto.toDomain dto.opsActionRates)
            (dto.mutVariant |> MutatorVariant.fromString)

