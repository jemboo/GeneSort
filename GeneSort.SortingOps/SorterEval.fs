namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open System


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

    member this.ToDataTableRecordWithPrefix(prefix: string) : dataTableRecord =
            let isSorted = this.unsortedCount = 0<sortableCount>
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (prefix + "SorterId") (string %this.sorterId)
            |> dataTableRecord.addData (prefix + "SortingWidth") (string %this.sortingWidth)
            |> dataTableRecord.addData (prefix + "UnsortedCount") (string %this.unsortedCount)
            |> dataTableRecord.addData (prefix + "StageLength") (string %this.stageLength)
            |> dataTableRecord.addData (prefix + "CeLength") (string %this.ceLength)
            |> dataTableRecord.addData (prefix + "IsSorted") (string isSorted)
            |> dataTableRecord.addData (prefix + "SequenceHash") (string %this.sequenceHash)
            |> dataTableRecord.addData (prefix + "LastCeIndex") (string %this.lastCeIndex)



type ceUse = 
    private {
        ceIndex :int<ceIndex>
        useCount :int
        ce: ce
    }
    static member create 
                (ceIndex: int<ceIndex>) 
                (useCount: int)
                (ce: ce): ceUse =
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
        ceUseArray: ceUse array
    }

    static member create 
                    (sorterId: Guid<sorterId>) 
                    (sortingWidth: int<sortingWidth>) 
                    (unsortedCount: int<sortableCount>)
                    (sequenceKey: int<sequenceHash>)
                    (stageLength: int<stageLength>)
                    (ceUseArray: ceUse array): sorterEvalV2 =
        { 
                sorterId = sorterId; 
                sortingWidth = sortingWidth;
                unsortedCount = unsortedCount;
                sequenceHash = sequenceKey; 
                stageLength = stageLength;
                ceUseArray = ceUseArray;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceUseArray.Length |> UMX.tag<ceLength>
    member this.CeUseArray with get() : ceUse array = this.ceUseArray
    member this.UnsortedCount with get() : int<sortableCount>  = this.unsortedCount
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.LastCeIndex with get() : int<ceIndex>  = 
        if this.ceUseArray.Length = 0 then 0<ceIndex>
        else this.ceUseArray.[this.ceUseArray.Length - 1].CeIndex
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

    
    member this.ToDataTableRecordWithPrefix(prefix: string) : dataTableRecord =
        let isSorted = this.unsortedCount = 0<sortableCount>
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData (prefix + "SorterId") (string %this.sorterId)
        |> dataTableRecord.addData (prefix + "SortingWidth") (string %this.sortingWidth)
        |> dataTableRecord.addData (prefix + "UnsortedCount") (string %this.unsortedCount)
        |> dataTableRecord.addData (prefix + "StageLength") (string %this.stageLength)
        |> dataTableRecord.addData (prefix + "CeLength") (string %this.CeLength)
        |> dataTableRecord.addData (prefix + "IsSorted") (string isSorted)
        |> dataTableRecord.addData (prefix + "SequenceHash") (string %this.sequenceHash)
        |> dataTableRecord.addData (prefix + "LastCeIndex") (string %this.LastCeIndex)
        


type sorterEvalV3 =
    private { 
        sorterId: Guid<sorterId>
        sortingWidth: int<sortingWidth>
        sequenceHash: int<sequenceHash>
        stageLength: int<stageLength>
        ceUseArray: ceUse array
        sortableTest: sortableTest 
    }

    static member create 
                    (sorterId: Guid<sorterId>)
                    (sortingWidth: int<sortingWidth>) 
                    (sequenceKey: int<sequenceHash>)
                    (stageLength: int<stageLength>)
                    (ceUseArray: ceUse array) 
                    (sortableTest:sortableTest) :sorterEvalV3 =
        { 
                sorterId = sorterId;
                sortingWidth = sortingWidth;
                sequenceHash = sequenceKey; 
                stageLength = stageLength;
                ceUseArray = ceUseArray;
                sortableTest =sortableTest;
        }

    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceUseArray.Length |> UMX.tag<ceLength>
    member this.CeUseArray with get() : ceUse array = this.ceUseArray
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.SortableTest with get() : sortableTest = this.sortableTest
    member this.UnsortedCount with get() : int<sortableCount>  = 
            this.sortableTest |> SortableTests.getUnsortedCount
    member this.LastCeIndex with get() : int<ceIndex>  = 
        if this.ceUseArray.Length = 0 then 0<ceIndex>
        else this.ceUseArray.[this.ceUseArray.Length - 1].CeIndex
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

    member this.ToDataTableRecordWithPrefix(prefix: string) : dataTableRecord =
            let isSorted = this.UnsortedCount = 0<sortableCount>
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (prefix + "SorterId") (string %this.sorterId)
            |> dataTableRecord.addData (prefix + "SortingWidth") (string %this.sortingWidth)
            |> dataTableRecord.addData (prefix + "UnsortedCount") (string %this.UnsortedCount)
            |> dataTableRecord.addData (prefix + "StageLength") (string %this.stageLength) 
            |> dataTableRecord.addData (prefix + "CeLength") (string %this.CeLength)
            |> dataTableRecord.addData (prefix + "IsSorted") (string isSorted)
            |> dataTableRecord.addData (prefix + "SequenceHash") (string %this.sequenceHash)
            |> dataTableRecord.addData (prefix + "LastCeIndex") (string %this.LastCeIndex)



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

    let getIsSorted (eval: sorterEval) : bool =
        match eval with
        | V1 v1 -> v1.UnsortedCount = 0<sortableCount>
        | V2 v2 -> v2.UnsortedCount = 0<sortableCount>
        | V3 v3 -> v3.UnsortedCount = 0<sortableCount>

    let getIsUnSorted (eval: sorterEval) : bool =
        match eval with
        | V1 v1 -> v1.UnsortedCount > 0<sortableCount>
        | V2 v2 -> v2.UnsortedCount > 0<sortableCount>
        | V3 v3 -> v3.UnsortedCount > 0<sortableCount>

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

    let getCeUseArray (eval: sorterEval) : ceUse array =
        match eval with
        | V1 v1 -> failwith "V1 does not have CeDataSequence"
        | V2 v2 -> v2.CeUseArray
        | V3 v3 -> v3.CeUseArray

    let toDataTableRecord (eval: sorterEval) : dataTableRecord =
        match eval with
        | V1 v1 -> v1.ToDataTableRecord()
        | V2 v2 -> v2.ToDataTableRecord()
        | V3 v3 -> v3.ToDataTableRecord()

    let toDataTableRecordWithPrefix (prefix: string) (eval: sorterEval) : dataTableRecord =
        match eval with
        | V1 v1 -> v1.ToDataTableRecordWithPrefix(prefix)
        | V2 v2 -> v2.ToDataTableRecordWithPrefix(prefix)
        | V3 v3 -> v3.ToDataTableRecordWithPrefix(prefix)


    /// Internal helper to extract ceData array for V2 and V3 records
    let private extractCeUseArray (ceb: ceBlock) (useCounts: ceUseCounts) : ceUse array =
        let results = ResizeArray<ceUse>()
        for i in 0 .. (%ceb.CeLength - 1) do
            let idx = i |> UMX.tag<ceIndex>
            let count = useCounts.[idx]
            if count > 0 then
                results.Add(ceUse.create idx count (ceb.getCe i))
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
        let ceDataSeq = extractCeUseArray ceBlockEval.CeBlock ceBlockEval.CeUseCounts
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

            let ceDataSeq = extractCeUseArray ceBlockEval.CeBlock ceBlockEval.CeUseCounts
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

      
