namespace GeneSort.Model.Mp.Sorter.Uf6

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Sorting

[<MessagePackObject>]
type msuf6RandGenDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngType: rngType
      [<Key(2)>] sortingWidth: int
      [<Key(3)>] stageLength: int
      [<Key(4)>] uf6GenRatesArrayDto: Uf6GenRatesArrayDto }

module Msuf6RandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf6RandGenDto (msuf6RandGen: msuf6RandGen) : msuf6RandGenDto =
        { id = %msuf6RandGen.Id
          rngType = msuf6RandGen.RngType
          sortingWidth = %msuf6RandGen.SortingWidth
          stageLength = %msuf6RandGen.StageLength
          uf6GenRatesArrayDto = Uf6GenRatesArrayDto.fromDomain msuf6RandGen.GenRates }

    let fromMsuf6RandGenDto (dto: msuf6RandGenDto) : msuf6RandGen =
        try
            if dto.sortingWidth < 6 || dto.sortingWidth % 6 <> 0 then
                failwith $"SortingWidth must be at least 6 and divisible by 6, got {dto.sortingWidth}"
            if dto.stageLength < 1 then
                failwith $"StageLength must be at least 1, got {dto.stageLength}"
            let genRates = Uf6GenRatesArrayDto.toDomain dto.uf6GenRatesArrayDto
            if genRates.Length <> dto.stageLength then
                failwith $"GenRates array length ({genRates.Length}) must equal StageLength ({dto.stageLength})"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.Order <> dto.sortingWidth) then
                failwith $"All GenRates must have order {dto.sortingWidth}"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.OpsGenRatesArray.Length <> MathUtils.exactLog2(gr.Order / 6)) then
                failwith "opsGenRatesArray length must equal log2(order/6)"

            msuf6RandGen.create dto.rngType (UMX.tag<sortingWidth> dto.sortingWidth) (UMX.tag<stageLength> dto.stageLength) genRates

        with
        | ex -> failwith $"Failed to convert Msuf6RandGenDto: {ex.Message}"