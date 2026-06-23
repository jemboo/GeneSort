namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf6

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Core.Mp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Sorting

[<MessagePackObject>]
type msuf6RandGenDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] sortingWidth: int
      [<Key(3)>] stageLength: int
      [<Key(4)>] uf6GenRatesDto: uf6GenRatesDto }

module Msuf6RandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msuf6RandGen: msuf6RandGen) : msuf6RandGenDto =
        { id = %msuf6RandGen.Id
          rngFactoryDto = msuf6RandGen.RngFactory |> RngFactoryDto.fromDomain
          sortingWidth = %msuf6RandGen.SortingWidth
          stageLength = %msuf6RandGen.StageLength
          uf6GenRatesDto = Uf6GenRatesDto.fromDomain msuf6RandGen.GenRates }

    let toDomain (dto: msuf6RandGenDto) : msuf6RandGen =
        try
            if dto.sortingWidth < 6 || dto.sortingWidth % 6 <> 0 then
                failwith $"SortingWidth must be at least 6 and divisible by 6, got {dto.sortingWidth}"
            if dto.stageLength < 1 then
                failwith $"StageLength must be at least 1, got {dto.stageLength}"
            let genRates = Uf6GenRatesDto.toDomain dto.uf6GenRatesDto

            msuf6RandGen.create 
                    (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                    (UMX.tag<sortingWidth> dto.sortingWidth) 
                    (UMX.tag<stageLength> dto.stageLength) 
                    genRates

        with
        | ex -> failwith $"Failed to convert Msuf6RandGenDto: {ex.Message}"