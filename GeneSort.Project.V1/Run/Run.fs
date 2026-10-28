namespace GeneSort.Project.V1.SortableSet

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1


type Run =
     | SortableSet of RunSortableSet



module Run =

    let toString (run: Run) : string =
        match run with
        | SortableSet s -> sprintf "SortableSet: %s" %s.RunName