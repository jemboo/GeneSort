namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution

/// The workflow of a single path. No parameter spans or alternative source runs.
type evoWorkflow = {
    ProjectName: string<projectName>
    DatabaseName: string<databaseName>
    RunName: string<runName>
    ExecuteFunctionName: string
    RunQueryCatalogName: string
    SetupQueryCatalogName: string
    MakerNames: evolutionMakerNames
    GenSaveIntervalsName: string
    GenSaveSubIntervalsName: string
}

/// Execution state, separate from the selected evolutionary specification.
/// Keep original strings internally so old parameter files round-trip exactly.
type evoProgress = private { values: Map<string, string> } with
    member this.CurrentGeneration = ParamKeys.tryGetInt ParamKeys.generationCurrentKey this.values |> Option.map UMX.tag<generationNumber>
    member this.Finished = ParamKeys.tryGetBool ParamKeys.runFinishedKey this.values
    member this.ElapsedTime = this.values.TryFind ParamKeys.elapsedTimeKey
    member this.Message = this.values.TryFind ParamKeys.messageKey
    member this.WithCurrentGeneration(value: int<generationNumber> option) =
        if value |> Option.exists (fun generation -> int generation < 0) then invalidArg "value" "Generation cannot be negative."
        { this with values = ParamKeys.addOrRemove ParamKeys.generationCurrentKey (value |> Option.map string) this.values }
    member this.WithFinished(value: bool option) =
        { this with values = ParamKeys.addOrRemove ParamKeys.runFinishedKey (value |> Option.map string) this.values }
    member this.WithElapsedTime(value) = { this with values = ParamKeys.addOrRemove ParamKeys.elapsedTimeKey value this.values }
    member this.WithMessage(value) = { this with values = ParamKeys.addOrRemove ParamKeys.messageKey value this.values }

module EvoProgress =
    let private keys = Set.ofList [ParamKeys.generationCurrentKey; ParamKeys.runFinishedKey; ParamKeys.elapsedTimeKey; ParamKeys.messageKey]
    let internal isProgress key = Set.contains key keys
    let empty = { values = Map.empty }
    let fromParameters (parameters: runParameters) =
        let values = parameters.ParamMap |> Map.filter (fun key _ -> isProgress key)
        if values.ContainsKey ParamKeys.generationCurrentKey && (parameters.GetGenerationCurrent() |> Option.forall (fun generation -> int generation < 0)) then
            invalidArg "parameters" "Invalid GenerationCurrent."
        if values.ContainsKey ParamKeys.runFinishedKey && parameters.GetRunFinished().IsNone then
            invalidArg "parameters" "Invalid RunFinished."
        { values = values }
    let internal apply (progress: evoProgress) settings =
        progress.values |> Map.fold (fun values key value -> Map.add key value values) settings

/// One selected specification with concrete Setup file bindings. Progress is never
/// included in Settings; GenerationFirst remains the immutable checkpoint origin.
type evoPath = private {
    id: Guid<queryParamsId>
    replica: int<replNumber>
    workflow: evoWorkflow
    settings: Map<string, string>
    start: evoStart
    intervals: genIntervalConfig
    subIntervals: genIntervalConfig
} with
    member this.Id = this.id
    member this.Replica = this.replica
    member this.Workflow = this.workflow
    member this.Settings = this.settings
    member this.Start = this.start
    member this.SetupInputs = Map.ofList [this.start.Tests.OutputDataType, this.start.Tests; this.start.Pool.OutputDataType, this.start.Pool]
    member this.GenSaveIntervals = this.intervals
    member this.GenSaveSubIntervals = this.subIntervals

/// A continuation becomes Ready only after Setup binds its validated checkpoint.
type evoPathPreparation =
    | Ready of path: evoPath * progress: evoProgress
    | AwaitingSetup of executionId: Guid<queryParamsId>

module EvoPath =
    let private require label value = value |> Option.defaultWith (fun () -> invalidArg "parameters" ("Missing " + label + "."))
    let private awaitingSetup (configured: run) (parameters: runParameters) =
        let bound output = SetupInputReferences.tryRecordedInput configured.ProjectName configured.DatabaseName parameters output |> Option.isSome
        configured.ParameterSource.IsSome &&
            (not (bound (geneSortDataType.SorterPoolSet "")) || not (bound (geneSortDataType.SortableTestsWithPrefix "")) || parameters.GetGenerationFirst().IsNone)

    /// Read the existing persisted representation without changing IDs, spelling,
    /// optional fields, or references. A concrete reference need not exist on disk yet.
    let fromParameters (configured: run) (parameters: runParameters) =
        let sgd = match configured with SgdRun sgd -> sgd | _ -> invalidArg "configured" "An evoPath requires an SgdRun."
        let id = parameters.GetId() |> require "Id"
        let replica = parameters.GetRepl() |> require "Repl"
        if int replica < 0 then invalidArg "parameters" "Replica cannot be negative."
        if parameters.GetRunName() <> Some configured.RunName then invalidArg "parameters" "Parameters belong to another run."
        if parameters.GetDatabaseName() |> Option.exists ((<>) configured.DatabaseName) then
            invalidArg "parameters" "Parameters belong to another database."
        if parameters.ParamMap.ContainsKey ParamKeys.generationFirstKey && (parameters.GetGenerationFirst() |> Option.forall (fun generation -> int generation < 0)) then
            invalidArg "parameters" "Invalid GenerationFirst."
        if awaitingSetup configured parameters then invalidArg "parameters" "The continuation requires Setup to resolve its source checkpoint."
        let last = parameters.GetGenerationIntervalLast() |> require "GenerationIntervalLast"
        let intervals = RunIntervals.resolve configured.RunName (SgdRun.genSaveIntervalsName sgd)
        if int last < 0 || (GenIntervalConfig.getSampleAtIndex intervals (int last)).IsNone then
            invalidArg "parameters" "Invalid GenerationIntervalLast."
        let progress = EvoProgress.fromParameters parameters
        let settings = parameters.ParamMap |> Map.filter (fun key _ -> not (EvoProgress.isProgress key))
        // Use the origin, not the current checkpoint, when resolving legacy Setup
        // queries. It is not inserted into Settings or the persisted representation.
        let initial = parameters.WithGenerationCurrent(Some (parameters.GetGenerationFirst() |> Option.defaultValue (UMX.tag 0)))
        let queryFor = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        let setupQuery parameters output =
            QueryParamsCatalogSetup.tryGet configured.SetupQueryCatalogName
            |> Option.bind (fun make -> make configured.ProjectName configured.DatabaseName parameters output)
        let inputs =
            [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            |> List.map (fun output ->
                let query =
                    SetupInputReferences.tryRecordedInput configured.ProjectName configured.DatabaseName parameters output
                    |> Option.orElseWith (fun () ->
                        queryFor initial output |> Option.map (SetupInputReferences.resolveSavedInput setupQuery parameters))
                    |> require (sprintf "Setup input for %A" output)
                output, query)
            |> Map.ofList
        let workflow = {
            ProjectName = configured.ProjectName; DatabaseName = configured.DatabaseName; RunName = configured.RunName
            ExecuteFunctionName = configured.ExecuteFunctionName
            RunQueryCatalogName = configured.RunQueryCatalogName; SetupQueryCatalogName = configured.SetupQueryCatalogName
            MakerNames = SgdRun.makerNames sgd
            GenSaveIntervalsName = SgdRun.genSaveIntervalsName sgd; GenSaveSubIntervalsName = SgdRun.genSaveSubIntervalsName sgd
        }
        let start = {
            Tests = inputs.[geneSortDataType.SortableTestsWithPrefix ""]
            Pool = inputs.[geneSortDataType.SorterPoolSet ""]
            FirstGeneration = parameters.GetGenerationFirst() |> Option.defaultValue (UMX.tag 0)
        }
        { id = id; replica = replica; workflow = workflow; settings = settings; start = start
          intervals = intervals; subIntervals = RunIntervals.resolve configured.RunName workflow.GenSaveSubIntervalsName }, progress

    let prepare configured parameters =
        if awaitingSetup configured parameters then AwaitingSetup (parameters.GetId() |> require "Id")
        else let path, progress = fromParameters configured parameters in Ready (path, progress)

    /// Adapter for existing executors and MessagePack DTOs. No new file schema.
    let toParameters (path: evoPath) progress =
        EvoProgress.apply progress path.settings |> runParameters.create

    let setupQuery output (path: evoPath) =
        path.SetupInputs.TryFind output
        |> require (sprintf "planned Setup file for %A" output)

    /// Existing executors may advance state but cannot silently alter the path.
    let progressFromParameters (path: evoPath) (updated: runParameters) =
        let settings = updated.ParamMap |> Map.filter (fun key _ -> not (EvoProgress.isProgress key))
        if settings <> path.settings then invalidArg "updated" "Execution changed the evolutionary specification."
        EvoProgress.fromParameters updated
