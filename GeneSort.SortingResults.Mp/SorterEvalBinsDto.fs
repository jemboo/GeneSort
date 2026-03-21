namespace GeneSort.SortingResults.Mp

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.SortingResults
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp
open GeneSort.Sorting.Sorter

[<MessagePackObject>]
type sorterEvalKeyDto = {
    [<Key(0)>]
    ceLength: int
    [<Key(1)>]
    stageLength: int
    [<Key(2)>]
    isSorted: bool
}

module SorterEvalKeyDto =
    let toDto (key: sorterEvalKey) : sorterEvalKeyDto =
        { 
            ceLength    = %key.CeCount
            stageLength = %key.StageLength
            isSorted    = key.IsSorted
        }
    let fromDto (dto: sorterEvalKeyDto) : sorterEvalKey =
        if dto.ceLength < 0    then failwith "CeLength must not be negative"
        if dto.stageLength < 0 then failwith "StageLength must not be negative"
        sorterEvalKey.create
            (UMX.tag<ceLength>    dto.ceLength)
            (UMX.tag<stageLength> dto.stageLength)
            dto.isSorted


// ---------------------------------------------------------------------------
// Bins DTO: flat key → sorterIds, no ceSequenceKey dimension
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalBinsLeafDto = {
    [<Key(0)>]
    sorterEvalKey: sorterEvalKeyDto
    [<Key(1)>]
    sorterIds: Guid array
}

[<MessagePackObject>]
type sorterEvalBinsDto = {
    [<Key(0)>]
    sorterEvalBinsId: Guid
    [<Key(1)>]
    bins: sorterEvalBinsLeafDto array
}

module SorterEvalBinsDto =

    let fromDomain (bins: sorterEvalBins) : sorterEvalBinsDto =
        {
            sorterEvalBinsId = %bins.SorterEvalBinsId
            bins =
                bins.Layers
                |> Seq.map (fun kvp ->
                    {
                        sorterEvalKey = SorterEvalKeyDto.toDto kvp.Key
                        sorterIds     = kvp.Value.SorterIds |> Seq.map UMX.untag |> Seq.toArray
                    })
                |> Seq.toArray
        }

    let toDomain (dto: sorterEvalBinsDto) : sorterEvalBins =
        let bins = sorterEvalBins.create (UMX.tag<sorterEvalBinsId> dto.sorterEvalBinsId)
        for leafDto in dto.bins do
            let key  = SorterEvalKeyDto.fromDto leafDto.sorterEvalKey
            match leafDto.sorterIds with
            | [||] -> failwith "Cannot reconstruct sorterEvalBins entry with no sorterIds."
            | ids  ->
                let leaf = sorterEvalLeaf.createWithIds(ids |> Array.map (UMX.tag<sorterId>))
                bins.MergeLeaf key leaf
        bins

    let serialize (options: MessagePackSerializerOptions)
                  (bins: sorterEvalBins) : byte array =
        bins
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : sorterEvalBins =
        MessagePackSerializer.Deserialize<sorterEvalBinsDto>(data, options)
        |> toDomain