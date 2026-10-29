

namespace GeneSort.Model.Sorting.Mp.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Core.Mp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Rs

type msrsRandGenDto = 
    { channelCount: int
      stageLength: int 
      rngFactoryDto: rngFactoryDto
      opsGenRatesDto: opsGenRatesDto }

module MsrsRandGenDto =

    let fromDomain (msrsRandGen: msrsRandGen) : msrsRandGenDto =
        { channelCount = %msrsRandGen.ChannelCount
          stageLength = %msrsRandGen.StageLength
          rngFactoryDto = msrsRandGen.RngFactory |> RngFactoryDto.fromDomain
          opsGenRatesDto = OpsGenRatesDto.fromDomain msrsRandGen.OpsGenRates }

    let toDomain (dto: msrsRandGenDto) : msrsRandGen =
        msrsRandGen.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (UMX.tag<channelCount> dto.channelCount)
            (OpsGenRatesDto.toDomain dto.opsGenRatesDto)
            (UMX.tag<stageLength> dto.stageLength)

