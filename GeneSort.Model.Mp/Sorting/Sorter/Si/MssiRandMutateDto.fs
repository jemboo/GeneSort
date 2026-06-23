namespace GeneSort.Model.Mp.Sorting.Sorter.Si

open FSharp.UMX
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp

[<MessagePackObject>]
type mssiRandMutateDto = 
    { [<Key(0)>] mssiDto: mssiDto
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] opActionRatesArrayDto: opActionRatesArrayDto }

module MssiRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (mssiRandMutate: mssiRandMutate) : mssiRandMutateDto =
        { mssiDto = MssiDto.fromDomain mssiRandMutate.Mssi
          rngFactoryDto = mssiRandMutate.RngFactory |> RngFactoryDto.fromDomain
          opActionRatesArrayDto = OpActionRatesArrayDto.fromDomain mssiRandMutate.OpActionRates }

    let toDomain (dto: mssiRandMutateDto) : mssiRandMutate =
        try
            let mssi = MssiDto.toDomain dto.mssiDto
            let opActionRates = OpActionRatesArrayDto.toDomain dto.opActionRatesArrayDto
            
            if %mssi.StageLength <> opActionRates.Length then
                failwith $"StageLength ({%mssi.StageLength}) must match OpActionRatesArray length ({opActionRates.Length})"
            
            mssiRandMutate.create
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                opActionRates
                mssi
        with
        | ex -> failwith $"Failed to convert MssiRandMutateDto: {ex.Message}"