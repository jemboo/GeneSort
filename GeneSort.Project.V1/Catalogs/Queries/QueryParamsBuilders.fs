namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters

/// Named, configuration-independent query parameter builders.
module QueryParamsBuilders =

    module SorterSgd =

        let private withAdjustmentFrequency (rp: runParameters) (query: queryParams) =
            match rp.GetAdjustCeLengthFrequency() with
            | None -> query
            | Some frequency ->
                let properties = query.Properties |> Map.add runParameters.adjustCeLengthFrequencyKey (string %frequency)
                queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties)

        module Uf6MutationRate =

            let makeQueryParams
                    (projectName: string<projectName>)
                    (dbName: string<databaseName>)
                    (repl: int<replNumber>)
                    (codeMod: string<codeModKey>)
                    (genCurrent: int<generationNumber>)
                    (sorterCtPerPool: int<sorterCountPerPool>)
                    (sorterPoolCt: int<sorterPoolCount>)
                    (sorterEvalMeasure: sorterEvalMeasure)
                    (sorterEvalMeasureInitial: sorterEvalMeasure)
                    (para: float<paraRate>)
                    (selfSym: float<selfSymRate>)
                    (seedModR: float<seedModificationRate>)
                    (modR: float<modificationRate>)
                    (mmod: int<mutationMod>)
                    (outDt: geneSortDataType) : queryParams =
                queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                    [|
                        (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.sorterEvalMeasureKey, sorterEvalMeasure |> SorterEvalMeasure.toCompactString)
                        (runParameters.sorterEvalMeasureInitialKey, sorterEvalMeasureInitial |> SorterEvalMeasure.toCompactString)
                        (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                        (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (runParameters.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
                        (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                    |]

            let queryParamsFromRunParams
                    (projectName: string<projectName>)
                    (dbName: string<databaseName>)
                    (rp: runParameters)
                    (odt: geneSortDataType) : queryParams option =
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
                    return makeQueryParams projectName dbName repl codeMod curGen scPP spc sem semi para self seedModR modR mmod odt
                           |> withAdjustmentFrequency rp
                }

        module MsrsOrthoPara =

            let makeQueryParams projectName dbName repl codeMod genCurrent sorterCtPerPool sorterPoolCt para selfSym mmod outDt =
                queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                    [|
                        (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                        (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                    |]

            let queryParamsFromRunParams projectName dbName (rp: runParameters) (odt: geneSortDataType) =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return makeQueryParams projectName dbName repl codeMod curGen scPP spc para self mmod odt
                           |> withAdjustmentFrequency rp
                }

        module MssiOrthoPara =

            let makeQueryParams projectName dbName repl codeMod genCurrent sorterCtPerPool sorterPoolCt modR para mmod outDt =
                queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                    [|
                        (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                        (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                    |]

            let queryParamsFromRunParams projectName dbName (rp: runParameters) (odt: geneSortDataType) =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return makeQueryParams projectName dbName repl codeMod curGen scPP spc modR para mmod odt
                           |> withAdjustmentFrequency rp
                }

        module Msuf =

            module Standard =
                let makeQueryParams projectName dbName repl codeMod genCurrent sorterCtPerPool sorterPoolCt para selfSym seedModR modR mmod mutVar outDt  =
                    queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                        [|
                            (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                            (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                            (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                            (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                            (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                            (runParameters.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
                            (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
                            (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                            (runParameters.mutatorVariantKey, mutVar |> MutatorVariant.toString)
                        |]
                let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                        return makeQueryParams projectName dbName repl codeMod curGen scPP spc para self 
                                               seedModR modR mmod mutVar odt
                               |> withAdjustmentFrequency rp
                    }

            module Soss =

                let makeQueryParams projectName dbName repl codeMod genCurrent 
                                    sorterCtPerPool sorterPoolCt para selfSym seedModR 
                                    modR mmod mutVar seedSoss partCt genSoss genMerged outDt =
                    queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                        [|
                            (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                            (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                            (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                            (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                            (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                            (runParameters.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
                            (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
                            (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                            (runParameters.mutatorVariantKey, mutVar |> MutatorVariant.toString)
                            (runParameters.seedSossKey, seedSoss |> string)
                            (runParameters.partitionCountKey, (Some partCt) |> PartitionCount.toString)
                            (runParameters.generationsPerSubsetKey, (Some genSoss) |> GenerationsPerSubset.toString)
                            (runParameters.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString)
                        |]

                let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                        let query = makeQueryParams projectName dbName repl codeMod curGen scPP spc para self seedModR 
                                                    modR mmod mutVar seedSoss partCt genSoss genMerged odt
                        let properties =
                            query.Properties
                            |> fun properties ->
                                match rp.GetPartitionCount() with
                                | None -> properties
                                | Some count -> properties |> Map.add runParameters.partitionCountKey (string %count)
                            |> fun properties ->
                                match rp.GetGenerationsPerSubset() with
                                | None -> properties
                                | Some count -> properties |> Map.add runParameters.generationsPerSubsetKey (string %count)
                            |> fun properties ->
                                match rp.GetAdjustCeLengthFrequency() with
                                | None -> properties
                                | Some count -> properties |> Map.add runParameters.adjustCeLengthFrequencyKey (string %count)
                        return queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties)
                    }

        module SossAppend =
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! query = Msuf.Soss.queryParamsFromRunParams projectName dbName rp odt
                    let! sourceProject = rp.GetSourceProjectName()
                    let! sourceDb = rp.GetSourceDbName()
                    let! sourceRun = rp.GetSourceRunName()
                    let! sourceInterval = rp.GetSourceGenerationInterval()
                    let properties =
                        query.Properties
                        |> Map.add runParameters.sourceProjectNameKey %sourceProject
                        |> Map.add runParameters.sourceDbNameKey %sourceDb
                        |> Map.add runParameters.sourceRunNameKey %sourceRun
                        |> Map.add runParameters.sourceGenerationIntervalKey (string sourceInterval)
                    return queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties)
                }

        module Msrs32MutationRate =
            let makeQueryParams projectName dbName repl codeMod genCurrent sorterCtPerPool sorterPoolCt para selfSym modR mmod outDt =
                queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                    [|
                        (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                        (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                    return makeQueryParams projectName dbName repl codeMod curGen scPP spc para selfSym modR mmod odt
                           |> withAdjustmentFrequency rp
                }

        module MsrsPoolModComp =
            let makeQueryParams projectName dbName repl genCurrent sorterCtPerPool sorterPoolCt modR para selfSym seedSelection selectedSorterCount mmod outDt =
                queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                    [|
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString)
                        (runParameters.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                        (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (runParameters.selectedSorterCountPerPoolKey, (Some selectedSorterCount) |> SorterCountPerPool.toString)
                        (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                    return makeQueryParams projectName dbName repl gen scpp spc modR para selfSym seedSelection selectedCount mmod odt
                           |> withAdjustmentFrequency rp
                }

    module SortableTests =

        module Merge =
            let makeQueryParams projectName dbName repl (mergeLib: mergeLibId) (sortableDataFormat: sortableDataFormat) outDt =
                queryParams.create dbName projectName (Some repl) None outDt
                    [|
                        (runParameters.mergeLibIdKey, MergeLibId.toString mergeLib)
                        (runParameters.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeQueryParams projectName dbName repl mergeLib sdf odt
                }

        module Prefix =
            let makeQueryParams projectName dbName repl (prefixLib: prefixLibId) (sortableDataFormat: sortableDataFormat) outDt =
                queryParams.create dbName projectName (Some repl) None outDt
                    [|
                        (runParameters.prefixLibIdKey, PrefixLibId.toString prefixLib)
                        (runParameters.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeQueryParams projectName dbName repl prefixLib sdf odt
                }

    module SorterEval =

        module Standard =
            let makeQueryParams projectName dbName repl rng sortingWidth simpleModel evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (runParameters.rngTypeKey, rng |> RngType.toString)
                        (runParameters.sortingWidthKey, (Some sortingWidth) |> SortingWidth.toString)
                        (runParameters.simpleSorterModelTypeKey, simpleModel |> SimpleSorterModelType.toString)
                        (runParameters.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return makeQueryParams projectName dbName repl rng width model evalType odt
                }

        module Merge =
            let makeQueryParams projectName dbName repl rng mergeLibId model dataFormat evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (runParameters.rngTypeKey, rng |> RngType.toString)
                        (runParameters.mergeLibIdKey, MergeLibId.toString mergeLibId)
                        (runParameters.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                        (runParameters.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (runParameters.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return makeQueryParams projectName dbName repl rng mergeLibId model dataFormat evalType odt
                }

        module Prefix =
            let makeQueryParams projectName dbName repl rng prefixLibId model dataFormat evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (runParameters.rngTypeKey, rng |> RngType.toString)
                        (runParameters.prefixLibIdKey, PrefixLibId.toString prefixLibId)
                        (runParameters.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                        (runParameters.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (runParameters.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return makeQueryParams projectName dbName repl rng prefixLibId model dataFormat evalType odt
                }

    module SorterMutate =

        module Standard =
            let makeQueryParams projectName dbName repl odt rng selection measure sortingWidth simpleModel dataFormat evalType mutatorParams modRate =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (runParameters.rngTypeKey, rng |> RngType.toString)
                        (runParameters.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString)
                        (runParameters.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString)
                        (runParameters.sortingWidthKey, (Some sortingWidth) |> SortingWidth.toString)
                        (runParameters.simpleSorterModelTypeKey, simpleModel |> SimpleSorterModelType.toString)
                        (runParameters.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                        (runParameters.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (runParameters.mutatorParamsKey, mutatorParams |> MutatorParams.toString)
                        (runParameters.modificationRateKey, (Some modRate) |> ModificationRate.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                    return makeQueryParams projectName dbName repl odt rng selection measure width model format evalType mutatorParams modRate
                }

        module Merge =
            let makeQueryParams projectName dbName repl odt rng selection measure mergeLib model format evalType mutatorParams modRate =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (runParameters.rngTypeKey, rng |> RngType.toString)
                        (runParameters.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString)
                        (runParameters.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString)
                        (runParameters.mergeLibIdKey, mergeLib |> MergeLibId.toString)
                        (runParameters.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                        (runParameters.sortableDataFormatKey, format |> SortableDataFormat.toString)
                        (runParameters.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (runParameters.mutatorParamsKey, mutatorParams |> MutatorParams.toString)
                        (runParameters.modificationRateKey, (Some modRate) |> ModificationRate.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                    return makeQueryParams projectName dbName repl odt rng selection measure mergeLib model format evalType mutatorParams modRate
                }

        module Prefix =
            let makeQueryParams projectName dbName repl odt rng selection measure prefixLib model format evalType mutatorParams modRate =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (runParameters.rngTypeKey, rng |> RngType.toString)
                        (runParameters.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString)
                        (runParameters.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString)
                        (runParameters.prefixLibIdKey, prefixLib |> PrefixLibId.toString)
                        (runParameters.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                        (runParameters.sortableDataFormatKey, format |> SortableDataFormat.toString)
                        (runParameters.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (runParameters.mutatorParamsKey, mutatorParams |> MutatorParams.toString)
                        (runParameters.modificationRateKey, (Some modRate) |> ModificationRate.toString)
                    |]
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
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
                    return makeQueryParams projectName dbName repl odt rng selection measure prefixLib model format evalType mutatorParams modRate
                }
    let private registrationLock = obj ()
    let mutable private registered = false

    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                [
                    QueryCatalogNames.sorterSgdUf6MutationRate, SorterSgd.Uf6MutationRate.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMsrsOrthoPara, SorterSgd.MsrsOrthoPara.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMssiOrthoPara, SorterSgd.MssiOrthoPara.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMsufStandard, SorterSgd.Msuf.Standard.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMsufSoss, SorterSgd.Msuf.Soss.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMsufSossAppend, SorterSgd.SossAppend.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMsrs32MutationRate, SorterSgd.Msrs32MutationRate.queryParamsFromRunParams
                    QueryCatalogNames.sorterSgdMsrsPoolModComp, SorterSgd.MsrsPoolModComp.queryParamsFromRunParams
                    QueryCatalogNames.sortableTestMerge, SortableTests.Merge.queryParamsFromRunParams
                    QueryCatalogNames.sortableTestPrefix, SortableTests.Prefix.queryParamsFromRunParams
                    QueryCatalogNames.sorterEvalStandard, SorterEval.Standard.queryParamsFromRunParams
                    QueryCatalogNames.sorterEvalMerge, SorterEval.Merge.queryParamsFromRunParams
                    QueryCatalogNames.sorterEvalPrefix, SorterEval.Prefix.queryParamsFromRunParams
                    QueryCatalogNames.sorterMutateStandard, SorterMutate.Standard.queryParamsFromRunParams
                    QueryCatalogNames.sorterMutateMerge, SorterMutate.Merge.queryParamsFromRunParams
                    QueryCatalogNames.sorterMutatePrefix, SorterMutate.Prefix.queryParamsFromRunParams
                ]
                |> List.iter (fun (name, builder) -> QueryParamsCatalog.register name builder)

                registered <- true
        )
