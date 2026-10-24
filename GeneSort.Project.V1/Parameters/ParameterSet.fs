namespace GeneSort.Project.V1.Parameters

open System
open System.Globalization
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.ParamBundles

/// Shared immutable parameters; the kind keeps setup and run values distinct.
type parameterSet<'kind> =
    private { paramMap : Map<string, string> }

    static member create (paramMap: Map<string, string>) : parameterSet<'kind> =
        { paramMap = paramMap }



    member this.ParamMap with get() : Map<string,string> = this.paramMap

    // ============================================================
    // Getters
    // ============================================================

    member this.GetAdjustCeLengthFrequency() : int<adjustCeLengthFrequency> option =
        ParamKeys.tryGetInt ParamKeys.adjustCeLengthFrequencyKey this.paramMap
        |> Option.map UMX.tag<adjustCeLengthFrequency>

    member this.GetCeLength() =
        ParamKeys.tryGetInt ParamKeys.ceLengthKey this.paramMap
        |> Option.map UMX.tag<ceLength>

    member this.GetCodeModKey() =
        this.paramMap.TryFind ParamKeys.codeModKey
        |> Option.map UMX.tag<codeModKey>

    member this.GetCollectNewSortableTests() =
        ParamKeys.tryGetBool ParamKeys.collectSortableTestsKey this.paramMap
        |> Option.map UMX.tag<collectNewSortableTests>

    member this.GetDatabaseName() =
        this.paramMap.TryFind ParamKeys.databaseNameKey
        |> Option.map UMX.tag<databaseName>

    member this.GetDeletionRate() =
        ParamKeys.tryGetFloat ParamKeys.deletionRateKey this.paramMap
        |> Option.map UMX.tag<deletionRate>

    member this.GetDistinctSorterHashes() =
        ParamKeys.tryGetBool ParamKeys.distinctSorterHashesKey this.paramMap
        |> Option.map UMX.tag<distinctSorterHashes>

    member this.GetElapsedTime() =
        this.paramMap.TryFind ParamKeys.elapsedTimeKey

    member this.GetExcludeSelfCe() =
        ParamKeys.tryGetBool ParamKeys.excludeSelfCeKey this.paramMap
        |> Option.map UMX.tag<excludeSelfCe>

    member this.GetGenerationCurrent() =
        ParamKeys.tryGetInt ParamKeys.generationCurrentKey this.paramMap
        |> Option.map UMX.tag<generationNumber>

    member this.GetGenerationFirst() =
        ParamKeys.tryGetInt ParamKeys.generationFirstKey this.paramMap
        |> Option.map UMX.tag<generationNumber>

    member this.GetGenerationIntervalCount() =
        ParamKeys.tryGetInt ParamKeys.generationIntervalCountKey this.paramMap
        |> Option.map UMX.tag<generationIntervalCount>

    member this.GetGenerationIntervalLast() =
        ParamKeys.tryGetInt ParamKeys.generationIntervalLastKey this.paramMap
        |> Option.map UMX.tag<generationIntervalLast>

    member this.GetGenerationsPerMergedSet() : int<generationsPerSubset> option =
        ParamKeys.tryGetInt ParamKeys.generationsPerMergedSetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetGenerationsPerSubset() : int<generationsPerSubset> option =
        ParamKeys.tryGetInt ParamKeys.generationsPerSubsetKey this.paramMap
        |> Option.map UMX.tag<generationsPerSubset>

    member this.GetId() =
        ParamKeys.tryGetGuid ParamKeys.idKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetInsertionRate() =
        ParamKeys.tryGetFloat ParamKeys.insertionRateKey this.paramMap
        |> Option.map UMX.tag<insertionRate>

    member this.GetLatticeDistance() =
        ParamKeys.tryGetInt ParamKeys.latticeDistanceKey this.paramMap
        |> Option.map UMX.tag<latticeDistance>

    member this.GetMaxOrbit() =
        ParamKeys.tryGetInt ParamKeys.maxOrbitKey this.paramMap

    member this.GetMergeDimension() =
        ParamKeys.tryGetInt ParamKeys.mergeDimensionKey this.paramMap
        |> Option.map UMX.tag<mergeDimension>

    member this.GetMergeLibId() =
        this.paramMap.TryFind ParamKeys.mergeLibIdKey
        |> Option.map MergeLibId.fromString

    member this.GetMergeSuffixType() =
        this.paramMap.TryFind ParamKeys.mergeSuffixTypeKey
        |> Option.bind (fun v ->
            try Some (MergeSuffixType.fromString v)
            with _ -> None)

    member this.GetModificationRate() =
        ParamKeys.tryGetFloat ParamKeys.modificationRateKey this.paramMap
        |> Option.map UMX.tag<modificationRate>

    member this.GetMutationMod() =
        ParamKeys.tryGetInt ParamKeys.mutationModKey this.paramMap
        |> Option.map UMX.tag<mutationMod>

    member this.GetMutationRate() =
        ParamKeys.tryGetFloat ParamKeys.mutationRateKey this.paramMap
        |> Option.map UMX.tag<mutationRate>

    member this.GetMutatorParams() =
        this.paramMap.TryFind ParamKeys.mutatorParamsKey
        |> Option.bind (fun v ->
            match MutatorParams.fromString v with
            | Ok p -> Some p
            | Error _ -> None)

    member this.GetMutatorVariant() =
        this.paramMap.TryFind ParamKeys.mutatorVariantKey
        |> Option.map MutatorVariant.fromString

    member this.GetOrthoRate() =
        ParamKeys.tryGetFloat ParamKeys.orthoRateKey this.paramMap
        |> Option.map UMX.tag<orthoRate>

    member this.GetParaRate() =
        ParamKeys.tryGetFloat ParamKeys.paraRateKey this.paramMap
        |> Option.map UMX.tag<paraRate>

    member this.GetPartitionCount() : int<GeneSort.Sorting.partitionCount> option =
        ParamKeys.tryGetInt ParamKeys.partitionCountKey this.paramMap
        |> Option.map UMX.tag<GeneSort.Sorting.partitionCount>

    member this.GetPrefixLibId() =
        this.paramMap.TryFind ParamKeys.prefixLibIdKey
            |> Option.map PrefixLibId.fromString

    member this.GetPrioritizeNewMutants() =
        ParamKeys.tryGetBool ParamKeys.prioritizeNewMutantsKey this.paramMap
        |> Option.map UMX.tag<prioritizeNewMutants>

    member this.GetProjectName() =
        this.paramMap.TryFind ParamKeys.projectNameKey
        |> Option.map UMX.tag<projectName>

    member this.GetQueryName() =
        this.paramMap.TryFind ParamKeys.queryNameKey
        |> Option.map UMX.tag<queryName>

    member this.GetRepl() =
        ParamKeys.tryGetInt ParamKeys.replKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetReplSpan() =
        ParamKeys.tryGetInt ParamKeys.replSpanKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetRngType() =
        this.paramMap.TryFind ParamKeys.rngTypeKey
        |> Option.map RngType.fromString

    member this.GetRunFinished() =
        ParamKeys.tryGetBool ParamKeys.runFinishedKey this.paramMap

    member this.GetRunName() =
        this.paramMap.TryFind ParamKeys.runNameKey
        |> Option.map UMX.tag<runName>

    member this.GetSeedModificationRate() =
        ParamKeys.tryGetFloat ParamKeys.seedModificationRateKey this.paramMap
        |> Option.map UMX.tag<seedModificationRate>

    member this.GetSeedSorterPoolSelectionType() =
        this.paramMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey
        |> Option.map SorterSelectionType.fromString

    member this.GetSeedSoss() : uint64<randomSeed> option =
        ParamKeys.tryGetUInt64 ParamKeys.seedSossKey this.paramMap
        |> Option.map UMX.tag<randomSeed>

    member this.GetSelectedSorterCountPerPool() =
        ParamKeys.tryGetInt ParamKeys.selectedSorterCountPerPoolKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSelectedSorterPoolCount() : int<sorterPoolCount> option =
        ParamKeys.tryGetInt ParamKeys.selectedSorterPoolCountKey this.paramMap
        |> Option.map UMX.tag<sorterPoolCount>

    member this.GetSelfSymRate() =
        ParamKeys.tryGetFloat ParamKeys.selfSymRateKey this.paramMap
        |> Option.map UMX.tag<selfSymRate>

    member this.GetSetupParametersId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupParametersIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSortableTestsId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSortableTestsIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSortableTestsWithPrefixId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSortableTestsWithPrefixIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterModelSetId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterModelSetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterPoolBinsSetSeriesId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterPoolBinsSetSeriesIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterPoolSetHistoryId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterPoolSetHistoryIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterPoolSetId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterPoolSetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterPoolSetSummarySetId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterPoolSetSummarySetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterSetEvalId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterSetEvalIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSetupSorterSetId() : Guid<queryParamsId> option =
        ParamKeys.tryGetGuid ParamKeys.setupSorterSetIdKey this.paramMap
        |> Option.map UMX.tag<queryParamsId>

    member this.GetSimpleSorterModelType() =
        this.paramMap.TryFind ParamKeys.simpleSorterModelTypeKey
        |> Option.bind (fun v ->
            try Some (SimpleSorterModelType.fromString v)
            with _ -> None)

    member this.GetSortableCount() =
        ParamKeys.tryGetInt ParamKeys.sortableCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSortableDataFormat() =
        this.paramMap.TryFind ParamKeys.sortableDataFormatKey
        |> Option.bind (fun v ->
            try Some (SortableDataFormat.fromString v)
            with _ -> None)

    member this.GetSortedFraction() =
        ParamKeys.tryGetFloat ParamKeys.sortedFractionKey this.paramMap
        |> Option.map UMX.tag<sortedFraction>

    member this.GetSorterChildCount() =
        ParamKeys.tryGetInt ParamKeys.sorterChildCountKey this.paramMap
        |> Option.map UMX.tag<sorterChildCount>

    member this.GetSorterCount() =
        ParamKeys.tryGetInt ParamKeys.sorterCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSorterCountCycle() =
        ParamKeys.tryGetInt ParamKeys.sorterCountCycleKey this.paramMap
        |> Option.map UMX.tag<sorterCountCycle>

    member this.GetSorterCountCycleMultiplier() =
        ParamKeys.tryGetFloat ParamKeys.sorterCountCycleMultiplierKey this.paramMap
        |> Option.map UMX.tag<sorterCountCycleMultiplier>

    member this.GetSorterCountPerPool() =
        ParamKeys.tryGetInt ParamKeys.sorterCountPerPoolKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPool>

    member this.GetSorterCountPerPoolSet() =
        ParamKeys.tryGetInt ParamKeys.sorterCountPerPoolSetKey this.ParamMap
        |> Option.map UMX.tag<sorterCountPerPoolSet>

    member this.GetSorterEvalMeasure() =
        this.paramMap.TryFind ParamKeys.sorterEvalMeasureKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterEvalMeasureInitial() =
        this.paramMap.TryFind ParamKeys.sorterEvalMeasureInitialKey
        |> Option.map SorterEvalFunctions.fromCompactString

    member this.GetSorterEvalType() =
        this.paramMap.TryFind ParamKeys.sorterEvalTypeKey
        |> Option.map SorterEvalType.fromString

    member this.GetSorterLibId() =
        this.paramMap.TryFind ParamKeys.sorterLibIdKey
        |> Option.map SorterLibId.fromString

    member this.GetSorterLibVariant() =
        this.paramMap.TryFind ParamKeys.sorterLibVariantKey
        |> Option.bind (fun v ->
            try Some (SorterLibVariant.fromString v)
            with _ -> None)

    member this.GetSorterParentCount() =
        ParamKeys.tryGetInt ParamKeys.sorterParentCountKey this.paramMap
        |> Option.map UMX.tag<sorterCount>

    member this.GetSorterPoolCount() =
        ParamKeys.tryGetInt ParamKeys.sorterPoolCountKey this.paramMap
        |> Option.map UMX.tag<sorterPoolCount>

    member this.GetSorterPoolExpansionRate() =
        ParamKeys.tryGetInt ParamKeys.sorterPoolExpansionRateKey this.paramMap
        |> Option.map UMX.tag<sorterPoolExpansionRate>

    member this.GetSorterPoolMeasure() =
        this.paramMap.TryFind ParamKeys.sorterPoolMeasureKey
        |> Option.map SorterPoolMeasure.fromCompactString

    member this.GetSorterPoolSelectionIntervals() =
        this.paramMap.TryFind ParamKeys.sorterPoolSelectionIntervalsKey
        |> Option.map (fun v -> GenIntervalConfig.fromString(v))

    member this.GetSortingWidth() =
        ParamKeys.tryGetInt ParamKeys.sortingWidthKey this.paramMap
        |> Option.map UMX.tag<sortingWidth>

    member this.GetSourceDbName() =
        this.paramMap.TryFind ParamKeys.sourceDbNameKey |> Option.map UMX.tag<databaseName>

    /// Zero-based index in the source run's generation save interval configuration.
    member this.GetSourceGenerationInterval() =
        ParamKeys.tryGetInt ParamKeys.sourceGenerationIntervalKey this.paramMap

    member this.GetSourceProjectName() =
        this.paramMap.TryFind ParamKeys.sourceProjectNameKey |> Option.map UMX.tag<projectName>

    member this.GetSourceRunName() =
        this.paramMap.TryFind ParamKeys.sourceRunNameKey |> Option.map UMX.tag<runName>

    member this.GetSourceMutationMod() =
        ParamKeys.tryGetInt ParamKeys.sourceMutationModKey this.paramMap |> Option.map UMX.tag<mutationMod>

    member this.GetStageLength() =
        ParamKeys.tryGetInt ParamKeys.stageLengthKey this.paramMap
        |> Option.map UMX.tag<stageLength>

    member this.GetStartingRepl() =
        ParamKeys.tryGetInt ParamKeys.startingReplKey this.paramMap
        |> Option.map UMX.tag<replNumber>

    member this.GetTextReportName() =
        this.paramMap.TryFind ParamKeys.textReportNameKey
        |> Option.map UMX.tag<textReportName>

    // ============================================================
    // With* members
    // ============================================================

    member this.WithAdjustCeLengthFrequency(count: int<adjustCeLengthFrequency> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.adjustCeLengthFrequencyKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithCeLength(cl: int<ceLength> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.ceLengthKey (cl |> Option.map UmxExt.intToRaw) }

    member this.WithCodeModKey(dbn: string<codeModKey> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.codeModKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithCollectNewSortableTests(cst: bool<collectNewSortableTests> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.collectSortableTestsKey (cst |> Option.map string) }

    member this.WithDatabaseName(dbn: string<databaseName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.databaseNameKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithDeletionRate(dr: float<deletionRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.deletionRateKey (dr |> Option.map UmxExt.floatToRaw) }

    member this.WithDistinctSorterHashes(dsh: bool option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.distinctSorterHashesKey (dsh |> Option.map string) }

    member this.WithElapsedTime(time: string option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.elapsedTimeKey time }

    member this.WithExcludeSelfCe(esc: bool<excludeSelfCe> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.excludeSelfCeKey (esc |> Option.map UmxExt.boolToRaw) }

    member this.WithGenerationCurrent(gen: int<generationNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.generationCurrentKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationFirst(gen: int<generationNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.generationFirstKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationIntervalCount(count: int<generationIntervalCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.generationIntervalCountKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationIntervalLast(last: int<generationIntervalLast> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.generationIntervalLastKey (last |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationsPerMergedSet(count: int<generationsPerSubset> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.generationsPerMergedSetKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithGenerationsPerSubset(count: int<generationsPerSubset> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.generationsPerSubsetKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.idKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithInsertionRate(ir: float<insertionRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.insertionRateKey (ir |> Option.map UmxExt.floatToRaw) }

    member this.WithLatticeDistance(ld: int<latticeDistance> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.latticeDistanceKey (ld |> Option.map UmxExt.intToRaw) }

    member this.WithMaxOrbit(mo: int option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.maxOrbitKey (mo |> Option.map string) }

    member this.WithMergeDimension(md: int<mergeDimension> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mergeDimensionKey (md |> Option.map UmxExt.intToRaw) }

    member this.WithMergeLibId(mlid: mergeLibId option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mergeLibIdKey (mlid |> Option.map MergeLibId.toString) }

    member this.WithMergeSuffixType(mft: mergeSuffixType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mergeSuffixTypeKey (mft |> Option.map MergeSuffixType.toString) }

    member this.WithMessage(message: string option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.messageKey message }

    member this.WithModificationRate(mr: float<modificationRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.modificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutationMod(mm: int<mutationMod> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mutationModKey (mm |> Option.map UmxExt.intToRaw) }

    member this.WithMutationRate(mr: float<mutationRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mutationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutatorParams(mtp: mutatorParams option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mutatorParamsKey (mtp |> Option.map MutatorParams.toString) }

    member this.WithMutatorVariant(mutv: mutatorVariant option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.mutatorVariantKey (mutv |> Option.map MutatorVariant.toString) }

    member this.WithOrthoRate(ortho: float<orthoRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.orthoRateKey (ortho |> Option.map UmxExt.floatToRaw) }

    member this.WithParaRate(para: float<paraRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.paraRateKey (para |> Option.map UmxExt.floatToRaw) }

    member this.WithPartitionCount(count: int<partitionCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.partitionCountKey (count |> Option.map UmxExt.intToRaw) }

    member this.WithPrefixLibId(pfxLibId: prefixLibId option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.prefixLibIdKey (pfxLibId |> Option.map PrefixLibId.toString) }

    member this.WithPrioritizeNewMutants(pnm: bool option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.prioritizeNewMutantsKey (pnm |> Option.map string) }

    member this.WithProjectName(pn: string<projectName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.projectNameKey (pn |> Option.map UmxExt.stringToRaw) }

    member this.WithQueryName(qn: string<queryName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.queryNameKey (qn |> Option.map UmxExt.stringToRaw) }

    member this.WithRepl(repl: int<replNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.replKey (repl |> Option.map UmxExt.intToRaw) }

    member this.WithReplSpan(rs: int<replNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.replSpanKey (rs |> Option.map UmxExt.intToRaw) }

    member this.WithRngType(rng: rngType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.rngTypeKey (rng |> Option.map RngType.toString) }

    member this.WithRunFinished(fin: bool option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.runFinishedKey (fin |> Option.map string) }

    member this.WithRunName(rn: string<runName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.runNameKey (rn |> Option.map UmxExt.stringToRaw) }

    member this.WithSeedModificationRate(mr: float<seedModificationRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.seedModificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithSeedSorterPoolSelectionType(ses: sorterSelectionType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.seedSorterPoolSelectionTypeKey (ses |> Option.map SorterSelectionType.toString) }

    member this.WithSeedSoss(seed: uint64<randomSeed> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.seedSossKey (seed |> Option.map UmxExt.uint64ToRaw) }

    member this.WithSelectedSorterCountPerPool(sc: int<sorterCountPerPool> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.selectedSorterCountPerPoolKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSelectedSorterPoolCount(sc: int<sorterPoolCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.selectedSorterPoolCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSelfSymRate(ssr: float<selfSymRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.selfSymRateKey (ssr |> Option.map UmxExt.floatToRaw) }

    member this.WithSetupParametersId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupParametersIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSortableTestsId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSortableTestsIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSortableTestsWithPrefixId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSortableTestsWithPrefixIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterModelSetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterModelSetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterPoolBinsSetSeriesId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterPoolBinsSetSeriesIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterPoolSetHistoryId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterPoolSetHistoryIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterPoolSetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterPoolSetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterPoolSetSummarySetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterPoolSetSummarySetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterSetEvalId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterSetEvalIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSetupSorterSetId(id: Guid<queryParamsId> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.setupSorterSetIdKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithSimpleSorterModelType(smt: simpleSorterModelType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.simpleSorterModelTypeKey (smt |> Option.map SimpleSorterModelType.toString) }

    member this.WithSortableCount(sc: int<sortableCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sortableCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSortableDataFormat(sdt: sortableDataFormat option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sortableDataFormatKey (sdt |> Option.map SortableDataFormat.toString) }

    member this.WithSortedFraction(sf: float<sortedFraction> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sortedFractionKey (sf |> Option.map UmxExt.floatToRaw) }

    member this.WithSorterChildCount(scc: int<sorterChildCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterChildCountKey (scc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCount(sc: int<sorterCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountCycle(scc: int<sorterCountCycle> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterCountCycleKey (scc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountCycleMultiplier(scc: float<sorterCountCycleMultiplier> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterCountCycleMultiplierKey (scc |> Option.map UmxExt.floatToRaw) }

    member this.WithSorterCountPerPool(sc: int<sorterCountPerPool> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterCountPerPoolKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterCountPerPoolSet(sc: int<sorterCountPerPoolSet> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterCountPerPoolSetKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterEvalMeasure(sem: sorterEvalMeasure option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterEvalMeasureKey (sem |> Option.map SorterEvalFunctions.toCompactString) }

    member this.WithSorterEvalMeasureInitial(sem: sorterEvalMeasure option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterEvalMeasureInitialKey (sem |> Option.map SorterEvalFunctions.toCompactString) }

    member this.WithSorterEvalType(set: sorterEvalType option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterEvalTypeKey (set |> Option.map SorterEvalType.toString) }

    member this.WithSorterLibId(slId: sorterLibId option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterLibIdKey (slId |> Option.map SorterLibId.toString) }

    member this.WithSorterLibVariant(slv: sorterLibVariant option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterLibVariantKey (slv |> Option.map SorterLibVariant.toString) }

    member this.WithSorterParentCount(spc: int<sorterCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterParentCountKey (spc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolCount(sc: int<sorterPoolCount> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterPoolCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolExpansionRate(sper: int<sorterPoolExpansionRate> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterPoolExpansionRateKey (sper |> Option.map UmxExt.intToRaw) }

    member this.WithSorterPoolMeasure(spm: sorterPoolMeasure option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterPoolMeasureKey (spm |> Option.map SorterPoolMeasure.toCompactString) }

    member this.WithSorterPoolSelectionIntervals(sConfig: genIntervalConfig option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sorterPoolSelectionIntervalsKey (sConfig |> Option.map GenIntervalConfig.toString) }

    member this.WithSortingWidth(w: int<sortingWidth> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sortingWidthKey (w |> Option.map UmxExt.intToRaw) }

    member this.WithSourceDbName(value: string<databaseName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sourceDbNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceGenerationInterval(value: int option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sourceGenerationIntervalKey (value |> Option.map string) }

    member this.WithSourceProjectName(value: string<projectName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sourceProjectNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceRunName(value: string<runName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sourceRunNameKey (value |> Option.map UmxExt.stringToRaw) }

    member this.WithSourceMutationMod(value: int<mutationMod> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.sourceMutationModKey (value |> Option.map UmxExt.intToRaw) }

    member this.WithStageLength(sl: int<stageLength> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.stageLengthKey (sl |> Option.map UmxExt.intToRaw) }

    member this.WithStartingRepl(sr: int<replNumber> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.startingReplKey (sr |> Option.map UmxExt.intToRaw) }

    member this.WithTextReportName(trn: string<textReportName> option) : parameterSet<'kind> =
        { paramMap = this.paramMap |> ParamKeys.addOrRemove ParamKeys.textReportNameKey (trn |> Option.map UmxExt.stringToRaw) }
