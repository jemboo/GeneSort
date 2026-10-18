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
type parameterSet<'kind> =
    private { paramMap : Map<string, string> }

    static member create (paramMap: Map<string, string>) : parameterSet<'kind> =
        { paramMap = paramMap }

    // ============================================================
    // Keys
    // ============================================================

    static member ceLengthKey = "CeLength"
    static member codeModKey = "CodeModKey"
    static member collectSortableTestsKey = "CollectSortableTests"
    static member databaseNameKey = "DatabaseName"
    static member sourceProjectNameKey = "SourceProjectName"
    static member sourceDbNameKey = "SourceDbName"
    static member sourceRunNameKey = "SourceRunName"
    static member sourceGenerationIntervalKey = "SourceGenerationInterval"
    static member deletionRateKey = "DeletionRate"
    static member distinctSorterHashesKey = "DistinctSorterHashes"
    static member elapsedTimeKey = "ElapsedTime"
    static member excludeSelfCeKey = "ExcludeSelfCe"
    static member generationCurrentKey = "GenerationCurrent"
    static member generationFirstKey = "GenerationFirst"
    static member generationIntervalCountKey = "GenerationIntervalCount"
    static member generationIntervalLastKey = "GenerationIntervalLast"
    static member generationsPerSubsetKey = "GenerationsPerSubset"
    static member generationsPerMergedSetKey = "GenerationsPerMergedSet"
    static member adjustCeLengthFrequencyKey = "AdjustCeLengthFrequency"
    static member idKey = "Id"

    // Setup output file IDs (queryParamsId), distinct from IDs inside the stored payloads.
    static member setupSortableTestsIdKey = "SetupSortableTestsId"
    static member setupSortableTestsWithPrefixIdKey = "SetupSortableTestsWithPrefixId"
    static member setupSorterPoolSetIdKey = "SetupSorterPoolSetId"
    static member setupSorterPoolSetSummarySetIdKey = "SetupSorterPoolSetSummarySetId"
    static member setupSorterSetIdKey = "SetupSorterSetId"
    static member setupSorterModelSetIdKey = "SetupSorterModelSetId"
    static member setupSorterSetEvalIdKey = "SetupSorterSetEvalId"
    static member setupSorterPoolBinsSetSeriesIdKey = "SetupSorterPoolBinsSetSeriesId"
    static member setupSorterPoolSetHistoryIdKey = "SetupSorterPoolSetHistoryId"

    static member insertionRateKey = "InsertionRate"
    static member latticeDistanceKey = "LatticeDistance"
    static member maxOrbitKey = "MaxOrbit"
    static member mergeDimensionKey = "MergeDimension"
    static member mergeLibIdKey = "MergeLibId"
    static member mergeSuffixTypeKey = "MergeSuffixType"
    static member messageKey = "Message"
    static member modificationRateKey = "ModificationRate"
    static member mutationModKey = "MutationMod"
    static member mutationRateKey = "MutationRate"
    static member mutatorParamsKey = "MutatorParams"
    static member mutatorVariantKey = "MutatorVariantKey"
    static member orthoRateKey = "OrthoRate"
    static member paraRateKey = "ParaRate"
    static member partitionCountKey = "PartitionCount"
    static member prefixLibIdKey = "PrefixLibId"
    static member prioritizeNewMutantsKey = "PrioritizeNewMutants"
    static member projectNameKey = "ProjectName"
    static member queryNameKey = "QueryName"
    static member replKey = "Repl"
    static member replSpanKey = "ReplSpan"
    static member rngTypeKey = "RngType"
    static member runFinishedKey = "RunFinished"
    static member runNameKey = "RunName"
    static member seedModificationRateKey = "SeedModificationRate"
    static member seedSossKey = "SeedSoss"
    static member seedSorterPoolSelectionTypeKey = "SeedPoolSorterSelectionType"
    static member selectedSorterCountPerPoolKey = "SelectedSorterCountPerPool"
    static member selfSymRateKey = "SelfSym"
    static member simpleSorterModelTypeKey = "SimpleSorterModelType"
    static member sortableCountKey = "SortableCount"
    static member sortableDataFormatKey = "SortableDataFormat"
    static member sortedFractionKey = "SortedFraction"
    static member sorterChildCountKey = "SorterChildCount"
    static member sorterCountCycleKey = "SorterCountCycle"
    static member sorterCountCycleMultiplierKey = "SorterCountCycleMultiplier"
    static member sorterCountKey = "SorterCount"
    static member sorterCountPerPoolKey = "SorterCountPerPool"
    static member sorterCountPerPoolSetKey = "SorterCountPerPoolSet"
    static member sorterEvalMeasureInitialKey = "SorterEvalMeasureInitial"
    static member sorterEvalMeasureKey = "SorterEvalMeasure"
    static member sorterEvalTypeKey = "SorterEvalType"
    static member sorterLibIdKey = "SorterLibId"
    static member sorterLibVariantKey = "SorterLibVariant"
    static member sorterParentCountKey = "SorterParentCount"
    static member sorterPoolCountKey = "SorterPoolCount"
    static member sorterPoolExpansionRateKey = "SorterPoolExpansionRate"
    static member sorterPoolMeasureKey = "sorterPoolMeasure"
    static member sorterPoolSelectionIntervalsKey = "SorterPoolSelectionIntervals"
    static member sortingWidthKey = "SortingWidth"
    static member stageLengthKey = "StageLength"
    static member startingReplKey = "StartingRepl"
    static member textReportNameKey = "TextReportName"

    // ============================================================
    // Private Helpers
    // ============================================================

    static member private addOrRemove key valueOpt map =
        match valueOpt with
        | Some v -> Map.add key v map
        | None -> Map.remove key map

    static member private tryGetBool (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Boolean.TryParse v with true, b -> Some b | _ -> None)

    static member private tryGetFloat (key: string) (map: Map<string, string>) =
        map.TryFind key
        |> Option.bind (fun v ->
            match Double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture) with
            | true, f -> Some f
            | _ -> None)

    static member private tryGetGuid (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Guid.TryParse v with true, g -> Some g | _ -> None)

    static member private tryGetInt (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Int32.TryParse v with true, i -> Some i | _ -> None)

    static member private tryGetUInt64 (key: string) (map: Map<string, string>) =
        map.TryFind key
        |> Option.bind (fun v ->
            match UInt64.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture) with
            | true, seed -> Some seed
            | _ -> None)

    member this.ParamMap with get() :Map<string,string> = this.paramMap

    // ============================================================
    // Getters
    // ============================================================

    member this.GetCeLength() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.ceLengthKey this.paramMap
        |> Option.map UMX.tag<ceLength>

    member this.GetCodeModKey() =
        this.paramMap.TryFind parameterSet<'kind>.codeModKey
        |> Option.map UMX.tag<codeModKey>

    member this.GetCollectNewSortableTests() =
        parameterSet<'kind>.tryGetBool parameterSet<'kind>.collectSortableTestsKey this.paramMap
        |> Option.map UMX.tag<collectNewSortableTests>

    member this.GetDatabaseName() =
        this.paramMap.TryFind parameterSet<'kind>.databaseNameKey
        |> Option.map UMX.tag<databaseName>

    member this.GetSourceProjectName() =
        this.paramMap.TryFind parameterSet<'kind>.sourceProjectNameKey |> Option.map UMX.tag<projectName>

    member this.GetSourceDbName() =
        this.paramMap.TryFind parameterSet<'kind>.sourceDbNameKey |> Option.map UMX.tag<databaseName>

    member this.GetSourceRunName() =
        this.paramMap.TryFind parameterSet<'kind>.sourceRunNameKey |> Option.map UMX.tag<runName>

    /// Zero-based index in the source run's generation save interval configuration.
    member this.GetSourceGenerationInterval() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sourceGenerationIntervalKey this.paramMap

    member this.GetDeletionRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.deletionRateKey this.paramMap
        |> Option.map UMX.tag<deletionRate>

    member this.GetDistinctSorterHashes() =
        parameterSet<'kind>.tryGetBool parameterSet<'kind>.distinctSorterHashesKey this.paramMap
        |> Option.map UMX.tag<distinctSorterHashes>

    member this.GetElapsedTime() =
        this.paramMap.TryFind parameterSet<'kind>.elapsedTimeKey

    member this.GetExcludeSelfCe() =
        parameterSet<'kind>.tryGetBool parameterSet<'kind>.excludeSelfCeKey this.paramMap
        |> Option.map UMX.tag<excludeSelfCe>

    member this.GetGenerationCurrent() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.generationCurrentKey this.paramMap
        |> Option.map UMX.tag<generationNumber>

    member this.GetGenerationFirst() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.generationFirstKey this.paramMap
        |> Option.map UMX.tag<generationNumber>

    member this.GetGenerationIntervalCount() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.generationIntervalCountKey this.paramMap
        |> Option.map UMX.tag<generationIntervalCount>

    member this.GetGenerationIntervalLast() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.generationIntervalLastKey this.paramMap
        |> Option.map UMX.tag<generationIntervalLast>

    member this.GetId() =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.idKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetInsertionRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.insertionRateKey this.paramMap
        |> Option.map UMX.tag<insertionRate>

    member this.GetLatticeDistance() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.latticeDistanceKey this.paramMap
        |> Option.map UMX.tag<latticeDistance>

    member this.GetMaxOrbit() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.maxOrbitKey this.paramMap

    member this.GetMergeDimension() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.mergeDimensionKey this.paramMap
        |> Option.map UMX.tag<mergeDimension>

    member this.GetMergeLibId() =
        this.paramMap.TryFind parameterSet<'kind>.mergeLibIdKey
        |> Option.map MergeLibId.fromString

    member this.GetMergeSuffixType() =
        this.paramMap.TryFind parameterSet<'kind>.mergeSuffixTypeKey
        |> Option.bind (fun v ->
            try Some (MergeSuffixType.fromString v)
            with _ -> None)

    member this.GetModificationRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.modificationRateKey this.paramMap
        |> Option.map UMX.tag<modificationRate>

    member this.GetMutationMod() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.mutationModKey this.paramMap
        |> Option.map UMX.tag<mutationMod>

    member this.GetMutationRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.mutationRateKey this.paramMap
        |> Option.map UMX.tag<mutationRate>

    member this.GetMutatorVariant() =
        this.paramMap.TryFind parameterSet<'kind>.mutatorVariantKey
        |> Option.map MutatorVariant.fromString

    member this.GetOrthoRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.orthoRateKey this.paramMap
        |> Option.map UMX.tag<orthoRate>

    member this.GetParaRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.paraRateKey this.paramMap
        |> Option.map UMX.tag<paraRate>

    member this.GetPartitionCount() : int<GeneSort.Sorting.partitionCount> option =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.partitionCountKey this.paramMap
        |> Option.map UMX.tag<GeneSort.Sorting.partitionCount>

    member this.GetGenerationsPerSubset() : int<generationsPerSubset> option =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.generationsPerSubsetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetGenerationsPerMergedSet() : int<generationsPerSubset> option =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.generationsPerMergedSetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetAdjustCeLengthFrequency() : int<adjustCeLengthFrequency> option =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.adjustCeLengthFrequencyKey this.paramMap
        |> Option.map UMX.tag<adjustCeLengthFrequency>

    member this.GetPrefixLibId() =
        this.paramMap.TryFind parameterSet<'kind>.prefixLibIdKey
            |> Option.map PrefixLibId.fromString

    member this.GetPrioritizeNewMutants() =
        parameterSet<'kind>.tryGetBool parameterSet<'kind>.prioritizeNewMutantsKey this.paramMap
        |> Option.map UMX.tag<prioritizeNewMutants>

    member this.GetProjectName() =
        this.paramMap.TryFind parameterSet<'kind>.projectNameKey
        |> Option.map UMX.tag<projectName>

    member this.GetQueryName() =
        this.paramMap.TryFind parameterSet<'kind>.queryNameKey
        |> Option.map UMX.tag<queryName>

    member this.GetRepl() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.replKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetReplSpan() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.replSpanKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetRngType() =
        this.paramMap.TryFind parameterSet<'kind>.rngTypeKey
        |> Option.map RngType.fromString

    member this.GetRunFinished() =
        parameterSet<'kind>.tryGetBool parameterSet<'kind>.runFinishedKey this.paramMap

    member this.GetRunName() =
        this.paramMap.TryFind parameterSet<'kind>.runNameKey
        |> Option.map UMX.tag<runName>

    member this.GetSeedModificationRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.seedModificationRateKey this.paramMap
        |> Option.map UMX.tag<seedModificationRate>

    member this.GetSeedSoss() : uint64<randomSeed> option =
        parameterSet<'kind>.tryGetUInt64 parameterSet<'kind>.seedSossKey this.paramMap
        |> Option.map UMX.tag<randomSeed>

    member this.GetSeedSorterPoolSelectionType() =
        this.paramMap.TryFind parameterSet<'kind>.seedSorterPoolSelectionTypeKey
        |> Option.map SorterSelectionType.fromString

    member this.GetSelectedSorterCountPerPool() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.selectedSorterCountPerPoolKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSelfSymRate() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.selfSymRateKey this.paramMap
        |> Option.map UMX.tag<selfSymRate>

    member this.GetMutatorParams() =
        this.paramMap.TryFind parameterSet<'kind>.mutatorParamsKey
        |> Option.bind (fun v ->
            match MutatorParams.fromString v with
            | Ok p -> Some p
            | Error _ -> None)

    member this.GetSimpleSorterModelType() =
        this.paramMap.TryFind parameterSet<'kind>.simpleSorterModelTypeKey
        |> Option.bind (fun v ->
            try Some (SimpleSorterModelType.fromString v)
            with _ -> None)

    member this.GetSortableCount() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sortableCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSortableDataFormat() =
        this.paramMap.TryFind parameterSet<'kind>.sortableDataFormatKey
        |> Option.bind (fun v ->
            try Some (SortableDataFormat.fromString v)
            with _ -> None)

    member this.GetSortedFraction() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.sortedFractionKey this.paramMap
        |> Option.map UMX.tag<sortedFraction>

    member this.GetSorterChildCount() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterChildCountKey this.paramMap
        |> Option.map UMX.tag<sorterChildCount>

    member this.GetSorterCount() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSorterCountCycle() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterCountCycleKey this.paramMap
        |> Option.map UMX.tag<sorterCountCycle>

    member this.GetSorterCountCycleMultiplier() =
        parameterSet<'kind>.tryGetFloat parameterSet<'kind>.sorterCountCycleMultiplierKey this.paramMap
        |> Option.map UMX.tag<sorterCountCycleMultiplier>

    member this.GetSorterCountPerPool() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterCountPerPoolKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSorterCountPerPoolSet() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterCountPerPoolSetKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPoolSet>

    member this.GetSorterEvalMeasure() =
        this.paramMap.TryFind parameterSet<'kind>.sorterEvalMeasureKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterEvalMeasureInitial() =
        this.paramMap.TryFind parameterSet<'kind>.sorterEvalMeasureInitialKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterEvalType() =
        this.paramMap.TryFind parameterSet<'kind>.sorterEvalTypeKey
        |> Option.map SorterEvalType.fromString

    member this.GetSorterLibId() =
        this.paramMap.TryFind parameterSet<'kind>.sorterLibIdKey
        |> Option.map SorterLibId.fromString

    member this.GetSorterLibVariant() =
        this.paramMap.TryFind parameterSet<'kind>.sorterLibVariantKey
        |> Option.bind (fun v ->
            try Some (SorterLibVariant.fromString v)
            with _ -> None)

    member this.GetSorterParentCount() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterParentCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSorterPoolCount() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterPoolCountKey this.paramMap
        |> Option.map UMX.tag<sorterPoolCount>

    member this.GetSorterPoolExpansionRate() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sorterPoolExpansionRateKey this.paramMap
        |> Option.map UMX.tag<sorterPoolExpansionRate>

    member this.GetSorterPoolMeasure() =
        this.paramMap.TryFind parameterSet<'kind>.sorterPoolMeasureKey
        |> Option.map SorterPoolMeasure.fromCompactString

    member this.GetSorterPoolSelectionIntervals() =
        this.paramMap.TryFind parameterSet<'kind>.sorterPoolSelectionIntervalsKey
        |> Option.map (fun v -> GenIntervalConfig.fromString(v))

    member this.GetSortingWidth() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.sortingWidthKey this.paramMap
        |> Option.map UMX.tag<sortingWidth>

    member this.GetStageLength() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.stageLengthKey this.paramMap
        |> Option.map UMX.tag<stageLength>

    member this.GetStartingRepl() =
        parameterSet<'kind>.tryGetInt parameterSet<'kind>.startingReplKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetTextReportName() =
        this.paramMap.TryFind parameterSet<'kind>.textReportNameKey
        |> Option.map UMX.tag<textReportName>

    // ============================================================
    // With* members
    // ============================================================

    member this.WithCodeModKey(dbn: string<codeModKey> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.codeModKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithCeLength(cl: int<ceLength> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.ceLengthKey (cl |> Option.map UmxExt.intToRaw) }

    member this.WithCollectNewSortableTests(cst: bool<collectNewSortableTests> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.collectSortableTestsKey (cst |> Option.map string) }

    member this.WithDatabaseName(dbn: string<databaseName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.databaseNameKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceProjectName(value: string<projectName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sourceProjectNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceDbName(value: string<databaseName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sourceDbNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceRunName(value: string<runName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sourceRunNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceGenerationInterval(value: int option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sourceGenerationIntervalKey (value |> Option.map string) }

    member this.WithDeletionRate(dr: float<deletionRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.deletionRateKey (dr |> Option.map UmxExt.floatToRaw) }

    member this.WithDistinctSorterHashes(dsh: bool option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.distinctSorterHashesKey (dsh |> Option.map string) }

    member this.WithElapsedTime(time: string option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.elapsedTimeKey time }

    member this.WithExcludeSelfCe(esc: bool<excludeSelfCe> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.excludeSelfCeKey (esc |> Option.map UmxExt.boolToRaw) }

    member this.WithGenerationCurrent(gen: int<generationNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.generationCurrentKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationFirst(gen: int<generationNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.generationFirstKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationIntervalCount(count: int<generationIntervalCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.generationIntervalCountKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationIntervalLast(last: int<generationIntervalLast> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.generationIntervalLastKey (last |> Option.map UmxExt.intToRaw) }

    member this.WithId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.idKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithInsertionRate(ir: float<insertionRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.insertionRateKey (ir |> Option.map UmxExt.floatToRaw) }

    member this.WithLatticeDistance(ld: int<latticeDistance> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.latticeDistanceKey (ld |> Option.map UmxExt.intToRaw) }

    member this.WithMaxOrbit(mo: int option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.maxOrbitKey (mo |> Option.map string) }

    member this.WithMergeDimension(md: int<mergeDimension> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mergeDimensionKey (md |> Option.map UmxExt.intToRaw) }

    member this.WithMergeLibId(mlid: mergeLibId option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mergeLibIdKey (mlid |> Option.map MergeLibId.toString) }

    member this.WithMergeSuffixType(mft: mergeSuffixType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mergeSuffixTypeKey (mft |> Option.map MergeSuffixType.toString) }

    member this.WithMessage(message: string option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.messageKey message }

    member this.WithModificationRate(mr: float<modificationRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.modificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutationMod(mm: int<mutationMod> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mutationModKey (mm |> Option.map UmxExt.intToRaw) }

    member this.WithMutationRate(mr: float<mutationRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mutationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutatorParams(mtp: mutatorParams option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mutatorParamsKey (mtp |> Option.map MutatorParams.toString) }

    member this.WithMutatorVariant(mutv: mutatorVariant option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.mutatorVariantKey (mutv |> Option.map MutatorVariant.toString) }

    member this.WithOrthoRate(ortho: float<orthoRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.orthoRateKey (ortho |> Option.map UmxExt.floatToRaw) }

    member this.WithParaRate(para: float<paraRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.paraRateKey (para |> Option.map UmxExt.floatToRaw) }

    member this.WithPartitionCount(count: int<partitionCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.partitionCountKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationsPerSubset(count: int<generationsPerSubset> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.generationsPerSubsetKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationsPerMergedSet(count: int<generationsPerSubset> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.generationsPerMergedSetKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithAdjustCeLengthFrequency(count: int<adjustCeLengthFrequency> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.adjustCeLengthFrequencyKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithPrefixLibId(pfxLibId: prefixLibId option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.prefixLibIdKey (pfxLibId |> Option.map PrefixLibId.toString) }

    member this.WithPrioritizeNewMutants(pnm: bool option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.prioritizeNewMutantsKey (pnm |> Option.map string) }

    member this.WithProjectName(pn: string<projectName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.projectNameKey (pn |> Option.map UmxExt.stringToRaw) }

    member this.WithQueryName(qn: string<queryName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.queryNameKey (qn |> Option.map UmxExt.stringToRaw) }

    member this.WithRepl(repl: int<replNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.replKey (repl |> Option.map UmxExt.intToRaw) }

    member this.WithReplSpan(rs: int<replNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.replSpanKey (rs |> Option.map UmxExt.intToRaw) }

    member this.WithRngType(rng: rngType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.rngTypeKey (rng |> Option.map RngType.toString) }

    member this.WithRunFinished(fin: bool option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.runFinishedKey (fin |> Option.map string) }

    member this.WithRunName(rn: string<runName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.runNameKey (rn |> Option.map UmxExt.stringToRaw) }

    member this.WithSeedModificationRate(mr: float<seedModificationRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.seedModificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithSeedSoss(seed: uint64<randomSeed> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.seedSossKey (seed |> Option.map UmxExt.uint64ToRaw) }

    member this.WithSeedSorterPoolSelectionType(ses: sorterSelectionType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.seedSorterPoolSelectionTypeKey (ses |> Option.map SorterSelectionType.toString) }

    member this.WithSelectedSorterCountPerPool(sc: int<sorterCountPerPool> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.selectedSorterCountPerPoolKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSelfSymRate(ssr: float<selfSymRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.selfSymRateKey (ssr |> Option.map UmxExt.floatToRaw) }

    member this.WithSimpleSorterModelType(smt: simpleSorterModelType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.simpleSorterModelTypeKey (smt |> Option.map SimpleSorterModelType.toString) }

    member this.WithSortableCount(sc: int<sortableCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sortableCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSortableDataFormat(sdt: sortableDataFormat option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sortableDataFormatKey (sdt |> Option.map SortableDataFormat.toString) }

    member this.WithSortedFraction(sf: float<sortedFraction> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sortedFractionKey (sf |> Option.map UmxExt.floatToRaw) }

    member this.WithSorterChildCount(scc: int<sorterChildCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterChildCountKey (scc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCount(sc: int<sorterCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountCycle(scc: int<sorterCountCycle> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterCountCycleKey (scc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountCycleMultiplier(scc: float<sorterCountCycleMultiplier> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterCountCycleMultiplierKey (scc |> Option.map UmxExt.floatToRaw) }

    member this.WithSorterCountPerPool(sc: int<sorterCountPerPool> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterCountPerPoolKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountPerPoolSet(sc: int<sorterCountPerPoolSet> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterCountPerPoolSetKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterEvalMeasure(sem: sorterEvalMeasure option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterEvalMeasureKey (sem |> Option.map SorterEvalFunctions.toCompactString) }

    member this.WithSorterEvalMeasureInitial(sem: sorterEvalMeasure option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterEvalMeasureInitialKey (sem |> Option.map SorterEvalFunctions.toCompactString) }

    member this.WithSorterEvalType(set: sorterEvalType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterEvalTypeKey (set |> Option.map SorterEvalType.toString) }

    member this.WithSorterLibId(slId: sorterLibId option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterLibIdKey (slId |> Option.map SorterLibId.toString) }

    member this.WithSorterLibVariant(slv: sorterLibVariant option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterLibVariantKey (slv |> Option.map SorterLibVariant.toString) }

    member this.WithSorterParentCount(spc: int<sorterCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterParentCountKey (spc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolCount(sc: int<sorterPoolCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterPoolCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolExpansionRate(sper: int<sorterPoolExpansionRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterPoolExpansionRateKey (sper |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolMeasure(spm: sorterPoolMeasure option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterPoolMeasureKey (spm |> Option.map SorterPoolMeasure.toCompactString) }

    member this.WithSorterPoolSelectionIntervals(sConfig: genIntervalConfig option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sorterPoolSelectionIntervalsKey (sConfig |> Option.map GenIntervalConfig.toString) }

    member this.WithSortingWidth(w: int<sortingWidth> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.sortingWidthKey (w |> Option.map UmxExt.intToRaw) }

    member this.WithStageLength(sl: int<stageLength> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.stageLengthKey (sl |> Option.map UmxExt.intToRaw) }

    member this.WithStartingRepl(sr: int<replNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.startingReplKey (sr |> Option.map UmxExt.intToRaw) }

    member this.WithTextReportName(trn: string<textReportName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.textReportNameKey (trn |> Option.map UmxExt.stringToRaw) }

    // Setup output file ID accessors.
    member this.GetSetupSortableTestsId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSortableTestsIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSortableTestsId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSortableTestsIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSortableTestsWithPrefixId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSortableTestsWithPrefixIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSortableTestsWithPrefixId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSortableTestsWithPrefixIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterPoolSetId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterPoolSetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterPoolSetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterPoolSetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterPoolSetSummarySetId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterPoolSetSummarySetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterPoolSetSummarySetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterPoolSetSummarySetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterSetId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterSetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterSetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterSetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterModelSetId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterModelSetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterModelSetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterModelSetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterSetEvalId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterSetEvalIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterSetEvalId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterSetEvalIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterPoolBinsSetSeriesId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterPoolBinsSetSeriesIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterPoolBinsSetSeriesId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterPoolBinsSetSeriesIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.GetSetupSorterPoolSetHistoryId() : Guid<queryParamsId> option =
        parameterSet<'kind>.tryGetGuid parameterSet<'kind>.setupSorterPoolSetHistoryIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.WithSetupSorterPoolSetHistoryId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> parameterSet<'kind>.addOrRemove parameterSet<'kind>.setupSorterPoolSetHistoryIdKey (id |> Option.map UmxExt.guidToRaw) }

