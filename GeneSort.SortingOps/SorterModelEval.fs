namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorting


type sorterModelEval =
     | Sorter of sorterEval


module SorterModelEval =

    let getId (modelEval: sorterModelEval) : Guid<sorterId> =
        match modelEval with
        | Sorter eval -> eval.SorterId
