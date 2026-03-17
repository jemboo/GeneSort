namespace GeneSort.SortingResults.Mp

open System
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
    [<Key(2)>]
    isSorted: bool
}

module SorterEvalKeyDto =
    let toDto (key: sorterEvalKey) : sorterEvalKeyDto =
        { 
            ceLength = %key.CeCount
            stageLength = %key.StageLength
            isSorted = key.IsSorted
        }
    let fromDto (dto: sorterEvalKeyDto) : sorterEvalKey =
        if dto.ceLength < 0 then failwith "CeLength must not be negative"
        if dto.stageLength < 0 then failwith "StageLength must not be negative"
        sorterEvalKey.create
            (UMX.tag<ceLength> dto.ceLength)
            (UMX.tag<stageLength> dto.stageLength)
            dto.isSorted

// ---------------------------------------------------------------------------
// Leaf DTO: Updated for optional single representative
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalLeafDto = {
    [<Key(0)>]
    hashKey: int
    [<Key(1)>]
    sorterIds: Guid array
    [<Key(2)>]
    sorterEval: sorterEvalDto option 
}

module SorterEvalLeafDto =
    let toDto (hashKey: int) (leaf: sorterEvalLeaf) : sorterEvalLeafDto =
        {
            hashKey = hashKey
            sorterIds = leaf.SorterIds |> Seq.map UMX.untag |> Seq.toArray
            sorterEval = leaf.SorterEval |> Option.map SorterEvalDto.toSorterEvalDto
        }

    let fromDto (dto: sorterEvalLeafDto) : int * sorterEvalLeaf =
        // We handle the creation differently depending on if an eval exists
        let leaf = 
            match dto.sorterEval with
            | Some evalDto ->
                let eval = SorterEvalDto.fromSorterEvalDto evalDto
                let l = sorterEvalLeaf.create(eval, true)
                // Add the rest of the IDs, skipping the one added by create
                for id in dto.sorterIds do
                    let taggedId = UMX.tag<sorterId> id
                    if taggedId <> eval.SorterId then l.AddId(taggedId)
                l
            | None ->
                // If no eval, we use a dummy/temporary state to hydrate IDs.
                // Since domain 'create' requires an eval, we find the first ID.
                match dto.sorterIds |> Array.tryHead with
                | None -> failwith "Cannot reconstruct leaf with no IDs."
                | Some headId ->
                    // Note: This assumes a internal/private way to create an empty leaf 
                    // or we temporarily use a "Shell" eval that is never stored.
                    // Given the domain constraints, we'll assume the first ID is the source.
                    // If your domain 'create' is strictly tied to a full eval, 
                    // you may need a 'createEmpty' in the domain.
                    failwith "Reconstruction of leaf without a representative requires domain createEmpty."
        
        (dto.hashKey, leaf)

// ---------------------------------------------------------------------------
// Layer DTO
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalLayerDto = {
    [<Key(0)>]
    leaves: sorterEvalLeafDto array
}

module SorterEvalLayerDto =
    let toDto (layer: sorterEvalLayer) : sorterEvalLayerDto =
        {
            leaves =
                layer.Leaves
                |> Seq.map (fun kvp -> SorterEvalLeafDto.toDto kvp.Key kvp.Value)
                |> Seq.toArray
        }

    let fromDto (dto: sorterEvalLayerDto, maxReps: int) : sorterEvalLayer =
        let layer = sorterEvalLayer.create()
        for leafDto in dto.leaves do
            let (hashKey, leaf) = SorterEvalLeafDto.fromDto leafDto
            layer.MergeLeaf hashKey (leaf, UMX.tag<maxReps> maxReps)
        layer

// ---------------------------------------------------------------------------
// Hierarchy DTO: maxRepresentativesPerLayer
// ---------------------------------------------------------------------------
[<MessagePackObject>]
type sorterEvalHierarchyDto = {
    [<Key(0)>]
    sorterEvalHierarchyId: Guid
    [<Key(1)>]
    layers: (sorterEvalKeyDto * sorterEvalLayerDto) array
    [<Key(2)>]
    maxRepresentativesPerLayer: int
}

module SorterEvalHierarchyDto =

    let fromDomain (hierarchy: sorterEvalHierarchy) : sorterEvalHierarchyDto =
        {
            sorterEvalHierarchyId = %hierarchy.SorterEvalHierarchyId
            maxRepresentativesPerLayer = %hierarchy.MaxRepresentativesPerLayer
            layers =
                hierarchy.Layers
                |> Seq.map (fun kvp ->
                    (SorterEvalKeyDto.toDto kvp.Key, SorterEvalLayerDto.toDto kvp.Value))
                |> Seq.toArray
        }

    let toDomain (dto: sorterEvalHierarchyDto) : sorterEvalHierarchy =
        let hierarchy = sorterEvalHierarchy.create (
                            UMX.tag<sorterEvalHierarchyId> dto.sorterEvalHierarchyId, 
                            UMX.tag<maxReps> dto.maxRepresentativesPerLayer)
        for (keyDto, layerDto) in dto.layers do
            let key = SorterEvalKeyDto.fromDto keyDto
            let layer = SorterEvalLayerDto.fromDto (layerDto, dto.maxRepresentativesPerLayer)
            hierarchy.MergeLayer key layer
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