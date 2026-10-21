namespace GeneSort.Project.V1.Parameters

open System
open System.Globalization
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Eval.V1.Sgd
open GeneSort.Project.V1.ParamBundles
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1

/// Shared immutable parameters; the kind keeps setup and run values distinct.
module ParamKeys =

    let addOrRemove key valueOpt map =
        match valueOpt with
        | Some v -> Map.add key v map
        | None -> Map.remove key map

    let tryGetBool (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Boolean.TryParse v with true, b -> Some b | _ -> None)

    let tryGetFloat (key: string) (map: Map<string, string>) =
        map.TryFind key
        |> Option.bind (fun v ->
            match Double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture) with
            | true, f -> Some f
            | _ -> None)

    let tryGetGuid (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Guid.TryParse v with true, g -> Some g | _ -> None)

    let tryGetInt (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Int32.TryParse v with true, i -> Some i | _ -> None)

    let tryGetUInt64 (key: string) (map: Map<string, string>) =
        map.TryFind key
        |> Option.bind (fun v ->
            match UInt64.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture) with
            | true, seed -> Some seed
            | _ -> None)

    [<Literal>]
    let ceLengthKey = "CeLength"
    [<Literal>]
    let codeModKey = "CodeModKey"
    [<Literal>]
    let collectSortableTestsKey = "CollectSortableTests"
    [<Literal>]
    let databaseNameKey = "DatabaseName"
    [<Literal>]
    let sourceProjectNameKey = "SourceProjectName"
    [<Literal>]
    let sourceDbNameKey = "SourceDbName"
    [<Literal>]
    let sourceRunNameKey = "SourceRunName"
    [<Literal>]
    let sourceGenerationIntervalKey = "SourceGenerationInterval"
    [<Literal>]
    let sourceExecutionIdKey = "SourceExecutionId"
    [<Literal>]
    let sourcePoolFileIdKey = "SourcePoolFileId"
    [<Literal>]
    let sourceTestsFileIdKey = "SourceTestsFileId"
    [<Literal>]
    let deletionRateKey = "DeletionRate"
    [<Literal>]
    let distinctSorterHashesKey = "DistinctSorterHashes"
    [<Literal>]
    let elapsedTimeKey = "ElapsedTime"
    [<Literal>]
    let excludeSelfCeKey = "ExcludeSelfCe"
    [<Literal>]
    let generationCurrentKey = "GenerationCurrent"
    [<Literal>]
    let generationFirstKey = "GenerationFirst"
    [<Literal>]
    let generationIntervalCountKey = "GenerationIntervalCount"
    [<Literal>]
    let generationIntervalLastKey = "GenerationIntervalLast"
    [<Literal>]
    let generationsPerSubsetKey = "GenerationsPerSubset"
    [<Literal>]
    let generationsPerMergedSetKey = "GenerationsPerMergedSet"
    [<Literal>]
    let adjustCeLengthFrequencyKey = "AdjustCeLengthFrequency"
    [<Literal>]
    let idKey = "Id"

    // Setup output file IDs (queryParamsId), distinct from IDs inside the stored payloads.
    [<Literal>]
    let setupParametersIdKey = "SetupParametersId"
    [<Literal>]
    let setupSortableTestsIdKey = "SetupSortableTestsId"
    [<Literal>]
    let setupSortableTestsWithPrefixIdKey = "SetupSortableTestsWithPrefixId"
    [<Literal>]
    let setupSorterPoolSetIdKey = "SetupSorterPoolSetId"
    [<Literal>]
    let setupSorterPoolSetSummarySetIdKey = "SetupSorterPoolSetSummarySetId"
    [<Literal>]
    let setupSorterSetIdKey = "SetupSorterSetId"
    [<Literal>]
    let setupSorterModelSetIdKey = "SetupSorterModelSetId"
    [<Literal>]
    let setupSorterSetEvalIdKey = "SetupSorterSetEvalId"
    [<Literal>]
    let setupSorterPoolBinsSetSeriesIdKey = "SetupSorterPoolBinsSetSeriesId"
    [<Literal>]
    let setupSorterPoolSetHistoryIdKey = "SetupSorterPoolSetHistoryId"
    [<Literal>]
    let insertionRateKey = "InsertionRate"
    [<Literal>]
    let latticeDistanceKey = "LatticeDistance"
    [<Literal>]
    let maxOrbitKey = "MaxOrbit"
    [<Literal>]
    let mergeDimensionKey = "MergeDimension"
    [<Literal>]
    let mergeLibIdKey = "MergeLibId"
    [<Literal>]
    let mergeSuffixTypeKey = "MergeSuffixType"
    [<Literal>]
    let messageKey = "Message"
    [<Literal>]
    let modificationRateKey = "ModificationRate"
    [<Literal>]
    let mutationModKey = "MutationMod"
    [<Literal>]
    let mutationRateKey = "MutationRate"
    [<Literal>]
    let mutatorParamsKey = "MutatorParams"
    [<Literal>]
    let mutatorVariantKey = "MutatorVariantKey"
    [<Literal>]
    let orthoRateKey = "OrthoRate"
    [<Literal>]
    let paraRateKey = "ParaRate"
    [<Literal>]
    let partitionCountKey = "PartitionCount"
    [<Literal>]
    let prefixLibIdKey = "PrefixLibId"
    [<Literal>]
    let prioritizeNewMutantsKey = "PrioritizeNewMutants"
    [<Literal>]
    let projectNameKey = "ProjectName"
    [<Literal>]
    let queryNameKey = "QueryName"
    [<Literal>]
    let replKey = "Repl"
    [<Literal>]
    let replSpanKey = "ReplSpan"
    [<Literal>]
    let rngTypeKey = "RngType"
    [<Literal>]
    let runFinishedKey = "RunFinished"
    [<Literal>]
    let runNameKey = "RunName"
    [<Literal>]
    let seedModificationRateKey = "SeedModificationRate"
    [<Literal>]
    let seedSossKey = "SeedSoss"
    [<Literal>]
    let seedSorterPoolSelectionTypeKey = "SeedPoolSorterSelectionType"
    [<Literal>]
    let selectedSorterCountPerPoolKey = "SelectedSorterCountPerPool"
    [<Literal>]
    let selfSymRateKey = "SelfSym"
    [<Literal>]
    let simpleSorterModelTypeKey = "SimpleSorterModelType"
    [<Literal>]
    let sortableCountKey = "SortableCount"
    [<Literal>]
    let sortableDataFormatKey = "SortableDataFormat"
    [<Literal>]
    let sortedFractionKey = "SortedFraction"
    [<Literal>]
    let sorterChildCountKey = "SorterChildCount"
    [<Literal>]
    let sorterCountCycleKey = "SorterCountCycle"
    [<Literal>]
    let sorterCountCycleMultiplierKey = "SorterCountCycleMultiplier"
    [<Literal>]
    let sorterCountKey = "SorterCount"
    [<Literal>]
    let sorterCountPerPoolKey = "SorterCountPerPool"
    [<Literal>]
    let sorterCountPerPoolSetKey = "SorterCountPerPoolSet"
    [<Literal>]
    let sorterEvalMeasureInitialKey = "SorterEvalMeasureInitial"
    [<Literal>]
    let sorterEvalMeasureKey = "SorterEvalMeasure"
    [<Literal>]
    let sorterEvalTypeKey = "SorterEvalType"
    [<Literal>]
    let sorterLibIdKey = "SorterLibId"
    [<Literal>]
    let sorterLibVariantKey = "SorterLibVariant"
    [<Literal>]
    let sorterParentCountKey = "SorterParentCount"
    [<Literal>]
    let sorterPoolCountKey = "SorterPoolCount"
    [<Literal>]
    let sorterPoolExpansionRateKey = "SorterPoolExpansionRate"
    [<Literal>]
    let sorterPoolMeasureKey = "sorterPoolMeasure"
    [<Literal>]
    let sorterPoolSelectionIntervalsKey = "SorterPoolSelectionIntervals"
    [<Literal>]
    let sortingWidthKey = "SortingWidth"
    [<Literal>]
    let stageLengthKey = "StageLength"
    [<Literal>]
    let startingReplKey = "StartingRepl"
    [<Literal>]
    let textReportNameKey = "TextReportName"
