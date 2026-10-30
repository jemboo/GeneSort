namespace GeneSort.Project.V1.Parameters

/// Validators inspect a collection without changing it. Finalization computes
/// identities/references only after the effective collection has passed validation.
type parameterPipeline<'context, 'parameters> = {
    ValidateInput: 'context -> 'parameters -> validationIssue[]
    Enhance: 'context -> 'parameters -> 'parameters
    ValidateEffective: 'context -> 'parameters -> validationIssue[]
    Finalize: 'context -> 'parameters -> 'parameters
}

module ParameterPipeline =
    let tryApply definition context parameters =
        match definition.ValidateInput context parameters with
        | issues when issues.Length > 0 -> Error issues
        | _ ->
            let enhanced = definition.Enhance context parameters
            match definition.ValidateEffective context enhanced with
            | issues when issues.Length > 0 -> Error issues
            | _ -> Ok (definition.Finalize context enhanced)

    let apply definition context parameters =
        match tryApply definition context parameters with
        | Ok enhanced -> enhanced
        | Error issues ->
            issues
            |> Array.map (fun issue -> issue.Field + ": " + issue.Message)
            |> String.concat "\n"
            |> invalidArg "parameters"
