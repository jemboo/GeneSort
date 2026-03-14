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

    member this.CeCount with get() : int<ceLength> = this.ceCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.IsSorted with get() : bool = this.isSorted

// ---------------------------------------------------------------------------
// Sub-Bin: Stores IDs and a limited set of representative evaluations
// ---------------------------------------------------------------------------
type sorterEvalSubBin =
    private {
        representativeEvals: ResizeArray<sorterEval>
        sorterIds: ResizeArray<Guid<sorterId>>
    }
    static member create(eval: sorterEval) =
        let reps = ResizeArray<sorterEval>()
        reps.Add(eval)
        let ids = ResizeArray<Guid<sorterId>>()
        ids.Add(eval.SorterId)
        { 
            representativeEvals = reps
            sorterIds = ids 
        }

    member this.EvalCount with get() = this.sorterIds.Count
    member this.RepresentativeEvals with get() = this.representativeEvals :> IReadOnlyList<sorterEval>
    member this.SorterIds with get() = this.sorterIds :> IReadOnlyList<Guid<sorterId>>

    member this.Add(eval: sorterEval, maxReps: int) = 
        if this.representativeEvals.Count < maxReps then
            this.representativeEvals.Add(eval)
        this.sorterIds.Add(eval.SorterId)

    member this.AddIdOnly(sorterId: Guid<sorterId>) =
        this.sorterIds.Add(sorterId)

// ---------------------------------------------------------------------------
// Bin: Collection of sub-bins keyed by the hash of ceUseCounts
// ---------------------------------------------------------------------------
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
    
    member internal this.Add(eval: sorterEval, maxReps: int) =
        let key = eval.CeBlockEval.CeUseCounts.GetHashCode()
        match this.subBins.TryGetValue(key) with
        | true, existing -> 
            existing.Add(eval, maxReps)
        | false, _ ->
            this.subBins.[key] <- sorterEvalSubBin.create(eval)

    member this.MergeSubBin (hashKey: int) (subBin: sorterEvalSubBin, maxReps: int) =
        match this.subBins.TryGetValue(hashKey) with
        | true, existing -> 
            for id in subBin.SorterIds do existing.AddIdOnly(id)
            for rep in subBin.RepresentativeEvals do
                if existing.RepresentativeEvals.Count < maxReps then
                    existing.Add(rep, maxReps)
        | false, _ ->
            this.subBins.[hashKey] <- subBin

// ---------------------------------------------------------------------------
// SetBins: Top level container for the evaluation run
// ---------------------------------------------------------------------------
type sorterSetEvalBins = 
    private {
        sorterSetEvalId: Guid<sorterSetEvalId>
        evalBins: Dictionary<sorterEvalKey, sorterEvalBin>
        maxRepresentativesPerSubBin: int
    }
    static member create (sorterSetEvalId: Guid<sorterSetEvalId>, maxReps: int) =
        {
            sorterSetEvalId = sorterSetEvalId
            evalBins = Dictionary<sorterEvalKey, sorterEvalBin>()
            maxRepresentativesPerSubBin = maxReps
        }

    member this.SorterSetEvalId with get() = this.sorterSetEvalId
    member this.EvalBins with get() = this.evalBins :> IReadOnlyDictionary<sorterEvalKey, sorterEvalBin>
    member this.BinCount with get() = this.evalBins.Count
    member this.MaxRepresentativesPerSubBin with get() = this.maxRepresentativesPerSubBin
    member this.TotalEvalCount with get() = 
        this.evalBins.Values |> Seq.sumBy (fun b -> b.EvalCount)

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
        bin.Add(sorterEval, this.maxRepresentativesPerSubBin)

    member this.MergeBin (key: sorterEvalKey) (bin: sorterEvalBin) =
        let existing =
            match this.evalBins.TryGetValue(key) with
            | true, existing -> existing
            | false, _ ->
                let newBin = sorterEvalBin.create()
                this.evalBins.[key] <- newBin
                newBin
        for kvp in bin.SubBins do
            existing.MergeSubBin kvp.Key (kvp.Value, this.maxRepresentativesPerSubBin)


    member this.GetRepresentativeSorterEvals : sorterEval [] =
            this.evalBins.Values                 // Get all sorterEvalBins
            |> Seq.collect (fun bin -> 
                bin.SubBins.Values               // Get all sorterEvalSubBins
                |> Seq.collect (fun subBin -> 
                    subBin.RepresentativeEvals)) // Collect the evaluations
            |> Seq.toArray


// ---------------------------------------------------------------------------
// Module: Logic for operating on sorterSetEvalBins
// ---------------------------------------------------------------------------
module SorterSetEvalBins =

    let create (maxReps: int) (sorterSetEval: sorterSetEval) : sorterSetEvalBins =
        let bins = sorterSetEvalBins.create (sorterSetEval.SorterSetEvalId, maxReps)
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