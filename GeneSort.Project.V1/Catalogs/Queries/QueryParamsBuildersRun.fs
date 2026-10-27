namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters

/// Execution file identities. Setup inputs use their separate Setup constructors and saved references.
module QueryParamsBuildersRun =
    /// Include persisted Setup references explicitly when they belong to this identity.
    let withSetupReferences (rp: runParameters) (query: queryParams) =
        let properties = SetupFileReferences.fromRunParameters rp |> List.fold (fun map (key, value) -> Map.add key value map) query.Properties
        if properties = query.Properties then query
        else queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties)


    module SorterSgd =

        module Uf6MutationRate =
            /// Constructs only the Run address for SorterSgd.Uf6MutationRate.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.Uf6MutationRate.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.Uf6MutationRate.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.Uf6MutationRate.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.Uf6MutationRate.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Uf6MutationRate.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterSgd.Uf6MutationRate.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.Uf6MutationRate.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module MsrsOrthoPara =
            /// Constructs only the Run address for SorterSgd.MsrsOrthoPara.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.MsrsOrthoPara.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.MsrsOrthoPara.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.MsrsOrthoPara.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.MsrsOrthoPara.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.MsrsOrthoPara.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterSgd.MsrsOrthoPara.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.MsrsOrthoPara.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module MssiOrthoPara =
            /// Constructs only the Run address for SorterSgd.MssiOrthoPara.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.MssiOrthoPara.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.MssiOrthoPara.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.MssiOrthoPara.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.MssiOrthoPara.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.MssiOrthoPara.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterSgd.MssiOrthoPara.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.MssiOrthoPara.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Msuf =

            module Standard =
                /// Constructs only the Run address for SorterSgd.Msuf.Standard.
                let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the RunParameters address for SorterSgd.Msuf.Standard.
                let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSet address for SorterSgd.Msuf.Standard.
                let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msuf.Standard.
                let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSet address for SorterSgd.Msuf.Standard.
                let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterModelSet address for SorterSgd.Msuf.Standard.
                let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTests address for SorterSgd.Msuf.Standard.
                let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msuf.Standard.
                let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSetEval address for SorterSgd.Msuf.Standard.
                let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msuf.Standard.
                let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msuf.Standard.
                let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the TextReport address for SorterSgd.Msuf.Standard.
                let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Catalog adapter: selection only; each constructor owns its fields.
                let makeQueryParams projectName dbName parameters = function
                    | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                    | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                    | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            module Soss =
                /// Constructs only the Run address for SorterSgd.Msuf.Soss.
                let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the RunParameters address for SorterSgd.Msuf.Soss.
                let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSet address for SorterSgd.Msuf.Soss.
                let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msuf.Soss.
                let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSet address for SorterSgd.Msuf.Soss.
                let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterModelSet address for SorterSgd.Msuf.Soss.
                let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTests address for SorterSgd.Msuf.Soss.
                let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msuf.Soss.
                let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSetEval address for SorterSgd.Msuf.Soss.
                let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msuf.Soss.
                let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msuf.Soss.
                let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the TextReport address for SorterSgd.Msuf.Soss.
                let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Catalog adapter: selection only; each constructor owns its fields.
                let makeQueryParams projectName dbName parameters = function
                    | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                    | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                    | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            module StandardAppend =
                /// Constructs only the Run address for SorterSgd.Msuf.StandardAppend.
                let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the RunParameters address for SorterSgd.Msuf.StandardAppend.
                let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSet address for SorterSgd.Msuf.StandardAppend.
                let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msuf.StandardAppend.
                let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSet address for SorterSgd.Msuf.StandardAppend.
                let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterModelSet address for SorterSgd.Msuf.StandardAppend.
                let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTests address for SorterSgd.Msuf.StandardAppend.
                let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msuf.StandardAppend.
                let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSetEval address for SorterSgd.Msuf.StandardAppend.
                let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msuf.StandardAppend.
                let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msuf.StandardAppend.
                let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the TextReport address for SorterSgd.Msuf.StandardAppend.
                let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Catalog adapter: selection only; each constructor owns its fields.
                let makeQueryParams projectName dbName parameters = function
                    | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                    | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                    | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            module SossAppend =
                /// Constructs only the Run address for SorterSgd.Msuf.SossAppend.
                let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the RunParameters address for SorterSgd.Msuf.SossAppend.
                let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSet address for SorterSgd.Msuf.SossAppend.
                let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msuf.SossAppend.
                let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSet address for SorterSgd.Msuf.SossAppend.
                let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterModelSet address for SorterSgd.Msuf.SossAppend.
                let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTests address for SorterSgd.Msuf.SossAppend.
                let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msuf.SossAppend.
                let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSetEval address for SorterSgd.Msuf.SossAppend.
                let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msuf.SossAppend.
                let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msuf.SossAppend.
                let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the TextReport address for SorterSgd.Msuf.SossAppend.
                let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        let! sourceProject = rp.GetSourceProjectName()
                        let! sourceDb = rp.GetSourceDbName()
                        let! sourceRun = rp.GetSourceRunName()
                        let! sourceInterval = rp.GetSourceGenerationInterval()
                        let! sourceExecutionId = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey
                        let! runName = rp.GetRunName()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                yield ParamKeys.sourceProjectNameKey, %sourceProject
                                yield ParamKeys.sourceDbNameKey, %sourceDb
                                yield ParamKeys.sourceRunNameKey, %sourceRun
                                yield ParamKeys.sourceGenerationIntervalKey, string sourceInterval
                                yield ParamKeys.sourceExecutionIdKey, sourceExecutionId
                                for modifier in rp.GetSourceMutationMod() |> Option.toList do
                                    yield ParamKeys.sourceMutationModKey, string %modifier
                                yield ParamKeys.runNameKey, %runName
                                for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                    yield ParamKeys.setupParametersIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterModelSetIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                    yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Catalog adapter: selection only; each constructor owns its fields.
                let makeQueryParams projectName dbName parameters = function
                    | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                    | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                    | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Msrs32MutationRate =
            /// Constructs only the Run address for SorterSgd.Msrs32MutationRate.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.Msrs32MutationRate.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.Msrs32MutationRate.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.Msrs32MutationRate.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.Msrs32MutationRate.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msrs32MutationRate.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterSgd.Msrs32MutationRate.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.Msrs32MutationRate.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module MsrsPoolModComp =
            /// Constructs only the Run address for SorterSgd.MsrsPoolModComp.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.MsrsPoolModComp.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.MsrsPoolModComp.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.MsrsPoolModComp.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.MsrsPoolModComp.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.MsrsPoolModComp.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterSgd.MsrsPoolModComp.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.MsrsPoolModComp.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


    module SortableTests =

        module Merge =
            /// Constructs only the Run address for SortableTests.Merge.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SortableTests.Merge.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SortableTests.Merge.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SortableTests.Merge.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SortableTests.Merge.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SortableTests.Merge.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SortableTests.Merge.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SortableTests.Merge.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SortableTests.Merge.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SortableTests.Merge.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SortableTests.Merge.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SortableTests.Merge.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl (mergeLib: mergeLibId) (sortableDataFormat: sortableDataFormat) outDt =
                queryParams.create dbName projectName (Some repl) None outDt
                    [|
                        (ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib)
                        (ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: runParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeLibraryQueryParams projectName dbName repl mergeLib sdf odt
                }


        module Prefix =
            /// Constructs only the Run address for SortableTests.Prefix.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SortableTests.Prefix.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SortableTests.Prefix.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SortableTests.Prefix.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SortableTests.Prefix.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SortableTests.Prefix.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SortableTests.Prefix.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SortableTests.Prefix.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SortableTests.Prefix.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SortableTests.Prefix.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SortableTests.Prefix.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SortableTests.Prefix.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl (prefixLib: prefixLibId) (sortableDataFormat: sortableDataFormat) outDt =
                queryParams.create dbName projectName (Some repl) None outDt
                    [|
                        (ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib)
                        (ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: runParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeLibraryQueryParams projectName dbName repl prefixLib sdf odt
                }


    module SorterEval =

        module Standard =
            /// Constructs only the Run address for SorterEval.Standard.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterEval.Standard.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterEval.Standard.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterEval.Standard.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterEval.Standard.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterEval.Standard.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterEval.Standard.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterEval.Standard.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterEval.Standard.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterEval.Standard.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterEval.Standard.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterEval.Standard.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl rng sortingWidth simpleModel evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.sortingWidthKey, (Some sortingWidth) |> SortingWidth.toString)
                        (ParamKeys.simpleSorterModelTypeKey, simpleModel |> SimpleSorterModelType.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: runParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return makeLibraryQueryParams projectName dbName repl rng width model evalType odt
                }


        module Merge =
            /// Constructs only the Run address for SorterEval.Merge.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterEval.Merge.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterEval.Merge.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterEval.Merge.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterEval.Merge.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterEval.Merge.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterEval.Merge.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterEval.Merge.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterEval.Merge.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterEval.Merge.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterEval.Merge.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterEval.Merge.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl rng mergeLibId model dataFormat evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId)
                        (ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: runParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return makeLibraryQueryParams projectName dbName repl rng mergeLibId model dataFormat evalType odt
                }


        module Prefix =
            /// Constructs only the Run address for SorterEval.Prefix.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterEval.Prefix.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterEval.Prefix.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterEval.Prefix.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterEval.Prefix.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterEval.Prefix.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterEval.Prefix.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterEval.Prefix.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterEval.Prefix.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterEval.Prefix.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterEval.Prefix.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterEval.Prefix.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, RngType.toString rng
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, SimpleSorterModelType.toString model
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString dataFormat
                            yield ParamKeys.sorterEvalTypeKey, SorterEvalType.toString evalType
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl rng prefixLibId model dataFormat sorterCount evalType odt =
                let setupQuery =
                    QueryParamsBuildersSetup.SorterEval.Prefix.makeLibraryQueryParams
                        projectName dbName repl rng prefixLibId model dataFormat sorterCount odt
                match odt with
                | geneSortDataType.SorterModelSet _
                | geneSortDataType.SortableTestsWithPrefix _ -> setupQuery
                | _ ->
                    setupQuery.Properties
                    |> Map.add ParamKeys.sorterEvalTypeKey (SorterEvalType.toString evalType)
                    |> Map.toArray
                    |> queryParams.create dbName projectName (Some repl) None odt

            let libraryQueryFromParameters projectName dbName (rp: runParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return makeLibraryQueryParams projectName dbName repl rng prefixLibId model dataFormat sorterCount evalType odt
                }


    module SorterMutate =

        module Standard =
            /// Constructs only the Run address for SorterMutate.Standard.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterMutate.Standard.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterMutate.Standard.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterMutate.Standard.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterMutate.Standard.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterMutate.Standard.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterMutate.Standard.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterMutate.Standard.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterMutate.Standard.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterMutate.Standard.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterMutate.Standard.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterMutate.Standard.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Merge =
            /// Constructs only the Run address for SorterMutate.Merge.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterMutate.Merge.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterMutate.Merge.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterMutate.Merge.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterMutate.Merge.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterMutate.Merge.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterMutate.Merge.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterMutate.Merge.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterMutate.Merge.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterMutate.Merge.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterMutate.Merge.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterMutate.Merge.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Prefix =
            /// Constructs only the Run address for SorterMutate.Prefix.
            let makeRunQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterMutate.Prefix.
            let makeRunParametersQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterMutate.Prefix.
            let makeSorterPoolSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterMutate.Prefix.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterMutate.Prefix.
            let makeSorterSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterMutate.Prefix.
            let makeSorterModelSetQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterMutate.Prefix.
            let makeSortableTestsQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterMutate.Prefix.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SorterMutate.Prefix.
            let makeSorterSetEvalQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterMutate.Prefix.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterMutate.Prefix.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterMutate.Prefix.
            let makeTextReportQueryParams projectName dbName (rp: runParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.setupParametersIdKey |> Option.toList do
                                yield ParamKeys.setupParametersIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsWithPrefixIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetSummarySetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetSummarySetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterModelSetIdKey |> Option.toList do
                                yield ParamKeys.setupSorterModelSetIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterSetEvalIdKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolBinsSetSeriesIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolBinsSetSeriesIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.setupSorterPoolSetHistoryIdKey |> Option.toList do
                                yield ParamKeys.setupSorterPoolSetHistoryIdKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


    let registrations : (string * runQueryParamsCatalogBuilder) list = [
        QueryCatalogNamesRun.sorterSgdUf6MutationRate, SorterSgd.Uf6MutationRate.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsrsOrthoPara, SorterSgd.MsrsOrthoPara.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMssiOrthoPara, SorterSgd.MssiOrthoPara.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsufStandard, SorterSgd.Msuf.Standard.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsufSoss, SorterSgd.Msuf.Soss.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsufStandardAppend, SorterSgd.Msuf.StandardAppend.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsufSossAppend, SorterSgd.Msuf.SossAppend.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsrs32MutationRate, SorterSgd.Msrs32MutationRate.makeQueryParams
        QueryCatalogNamesRun.sorterSgdMsrsPoolModComp, SorterSgd.MsrsPoolModComp.makeQueryParams
        QueryCatalogNamesRun.sortableTestMerge, SortableTests.Merge.makeQueryParams
        QueryCatalogNamesRun.sortableTestPrefix, SortableTests.Prefix.makeQueryParams
        QueryCatalogNamesRun.sorterEvalStandard, SorterEval.Standard.makeQueryParams
        QueryCatalogNamesRun.sorterEvalMerge, SorterEval.Merge.makeQueryParams
        QueryCatalogNamesRun.sorterEvalPrefix, SorterEval.Prefix.makeQueryParams
        QueryCatalogNamesRun.sorterMutateStandard, SorterMutate.Standard.makeQueryParams
        QueryCatalogNamesRun.sorterMutateMerge, SorterMutate.Merge.makeQueryParams
        QueryCatalogNamesRun.sorterMutatePrefix, SorterMutate.Prefix.makeQueryParams
    ]
    let private registrationLock = obj ()
    let mutable private registered = false
    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                registrations |> List.iter (fun (name, make) -> QueryParamsCatalogRun.register name make)
                registered <- true)
