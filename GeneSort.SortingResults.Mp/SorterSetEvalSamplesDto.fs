namespace GeneSort.SortingResults.Mp

open System
open System.Collections.Generic
open FSharp.UMX
open MessagePack
open GeneSort.SortingResults
open GeneSort.SortingOps


[<MessagePackObject>]
type sorterSetEvalSamplesDto = {
    [<Key(0)>]
    SorterSetEvalId: Guid
    [<Key(1)>]
    TotalSampleCount: int
    [<Key(2)>]
    MaxBinCount: int
    [<Key(3)>]
    EvalBins: (sorterEvalKeyDto * sorterEvalBinDto) array
}

module SorterSetEvalSamplesDto =
    let fromDomain (samples: sorterSetEvalSamples) : sorterSetEvalSamplesDto =
        { 
            SorterSetEvalId = %samples.sorterSetEvalId
            TotalSampleCount = samples.totalSampleCount
            MaxBinCount = samples.maxBinCount
            EvalBins = 
                samples.evalBins
                |> Seq.map (fun kvp -> (SorterEvalKeyDto.toSorterEvalKeyDto kvp.Key, SorterEvalBinDto.fromDomain kvp.Value))
                |> Seq.toArray
        }

    let toDomain (dto: sorterSetEvalSamplesDto) : sorterSetEvalSamples =
        if dto.SorterSetEvalId = Guid.Empty then
            failwith "SorterSetEvalId must not be empty"
        if dto.TotalSampleCount < 0 then
            failwith "TotalSampleCount must not be negative"
        if dto.MaxBinCount < 1 then
            failwith "MaxBinCount must be at least 1"
        let evalBins = Dictionary<sorterEvalKey, sorterEvalBin>()
        for (keyDto, binDto) in dto.EvalBins do
            evalBins.[SorterEvalKeyDto.fromSorterEvalKeyDto keyDto] <- SorterEvalBinDto.toDomain binDto
        { 
            sorterSetEvalId = UMX.tag<sorterSetEvalId> dto.SorterSetEvalId
            totalSampleCount = dto.TotalSampleCount
            maxBinCount = dto.MaxBinCount
            evalBins = evalBins
        }

