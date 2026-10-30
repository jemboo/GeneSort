namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.SortingLib.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1
open GeneSort.FileDb.V1

module RunPhaseFileTests =
    let private withFolder test = task {
        let folder = Path.Combine(Path.GetTempPath(), "GeneSort-SetupFiles-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory folder |> ignore
        try do! test folder
        finally Directory.Delete(folder, true)
    }

    let private get result = Result.defaultWith failwith result
    let private publish folder configured = task {
        let! prepared = RunParameterFiles.prepareAsync folder { Run = configured; FirstReplica = 0; ReplicaCount = 1 }
        do! RunParameterFiles.generateAsync prepared false null
        return! RunParameterFiles.loadAllAsync folder configured
    }
    let private db folder (configured: run) =
        new GeneSortDbMp(DatabasePaths.dataRoot folder (string configured.ProjectName) (string configured.DatabaseName) |> UMX.tag, configured.RunQueryCatalogName, configured.ProjectName) :> IGeneSortDb
    let private save folder (configured: run) row =
        let query = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName row (geneSortDataType.RunParameters configured.RunName) |> Option.get
        (db folder configured).saveAsync geneSortDataMode.Project query (outputData.RunParameters row) (UMX.tag true) |> Async.StartAsTask
    let private custom setup =
        let name = "setup-selection-" + Guid.NewGuid().ToString("N")
        SetupExecutorCatalog.register name (fun _ _ _ rp overwrite cts progress ->
            setup rp overwrite cts progress |> Async.map (Result.map Parameters))
        // There deliberately is no ExecuteFunction registration: setup must never resolve or call it.
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        { configured with ExecuteFunctionName = name; SetupFunctionName = name } |> RunDto.toDomain

    let private phaseRunner = function
        | "Setup" -> RunSetupFiles.runAsync
        | "Execution" -> RunExecutionFiles.runSavedAsync
        | "Summary Reports" -> RunSummaryFiles.runAsync
        | name -> invalidArg "name" name

    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``Setup passes its exact path to execution while reopened rows retain file identities`` reopen = withFolder (fun folder -> task {
        let name = "live-setup-" + Guid.NewGuid().ToString("N")
        let mutable created: evoPath option = None
        SetupExecutorCatalog.register name (fun _ configured _ row _ _ _ -> async {
            let reference output id = queryParams.create configured.DatabaseName configured.ProjectName None None output [||] |> queryParams.withId id
            let start = {
                Tests = reference (geneSortDataType.SortableTestsWithPrefix "") (row.GetSetupSortableTestsWithPrefixId().Value)
                Pool = reference (geneSortDataType.SorterPoolSet "") (row.GetSetupSorterPoolSetId().Value)
                FirstGeneration = 0<generationNumber>
            }
            let path, progress = EvoPath.fromStart configured start (EvoStart.initializeExecution start row)
            created <- Some path
            return Ok (Evolution (path, progress))
        })
        ExecuteFunctionCatalog.register name (SgdExecution (fun _ _ path state _ _ _ -> async {
            if reopen then Assert.NotSame(created.Value, path) else Assert.Same(created.Value, path)
            Assert.Equal(created.Value.Start, path.Start)
            return Ok (state.WithFinished(Some true))
        }))
        let dto = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        let configured = { dto with ExecuteFunctionName = name; SetupFunctionName = name } |> RunDto.toDomain
        let! rows = publish folder configured
        let files = Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories) |> Array.sort
        let! setup = RunSetupFiles.runAsync folder configured [|rows[0]|] null
        Assert.Empty(setup.Errors)
        let path, progress = Assert.Single(setup.EvolutionPaths)
        Assert.Same(created.Value, path)
        let! result =
            if reopen then task {
                let! saved = RunParameterFiles.loadAllAsync folder configured
                let selected = saved |> Array.find (fun row -> row.GetId() = rows[0].GetId())
                return! RunExecutionFiles.runSavedAsync folder configured [|selected|] null
            }
            else RunExecutionFiles.runPathsAsync folder configured [|path, progress|] null
        Assert.Empty(result.Errors)
        Assert.Equal(1, result.Succeeded)
        let finishedPath, finishedProgress = Assert.Single(result.EvolutionPaths)
        if reopen then Assert.NotSame(path, finishedPath) else Assert.Same(path, finishedPath)
        Assert.Equal(Some false, progress.Finished)
        Assert.Equal(Some true, finishedProgress.Finished)
        let finished = EvoPath.toParameters finishedPath finishedProgress
        Assert.Equal(rows[0].GetId(), finished.GetId())
        Assert.Equal(rows[0].GetSetupSorterPoolSetId(), finished.GetSetupSorterPoolSetId())
        Assert.Equal(rows[0].GetSetupSortableTestsWithPrefixId(), finished.GetSetupSortableTestsWithPrefixId())
        Assert.Equal<string>(files, Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories) |> Array.sort)
    })

    [<Fact>]
    let ``selected setup persists successes skips finished rows and continues after errors`` () = withFolder (fun folder -> task {
        let calls = ResizeArray<int>()
        let configured = custom (fun row _ _ progress -> async {
            let mutation = int (row.GetMutationMod().Value)
            calls.Add mutation
            progress |> Option.iter (fun sink -> sink.Report "Preparing selected input")
            if mutation = 1 then return Error "Missing source input"
            else return Ok (row.WithMessage(Some "setup persisted"))
        })
        let! rows = publish folder configured
        let byMutation = rows |> Array.sortBy (fun row -> row.GetMutationMod())
        let finished = byMutation[2].WithRunFinished(Some true)
        let! saved = save folder configured finished
        get saved
        let progress = ResizeArray<runPhaseProgress>()
        let sink = { new IProgress<runPhaseProgress> with member _.Report update = progress.Add update }
        let! result = RunSetupFiles.runAsync folder configured [|byMutation[1]; finished; byMutation[0]|] sink
        Assert.Equal(1, result.Succeeded)
        Assert.Equal(1, result.Failed)
        Assert.Equal(1, result.Skipped)
        Assert.Contains("Missing source input", result.RowErrors[0]);
        Assert.Equal("", result.RowErrors[1]);
        Assert.Equal("", result.RowErrors[2]);
        Assert.Equal<runRowStatus>([|runRowStatus.Failed; runRowStatus.Completed; runRowStatus.Completed|], result.RowStatuses)
        Assert.Contains(progress, fun p -> p.RowIndex = Some 0 && p.Status = Some runRowStatus.Running)
        Assert.Contains(progress, fun p -> p.RowIndex = Some 0 && p.Status = Some runRowStatus.Failed)
        Assert.Contains(progress, fun p -> p.RowIndex = Some 2 && p.Status = Some runRowStatus.Completed)
        let index, _ = Assert.Single(result.Updates)
        Assert.Equal(2, index)
        Assert.Contains("Missing source input", result.Errors[0])
        Assert.Equal<int>([|1; 0|], calls.ToArray())
        Assert.Equal(0, progress[0].Completed)
        Assert.Equal(3, progress[progress.Count - 1].Completed)
        Assert.All(progress, fun update -> Assert.Equal(3, update.Total))
        Assert.Contains(progress, fun update -> update.Message.Contains("Preparing selected input"))
        Assert.Equal<int>(progress |> Seq.map (fun update -> update.Completed) |> Seq.sort, progress |> Seq.map (fun update -> update.Completed))
        let! after = RunParameterFiles.loadAllAsync folder configured
        let after = after |> Array.sortBy (fun row -> row.GetMutationMod())
        Assert.Equal(Some "setup persisted", after[0].ParamMap.TryFind ParamKeys.messageKey)
        Assert.Equal(byMutation[1], after[1])
        Assert.Equal(finished, after[2])
        Assert.Equal(byMutation[3], after[3])
        Assert.Empty(Directory.GetDirectories(folder, "Results", SearchOption.AllDirectories))
    })

    [<Theory>]
    [<InlineData("changed")>]
    [<InlineData("duplicate")>]
    [<InlineData("workflow")>]
    let ``direct path execution validates the whole selection before running`` scenario = withFolder (fun folder -> task {
        let mutable calls = 0
        let configured = custom (fun row _ _ _ -> async { return Ok row })
        ExecuteFunctionCatalog.register configured.ExecuteFunctionName (SgdExecution (fun _ _ _ row _ _ _ -> async {
            calls <- calls + 1
            return Ok row
        }))
        let! rows = publish folder configured
        let paths = rows |> Array.take 2 |> Array.map (EvoPath.fromParameters configured)
        match scenario with
        | "changed" ->
            let! result = save folder configured (rows[1].WithMessage(Some "Changed externally"))
            get result
        | "duplicate" -> paths[1] <- paths[0]
        | "workflow" ->
            let different = { RunDto.fromDomain configured with ExecuteFunctionName = "another-executor" } |> RunDto.toDomain
            paths[1] <- EvoPath.fromParameters different rows[1]
        | _ -> failwith scenario
        let! _ = Assert.ThrowsAnyAsync<Exception>(fun () -> RunExecutionFiles.runPathsAsync folder configured paths null :> Task)
        Assert.Equal(0, calls)
    })

    [<Theory>]
    [<InlineData("empty", "Setup")>]
    [<InlineData("duplicate", "Setup")>]
    [<InlineData("missing", "Setup")>]
    [<InlineData("changed", "Setup")>]
    [<InlineData("definition", "Setup")>]
    [<InlineData("other-run", "Setup")>]
    [<InlineData("empty", "Execution")>]
    [<InlineData("duplicate", "Execution")>]
    [<InlineData("missing", "Execution")>]
    [<InlineData("changed", "Execution")>]
    [<InlineData("definition", "Execution")>]
    [<InlineData("other-run", "Execution")>]
    [<InlineData("empty", "Summary Reports")>]
    [<InlineData("duplicate", "Summary Reports")>]
    [<InlineData("missing", "Summary Reports")>]
    [<InlineData("changed", "Summary Reports")>]
    [<InlineData("definition", "Summary Reports")>]
    [<InlineData("other-run", "Summary Reports")>]
    let ``invalid or stale selections are rejected before any phase runs`` scenario phase = withFolder (fun folder -> task {
        let mutable calls = 0
        let executor row _ _ _ = async { calls <- calls + 1; return Ok row }
        let configured = custom executor
        ExecuteFunctionCatalog.register configured.ExecuteFunctionName (SimpleExecution (fun _ -> executor))
        let runPhase = phaseRunner phase
        let! rows = publish folder configured
        let mutable selection = [|rows[0]; rows[1]|]
        let mutable requested = configured
        match scenario with
        | "empty" -> selection <- [||]
        | "duplicate" -> selection <- [|rows[0]; rows[0]|]
        | "missing" ->
            let query = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName rows[1] (geneSortDataType.RunParameters configured.RunName) |> Option.get
            let root = DatabasePaths.dataRoot folder (string configured.ProjectName) (string configured.DatabaseName) |> UMX.tag
            File.Delete(string (OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Project query))
        | "changed" ->
            let! result = save folder configured (rows[1].WithMessage(Some "changed externally"))
            get result
        | "definition" -> requested <- { RunDto.fromDomain configured with Description = "unsaved changes" } |> RunDto.toDomain
        | "other-run" -> selection <- [|rows[0].WithRunName(Some (UMX.tag "AnotherRun"))|]
        | _ -> failwith scenario
        let! _ = Assert.ThrowsAnyAsync<Exception>(fun () -> runPhase folder requested selection null :> Task)
        Assert.Equal(0, calls)
        Assert.Empty(Directory.GetDirectories(folder, "Results", SearchOption.AllDirectories))
    })

    [<Theory>]
    [<InlineData("Setup")>]
    [<InlineData("Execution")>]
    [<InlineData("Summary Reports")>]
    let ``each phase holds the publication lock until the selected batch finishes`` phase = withFolder (fun folder -> task {
        let started = TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        let release = TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        let executor row _ _ _ = async { return Ok row }
        let configured = custom executor
        ExecuteFunctionCatalog.register configured.ExecuteFunctionName (SimpleExecution (fun _ -> executor))
        let runPhase = phaseRunner phase
        let otherPhase = if phase = "Execution" then RunSetupFiles.runAsync else RunExecutionFiles.runSavedAsync
        let! rows = publish folder configured
        if phase = "Summary Reports" then
            rows[0] <- rows[0].WithRunFinished(Some true)
            let! saved = save folder configured rows[0]
            get saved
        let progress = { new IProgress<runPhaseProgress> with
                            member _.Report _ =
                                if started.TrySetResult() then release.Task.GetAwaiter().GetResult() }
        let running = runPhase folder configured [|rows[0]|] progress
        try
            do! started.Task.WaitAsync(TimeSpan.FromSeconds 10.)
            let! prepared = RunParameterFiles.prepareAsync folder { Run = configured; FirstReplica = 0; ReplicaCount = 1 }
            let! error = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunParameterFiles.generateAsync prepared true null :> Task)
            Assert.Contains("already", error.Message)
            let! conflict = Assert.ThrowsAsync<IOException>(fun () -> otherPhase folder configured [|rows[1]|] null :> Task)
            Assert.Contains("already", conflict.Message)
        finally release.TrySetResult() |> ignore
        let! result = running
        if phase = "Summary Reports" then Assert.Equal(1, result.Failed) // No saved result slices in this fixture.
        else Assert.Equal(1, result.Succeeded)
    })

    let private smallLibrary () =
        SimpleRun.create {
            ProjectName = UMX.tag "SorterEval"; DatabaseName = UMX.tag "Standard"; RunName = UMX.tag "Test"
            Description = "Small saved source library"; AllowOverwrite = false
            Setup = {
                FunctionName = SetupCatalogNames.standard
                QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
                ParametersPipelineName = SetupParametersPipelineNames.sorterEval
                ParameterSpans = [ParamKeys.excludeSelfCeKey, ["true"]; ParamKeys.collectSortableTestsKey, ["false"];
                                  ParamKeys.channelCountKey, ["4"]; ParamKeys.sortableDataFormatKey, ["BitVector512"]; ParamKeys.rngTypeKey, ["LCG"]
                                  ParamKeys.simpleSorterModelTypeKey, ["Msce"; "Msrs"]; ParamKeys.sorterCountKey, ["2"]]
            }
            Execution = {
                FunctionName = ExecuteFunctionNames.sorterEval
                QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                ParametersPipelineName = RunParametersPipelineNames.sorterEvalStandard
                ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V2"]]
            }
        } |> Run.ofSimple

    [<Fact>]
    let ``real setup creates only selected inputs reuses them and sources load from the selected folder`` () = withFolder (fun folder -> task {
        let configured = smallLibrary()
        let! rows = publish folder configured
        let selected = rows |> Array.find (fun row -> row.GetSimpleSorterModelType() = Some simpleSorterModelType.Msce)
        let! result = RunSetupFiles.runAsync folder configured [|selected|] null
        Assert.Equal(1, result.Succeeded)
        Assert.Empty(result.Errors)
        let files = Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories) |> Array.filter (fun path -> path.Contains(string Path.DirectorySeparatorChar + "Setup" + string Path.DirectorySeparatorChar)) |> Array.sort
        Assert.Equal(2, files.Length)
        Assert.Empty(Directory.GetDirectories(folder, "Results", SearchOption.AllDirectories))
        let before = files |> Array.map File.GetLastWriteTimeUtc
        let selected = result.Updates |> Array.exactlyOne |> snd |> RunPhaseOutput.toParameters
        let! again = RunSetupFiles.runAsync folder configured [|selected|] null
        Assert.Equal(1, again.Succeeded)
        Assert.Equal<DateTime>(before, files |> Array.map File.GetLastWriteTimeUtc)
        // The actual executor consumes prepared inputs, persists completion, and leaves the other row alone.
        let! evaluated = RunExecutionFiles.runSavedAsync folder configured [|selected|] null
        Assert.Empty(evaluated.Errors)
        Assert.Equal(1, evaluated.Succeeded)
        let! saved = RunParameterFiles.loadAllAsync folder configured
        let finished = saved |> Array.find (fun row -> row.GetId() = selected.GetId())
        let pending = saved |> Array.find (fun row -> row.GetId() <> selected.GetId())
        Assert.Equal(Some true, finished.GetRunFinished())
        Assert.NotEqual(Some true, pending.GetRunFinished())
        Assert.Equal(rows |> Array.find (fun row -> row.GetId() = pending.GetId()), pending)
        let! skipped = RunExecutionFiles.runSavedAsync folder configured [|finished|] null
        Assert.Equal(1, skipped.Skipped)
        Assert.Equal(0, skipped.Succeeded)
        let! unprepared = RunExecutionFiles.runSavedAsync folder configured [|pending|] null
        Assert.Equal(1, unprepared.Failed)
        Assert.Equal<DateTime>(before, files |> Array.map File.GetLastWriteTimeUtc)
        let setupFiles = Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories) |> Array.filter (fun path -> path.Contains(string Path.DirectorySeparatorChar + "Setup" + string Path.DirectorySeparatorChar)) |> Array.sort
        Assert.Equal<string>(files, setupFiles)
        let sources = WorkflowSourceFiles.create folder
        let! loaded = (sources.Evolution (UMX.tag "Standard") (UMX.tag "Standard")).LoadStandardSorterEvals 4<channelCount> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        Assert.Equal(2, (get loaded).SorterEvals.Length)
        let! absent = ((WorkflowSourceFiles.create (Path.Combine(folder, "other"))).Evolution (UMX.tag "Standard") (UMX.tag "Standard")).LoadStandardSorterEvals 4<channelCount> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        Assert.True(Result.isError absent)
        match absent with
        | Error message ->
            Assert.Contains("Missing source library SorterEval/Standard", message)
            Assert.Contains("Expected definitions under:", message)
            Assert.Contains("Prepare and execute this library", message)
            Assert.Contains(Path.Combine(folder, "other"), message)
        | Ok _ -> failwith "Expected a missing source library error."
    })

    [<Fact>]
    let ``SGD sources load from independently selected modules and catalogs with arbitrary run names`` () = withFolder (fun folder -> task {
        GeneSort.Project.V1.Catalogs.ProjectCatalogs.registerAll ()
        let registerContract name original =
            let builder = QueryParamsCatalogRun.get original
            QueryParamsCatalogRun.register name (fun project database parameters output ->
                builder project database parameters output |> Option.map (fun query ->
                    queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType
                        (Array.append (Map.toArray query.Properties) [|"ProducerContract", name|])))
        let testsCatalog = "tests.sgd-module-source"
        let evalsCatalog = "evals.sgd-module-source"
        registerContract testsCatalog QueryCatalogNamesRun.sortableTestPrefix
        registerContract evalsCatalog QueryCatalogNamesRun.sorterEvalStandard
        let dto = smallLibrary() |> RunDto.fromDomain
        let evaluation = RunDto.toDomain { dto with ProjectName = "Evaluation.Producers"; DataBaseName = "SeedBank"; RunName = "MyEvaluations"; RunQueryCatalogName = evalsCatalog }
        let! rows = publish folder evaluation
        let! setup = RunSetupFiles.runAsync folder evaluation rows null
        Assert.Empty(setup.Errors)
        let! rows = RunParameterFiles.loadAllAsync folder evaluation
        let! executed = RunExecutionFiles.runSavedAsync folder evaluation rows null
        Assert.Empty(executed.Errors)
        let prefix = prefixLibId.create 8<channelCount> 3<stageLength> prefixLibVariant.PrefixA
        let tests =
            DirectRun.create {
                ProjectName = UMX.tag "Test.Producers"; DatabaseName = UMX.tag "TestBank"; RunName = UMX.tag "MyTests"
                Description = "Selected test source"; AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sortableTests
                    QueryCatalogName = testsCatalog
                    ParametersPipelineName = RunParametersPipelineNames.sortableTests
                    ParameterSpans = [ParamKeys.prefixLibIdKey, [PrefixLibId.toString prefix]
                                      ParamKeys.sortableDataFormatKey, [SortableDataFormat.toString sortableDataFormat.BitVector512]]
                }
            } |> run.DirectRun
        let! testRows = publish folder tests
        let! generated = RunExecutionFiles.runSavedAsync folder tests testRows null
        Assert.Empty(generated.Errors)
        let host = WorkflowSourceFiles.create folder
        RunModuleCatalog.register "Sources.Tests" { ProjectName = tests.ProjectName; DatabaseName = tests.DatabaseName; ExecutionQueryCatalogName = testsCatalog }
        RunModuleCatalog.register "Sources.Evaluations" { ProjectName = evaluation.ProjectName; DatabaseName = evaluation.DatabaseName; ExecutionQueryCatalogName = evalsCatalog }
        let parameters = setupParameters.create (Map.ofList [
            ParamKeys.sortableTestsModuleKey, "Sources.Tests"; ParamKeys.sorterEvalModuleKey, "Sources.Evaluations"
            ParamKeys.replKey, "0"; ParamKeys.rngTypeKey, "LCG"; ParamKeys.sortableDataFormatKey, "BitVector512" ])
        let sources = host.EvolutionModules parameters
        let! loadedTests = sources.LoadPrefixTests 0<replNumber> prefix sortableDataFormat.BitVector512 |> Async.StartAsTask
        let! loadedEvals = sources.LoadStandardSorterEvals 4<channelCount> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        Assert.True(Result.isOk loadedTests)
        Assert.Equal(2, (get loadedEvals).SorterEvals.Length)
        Assert.False(Directory.Exists(Path.Combine(folder, "SortableTests", "Prefix")))
        Assert.False(Directory.Exists(Path.Combine(folder, "SorterEval", "Standard")))
        // A definition from another evaluation family is not a matching seed source.
        let differentFamily = RunDto.toDomain {
            RunDto.fromDomain evaluation with RunName = "DifferentFamily"; RunQueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
        }
        let! _ = RunDefinitionFiles.saveAsync folder differentFamily false
        let! stillStandard = sources.LoadStandardSorterEvals 4<channelCount> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        Assert.True(Result.isOk stillStandard)
        // A database with two different matching evaluations needs a narrower source choice.
        let alternative = RunDto.toDomain {
            RunDto.fromDomain evaluation with
                RunName = "OtherInputs"
                SetupParameterSpans = (RunDto.fromDomain evaluation).SetupParameterSpans |> Array.map (fun span ->
                    if span.Key = ParamKeys.sorterCountKey then { span with Values = [|"3"|] } else span)
        }
        let! _ = RunDefinitionFiles.saveAsync folder alternative false
        let! ambiguous = sources.LoadStandardSorterEvals 4<channelCount> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        match ambiguous with
        | Error message -> Assert.Contains("found 2", message)
        | Ok _ -> failwith "Do not choose an arbitrary evaluation from an ambiguous database."
        let wrong = host.EvolutionModules (setupParameters.create (parameters.ParamMap
            |> Map.add ParamKeys.sortableTestsModuleKey "Sources.WrongTests"
            |> Map.add ParamKeys.sorterEvalModuleKey "Sources.WrongEvals"))
        let! missingTests = wrong.LoadPrefixTests 0<replNumber> prefix sortableDataFormat.BitVector512 |> Async.StartAsTask
        let! missingEvals = wrong.LoadStandardSorterEvals 4<channelCount> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        match missingTests, missingEvals with
        | Error testsError, Error evalsError ->
            Assert.Contains("WrongTests", testsError)
            Assert.Contains("Unknown producing module", testsError)
            Assert.Contains("WrongEvals", evalsError)
        | _ -> failwith "Source loaders must not fall back to another database."
    })

    [<Theory>]
    [<InlineData("Prefix")>]
    [<InlineData("Merge")>]
    let ``evaluation setup requires matching tests and accepts any source run name`` database = withFolder (fun folder -> task {
        let prefix = prefixLibId.create 8<channelCount> 3<stageLength> prefixLibVariant.PrefixA
        let merge = mergeLibId.create 8<channelCount> 2<mergeDimension> sorterLibVariant.VariantA
        let key, library, format, testCatalog, setupCatalog, querySetup, queryRun, setupParametersPipelineName, runParametersPipelineName =
            if database = "Prefix" then
                ParamKeys.prefixLibIdKey, PrefixLibId.toString prefix, sortableDataFormat.BitVector512,
                QueryCatalogNamesRun.sortableTestPrefix, SetupCatalogNames.prefix,
                QueryCatalogNamesSetup.sorterEvalPrefix, QueryCatalogNamesRun.sorterEvalPrefix,
                SetupParametersPipelineNames.sorterEvalPrefix, RunParametersPipelineNames.sorterEvalPrefix
            else
                ParamKeys.mergeLibIdKey, MergeLibId.toString merge, sortableDataFormat.Int8Vector512,
                QueryCatalogNamesRun.sortableTestMerge, SetupCatalogNames.merge,
                QueryCatalogNamesSetup.sorterEvalMerge, QueryCatalogNamesRun.sorterEvalMerge,
                SetupParametersPipelineNames.sorterEval, RunParametersPipelineNames.sorterEvalMerge
        GeneSort.Project.V1.Catalogs.ProjectCatalogs.registerAll ()
        let producerCatalog = testCatalog + ".module-source-test"
        let originalBuilder = QueryParamsCatalogRun.get testCatalog
        QueryParamsCatalogRun.register producerCatalog (fun project database parameters outputType ->
            originalBuilder project database parameters outputType
            |> Option.map (fun query ->
                queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType
                    (Array.append (Map.toArray query.Properties) [|"ProducerContract", producerCatalog|])))
        let inputs = [key, [library]; ParamKeys.sortableDataFormatKey, [SortableDataFormat.toString format];
                      ParamKeys.channelCountKey, ["8"]; ParamKeys.excludeSelfCeKey, ["true"]; ParamKeys.collectSortableTestsKey, ["false"]]
        let sourceProject = "SortableTests." + database + ".Channels8"
        let sourceModule = sourceProject
        RunModuleCatalog.register sourceModule {
            ProjectName = UMX.tag sourceProject
            DatabaseName = UMX.tag database
            ExecutionQueryCatalogName = producerCatalog
        }
        let tests =
            DirectRun.create {
                ProjectName = UMX.tag sourceProject; DatabaseName = UMX.tag database; RunName = UMX.tag "NewRun"
                Description = "Tests saved under a user-selected name"; AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sortableTests; QueryCatalogName = producerCatalog
                    ParametersPipelineName = RunParametersPipelineNames.sortableTests; ParameterSpans = inputs
                }
            } |> run.DirectRun
        let evaluation =
            SimpleRun.create {
                ProjectName = UMX.tag "SorterEval"; DatabaseName = UMX.tag database; RunName = UMX.tag "NewRun"
                Description = "Evaluation of the saved test library"; AllowOverwrite = false
                Setup = {
                    FunctionName = setupCatalog; QueryCatalogName = querySetup; ParametersPipelineName = setupParametersPipelineName
                    ParameterSpans = inputs @ [ParamKeys.sortableTestsModuleKey, [sourceModule]; ParamKeys.rngTypeKey, ["LCG"]; ParamKeys.sorterCountKey, ["2"];
                                              ParamKeys.simpleSorterModelTypeKey, ["Msce"]]
                }
                Execution = {
                    FunctionName = ExecuteFunctionNames.sorterEval; QueryCatalogName = queryRun
                    ParametersPipelineName = runParametersPipelineName; ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V2"]]
                }
            } |> run.PreparedRun
        let! rows = publish folder evaluation
        let row = Assert.Single rows
        let messages = ResizeArray<string>()
        let sink = { new IProgress<runPhaseProgress> with member _.Report progress = messages.Add progress.Message }
        let! missing = RunSetupFiles.runAsync folder evaluation rows sink
        Assert.Equal(0, missing.Succeeded)
        Assert.Equal(1, missing.Failed)
        let error = Assert.Single missing.Errors
        Assert.Contains(library, error)
        Assert.Contains(SortableDataFormat.toString format, error)
        Assert.Contains(sourceModule, error)
        Assert.Contains("then rerun Setup", error)
        Assert.DoesNotContain(messages, fun message -> message.Contains("Setup complete"))
        Assert.Empty(Directory.GetDirectories(folder, "SortableTestsWithPrefix", SearchOption.AllDirectories))

        let! testRows = publish folder tests
        let! generated = RunExecutionFiles.runSavedAsync folder tests testRows null
        Assert.Empty(generated.Errors)
        Assert.Equal(1, generated.Succeeded)
        let query = QueryParamsCatalogRun.get producerCatalog tests.ProjectName tests.DatabaseName testRows[0] (geneSortDataType.SortableTests "") |> Option.get
        let sourceRoot = DatabasePaths.dataRoot folder sourceProject database |> UMX.tag
        let sourceFile = OutputDataFile.getFullOutputDataFilePath sourceRoot geneSortDataMode.Results query |> UMX.untag
        Assert.True(File.Exists sourceFile)
        Assert.Contains(sourceFile, error)
        let sourceTime = File.GetLastWriteTimeUtc sourceFile
        let sources = WorkflowSourceFiles.create folder
        let load replica format =
            let parameters = testRows[0].WithRepl(Some replica).WithSortableDataFormat(Some format)
            sources.LoadModuleOutput sourceModule (setupParameters.create parameters.ParamMap) (geneSortDataType.SortableTests "")
        let! unknown = sources.LoadModuleOutput "Unknown.Producer" (setupParameters.create testRows[0].ParamMap) (geneSortDataType.SortableTests "") |> Async.StartAsTask
        Assert.True(Result.isError unknown)
        let! loaded = load 0<replNumber> format |> Async.StartAsTask
        get loaded |> ignore
        let! wrongReplica = load 1<replNumber> format |> Async.StartAsTask
        Assert.True(Result.isError wrongReplica)
        let! wrongFormat = load 0<replNumber> sortableDataFormat.BoolArray |> Async.StartAsTask
        Assert.True(Result.isError wrongFormat)

        let! setup = RunSetupFiles.runAsync folder evaluation rows null
        Assert.Empty(setup.Errors)
        let! rows = RunParameterFiles.loadAllAsync folder evaluation
        Assert.Equal(1, setup.Succeeded)
        let setupFiles = Directory.GetFiles(Path.Combine(folder, "SorterEval", database, "Data", "Setup"), "*.msgpack", SearchOption.AllDirectories) |> Array.sort
        Assert.Equal(2, setupFiles.Length)
        Assert.Contains(setupFiles, fun file -> Path.GetFileNameWithoutExtension file = string (row.GetSetupSortableTestsWithPrefixId().Value))
        let setupTimes = setupFiles |> Array.map File.GetLastWriteTimeUtc
        let! again = RunSetupFiles.runAsync folder evaluation rows null
        Assert.Empty(again.Errors)
        Assert.Equal<DateTime>(setupTimes, setupFiles |> Array.map File.GetLastWriteTimeUtc)
        let! executed = RunExecutionFiles.runSavedAsync folder evaluation rows null
        Assert.Empty(executed.Errors)
        Assert.Equal(1, executed.Succeeded)
        Assert.Equal(sourceTime, File.GetLastWriteTimeUtc sourceFile)
        let! saved = RunParameterFiles.loadAllAsync folder evaluation
        Assert.Equal(row.GetId(), saved[0].GetId())
        Assert.Equal(row.GetSetupSortableTestsWithPrefixId(), saved[0].GetSetupSortableTestsWithPrefixId())
        Assert.Equal(Some true, saved[0].GetRunFinished())
    })

    [<Fact>]
    let ``execution uses saved state persists completion and continues after individual failures`` () = withFolder (fun folder -> task {
        let name = "execution-selection-" + Guid.NewGuid().ToString("N")
        let calls = ResizeArray<int>()
        ExecuteFunctionCatalog.register name (SimpleExecution (fun _ row overwrite _ progress -> async {
            Assert.True(UMX.untag overwrite)
            let mutation = int (row.GetMutationMod().Value)
            calls.Add mutation
            if mutation = 1 then return Error "Missing prepared input"
            else
                Assert.Equal(Some 2<generationNumber>, row.GetGenerationCurrent())
                progress |> Option.iter (fun sink -> sink.Report "Saved generation 3")
                return Ok (row.WithGenerationCurrent(Some 3<generationNumber>).WithRunFinished(Some true))
        }))
        // No setup registration for this function: Execution must never resolve Setup.
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        let configured = { configured with ExecuteFunctionName = name; AllowOverwrite = true } |> RunDto.toDomain
        let! rows = publish folder configured
        let rows = rows |> Array.sortBy (fun row -> row.GetMutationMod())
        let resumed = rows[0].WithGenerationCurrent(Some 2<generationNumber>)
        let finished = rows[2].WithRunFinished(Some true)
        let! savedResume = save folder configured resumed
        let! savedFinished = save folder configured finished
        get savedResume
        get savedFinished
        let updates = ResizeArray<runPhaseProgress>()
        let progress = { new IProgress<runPhaseProgress> with member _.Report update = updates.Add update }
        let! result = RunExecutionFiles.runSavedAsync folder configured [|rows[1]; finished; resumed|] progress
        Assert.Equal(1, result.Succeeded)
        Assert.Equal(1, result.Skipped)
        Assert.Equal(1, result.Failed)
        Assert.Contains("Missing prepared input", result.Errors[0])
        Assert.Equal<int>([|1; 0|], calls.ToArray())
        Assert.Equal(0, updates[0].Completed)
        Assert.Equal(3, updates[updates.Count - 1].Completed)
        Assert.All(updates, fun update -> Assert.Equal(3, update.Total))
        Assert.Contains(updates, fun update -> update.Message.Contains("Saved generation 3"))
        let! after = RunParameterFiles.loadAllAsync folder configured
        let after = after |> Array.sortBy (fun row -> row.GetMutationMod())
        Assert.Equal(Some true, after[0].GetRunFinished())
        Assert.Equal(Some 3<generationNumber>, after[0].GetGenerationCurrent())
        Assert.Equal(resumed.GetId(), after[0].GetId())
        Assert.Equal(rows[1], after[1])
        Assert.Equal(finished, after[2])
        Assert.Equal(rows[3], after[3])
        Assert.Empty(Directory.GetDirectories(folder, "Setup", SearchOption.AllDirectories))
    })

    [<Fact>]
    let ``SGD execution binds the saved run intervals and mutator without source makers`` () = withFolder (fun folder -> task {
        let name = "sgd-execution-selection-" + Guid.NewGuid().ToString("N")
        let mutable bound = false
        ExecuteFunctionCatalog.register name (SgdExecution (fun _ dependencies path state _ _ _ -> async {
            Assert.Equal(RunIntervals.resolve (UMX.tag "test") GenIntervalNames.uniformInterval2, dependencies.GenSaveIntervals)
            Assert.Equal(RunIntervals.resolve (UMX.tag "test") GenIntervalNames.uniformInterval10, dependencies.GenSaveSubIntervals)
            let! mutator = dependencies.MakeMutator path
            get mutator |> ignore
            bound <- true
            return Ok (state.WithFinished(Some true))
        }))
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        let configured = { configured with ExecuteFunctionName = name; GenSaveIntervalsName = GenIntervalNames.uniformInterval2
                                           GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
                                           SortableTestsMakerName = "not-required-for-execution"; SeedPoolSetMakerName = "not-required-for-execution" } |> RunDto.toDomain
        let! rows = publish folder configured
        let! result = RunExecutionFiles.runSavedAsync folder configured [|rows[0]|] null
        Assert.Empty(result.Errors)
        Assert.Equal(1, result.Succeeded)
        Assert.True(bound)
        Assert.Empty(Directory.GetDirectories(folder, "Setup", SearchOption.AllDirectories))
    })

    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``summary reports include finished selections preserve parameters and respect overwrite`` overwrite = withFolder (fun folder -> task {
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        // Reporting needs only saved summaries and intervals, not setup sources or an executor.
        let configured = { configured with AllowOverwrite = overwrite; GenSaveIntervalsName = GenIntervalNames.uniformInterval2
                                           SetupParameterSpans = configured.SetupParameterSpans |> Array.map (fun span ->
                                               if span.Key = ParamKeys.generationIntervalLastKey then { span with Values = [|"1"|] } else span)
                                           ExecuteFunctionName = "not-required-for-reporting"
                                           SortableTestsMakerName = "not-required-for-reporting"; SeedPoolSetMakerName = "not-required-for-reporting" } |> RunDto.toDomain
        let! generated = publish folder configured
        let rows = generated |> Array.sortBy (fun row -> row.GetMutationMod())
                            |> Array.map (fun row -> row.WithGenerationCurrent(Some 99<generationNumber>))
        for index in 0 .. 2 do rows[index] <- rows[index].WithRunFinished(Some true)
        for row in rows do
            let! result = save folder configured row
            get result
        let database = db folder configured
        let intervals = RunIntervals.forRun configured |> Option.get
        let generations = [|0; 1|] |> Array.map (GenIntervalConfig.getSampleAtIndex intervals >> Option.get >> UMX.tag<generationNumber>)
        let root = DatabasePaths.dataRoot folder (string configured.ProjectName) (string configured.DatabaseName) |> UMX.tag
        let summaryType = geneSortDataType.SorterPoolSetSummarySet ""
        let reportType = geneSortDataType.TextReport (UMX.tag "SummaryReport")
        let outputQuery (row: runParameters) generation kind =
            database.makeQueryParamsFromRunParams (row.WithGenerationCurrent(Some generation)) kind |> Option.get
        let reportPath row = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results (outputQuery row generations[1] reportType) |> string
        // Three rows have two slices each. Only two of those rows will be selected.
        for row in rows[1..3] do
            for generation in generations do
                let summary = spSummary_Standard.create (UMX.tag (Guid.NewGuid())) (UMX.tag "pool")
                                  100<ceLength> 10<ceLength> 11.0<ceLength> 1.0<ceLength> 2<stageLength>
                                  3.0<stageLength> 1.0<stageLength> (UMX.tag 1.0) 0.0<reflectiveCount> 0.0
                let poolSet = sorterPoolSetSummary_Standard.Create(UMX.tag (Guid.NewGuid()), generation, 100.0, [|summary|])
                let slice = spSummarySet_Standard.create (UMX.tag (Guid.NewGuid())) generation [|poolSet|] |> spSummarySet.Standard
                let! saved = database.saveAsync geneSortDataMode.Results (outputQuery row generation summaryType)
                                (outputData.SorterPoolSetSummarySet slice) (UMX.tag false) |> Async.StartAsTask
                get saved
        let parameterFiles = Directory.GetFiles(RunParameterFiles.folderFor folder configured, "*.msgpack", SearchOption.AllDirectories)
        let before = parameterFiles |> Array.map (fun path -> File.ReadAllBytes path, File.GetLastWriteTimeUtc path)
        let progress = ResizeArray<runPhaseProgress>()
        let sink = { new IProgress<runPhaseProgress> with member _.Report update = progress.Add update }
        // A missing result fails first; both the completed and unfinished rows must still be reported.
        let! result = RunSummaryFiles.runAsync folder configured rows[..2] sink
        Assert.Equal(2, result.Succeeded)
        Assert.Equal(1, result.Failed)
        Assert.Equal(0, result.Skipped)
        Assert.Contains("No valid slice files", result.Errors[0])
        Assert.Equal(0, progress[0].Completed)
        Assert.Equal(3, progress[progress.Count - 1].Completed)
        Assert.All(progress, fun update -> Assert.Equal(3, update.Total))
        Assert.Contains(progress, fun update -> update.Message.Contains("successfully completed"))
        Assert.Equal<int>(progress |> Seq.map (fun update -> update.Completed) |> Seq.sort, progress |> Seq.map (fun update -> update.Completed))
        for row in rows[1..2] do
            let lines = File.ReadAllLines(reportPath row)
            Assert.Equal(3, lines.Length) // One header and two saved generations.
            let headers = lines[0].Split '\t'
            let runColumn = headers |> Array.findIndex ((=) "RunName")
            let mutationColumn = headers |> Array.findIndex ((=) ParamKeys.mutationModKey)
            Assert.All(lines[1..], fun line ->
                let values = line.Split '\t'
                Assert.Equal(string configured.RunName, values[runColumn])
                Assert.Equal(string (row.GetMutationMod().Value), values[mutationColumn]))
        Assert.False(File.Exists(reportPath rows[0]))
        Assert.False(File.Exists(reportPath rows[3]))
        let! displayed = RunSummaryFiles.loadAsync folder configured rows[..2] CancellationToken.None
        Assert.Equal(2, displayed.Reports.Length)
        Assert.Single(displayed.Errors) |> ignore
        Assert.All(displayed.Reports, fun report ->
            Assert.Equal(int generations[1], report.Generation)
            Assert.Equal(2, report.Rows.Length)
            Assert.Contains("RunName", report.Headers))
        let reportBefore = rows[1..2] |> Array.map (reportPath >> File.ReadAllBytes)
        let! repeated = RunSummaryFiles.runAsync folder configured rows[1..2] null
        Assert.Equal((if overwrite then 2 else 0), repeated.Succeeded)
        Assert.Equal((if overwrite then 0 else 2), repeated.Failed)
        if not overwrite then
            Assert.All(repeated.Errors, fun error -> Assert.Contains("Overwrite not allowed", error))
            Array.iter2 (fun (expected: byte[]) row -> Assert.Equal<byte>(expected, File.ReadAllBytes(reportPath row))) reportBefore rows[1..2]
        // Reports return their last discovered generation; they must not write it back to execution state.
        Array.iter2 (fun path ((bytes: byte[]), modified) ->
            Assert.Equal<byte>(bytes, File.ReadAllBytes path)
            Assert.Equal(modified, File.GetLastWriteTimeUtc path)) parameterFiles before
        Assert.Empty(Directory.GetDirectories(folder, "Setup", SearchOption.AllDirectories))
    })

    [<Fact>]
    let ``parallel setup bounds concurrency and persists completion`` () = withFolder (fun folder -> task {
        let sync = obj ()
        let mutable active, peak = 0, 0
        let started = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
        let configured = custom (fun row _ _ _ -> async {
            lock sync (fun () ->
                active <- active + 1
                peak <- max peak active
                if active = 2 then started.TrySetResult(()) |> ignore)
            try
                do! started.Task.WaitAsync(TimeSpan.FromSeconds 10.) |> Async.AwaitTask
                do! Async.Sleep 20
                return Ok row
            finally lock sync (fun () -> active <- active - 1)
        })
        let! rows = publish folder configured
        let progress = ResizeArray<runPhaseProgress>()
        let sink = { new IProgress<runPhaseProgress> with member _.Report update = progress.Add update }
        let! result = RunSetupFiles.runParallelAsync 2 folder configured rows sink
        Assert.Equal(2, peak)
        Assert.Equal(rows.Length, result.Succeeded)
        Assert.Empty(result.Errors)
        Assert.Equal(rows.Length, result.Updates.Length)
        Assert.Equal<int>(progress |> Seq.map (fun p -> p.Completed) |> Seq.sort, progress |> Seq.map (fun p -> p.Completed))
        for index, output in result.Updates do
            Assert.Equal(rows[index].GetId(), (RunPhaseOutput.toParameters output).GetId())
        let! saved = RunParameterFiles.loadAllAsync folder configured
        Assert.All(saved, fun row -> Assert.Equal(Some true, row.GetSetupCompleted()))
    })

    [<Theory>]
    [<InlineData(false, false)>]
    [<InlineData(false, true)>]
    [<InlineData(true, false)>]
    [<InlineData(true, true)>]
    let ``cancelled evolution restores its saved generation and can resume`` soss checkpoint = withFolder (fun folder -> task {
        let configured = custom (fun row _ _ _ -> async { return Ok row })
        use cancellation = new CancellationTokenSource()
        let mutable calls = 0
        let mutable resumed = false
        let mutable savedGeneration = 0<generationNumber>
        ExecuteFunctionCatalog.register configured.ExecuteFunctionName (SgdExecution (fun storage _ path state _ _ sink -> asyncResult {
            calls <- calls + 1
            if resumed then
                Assert.Equal(Some savedGeneration, state.CurrentGeneration)
                return state.WithFinished(Some true)
            else
                savedGeneration <- path.Start.FirstGeneration
                if checkpoint then
                    let generation = [0 .. 10] |> List.choose (GenIntervalConfig.getSampleAtIndex path.GenSaveIntervals)
                                     |> List.find (fun gen -> gen > int path.Start.FirstGeneration) |> UMX.tag<generationNumber>
                    let pool = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) generation (Dim1 1) None
                    let data = if soss then GeneSort.Eval.V1.Sgd.Soss.SorterPoolSet_Soss.fromStandard pool |> sorterPoolSet.Soss else sorterPoolSet.Standard pool
                    let query = EvolutionFiles.queryAtGeneration storage.RunDb path state generation (geneSortDataType.SorterPoolSet "") |> Option.get
                    do! storage.RunDb.saveAsync geneSortDataMode.Results query (outputData.SorterPoolSet data) (UMX.tag true)
                    savedGeneration <- generation
                sink |> Option.iter (fun progress -> progress.Report(sprintf "Starting evolution step. Generation %d of 999999" (int savedGeneration + 1)))
                do! checkCancellation cancellation.Token
                return state.WithFinished(Some true)
        }))
        let! rows = publish folder configured
        let paths = rows |> Array.take 3 |> Array.map (EvoPath.fromParameters configured)
        let progress = ResizeArray<runPhaseProgress>()
        let sink = { new IProgress<runPhaseProgress> with
                        member _.Report update =
                            progress.Add update
                            if update.Generation.IsSome then cancellation.Cancel() }
        let! result = RunExecutionFiles.runPathsCancellableAsync cancellation.Token 1 folder configured paths sink
        Assert.Equal(1, calls)
        Assert.Equal(3, result.Cancelled)
        Assert.All(result.RowStatuses, fun status -> Assert.Equal(runRowStatus.Cancelled, status))
        Assert.Equal(0, result.Failed)
        Assert.Equal(0, result.Succeeded)
        let index, update = Assert.Single(result.Updates)
        Assert.Equal(0, index)
        let restoredPath, restoredState = Assert.Single(result.EvolutionPaths)
        Assert.Same(fst paths[0], restoredPath)
        Assert.Equal(Some savedGeneration, restoredState.CurrentGeneration)
        Assert.Equal(Some false, restoredState.Finished)
        Assert.Contains(progress, fun p -> p.RowIndex = Some 0 && p.Generation = Some (int savedGeneration + 1))
        let! saved = RunParameterFiles.loadAllAsync folder configured
        let restored = saved |> Array.find (fun row -> row.GetId() = Some (fst paths[0]).Id)
        Assert.Equal(Some savedGeneration, restored.GetGenerationCurrent())
        for row in rows |> Array.skip 1 do
            Assert.Equal(row, saved |> Array.find (fun candidate -> candidate.GetId() = row.GetId()))
        resumed <- true
        let! finished = RunExecutionFiles.runPathsAsync folder configured [|restoredPath, restoredState|] null
        Assert.Equal(1, finished.Succeeded)
        Assert.Empty(finished.Errors)
    })

    [<Fact>]
    let ``parallel cancellation stops queued rows and accounts for active rows`` () = withFolder (fun folder -> task {
        use cancellation = new CancellationTokenSource()
        let mutable calls = 0
        let bothStarted = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
        let configured = custom (fun row _ _ _ -> async {
            if Interlocked.Increment(&calls) = 2 then bothStarted.TrySetResult(()) |> ignore
            do! bothStarted.Task.WaitAsync(TimeSpan.FromSeconds 10.) |> Async.AwaitTask
            cancellation.Cancel()
            return Error "Operation cancelled"
        })
        let! rows = publish folder configured
        let! result = RunSetupFiles.runCancellableAsync cancellation.Token 2 folder configured rows null
        Assert.Equal(2, calls)
        Assert.Equal(rows.Length, result.Cancelled)
        Assert.All(result.RowStatuses, fun status -> Assert.Equal(runRowStatus.Cancelled, status))
        Assert.Empty(result.Errors)
        Assert.Empty(result.Updates)
        let! saved = RunParameterFiles.loadAllAsync folder configured
        Assert.Equal<runParameters>(rows |> Array.sortBy (fun r -> r.GetId()), saved |> Array.sortBy (fun r -> r.GetId()))
    })

    [<Fact>]
    let ``successful execution attempt stays pending until its saved completion flag is true`` () = withFolder (fun folder -> task {
        let name = "execution-pending-" + Guid.NewGuid().ToString("N")
        ExecuteFunctionCatalog.register name (SimpleExecution (fun _ row _ _ _ -> async.Return (Ok row)))
        let dto = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        let configured = RunDto.toDomain { dto with ExecuteFunctionName = name }
        let! rows = publish folder configured
        let! result = RunExecutionFiles.runSavedAsync folder configured [|rows[0]|] null
        Assert.Empty(result.Errors)
        Assert.Equal(1, result.Succeeded)
        Assert.Equal(runRowStatus.Waiting, Assert.Single(result.RowStatuses))
        let _, output = Assert.Single(result.Updates)
        Assert.NotEqual(Some true, (RunPhaseOutput.toParameters output).GetRunFinished())
    })

    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``summary generation rejects unfinished and mixed selections before writing reports`` mixed = withFolder (fun folder -> task {
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors)
        let! rows = publish folder configured
        let selected = if mixed then [| rows[0].WithRunFinished(Some true); rows[1] |] else [| rows[0] |]
        if mixed then
            let! saved = save folder configured selected[0]
            get saved
        let before = Directory.GetFiles(folder, "*", SearchOption.AllDirectories) |> Array.sort
        let! error = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunSummaryFiles.runAsync folder configured selected null :> Task)
        Assert.Contains("Select only completed runs", error.Message)
        Assert.Equal<string>(before, Directory.GetFiles(folder, "*", SearchOption.AllDirectories) |> Array.sort)
    })
