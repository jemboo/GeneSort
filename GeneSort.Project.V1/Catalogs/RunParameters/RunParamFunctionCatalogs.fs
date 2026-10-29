namespace GeneSort.Project.V1.Catalogs.RunParameters

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd

/// Context available to a run parameter enhancer without depending on dispatch infrastructure.
type runParamEnhancerContext =
    private {
        queryParamsFromRunParams: runQueryParamsBuilder
        run: run
        setupParameters: setupParameters option
    }

    static member create
            (queryParamsFromRunParams: runQueryParamsBuilder)
            (run: run)
            : runParamEnhancerContext =
        {
            queryParamsFromRunParams = queryParamsFromRunParams
            run = run
            setupParameters = None
        }

    static member createForSetup query run setup =
        { queryParamsFromRunParams = query; run = run; setupParameters = Some setup }

    member this.QueryParamsFromRunParams with get() = this.queryParamsFromRunParams
    member this.Run with get() = this.run
    member this.SetupParameters = this.setupParameters


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

module RunParamEnhancerBuilders =
    let private syncRoot = obj ()
    let private enhancers: Map<string, runParamEnhancerBuilder> ref = ref Map.empty

    let register name enhancer = RunParamCatalog.register "run parameter enhancer" name enhancer enhancers syncRoot
    let get name = RunParamCatalog.get "run parameter enhancer" name enhancers syncRoot

    let private queryParamsForRun (context: runParamEnhancerContext) (rp: runParameters) =
        context.QueryParamsFromRunParams rp (geneSortDataType.Run context.Run.RunName) |> Option.get

    let private inheritSetup (context: runParamEnhancerContext) (rp: runParameters) =
        match context.SetupParameters with
        | None -> rp
        | Some setup ->
            (SetupFileReferences.withoutReferences setup).ParamMap
            |> Map.fold (fun map key value -> if Map.containsKey key map then map else Map.add key value map) rp.ParamMap
            |> runParameters.create

    let private initializeRun (context: runParamEnhancerContext) (rp: runParameters) =
        let qp = queryParamsForRun context rp
        rp.WithDatabaseName(Some context.Run.DatabaseName)
            .WithRunName(Some context.Run.RunName)
            .WithRunFinished(Some false)
            .WithId(Some qp.Id)

    let sortableTests = initializeRun

    /// Execution spans win over paired setup values; file references are handled separately.
    let private fromSetup context parameters =
        inheritSetup context parameters |> initializeRun context

    let sorterEvalStandard = fromSetup
    let sorterMutateStandard = fromSetup
    let sorterMutateStandardFormat = fromSetup
    let sorterEvalMerge = fromSetup

    let sorterEvalPrefix (context: runParamEnhancerContext) (rp: runParameters) =
        let rp =
            match context.SetupParameters with
            | None -> rp
            | Some setup ->
                let keys = [ParamKeys.rngTypeKey; ParamKeys.prefixLibIdKey; ParamKeys.simpleSorterModelTypeKey;
                            ParamKeys.sortableDataFormatKey; ParamKeys.sorterCountKey]
                keys
                |> List.fold (fun map key ->
                    let value = setup.ParamMap.TryFind key
                                |> Option.defaultWith (fun () -> invalidOp (sprintf "Prefix evaluation setup is missing '%s'." key))
                    Map.add key value map) rp.ParamMap
                |> fun map ->
                    [ParamKeys.aMaxKey; ParamKeys.activationProbabilityKey; ParamKeys.activatorMutationRateKey]
                    |> List.fold (fun map key ->
                        match setup.ParamMap.TryFind key with
                        | Some value -> Map.add key value map
                        | None -> map) map
                |> runParameters.create
        initializeRun context rp

    module Sgd =

        let private fromSorterPoolSet setGenerationCurrent : runParamEnhancerBuilder =
            fun host parameters ->
                let rp = (inheritSetup host parameters).ParamMap |> ParameterValidation.resolveSelection |> runParameters.create
                ParameterValidation.validateValues SgdParameterValidation.seedPoolKeys rp.ParamMap
                |> ParameterValidation.ensureValid "parameters"
                ParameterValidation.poolSizes "Execution" (ParameterValidation.ofMap rp.ParamMap)
                |> ParameterValidation.ensureValid "parameters"
                let perPool = rp.GetSorterCountPerPool().Value
                let total = rp.GetSorterCountPerPoolSet().Value
                let enhanced = rp.WithSorterPoolCount(Some (UMX.tag (int total / int perPool)))
                let enhanced = if setGenerationCurrent then enhanced.WithGenerationCurrent(Some 0<generationNumber>) else enhanced
                let qp = host.QueryParamsFromRunParams enhanced (geneSortDataType.Run host.Run.RunName)
                enhanced.WithRunFinished(Some false).WithId(Some qp.Value.Id).WithRunName(Some host.Run.RunName)

        let msuf32MutationRate : runParamEnhancerBuilder = fun context parameters ->
            parameters.ParamMap |> ParameterValidation.ofMap |> SgdParameterValidation.msufExecution
            |> ParameterValidation.ensureValid "parameters"
            fromSorterPoolSet true context parameters

        let private combineSetup (context: runParamEnhancerContext) (parameters: runParameters) =
            match context.SetupParameters with
            | None -> parameters
            | Some setup ->
                Map.fold (fun map key value -> Map.add key value map) setup.ParamMap parameters.ParamMap
                |> runParameters.create

        /// Combine the seed setup with the independently varied evolution settings.
        let msuf32FromSetup : runParamEnhancerBuilder =
            fun context parameters -> msuf32MutationRate context (combineSetup context parameters)

        /// Validate explicit SimpleR mutation settings before execution IDs are calculated.
        let msuf32RFromSetup : runParamEnhancerBuilder = fun context parameters ->
            let combined = combineSetup context parameters
            if Map.containsKey ParamKeys.activationProbabilityKey combined.ParamMap then
                invalidArg "parameters" "SimpleR execution uses AMax; ActivationProbability belongs to SimpleS."
            ParameterValidation.validateValues [ParamKeys.aMaxKey] combined.ParamMap |> ParameterValidation.ensureValid "parameters"
            ParameterValidation.validateValues [ParamKeys.activatorMutationRateKey] combined.ParamMap |> ParameterValidation.ensureValid "parameters"
            msuf32MutationRate context combined

        /// Validate explicit SimpleS flip-rate settings before execution IDs are calculated.
        let msuf32SFromSetup : runParamEnhancerBuilder = fun context parameters ->
            let combined = combineSetup context parameters
            if Map.containsKey ParamKeys.aMaxKey combined.ParamMap then
                invalidArg "parameters" "SimpleS execution uses ActivationProbability; AMax belongs to SimpleR."
            ParameterValidation.validateValues [ParamKeys.activationProbabilityKey] combined.ParamMap |> ParameterValidation.ensureValid "parameters"
            ParameterValidation.validateValues [ParamKeys.activatorMutationRateKey] combined.ParamMap |> ParameterValidation.ensureValid "parameters"
            msuf32MutationRate context combined

        /// These families retain the supplied generation and share pool validation/calculation.
        let private continueFromSorterPoolSet = fromSorterPoolSet false
        let msrs32MutationRate = continueFromSorterPoolSet
        let msrs32MutationRateMax = continueFromSorterPoolSet
        let msuf624p3bMutationRate = continueFromSorterPoolSet
        let msrs24p3aOrthoPara = continueFromSorterPoolSet
        let msrs24p3aPoolModComp = continueFromSorterPoolSet
        let msrs24p3bOrthoPara = continueFromSorterPoolSet
        let mssi24p3bOrthoPara = continueFromSorterPoolSet

module RunParamBuilderNames =
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
        let sossMsuf32 = "sgd-soss-msuf32"
        [<Literal>]
        let evoMsuf32 = "sgd-evo-msuf32"
        [<Literal>]
        let evoMsuf32R = "sgd-evo-msuf32-r"
        [<Literal>]
        let sossMsuf32R = "sgd-soss-msuf32-r"
        [<Literal>]
        let evoMsuf32S = "sgd-evo-msuf32-s"
        [<Literal>]
        let sossMsuf32S = "sgd-soss-msuf32-s"
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
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sortableTests RunParamEnhancerBuilders.sortableTests |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterEvalStandard RunParamEnhancerBuilders.sorterEvalStandard |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterEvalMerge RunParamEnhancerBuilders.sorterEvalMerge |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterEvalPrefix RunParamEnhancerBuilders.sorterEvalPrefix |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterMutateStandard RunParamEnhancerBuilders.sorterMutateStandard |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sorterMutateStandardFormat RunParamEnhancerBuilders.sorterMutateStandardFormat |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msuf32MutationRate RunParamEnhancerBuilders.Sgd.msuf32MutationRate |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sossMsuf32 RunParamEnhancerBuilders.Sgd.msuf32FromSetup |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.evoMsuf32 RunParamEnhancerBuilders.Sgd.msuf32FromSetup |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.evoMsuf32R RunParamEnhancerBuilders.Sgd.msuf32RFromSetup |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sossMsuf32R RunParamEnhancerBuilders.Sgd.msuf32RFromSetup |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.evoMsuf32S RunParamEnhancerBuilders.Sgd.msuf32SFromSetup |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.sossMsuf32S RunParamEnhancerBuilders.Sgd.msuf32SFromSetup |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs32MutationRate RunParamEnhancerBuilders.Sgd.msrs32MutationRate |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs32MutationRateMax RunParamEnhancerBuilders.Sgd.msrs32MutationRateMax |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msuf624p3bMutationRate RunParamEnhancerBuilders.Sgd.msuf624p3bMutationRate |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara RunParamEnhancerBuilders.Sgd.msrs24p3aOrthoPara |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp RunParamEnhancerBuilders.Sgd.msrs24p3aPoolModComp |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.msrs24p3bOrthoPara RunParamEnhancerBuilders.Sgd.msrs24p3bOrthoPara |> ignore
                RunParamEnhancerBuilders.register RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara RunParamEnhancerBuilders.Sgd.mssi24p3bOrthoPara |> ignore
                registered <- true)
