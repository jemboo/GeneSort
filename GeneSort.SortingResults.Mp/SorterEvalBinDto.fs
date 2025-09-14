namespace GeneSort.SortingResults.Mp

open FSharp.UMX
open MessagePack
open GeneSort.Sorter
open GeneSort.SortingResults
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp

[<MessagePackObject>]
type sorterEvalKeyDto = {
    [<Key(0)>]
    CeCount: int
    [<Key(1)>]
    StageCount: int
}

module SorterEvalKeyDto =
    let toSorterEvalKeyDto (key: sorterEvalKey) : sorterEvalKeyDto =
        { 
            CeCount = %key.ceCount
            StageCount = %key.stageCount
        }

    let fromSorterEvalKeyDto (dto: sorterEvalKeyDto) : sorterEvalKey =
        if dto.CeCount < 0 then
            failwith "CeCount must not be negative"
        if dto.StageCount < 0 then
            failwith "StageCount must not be negative"
        { 
            ceCount = UMX.tag<ceLength> dto.CeCount
            stageCount = UMX.tag<stageLength> dto.StageCount
            //unsortedCount = 0
        }

[<MessagePackObject>]
type sorterEvalBinDto = {
    [<Key(0)>]
    BinCount: int
    [<Key(1)>]
    SorterEvals: sorterEvalDto array
}

module SorterEvalBinDto =

    let fromDomain (bin: sorterEvalBin) : sorterEvalBinDto =
        { 
            BinCount = bin.binCount
            SorterEvals = bin.sorterEvals.ToArray() |> Array.map SorterEvalDto.toSorterEvalDto
        }

    let toDomain (dto: sorterEvalBinDto) : sorterEvalBin =
        if dto.BinCount < 0 then
            failwith "BinCount must not be negative"
        { 
            binCount = dto.BinCount
            sorterEvals = ResizeArray<sorterEval>(dto.SorterEvals |> Array.map SorterEvalDto.fromSorterEvalDto)
        }
