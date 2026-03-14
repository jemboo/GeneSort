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

[<MessagePackObject>]
type sorterEvalSubBinDto = {
    [<Key(0)>]
    hashKey: int
    [<Key(1)>]
    sorterIds: Guid array
    [<Key(2)>]
    representativeEvals: sorterEvalDto array 
}

module SorterEvalSubBinDto =
    let toDto (hashKey: int) (subBin: sorterEvalSubBin) : sorterEvalSubBinDto =
        {
            hashKey = hashKey
            sorterIds = subBin.SorterIds |> Seq.map UMX.untag |> Seq.toArray
            representativeEvals = 
                subBin.RepresentativeEvals 
                |> Seq.map SorterEvalDto.toSorterEvalDto 
                |> Seq.toArray
        }

    let fromDto (dto: sorterEvalSubBinDto) : int * sorterEvalSubBin =
        match dto.representativeEvals |> Array.tryHead with
        | None -> failwith "Cannot reconstruct sub-bin without at least one representative evaluation."
        | Some firstEvalDto ->
            // Convert the first DTO back to domain to initialize the sub-bin
            let firstEval = SorterEvalDto.fromSorterEvalDto firstEvalDto
            let subBin = sorterEvalSubBin.create(firstEval)
            
            // Add remaining representative evaluations
            dto.representativeEvals 
            |> Array.skip 1 
            |> Array.iter (fun repDto -> 
                let rep = SorterEvalDto.fromSorterEvalDto repDto
                subBin.Add(rep, Int32.MaxValue))

            // Hydrate the full ID list, avoiding the ID already added by create()
            let firstId = firstEval.SorterId
            for id in dto.sorterIds do
                let taggedId = UMX.tag<sorterId> id
                if taggedId <> firstId then
                    subBin.AddIdOnly(taggedId)
            
            (dto.hashKey, subBin)

[<MessagePackObject>]
type sorterEvalBinDto = {
    [<Key(0)>]
    subBins: sorterEvalSubBinDto array
}

module SorterEvalBinDto =
    let toDto (bin: sorterEvalBin) : sorterEvalBinDto =
        {
            subBins =
                bin.SubBins
                |> Seq.map (fun kvp -> SorterEvalSubBinDto.toDto kvp.Key kvp.Value)
                |> Seq.toArray
        }
    let fromDto (dto: sorterEvalBinDto, maxReps: int) : sorterEvalBin =
        let bin = sorterEvalBin.create()
        for subBinDto in dto.subBins do
            let (hashKey, subBin) = SorterEvalSubBinDto.fromDto subBinDto
            bin.MergeSubBin hashKey (subBin, maxReps)
        bin

[<MessagePackObject>]
type sorterSetEvalBinsDto = {
    [<Key(0)>]
    sorterSetEvalId: Guid
    [<Key(1)>]
    bins: (sorterEvalKeyDto * sorterEvalBinDto) array
    [<Key(2)>]
    maxRepresentativesPerSubBin: int
}

module SorterSetEvalBinsDto =

    let fromDomain (sorterSetEvalBins: sorterSetEvalBins) : sorterSetEvalBinsDto =
        {
            sorterSetEvalId = %sorterSetEvalBins.SorterSetEvalId
            maxRepresentativesPerSubBin = sorterSetEvalBins.MaxRepresentativesPerSubBin
            bins =
                sorterSetEvalBins.EvalBins
                |> Seq.map (fun kvp ->
                    (SorterEvalKeyDto.toDto kvp.Key, SorterEvalBinDto.toDto kvp.Value))
                |> Seq.toArray
        }

    let toDomain (dto: sorterSetEvalBinsDto) : sorterSetEvalBins =
        let bins = sorterSetEvalBins.create (UMX.tag<sorterSetEvalId> dto.sorterSetEvalId, dto.maxRepresentativesPerSubBin)
        for (keyDto, binDto) in dto.bins do
            let key = SorterEvalKeyDto.fromDto keyDto
            let bin = SorterEvalBinDto.fromDto (binDto, dto.maxRepresentativesPerSubBin)
            bins.MergeBin key bin
        bins

    let serialize (options: MessagePackSerializerOptions) 
                  (sorterSetEvalBins: sorterSetEvalBins) : byte array =
        sorterSetEvalBins
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions) 
                    (data: byte array) : sorterSetEvalBins =
        MessagePackSerializer.Deserialize<sorterSetEvalBinsDto>(data, options)
        |> toDomain