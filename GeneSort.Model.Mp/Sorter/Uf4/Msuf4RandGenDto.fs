namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter.Uf4
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Sorter

[<MessagePackObject>]
type Msuf4RandGenDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] RngType: rngType
      [<Key(2)>] SortingWidth: int
      [<Key(3)>] StageCount: int
      [<Key(4)>] GenRates: Uf4GenRatesArrayDto }

module Msuf4RandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf4RandGenDto (msuf4RandGen: Msuf4RandGen) : Msuf4RandGenDto =
        { Id = %msuf4RandGen.Id
          RngType = msuf4RandGen.RngType
          SortingWidth = %msuf4RandGen.SortingWidth
          StageCount = %msuf4RandGen.StageCount
          GenRates = Uf4GenRatesArrayDto.fromDomain msuf4RandGen.GenRates }

    let fromMsuf4RandGenDto (dto: Msuf4RandGenDto) : Msuf4RandGen =
        try
            if dto.SortingWidth < 1 then
                failwith $"SortingWidth must be at least 1, got {dto.SortingWidth}"
            if (dto.SortingWidth - 1) &&& dto.SortingWidth <> 0 then
                failwith $"SortingWidth must be a power of 2, got {dto.SortingWidth}"
            if dto.StageCount < 1 then
                failwith $"StageCount must be at least 1, got {dto.StageCount}"
            let genRates = Uf4GenRatesArrayDto.toDomain dto.GenRates
            if genRates.Length <> dto.StageCount then
                failwith $"GenRates array length ({genRates.Length}) must equal StageCount ({dto.StageCount})"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.Order <> dto.SortingWidth) then
                failwith $"All GenRates must have order {dto.SortingWidth}"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.OpsGenRatesArray.Length <> MathUtils.exactLog2(gr.Order / 4)) then
                failwith "opsGenRatesArray length must equal log2(order/4)"

            Msuf4RandGen.create dto.RngType (UMX.tag<sortingWidth> dto.SortingWidth) (UMX.tag<stageCount> dto.StageCount) genRates
        with
        | ex -> failwith $"Failed to convert Msuf4RandGenDto: {ex.Message}"