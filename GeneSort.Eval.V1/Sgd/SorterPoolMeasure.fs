namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Core

[<Measure>] type stDevWeight
[<Measure>] type sorterPoolEvalScore

type stDevPoolMeasure = private {
    stDevWeight: float<stDevWeight>
    sorterEvalMeasure: sorterEvalMeasure
} with
    static member create 
                (stDevWeight: float<stDevWeight>) 
                (sorterEvalMeasure: sorterEvalMeasure) : stDevPoolMeasure =
        {
            stDevWeight = stDevWeight
            sorterEvalMeasure = sorterEvalMeasure
        }

    member this.StDevWeight: float<stDevWeight> = this.stDevWeight
    member this.SorterEvalMeasure: sorterEvalMeasure = this.sorterEvalMeasure

    member this.ToCompactString() =
        let stWStr = UmxExt.floatToRaw this.stDevWeight
        let childStr = SorterEvalMeasure.toCompactString this.sorterEvalMeasure
        sprintf "StDevPool(stDevW=%s, measure=%s)" stWStr childStr

    static member FromCompactString(s: string) : stDevPoolMeasure =
        let stDevW = CompactStringParser.parseFloat "stDevW" s
        let measureStr = CompactStringParser.getValue "measure" s
        let childMeasure = SorterEvalMeasure.fromCompactString measureStr
        stDevPoolMeasure.create stDevW childMeasure


type sorterPoolMeasure =
    | StDevPool of stDevPoolMeasure


module SorterPoolMeasure =

    let noStdev = 
        stDevPoolMeasure.create 
                (0.0<stDevWeight>) 
                SorterEvalMeasure.stageBiased
                |> sorterPoolMeasure.StDevPool

    let stdev = 
        stDevPoolMeasure.create 
                (0.4<stDevWeight>) 
                SorterEvalMeasure.stageBiased
                |> sorterPoolMeasure.StDevPool



    let toCompactString (measure: sorterPoolMeasure) : string =
        match measure with
        | StDevPool m -> m.ToCompactString()

    let fromCompactString (s: string) : sorterPoolMeasure =
        let trimmed = s.Trim()
        if trimmed.StartsWith("StDevPool") then
            StDevPool (stDevPoolMeasure.FromCompactString trimmed)
        else
            failwithf "Unknown compact pool measure format in '%s'" s

    let fromCompactStringOpt (s: string) : sorterPoolMeasure option =
        try Some (fromCompactString s) with _ -> None


module PoolEvalFunctions =

    /// Evaluates a sorterPool given a poolMeasure.
    /// Composite Score = (1.0 * AverageScore) - (stDevWeight * StandardDeviationOfScores)
    /// Lower scores represent better performance; larger std deviations decrease the final score.
    let getFunctionForMeasure (measure: sorterPoolMeasure) : (sorterPool -> float<sorterPoolEvalScore>) =
        match measure with
        | StDevPool m ->
            fun pool ->
                let avg = SorterPool.getAverageScore m.SorterEvalMeasure pool |> UMX.untag
                let stdDev = SorterPool.getStandardDeviationOfScores m.SorterEvalMeasure pool |> UMX.untag
                let weight = %m.StDevWeight
                // Subtract stdDev component since larger standard deviation is better (lowers score)
                let compositeScore = avg - (weight * stdDev)
                UMX.tag<sorterPoolEvalScore> compositeScore


    /// Evaluates the pool score using the specified poolMeasure.
    let getPoolScore (measure: sorterPoolMeasure) (pool: sorterPool) : float<sorterPoolEvalScore> =
        let evalFunc = getFunctionForMeasure measure
        evalFunc pool
