namespace GeneSort.Project.V1.Catalogs.RunParameters

open GeneSort.Eval.V1
open GeneSort.Project.V1.Parameters

/// Pure inheritance and derivation; validation and execution IDs are separate stages.
module RunParametersEnhancers =
    let noOp : runParametersTransform = fun _ parameters -> parameters

    let inheritSetup (context: runParametersPipelineContext) (parameters: runParameters) =
        match context.SetupParameters with
        | None -> parameters
        | Some setup ->
            (SetupFileReferences.withoutReferences setup).ParamMap
            |> Map.fold (fun map key value -> if Map.containsKey key map then map else Map.add key value map) parameters.ParamMap
            |> runParameters.create

    let prefixSetupKeys = [ParamKeys.rngTypeKey; ParamKeys.prefixLibIdKey; ParamKeys.simpleSorterModelTypeKey;
                          ParamKeys.sortableDataFormatKey; ParamKeys.sorterCountKey]

    let sorterEvalPrefix (context: runParametersPipelineContext) (parameters: runParameters) =
        match context.SetupParameters with
        | None -> parameters
        | Some setup ->
            prefixSetupKeys @ [ParamKeys.aMaxKey; ParamKeys.activationProbabilityKey; ParamKeys.activatorMutationRateKey]
            |> List.fold (fun map key ->
                match Map.tryFind key setup.ParamMap with
                | Some value -> Map.add key value map
                | None -> map) parameters.ParamMap
            |> runParameters.create

    let private fromSorterPoolSet resetGeneration : runParametersTransform = fun context parameters ->
        let inherited = inheritSetup context parameters
        if resetGeneration then inherited.WithGenerationCurrent(Some 0<generationNumber>) else inherited

    let initializeSgd = fromSorterPoolSet true
    let continueSgd = fromSorterPoolSet false

    let msuf32FromSetup (context: runParametersPipelineContext) (parameters: runParameters) =
        let combined =
            match context.SetupParameters with
            | None -> parameters
            | Some setup -> Map.fold (fun map key value -> Map.add key value map) setup.ParamMap parameters.ParamMap |> runParameters.create
        initializeSgd context combined

    let resolve (parameters: runParameters) =
        let resolved = parameters.ParamMap |> ParameterResolution.resolveSelection |> ParameterResolution.resolveEvaluationMeasures
        if resolved = parameters.ParamMap then parameters else runParameters.create resolved
