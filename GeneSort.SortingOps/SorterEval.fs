namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable


type sorterEvalV1 =
    private { 
        sorterId: Guid<sorterId>
        sortingWidth: int<sortingWidth>
        unsortedCount: int<sortableCount>
        sequenceHash: int<sequenceHash>
        lastCeIndex: int<ceIndex>
        stageLength: int<stageLength>
        ceLength: int<ceLength>
    }

    static member create 
                    (sorterId: Guid<sorterId>)
                    (sortingWidth: int<sortingWidth>) 
                    (unsortedCount: int<sortableCount>)
                    (sequenceKey: int<sequenceHash>) 
                    (lastCeIndex: int<ceIndex>) 
                    (stageLength: int<stageLength>)
                    (ceLength: int<ceLength>) : sorterEvalV1 =
        { 
                sorterId = sorterId; 
                sortingWidth = sortingWidth;
                unsortedCount = unsortedCount;
                sequenceHash = sequenceKey; 
                lastCeIndex = lastCeIndex;
                stageLength = stageLength;
                ceLength = ceLength;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceLength
    member this.UnsortedCount with get() : int<sortableCount>  = this.unsortedCount
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.LastCeIndex with get() : int<ceIndex>  = this.lastCeIndex
    member this.ToDataTableRecord() : dataTableRecord =
            let isSorted = this.unsortedCount = 0<sortableCount>
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> dataTableRecord.addData "SortingWidth" (string %this.sortingWidth)
            |> dataTableRecord.addData "UnsortedCount" (string %this.unsortedCount)
            |> dataTableRecord.addData "StageLength" (string %this.stageLength)
            |> dataTableRecord.addData "CeLength" (string %this.ceLength)
            |> dataTableRecord.addData "IsSorted" (string isSorted)
            |> dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> dataTableRecord.addData "LastCeIndex" (string %this.lastCeIndex)



type ceData = 
    private {
        ceIndex :int<ceIndex>
        useCount :int
        ce: ce
    }
    static member create 
                (ceIndex: int<ceIndex>) 
                (useCount: int)
                (ce: ce): ceData =
        { ceIndex = ceIndex; useCount = useCount; ce = ce }
    member this.CeIndex with get() : int<ceIndex> = this.ceIndex
    member this.UseCount with get() : int = this.useCount
    member this.Ce with get() : ce = this.ce


type sorterEvalV2 =
    private { 
        sorterId: Guid<sorterId>
        sortingWidth: int<sortingWidth>
        unsortedCount: int<sortableCount>
        sequenceHash: int<sequenceHash>
        stageLength: int<stageLength>
        ceDataSequence: ceData array
    }

    static member create 
                    (sorterId: Guid<sorterId>) 
                    (sortingWidth: int<sortingWidth>) 
                    (unsortedCount: int<sortableCount>)
                    (sequenceKey: int<sequenceHash>)
                    (stageLength: int<stageLength>)
                    (ceDataSequence: ceData array): sorterEvalV2 =
        { 
                sorterId = sorterId; 
                sortingWidth = sortingWidth;
                unsortedCount = unsortedCount;
                sequenceHash = sequenceKey; 
                stageLength = stageLength;
                ceDataSequence = ceDataSequence;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceDataSequence.Length |> UMX.tag<ceLength>
    member this.CeDataSequence with get() : ceData array = this.ceDataSequence
    member this.UnsortedCount with get() : int<sortableCount>  = this.unsortedCount
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.LastCeIndex with get() : int<ceIndex>  = 
        if this.ceDataSequence.Length = 0 then 0<ceIndex>
        else this.ceDataSequence.[this.ceDataSequence.Length - 1].CeIndex
    member this.ToDataTableRecord() : dataTableRecord =
            let isSorted = this.unsortedCount = 0<sortableCount>
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> dataTableRecord.addData "SortingWidth" (string %this.sortingWidth)
            |> dataTableRecord.addData "UnsortedCount" (string %this.unsortedCount)
            |> dataTableRecord.addData "StageLength" (string %this.stageLength)
            |> dataTableRecord.addData "CeLength" (string %this.CeLength)
            |> dataTableRecord.addData "IsSorted" (string isSorted)
            |> dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> dataTableRecord.addData "LastCeIndex" (string %this.LastCeIndex)




type sorterEvalV3 =
    private { 
        sorterId: Guid<sorterId>
        sortingWidth: int<sortingWidth>
        sequenceHash: int<sequenceHash>
        stageLength: int<stageLength>
        ceDataSequence: ceData array
        sortableTest: sortableTest 
    }

    static member create 
                    (sorterId: Guid<sorterId>)
                    (sortingWidth: int<sortingWidth>) 
                    (sequenceKey: int<sequenceHash>)
                    (stageLength: int<stageLength>)
                    (ceDataSequence: ceData array) 
                    (sortableTest:sortableTest) :sorterEvalV3 =
        { 
                sorterId = sorterId;
                sortingWidth = sortingWidth;
                sequenceHash = sequenceKey; 
                stageLength = stageLength;
                ceDataSequence = ceDataSequence;
                sortableTest =sortableTest;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceDataSequence.Length |> UMX.tag<ceLength>
    member this.CeDataSequence with get() : ceData array = this.ceDataSequence
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.SortableTest with get() : sortableTest = this.sortableTest
    member this.UnsortedCount with get() : int<sortableCount>  = 
            this.sortableTest |> SortableTests.getUnsortedCount
    member this.LastCeIndex with get() : int<ceIndex>  = 
        if this.ceDataSequence.Length = 0 then 0<ceIndex>
        else this.ceDataSequence.[this.ceDataSequence.Length - 1].CeIndex
    member this.ToDataTableRecord() : dataTableRecord =
            let isSorted = this.UnsortedCount = 0<sortableCount>
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> dataTableRecord.addData "SortingWidth" (string %this.sortingWidth)
            |> dataTableRecord.addData "UnsortedCount" (string %this.UnsortedCount)
            |> dataTableRecord.addData "StageLength" (string %this.stageLength)
            |> dataTableRecord.addData "CeLength" (string %this.CeLength)
            |> dataTableRecord.addData "IsSorted" (string isSorted)
            |> dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> dataTableRecord.addData "LastCeIndex" (string %this.LastCeIndex)



type sorterEvalType = 
    | V1
    | V2
    | V3

module SorterEvalType =
    let toString (sorterEvalType: sorterEvalType) : string =
        match sorterEvalType with
        | V1 -> "V1"
        | V2 -> "V2"
        | V3 -> "V3"

    let fromString (str: string) : sorterEvalType =
        match str with
        | "V1" -> V1
        | "V2" -> V2
        | "V3" -> V3
        | s -> failwithf "Invalid sorterEvalType string: %s" s

type sorterEval = 
    | V1 of sorterEvalV1
    | V2 of sorterEvalV2
    | V3 of sorterEvalV3

module SorterEval =

    let getSorterId (eval: sorterEval) : Guid<sorterId> =
        match eval with
        | V1 v1 -> v1.SorterId
        | V2 v2 -> v2.SorterId
        | V3 v3 -> v3.SorterId

    let getSortingWidth (eval: sorterEval) : int<sortingWidth> =
        match eval with
        | V1 v1 -> v1.SortingWidth
        | V2 v2 -> v2.SortingWidth
        | V3 v3 -> v3.SortingWidth

    let getStageLength (eval: sorterEval) : int<stageLength> =
        match eval with
        | V1 v1 -> v1.StageLength
        | V2 v2 -> v2.StageLength
        | V3 v3 -> v3.StageLength

    let getCeLength (eval: sorterEval) : int<ceLength> =
        match eval with
        | V1 v1 -> v1.CeLength
        | V2 v2 -> v2.CeLength
        | V3 v3 -> v3.CeLength

    let getUnsortedCount (eval: sorterEval) : int<sortableCount> =
        match eval with
        | V1 v1 -> v1.UnsortedCount
        | V2 v2 -> v2.UnsortedCount
        | V3 v3 -> v3.UnsortedCount

    let getSequenceHash (eval: sorterEval) : int<sequenceHash> =
        match eval with
        | V1 v1 -> v1.SequenceHash
        | V2 v2 -> v2.SequenceHash
        | V3 v3 -> v3.SequenceHash

    let getLastCeIndex (eval: sorterEval) : int<ceIndex> =
        match eval with
        | V1 v1 -> v1.LastCeIndex
        | V2 v2 -> v2.LastCeIndex
        | V3 v3 -> v3.LastCeIndex

    let getCeDataSequence (eval: sorterEval) : ceData array =
        match eval with
        | V1 v1 -> failwith "V1 does not have CeDataSequence"
        | V2 v2 -> v2.CeDataSequence
        | V3 v3 -> v3.CeDataSequence

    let toDataTableRecord (eval: sorterEval) : dataTableRecord =
        match eval with
        | V1 v1 -> v1.ToDataTableRecord()
        | V2 v2 -> v2.ToDataTableRecord()
        | V3 v3 -> v3.ToDataTableRecord()


    /// Internal helper to extract ceData array for V2 and V3 records
    let private extractCeDataSequence (ceb: ceBlock) (useCounts: ceUseCounts) : ceData array =
        let results = ResizeArray<ceData>()
        for i in 0 .. (%ceb.CeLength - 1) do
            let idx = i |> UMX.tag<ceIndex>
            let count = useCounts.[idx]
            if count > 0 then
                results.Add(ceData.create idx count (ceb.getCe i))
        results.ToArray()

    let createV1 
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =
        let stageSequence = 
            StageBuilderSequence.toStageSequence 
                                  ceBlockEval.CeBlock.SortingWidth 
                                  ceBlockEval.UsedCes
        sorterEvalV1.create 
            sorterId 
            ceBlockEval.CeBlock.SortingWidth
            ceBlockEval.UnsortedCount 
            (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>) 
            ceBlockEval.CeUseCounts.LastUsedCeIndex 
            stageSequence.StageLength 
            ceBlockEval.CeUseCounts.UsedCeCount
        |> V1


    let createV2
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =
        let stageSequence = 
            StageBuilderSequence.toStageSequence 
                                  ceBlockEval.CeBlock.SortingWidth 
                                  ceBlockEval.UsedCes
        let ceDataSeq = extractCeDataSequence ceBlockEval.CeBlock ceBlockEval.CeUseCounts
        sorterEvalV2.create 
            sorterId 
            ceBlockEval.CeBlock.SortingWidth
            ceBlockEval.UnsortedCount 
            (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>) 
            stageSequence.StageLength  
            ceDataSeq
        |> V2


    let createV3
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =

        let stageSequence = 
            StageBuilderSequence.toStageSequence 
                                  ceBlockEval.CeBlock.SortingWidth 
                                  ceBlockEval.UsedCes

        match ceBlockEval.SortableTest with
        | None -> 
            // V3 strictly requires a sortableTest instance. If none exists (because it sorted perfectly),
            // we fall back to generating a V2 evaluation instead.
            createV2 sorterId ceBlockEval
        | Some test ->

            let ceDataSeq = extractCeDataSequence ceBlockEval.CeBlock ceBlockEval.CeUseCounts
            sorterEvalV3.create 
                sorterId 
                ceBlockEval.CeBlock.SortingWidth
                (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>) 
                stageSequence.StageLength 
                ceDataSeq 
                test
            |> V3


    let create 
            (sorterEvalType:sorterEvalType) 
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =

        match sorterEvalType with
        | sorterEvalType.V1 -> createV1 sorterId ceBlockEval
        | sorterEvalType.V2 -> createV2 sorterId ceBlockEval
        | sorterEvalType.V3 -> createV3 sorterId ceBlockEval
      
