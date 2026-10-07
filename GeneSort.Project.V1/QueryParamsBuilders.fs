namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1.RunParamBundles

/// Named, configuration-independent query parameter builders.
module QueryParamsBuilders =

    module SorterSgd =

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
                    (outDt: outputDataType) : queryParams =
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
                    (odt: outputDataType) : queryParams option =
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

            let queryParamsFromRunParams projectName dbName (rp: runParameters) (odt: outputDataType) =
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

            let queryParamsFromRunParams projectName dbName (rp: runParameters) (odt: outputDataType) =
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
                }

        module Msuf32MutationRate =
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
                    return makeQueryParams projectName dbName repl codeMod curGen scPP spc para self seedModR modR mmod mutVar odt
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
                    "sorter-sgd.uf6-mutation-rate", SorterSgd.Uf6MutationRate.queryParamsFromRunParams
                    "sorter-sgd.msrs-ortho-para", SorterSgd.MsrsOrthoPara.queryParamsFromRunParams
                    "sorter-sgd.mssi-ortho-para", SorterSgd.MssiOrthoPara.queryParamsFromRunParams
                    "sorter-sgd.msuf32-mutation-rate", SorterSgd.Msuf32MutationRate.queryParamsFromRunParams
                    "sorter-sgd.msrs32-mutation-rate", SorterSgd.Msrs32MutationRate.queryParamsFromRunParams
                    "sorter-sgd.msrs-pool-mod-comp", SorterSgd.MsrsPoolModComp.queryParamsFromRunParams
                    "sortable-test.merge", SortableTests.Merge.queryParamsFromRunParams
                    "sortable-test.prefix", SortableTests.Prefix.queryParamsFromRunParams
                    "sorter-eval.standard", SorterEval.Standard.queryParamsFromRunParams
                    "sorter-eval.merge", SorterEval.Merge.queryParamsFromRunParams
                    "sorter-eval.prefix", SorterEval.Prefix.queryParamsFromRunParams
                    "sorter-mutate.standard", SorterMutate.Standard.queryParamsFromRunParams
                    "sorter-mutate.merge", SorterMutate.Merge.queryParamsFromRunParams
                    "sorter-mutate.prefix", SorterMutate.Prefix.queryParamsFromRunParams
                ]
                |> List.iter (fun (name, builder) -> QueryParamsCatalog.register name builder)

                registered <- true
        )
