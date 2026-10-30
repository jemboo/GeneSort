namespace GeneSort.Project.V1.Catalogs.SetupParameters

open GeneSort.Eval.V1
open GeneSort.Project.V1.Parameters

/// Pure parameter transformations. Validation and file identities are separate stages.
module SetupParametersEnhancers =
    let noOp : setupParametersTransform = fun _ parameters -> parameters
    let initializeGeneration : setupParametersTransform = fun _ parameters ->
        parameters.WithGenerationCurrent(Some 0<generationNumber>)

    let resolve (parameters: setupParameters) =
        let resolved = parameters.ParamMap |> ParameterResolution.resolveSelection |> ParameterResolution.resolveEvaluationMeasures
        if resolved = parameters.ParamMap then parameters else setupParameters.create resolved
