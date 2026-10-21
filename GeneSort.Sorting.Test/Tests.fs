module Tests

open System
open Xunit
open GeneSort.Sorting
open Microsoft.FSharp.Reflection

[<Fact>]
let ``All sortable data formats round trip through strings`` () =
    for case in FSharpType.GetUnionCases(typeof<sortableDataFormat>) do
        let format = FSharpValue.MakeUnion(case, [||]) :?> sortableDataFormat
        let restored = format |> SortableDataFormat.toString |> SortableDataFormat.fromString
        Assert.Equal<sortableDataFormat>(format, restored)

[<Fact>]
let ``My test`` () =
    Assert.True(true)
