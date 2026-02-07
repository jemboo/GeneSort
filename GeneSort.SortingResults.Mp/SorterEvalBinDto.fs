namespace GeneSort.SortingResults.Mp

open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.SortingResults
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp

[<MessagePackObject>]
type sorterEvalKeyDto = {
    [<Key(0)>]
    ceLength: int
    [<Key(1)>]
    stageLength: int
}

module SorterEvalKeyDto =
    let toSorterEvalKeyDto (key: sorterEvalKey) : sorterEvalKeyDto =
        { 
            ceLength = %key.ceCount
            stageLength = %key.stageLength
        }

    let fromSorterEvalKeyDto (dto: sorterEvalKeyDto) : sorterEvalKey =
        if dto.ceLength < 0 then
            failwith "CeLength must not be negative"
        if dto.stageLength < 0 then
            failwith "StageLength must not be negative"
        { 
            ceCount = UMX.tag<ceLength> dto.ceLength
            stageLength = UMX.tag<stageLength> dto.stageLength
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
