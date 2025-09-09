namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open GeneSort.Sorter.Sortable



module SorterTestModelKey =


    let maxOrbit () : string*string list =
        ("MaxOrbiit", ["100000"])


    let sortableArrayTypes () : string*string list =
        ("SortableArrayType", 
            [sortableArrayType.Ints; sortableArrayType.Bools] |> List.map(SortableArrayType.toString))

