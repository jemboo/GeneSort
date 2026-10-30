namespace GeneSort.Project.V1.Catalogs.SetupParameters

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

type setupParametersEnhancerContext = {
    Run: run
    QueryParamsFromSetupParams: setupQueryParamsBuilder
}

type setupParametersEnhancer = setupParametersEnhancerContext -> setupParameters -> setupParameters

/// Setup enhancers supply file references for the associated run parameters.
module SetupParametersEnhancers =
    /// Resolve required source files after applying setup settings.
    let withFileReferences outputTypes (context: setupParametersEnhancerContext) (parameters: setupParameters) =
        let outputs = outputTypes |> List.map (fun outputType -> SetupFileReferences.keyForOutputType outputType, outputType)
        match outputs |> List.countBy fst |> List.tryFind (fun (_, count) -> count > 1) with
        | Some (key, _) -> invalidArg "outputTypes" (sprintf "Multiple source files use '%s'; only one file per ID key can be linked." key)
        | None -> ()
        let references = outputs |> List.map (fun (key, outputType) ->
            match context.QueryParamsFromSetupParams parameters outputType with
            | Some query -> key, string query.Id
            | None -> invalidOp (sprintf "Setup query '%s' could not build %A for run '%s'.\n%s"
                                    context.Run.SetupQueryCatalogName outputType (string context.Run.RunName)
                                    (SetupParameters.reportKvps parameters)))
        references |> List.fold (fun map (key, value) -> Map.add key value map) parameters.ParamMap |> setupParameters.create

    let noOp : setupParametersEnhancer = fun _ parameters -> parameters
    let sortableTests : setupParametersEnhancer = noOp
    let sorterEval : setupParametersEnhancer =
        withFileReferences [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
    let sorterEvalPrefix : setupParametersEnhancer = sorterEval
    let sorterMutate : setupParametersEnhancer = sorterEval
    let sorterSgd : setupParametersEnhancer =
        withFileReferences [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]

    /// Seed-pool settings need no execution-only mutation or partition settings.
    let sossMsuf32 : setupParametersEnhancer = fun _ parameters ->
        parameters.ParamMap |> ParameterValidation.ofMap |> SgdParameterValidation.seedPool "Setup"
        |> ParameterValidation.ensureValid "parameters"
        parameters.WithGenerationCurrent(Some 0<generationNumber>)

    /// Standard seed queries include the full parameter settings and their setup references.
    let standardMsuf32 : setupParametersEnhancer = fun context parameters ->
        parameters.ParamMap |> ParameterValidation.ofMap |> SgdParameterValidation.msufSetup
        |> ParameterValidation.ensureValid "parameters"
        sossMsuf32 context parameters |> sorterSgd context

    /// Validate explicit SimpleR settings before queries are built.
    let private withSimpleR (enhancer: setupParametersEnhancer) : setupParametersEnhancer = fun context parameters ->
        if Map.containsKey ParamKeys.activationProbabilityKey parameters.ParamMap then
            invalidArg "parameters" "SimpleR setup uses AMax; ActivationProbability belongs to SimpleS."
        ParameterValidation.requireKeys "Setup" [ParamKeys.aMaxKey] (ParameterValidation.ofMap parameters.ParamMap)
        |> ParameterValidation.ensureValid "parameters"
        ParameterValidation.validateValues [ParamKeys.aMaxKey] parameters.ParamMap |> ParameterValidation.ensureValid "parameters"
        enhancer context parameters

    let standardMsuf32R = withSimpleR standardMsuf32
    let sossMsuf32R = withSimpleR sossMsuf32

    /// Validate explicit SimpleS settings before creating seed file references.
    let private withSimpleS (enhancer: setupParametersEnhancer) : setupParametersEnhancer = fun context parameters ->
        if Map.containsKey ParamKeys.aMaxKey parameters.ParamMap then
            invalidArg "parameters" "SimpleS setup uses ActivationProbability; AMax belongs to SimpleR."
        ParameterValidation.requireKeys "Setup" [ParamKeys.activationProbabilityKey] (ParameterValidation.ofMap parameters.ParamMap)
        |> ParameterValidation.ensureValid "parameters"
        ParameterValidation.validateValues [ParamKeys.activationProbabilityKey] parameters.ParamMap |> ParameterValidation.ensureValid "parameters"
        enhancer context parameters

    let standardMsuf32S = withSimpleS standardMsuf32
    let sossMsuf32S = withSimpleS sossMsuf32

