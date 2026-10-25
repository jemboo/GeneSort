module GeneSort.Dispatch.Test.V2.SgdCheckpointTests

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1
open GeneSort.Dispatch.V1.SorterSgd

[<Theory>]
[<InlineData(false, -1)>]
[<InlineData(true, -1)>]
[<InlineData(false, 0)>]
[<InlineData(true, 0)>]
[<InlineData(false, 1)>]
[<InlineData(true, 1)>]
[<InlineData(false, 2)>]
[<InlineData(true, 2)>]
let ``checkpoint search supports Setup-only seeds and legacy generation zero Results`` soss highest =
    let parameters = runParameters.create Map.empty
    let config = { Name = "test"; Min = 0; MaxCount = Some (if highest = 1 then 2 else 4); Scale = 1.0; Method = Constant 1 }
    let exists generation =
        if highest = 0 then generation = 0
        else generation > 0 && generation <= highest
    let db =
        { new IGeneSortDb with
            member _.databaseName = UMX.tag "test"
            member _.makeQueryParamsFromRunParams rp dataType =
                Some (queryParams.create (UMX.tag "test") (UMX.tag "test") None (rp.GetGenerationCurrent()) dataType [||])
            member _.doesOutPutDataExist mode qp = async {
                Assert.Equal(geneSortDataMode.Results, mode)
                return exists (int qp.Generation.Value)
            }
            member _.loadIfFoundAsync mode qp = async {
                Assert.Equal(geneSortDataMode.Results, mode)
                Assert.True(exists (int qp.Generation.Value))
                let standard = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) qp.Generation.Value (Dim1 1) None
                let pool =
                    if soss then SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
                    else sorterPoolSet.Standard standard
                return Some (outputData.SorterPoolSet pool)
            }
            member _.saveAsync _ _ _ _ = failwith "Search must not save"
            member _.loadAsync _ _ = failwith "Unexpected load"
            member _.getRunParameters _ _ _ _ _ = failwith "Unexpected run parameters load" }
    let actual =
        if soss then
            Utils.loadHighestGenSorterPoolSetSoss config db parameters
            |> Async.RunSynchronously |> Result.map (Option.map (fun pool -> int pool.GenerationNumber))
        else
            Utils.loadHighestGenSorterPoolSet config db parameters
            |> Async.RunSynchronously |> Result.map (Option.map (fun pool -> int pool.GenerationNumber))
    Assert.Equal(Ok (if highest < 0 then None else Some highest), actual)

[<Theory>]
[<InlineData("success")>]
[<InlineData("query")>]
[<InlineData("maker")>]
[<InlineData("save")>]
[<InlineData("load")>]
[<InlineData("type")>]
let ``evolution test setup is persisted once and reused across generations`` failure =
    let parameters = (runParameters.create Map.empty).WithGenerationCurrent(Some 7<generationNumber>)
    let tests = sortableTests.Bools sortableBinaryTests.Empty
    let prefix: ce array = [||]
    let mutable stored = None
    let mutable creations = 0
    let mutable saves = 0
    let mutable firstQuery = None
    let db =
        { new IGeneSortDb with
            member _.databaseName = UMX.tag "test"
            member _.makeQueryParamsFromRunParams rp dataType =
                Assert.Equal(Some 0<generationNumber>, rp.GetGenerationCurrent())
                Assert.Equal(geneSortDataType.SortableTestsWithPrefix "", dataType)
                if failure = "query" then None
                else Some (queryParams.create (UMX.tag "test") (UMX.tag "test") None (rp.GetGenerationCurrent()) dataType [||])
            member _.doesOutPutDataExist mode _ = async {
                Assert.Equal(geneSortDataMode.Setup, mode)
                return stored.IsSome
            }
            member _.saveAsync mode query data overwrite = async {
                Assert.Equal(geneSortDataMode.Setup, mode)
                Assert.False(%overwrite)
                saves <- saves + 1
                firstQuery <- Some query.Id
                stored <- Some data
                return if failure = "save" then Error "save failed" else Ok ()
            }
            member _.loadAsync mode query = async {
                Assert.Equal(geneSortDataMode.Setup, mode)
                Assert.Equal(firstQuery, Some query.Id)
                return
                    match failure with
                    | "load" -> Error "load failed"
                    | "type" -> Ok (outputData.RunParameters parameters)
                    | _ -> Ok stored.Value
            }
            member _.loadIfFoundAsync _ _ = failwith "Use loadAsync to preserve read errors"
            member _.getRunParameters _ _ _ _ _ = failwith "Unexpected run parameters load" }
    let makeTests (rp: runParameters) = async {
        Assert.Equal(Some 0<generationNumber>, rp.GetGenerationCurrent())
        creations <- creations + 1
        return if failure = "maker" then Error "maker failed" else Ok (tests, prefix)
    }
    let first = Utils.loadOrCreateSortableTestsWithPrefix makeTests db parameters |> Async.RunSynchronously
    if List.contains failure ["query"; "maker"; "save"; "load"; "type"] then
        Assert.True(Result.isError first)
        Assert.Equal((if failure = "query" then 0 else 1), creations)
        Assert.Equal((if List.contains failure ["save"; "load"; "type"] then 1 else 0), saves)
    else
        Assert.True(Result.isOk first)
        let resumed = parameters.WithGenerationCurrent(Some 42<generationNumber>)
        let second = Utils.loadOrCreateSortableTestsWithPrefix makeTests db resumed |> Async.RunSynchronously
        if failure = "success" then
            match second with
            | Ok (loadedTests, loadedPrefix) ->
                Assert.Same(tests, loadedTests)
                Assert.Same(prefix, loadedPrefix)
            | Error error -> failwith error
        else Assert.True(Result.isError second)
        Assert.Equal(1, creations)
        Assert.Equal(1, saves)
