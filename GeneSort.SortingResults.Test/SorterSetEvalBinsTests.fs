namespace GeneSort.SortingResults.Test

open System
open FSharp.UMX
open Xunit
open FsUnit.Xunit   
//open GeneSort.Sorter
//open GeneSort.Sorter.Sorter
//open GeneSort.SortingOps
//open GeneSort.Sorter.Sortable

type SorterSetEvalBinsTests() = 

    [<Fact>]
    let ``Placeholder test`` () =
        // Placeholder test to ensure the test framework is set up correctly
        let expected = 42
        let actual = 40 + 2
        actual |> should equal expected