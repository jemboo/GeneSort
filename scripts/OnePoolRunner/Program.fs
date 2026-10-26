module OnePoolRunner

open System
open System.Reflection
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Reports
open GeneSort.Dispatch.V1

let identity (configured: run) =
    %configured.ProjectName, %configured.DatabaseName, %configured.RunName

let orderedRuns moduleName =
    let presets = typeof<runHost>.Assembly.GetType(moduleName, true)
    let runs =
        typeof<runHost>.Assembly.GetTypes()
        |> Array.filter (fun candidate -> candidate = presets || candidate.FullName.StartsWith(presets.FullName + "+" , StringComparison.Ordinal))
        |> Array.collect (fun candidate -> candidate.GetMethods(BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly))
        |> Array.filter (fun method -> method.ReturnType = typeof<run> && method.GetParameters().Length = 0)
        |> Array.map (fun method -> method.Invoke(null, [||]) :?> run)
        |> Array.sortBy (fun configured -> %configured.RunName)
        |> Array.toList
    if runs.IsEmpty then failwith "No OnePool run configurations were found."
    let rec order completed (remaining: run list) =
        match remaining with
        | [] -> []
        | _ ->
            let ready, waiting = remaining |> List.partition (fun configured ->
                match configured.ParameterSource with
                | None -> true
                | Some (source, _) -> Set.contains (identity source) completed)
            if ready.IsEmpty then
                failwith "OnePool contains a source outside this module or a circular dependency."
            ready @ order (ready |> List.fold (fun seen configured -> Set.add (identity configured) seen) completed) waiting
    order Set.empty runs

let checkBatch phase result =
    match result with
    | Error error -> failwithf "%s: %s" phase error
    | Ok results ->
        let summary = RunResult.analyze results
        printfn "%s: %d succeeded, %d skipped, %d failed or cancelled."
            phase summary.Successes summary.MissingLog.Length summary.IssueLog.Length
        if summary.IssueLog.Length > 0 then
            failwith (String.concat Environment.NewLine summary.IssueLog)

let execute maxParallel includeSummary configured =
    let host = runHost.Create configured maxParallel
    use cts = new CancellationTokenSource()
    let progress = Some { new IProgress<string> with member _.Report message = printfn "%s" message }
    let maxReplica = if %configured.DatabaseName = "Controlled" then SorterSgd.RunCfg.p32.Test.OnePool.Controlled.replicaCount else 1
    let minReplica, maxReplica = 0<replNumber>, UMX.tag<replNumber> maxReplica
    printfn "\n=== %s / %s / %s ===" %configured.ProjectName %configured.DatabaseName %configured.RunName
    printfn "Setup"
    // Keep persisted completion/checkpoint state when resuming an interrupted batch.
    let newParameters =
        host.MakeRunParameters minReplica maxReplica
        |> Array.filter (fun parameters ->
            let query =
                host.QueryParamsFromRunParams parameters (geneSortDataType.RunParameters configured.RunName)
                |> Option.defaultWith (fun () -> failwith "Could not build the run-parameter query.")
            host.RunDb.doesOutPutDataExist geneSortDataMode.Project query
            |> Async.RunSynchronously |> not)
    ParamOps.initRunAndParamFiles host.RunDb progress host.Run minReplica maxReplica
        host.AllowOverwrite (fun _ _ -> newParameters)
    |> Async.RunSynchronously
    |> Result.defaultWith (fun error -> failwithf "Initialize parameters: %s" error)
    ProjectOps.executeSetupRuns minReplica maxReplica host.AllowOverwrite cts progress host maxParallel
    |> Async.RunSynchronously |> checkBatch "Setup"
    printfn "Execute"
    ProjectOps.executeRuns minReplica maxReplica host.AllowOverwrite cts progress host maxParallel
    |> Async.RunSynchronously |> checkBatch "Execute"
    if includeSummary then
        printfn "Summary report"
        // Reports are derived outputs and can safely be regenerated on a retry.
        ReportOps.executeReport ReportCatalog.evolutionSummary minReplica maxReplica (true |> UMX.tag) cts progress host maxParallel
        |> Async.RunSynchronously |> checkBatch "Summary report"

[<EntryPoint>]
let main argv =
    try
        let controlledOnly = Array.contains "--controlled" argv
        let msuf4r = Array.contains "--msuf4r" argv
        let msuf4s = Array.contains "--msuf4s" argv
        if [controlledOnly; msuf4r; msuf4s] |> List.filter id |> List.length > 1 then
            invalidArg "argv" "Choose only one of --controlled, --msuf4r, or --msuf4s."
        let argv = argv |> Array.filter (fun arg -> arg <> "--controlled" && arg <> "--msuf4r" && arg <> "--msuf4s")
        let listOnly, maxParallel =
            match argv with
            | [| parallelism |] -> false, int parallelism
            | [| "--list"; parallelism |] -> true, int parallelism
            | _ -> failwith "Use Run-OnePool.ps1 [-MaxParallel N] [-ListOnly] [-NoBuild]."
        if maxParallel < 1 then invalidArg "MaxParallel" "Must be positive."
        MessagePackSetup.configure ()
        ProjectCatalogs.registerAll ()
        let moduleName =
            if msuf4r || msuf4s then "GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a." + (if msuf4r then "SimpleR" else "SimpleS")
            else "GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Test+OnePool"
        let runs = orderedRuns moduleName |> List.filter (fun run -> not controlledOnly || %run.DatabaseName = "Controlled")
        let runs = if msuf4r || msuf4s then runs |> List.sortBy (fun run -> (if %run.RunName = "Test" then 0 else 1), %run.DatabaseName) else runs
        let prerequisites = [
            SortableTests.RunCfg.Prefix.Width32 ()
            SorterEval.RunCfg.Prefix.Width32 ()
        ]
        printfn "%s: %d configurations; projects root: %s" moduleName runs.Length (RunHost.projectsRoot ())
        for configured in prerequisites do
            if listOnly then
                printfn "Prerequisite %s / %s / %s: setup -> execute"
                    %configured.ProjectName %configured.DatabaseName %configured.RunName
            else execute maxParallel false configured
        for configured in runs do
            if listOnly then
                printfn "%s / %s / %s: setup -> execute -> summary"
                    %configured.ProjectName %configured.DatabaseName %configured.RunName
            else execute maxParallel true configured
        0
    with error ->
        eprintfn "Error: %s" (error.GetBaseException().Message)
        1
