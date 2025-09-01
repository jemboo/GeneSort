namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.SortingOps
open System.Collections.Generic

[<Struct; StructuralEquality; NoComparison>]
type sorterEvalKey = {
    ceCount: int<ceCount>
    stageCount: int<stageCount>
}

type sorterEvalBin = {
    mutable binCount: int
    mutable sorterEvals: ResizeArray<sorterEval>
}

type sorterSetEvalSamples = {
    sorterSetEvalId: Guid<sorterSetEvalId>
    mutable totalSampleCount: int
    maxBinCount: int
    evalBins: Dictionary<sorterEvalKey, sorterEvalBin>
}

module SorterSetEvalSamples =

    let addSorterEval (sorterEval: sorterEval) 
                      (sorterEvalSamples: sorterSetEvalSamples) : unit =
        let key = {
            ceCount = sorterEval.getUsedCeCount()
            stageCount = sorterEval.getStageCount()
        }
        let sorterEvalBin =
            if sorterEvalSamples.evalBins.ContainsKey(key) then
                sorterEvalSamples.evalBins.[key]
            else
                let newBin = {
                    binCount = 0
                    sorterEvals = ResizeArray<sorterEval>()
                }
                sorterEvalSamples.evalBins.[key] <- newBin
                newBin
        
        sorterEvalBin.binCount <- sorterEvalBin.binCount + 1
        if sorterEvalBin.sorterEvals.Count < sorterEvalSamples.maxBinCount then
            sorterEvalBin.sorterEvals.Add(sorterEval)
        
        sorterEvalSamples.totalSampleCount <- sorterEvalSamples.totalSampleCount + 1

    let create (maxBinCount: int) (sorterSetEval:sorterSetEval) : sorterSetEvalSamples =
        let sorterEvalSamples = {
            sorterSetEvalId = sorterSetEval.SorterSetEvalId
            totalSampleCount = 0
            maxBinCount = maxBinCount
            evalBins = Dictionary<sorterEvalKey, sorterEvalBin>()
        }
        sorterSetEval.SorterEvals
        |> Array.iter (fun se -> addSorterEval se sorterEvalSamples)
        sorterEvalSamples