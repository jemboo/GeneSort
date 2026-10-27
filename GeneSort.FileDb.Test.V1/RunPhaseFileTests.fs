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
        new GeneSortDbMp(Path.Combine(folder, string configured.ProjectName, string configured.DatabaseName, "Data") |> UMX.tag, configured.RunQueryCatalogName) :> IGeneSortDb
    let private save folder (configured: run) row =
        let query = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName row (geneSortDataType.RunParameters configured.RunName) |> Option.get
        (db folder configured).saveAsync geneSortDataMode.Project query (outputData.RunParameters row) (UMX.tag true) |> Async.StartAsTask
    let private custom setup =
        let name = "setup-selection-" + Guid.NewGuid().ToString("N")
        SetupExecutorCatalog.register name (fun _ _ _ -> setup)
        // There deliberately is no ExecuteFunction registration: setup must never resolve or call it.
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        { configured with ExecuteFunctionName = name } |> RunDto.toDomain

    let private phaseRunner = function
        | "Setup" -> RunSetupFiles.runAsync
        | "Execution" -> RunExecutionFiles.runAsync
        | "Summary Reports" -> RunSummaryFiles.runAsync
        | name -> invalidArg "name" name

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
            let root = Path.Combine(folder, string configured.ProjectName, string configured.DatabaseName, "Data") |> UMX.tag
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
        let otherPhase = if phase = "Execution" then RunSetupFiles.runAsync else RunExecutionFiles.runAsync
        let! rows = publish folder configured
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
            ProjectName = UMX.tag "SorterEval.RunCfg"; DatabaseName = UMX.tag "Standard"; RunName = UMX.tag "Test"
            Description = "Small saved source library"; AllowOverwrite = false
            Setup = {
                FunctionName = SetupCatalogNames.standard
                QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
                EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                ParameterSpans = [ParamKeys.sortingWidthKey, ["4"]; ParamKeys.rngTypeKey, ["LCG"]
                                  ParamKeys.simpleSorterModelTypeKey, ["Msce"; "Msrs"]; ParamKeys.sorterCountKey, ["2"]]
            }
            Execution = {
                FunctionName = ExecuteFunctionNames.sorterEval
                QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
                ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V2"]]
            }
        } |> run.SimpleRun

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
        let! again = RunSetupFiles.runAsync folder configured [|selected|] null
        Assert.Equal(1, again.Succeeded)
        Assert.Equal<DateTime>(before, files |> Array.map File.GetLastWriteTimeUtc)
        // The actual executor consumes prepared inputs, persists completion, and leaves the other row alone.
        let! evaluated = RunExecutionFiles.runAsync folder configured [|selected|] null
        Assert.Empty(evaluated.Errors)
        Assert.Equal(1, evaluated.Succeeded)
        let! saved = RunParameterFiles.loadAllAsync folder configured
        let finished = saved |> Array.find (fun row -> row.GetId() = selected.GetId())
        let pending = saved |> Array.find (fun row -> row.GetId() <> selected.GetId())
        Assert.Equal(Some true, finished.GetRunFinished())
        Assert.NotEqual(Some true, pending.GetRunFinished())
        Assert.Equal(rows |> Array.find (fun row -> row.GetId() = pending.GetId()), pending)
        let! skipped = RunExecutionFiles.runAsync folder configured [|finished|] null
        Assert.Equal(1, skipped.Skipped)
        Assert.Equal(0, skipped.Succeeded)
        let! unprepared = RunExecutionFiles.runAsync folder configured [|pending|] null
        Assert.Equal(1, unprepared.Failed)
        Assert.Equal<DateTime>(before, files |> Array.map File.GetLastWriteTimeUtc)
        let setupFiles = Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories) |> Array.filter (fun path -> path.Contains(string Path.DirectorySeparatorChar + "Setup" + string Path.DirectorySeparatorChar)) |> Array.sort
        Assert.Equal<string>(files, setupFiles)
        let sources = WorkflowSourceFiles.create folder
        let! loaded = sources.Evolution.LoadStandardSorterEvals 4<sortingWidth> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        Assert.Equal(2, (get loaded).SorterEvals.Length)
        let! absent = (WorkflowSourceFiles.create (Path.Combine(folder, "other"))).Evolution.LoadStandardSorterEvals 4<sortingWidth> simpleSorterModelType.Msce sorterEvalType.V2 |> Async.StartAsTask
        Assert.True(Result.isError absent)
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
        let! result = RunExecutionFiles.runAsync folder configured [|rows[1]; finished; resumed|] progress
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
        ExecuteFunctionCatalog.register name (SgdExecution (fun _ dependencies row _ _ _ -> async {
            Assert.Equal(RunIntervals.resolve (UMX.tag "test") GenIntervalNames.uniformInterval2, dependencies.GenSaveIntervals)
            Assert.Equal(RunIntervals.resolve (UMX.tag "test") GenIntervalNames.uniformInterval10, dependencies.GenSaveSubIntervals)
            let! mutator = dependencies.MakeMutator row
            get mutator |> ignore
            bound <- true
            return Ok (row.WithRunFinished(Some true))
        }))
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
        let configured = { configured with ExecuteFunctionName = name; GenSaveIntervalsName = GenIntervalNames.uniformInterval2
                                           GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
                                           SortableTestsMakerName = "not-required-for-execution"; SeedPoolSetMakerName = "not-required-for-execution" } |> RunDto.toDomain
        let! rows = publish folder configured
        let! result = RunExecutionFiles.runAsync folder configured [|rows[0]|] null
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
        rows[1] <- rows[1].WithRunFinished(Some true)
        for row in rows do
            let! result = save folder configured row
            get result
        let database = db folder configured
        let intervals = RunIntervals.forRun configured |> Option.get
        let generations = [|0; 1|] |> Array.map (GenIntervalConfig.getSampleAtIndex intervals >> Option.get >> UMX.tag<generationNumber>)
        let root = Path.Combine(folder, string configured.ProjectName, string configured.DatabaseName, "Data") |> UMX.tag
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
