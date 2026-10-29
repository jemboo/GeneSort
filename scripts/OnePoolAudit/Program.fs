module OnePoolAudit

open System
open System.IO
open System.Reflection
open FSharp.UMX
open OnePoolAuditVerification
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd

let isControlled (configured: run) =
    [ GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.Standard.runType.Name
      GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.Soss.runType.Name ]
    |> List.contains (string configured.ProjectName)

let unwrap computation = computation |> Async.RunSynchronously |> Result.defaultWith failwith
let require condition message = if not condition then failwith message
let mutable controlledOnly = false
let mutable msuf4Family: string option = None
let replicaLimit (configured: run) =
    (if isControlled configured then GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.replicaCount else 1) |> UMX.tag
let parameters (host: runHost) =
    host.RunDb.getRunParameters host.Run.RunName (Some (UMX.tag 0)) (Some (replicaLimit host.Run)) None None |> unwrap
let query (host: runHost) rp kind = host.QueryParamsFromRunParams rp kind |> Option.get
let checkpoint (host: runHost) rp = host.RunDb.loadAsync geneSortDataMode.Results (query host rp (geneSortDataType.SorterPoolSet "")) |> unwrap
let members soss data =
    if soss then
        (data |> OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith).SorterPools
        |> Map.values |> Seq.map (fun pool -> pool.SorterPoolMembers |> Seq.map (fun m -> string m.SorterPoolMemberId) |> Set.ofSeq) |> Seq.toArray
    else
        (data |> OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith).SorterPools
        |> Map.values |> Seq.map (fun pool -> pool.SorterPoolMembers |> Seq.map (fun m -> string m.SorterPoolMemberId) |> Set.ofSeq) |> Seq.toArray

let models soss data =
    if soss then
        (data |> OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith).SorterPools
        |> Map.values |> Seq.collect (fun pool -> pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel))
    else
        (data |> OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith).SorterPools
        |> Map.values |> Seq.collect (fun pool -> pool.SorterPoolMembers |> Seq.map (fun m -> m.SorterModel))

let report (host: runHost) (rp: runParameters) =
    let qp = query host rp (geneSortDataType.TextReport (UMX.tag "SummaryReport"))
    let path = OutputDataFile.getFullOutputDataFilePath (RunHost.makeDataFolder host.Run) geneSortDataMode.Results qp |> UMX.untag
    let lines = File.ReadAllLines path
    require (lines.Length > 1) ("Empty report: " + path)
    let headers = lines.[0].Split '\t'
    let rows = lines.[1..] |> Array.filter (String.IsNullOrWhiteSpace >> not) |> Array.map (fun line -> Array.zip headers (line.Split '\t') |> Map.ofArray)
    require (headers |> Array.filter ((=) ParamKeys.runNameKey) |> Array.length = 1)
        ("Summary report must contain exactly one RunName column: " + path)
    require (rows |> Array.forall (fun row -> row.[ParamKeys.runNameKey] = %host.Run.RunName))
        ("Summary report contains an incorrect RunName: " + path)
    verifyMutations path (string (int (rp.GetMutationMod().Value)))
        (rp.GetSourceMutationMod() |> Option.map (int >> string)) rows
    verifyActivatorSettings path rp.ParamMap rows
    require (rows |> Array.forall (fun row -> Map.tryFind "SortableTestsSubsetId" row <> Some "Default"))
        ("Summary report contains a stale Default evaluation: " + path)
    printfn "REPORT %s mutation=%d size=%d rows=%d file=%s" %host.Run.RunName (int (rp.GetMutationMod().Value)) (int (rp.GetSorterCountPerPool().Value)) rows.Length path
    rows

let singleSpan key spans =
    match spans |> List.tryFind (fst >> (=) key) with
    | Some (_, [value]) -> value
    | _ -> failwithf "Expected exactly one configured value for %s." key

let generated (host: runHost) = host.MakeRunParameters (UMX.tag 0) (replicaLimit host.Run)

// Compare the full configured sweep, including multiplicity, before inspecting artifacts.
let verifySweep (host: runHost) (saved: runParameters[]) =
    let expected = generated host
    let signature (rp: runParameters) =
        rp.GetId(), rp.GetRepl(), rp.GetMutationMod(), rp.GetSourceMutationMod(), rp.GetSorterCountPerPool(),
        rp.GetSorterPoolCount(), Map.tryFind ParamKeys.sourceExecutionIdKey rp.ParamMap,
        rp.GetAMax(), rp.GetActivationProbability(), rp.GetActivatorMutationRate()
    require ((expected |> Array.map signature |> Array.sort) = (saved |> Array.map signature |> Array.sort))
        (sprintf "%s: saved executions do not match the configured parameter sweep." %host.Run.RunName)
    expected

let verifyPools soss (host: runHost) (expected: runParameters) (rp: runParameters) =
    require (rp.GetRunFinished() = Some true) (sprintf "%s: execution is unfinished." %host.Run.RunName)
    let size, count = int (expected.GetSorterCountPerPool().Value), int (expected.GetSorterPoolCount().Value)
    let savedPoolSet = checkpoint host rp
    verifyModelActivationParameters (expected.GetAMax()) (expected.GetActivationProbability()) (models soss savedPoolSet)
    if isControlled host.Run && expected.GetAMax().IsSome then
        let values = models soss savedPoolSet |> Seq.collect (GeneSort.Model.Sorting.V1.SorterModel.asSimpleSorterModelR >> fun model -> model.Activators) |> Seq.toArray
        let boundary = values |> Array.filter (fun value -> value = 0 || value = 1) |> Array.length
        let flipRate = expected.GetActivatorMutationRate().Value * 0.5 * float boundary / float values.Length
        printfn "R ENDPOINT FLIP PROBABILITY: %s replica=%d mutation=%d rate=%.8f boundary=%d total=%d" %host.Run.RunName (int (rp.GetRepl().Value)) (int (rp.GetMutationMod().Value)) flipRate boundary values.Length
    let prepared = Utils.loadSetupOutputData host.RunDb rp (geneSortDataType.SorterPoolSet "") |> unwrap
    verifyModelActivationParameters (expected.GetAMax()) (expected.GetActivationProbability()) (models soss prepared)
    let pools = savedPoolSet |> members soss
    require (pools.Length = count && pools |> Array.forall (fun pool -> pool.Count = size))
        (sprintf "%s: checkpoint differs from configured %d pools of %d sorters." %host.Run.RunName count size)
    let rows = report host rp
    require (rows.Length > 0) "Summary report has no data rows."
    for row in rows do
        require (int row.["SorterCountPerPool"] = size && int row.["SorterPoolCount"] = count)
            (sprintf "%s: summary differs from configured pool sizes." %host.Run.RunName)
    if isControlled host.Run || msuf4Family.IsSome then
        let intervals = match host with SgdRunHost sgd -> sgd.GenSaveIntervals | _ -> failwith "Expected an SGD host."
        let generations = GenIntervalConfig.getSampleAtIndex intervals (int (expected.GetGenerationIntervalLast().Value))
                          |> Option.defaultWith (fun () -> failwith "Invalid final generation interval.")
        let first = if soss then 1 else 0
        let expectedGenerations = set [first .. first + generations - 1]
        let bySubset = rows |> Array.groupBy (fun row -> Map.tryFind "SortableTestsSubsetId" row |> Option.defaultValue "")
        require (bySubset.Length = (if soss then 3 else 1)) "Controlled summary has incorrect evaluation subsets."
        for _, subset in bySubset do
            require (subset.Length = generations && (subset |> Array.map (fun row -> int row.["GenerationNumber"]) |> Set.ofArray) = expectedGenerations)
                "Controlled summary has missing or duplicate generations."
    rows

let audit soss configured =
    let host = runHost.Create configured 1
    let sourceRun, sourceSpans = configured.ParameterSource |> Option.get
    let source = runHost.Create sourceRun 1
    // ParameterSource keeps only the base configuration; the saved run retains its schedule.
    let savedSourceRun =
        source.RunDb.loadAsync geneSortDataMode.Project
            (queryParams.createForRun sourceRun.DatabaseName sourceRun.ProjectName sourceRun.RunName)
        |> unwrap |> OutputData.asRun |> Result.defaultWith failwith
    require (savedSourceRun.ProjectName = sourceRun.ProjectName && savedSourceRun.DatabaseName = sourceRun.DatabaseName
             && savedSourceRun.RunName = sourceRun.RunName && savedSourceRun.ExecuteFunctionName = sourceRun.ExecuteFunctionName)
        "Saved source run does not match the configured continuation source."
    let sourceSchedule =
        (runHost.Create savedSourceRun 1).GenSaveIntervals
        |> Option.defaultWith (fun () -> failwith "Continuation source has no generation save schedule.")

    let sourceInterval = singleSpan ParamKeys.sourceGenerationIntervalKey configured.RunParameterSpans |> int
    let sourceGeneration =
        GenIntervalConfig.getSampleAtIndex sourceSchedule sourceInterval
        |> Option.defaultWith (fun () -> failwith "Configured source interval is unavailable.")
    let filters = sourceSpans |> List.filter (fst >> (<>) ParamKeys.selectedSorterPoolCountKey)
                              |> List.map (fun (key, values) -> ParamKeys.sourceParameterKey key, values)
    let candidates = generated source |> Array.filter (fun rp ->
        filters |> List.forall (fun (key, values) ->
            Map.tryFind key rp.ParamMap |> Option.exists (fun value -> List.contains value values)))
    let selectionCount =
        if sourceSpans |> List.exists (fst >> (=) ParamKeys.selectedSorterPoolCountKey) then
            singleSpan ParamKeys.selectedSorterPoolCountKey sourceSpans |> int
        else candidates.Length
    let sourceSoss = sourceRun.ExecuteFunctionName = GeneSort.Project.V1.Catalogs.Evolution.ExecuteFunctionNames.soss
    require (selectionCount > 0 && candidates.Length >= selectionCount) "Insufficient configured continuation candidates."
    let savedSources = parameters source
    let scores = candidates |> Array.map (fun candidate ->
        let rp = savedSources |> Array.filter (fun saved -> saved.GetId() = candidate.GetId()) |> Array.exactlyOne
        let atSource = rp.WithGenerationCurrent(Some (UMX.tag sourceGeneration))
        let data = checkpoint source atSource
        let measure = rp.GetSorterEvalMeasure().Value
        let score, subset =
            if sourceSoss then
                let pool = (data |> OutputData.asSorterPoolSetSoss |> Result.defaultWith failwith).SorterPools |> Map.values |> Seq.exactlyOne
                float (SorterPool_Soss.getAverageScore measure pool), string (pool.SorterPoolMembers |> Seq.head).SubsetIdForSelection
            else
                let pool = (data |> OutputData.asSorterPoolSetStandard |> Result.defaultWith failwith).SorterPools |> Map.values |> Seq.exactlyOne
                float (SorterPool_Standard.getAverageScore measure pool), "full"
        require (Double.IsFinite score) "Source checkpoint has a nonfinite score."
        let id = string (rp.GetId().Value)
        printfn "SOURCE %s mutation=%d generation=%d score=%.12f selectionSubset=%s id=%s" %source.Run.RunName (int (rp.GetMutationMod().Value)) sourceGeneration score subset id
        score, rp.GetId().Value, rp, data)
    // Rank checkpoint contents independently of the continuation's parameter generator.
    let sourceGroups = scores |> Array.groupBy (fun (_, _, rp, _) -> RunParameterGeneration.sourceCombinationKey sourceRun rp)
    let expectedIds =
        sourceGroups |> Array.collect (fun (_, group) ->
            let count = if sourceSpans |> List.exists (fst >> (=) ParamKeys.selectedSorterPoolCountKey) then selectionCount else group.Length
            require (group.Length >= count) "Insufficient candidates within a source parameter combination."
            group |> Array.sortBy (fun (score, id, _, _) -> score, id) |> Array.take count)
        |> Array.map (fun (_, id, _, _) -> string id) |> Set.ofArray
    let selected = parameters host
    let actualIds = selected |> Array.map (fun rp -> rp.ParamMap.[ParamKeys.sourceExecutionIdKey]) |> Set.ofArray
    require (expectedIds = actualIds) (sprintf "Continuation did not select the best %d configured source pools." selectionCount)
    let selectedSources =
        expectedIds |> Set.toArray |> Array.map (fun sourceId ->
            let _, _, sourceRp, _ = scores |> Array.find (fun (_, id, _, _) -> string id = sourceId)
            sourceId, string (int (sourceRp.GetMutationMod().Value)))
    let actualExpansion = selected |> Array.map (fun rp ->
        rp.ParamMap.[ParamKeys.sourceExecutionIdKey],
        rp.GetSourceMutationMod() |> Option.map (int >> string) |> Option.defaultValue "",
        string (int (rp.GetMutationMod().Value)),
        configured.RunParameterSpans |> List.map (fun (key, _) -> key, rp.ParamMap.[key]) |> Map.ofList)
    let combinationCount, executionCount =
        verifyExpansion %configured.RunName configured.RunParameterSpans selectedSources actualExpansion
    printfn "EXPANSION %s: %d selected sources x %d execution combinations = %d executions" %configured.RunName expectedIds.Count combinationCount executionCount
    let expectedContinuation = verifySweep host selected
    for rp in selected do
        let sourceId = rp.ParamMap.[ParamKeys.sourceExecutionIdKey]
        let _, _, sourceRp, data = scores |> Array.find (fun (_, id, _, _) -> string id = sourceId)
        require (rp.GetSourceMutationMod() = sourceRp.GetMutationMod()) "Continuation records the wrong source mutation."
        require (rp.GetSourceGenerationInterval() = Some sourceInterval && rp.GetGenerationFirst() = Some (UMX.tag sourceGeneration))
            "Continuation imported the wrong source generation."
        let imported = Utils.loadSetupOutputData host.RunDb rp (geneSortDataType.SorterPoolSet "") |> unwrap
        require (members soss imported = members sourceSoss data) "Continuation Setup does not contain the selected source members."
        let expected = expectedContinuation |> Array.find (fun candidate -> candidate.GetId() = rp.GetId())
        for row in verifyPools soss host expected rp do
            require (row.["SourceExecutionId"] = sourceId && int row.["SourceGenerationInterval"] = sourceInterval)
                "Continuation summary provenance is incorrect."
            require (row.[ParamKeys.sourceMutationModKey] = string (int (sourceRp.GetMutationMod().Value)))
                "Continuation summary source mutation is incorrect."
        printfn "SELECTED %s sourceMutation=%d sourceId=%s setupMembersMatch=true" %host.Run.RunName (int (sourceRp.GetMutationMod().Value)) sourceId

let currentRuns () =
    let moduleName =
        match msuf4Family with
        | Some family -> "GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a." + family
        | None -> "GeneSort.Dispatch.V1.SorterSgd.p32.Test+OnePool"
    let presets = typeof<runHost>.Assembly.GetType(moduleName, true)
    let runs =
        typeof<runHost>.Assembly.GetTypes()
        |> Array.filter (fun candidate -> candidate = presets || candidate.FullName.StartsWith(presets.FullName + "+" , StringComparison.Ordinal))
        |> Array.collect (fun candidate -> candidate.GetMethods(BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly))
        |> Array.filter (fun method -> method.ReturnType = typeof<run> && method.GetParameters().Length = 0)
        |> Array.map (fun method -> method.Invoke(null, [||]) :?> run)
        |> Array.filter (fun configured -> not controlledOnly || isControlled configured)
        |> Array.sortBy (fun configured -> %configured.RunName)
    require (runs.Length > 0) "No OnePool configurations found."
    runs

let auditControlledSeeds () =
    let runs = currentRuns () |> Array.filter (fun configured -> isControlled configured)
    for soss in [false; true] do
        let family = runs |> Array.filter (fun configured -> (configured.ExecuteFunctionName = ExecuteFunctionNames.soss) = soss)
        if family.Length > 0 then
            let seeds = family |> Array.collect (fun configured ->
                let host = runHost.Create configured 1
                parameters host |> Array.map (fun rp ->
                    let data = Utils.loadSetupOutputData host.RunDb rp (geneSortDataType.SorterPoolSet "") |> unwrap
                    rp, models soss data |> Seq.toArray))
            for _, group in seeds |> Array.groupBy (fun (rp, _) -> rp.GetRepl(), rp.GetMutationMod()) do
                require (group.Length = 4) "Controlled comparison is missing a model family or flip-rate comparison."
                let find predicate = group |> Array.find (fst >> predicate) |> snd
                for rp, booleanModels in group do
                    if rp.GetActivationProbability().IsSome then
                        verifyControlledSeeds
                            (find (fun rp -> rp.GetAMax().IsNone && rp.GetActivationProbability().IsNone))
                            (find (fun rp -> rp.GetAMax().IsSome))
                            booleanModels
                let rp = group.[0] |> fst
                if rp.GetMutationMod() = Some (UMX.tag 0) then
                    let values = find (fun rp -> rp.GetAMax().IsSome)
                                 |> Array.collect (GeneSort.Model.Sorting.V1.SorterModel.asSimpleSorterModelR >> fun model -> model.Activators)
                    let active = values |> Array.filter (fun value -> value > 0) |> Array.length
                    printfn "PAIRED MASK: %s replica=%d active=%d total=%d percentage=%.3f" (if soss then "Soss" else "Standard") (int (rp.GetRepl().Value)) active values.Length (100.0 * float active / float values.Length)
            printfn "PAIRED: %s saved seed arrays and R/S activation masks match across all replicas and mutation variants." (if soss then "Soss" else "Standard")

let combineReports () =
    let runs = currentRuns ()
    let mutable reportCount = 0
    let rows = runs |> Array.collect (fun configured ->
        let host = runHost.Create configured 1
        let saved = parameters host
        verifySweep host saved |> ignore
        saved |> Array.sortBy (fun rp -> rp.GetId()) |> Array.collect (fun rp ->
            reportCount <- reportCount + 1
            report host rp |> Array.map (fun row ->
                if msuf4Family.IsSome then row |> Map.add "Workflow" (%configured.DatabaseName) else row)))
    // Standard and Soss reports have different columns; align their union by name.
    let headers =
        rows |> Seq.collect Map.keys |> Set.ofSeq |> Set.remove ParamKeys.runNameKey |> Set.toArray
        |> Array.append [|ParamKeys.runNameKey|]
    let combined = DataTableReport.create "OnePool Summary" headers
    for row in rows do
        combined.AppendDataRow(headers |> Array.map (fun header -> Map.tryFind header row |> Option.defaultValue ""))
    // Keep the aggregate alongside the Standard base run.
    let baseRun = GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Standard.TestStandard()
    let filename = if controlledOnly then "OnePool_Controlled_SummaryReport.txt" else "OnePool_SummaryReport.txt"
    let path =
        match msuf4Family with
        | Some family -> Path.Combine(RunHost.projectsRoot(), %runs.[0].ProjectName, "Reports", sprintf "Msuf4a_%s_SummaryReport.txt" family)
        | None -> Path.Combine(%(RunHost.makeDataFolder baseRun), "Results", "Report", filename)
    Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
    // Replace only after the complete combined report has been written successfully.
    let temporaryPath = path + ".tmp"
    try
        do
            use stream = File.Create temporaryPath
            combined.SaveToStream stream
        verifyCombinedRows headers rows (File.ReadAllLines temporaryPath)
        File.Move(temporaryPath, path, true)
    finally
        if File.Exists temporaryPath then File.Delete temporaryPath
    printfn "COMBINED: %d reports, %d rows, %d runs -> %s" reportCount rows.Length runs.Length path
    printfn "PASS: Combined summary preserves SourceMutationMod, MutationMod, and every configured execution's report rows."

[<EntryPoint>]
let main args =
    try
        require (args |> Array.forall (fun arg -> arg = "--combine" || arg = "--controlled" || arg = "--msuf4r" || arg = "--msuf4s")) "Usage: OnePoolAudit [--combine] [--controlled|--msuf4r|--msuf4s]"
        controlledOnly <- Array.contains "--controlled" args
        let families = ["--msuf4r", "SimpleR"; "--msuf4s", "SimpleS"] |> List.filter (fun (flag, _) -> Array.contains flag args)
        require (families.Length + (if controlledOnly then 1 else 0) <= 1) "Choose only one of --controlled, --msuf4r, or --msuf4s."
        msuf4Family <- families |> List.tryHead |> Option.map snd
        MessagePackSetup.configure ()
        for configured in currentRuns () do
            match configured.ParameterSource with
            | Some _ -> audit (configured.ExecuteFunctionName = ExecuteFunctionNames.soss) configured
            | None ->
                let host = runHost.Create configured 1
                let saved = parameters host
                let expected = verifySweep host saved
                for rp in saved do
                    let configuredParameters = expected |> Array.find (fun candidate -> candidate.GetId() = rp.GetId())
                    verifyPools (configured.ExecuteFunctionName = ExecuteFunctionNames.soss) host configuredParameters rp |> ignore
        auditControlledSeeds ()
        match msuf4Family with
        | Some family ->
            printfn "PASS: Msuf4a.%s parameter sweep, pool sizes, activators, generation coverage, final checkpoints, and summary metadata." family
        | None ->
            printfn "PASS: Append sizes, Condense ranking, imported members, SimpleR activators and settings, final checkpoints, and current summary reports."
        if Array.contains "--combine" args then combineReports ()
        0
    with error ->
        eprintfn "FAIL: %s" error.Message
        1




