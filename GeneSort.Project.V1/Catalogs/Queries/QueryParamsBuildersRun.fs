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

/// Named, configuration-independent query parameter builders.
module QueryParamsBuildersRun =

    module SorterSgd =

        let private withAdjustmentFrequency (rp: runParameters) (query: queryParams) =
            match rp.GetAdjustCeLengthFrequency() with
            | None -> query
            | Some frequency ->
                let properties = query.Properties |> Map.add ParamKeys.adjustCeLengthFrequencyKey (string %frequency)
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
                        (ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (ParamKeys.sorterEvalMeasureKey, sorterEvalMeasure |> SorterEvalMeasure.toCompactString)
                        (ParamKeys.sorterEvalMeasureInitialKey, sorterEvalMeasureInitial |> SorterEvalMeasure.toCompactString)
                        (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                        (ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
                        (ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString)
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
                        (ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                        (ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString)
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
                        (ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                        (ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString)
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
                            (ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString)
                            (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                            (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                            (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                            (ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                            (ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
                            (ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString)
                            (ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString)
                            (ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString)
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
                            (ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString)
                            (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                            (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                            (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                            (ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                            (ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString)
                            (ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString)
                            (ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString)
                            (ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString)
                            (ParamKeys.seedSossKey, seedSoss |> string)
                            (ParamKeys.partitionCountKey, (Some partCt) |> PartitionCount.toString)
                            (ParamKeys.generationsPerSubsetKey, (Some genSoss) |> GenerationsPerSubset.toString)
                            (ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString)
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
                                | Some count -> properties |> Map.add ParamKeys.partitionCountKey (string %count)
                            |> fun properties ->
                                match rp.GetGenerationsPerSubset() with
                                | None -> properties
                                | Some count -> properties |> Map.add ParamKeys.generationsPerSubsetKey (string %count)
                            |> fun properties ->
                                match rp.GetAdjustCeLengthFrequency() with
                                | None -> properties
                                | Some count -> properties |> Map.add ParamKeys.adjustCeLengthFrequencyKey (string %count)
                        return queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties)
                    }

        module EvoAppend =
            let private metadataQuery (baseQuery: runQueryParamsCatalogBuilder) projectName dbName (rp: runParameters) odt =
                maybe {
                    let! query = baseQuery projectName dbName rp odt
                    let! sourceProject = rp.GetSourceProjectName()
                    let! sourceDb = rp.GetSourceDbName()
                    let! sourceRun = rp.GetSourceRunName()
                    let! sourceInterval = rp.GetSourceGenerationInterval()
                    let! sourceExecutionId = Map.tryFind ParamKeys.sourceExecutionIdKey rp.ParamMap
                    let properties =
                        query.Properties
                        |> Map.add ParamKeys.sourceProjectNameKey %sourceProject
                        |> Map.add ParamKeys.sourceDbNameKey %sourceDb
                        |> Map.add ParamKeys.sourceRunNameKey %sourceRun
                        |> Map.add ParamKeys.sourceGenerationIntervalKey (string sourceInterval)
                        |> Map.add ParamKeys.sourceExecutionIdKey sourceExecutionId
                    return queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties)
                }

            let queryParamsFromRunParams baseQuery projectName dbName (rp: runParameters) odt =
                match odt with
                | geneSortDataType.Run _
                | geneSortDataType.RunParameters _ ->
                    metadataQuery baseQuery projectName dbName rp odt
                | _ ->
                    // Setup links are attached by the catalog; ancestry is not part of result identity.
                    baseQuery projectName dbName rp odt

        module Msrs32MutationRate =
            let makeQueryParams projectName dbName repl codeMod genCurrent sorterCtPerPool sorterPoolCt para selfSym modR mmod outDt =
                queryParams.create dbName projectName (Some repl) (Some genCurrent) outDt
                    [|
                        (ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString)
                        (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                        (ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString)
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
                        (ParamKeys.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (ParamKeys.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString)
                        (ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString)
                        (ParamKeys.paraRateKey, (Some para) |> ParaRate.toString)
                        (ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                        (ParamKeys.selectedSorterCountPerPoolKey, (Some selectedSorterCount) |> SorterCountPerPool.toString)
                        (ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString)
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
                        (ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib)
                        (ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
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
                        (ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib)
                        (ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
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
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.sortingWidthKey, (Some sortingWidth) |> SortingWidth.toString)
                        (ParamKeys.simpleSorterModelTypeKey, simpleModel |> SimpleSorterModelType.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
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
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId)
                        (ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
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
            let makeQueryParams projectName dbName repl rng prefixLibId model dataFormat sorterCount evalType odt =
                let setupQuery =
                    QueryParamsBuildersSetup.SorterEval.Prefix.makeQueryParams
                        projectName dbName repl rng prefixLibId model dataFormat sorterCount odt
                match odt with
                | geneSortDataType.SorterModelSet _
                | geneSortDataType.SortableTestsWithPrefix _ -> setupQuery
                | _ ->
                    setupQuery.Properties
                    |> Map.add ParamKeys.sorterEvalTypeKey (SorterEvalType.toString evalType)
                    |> Map.toArray
                    |> queryParams.create dbName projectName (Some repl) None odt
            let queryParamsFromRunParams projectName dbName (rp: runParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! evalType = rp.GetSorterEvalType()
                    let! repl = rp.GetRepl()
                    return makeQueryParams projectName dbName repl rng prefixLibId model dataFormat sorterCount evalType odt
                }

    module SorterMutate =

        module Standard =
            let makeQueryParams projectName dbName repl odt rng selection measure sortingWidth simpleModel dataFormat evalType mutatorParams modRate =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString)
                        (ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString)
                        (ParamKeys.sortingWidthKey, (Some sortingWidth) |> SortingWidth.toString)
                        (ParamKeys.simpleSorterModelTypeKey, simpleModel |> SimpleSorterModelType.toString)
                        (ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString)
                        (ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString)
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
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString)
                        (ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString)
                        (ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString)
                        (ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                        (ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString)
                        (ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString)
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
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString)
                        (ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString)
                        (ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString)
                        (ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                        (ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString)
                        (ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString)
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
                    QueryCatalogNamesRun.sorterSgdUf6MutationRate, SorterSgd.Uf6MutationRate.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsrsOrthoPara, SorterSgd.MsrsOrthoPara.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMssiOrthoPara, SorterSgd.MssiOrthoPara.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsufStandard, SorterSgd.Msuf.Standard.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsufSoss, SorterSgd.Msuf.Soss.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsufSossAppend, SorterSgd.EvoAppend.queryParamsFromRunParams SorterSgd.Msuf.Soss.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsufStandardAppend, SorterSgd.EvoAppend.queryParamsFromRunParams SorterSgd.Msuf.Standard.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsrs32MutationRate, SorterSgd.Msrs32MutationRate.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterSgdMsrsPoolModComp, SorterSgd.MsrsPoolModComp.queryParamsFromRunParams
                    QueryCatalogNamesRun.sortableTestMerge, SortableTests.Merge.queryParamsFromRunParams
                    QueryCatalogNamesRun.sortableTestPrefix, SortableTests.Prefix.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterEvalStandard, SorterEval.Standard.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterEvalMerge, SorterEval.Merge.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterEvalPrefix, SorterEval.Prefix.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterMutateStandard, SorterMutate.Standard.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterMutateMerge, SorterMutate.Merge.queryParamsFromRunParams
                    QueryCatalogNamesRun.sorterMutatePrefix, SorterMutate.Prefix.queryParamsFromRunParams
                ]
                |> List.iter (fun (name, builder) -> QueryParamsCatalogRun.register name builder)

                registered <- true
        )
