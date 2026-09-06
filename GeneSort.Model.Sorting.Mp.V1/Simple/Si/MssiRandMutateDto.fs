
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si

open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

type mssiRandMutateDto = 
    { rngFactoryDto: rngFactoryDto
      opActionRatesDto: opActionRatesDto }

module MssiRandMutateDto =

    let fromDomain (mssiRandMutate: mssiRandMutate) : mssiRandMutateDto =
        { rngFactoryDto = mssiRandMutate.RngFactory |> RngFactoryDto.fromDomain
          opActionRatesDto = OpActionRatesDto.fromDomain mssiRandMutate.OpActionRates }

    let toDomain (dto: mssiRandMutateDto) : mssiRandMutate =
        mssiRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (OpActionRatesDto.toDomain dto.opActionRatesDto)

