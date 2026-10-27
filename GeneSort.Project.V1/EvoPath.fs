namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Project.V1.ParamBundles
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
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

    // Read fixed execution settings directly; generation and completion belong to evoProgress.
    member this.GetActivationProbability() = ParamKeys.tryGetFloat ParamKeys.activationProbabilityKey this.settings

    member this.GetActivatorMutationRate() = ParamKeys.tryGetFloat ParamKeys.activatorMutationRateKey this.settings

    member this.GetAdjustCeLengthFrequency() : int<adjustCeLengthFrequency> option =
        ParamKeys.tryGetInt ParamKeys.adjustCeLengthFrequencyKey this.settings
        |> Option.map UMX.tag<adjustCeLengthFrequency>

    member this.GetAMax() = ParamKeys.tryGetInt ParamKeys.aMaxKey this.settings

    member this.GetCollectNewSortableTests() =
        ParamKeys.tryGetBool ParamKeys.collectSortableTestsKey this.settings
        |> Option.map UMX.tag<collectNewSortableTests>

    member this.GetDeletionRate() =
        ParamKeys.tryGetFloat ParamKeys.deletionRateKey this.settings
        |> Option.map UMX.tag<deletionRate>

    member this.GetDistinctSorterHashes() =
        ParamKeys.tryGetBool ParamKeys.distinctSorterHashesKey this.settings
        |> Option.map UMX.tag<distinctSorterHashes>

    member this.GetExcludeSelfCe() =
        ParamKeys.tryGetBool ParamKeys.excludeSelfCeKey this.settings
        |> Option.map UMX.tag<excludeSelfCe>

    member this.GetGenerationIntervalLast() =
        ParamKeys.tryGetInt ParamKeys.generationIntervalLastKey this.settings
        |> Option.map UMX.tag<generationIntervalLast>

    member this.GetGenerationsPerMergedSet() : int<generationsPerSubset> option =
        ParamKeys.tryGetInt ParamKeys.generationsPerMergedSetKey this.settings
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetGenerationsPerSubset() : int<generationsPerSubset> option =
        ParamKeys.tryGetInt ParamKeys.generationsPerSubsetKey this.settings
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetInsertionRate() =
        ParamKeys.tryGetFloat ParamKeys.insertionRateKey this.settings
        |> Option.map UMX.tag<insertionRate>

    member this.GetModificationRate() =
        ParamKeys.tryGetFloat ParamKeys.modificationRateKey this.settings
        |> Option.map UMX.tag<modificationRate>

    member this.GetMutationMod() =
        ParamKeys.tryGetInt ParamKeys.mutationModKey this.settings
        |> Option.map UMX.tag<mutationMod>

    member this.GetMutationRate() =
        ParamKeys.tryGetFloat ParamKeys.mutationRateKey this.settings
        |> Option.map UMX.tag<mutationRate>

    member this.GetMutatorVariant() =
        this.settings.TryFind ParamKeys.mutatorVariantKey
        |> Option.map MutatorVariant.fromString

    member this.GetOrthoRate() =
        ParamKeys.tryGetFloat ParamKeys.orthoRateKey this.settings
        |> Option.map UMX.tag<orthoRate>

    member this.GetParaRate() =
        ParamKeys.tryGetFloat ParamKeys.paraRateKey this.settings
        |> Option.map UMX.tag<paraRate>

    member this.GetPartitionCount() : int<GeneSort.Sorting.partitionCount> option =
        ParamKeys.tryGetInt ParamKeys.partitionCountKey this.settings
        |> Option.map UMX.tag<GeneSort.Sorting.partitionCount>

    member this.GetPrioritizeNewMutants() =
        ParamKeys.tryGetBool ParamKeys.prioritizeNewMutantsKey this.settings
        |> Option.map UMX.tag<prioritizeNewMutants>

    member this.GetRngType() =
        this.settings.TryFind ParamKeys.rngTypeKey
        |> Option.map RngType.fromString

    member this.GetSeedModificationRate() =
        ParamKeys.tryGetFloat ParamKeys.seedModificationRateKey this.settings
        |> Option.map UMX.tag<seedModificationRate>

    member this.GetSeedSoss() : uint64<randomSeed> option =
        ParamKeys.tryGetUInt64 ParamKeys.seedSossKey this.settings
        |> Option.map UMX.tag<randomSeed>

    member this.GetSelectedSorterCountPerPool() =
        ParamKeys.tryGetInt ParamKeys.selectedSorterCountPerPoolKey this.settings
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSelfSymRate() =
        ParamKeys.tryGetFloat ParamKeys.selfSymRateKey this.settings
        |> Option.map UMX.tag<selfSymRate>

    member this.GetSimpleSorterModelType() =
        this.settings.TryFind ParamKeys.simpleSorterModelTypeKey
        |> Option.bind (fun v ->
            try Some (SimpleSorterModelType.fromString v)
            with _ -> None)

    member this.GetSortedFraction() =
        ParamKeys.tryGetFloat ParamKeys.sortedFractionKey this.settings
        |> Option.map UMX.tag<sortedFraction>

    member this.GetSorterChildCount() =
        ParamKeys.tryGetInt ParamKeys.sorterChildCountKey this.settings
        |> Option.map UMX.tag<sorterChildCount>

    member this.GetSorterCountCycle() =
        ParamKeys.tryGetInt ParamKeys.sorterCountCycleKey this.settings
        |> Option.map UMX.tag<sorterCountCycle>

    member this.GetSorterCountCycleMultiplier() =
        ParamKeys.tryGetFloat ParamKeys.sorterCountCycleMultiplierKey this.settings
        |> Option.map UMX.tag<sorterCountCycleMultiplier>

    member this.GetSorterCountPerPool() =
        ParamKeys.tryGetInt ParamKeys.sorterCountPerPoolKey this.settings
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSorterCountPerPoolSet() =
        ParamKeys.tryGetInt ParamKeys.sorterCountPerPoolSetKey this.settings
        |> Option.map UMX.tag<sorterCountPerPoolSet>

    member this.GetSorterEvalMeasure() =
        this.settings.TryFind ParamKeys.sorterEvalMeasureKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterPoolExpansionRate() =
        ParamKeys.tryGetInt ParamKeys.sorterPoolExpansionRateKey this.settings
        |> Option.map UMX.tag<sorterPoolExpansionRate>

    member this.GetSorterPoolMeasure() =
        this.settings.TryFind ParamKeys.sorterPoolMeasureKey
        |> Option.map SorterPoolMeasure.fromCompactString

    member this.GetSorterPoolSelectionIntervals() =
        this.settings.TryFind ParamKeys.sorterPoolSelectionIntervalsKey
        |> Option.map (fun v -> GenIntervalConfig.fromString(v))

    member this.GetSortingWidth() =
        ParamKeys.tryGetInt ParamKeys.sortingWidthKey this.settings
        |> Option.map UMX.tag<sortingWidth>

/// A continuation becomes Ready only after Setup binds its validated checkpoint.
type evoPathPreparation =
    | Ready of path: evoPath * progress: evoProgress
    | AwaitingSetup of executionId: Guid<queryParamsId>

module EvoPath =
    let private require label value = value |> Option.defaultWith (fun () -> invalidArg "parameters" ("Missing " + label + "."))
    let private workflowFor (configured: run) =
        let sgd = match configured with SgdRun sgd -> sgd | _ -> invalidArg "configured" "An evoPath requires an SgdRun."
        {
            ProjectName = configured.ProjectName; DatabaseName = configured.DatabaseName; RunName = configured.RunName
            ExecuteFunctionName = configured.ExecuteFunctionName
            RunQueryCatalogName = configured.RunQueryCatalogName; SetupQueryCatalogName = configured.SetupQueryCatalogName
            MakerNames = SgdRun.makerNames sgd
            GenSaveIntervalsName = SgdRun.genSaveIntervalsName sgd; GenSaveSubIntervalsName = SgdRun.genSaveSubIntervalsName sgd
        }

    /// Validate a supplied path without rebuilding it or its file references.
    let requireRun configured (path: evoPath) =
        if path.Workflow <> workflowFor configured then
            invalidArg "path" "The evolutionary path belongs to another run or workflow. Reopen the saved run."

    let private awaitingSetup (configured: run) (parameters: runParameters) =
        let bound output = SetupInputReferences.tryRecordedInput configured.ProjectName configured.DatabaseName parameters output |> Option.isSome
        configured.ParameterSource.IsSome &&
            (not (bound (geneSortDataType.SorterPoolSet "")) || not (bound (geneSortDataType.SortableTestsWithPrefix "")) || parameters.GetGenerationFirst().IsNone)

    /// Build a path with the exact start produced by Setup, without reconstructing its references.
    let fromStart (configured: run) (start: evoStart) (parameters: runParameters) =
        let sgd = match configured with SgdRun sgd -> sgd | _ -> invalidArg "configured" "An evoPath requires an SgdRun."
        let id = parameters.GetId() |> require "Id"
        let replica = parameters.GetRepl() |> require "Repl"
        if int replica < 0 then invalidArg "parameters" "Replica cannot be negative."
        if parameters.GetRunName() <> Some configured.RunName then invalidArg "parameters" "Parameters belong to another run."
        if parameters.GetDatabaseName() |> Option.exists ((<>) configured.DatabaseName) then
            invalidArg "parameters" "Parameters belong to another database."
        if parameters.ParamMap.ContainsKey ParamKeys.generationFirstKey && (parameters.GetGenerationFirst() |> Option.forall (fun generation -> int generation < 0)) then
            invalidArg "parameters" "Invalid GenerationFirst."
        if start.FirstGeneration <> (parameters.GetGenerationFirst() |> Option.defaultValue (UMX.tag 0)) then
            invalidArg "start" "The start generation does not match the parameter row."
        for reference, output, recorded in [
            start.Tests, geneSortDataType.SortableTestsWithPrefix "", parameters.GetSetupSortableTestsWithPrefixId()
            start.Pool, geneSortDataType.SorterPoolSet "", parameters.GetSetupSorterPoolSetId() ] do
            if reference.ProjectName <> configured.ProjectName || reference.DbName <> configured.DatabaseName
               || reference.OutputDataType <> output || (recorded |> Option.exists ((<>) reference.Id)) then
                invalidArg "start" "The start references do not match the run and parameter row."
        let last = parameters.GetGenerationIntervalLast() |> require "GenerationIntervalLast"
        let intervals = RunIntervals.resolve configured.RunName (SgdRun.genSaveIntervalsName sgd)
        if int last < 0 || (GenIntervalConfig.getSampleAtIndex intervals (int last)).IsNone then
            invalidArg "parameters" "Invalid GenerationIntervalLast."
        let progress = EvoProgress.fromParameters parameters
        let settings = parameters.ParamMap |> Map.filter (fun key _ -> not (EvoProgress.isProgress key))
        let workflow = workflowFor configured
        { id = id; replica = replica; workflow = workflow; settings = settings; start = start
          intervals = intervals; subIntervals = RunIntervals.resolve configured.RunName workflow.GenSaveSubIntervalsName }, progress

    /// Reopen the existing persisted representation, retaining compatibility with older saved inputs.
    let fromParameters (configured: run) (parameters: runParameters) =
        if awaitingSetup configured parameters then invalidArg "parameters" "The continuation requires Setup to resolve its source checkpoint."
        let first = parameters.GetGenerationFirst() |> Option.defaultValue (UMX.tag 0)
        let initial = parameters.WithGenerationCurrent(Some first)
        let input output =
            SetupInputReferences.tryRecordedInput configured.ProjectName configured.DatabaseName parameters output
            |> Option.orElseWith (fun () ->
                let queryFor = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
                let setupQuery parameters output =
                    QueryParamsCatalogSetup.tryGet configured.SetupQueryCatalogName
                    |> Option.bind (fun make -> make configured.ProjectName configured.DatabaseName parameters output)
                queryFor initial output |> Option.map (SetupInputReferences.resolveSavedInput setupQuery parameters))
            |> require (sprintf "Setup input for %A" output)
        let start = {
            Tests = input (geneSortDataType.SortableTestsWithPrefix "")
            Pool = input (geneSortDataType.SorterPoolSet "")
            FirstGeneration = first
        }
        fromStart configured start parameters

    let prepare configured parameters =
        if awaitingSetup configured parameters then AwaitingSetup (parameters.GetId() |> require "Id")
        else let path, progress = fromParameters configured parameters in Ready (path, progress)

    /// Saved-file boundary: retain the existing parameter representation and MessagePack schema.
    let toParameters (path: evoPath) progress =
        EvoProgress.apply progress path.settings |> runParameters.create

    let setupQuery output (path: evoPath) =
        path.SetupInputs.TryFind output
        |> require (sprintf "planned Setup file for %A" output)
