namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf4

open System
open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Core.Mp
open GeneSort.Sorting

[<MessagePackObject>]
type msuf4RandGenDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] sortingWidth: int
      [<Key(3)>] stageLength: int
      [<Key(4)>] uf4GenRatesDto: uf4GenRatesDto }

module Msuf4RandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msuf4RandGen: msuf4RandGen) : msuf4RandGenDto =
        { id = %msuf4RandGen.Id
          rngFactoryDto = msuf4RandGen.RngFactory |> RngFactoryDto.fromDomain
          sortingWidth = %msuf4RandGen.SortingWidth
          stageLength = %msuf4RandGen.StageLength
          uf4GenRatesDto = Uf4GenRatesDto.fromDomain msuf4RandGen.GenRates }

    let toDomain (dto: msuf4RandGenDto) : msuf4RandGen =
        try
            if dto.sortingWidth < 1 then
                failwith $"SortingWidth must be at least 1, got {dto.sortingWidth}"
            if (dto.sortingWidth - 1) &&& dto.sortingWidth <> 0 then
                failwith $"SortingWidth must be a power of 2, got {dto.sortingWidth}"
            if dto.stageLength < 1 then
                failwith $"StageLength must be at least 1, got {dto.stageLength}"
            let genRates = Uf4GenRatesDto.toDomain dto.uf4GenRatesDto

            msuf4RandGen.create 
                    (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                    (UMX.tag<sortingWidth> dto.sortingWidth) 
                    (UMX.tag<stageLength> dto.stageLength) 
                    genRates
        with
        | ex -> failwith $"Failed to convert Msuf4RandGenDto: {ex.Message}"