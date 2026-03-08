namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic

[<Struct; StructuralEquality; NoComparison>]
type sorterEvalKey =
    private {
        ceCount: int<ceLength>
        stageLength: int<stageLength>
        isSorted: bool
    }

    static member create 
                    (ceCount:int<ceLength>) 
                    (stageLength: int<stageLength>) 
                    (isSorted: bool) = 
          {
            ceCount = ceCount;
            stageLength = stageLength;
            isSorted = isSorted
          }

    member this.CeCount with get() : int<ceLength> =
        this.ceCount

    member this.StageLength with get() : int<stageLength> =
        this.stageLength

    member this.IsSorted with get() : bool = 
        this.isSorted
        


type sorterEvalSubBin =
    private {
        sorterIds: ResizeArray<Guid<sorterId>>
    }
    static member create() =
        { sorterIds = ResizeArray<Guid<sorterId>>() }
    member this.EvalCount with get() = this.sorterIds.Count
    member this.Add(sorterId: Guid<sorterId>) = this.sorterIds.Add(sorterId)
    member this.SorterIds with get() = this.sorterIds :> IReadOnlyList<Guid<sorterId>>


type sorterEvalBin =
    private {
        subBins: Dictionary<int, sorterEvalSubBin>
    }
    static member create() =
        { subBins = Dictionary<int, sorterEvalSubBin>() }
    member this.EvalCount with get() =
        this.subBins.Values |> Seq.sumBy (fun b -> b.EvalCount)
    member this.SubBins with get() = this.subBins :> IReadOnlyDictionary<int, sorterEvalSubBin>
    member this.SubBinCount with get() = this.subBins.Count
    member this.TryGetSubBin (hashKey: int) =
        match this.subBins.TryGetValue(hashKey) with
        | true, bin -> Some bin
        | false, _ -> None
    member internal this.Add(sorterId: Guid<sorterId>, ceUseCounts: ceUseCounts) =
        let key = ceUseCounts.GetHashCode()
        let subBin =
            match this.subBins.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newSubBin = sorterEvalSubBin.create()
                this.subBins.[key] <- newSubBin
                newSubBin
        subBin.Add(sorterId)
    member this.MergeSubBin (hashKey: int) (subBin: sorterEvalSubBin) =
        let existing =
            match this.subBins.TryGetValue(hashKey) with
            | true, existing -> existing
            | false, _ ->
                let newSubBin = sorterEvalSubBin.create()
                this.subBins.[hashKey] <- newSubBin
                newSubBin
        for sorterId in subBin.SorterIds do
            existing.Add(sorterId)


type sorterSetEvalBins = 
    private {
        sorterSetEvalId: Guid<sorterSetEvalId>
        evalBins: Dictionary<sorterEvalKey, sorterEvalBin>
    }
    static member create (sorterSetEvalId: Guid<sorterSetEvalId>) =
        {
            sorterSetEvalId = sorterSetEvalId
            evalBins = Dictionary<sorterEvalKey, sorterEvalBin>()
        }
    member this.SorterSetEvalId with get() = this.sorterSetEvalId
    member this.EvalBins with get() = this.evalBins :> IReadOnlyDictionary<sorterEvalKey, sorterEvalBin>
    member this.BinCount with get() = this.evalBins.Count
    member this.TotalEvalCount with get() = 
        this.evalBins.Values |> Seq.sumBy (fun b -> b.EvalCount)

    member this.TryGetBin (key: sorterEvalKey) =
        match this.evalBins.TryGetValue(key) with
        | true, bin -> Some bin
        | false, _ -> None

    member internal this.AddSorterEval (sorterEval: sorterEval) =
        let key = sorterEvalKey.create
                      sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
                      sorterEval.CeBlockEval.getStageSequence.StageLength
                      (sorterEval.CeBlockEval.UnsortedCount = 0<sortableCount>)
        let bin =
            match this.evalBins.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newBin = sorterEvalBin.create()
                this.evalBins.[key] <- newBin
                newBin
        bin.Add(sorterEval.SorterId, sorterEval.CeBlockEval.CeUseCounts)

    member this.MergeBin (key: sorterEvalKey) (bin: sorterEvalBin) =
        let existing =
            match this.evalBins.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newBin = sorterEvalBin.create()
                this.evalBins.[key] <- newBin
                newBin
        for kvp in bin.SubBins do
            existing.MergeSubBin kvp.Key kvp.Value



module SorterSetEvalBins =

    let create (sorterSetEval: sorterSetEval) : sorterSetEvalBins =
        let bins = sorterSetEvalBins.create sorterSetEval.SorterSetEvalId
        sorterSetEval.SorterEvals
        |> Array.iter (fun se -> bins.AddSorterEval se)
        bins

    let getBinCountReport 
            (sortingWidth: int<sortingWidth> option) 
            (sorterModelKey: string) 
            (sorterSetEvalBins: sorterSetEvalBins) : string array array =
        let widthStr = 
            sortingWidth 
            |> Option.map (fun w -> (%w).ToString()) 
            |> Option.defaultValue "N/A"
        sorterSetEvalBins.EvalBins
        |> Seq.collect (fun kvp ->
            kvp.Value.SubBins
            |> Seq.map (fun subKvp ->
                [|
                    widthStr
                    sorterModelKey
                    (%kvp.Key.CeCount).ToString()
                    (%kvp.Key.StageLength).ToString()
                    kvp.Key.IsSorted.ToString()
                    subKvp.Value.EvalCount.ToString()
                |]))
        |> Seq.toArray

    let merge (target: sorterSetEvalBins) (source: sorterSetEvalBins) : sorterSetEvalBins =
        for kvp in source.EvalBins do
            target.MergeBin kvp.Key kvp.Value
        target
