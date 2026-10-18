namespace GeneSort.Project.V1

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
open GeneSort.Project.V1.RunParamBundles
open GeneSort.Eval.V1.Sgd.Standard

type runParameters =
    private { paramMap : Map<string, string> }

    static member create (paramMap: Map<string, string>) : runParameters =
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
    static member sourceAdjustCeLengthFrequencyKey = "SourceAdjustCeLengthFrequency"
    static member sourceCodeModKey = "SourceCodeModKey"
    static member sourceSorterCountPerPoolKey = "SourceSorterCountPerPool"
    static member sourceSorterPoolCountKey = "SourceSorterPoolCount"
    static member sourceParaRateKey = "SourceParaRate"
    static member sourceSelfSymRateKey = "SourceSelfSymRate"
    static member sourceSeedModificationRateKey = "SourceSeedModificationRate"
    static member sourceModificationRateKey = "SourceModificationRate"
    static member sourceMutationModKey = "SourceMutationMod"
    static member sourceMutatorVariantKey = "SourceMutatorVariantKey"
    static member sourceSeedSossKey = "SourceSeedSoss"
    static member sourcePartitionCountKey = "SourcePartitionCount"
    static member sourceGenerationsPerSubsetKey = "SourceGenerationsPerSubset"
    static member sourceGenerationsPerMergedSetKey = "SourceGenerationsPerMergedSet"
    static member sourceSorterPoolSetId = "SourceSorterPoolSetId"
    static member sourceSourceSorterPoolSetId = "SourceSourceSorterPoolSetId"
    static member sourceSorterPoolId = "SourceSorterPoolId"
    static member sourceSourceSorterPoolId = "SourceSourceSorterPoolId"
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
        runParameters.tryGetInt runParameters.ceLengthKey this.paramMap
        |> Option.map UMX.tag<ceLength>

    member this.GetCodeModKey() =
        this.paramMap.TryFind runParameters.codeModKey
        |> Option.map UMX.tag<codeModKey>

    member this.GetCollectNewSortableTests() =
        runParameters.tryGetBool runParameters.collectSortableTestsKey this.paramMap
        |> Option.map UMX.tag<collectNewSortableTests>

    member this.GetDatabaseName() =
        this.paramMap.TryFind runParameters.databaseNameKey
        |> Option.map UMX.tag<databaseName>

    member this.GetSourceProjectName() =
        this.paramMap.TryFind runParameters.sourceProjectNameKey |> Option.map UMX.tag<projectName>

    member this.GetSourceDbName() =
        this.paramMap.TryFind runParameters.sourceDbNameKey |> Option.map UMX.tag<databaseName>

    member this.GetSourceRunName() =
        this.paramMap.TryFind runParameters.sourceRunNameKey |> Option.map UMX.tag<runName>

    /// Zero-based index in the source run's generation save interval configuration.
    member this.GetSourceGenerationInterval() =
        runParameters.tryGetInt runParameters.sourceGenerationIntervalKey this.paramMap

    member this.GetSourceAdjustCeLengthFrequency() : int<adjustCeLengthFrequency> option =
        runParameters.tryGetInt runParameters.sourceAdjustCeLengthFrequencyKey this.paramMap
        |> Option.map UMX.tag<adjustCeLengthFrequency>

    member this.WithSourceAdjustCeLengthFrequency(value: int<adjustCeLengthFrequency> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceAdjustCeLengthFrequencyKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceCodeModKey() : string<codeModKey> option =
        this.paramMap.TryFind runParameters.sourceCodeModKey
        |> Option.map UMX.tag<codeModKey>

    member this.WithSourceCodeModKey(value: string<codeModKey> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceCodeModKey (value |> Option.map UmxExt.stringToRaw) }

    member this.GetSourceSorterCountPerPool() : int<sorterCountPerPool> option =
        runParameters.tryGetInt runParameters.sourceSorterCountPerPoolKey this.paramMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.WithSourceSorterCountPerPool(value: int<sorterCountPerPool> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSorterCountPerPoolKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceSorterPoolCount() : int<sorterPoolCount> option =
        runParameters.tryGetInt runParameters.sourceSorterPoolCountKey this.paramMap
        |> Option.map UMX.tag<sorterPoolCount>

    member this.WithSourceSorterPoolCount(value: int<sorterPoolCount> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSorterPoolCountKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceParaRate() : float<paraRate> option =
        runParameters.tryGetFloat runParameters.sourceParaRateKey this.paramMap
        |> Option.map UMX.tag<paraRate>

    member this.WithSourceParaRate(value: float<paraRate> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceParaRateKey (value |> Option.map UmxExt.floatToRaw) }

    member this.GetSourceSelfSymRate() : float<selfSymRate> option =
        runParameters.tryGetFloat runParameters.sourceSelfSymRateKey this.paramMap
        |> Option.map UMX.tag<selfSymRate>

    member this.WithSourceSelfSymRate(value: float<selfSymRate> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSelfSymRateKey (value |> Option.map UmxExt.floatToRaw) }

    member this.GetSourceSeedModificationRate() : float<seedModificationRate> option =
        runParameters.tryGetFloat runParameters.sourceSeedModificationRateKey this.paramMap
        |> Option.map UMX.tag<seedModificationRate>

    member this.WithSourceSeedModificationRate(value: float<seedModificationRate> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSeedModificationRateKey (value |> Option.map UmxExt.floatToRaw) }

    member this.GetSourceModificationRate() : float<modificationRate> option =
        runParameters.tryGetFloat runParameters.sourceModificationRateKey this.paramMap
        |> Option.map UMX.tag<modificationRate>

    member this.WithSourceModificationRate(value: float<modificationRate> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceModificationRateKey (value |> Option.map UmxExt.floatToRaw) }

    member this.GetSourceMutationMod() : int<mutationMod> option =
        runParameters.tryGetInt runParameters.sourceMutationModKey this.paramMap
        |> Option.map UMX.tag<mutationMod>

    member this.WithSourceMutationMod(value: int<mutationMod> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceMutationModKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceMutatorVariant() : mutatorVariant option =
        this.paramMap.TryFind runParameters.sourceMutatorVariantKey
        |> Option.map MutatorVariant.fromString

    member this.WithSourceMutatorVariant(value: mutatorVariant option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceMutatorVariantKey (value |> Option.map MutatorVariant.toString) }

    member this.GetSourceSeedSoss() : uint64<randomSeed> option =
        runParameters.tryGetUInt64 runParameters.sourceSeedSossKey this.paramMap
        |> Option.map UMX.tag<randomSeed>

    member this.WithSourceSeedSoss(value: uint64<randomSeed> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSeedSossKey (value |> Option.map UmxExt.uint64ToRaw) }

    member this.GetSourcePartitionCount() : int<partitionCount> option =
        runParameters.tryGetInt runParameters.sourcePartitionCountKey this.paramMap
        |> Option.map UMX.tag<partitionCount>

    member this.WithSourcePartitionCount(value: int<partitionCount> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourcePartitionCountKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceGenerationsPerSubset() : int<generationsPerSubset> option =
        runParameters.tryGetInt runParameters.sourceGenerationsPerSubsetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.WithSourceGenerationsPerSubset(value: int<generationsPerSubset> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceGenerationsPerSubsetKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceGenerationsPerMergedSet() : int<generationsPerSubset> option =
        runParameters.tryGetInt runParameters.sourceGenerationsPerMergedSetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.WithSourceGenerationsPerMergedSet(value: int<generationsPerSubset> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceGenerationsPerMergedSetKey (value |> Option.map UmxExt.intToRaw) }

    member this.GetSourceSorterPoolSetId() : Guid<sorterPoolSetId> option =
        runParameters.tryGetGuid runParameters.sourceSorterPoolSetId this.paramMap
        |> Option.map UMX.tag<sorterPoolSetId>

    member this.WithSourceSorterPoolSetId(value: Guid<sorterPoolSetId> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSorterPoolSetId (value |> Option.map UmxExt.guidToRaw) }

    member this.GetSourceSourceSorterPoolSetId() : Guid<sorterPoolSetId> option =
        runParameters.tryGetGuid runParameters.sourceSourceSorterPoolSetId this.paramMap
        |> Option.map UMX.tag<sorterPoolSetId>

    member this.WithSourceSourceSorterPoolSetId(value: Guid<sorterPoolSetId> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSourceSorterPoolSetId (value |> Option.map UmxExt.guidToRaw) }

    member this.GetSourceSorterPoolId() : Guid<sorterPoolId> option =
        runParameters.tryGetGuid runParameters.sourceSorterPoolId this.paramMap
        |> Option.map UMX.tag<sorterPoolId>

    member this.WithSourceSorterPoolId(value: Guid<sorterPoolId> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSorterPoolId (value |> Option.map UmxExt.guidToRaw) }

    member this.GetSourceSourceSorterPoolId() : Guid<sorterPoolId> option =
        runParameters.tryGetGuid runParameters.sourceSourceSorterPoolId this.paramMap
        |> Option.map UMX.tag<sorterPoolId>

    member this.WithSourceSourceSorterPoolId(value: Guid<sorterPoolId> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceSourceSorterPoolId (value |> Option.map UmxExt.guidToRaw) }

    member this.GetDeletionRate() =
        runParameters.tryGetFloat runParameters.deletionRateKey this.paramMap
        |> Option.map UMX.tag<deletionRate>

    member this.GetDistinctSorterHashes() =
        runParameters.tryGetBool runParameters.distinctSorterHashesKey this.paramMap
        |> Option.map UMX.tag<distinctSorterHashes>

    member this.GetElapsedTime() =
        this.paramMap.TryFind runParameters.elapsedTimeKey

    member this.GetExcludeSelfCe() =
        runParameters.tryGetBool runParameters.excludeSelfCeKey this.paramMap
        |> Option.map UMX.tag<excludeSelfCe>

    member this.GetGenerationCurrent() =
        runParameters.tryGetInt runParameters.generationCurrentKey this.paramMap
        |> Option.map UMX.tag<generationNumber>

    member this.GetGenerationFirst() =
        runParameters.tryGetInt runParameters.generationFirstKey this.paramMap
        |> Option.map UMX.tag<generationNumber>

    member this.GetGenerationIntervalCount() =
        runParameters.tryGetInt runParameters.generationIntervalCountKey this.paramMap
        |> Option.map UMX.tag<generationIntervalCount>

    member this.GetGenerationIntervalLast() =
        runParameters.tryGetInt runParameters.generationIntervalLastKey this.paramMap
        |> Option.map UMX.tag<generationIntervalLast>

    member this.GetId() =
        runParameters.tryGetGuid runParameters.idKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetInsertionRate() =
        runParameters.tryGetFloat runParameters.insertionRateKey this.paramMap
        |> Option.map UMX.tag<insertionRate>

    member this.GetLatticeDistance() =
        runParameters.tryGetInt runParameters.latticeDistanceKey this.paramMap
        |> Option.map UMX.tag<latticeDistance>

    member this.GetMaxOrbit() =
        runParameters.tryGetInt runParameters.maxOrbitKey this.paramMap

    member this.GetMergeDimension() =
        runParameters.tryGetInt runParameters.mergeDimensionKey this.paramMap
        |> Option.map UMX.tag<mergeDimension>

    member this.GetMergeLibId() =
        this.paramMap.TryFind runParameters.mergeLibIdKey
        |> Option.map MergeLibId.fromString

    member this.GetMergeSuffixType() =
        this.paramMap.TryFind runParameters.mergeSuffixTypeKey
        |> Option.bind (fun v ->
            try Some (MergeSuffixType.fromString v)
            with _ -> None)

    member this.GetModificationRate() =
        runParameters.tryGetFloat runParameters.modificationRateKey this.paramMap
        |> Option.map UMX.tag<modificationRate>

    member this.GetMutationMod() =
        runParameters.tryGetInt runParameters.mutationModKey this.paramMap
        |> Option.map UMX.tag<mutationMod>

    member this.GetMutationRate() =
        runParameters.tryGetFloat runParameters.mutationRateKey this.paramMap
        |> Option.map UMX.tag<mutationRate>

    member this.GetMutatorVariant() =
        this.paramMap.TryFind runParameters.mutatorVariantKey
        |> Option.map MutatorVariant.fromString

    member this.GetOrthoRate() =
        runParameters.tryGetFloat runParameters.orthoRateKey this.paramMap
        |> Option.map UMX.tag<orthoRate>

    member this.GetParaRate() =
        runParameters.tryGetFloat runParameters.paraRateKey this.paramMap
        |> Option.map UMX.tag<paraRate>

    member this.GetPartitionCount() : int<GeneSort.Sorting.partitionCount> option =
        runParameters.tryGetInt runParameters.partitionCountKey this.paramMap
        |> Option.map UMX.tag<GeneSort.Sorting.partitionCount>

    member this.GetGenerationsPerSubset() : int<generationsPerSubset> option =
        runParameters.tryGetInt runParameters.generationsPerSubsetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetGenerationsPerMergedSet() : int<generationsPerSubset> option =
        runParameters.tryGetInt runParameters.generationsPerMergedSetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetAdjustCeLengthFrequency() : int<adjustCeLengthFrequency> option =
        runParameters.tryGetInt runParameters.adjustCeLengthFrequencyKey this.paramMap
        |> Option.map UMX.tag<adjustCeLengthFrequency>

    member this.GetPrefixLibId() =
        this.paramMap.TryFind runParameters.prefixLibIdKey
            |> Option.map PrefixLibId.fromString

    member this.GetPrioritizeNewMutants() =
        runParameters.tryGetBool runParameters.prioritizeNewMutantsKey this.paramMap
        |> Option.map UMX.tag<prioritizeNewMutants>

    member this.GetProjectName() =
        this.paramMap.TryFind runParameters.projectNameKey
        |> Option.map UMX.tag<projectName>

    member this.GetQueryName() =
        this.paramMap.TryFind runParameters.queryNameKey
        |> Option.map UMX.tag<queryName>

    member this.GetRepl() =
        runParameters.tryGetInt runParameters.replKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetReplSpan() =
        runParameters.tryGetInt runParameters.replSpanKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetRngType() =
        this.paramMap.TryFind runParameters.rngTypeKey
        |> Option.map RngType.fromString

    member this.GetRunFinished() =
        runParameters.tryGetBool runParameters.runFinishedKey this.paramMap

    member this.GetRunName() =
        this.paramMap.TryFind runParameters.runNameKey
        |> Option.map UMX.tag<runName>

    member this.GetSeedModificationRate() =
        runParameters.tryGetFloat runParameters.seedModificationRateKey this.paramMap
        |> Option.map UMX.tag<seedModificationRate>

    member this.GetSeedSoss() : uint64<randomSeed> option =
        runParameters.tryGetUInt64 runParameters.seedSossKey this.paramMap
        |> Option.map UMX.tag<randomSeed>

    member this.GetSeedSorterPoolSelectionType() =
        this.paramMap.TryFind runParameters.seedSorterPoolSelectionTypeKey
        |> Option.map SorterSelectionType.fromString

    member this.GetSelectedSorterCountPerPool() =
        runParameters.tryGetInt runParameters.selectedSorterCountPerPoolKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSelfSymRate() =
        runParameters.tryGetFloat runParameters.selfSymRateKey this.paramMap
        |> Option.map UMX.tag<selfSymRate>

    member this.GetMutatorParams() =
        this.paramMap.TryFind runParameters.mutatorParamsKey
        |> Option.bind (fun v ->
            match MutatorParams.fromString v with
            | Ok p -> Some p
            | Error _ -> None)

    member this.GetSimpleSorterModelType() =
        this.paramMap.TryFind runParameters.simpleSorterModelTypeKey
        |> Option.bind (fun v ->
            try Some (SimpleSorterModelType.fromString v)
            with _ -> None)

    member this.GetSortableCount() =
        runParameters.tryGetInt runParameters.sortableCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSortableDataFormat() =
        this.paramMap.TryFind runParameters.sortableDataFormatKey
        |> Option.bind (fun v ->
            try Some (SortableDataFormat.fromString v)
            with _ -> None)

    member this.GetSortedFraction() =
        runParameters.tryGetFloat runParameters.sortedFractionKey this.paramMap
        |> Option.map UMX.tag<sortedFraction>

    member this.GetSorterChildCount() =
        runParameters.tryGetInt runParameters.sorterChildCountKey this.paramMap
        |> Option.map UMX.tag<sorterChildCount>

    member this.GetSorterCount() =
        runParameters.tryGetInt runParameters.sorterCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSorterCountCycle() =
        runParameters.tryGetInt runParameters.sorterCountCycleKey this.paramMap
        |> Option.map UMX.tag<sorterCountCycle>

    member this.GetSorterCountCycleMultiplier() =
        runParameters.tryGetFloat runParameters.sorterCountCycleMultiplierKey this.paramMap
        |> Option.map UMX.tag<sorterCountCycleMultiplier>

    member this.GetSorterCountPerPool() =
        runParameters.tryGetInt runParameters.sorterCountPerPoolKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSorterCountPerPoolSet() =
        runParameters.tryGetInt runParameters.sorterCountPerPoolSetKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPoolSet>

    member this.GetSorterEvalMeasure() =
        this.paramMap.TryFind runParameters.sorterEvalMeasureKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterEvalMeasureInitial() =
        this.paramMap.TryFind runParameters.sorterEvalMeasureInitialKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterEvalType() =
        this.paramMap.TryFind runParameters.sorterEvalTypeKey
        |> Option.map SorterEvalType.fromString

    member this.GetSorterLibId() =
        this.paramMap.TryFind runParameters.sorterLibIdKey
        |> Option.map SorterLibId.fromString

    member this.GetSorterLibVariant() =
        this.paramMap.TryFind runParameters.sorterLibVariantKey
        |> Option.bind (fun v ->
            try Some (SorterLibVariant.fromString v)
            with _ -> None)

    member this.GetSorterParentCount() =
        runParameters.tryGetInt runParameters.sorterParentCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSorterPoolCount() =
        runParameters.tryGetInt runParameters.sorterPoolCountKey this.paramMap
        |> Option.map UMX.tag<sorterPoolCount>

    member this.GetSorterPoolExpansionRate() =
        runParameters.tryGetInt runParameters.sorterPoolExpansionRateKey this.paramMap
        |> Option.map UMX.tag<sorterPoolExpansionRate>

    member this.GetSorterPoolMeasure() =
        this.paramMap.TryFind runParameters.sorterPoolMeasureKey
        |> Option.map SorterPoolMeasure.fromCompactString

    member this.GetSorterPoolSelectionIntervals() =
        this.paramMap.TryFind runParameters.sorterPoolSelectionIntervalsKey
        |> Option.map (fun v -> GenIntervalConfig.fromString(v))

    member this.GetSortingWidth() =
        runParameters.tryGetInt runParameters.sortingWidthKey this.paramMap
        |> Option.map UMX.tag<sortingWidth>

    member this.GetStageLength() =
        runParameters.tryGetInt runParameters.stageLengthKey this.paramMap
        |> Option.map UMX.tag<stageLength>

    member this.GetStartingRepl() =
        runParameters.tryGetInt runParameters.startingReplKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetTextReportName() =
        this.paramMap.TryFind runParameters.textReportNameKey
        |> Option.map UMX.tag<textReportName>

    // ============================================================
    // With* members
    // ============================================================

    member this.WithCodeModKey(dbn: string<codeModKey> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.codeModKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithCeLength(cl: int<ceLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.ceLengthKey (cl |> Option.map UmxExt.intToRaw) }

    member this.WithCollectNewSortableTests(cst: bool<collectNewSortableTests> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.collectSortableTestsKey (cst |> Option.map string) }

    member this.WithDatabaseName(dbn: string<databaseName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.databaseNameKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceProjectName(value: string<projectName> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceProjectNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceDbName(value: string<databaseName> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceDbNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceRunName(value: string<runName> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceRunNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceGenerationInterval(value: int option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sourceGenerationIntervalKey (value |> Option.map string) }

    member this.WithDeletionRate(dr: float<deletionRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.deletionRateKey (dr |> Option.map UmxExt.floatToRaw) }

    member this.WithDistinctSorterHashes(dsh: bool option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.distinctSorterHashesKey (dsh |> Option.map string) }

    member this.WithElapsedTime(time: string option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.elapsedTimeKey time }

    member this.WithExcludeSelfCe(esc: bool<excludeSelfCe> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.excludeSelfCeKey (esc |> Option.map UmxExt.boolToRaw) }

    member this.WithGenerationCurrent(gen: int<generationNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationCurrentKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationFirst(gen: int<generationNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationFirstKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationIntervalCount(count: int<generationIntervalCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationIntervalCountKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationIntervalLast(last: int<generationIntervalLast> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationIntervalLastKey (last |> Option.map UmxExt.intToRaw) }

    member this.WithId(id: Guid<queryParamsId> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.idKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithInsertionRate(ir: float<insertionRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.insertionRateKey (ir |> Option.map UmxExt.floatToRaw) }

    member this.WithLatticeDistance(ld: int<latticeDistance> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.latticeDistanceKey (ld |> Option.map UmxExt.intToRaw) }

    member this.WithMaxOrbit(mo: int option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.maxOrbitKey (mo |> Option.map string) }

    member this.WithMergeDimension(md: int<mergeDimension> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeDimensionKey (md |> Option.map UmxExt.intToRaw) }

    member this.WithMergeLibId(mlid: mergeLibId option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeLibIdKey (mlid |> Option.map MergeLibId.toString) }

    member this.WithMergeSuffixType(mft: mergeSuffixType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeSuffixTypeKey (mft |> Option.map MergeSuffixType.toString) }

    member this.WithMessage(message: string option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.messageKey message }

    member this.WithModificationRate(mr: float<modificationRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.modificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutationMod(mm: int<mutationMod> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mutationModKey (mm |> Option.map UmxExt.intToRaw) }

    member this.WithMutationRate(mr: float<mutationRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mutationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutatorParams(mtp: mutatorParams option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mutatorParamsKey (mtp |> Option.map MutatorParams.toString) }

    member this.WithMutatorVariant(mutv: mutatorVariant option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mutatorVariantKey (mutv |> Option.map MutatorVariant.toString) }

    member this.WithOrthoRate(ortho: float<orthoRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.orthoRateKey (ortho |> Option.map UmxExt.floatToRaw) }

    member this.WithParaRate(para: float<paraRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.paraRateKey (para |> Option.map UmxExt.floatToRaw) }

    member this.WithPartitionCount(count: int<partitionCount> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.partitionCountKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationsPerSubset(count: int<generationsPerSubset> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationsPerSubsetKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationsPerMergedSet(count: int<generationsPerSubset> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationsPerMergedSetKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithAdjustCeLengthFrequency(count: int<adjustCeLengthFrequency> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.adjustCeLengthFrequencyKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithPrefixLibId(pfxLibId: prefixLibId option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.prefixLibIdKey (pfxLibId |> Option.map PrefixLibId.toString) }

    member this.WithPrioritizeNewMutants(pnm: bool option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.prioritizeNewMutantsKey (pnm |> Option.map string) }

    member this.WithProjectName(pn: string<projectName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.projectNameKey (pn |> Option.map UmxExt.stringToRaw) }

    member this.WithQueryName(qn: string<queryName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.queryNameKey (qn |> Option.map UmxExt.stringToRaw) }

    member this.WithRepl(repl: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.replKey (repl |> Option.map UmxExt.intToRaw) }

    member this.WithReplSpan(rs: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.replSpanKey (rs |> Option.map UmxExt.intToRaw) }

    member this.WithRngType(rng: rngType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.rngTypeKey (rng |> Option.map RngType.toString) }

    member this.WithRunFinished(fin: bool option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.runFinishedKey (fin |> Option.map string) }

    member this.WithRunName(rn: string<runName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.runNameKey (rn |> Option.map UmxExt.stringToRaw) }

    member this.WithSeedModificationRate(mr: float<seedModificationRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.seedModificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithSeedSoss(seed: uint64<randomSeed> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.seedSossKey (seed |> Option.map UmxExt.uint64ToRaw) } 

    member this.WithSeedSorterPoolSelectionType(ses: sorterSelectionType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.seedSorterPoolSelectionTypeKey (ses |> Option.map SorterSelectionType.toString) }

    member this.WithSelectedSorterCountPerPool(sc: int<sorterCountPerPool> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.selectedSorterCountPerPoolKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSelfSymRate(ssr: float<selfSymRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.selfSymRateKey (ssr |> Option.map UmxExt.floatToRaw) }

    member this.WithSimpleSorterModelType(smt: simpleSorterModelType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.simpleSorterModelTypeKey (smt |> Option.map SimpleSorterModelType.toString) }

    member this.WithSortableCount(sc: int<sortableCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortableCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSortableDataFormat(sdt: sortableDataFormat option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortableDataFormatKey (sdt |> Option.map SortableDataFormat.toString) }

    member this.WithSortedFraction(sf: float<sortedFraction> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortedFractionKey (sf |> Option.map UmxExt.floatToRaw) }

    member this.WithSorterChildCount(scc: int<sorterChildCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterChildCountKey (scc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCount(sc: int<sorterCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountCycle(scc: int<sorterCountCycle> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountCycleKey (scc |> Option.map UmxExt.intToRaw) }
        
    member this.WithSorterCountCycleMultiplier(scc: float<sorterCountCycleMultiplier> option) =
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountCycleMultiplierKey (scc |> Option.map UmxExt.floatToRaw) }

    member this.WithSorterCountPerPool(sc: int<sorterCountPerPool> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountPerPoolKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountPerPoolSet(sc: int<sorterCountPerPoolSet> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountPerPoolSetKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterEvalMeasure(sem: sorterEvalMeasure option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterEvalMeasureKey (sem |> Option.map SorterEvalFunctions.toCompactString) }

    member this.WithSorterEvalMeasureInitial(sem: sorterEvalMeasure option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterEvalMeasureInitialKey (sem |> Option.map SorterEvalFunctions.toCompactString) }

    member this.WithSorterEvalType(set: sorterEvalType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterEvalTypeKey (set |> Option.map SorterEvalType.toString) }

    member this.WithSorterLibId(slId: sorterLibId option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterLibIdKey (slId |> Option.map SorterLibId.toString) }

    member this.WithSorterLibVariant(slv: sorterLibVariant option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterLibVariantKey (slv |> Option.map SorterLibVariant.toString) }

    member this.WithSorterParentCount(spc: int<sorterCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterParentCountKey (spc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolCount(sc: int<sorterPoolCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterPoolCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolExpansionRate(sper: int<sorterPoolExpansionRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterPoolExpansionRateKey (sper |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolMeasure(spm: sorterPoolMeasure option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterPoolMeasureKey (spm |> Option.map SorterPoolMeasure.toCompactString) }

    member this.WithSorterPoolSelectionIntervals(sConfig: genIntervalConfig option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterPoolSelectionIntervalsKey (sConfig |> Option.map GenIntervalConfig.toString) }

    member this.WithSortingWidth(w: int<sortingWidth> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortingWidthKey (w |> Option.map UmxExt.intToRaw) }

    member this.WithStageLength(sl: int<stageLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.stageLengthKey (sl |> Option.map UmxExt.intToRaw) }

    member this.WithStartingRepl(sr: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.startingReplKey (sr |> Option.map UmxExt.intToRaw) }

    member this.WithTextReportName(trn: string<textReportName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.textReportNameKey (trn |> Option.map UmxExt.stringToRaw) }

// --- 3. The Logic Module ---
module RunParameters =

    /// Filters a set of parameters based on a list of key-value pairs.
    let filterByParameters (runParametersSet: runParameters array) (filter: (string * string) array) =
        runParametersSet
        |> Array.filter (fun rp ->
            filter |> Array.forall (fun (k, v) -> rp.ParamMap.TryFind k = Some v))

    let pickByParameters (runParametersSet: runParameters array) (filter: (string * string) array) =
        let results = filterByParameters runParametersSet filter
        if results.Length = 1 then Some results.[0] else None

    let getAllKeys (runParams: runParameters seq) =
        runParams |> Seq.collect (fun rp -> rp.ParamMap.Keys) |> Seq.distinct |> Seq.toArray

    let reportKvps (runParameters: runParameters) : string =
        let concats =
            runParameters.ParamMap.Keys 
            |> Seq.map(fun k -> sprintf "\t%s: %s" k (runParameters.ParamMap.[k])) 
            |> String.concat "\n"

        concats + "\n"
        

    /// Generates a structured table where rows represent individual runs and columns represent parameter keys.
    let makeIndexAndReplTable (runParams: runParameters seq) : string[][] =
        let keys = getAllKeys runParams
        let headerRow = Array.append [| "Run_Index" |] keys
        let dataRows =
            runParams
            |> Seq.mapi (fun i rp ->
                Array.append 
                    [| string i |] 
                    (keys |> Array.map (fun k -> rp.ParamMap.TryFind k |> Option.defaultValue "N/A")))
            |> Seq.toArray
        Array.append [| headerRow |] dataRows

    let toStringTable (runParams: runParameters seq) : string =
        makeIndexAndReplTable runParams
        |> Array.map (String.concat "\t")
        |> String.concat "\n\n"

    let getIdString (runParameters:runParameters) =
        runParameters.GetId() |> UmxExt.guidOptionToString
