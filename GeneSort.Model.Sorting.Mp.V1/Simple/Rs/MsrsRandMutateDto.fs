
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp
open GeneSort.Model.Sorting.V1.Simple.Rs

type msrsRandMutateDto = 
    { rngFactoryDto: rngFactoryDto
      opsActionRates: opsActionRatesDto }

module MsrsRandMutateDto =

    let fromDomain (msrsRandMutate: msrsRandMutate) : msrsRandMutateDto =
        { rngFactoryDto = msrsRandMutate.RngFactory |> RngFactoryDto.fromDomain
          opsActionRates = OpsActionRatesDto.fromDomain msrsRandMutate.OpsActionRates }

    let toDomain (dto: msrsRandMutateDto) : msrsRandMutate =
        msrsRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (OpsActionRatesDto.toDomain dto.opsActionRates)

