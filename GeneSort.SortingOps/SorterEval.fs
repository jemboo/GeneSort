namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable

type sorterEvalV1 =
    private { 
        sorterId: Guid<sorterId>
        sortableTestsSubsetId: string<sortableTestsSubsetId>
        sortingWidth: int<sortingWidth>
        unsortedCount: int<sortableCount> option
        sequenceHash: int<sequenceHash>
        lastCeIndex: int<ceIndex>
        stageLength: int<stageLength>
        ceLength: int<ceLength>
        reflectionSymmetric: bool<isReflectionSymmetric>
        stageCrossingsCount: int<stageCrossings>
        reflectiveCount: int<reflectiveCount>
    }

    static member create 
            (sorterId: Guid<sorterId>)
            (sortableTestsSubsetId: string<sortableTestsSubsetId>)
            (sortingWidth: int<sortingWidth>) 
            (unsortedCount: int<sortableCount> option)
            (sequenceKey: int<sequenceHash>) 
            (lastCeIndex: int<ceIndex>) 
            (stageLength: int<stageLength>)
            (ceLength: int<ceLength>) 
            (reflectionSymmetric: bool<isReflectionSymmetric>) 
            (stageCrossingsCount: int<stageCrossings>)
            (reflectiveCount: int<reflectiveCount>): sorterEvalV1 =
        { 
                sorterId = sorterId; 
                sortableTestsSubsetId = sortableTestsSubsetId
                sortingWidth = sortingWidth;
                unsortedCount = unsortedCount;
                sequenceHash = sequenceKey; 
                lastCeIndex = lastCeIndex;
                stageLength = stageLength;
                ceLength = ceLength;
                reflectionSymmetric = reflectionSymmetric
                stageCrossingsCount = stageCrossingsCount
                reflectiveCount = reflectiveCount
        }

    member this.ReflectionSymmetric with get() : bool<isReflectionSymmetric> = this.reflectionSymmetric
    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortableTestsSubsetId with get() : string<sortableTestsSubsetId> = this.sortableTestsSubsetId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageCrossingsCount with get() : int<stageCrossings> = this.stageCrossingsCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceLength
    member this.UnsortedCount with get() : int<sortableCount> option = this.unsortedCount
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.LastCeIndex with get() : int<ceIndex>  = this.lastCeIndex
    member this.ReflectiveCount with get() : int<reflectiveCount> = this.reflectiveCount

    member this.ToDataTableRecord() : dataTableRecord =
            let isSorted = this.unsortedCount |> Option.map ((=) 0<sortableCount>)
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> dataTableRecord.addData "SortableTestsSubsetId" (string %this.sortableTestsSubsetId)
            |> dataTableRecord.addData "SortingWidth" (string %this.sortingWidth)
            |> dataTableRecord.addData "UnsortedCount" (this.unsortedCount |> Option.map (fun count -> string %count) |> Option.defaultValue "")
            |> dataTableRecord.addData "StageLength" (string %this.stageLength)
            |> dataTableRecord.addData "CeLength" (string %this.ceLength)
            |> dataTableRecord.addData "IsSorted" (isSorted |> Option.map string |> Option.defaultValue "")
            |> dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> dataTableRecord.addData "LastCeIndex" (string %this.lastCeIndex)
            |> dataTableRecord.addData "IsReflectionSymmetric" (string %this.reflectionSymmetric)
            |> dataTableRecord.addData "StageCrossingsCount" (string %this.stageCrossingsCount)
            |> dataTableRecord.addData "ReflectiveCount" (string %this.reflectiveCount)

    member this.ToDataTableRecordWithPrefix(prefix: string) : dataTableRecord =
            let isSorted = this.unsortedCount |> Option.map ((=) 0<sortableCount>)
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (prefix + "SorterId") (string %this.sorterId)
            |> dataTableRecord.addData (prefix + "SortableTestsSubsetId") (string %this.sortableTestsSubsetId)
            |> dataTableRecord.addData (prefix + "SortingWidth") (string %this.sortingWidth)
            |> dataTableRecord.addData (prefix + "UnsortedCount") (this.unsortedCount |> Option.map (fun count -> string %count) |> Option.defaultValue "")
            |> dataTableRecord.addData (prefix + "StageLength") (string %this.stageLength)
            |> dataTableRecord.addData (prefix + "CeLength") (string %this.ceLength)
            |> dataTableRecord.addData (prefix + "IsSorted") (isSorted |> Option.map string |> Option.defaultValue "")
            |> dataTableRecord.addData (prefix + "SequenceHash") (string %this.sequenceHash)
            |> dataTableRecord.addData (prefix + "LastCeIndex") (string %this.lastCeIndex)
            |> dataTableRecord.addData (prefix + "IsReflectionSymmetric") (string %this.reflectionSymmetric)
            |> dataTableRecord.addData (prefix + "StageCrossingsCount") (string %this.stageCrossingsCount)
            |> dataTableRecord.addData (prefix + "ReflectiveCount") (string %this.reflectiveCount)


type sorterEvalV2 =
    private { 
        sorterId: Guid<sorterId>
        sortableTestsSubsetId: string<sortableTestsSubsetId>
        sortingWidth: int<sortingWidth>
        unsortedCount: int<sortableCount> option
        sequenceHash: int<sequenceHash>
        stageLength: int<stageLength>
        ceUseArray: ceUse array
        isReflectionSymmetric: bool<isReflectionSymmetric>
        stageCrossingsCount: int<stageCrossings>
        reflectiveCount: int<reflectiveCount>
    }

    static member create 
                    (sorterId: Guid<sorterId>) 
                    (sortableTestsSubsetId: string<sortableTestsSubsetId>)
                    (sortingWidth: int<sortingWidth>) 
                    (unsortedCount: int<sortableCount> option)
                    (sequenceKey: int<sequenceHash>)
                    (stageLength: int<stageLength>)
                    (ceUseArray: ceUse array)
                    (reflectionSymmetric: bool<isReflectionSymmetric>)
                    (stageCrossingsCount: int<stageCrossings>)
                    (reflectiveCount: int<reflectiveCount>): sorterEvalV2 =
        { 
                sorterId = sorterId; 
                sortableTestsSubsetId = sortableTestsSubsetId
                sortingWidth = sortingWidth;
                unsortedCount = unsortedCount;
                sequenceHash = sequenceKey; 
                stageLength = stageLength;
                ceUseArray = ceUseArray;
                isReflectionSymmetric = reflectionSymmetric
                stageCrossingsCount = stageCrossingsCount
                reflectiveCount = reflectiveCount
        }
    
    member this.IsReflectionSymmetric with get() : bool<isReflectionSymmetric> = this.isReflectionSymmetric
    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortableTestsSubsetId with get() : string<sortableTestsSubsetId> = this.sortableTestsSubsetId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageCrossingsCount with get() : int<stageCrossings> = this.stageCrossingsCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceUseArray.Length |> UMX.tag<ceLength>
    member this.CeUseArray with get() : ceUse array = this.ceUseArray
    member this.UnsortedCount with get() : int<sortableCount> option = this.unsortedCount
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.ReflectiveCount with get() : int<reflectiveCount> = this.reflectiveCount
    member this.LastCeIndex with get() : int<ceIndex>  = 
        if this.ceUseArray.Length = 0 then 0<ceIndex>
        else this.ceUseArray.[this.ceUseArray.Length - 1].CeIndex

    /// Downgrades sorterEvalV2 to sorterEvalV1 by stripping the ceUseArray details.
    member this.ToV1() : sorterEvalV1 =
        sorterEvalV1.create
            this.sorterId
            this.sortableTestsSubsetId
            this.sortingWidth
            this.unsortedCount
            this.sequenceHash
            this.LastCeIndex
            this.stageLength
            this.CeLength
            this.isReflectionSymmetric
            this.stageCrossingsCount
            this.reflectiveCount

    member this.ToDataTableRecord() : dataTableRecord =
            let isSorted = this.unsortedCount |> Option.map ((=) 0<sortableCount>)
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> dataTableRecord.addData "SortableTestsSubsetId" (string %this.sortableTestsSubsetId)
            |> dataTableRecord.addData "SortingWidth" (string %this.sortingWidth)
            |> dataTableRecord.addData "UnsortedCount" (this.unsortedCount |> Option.map (fun count -> string %count) |> Option.defaultValue "")
            |> dataTableRecord.addData "StageLength" (string %this.stageLength)
            |> dataTableRecord.addData "CeLength" (string %this.CeLength)
            |> dataTableRecord.addData "IsSorted" (isSorted |> Option.map string |> Option.defaultValue "")
            |> dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> dataTableRecord.addData "LastCeIndex" (string %this.LastCeIndex)
            |> dataTableRecord.addData "CeUseArray" (CeUse.arrayToString this.ceUseArray)
            |> dataTableRecord.addData "IsReflectionSymmetric" (string %this.isReflectionSymmetric)
            |> dataTableRecord.addData "StageCrossingsCount" (string %this.stageCrossingsCount)
            |> dataTableRecord.addData "ReflectiveCount" (string %this.reflectiveCount)

    member this.ToDataTableRecordWithPrefix(prefix: string) : dataTableRecord =
        let isSorted = this.unsortedCount |> Option.map ((=) 0<sortableCount>)
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData (prefix + "SorterId") (string %this.sorterId)
        |> dataTableRecord.addData (prefix + "SortableTestsSubsetId") (string %this.sortableTestsSubsetId)
        |> dataTableRecord.addData (prefix + "SortingWidth") (string %this.sortingWidth)
        |> dataTableRecord.addData (prefix + "UnsortedCount") (this.unsortedCount |> Option.map (fun count -> string %count) |> Option.defaultValue "")
        |> dataTableRecord.addData (prefix + "StageLength") (string %this.stageLength)
        |> dataTableRecord.addData (prefix + "CeLength") (string %this.CeLength)
        |> dataTableRecord.addData (prefix + "IsSorted") (isSorted |> Option.map string |> Option.defaultValue "")
        |> dataTableRecord.addData (prefix + "SequenceHash") (string %this.sequenceHash)
        |> dataTableRecord.addData (prefix + "LastCeIndex") (string %this.LastCeIndex)
        |> dataTableRecord.addData (prefix + "CeUseArray") (CeUse.arrayToString this.ceUseArray)
        |> dataTableRecord.addData (prefix + "IsReflectionSymmetric") (string %this.isReflectionSymmetric)
        |> dataTableRecord.addData (prefix + "StageCrossingsCount") (string %this.stageCrossingsCount)
        |> dataTableRecord.addData (prefix + "ReflectiveCount") (string %this.reflectiveCount)


type sorterEvalV3 =
    private { 
        sorterId: Guid<sorterId>
        sortableTestsSubsetId: string<sortableTestsSubsetId>
        sortingWidth: int<sortingWidth>
        unsortedCount: int<sortableCount> option
        sequenceHash: int<sequenceHash>
        stageLength: int<stageLength>
        ceUseArray: ceUse array
        sortableTests: sortableTests 
        reflectionSymmetric: bool<isReflectionSymmetric>
        stageCrossingsCount: int<stageCrossings>
        reflectiveCount: int<reflectiveCount>
    }

    static member create 
                    (sorterId: Guid<sorterId>)
                    (sortableTestsSubsetId: string<sortableTestsSubsetId>)
                    (sortingWidth: int<sortingWidth>) 
                    (unsortedCount: int<sortableCount> option)
                    (sequenceKey: int<sequenceHash>)
                    (stageLength: int<stageLength>)
                    (ceUseArray: ceUse array) 
                    (sortableTests:sortableTests) 
                    (reflectionSymmetric: bool<isReflectionSymmetric>) 
                    (stageCrossingsCount: int<stageCrossings>)
                    (reflectiveCount: int<reflectiveCount>): sorterEvalV3 =
        { 
                sorterId = sorterId;
                sortableTestsSubsetId = sortableTestsSubsetId
                sortingWidth = sortingWidth;
                unsortedCount = unsortedCount
                sequenceHash = sequenceKey; 
                stageLength = stageLength;
                ceUseArray = ceUseArray;
                sortableTests =sortableTests;
                reflectionSymmetric = reflectionSymmetric
                stageCrossingsCount = stageCrossingsCount
                reflectiveCount = reflectiveCount
        }
    
    member this.ReflectionSymmetric with get() : bool<isReflectionSymmetric> = this.reflectionSymmetric
    member this.SorterId with get() : Guid<sorterId>  = this.sorterId
    member this.SortableTestsSubsetId with get() : string<sortableTestsSubsetId> = this.sortableTestsSubsetId
    member this.SortingWidth with get() : int<sortingWidth> = this.sortingWidth
    member this.StageCrossingsCount with get() : int<stageCrossings> = this.stageCrossingsCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.CeLength with get() : int<ceLength> = this.ceUseArray.Length |> UMX.tag<ceLength>
    member this.CeUseArray with get() : ceUse array = this.ceUseArray
    member this.SequenceHash with get() : int<sequenceHash>  = this.sequenceHash
    member this.SortableTests with get() : sortableTests = this.sortableTests
    member this.ReflectiveCount with get() : int<reflectiveCount> = this.reflectiveCount
    member this.UnsortedCount with get() : int<sortableCount> option = this.unsortedCount
    member this.LastCeIndex with get() : int<ceIndex>  = 
        if this.ceUseArray.Length = 0 then 0<ceIndex>
        else this.ceUseArray.[this.ceUseArray.Length - 1].CeIndex

    /// Downgrades sorterEvalV3 to sorterEvalV2 by evaluating sortableTests into unsortedCount.
    member this.ToV2() : sorterEvalV2 =
        sorterEvalV2.create
            this.sorterId
            this.sortableTestsSubsetId
            this.sortingWidth
            this.UnsortedCount
            this.sequenceHash
            this.stageLength
            this.ceUseArray
            this.reflectionSymmetric
            this.stageCrossingsCount
            this.reflectiveCount

    /// Downgrades sorterEvalV3 directly to sorterEvalV1.
    member this.ToV1() : sorterEvalV1 =
        sorterEvalV1.create
            this.sorterId
            this.sortableTestsSubsetId
            this.sortingWidth
            this.UnsortedCount
            this.sequenceHash
            this.LastCeIndex
            this.stageLength
            this.CeLength
            this.reflectionSymmetric
            this.stageCrossingsCount
            this.reflectiveCount

    member this.ToDataTableRecord() : dataTableRecord =
            let isSorted = this.UnsortedCount |> Option.map ((=) 0<sortableCount>)
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SorterId" (string %this.sorterId)
            |> dataTableRecord.addData "SortableTestsSubsetId" (string %this.sortableTestsSubsetId)
            |> dataTableRecord.addData "SortingWidth" (string %this.sortingWidth)
            |> dataTableRecord.addData "UnsortedCount" (this.UnsortedCount |> Option.map (fun count -> string %count) |> Option.defaultValue "")
            |> dataTableRecord.addData "StageLength" (string %this.stageLength)
            |> dataTableRecord.addData "CeLength" (string %this.CeLength)
            |> dataTableRecord.addData "IsSorted" (isSorted |> Option.map string |> Option.defaultValue "")
            |> dataTableRecord.addData "SequenceHash" (string %this.sequenceHash)
            |> dataTableRecord.addData "LastCeIndex" (string %this.LastCeIndex)
            |> dataTableRecord.addData "CeUseArray" (CeUse.arrayToString this.ceUseArray)
            |> dataTableRecord.addData "IsReflectionSymmetric" (string %this.reflectionSymmetric)
            |> dataTableRecord.addData "StageCrossingsCount" (string %this.stageCrossingsCount)
            |> dataTableRecord.addData "ReflectiveCount" (string %this.reflectiveCount)

    member this.ToDataTableRecordWithPrefix(prefix: string) : dataTableRecord =
            let isSorted = this.UnsortedCount |> Option.map ((=) 0<sortableCount>)
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (prefix + "SorterId") (string %this.sorterId)
            |> dataTableRecord.addData (prefix + "SortableTestsSubsetId") (string %this.sortableTestsSubsetId)
            |> dataTableRecord.addData (prefix + "SortingWidth") (string %this.sortingWidth)
            |> dataTableRecord.addData (prefix + "UnsortedCount") (this.UnsortedCount |> Option.map (fun count -> string %count) |> Option.defaultValue "")
            |> dataTableRecord.addData (prefix + "StageLength") (string %this.stageLength) 
            |> dataTableRecord.addData (prefix + "CeLength") (string %this.CeLength)
            |> dataTableRecord.addData (prefix + "IsSorted") (isSorted |> Option.map string |> Option.defaultValue "")
            |> dataTableRecord.addData (prefix + "SequenceHash") (string %this.sequenceHash)
            |> dataTableRecord.addData (prefix + "LastCeIndex") (string %this.LastCeIndex)
            |> dataTableRecord.addData (prefix + "CeUseArray") (CeUse.arrayToString this.ceUseArray)
            |> dataTableRecord.addData (prefix + "IsReflectionSymmetric") (string %this.reflectionSymmetric)
            |> dataTableRecord.addData (prefix + "StageCrossingsCount") (string %this.stageCrossingsCount)
            |> dataTableRecord.addData (prefix + "ReflectiveCount") (string %this.reflectiveCount)


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

    let getIsReflectionSymmetric (eval: sorterEval) : bool<isReflectionSymmetric> =
        match eval with
        | V1 v1 -> v1.ReflectionSymmetric
        | V2 v2 -> v2.IsReflectionSymmetric
        | V3 v3 -> v3.ReflectionSymmetric

    let getStageCrossingsCount (eval: sorterEval) : int<stageCrossings> =
        match eval with
        | V1 v1 -> v1.StageCrossingsCount
        | V2 v2 -> v2.StageCrossingsCount
        | V3 v3 -> v3.StageCrossingsCount

    let getReflectiveCount (eval: sorterEval) : int<reflectiveCount> =
        match eval with
        | V1 v1 -> v1.ReflectiveCount
        | V2 v2 -> v2.ReflectiveCount
        | V3 v3 -> v3.ReflectiveCount

    let getSorterId (eval: sorterEval) : Guid<sorterId> =
        match eval with
        | V1 v1 -> v1.SorterId
        | V2 v2 -> v2.SorterId
        | V3 v3 -> v3.SorterId

    let getSortableTestsSubsetId (eval: sorterEval) : string<sortableTestsSubsetId> =
        match eval with
        | V1 v1 -> v1.SortableTestsSubsetId
        | V2 v2 -> v2.SortableTestsSubsetId
        | V3 v3 -> v3.SortableTestsSubsetId

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

    let getUnsortedCount (eval: sorterEval) : int<sortableCount> option =
        match eval with
        | V1 v1 -> v1.UnsortedCount
        | V2 v2 -> v2.UnsortedCount
        | V3 v3 -> v3.UnsortedCount

    let getIsSorted (eval: sorterEval) : bool option =
        match eval with
        | V1 v1 -> v1.UnsortedCount |> Option.map ((=) 0<sortableCount>)
        | V2 v2 -> v2.UnsortedCount |> Option.map ((=) 0<sortableCount>)
        | V3 v3 -> v3.UnsortedCount |> Option.map ((=) 0<sortableCount>)

    let getIsUnSorted (eval: sorterEval) : bool option =
        match eval with
        | V1 v1 -> v1.UnsortedCount |> Option.map ((<) 0<sortableCount>)
        | V2 v2 -> v2.UnsortedCount |> Option.map ((<) 0<sortableCount>)
        | V3 v3 -> v3.UnsortedCount |> Option.map ((<) 0<sortableCount>)

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

    let getIsShortEnough
            (lengthCutoff: int<ceLength>) 
            (eval: sorterEval): bool =
        (%(getLastCeIndex eval) <= %lengthCutoff)
        &&
        (getIsSorted eval |> Option.defaultValue false)

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

    let toDataTableRecordWithPrefix (prefix: string) 
                                    (eval: sorterEval) : dataTableRecord =
        match eval with
        | V1 v1 -> v1.ToDataTableRecordWithPrefix(prefix)
        | V2 v2 -> v2.ToDataTableRecordWithPrefix(prefix)
        | V3 v3 -> v3.ToDataTableRecordWithPrefix(prefix)

    /// Downgrades any sorterEval DU instance down to a V2 sorterEval DU.
    let toV2 (eval: sorterEval) : sorterEval =
        match eval with
        | V1 _ -> eval // Already lower than V2
        | V2 _ -> eval
        | V3 v3 -> V2 (v3.ToV2())

    /// Downgrades any sorterEval DU instance down to a V1 sorterEval DU.
    let toV1 (eval: sorterEval) : sorterEval =
        match eval with
        | V1 _ -> eval
        | V2 v2 -> V1 (v2.ToV1())
        | V3 v3 -> V1 (v3.ToV1())

    /// Target-driven downgrade function given a target sorterEvalType
    let downgradeTo (targetType: sorterEvalType) (eval: sorterEval) : sorterEval =
        match targetType with
        | sorterEvalType.V1 -> toV1 eval
        | sorterEvalType.V2 -> toV2 eval
        | sorterEvalType.V3 -> eval

    let createV1 
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =
        let stageSequence, moves = 
            StageBuilderSequence.toStageSequenceWithMoveInfo 
                                  ceBlockEval.CeBlock.SortingWidth 
                                  ceBlockEval.UsedCes
        let stageCrossingsCount = moves |> Array.sumBy Array.sum |> UMX.tag<stageCrossings>

        let isReflectionSymmetric = stageSequence.Stages 
                                    |> Array.forall(fun st -> st |> Stage.isReflectionSymmetric)
                                    |> UMX.tag<isReflectionSymmetric>

        let reflectiveCount =  stageSequence.GetReflectiveCount()

        sorterEvalV1.create 
            sorterId 
            SortableTestsSubsetId.Default
            ceBlockEval.CeBlock.SortingWidth
            (Some ceBlockEval.UnsortedCount)
            (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>) 
            ceBlockEval.LastUsedIndex
            stageSequence.StageLength 
            ceBlockEval.CeLength
            isReflectionSymmetric
            stageCrossingsCount
            reflectiveCount
        |> V1

    let createV2
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =
        let stageSequence, moves = 
            StageBuilderSequence.toStageSequenceWithMoveInfo 
                                  ceBlockEval.CeBlock.SortingWidth 
                                  ceBlockEval.UsedCes
        let stageCrossingsCount = moves |> Array.sumBy Array.sum |> UMX.tag<stageCrossings>
        let ceUseArray = ceBlockEval.extractCeUseArray

        let isReflectionSymmetric = stageSequence.Stages 
                                    |> Array.forall(fun st -> st |> Stage.isReflectionSymmetric)
                                    |> UMX.tag<isReflectionSymmetric>

        let reflectiveCount =  stageSequence.GetReflectiveCount()

        sorterEvalV2.create 
            sorterId 
            SortableTestsSubsetId.Default
            ceBlockEval.CeBlock.SortingWidth
            (Some ceBlockEval.UnsortedCount)
            (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>) 
            stageSequence.StageLength  
            ceUseArray
            isReflectionSymmetric
            stageCrossingsCount
            reflectiveCount
        |> V2

    let createV3
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =
        let stageSequence, moves = 
            StageBuilderSequence.toStageSequenceWithMoveInfo 
                                  ceBlockEval.CeBlock.SortingWidth 
                                  ceBlockEval.UsedCes
        let stageCrossingsCount = moves |> Array.sumBy Array.sum |> UMX.tag<stageCrossings>

        let isReflectionSymmetric = stageSequence.Stages 
                                    |> Array.forall(fun st -> st |> Stage.isReflectionSymmetric)
                                    |> UMX.tag<isReflectionSymmetric>

        let reflectiveCount =  stageSequence.GetReflectiveCount()

        match ceBlockEval.SortableTests with
        | None -> 
            createV2 sorterId ceBlockEval
        | Some test ->

            let ceUseArray = ceBlockEval.extractCeUseArray
            sorterEvalV3.create 
                sorterId 
                SortableTestsSubsetId.Default
                ceBlockEval.CeBlock.SortingWidth
                (Some ceBlockEval.UnsortedCount)
                (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>) 
                stageSequence.StageLength 
                ceUseArray 
                test
                isReflectionSymmetric
                stageCrossingsCount
                reflectiveCount
            |> V3

    let create 
            (sorterEvalType:sorterEvalType) 
            (sorterId: Guid<sorterId>) 
            (ceBlockEval: ceBlockEval) : sorterEval =

        match sorterEvalType with
        | sorterEvalType.V1 -> createV1 sorterId ceBlockEval
        | sorterEvalType.V2 -> createV2 sorterId ceBlockEval
        | sorterEvalType.V3 -> createV3 sorterId ceBlockEval

    let private mergeCeUseArrays (first: ceUse[]) (second: ceUse[]) : ceUse[] =
        let toMap parameterName (values: ceUse[]) =
            values
            |> Array.fold (fun result value ->
                if Map.containsKey value.CeIndex result then
                    invalidArg parameterName $"CE index {%value.CeIndex} occurs more than once."
                Map.add value.CeIndex value result) Map.empty

        let firstByIndex = toMap "first" first
        let secondByIndex = toMap "second" second
        secondByIndex
        |> Map.fold (fun merged index right ->
            match Map.tryFind index merged with
            | Some left -> Map.add index (CeUse.merge left right) merged
            | None -> Map.add index right merged) firstByIndex
        |> Map.toArray
        |> Array.map snd

    let private calculateMetrics
            (sortingWidth: int<sortingWidth>)
            (ceUseArray: ceUse[]) =
        let ces = ceUseArray |> Array.map (fun ceUse -> ceUse.Ce)
        let stageSequence, moves =
            StageBuilderSequence.toStageSequenceWithMoveInfo sortingWidth ces
        let stageCrossingsCount = moves |> Array.sumBy Array.sum |> UMX.tag<stageCrossings>
        let reflectionSymmetric =
            stageSequence.Stages
            |> Array.forall Stage.isReflectionSymmetric
            |> UMX.tag<isReflectionSymmetric>

        (stageSequence.GetHashCode() |> UMX.tag<sequenceHash>),
        stageSequence.StageLength,
        reflectionSymmetric,
        stageCrossingsCount,
        stageSequence.GetReflectiveCount()

    let private validateMergeInputs (first: sorterEval) (second: sorterEval) =
        match first, second with
        | V1 _, _
        | _, V1 _ ->
            invalidArg "first" "Cannot merge a V1 sorter evaluation because it has no ceUseArray."
        | _ ->
            if getSorterId first <> getSorterId second then
                invalidArg "second" "Sorter evaluations must belong to the same sorter."
            if getSortingWidth first <> getSortingWidth second then
                invalidArg "second" "Sorter evaluations must have the same sorting width."

    /// Combines two evaluations of the same sorter over separate sortable-test partitions.
    let merge (first: sorterEval) (second: sorterEval) : sorterEval =
        validateMergeInputs first second

        let sorterId = getSorterId first
        let sortingWidth = getSortingWidth first
        let ceUseArray = mergeCeUseArrays (getCeUseArray first) (getCeUseArray second)
        let sequenceHash, stageLength, reflectionSymmetric, stageCrossingsCount, reflectiveCount =
            calculateMetrics sortingWidth ceUseArray

        match first, second with
        | V2 firstV2, V2 secondV2 ->
            sorterEvalV2.create
                sorterId
                SortableTestsSubsetId.Default
                sortingWidth
                None
                sequenceHash
                stageLength
                ceUseArray
                reflectionSymmetric
                stageCrossingsCount
                reflectiveCount
            |> V2
        | V3 firstV3, V3 secondV3 ->
            sorterEvalV3.create
                sorterId
                SortableTestsSubsetId.Default
                sortingWidth
                None
                sequenceHash
                stageLength
                ceUseArray
                (SortableTests.mergeSortableTests firstV3.SortableTests secondV3.SortableTests)
                reflectionSymmetric
                stageCrossingsCount
                reflectiveCount
            |> V3
        | V2 firstV2, V3 secondV3 ->
            sorterEvalV2.create
                sorterId
                SortableTestsSubsetId.Default
                sortingWidth
                None
                sequenceHash
                stageLength
                ceUseArray
                reflectionSymmetric
                stageCrossingsCount
                reflectiveCount
            |> V2
        | V3 firstV3, V2 secondV2 ->
            sorterEvalV2.create
                sorterId
                SortableTestsSubsetId.Default
                sortingWidth
                None
                sequenceHash
                stageLength
                ceUseArray
                reflectionSymmetric
                stageCrossingsCount
                reflectiveCount
            |> V2
        | _ ->
            invalidArg "first" "Cannot merge a V1 sorter evaluation because it has no ceUseArray."
