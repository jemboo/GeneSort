namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open System.Collections.Generic

[<Struct; StructuralEquality; NoComparison>]
type sorterEvalKey = {
    ceCount: int<ceLength>
    stageLength: int<stageLength>
}

type sorterEvalBin = {
    mutable binCount: int
    mutable sorterEvals: ResizeArray<sorterEval>
}


module SorterEvalBin =
    //returns [(uuct1, ct1); (uuct2, ct2) ... ], a histogram report of
    // unsortedCount properties of sorterEvalBin.sorterEvals, where ct1 is the
    // number of sorterEvals that have unsortedCount = uuct1
    let getUnsortedHistogram (seb: sorterEvalBin) :string =
            seb.sorterEvals
            |> Seq.groupBy (fun se -> se.CeBlockEval.UnsortedCount)
            |> Seq.sortBy fst
            |> Seq.map (fun (key, group) -> sprintf "(%d, %d)" key (Seq.length group))
            |> String.concat "; "
            |> sprintf "[%s]"



type sorterSetEvalBins = {
    sorterSetEvalId: Guid<sorterSetEvalId>
    mutable totalSampleCount: int
    maxSorterEvalCount: int
    evalBins: Dictionary<sorterEvalKey, sorterEvalBin>
}


module SorterSetEvalBins =

    let addSorterEval (sorterEval: sorterEval) 
                      (sorterSetEvalBins: sorterSetEvalBins) : unit =
        let key = {
            ceCount = sorterEval.CeBlockEval.CeUseCounts.UsedCeCount
            stageLength = sorterEval.CeBlockEval.getStageSequence.StageLength
        }

        let sorterEvalBin =
            if sorterSetEvalBins.evalBins.ContainsKey(key) then
                sorterSetEvalBins.evalBins.[key]
            else
                let newBin = {
                    binCount = 0
                    sorterEvals = ResizeArray<sorterEval>()
                }
                sorterSetEvalBins.evalBins.[key] <- newBin
                newBin
        
        if sorterEvalBin.sorterEvals.Count < sorterSetEvalBins.maxSorterEvalCount then
            sorterEvalBin.sorterEvals.Add(sorterEval)
            sorterEvalBin.binCount <- sorterEvalBin.binCount + 1
        
        sorterSetEvalBins.totalSampleCount <- sorterSetEvalBins.totalSampleCount + 1


    let create (maxBinCount: int) (sorterSetEval:sorterSetEval) : sorterSetEvalBins =
        let sorterSetEvalBins = {
            sorterSetEvalId = sorterSetEval.SorterModelSetEvalId
            totalSampleCount = 0
            maxSorterEvalCount = maxBinCount
            evalBins = Dictionary<sorterEvalKey, sorterEvalBin>()
        }

        //let gps = sorterSetEval.SorterEvals |> Array.groupBy (fun se -> (se.getUsedCeCount(), se.getStageLength()))

        sorterSetEval.SorterEvals
        |> Array.iter (fun se -> addSorterEval se sorterSetEvalBins)
        sorterSetEvalBins


    /// Returns an array of int arrays, each inner array containing [| ceCount; stageLength; binCount |]
    let getBinCountReport 
                (sortingWidth:int<sortingWidth> option) 
                (sorterModelKey:string) 
                (sorterSetEvalBins: sorterSetEvalBins) : string array array =
        let lines = 
            (sorterSetEvalBins.evalBins : Dictionary<sorterEvalKey, sorterEvalBin>)
            |> Seq.map (fun kvp -> 
                        [| 
                            (%sortingWidth.Value).ToString();
                            sorterModelKey;
                            (%kvp.Key.ceCount).ToString(); 
                            (%kvp.Key.stageLength).ToString(); 
                            (kvp.Value.binCount).ToString();
                            (kvp.Value |> SorterEvalBin.getUnsortedHistogram)
                        |])
            |> Seq.toArray
        lines