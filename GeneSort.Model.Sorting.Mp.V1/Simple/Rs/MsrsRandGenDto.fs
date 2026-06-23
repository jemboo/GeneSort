namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp

[<MessagePackObject>]
type msrsRandGenDto = 
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] stageLength: int 
      [<Key(2)>] rngFactoryDto: rngFactoryDto
      [<Key(3)>] opsGenRatesDto: opsGenRatesDto }

module MsrsRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

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