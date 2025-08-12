namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Sorter

[<MessagePackObject>]
type Msuf6RandGenDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] RngType: rngType
      [<Key(2)>] SortingWidth: int
      [<Key(3)>] StageCount: int
      [<Key(4)>] GenRates: Uf6GenRatesArrayDto }

module Msuf6RandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf6RandGenDto (msuf6RandGen: Msuf6RandGen) : Msuf6RandGenDto =
        { Id = %msuf6RandGen.Id
          RngType = msuf6RandGen.RngType
          SortingWidth = %msuf6RandGen.SortingWidth
          StageCount = %msuf6RandGen.StageCount
          GenRates = Uf6GenRatesArrayDto.toUf6GenRatesArrayDto msuf6RandGen.GenRates }

    let fromMsuf6RandGenDto (dto: Msuf6RandGenDto) : Msuf6RandGen =
        try
            if dto.SortingWidth < 6 || dto.SortingWidth % 6 <> 0 then
                failwith $"SortingWidth must be at least 6 and divisible by 6, got {dto.SortingWidth}"
            if dto.StageCount < 1 then
                failwith $"StageCount must be at least 1, got {dto.StageCount}"
            let genRates = Uf6GenRatesArrayDto.fromUf6GenRatesArrayDto dto.GenRates
            if genRates.Length <> dto.StageCount then
                failwith $"GenRates array length ({genRates.Length}) must equal StageCount ({dto.StageCount})"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.order <> dto.SortingWidth) then
                failwith $"All GenRates must have order {dto.SortingWidth}"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.opsGenRatesArray.Length <> MathUtils.exactLog2(gr.order / 6)) then
                failwith "opsGenRatesArray length must equal log2(order/6)"

            Msuf6RandGen.create dto.RngType (UMX.tag<sortingWidth> dto.SortingWidth) (UMX.tag<stageCount> dto.StageCount) genRates

        with
        | ex -> failwith $"Failed to convert Msuf6RandGenDto: {ex.Message}"