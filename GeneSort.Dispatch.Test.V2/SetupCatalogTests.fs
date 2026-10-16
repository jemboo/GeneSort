module GeneSort.Dispatch.Test.V2.SetupCatalogTests

open System
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.Catalogs.SortableTestsWithPrefix

let private parameters = runParameters.create Map.empty

let private context (step: string -> setupFunction) : setupContext = {
    MakeUniformSorterModelSet = step "models"
    MakeStandardTestsWithPrefix = step "standard"
    MakeMergeTestsWithPrefix = step "merge"
    MakePrefixTestsWithPrefix = step "prefix"
}

[<Theory>]
[<InlineData("setup.standard", "standard")>]
[<InlineData("setup.merge", "merge")>]
[<InlineData("setup.prefix", "prefix")>]
let ``combined setup executes models then the selected tests`` name expectedTests =
    let calls = ResizeArray<string>()
    let bindings = context (fun label rp -> async {
        Assert.Same(parameters, rp)
        calls.Add label
        return Ok ()
    })
    let result = SetupCatalog.get name bindings parameters |> Async.RunSynchronously
    Assert.Equal(Ok (), result)
    Assert.Equal<string list>(["models"; expectedTests], List.ofSeq calls)

[<Theory>]
[<InlineData("models")>]
[<InlineData("standard")>]
let ``combined setup propagates errors and stops at the failing step`` failingStep =
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async {
        calls.Add label
        return if label = failingStep then Error "setup failed" else Ok ()
    })
    let result = SetupCatalog.get SetupCatalogNames.standard bindings parameters |> Async.RunSynchronously
    Assert.Equal(Error "setup failed", result)
    let expected = if failingStep = "models" then ["models"] else ["models"; "standard"]
    Assert.Equal<string list>(expected, List.ofSeq calls)

[<Fact>]
let ``setup catalog bindings do not leak across runs`` () =
    let calls = ResizeArray<string>()
    let bind label = context (fun step _ -> async { calls.Add(label + step); return Ok () })
    let first = SetupCatalog.get SetupCatalogNames.prefix (bind "first:")
    let second = SetupCatalog.get SetupCatalogNames.prefix (bind "second:")
    second parameters |> Async.RunSynchronously |> ignore
    first parameters |> Async.RunSynchronously |> ignore
    Assert.Equal<string list>(["second:models"; "second:prefix"; "first:models"; "first:prefix"], List.ofSeq calls)

[<Fact>]
let ``custom catalog entries can be composed and reject invalid registrations`` () =
    let modelName, testName, setupName = string (Guid.NewGuid()), string (Guid.NewGuid()), string (Guid.NewGuid())
    let modelBuilder : setupBuilder = fun ctx -> ctx.MakeUniformSorterModelSet
    let testsBuilder : setupBuilder = fun ctx -> ctx.MakeMergeTestsWithPrefix
    SorterModelSetCatalog.register modelName modelBuilder
    SortableTestsWithPrefixCatalog.register testName testsBuilder
    SetupCatalog.register setupName (SetupFunctions.fromCatalogs modelName testName)
    let calls = ResizeArray<string>()
    let bindings = context (fun label _ -> async { calls.Add label; return Ok () })
    Assert.Equal(Ok (), SetupCatalog.get setupName bindings parameters |> Async.RunSynchronously)
    Assert.Equal<string list>(["models"; "merge"], List.ofSeq calls)
    Assert.Throws<InvalidOperationException>(fun () -> SorterModelSetCatalog.register modelName modelBuilder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SortableTestsWithPrefixCatalog.register testName testsBuilder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SetupCatalog.register setupName modelBuilder) |> ignore
    Assert.Throws<ArgumentException>(fun () -> SetupCatalog.register " " modelBuilder) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SetupCatalog.get "missing-setup" |> ignore) |> ignore
