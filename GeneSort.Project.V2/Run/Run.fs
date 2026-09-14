namespace GeneSort.Project.V2.Run

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V2


type Run =
     | SortableSet of RunSortableSet



module Run =

    let toString (run: Run) : string =
        match run with
        | SortableSet s -> sprintf "SortableSet: %s" %s.RunName