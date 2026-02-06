namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting


type sortingModelEval =
     | Sorter of sorterEval


module SoringModelEval =

    let getId (modelEval: sortingModelEval) : Guid<sorterId> =
        match modelEval with
        | Sorter eval -> eval.SorterId
