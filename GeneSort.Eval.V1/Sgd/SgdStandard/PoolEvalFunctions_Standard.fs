namespace GeneSort.Eval.V1.Sgd.Standard

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Core
open GeneSort.Eval.V1.Sgd

module PoolEvalFunctions_Standard  =

    /// Evaluates a sorterPool given a poolMeasure.
    /// Composite Score = (1.0 * AverageScore) - (stDevWeight * StandardDeviationOfScores)
    /// Lower scores represent better performance; larger std deviations decrease the final score.
    let getFunctionForMeasure (measure: sorterPoolMeasure) : (sorterPool_Standard -> float<sorterPoolEvalScore>) =
        match measure with
        | StDevPool m ->
            fun pool ->
                let avg = SorterPool_Standard.getAverageScore m.SorterEvalMeasure pool |> UMX.untag
                let stdDev = SorterPool_Standard.getStandardDeviationOfScores m.SorterEvalMeasure pool |> UMX.untag
                let weight = %m.StDevWeight
                // Subtract stdDev component since larger standard deviation is better (lowers score)
                let compositeScore = avg - (weight * stdDev)
                UMX.tag<sorterPoolEvalScore> compositeScore


    /// Evaluates the pool score using the specified poolMeasure.
    let getPoolScore (measure: sorterPoolMeasure) (pool: sorterPool_Standard) : float<sorterPoolEvalScore> =
        let evalFunc = getFunctionForMeasure measure
        evalFunc pool
