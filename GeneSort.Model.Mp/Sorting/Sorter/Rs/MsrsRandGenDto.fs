namespace GeneSort.Model.Mp.Sorting.Sorter.Rs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp

[<MessagePackObject>]
type msrsRandGenDto = 
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] opsGenRatesArrayDto: opsGenRatesArrayDto }

module MsrsRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msrsRandGen: msrsRandGen) : msrsRandGenDto =
        { sortingWidth = %msrsRandGen.SortingWidth
          rngFactoryDto = msrsRandGen.RngFactory |> RngFactoryDto.fromDomain
          opsGenRatesArrayDto = OpsGenRatesArrayDto.fromDomain msrsRandGen.OpsGenRatesArray }

    let toDomain (dto: msrsRandGenDto) : msrsRandGen =
        try
            if dto.sortingWidth < 2 then
                failwith $"SortingWidth must be at least 2, got {dto.sortingWidth}"
            if (dto.opsGenRatesArrayDto.opsGenRatesDtos.Length) < 1 then
                failwith $"OpsGenRatesArray must have at least 1 rate, got {dto.opsGenRatesArrayDto.opsGenRatesDtos.Length}"
            msrsRandGen.create
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (UMX.tag<sortingWidth> dto.sortingWidth)
                (OpsGenRatesArrayDto.toDomain dto.opsGenRatesArrayDto)
        with
        | ex -> failwith $"Failed to convert MsrsRandGenDto: {ex.Message}"