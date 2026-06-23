namespace GeneSort.Model.Mp.Sorting.Sorter.Rs

open FSharp.UMX
open GeneSort.Model.Mp.Sorting.Sorter.Rs
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Core.Mp

[<MessagePackObject>]
type msrsRandMutateDto = 
    { [<Key(0)>] msrsDto: msrsDto
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] opsActionRatesArray: opsActionRatesArrayDto }

module MsrsRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msrsRandMutate: msrsRandMutate) : msrsRandMutateDto =
        { msrsDto = MsrsDto.fromDomain msrsRandMutate.Msrs
          rngFactoryDto = msrsRandMutate.RngFactory |> RngFactoryDto.fromDomain
          opsActionRatesArray = OpsActionRatesArrayDto.fromDomain msrsRandMutate.OpsActionRates }

    let toDomain (dto: msrsRandMutateDto) : msrsRandMutate =
        try
            let msrs = MsrsDto.toDomain dto.msrsDto
            
            if %msrs.StageLength <> dto.opsActionRatesArray.opsActionRatesDtos.Length then
                failwith $"StageLength ({%msrs.StageLength}) must match OpsActionRatesArray length ({dto.opsActionRatesArray.opsActionRatesDtos.Length})"
            
            msrsRandMutate.create
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                (OpsActionRatesArrayDto.toDomain dto.opsActionRatesArray)
                msrs
        with
        | ex -> failwith $"Failed to convert MsrsRandMutateDto: {ex.Message}"