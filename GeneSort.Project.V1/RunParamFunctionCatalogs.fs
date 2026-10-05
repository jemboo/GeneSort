namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Eval.V1

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

    let register filter =
        let name = sprintf "run-param-filter.%s" (System.Guid.NewGuid().ToString("N"))
        RunParamCatalog.register "run parameter filter" name filter filters syncRoot
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

    let register enhancer =
        let name = sprintf "run-param-enhancer.%s" (System.Guid.NewGuid().ToString("N"))
        RunParamCatalog.register "run parameter enhancer" name enhancer enhancers syncRoot
    let get name = RunParamCatalog.get "run parameter enhancer" name enhancers syncRoot

    let private queryParamsForRun (context: runParamEnhancerContext) (rp: runParameters) =
        context.QueryParamsFromRunParams rp (outputDataType.Run context.Run.RunName) |> Option.get

    let sortableTest (context: runParamEnhancerContext) (rp: runParameters) =
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
