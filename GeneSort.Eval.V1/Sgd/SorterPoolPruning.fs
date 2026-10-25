namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.SortingOps

module internal SorterPoolPruning =
    /// Relax strict filtering only when it would remove every evaluated candidate.
    let candidates filterUnsorted lengthCutoff getEvaluation members =
        if not filterUnsorted then false, members
        else
            let evaluated = members |> Array.filter (getEvaluation >> Option.isSome)
            let eligible = evaluated |> Array.filter (getEvaluation >> Option.get >> SorterEval.getIsShortEnough lengthCutoff)
            if Array.isEmpty eligible then true, evaluated
            else false, eligible

    /// During recovery, improve sorting correctness before optimizing size.
    let recoveryRank recovering evaluation =
        if recovering then
            evaluation |> Option.map (SorterEval.getUnsortedCount >> int) |> Option.defaultValue System.Int32.MaxValue
        else 0
