namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter

/// Context available to a run parameter enhancer without depending on dispatch infrastructure.
type runParamEnhancerContext = {
    QueryParamsFromRunParams: queryParamsBuilder
    Run: run
}

type runParamFilterBuilder = runParameters -> runParameters option
type runParamEnhancerBuilder = runParamEnhancerContext -> runParameters -> runParameters

module private RunParamCatalog =
    let register (catalogName: string) (name: string) (value: 'T) (catalog: Map<string, 'T> ref) (syncRoot: obj) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) (sprintf "A %s catalog name cannot be empty." catalogName)

        lock syncRoot (fun () -> catalog.Value <- Map.add name value catalog.Value)
        name

    let get (catalogName: string) (name: string) (catalog: Map<string, 'T> ref) (syncRoot: obj) =
        lock syncRoot (fun () ->
            match Map.tryFind name catalog.Value with
            | Some value -> value
            | None -> invalidOp (sprintf "No %s named '%s' is registered." catalogName name))

module RunParamFilterBuilders =
    let private syncRoot = obj ()
    let private filters: Map<string, runParamFilterBuilder> ref = ref Map.empty

    let register name filter = RunParamCatalog.register "run parameter filter" name filter filters syncRoot
    let get name = RunParamCatalog.get "run parameter filter" name filters syncRoot

    let identity (rp: runParameters) = Some rp

    let seedModificationRateDiffers (rp: runParameters) =
        match rp.GetSeedModificationRate(), rp.GetModificationRate() with
        | Some seedRate, Some modificationRate when %seedRate <> %modificationRate -> Some rp
        | _ -> None

    let private modelTypeAndWidthFilter (rp: runParameters) (getWidth: runParameters -> int<sortingWidth> option) (checkWidth: int<sortingWidth> -> bool) =
        match rp.GetSimpleSorterModelType(), getWidth rp with
        | Some modelType, Some width ->
            let isPowerOfTwo = MathUtils.isAPowerOfTwo %width
            let isDivisibleByTwo = (%width % 2 = 0)
            let isValid =
                match modelType with
                | simpleSorterModelType.Msce -> true
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> isDivisibleByTwo
                | simpleSorterModelType.Msuf4 -> isPowerOfTwo
                | simpleSorterModelType.Msuf6 -> (%width % 3 = 0) && MathUtils.isAPowerOfTwo (%width / 3)
            if isValid && checkWidth width then Some rp else None
        | _ -> None

    let mergeSorterModelCompatibility (rp: runParameters) =
        match rp.GetMergeLibId() with
        | Some libId ->
            modelTypeAndWidthFilter rp (fun _ -> Some libId.SortingWidth)
                (fun width -> %width % %libId.MergeDimension = 0)
        | None -> None

    let prefixSorterModelCompatibility (rp: runParameters) =
        modelTypeAndWidthFilter rp (fun value -> value.GetPrefixLibId() |> Option.map (fun libId -> libId.SortingWidth)) (fun _ -> true)

    let standardSorterModelCompatibility (rp: runParameters) =
        match rp.GetSimpleSorterModelType(), rp.GetSortingWidth() with
        | Some modelType, Some width ->
            let isPowerOfTwo = (%width &&& (%width - 1) = 0)
            let isValid =
                (%width > 4)
                && (modelType = simpleSorterModelType.Msce
                    || ((modelType = simpleSorterModelType.Mssi || modelType = simpleSorterModelType.Msrs) && %width % 2 = 0)
                    || (modelType = simpleSorterModelType.Msuf4 && isPowerOfTwo))
            if isValid then Some rp else None
        | _ -> None

    let mergeDimensionDividesSortingWidth (rp: runParameters) =
        match rp.GetMergeLibId() with
        | Some mergeLibId when %mergeLibId.SortingWidth % %mergeLibId.MergeDimension = 0 -> Some rp
        | _ -> None

module RunParamEnhancerBuilders =
    let private syncRoot = obj ()
    let private enhancers: Map<string, runParamEnhancerBuilder> ref = ref Map.empty

    let register name enhancer = RunParamCatalog.register "run parameter enhancer" name enhancer enhancers syncRoot
    let get name = RunParamCatalog.get "run parameter enhancer" name enhancers syncRoot

    let private queryParamsForRun (context: runParamEnhancerContext) (rp: runParameters) =
        context.QueryParamsFromRunParams rp (outputDataType.Run context.Run.RunName) |> Option.get

    let sortableTests (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithId(Some qp.Id)

    let sorterEvalStandard (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
            .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
            .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
            .WithId(Some qp.Id)

    let sorterMutateStandard (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
            .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
            .WithId(Some qp.Id)

    let sorterMutateStandardFormat (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
            .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
            .WithId(Some qp.Id)

    let sorterEvalMerge (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        let mergeLibId = rp.GetMergeLibId().Value
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithSortingWidth(Some mergeLibId.SortingWidth)
            .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
            .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
            .WithId(Some qp.Id)

    let sorterEvalPrefix (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        let prefixLibId = rp.GetPrefixLibId().Value
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithSortingWidth(Some prefixLibId.SortingWidth)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
            .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
            .WithId(Some qp.Id)

    module Sgd =

        let private applyPrefixDefaults
                (sortingWidth: int<sortingWidth>)
                (stageLength: int<stageLength>)
                (prefixVariant: prefixLibVariant)
                (modelType: simpleSorterModelType)
                (includeSorterEvalMeasure: bool)
                (rp: runParameters) =
            let selectionType = sorterSelectionType.GuidOrder (512<sorterCount>)
            let prefixLibId = prefixLibId.create sortingWidth stageLength prefixVariant
            let result =
                rp.WithRngType(Some rngType.Lcg)
                    .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
                    .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
                    .WithSorterChildCount(Some 1<sorterChildCount>)
                    .WithSimpleSorterModelType(Some modelType)
                    .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
                    .WithDistinctSorterHashes(Some true)
                    .WithPrioritizeNewMutants(Some true)
                    .WithSortedFraction(Some 0.99<sortedFraction>)
                    .WithSeedSorterPoolSelectionType(Some selectionType)
                    .WithPrefixLibId(Some prefixLibId)
                    .WithSortingWidth(Some prefixLibId.SortingWidth)
            if includeSorterEvalMeasure then
                result.WithSorterEvalMeasureInitial(Some SorterEvalMeasure.stageBiasedFilterUnsorted)
                    .WithSorterEvalMeasure(Some SorterEvalMeasure.stageBiasedFilterUnsorted)
            else result

        let private withOrthoRate (rate: float<orthoRate>) (applyDefaults: runParameters -> runParameters) (rp: runParameters) =
            applyDefaults rp |> fun value -> value.WithOrthoRate(Some rate)

        let private makeFinishRunParams
                (withLocalParams: runParameters -> runParameters)
                (poolCount: runParameters -> int<sorterPoolCount>)
                (selectSorterCountPerPool: bool)
                (setGenerationCurrent: bool)
                (modificationRate: float<modificationRate> option) : runParamEnhancerBuilder =
            fun host rp ->
                let scpp = rp.GetSorterCountPerPool().Value
                let rp2 = withLocalParams rp
                let rp3 = rp2.WithSorterPoolCount(Some (poolCount rp))
                let rp3 =
                    if setGenerationCurrent then rp3.WithGenerationCurrent(Some 0<generationNumber>)
                    else rp3
                let rp3 =
                    match modificationRate with
                    | Some rate -> rp3.WithModificationRate(Some rate)
                    | None -> rp3
                let qp = host.QueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

                let result =
                    rp3.WithRunFinished(Some false)
                        .WithId(Some qp.Value.Id)
                        .WithRunName(Some host.Run.RunName)
                if selectSorterCountPerPool then result.WithSelectedSorterCountPerPool(Some scpp) else result

        let fromGlobalSorterCount
                (withLocalParams: runParameters -> runParameters)
                (globalSorterCount: int<sorterCount>)
                (selectSorterCountPerPool: bool)
                (modificationRate: float<modificationRate> option) : runParamEnhancerBuilder =
            makeFinishRunParams
                withLocalParams
                (fun rp -> (%globalSorterCount / %rp.GetSorterCountPerPool().Value) |> UMX.tag<sorterPoolCount>)
                selectSorterCountPerPool
                false
                modificationRate

        let fromSorterPoolSet
                (withLocalParams: runParameters -> runParameters)
                (setGenerationCurrent: bool)
                (selectSorterCountPerPool: bool)
                (modificationRate: float<modificationRate> option) : runParamEnhancerBuilder =
            makeFinishRunParams
                withLocalParams
                (fun rp -> (%rp.GetSorterCountPerPoolSet().Value / %rp.GetSorterCountPerPool().Value) |> UMX.tag<sorterPoolCount>)
                selectSorterCountPerPool
                setGenerationCurrent
                modificationRate

        let msuf32MutationRate =
            fromSorterPoolSet
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA simpleSorterModelType.Msuf4 true))
                true
                true
                None

        let msrs32MutationRate =
            fromGlobalSorterCount
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA simpleSorterModelType.Msrs true))
                512<sorterCount>
                true
                None

        let msrs32MutationRateMax =
            fromGlobalSorterCount
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA simpleSorterModelType.Msrs true))
                512<sorterCount>
                true
                (Some 0.99<modificationRate>)

        let msuf624p3bMutationRate =
            let defaults rp =
                let result = applyPrefixDefaults 24<sortingWidth> 3<stageLength> prefixLibVariant.PrefixB simpleSorterModelType.Msuf6 false rp
                let sorterEvalMeasure = result.GetSorterEvalMeasure().Value
                result.WithSorterEvalMeasureInitial(Some sorterEvalMeasure).WithOrthoRate(Some 4.001<orthoRate>)
            fromSorterPoolSet defaults false true None

        let msrs24p3aOrthoPara =
            fromGlobalSorterCount
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 24<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA simpleSorterModelType.Msrs true))
                8192<sorterCount>
                true
                (Some 0.99<modificationRate>)

        let msrs24p3aPoolModComp =
            fromGlobalSorterCount
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 24<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA simpleSorterModelType.Msrs true))
                512<sorterCount>
                false
                None

        let msrs24p3bOrthoPara =
            fromGlobalSorterCount
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 24<sortingWidth> 4<stageLength> prefixLibVariant.PrefixB simpleSorterModelType.Msrs true))
                8192<sorterCount>
                true
                (Some 0.99<modificationRate>)

        let mssi24p3bOrthoPara =
            fromGlobalSorterCount
                (withOrthoRate 4.001<orthoRate> (applyPrefixDefaults 24<sortingWidth> 4<stageLength> prefixLibVariant.PrefixB simpleSorterModelType.Mssi true))
                1024<sorterCount>
                true
                None

module RunParamBuilderNames =
    module Filter =
        [<Literal>]
        let identity = "identity"
        [<Literal>]
        let seedModificationRateDiffers = "seed-modification-rate-differs"
        [<Literal>]
        let mergeSorterModelCompatibility = "merge-sorter-model-compatibility"
        [<Literal>]
        let prefixSorterModelCompatibility = "prefix-sorter-model-compatibility"
        [<Literal>]
        let standardSorterModelCompatibility = "standard-sorter-model-compatibility"
        [<Literal>]
        let mergeDimensionDividesSortingWidth = "merge-dimension-divides-sorting-width"

    module Enhancer =
        [<Literal>]
        let sortableTests = "sortable-test"
        [<Literal>]
        let sorterEvalStandard = "sorter-eval-standard"
        [<Literal>]
        let sorterEvalMerge = "sorter-eval-merge"
        [<Literal>]
        let sorterEvalPrefix = "sorter-eval-prefix"
        [<Literal>]
        let sorterMutateStandard = "sorter-mutate-standard"
        [<Literal>]
        let sorterMutateStandardFormat = "sorter-mutate-standard-format"
        [<Literal>]
        let msuf32MutationRate = "sgd-msuf32-mutation-rate"
        [<Literal>]
        let msrs32MutationRate = "sgd-msrs32-mutation-rate"
        [<Literal>]
        let msrs32MutationRateMax = "sgd-msrs32-mutation-rate-max"
        [<Literal>]
        let msuf624p3bMutationRate = "sgd-msuf624p3b-mutation-rate"
        [<Literal>]
        let msrs24p3aOrthoPara = "sgd-msrs24p3a-ortho-para"
        [<Literal>]
        let msrs24p3aPoolModComp = "sgd-msrs24p3a-pool-mod-comp"
        [<Literal>]
        let msrs24p3bOrthoPara = "sgd-msrs24p3b-ortho-para"
        [<Literal>]
        let mssi24p3bOrthoPara = "sgd-mssi24p3b-ortho-para"

module RunParamBuilders =
    let private registrationLock = obj ()
    let mutable private registered = false

    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                RunParamFilterBuilders.register RunParamBuilderNames.Filter.identity RunParamFilterBuilders.identity |> ignore
                RunParamFilterBuilders.register RunParamBuilderNames.Filter.seedModificationRateDiffers RunParamFilterBuilders.seedModificationRateDiffers |> ignore
                RunParamFilterBuilders.register RunParamBuilderNames.Filter.mergeSorterModelCompatibility RunParamFilterBuilders.mergeSorterModelCompatibility |> ignore
                RunParamFilterBuilders.register RunParamBuilderNames.Filter.prefixSorterModelCompatibility RunParamFilterBuilders.prefixSorterModelCompatibility |> ignore
                RunParamFilterBuilders.register RunParamBuilderNames.Filter.standardSorterModelCompatibility RunParamFilterBuilders.standardSorterModelCompatibility |> ignore
                RunParamFilterBuilders.register RunParamBuilderNames.Filter.mergeDimensionDividesSortingWidth RunParamFilterBuilders.mergeDimensionDividesSortingWidth |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sortableTests RunParamEnhancerBuilders.sortableTests |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterEvalStandard RunParamEnhancerBuilders.sorterEvalStandard |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterEvalMerge RunParamEnhancerBuilders.sorterEvalMerge |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterEvalPrefix RunParamEnhancerBuilders.sorterEvalPrefix |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterMutateStandard RunParamEnhancerBuilders.sorterMutateStandard |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterMutateStandardFormat RunParamEnhancerBuilders.sorterMutateStandardFormat |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msuf32MutationRate RunParamEnhancerBuilders.Sgd.msuf32MutationRate |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs32MutationRate RunParamEnhancerBuilders.Sgd.msrs32MutationRate |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs32MutationRateMax RunParamEnhancerBuilders.Sgd.msrs32MutationRateMax |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msuf624p3bMutationRate RunParamEnhancerBuilders.Sgd.msuf624p3bMutationRate |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara RunParamEnhancerBuilders.Sgd.msrs24p3aOrthoPara |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp RunParamEnhancerBuilders.Sgd.msrs24p3aPoolModComp |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs24p3bOrthoPara RunParamEnhancerBuilders.Sgd.msrs24p3bOrthoPara |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara RunParamEnhancerBuilders.Sgd.mssi24p3bOrthoPara |> ignore
                registered <- true)
