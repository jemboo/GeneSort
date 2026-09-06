

namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

type msrsRandGenDto = 
    { sortingWidth: int
      stageLength: int 
      rngFactoryDto: rngFactoryDto
      opsGenRatesDto: opsGenRatesDto }

module MsrsRandGenDto =

    let fromDomain (msrsRandGen: msrsRandGen) : msrsRandGenDto =
        { sortingWidth = %msrsRandGen.SortingWidth
          stageLength = %msrsRandGen.StageLength
          rngFactoryDto = msrsRandGen.RngFactory |> RngFactoryDto.fromDomain
          opsGenRatesDto = OpsGenRatesDto.fromDomain msrsRandGen.OpsGenRates }

    let toDomain (dto: msrsRandGenDto) : msrsRandGen =
        msrsRandGen.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (UMX.tag<sortingWidth> dto.sortingWidth)
            (OpsGenRatesDto.toDomain dto.opsGenRatesDto)
            (UMX.tag<stageLength> dto.stageLength)

