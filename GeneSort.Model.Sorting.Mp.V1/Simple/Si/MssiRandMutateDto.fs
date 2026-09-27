
namespace GeneSort.Model.Sorting.Mp.V1.Simple.Si

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

type mssiRandMutateDto = 
    { rngFactoryDto: rngFactoryDto
      opActionRatesDto: opActionRatesDto 
      mutVariant: string }

module MssiRandMutateDto =

    let fromDomain (mssiRm: mssiRandMutate) : mssiRandMutateDto =
        { rngFactoryDto = mssiRm.RngFactory |> RngFactoryDto.fromDomain
          opActionRatesDto = OpActionRatesDto.fromDomain mssiRm.OpActionRates 
          mutVariant = mssiRm.MutatorVariant |> MutatorVariant.toString 
        }

    let toDomain (dto: mssiRandMutateDto) : mssiRandMutate =
        mssiRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (OpActionRatesDto.toDomain dto.opActionRatesDto)
            (dto.mutVariant |> MutatorVariant.fromString)

