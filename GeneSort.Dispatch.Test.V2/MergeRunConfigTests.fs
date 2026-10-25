module GeneSort.Dispatch.Test.V2.MergeRunConfigTests

open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1

let private configuration = function
    | "Test" -> SorterEval.RunCfg.Merge.Test()
    | "Small" -> SorterEval.RunCfg.Merge.Small()
    | "MediumLd" -> SorterEval.RunCfg.Merge.MediumLd()
    | "MediumHd" -> SorterEval.RunCfg.Merge.MediumHd()
    | "LargeLd" -> SorterEval.RunCfg.Merge.LargeLd()
    | name -> failwithf "Unknown Merge preset: %s" name

[<Theory>]
[<InlineData("Test")>]
[<InlineData("Small")>]
[<InlineData("MediumLd")>]
[<InlineData("MediumHd")>]
[<InlineData("LargeLd")>]
let ``Merge presets generate one correctly linked execution per setup combination and replica`` name =
    let configured = configuration name
    let host = runHost.Create configured 1
    let setups = host.MakeSetupParameters 2<replNumber> 4<replNumber>
    let executions = host.MakeRunParameters 2<replNumber> 4<replNumber>
    let expectedPerReplica =
        configured.SetupParameterSpans
        |> List.fold (fun count (_, values) -> count * values.Length) 1

    Assert.Equal("SorterEval.RunCfg", %configured.ProjectName)
    Assert.Equal("Merge", %configured.DatabaseName)
    Assert.Equal(name, %configured.RunName)
    Assert.NotEmpty(setups)
    Assert.Equal(2 * expectedPerReplica, setups.Length)
    Assert.Equal(setups.Length, executions.Length)
    Assert.Equal(executions.Length, executions |> Array.map (fun rp -> rp.GetId()) |> Array.distinct |> Array.length)

    let settings = setups |> Array.map (fun sp -> (sp.GetRepl(), sp.GetMergeLibId(), sp.GetSimpleSorterModelType()), sp) |> Map.ofArray
    Assert.Equal(setups.Length, settings.Count)
    let actual = executions |> Array.map (fun rp -> (rp.GetRepl(), rp.GetMergeLibId(), rp.GetSimpleSorterModelType())) |> Set.ofArray
    Assert.Equal(settings.Count, actual.Count)
    Assert.Equal<Set<_>>(settings |> Map.keys |> Set.ofSeq, actual)

    for rp in executions do
        let setup = settings.[rp.GetRepl(), rp.GetMergeLibId(), rp.GetSimpleSorterModelType()]
        let library = rp.GetMergeLibId().Value
        Assert.Equal(0, int library.SortingWidth % int library.MergeDimension)
        Assert.Equal(Some library.SortingWidth, rp.GetSortingWidth())
        Assert.Equal(Some configured.RunName, rp.GetRunName())
        for key, value in Map.toList setup.ParamMap do
            Assert.Equal(Some value, Map.tryFind key rp.ParamMap)
        let expectedReferences = SetupOutputs.referencesFor configured setup |> Map.ofList
        let actualReferences = SetupFileReferences.fromRunParameters rp |> Map.ofList
        Assert.Equal<Map<string, string>>(expectedReferences, actualReferences)
        for output, linkedId in [
            geneSortDataType.SorterModelSet "", rp.GetSetupSorterModelSetId()
            geneSortDataType.SortableTestsWithPrefix "", rp.GetSetupSortableTestsWithPrefixId()
        ] do
            let source = host.QueryParamsFromSetupParams setup output |> Option.get
            Assert.Equal(Some source.Id, linkedId)
        let execution = host.QueryParamsFromRunParams rp (geneSortDataType.Run configured.RunName) |> Option.get
        Assert.Equal(Some execution.Id, rp.GetId())

[<Fact>]
let ``Merge sweeps retain divisible width dimension combinations and omit invalid pairs`` () =
    let pairs name =
        let configured = configuration name
        (runHost.Create configured 1).MakeSetupParameters 0<replNumber> 1<replNumber>
        |> Array.map (fun sp ->
            let library = sp.GetMergeLibId().Value
            Assert.Equal(sorterLibVariant.VariantA, library.SorterLibVariant)
            int library.SortingWidth, int library.MergeDimension)
        |> Set.ofArray
    let small = pairs "Small"
    Assert.Contains((18, 3), small)
    Assert.Contains((36, 6), small)
    Assert.DoesNotContain((16, 3), small)
    Assert.DoesNotContain((18, 4), small)
    let medium = pairs "MediumLd"
    Assert.Contains((96, 3), medium)
    Assert.DoesNotContain((128, 3), medium)
    Assert.Equal<Set<int * int>>(set [96, 6], pairs "MediumHd")
    Assert.Equal<Set<int * int>>(set [192, 2; 256, 2; 512, 2], pairs "LargeLd")
