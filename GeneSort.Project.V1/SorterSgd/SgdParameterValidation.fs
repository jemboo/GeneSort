namespace GeneSort.Project.V1.SorterSgd

open GeneSort.Project.V1.Parameters

/// Requirements shared by parameter generation, execution and authoring.
module SgdParameterValidation =
    let seedPoolKeys = [ParamKeys.sorterCountPerPoolKey; ParamKeys.sorterCountPerPoolSetKey]
    let msufQueryKeys = seedPoolKeys @ [
        ParamKeys.codeModKey; ParamKeys.paraRateKey; ParamKeys.selfSymRateKey; ParamKeys.mutationModKey
        ParamKeys.seedModificationRateKey; ParamKeys.modificationRateKey; ParamKeys.mutatorVariantKey
    ]
    let standardEvolutionKeys = [ParamKeys.adjustCeLengthFrequencyKey; ParamKeys.generationIntervalLastKey]

    let private validate phase required spans =
        Array.concat [|
            ParameterValidation.validateSpans phase spans
            ParameterValidation.requireKeys phase required spans
            ParameterValidation.poolSizes phase spans
        |]

    let seedPool phase spans = validate phase seedPoolKeys spans
    let msufSetup spans = validate "Setup" msufQueryKeys spans
    let msufExecution spans = validate "Execution" msufQueryKeys spans

    let effectiveSpans setup execution =
        setup @ execution |> Map.ofList |> Map.toList

    let standardMsuf setup execution =
        Array.concat [|
            msufSetup setup
            ParameterValidation.validateSpans "Execution" execution
            ParameterValidation.requireKeys "Parameters" standardEvolutionKeys (effectiveSpans setup execution)
            if execution |> List.exists (fun (key, _) -> List.contains key seedPoolKeys) then
                ParameterValidation.poolSizes "Execution" (effectiveSpans setup execution)
            else [||]
        |] |> Array.distinctBy (fun issue -> issue.Field, issue.Message)
