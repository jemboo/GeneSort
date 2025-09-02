namespace GeneSort.Model.Mp.Sorter.Rs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter.Rs
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type MsrsRandGenDto = 
    { [<Key(0)>] SortingWidth: int
      [<Key(1)>] RngType: rngType
      [<Key(2)>] OpsGenRatesArray: OpsGenRatesArrayDto }

module MsrsRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsrsRandGenDto (msrsRandGen: MsrsRandGen) : MsrsRandGenDto =
        { SortingWidth = %msrsRandGen.SortingWidth
          RngType = msrsRandGen.RngType
          OpsGenRatesArray = OpsGenRatesArrayDto.toDomain msrsRandGen.OpsGenRatesArray }

    let fromMsrsRandGenDto (dto: MsrsRandGenDto) : MsrsRandGen =
        try
            if dto.SortingWidth < 2 then
                failwith $"SortingWidth must be at least 2, got {dto.SortingWidth}"
            if (dto.OpsGenRatesArray.Rates.Length) < 1 then
                failwith $"OpsGenRatesArray must have at least 1 rate, got {dto.OpsGenRatesArray.Rates.Length}"
            MsrsRandGen.create
                (dto.RngType)
                (UMX.tag<sortingWidth> dto.SortingWidth)
                (OpsGenRatesArrayDto.fromDomain dto.OpsGenRatesArray)
        with
        | ex -> failwith $"Failed to convert MsrsRandGenDto: {ex.Message}"