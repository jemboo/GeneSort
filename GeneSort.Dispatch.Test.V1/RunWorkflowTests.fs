module GeneSort.Dispatch.Test.V1.RunWorkflowTests

open System
open System.IO
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.Mp.V1
open GeneSort.Dispatch.V1

let private generate configured = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>

[<Fact>]
let ``workflows distinguish publication preparation fresh evolution and continuation`` () =
    Assert.True((SortableTests.Merge.Test()).IsDirectRun)
    Assert.True((SorterEval.Standard.Test()).IsPreparedRun)
    Assert.True((SorterMutate.Standard.Msrs()).IsPreparedRun)
    Assert.True((SorterSgd.p32.Msuf4a.SimpleS.Soss.SossS()).IsEvolutionRun)
    Assert.True((SorterSgd.p32.Msuf4a.Standard_append.Test()).IsContinuationRun)
    for kind in RunTypeCatalog.all do
        for example in kind.Examples do
            Assert.Equal<run>(example.Run, example.Run |> RunDto.fromDomain |> RunDto.toDomain)

[<Fact>]
let ``direct runs have one sweep and no Setup artifacts or action`` () =
    for configured in [SortableTests.Merge.Test(); SortableTests.Prefix.Width24s()] do
        Assert.False(configured.HasSetup)
        Assert.Empty(configured.SetupParameterSpans)
        Assert.Equal(4I, RunPreparation.count { Run = configured; FirstReplica = 0; ReplicaCount = 1 })
        let prepared = RunPreparation.prepare { Run = configured; FirstReplica = 0; ReplicaCount = 1 }
        Assert.Equal(4, prepared.FileCount)
        Assert.Empty(prepared.SetupFiles)
        Assert.Empty((runHost.Create configured 1).MakeSetupParameters 0<replNumber> 1<replNumber>)
        Assert.Throws<ArgumentException>(fun () -> SetupExecutorCatalog.prepare Unchecked.defaultof<_> configured Unchecked.defaultof<_> |> ignore) |> ignore
        Assert.DoesNotContain(RunTypeEditor.catalogChoices configured, fun choice -> choice.Role.StartsWith("Setup."))

[<Fact>]
let ``old direct definitions fold duplicate Setup spans into Execution without changing file queries`` () =
    let original = SortableTests.Merge.Test()
    let dto = RunDto.fromDomain original
    let old = { dto with SetupParameterSpans = dto.RunParameterSpans } |> RunDto.toDomain
    Assert.Equal<run>(original, old)
    Assert.Equal<Parameters.runParameters>(generate original, generate old)

[<Fact>]
let ``fixed overrides are rejected by runtime and editor including enhancer supplied defaults`` () =
    let configured = SorterSgd.p32.Msuf4a.SimpleS.Soss.SossS()
    for key, value in [ParamKeys.activationProbabilityKey, "0.5"; ParamKeys.sorterCountPerPoolKey, "64"
                       ParamKeys.rngTypeKey, "LCG"; ParamKeys.sortingWidthKey, "24"] do
        let changed = Run.withParameterSpans configured.SetupParameterSpans (configured.RunParameterSpans @ [key, [value]]) configured
        Assert.Throws<ArgumentException>(fun () -> generate changed |> ignore) |> ignore
        let args = (RunTypeEditor.fromRun configured).ToArgs()
        let draft = runDraft.create { args with ExecutionParameters = Array.append args.ExecutionParameters [|parameterDraft.create key [|value|]|] }
        Assert.True(RunTypeEditor.build configured draft |> Result.isError)

[<Fact>]
let ``read only rows include resolved defaults and follow Setup edits`` () =
    let configured = SorterSgd.p32.Msuf4a.SimpleS.Soss.SossS()
    let args = (RunTypeEditor.fromRun configured).ToArgs()
    let setup = args.SetupParameters |> Array.map (fun row ->
        if row.Key = ParamKeys.activationProbabilityKey then parameterDraft.create row.Key [|"0.25"; "0.75"|] else row)
    let draft = runDraft.create { args with SetupParameters = setup }
    let inherited = RunTypeEditor.inheritedParameters configured draft
    let values key = inherited |> Array.find (fun row -> row.Key = key) |> fun row -> row.Values
    Assert.Equal<string>([|"0.25"; "0.75"|], values ParamKeys.activationProbabilityKey)
    Assert.Equal<string>([|"32"|], values ParamKeys.sortingWidthKey)
    Assert.Equal<string>([|"LCG"|], values ParamKeys.rngTypeKey)
    Assert.DoesNotContain(inherited, fun row -> row.Key = ParamKeys.activatorMutationRateKey)

[<Fact>]
let ``roles depend on workflow and continuations retain explicit resize and branching permissions`` () =
    let fresh = SorterSgd.p32.Msuf4a.SimpleS.Soss.SossS()
    let mutate = SorterMutate.Standard.Msrs()
    let continuation = SorterSgd.p32.Msuf4a.Standard_append.Test()
    Assert.Equal(parameterRole.InheritedReadOnly, Run.parameterRole mutate ParamKeys.modificationRateKey)
    Assert.Equal(parameterRole.InheritedOverridable, Run.parameterRole fresh ParamKeys.modificationRateKey)
    Assert.Equal(parameterRole.SetupOnly, Run.parameterRole fresh ParamKeys.controlledSeedAMaxKey)
    Assert.Equal(parameterRole.ExecutionOnly, Run.parameterRole fresh ParamKeys.partitionCountKey)
    for key in [ParamKeys.mutationModKey; ParamKeys.sorterCountPerPoolKey; ParamKeys.sorterCountPerPoolSetKey] do
        Assert.Equal(parameterRole.InheritedReadOnly, Run.parameterRole fresh key)
        Assert.Equal(parameterRole.InheritedOverridable, Run.parameterRole continuation key)
    Assert.Equal(parameterRole.InheritedReadOnly, Run.parameterRole continuation ParamKeys.sourceRunNameKey)

[<Fact>]
let ``Soss activator rates still share Setup references and have separate execution identities`` () =
    let configured = SorterSgd.p32.Msuf4a.SimpleS.Soss.SossS()
    let parameters = generate configured
    Assert.Equal(8, parameters.Length)
    for _, group in parameters |> Array.groupBy (fun rp -> rp.GetMutationMod()) do
        Assert.Equal(2, group.Length)
        Assert.Equal(Some 0.0, group[0].GetActivatorMutationRate())
        Assert.Equal(Some 0.01, group[1].GetActivatorMutationRate())
        Assert.Equal(group[0].GetSetupSorterPoolSetId(), group[1].GetSetupSorterPoolSetId())
        Assert.Equal(group[0].GetSetupSortableTestsWithPrefixId(), group[1].GetSetupSortableTestsWithPrefixId())
        Assert.NotEqual(group[0].GetId(), group[1].GetId())

[<Fact>]
let ``direct SortableTests file IDs remain unchanged`` () =
    let expected = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "WorkflowFileIds.txt")) |> Array.filter (fun line -> line.StartsWith("SortableTests.")) |> Set.ofArray
    let actual =
        [SortableTests.Merge.Test(), "SortableTests.Merge.Test"]
        |> List.collect (fun (configured, name) ->
            generate configured |> Array.toList |> List.collect (fun rp ->
                let query = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
                [geneSortDataType.Run configured.RunName; geneSortDataType.RunParameters configured.RunName
                 geneSortDataType.SortableTests ""; geneSortDataType.SorterSetEval ""; geneSortDataType.SorterPoolSet ""]
                |> List.choose (fun output -> query rp output |> Option.map (fun q -> sprintf "%s|%A|%O" name output q.Id))))
        |> Set.ofList
    Assert.Empty(Set.difference actual expected)
    // Only redundant direct-run bookkeeping rows disappear.
    let retained (line: string) = line.Contains("|SortableTests ")
    Assert.Equal<Set<string>>(Set.filter retained expected, Set.filter retained actual)
