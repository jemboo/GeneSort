namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorter.Uf4
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Component

[<MessagePackObject>]
type msuf4RandGenDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] rngType: rngType
      [<Key(2)>] sortingWidth: int
      [<Key(3)>] stageLength: int
      [<Key(4)>] uf4GenRatesArrayDto: Uf4GenRatesArrayDto }

module Msuf4RandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsuf4RandGenDto (msuf4RandGen: msuf4RandGen) : msuf4RandGenDto =
        { id = %msuf4RandGen.Id
          rngType = msuf4RandGen.RngType
          sortingWidth = %msuf4RandGen.SortingWidth
          stageLength = %msuf4RandGen.StageLength
          uf4GenRatesArrayDto = Uf4GenRatesArrayDto.fromDomain msuf4RandGen.GenRates }

    let fromMsuf4RandGenDto (dto: msuf4RandGenDto) : msuf4RandGen =
        try
            if dto.sortingWidth < 1 then
                failwith $"SortingWidth must be at least 1, got {dto.sortingWidth}"
            if (dto.sortingWidth - 1) &&& dto.sortingWidth <> 0 then
                failwith $"SortingWidth must be a power of 2, got {dto.sortingWidth}"
            if dto.stageLength < 1 then
                failwith $"StageLength must be at least 1, got {dto.stageLength}"
            let genRates = Uf4GenRatesArrayDto.toDomain dto.uf4GenRatesArrayDto
            if genRates.Length <> dto.stageLength then
                failwith $"GenRates array length ({genRates.Length}) must equal StageLength ({dto.stageLength})"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.Order <> dto.sortingWidth) then
                failwith $"All GenRates must have order {dto.sortingWidth}"
            if genRates.RatesArray |> Array.exists (fun gr -> gr.OpsGenRatesArray.Length <> MathUtils.exactLog2(gr.Order / 4)) then
                failwith "opsGenRatesArray length must equal log2(order/4)"

            msuf4RandGen.create dto.rngType (UMX.tag<sortingWidth> dto.sortingWidth) (UMX.tag<stageLength> dto.stageLength) genRates
        with
        | ex -> failwith $"Failed to convert Msuf4RandGenDto: {ex.Message}"