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
type runParametersEnhancerContext =
    private {
        queryParamsFromRunParams: runQueryParamsBuilder
        run: run
        setupParameters: setupParameters option
    }

    static member create
            (queryParamsFromRunParams: runQueryParamsBuilder)
            (run: run)
            : runParametersEnhancerContext =
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


type runParametersEnhancer = runParametersEnhancerContext -> runParameters -> runParameters

module RunParametersEnhancers =
    let private queryParamsForRun (context: runParametersEnhancerContext) (rp: runParameters) =
        context.QueryParamsFromRunParams rp (geneSortDataType.Run context.Run.RunName) |> Option.get

    let private inheritSetup (context: runParametersEnhancerContext) (rp: runParameters) =
        match context.SetupParameters with
        | None -> rp
        | Some setup ->
            (SetupFileReferences.withoutReferences setup).ParamMap
            |> Map.fold (fun map key value -> if Map.containsKey key map then map else Map.add key value map) rp.ParamMap
            |> runParameters.create

    let private initializeRun (context: runParametersEnhancerContext) (rp: runParameters) =
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

    let sorterEvalPrefix (context: runParametersEnhancerContext) (rp: runParameters) =
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

        let private fromSorterPoolSet setGenerationCurrent : runParametersEnhancer =
            fun host parameters ->
                let rp = (inheritSetup host parameters).ParamMap |> ParameterValidation.resolveSelection |> runParameters.create
                ParameterValidation.requireKeys "Execution" SgdParameterValidation.seedPoolKeys (ParameterValidation.ofMap rp.ParamMap)
                |> ParameterValidation.ensureValid "parameters"
                ParameterValidation.validateValues SgdParameterValidation.seedPoolKeys rp.ParamMap
                |> ParameterValidation.ensureValid "parameters"
                ParameterValidation.poolSizes "Execution" (ParameterValidation.ofMap rp.ParamMap)
                |> ParameterValidation.ensureValid "parameters"
                let enhanced = rp
                let enhanced = if setGenerationCurrent then enhanced.WithGenerationCurrent(Some 0<generationNumber>) else enhanced
                let qp = host.QueryParamsFromRunParams enhanced (geneSortDataType.Run host.Run.RunName)
                enhanced.WithRunFinished(Some false).WithId(Some qp.Value.Id).WithRunName(Some host.Run.RunName)

        let msuf32MutationRate : runParametersEnhancer = fun context parameters ->
            parameters.ParamMap |> ParameterValidation.ofMap |> SgdParameterValidation.msufExecution
            |> ParameterValidation.ensureValid "parameters"
            fromSorterPoolSet true context parameters

        let private combineSetup (context: runParametersEnhancerContext) (parameters: runParameters) =
            match context.SetupParameters with
            | None -> parameters
            | Some setup ->
                Map.fold (fun map key value -> Map.add key value map) setup.ParamMap parameters.ParamMap
                |> runParameters.create

        /// Combine the seed setup with the independently varied evolution settings.
        let msuf32FromSetup : runParametersEnhancer =
            fun context parameters -> msuf32MutationRate context (combineSetup context parameters)

        /// Validate explicit SimpleR mutation settings before execution IDs are calculated.
        let msuf32RFromSetup : runParametersEnhancer = fun context parameters ->
            let combined = combineSetup context parameters
            if Map.containsKey ParamKeys.activationProbabilityKey combined.ParamMap then
                invalidArg "parameters" "SimpleR execution uses AMax; ActivationProbability belongs to SimpleS."
            ParameterValidation.validateValues [ParamKeys.aMaxKey] combined.ParamMap |> ParameterValidation.ensureValid "parameters"
            ParameterValidation.validateValues [ParamKeys.activatorMutationRateKey] combined.ParamMap |> ParameterValidation.ensureValid "parameters"
            msuf32MutationRate context combined

        /// Validate explicit SimpleS flip-rate settings before execution IDs are calculated.
        let msuf32SFromSetup : runParametersEnhancer = fun context parameters ->
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

