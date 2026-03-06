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
        if dto.ceLength < 0 then
            failwith "CeLength must not be negative"
        if dto.stageLength < 0 then
            failwith "StageLength must not be negative"
        sorterEvalKey.create
            (UMX.tag<ceLength> dto.ceLength)
            (UMX.tag<stageLength> dto.stageLength)
            dto.isSorted


[<MessagePackObject>]
type sorterEvalSubBinDto = {
    [<Key(0)>]
    ceUseCounts: int array
    [<Key(1)>]
    sorterIds: System.Guid array
}

module SorterEvalSubBinDto =
    let toDto (ceUseCounts: ceUseCounts) (subBin: sorterEvalSubBin) : sorterEvalSubBinDto =
        {
            ceUseCounts = ceUseCounts.ToArray()
            sorterIds = subBin.SorterIds |> Seq.map UMX.untag |> Seq.toArray
        }
    let fromDto (dto: sorterEvalSubBinDto) : ceUseCounts * sorterEvalSubBin =
        let ceUseCounts = ceUseCounts.CreateFromArray dto.ceUseCounts
        let subBin = sorterEvalSubBin.create()
        for id in dto.sorterIds do
            subBin.Add(UMX.tag<sorterId> id)
        (ceUseCounts, subBin)


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
    let fromDto (dto: sorterEvalBinDto) : sorterEvalBin =
        let bin = sorterEvalBin.create()
        for subBinDto in dto.subBins do
            let (ceUseCounts, subBin) = SorterEvalSubBinDto.fromDto subBinDto
            bin.MergeSubBin ceUseCounts subBin
        bin


[<MessagePackObject>]
type sorterSetEvalBinsDto = {
    [<Key(0)>]
    sorterSetEvalId: System.Guid
    [<Key(1)>]
    bins: (sorterEvalKeyDto * sorterEvalBinDto) array
}

module SorterSetEvalBinsDto =

    let fromDomain (sorterSetEvalBins: sorterSetEvalBins) : sorterSetEvalBinsDto =
        {
            sorterSetEvalId = %sorterSetEvalBins.SorterSetEvalId
            bins =
                sorterSetEvalBins.EvalBins
                |> Seq.map (fun kvp ->
                    (SorterEvalKeyDto.toDto kvp.Key, SorterEvalBinDto.toDto kvp.Value))
                |> Seq.toArray
        }

    let toDomain (dto: sorterSetEvalBinsDto) : sorterSetEvalBins =
        let bins = sorterSetEvalBins.create (UMX.tag<sorterSetEvalId> dto.sorterSetEvalId)
        for (keyDto, binDto) in dto.bins do
            let key = SorterEvalKeyDto.fromDto keyDto
            let bin = SorterEvalBinDto.fromDto binDto
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