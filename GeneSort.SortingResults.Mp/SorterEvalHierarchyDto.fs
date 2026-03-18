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
// Leaf DTO: sorterEval removed; cesLowHi encodes the ceSequenceKey
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalLeafDto = {
    [<Key(0)>]
    cesLowHi:  int array     // interleaved low,hi pairs encoding the ceSequenceKey
    [<Key(1)>]
    sorterIds: Guid array
}

module SorterEvalLeafDto =

    let toDto (key: ceSequenceKey) (leaf: sorterEvalLeaf) : sorterEvalLeafDto =
        {
            cesLowHi  = key.Ces |> Array.collect (fun c -> [| c.Low; c.Hi |])
            sorterIds = leaf.SorterIds |> Seq.map UMX.untag |> Seq.toArray
        }

    let fromDto (dto: sorterEvalLeafDto) : ceSequenceKey * sorterEvalLeaf =
        let ces =
            dto.cesLowHi
            |> Array.chunkBySize 2
            |> Array.map (fun pair -> ce.create pair.[0] pair.[1])
        let key  = ceSequenceKey.create ces
        let leaf =
            match dto.sorterIds with
            | [||] -> failwith "Cannot reconstruct sorterEvalLeaf with no sorterIds."
            | ids  -> sorterEvalLeaf.createWithIds(ids |> Array.map (UMX.tag<sorterId>))
        key, leaf

// ---------------------------------------------------------------------------
// Layer DTO
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalLayerDto = {
    [<Key(0)>]
    sorterEvalKey: sorterEvalKeyDto
    [<Key(1)>]
    leaves: sorterEvalLeafDto array
}

module SorterEvalLayerDto =

    let toDto (layer: sorterEvalLayer) : sorterEvalLayerDto =
        {
            sorterEvalKey = SorterEvalKeyDto.toDto layer.Key
            leaves =
                layer.Leaves
                |> Seq.map (fun kvp -> SorterEvalLeafDto.toDto kvp.Key kvp.Value)
                |> Seq.toArray
        }

    let fromDto (dto: sorterEvalLayerDto) : sorterEvalLayer =
        let key   = SorterEvalKeyDto.fromDto dto.sorterEvalKey
        let layer = sorterEvalLayer.create(key)
        for leafDto in dto.leaves do
            let (ceSeqKey, leaf) = SorterEvalLeafDto.fromDto leafDto
            layer.MergeLeaf ceSeqKey leaf
        layer

// ---------------------------------------------------------------------------
// Hierarchy DTO
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalHierarchyDto = {
    [<Key(0)>]
    sorterEvalHierarchyId: Guid
    [<Key(1)>]
    layers: sorterEvalLayerDto array
}

module SorterEvalHierarchyDto =

    let fromDomain (hierarchy: sorterEvalHierarchy) : sorterEvalHierarchyDto =
        {
            sorterEvalHierarchyId = %hierarchy.SorterEvalHierarchyId
            layers =
                hierarchy.Layers
                |> Seq.map (fun kvp -> SorterEvalLayerDto.toDto kvp.Value)
                |> Seq.toArray
        }

    let toDomain (dto: sorterEvalHierarchyDto) : sorterEvalHierarchy =
        let hierarchy =
            sorterEvalHierarchy.create (UMX.tag<sorterEvalHierarchyId> dto.sorterEvalHierarchyId)
        for layerDto in dto.layers do
            hierarchy.MergeLayer (SorterEvalLayerDto.fromDto layerDto)
        hierarchy

    let serialize (options: MessagePackSerializerOptions)
                  (hierarchy: sorterEvalHierarchy) : byte array =
        hierarchy
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : sorterEvalHierarchy =
        MessagePackSerializer.Deserialize<sorterEvalHierarchyDto>(data, options)
        |> toDomain