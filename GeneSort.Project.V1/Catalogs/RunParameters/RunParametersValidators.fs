namespace GeneSort.Project.V1.Catalogs.RunParameters

open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd

type runParametersValidation = {
    RequiredKeys: string list
    ForbiddenKeys: string list
    RequiresPrefixSetup: bool
}

module RunParametersValidators =
    let general = { RequiredKeys = []; ForbiddenKeys = []; RequiresPrefixSetup = false }
    let prefix = { general with RequiresPrefixSetup = true }
    let seedPool = { general with RequiredKeys = SgdParameterValidation.seedPoolKeys }
    let msuf = { general with RequiredKeys = SgdParameterValidation.msufQueryKeys }
    let simpleR = { msuf with RequiredKeys = [ParamKeys.aMaxKey; ParamKeys.activatorMutationRateKey] @ msuf.RequiredKeys; ForbiddenKeys = [ParamKeys.activationProbabilityKey] }
    let simpleS = { msuf with RequiredKeys = [ParamKeys.activationProbabilityKey; ParamKeys.activatorMutationRateKey] @ msuf.RequiredKeys; ForbiddenKeys = [ParamKeys.aMaxKey] }

    let private fixedSettings requirePresent (context: runParametersPipelineContext) (parameters: runParameters) =
        match context.SetupParameters with
        | None -> [||]
        | Some setup ->
            setup.ParamMap |> Map.toArray |> Array.choose (fun (key, value) ->
                let actual = Map.tryFind key parameters.ParamMap
                if RunParameterRoles.isFixed (Run.parameterRole context.Run key)
                   && (requirePresent || actual.IsSome) && actual <> Some value then
                    Some (validationIssue.create ("Execution." + key) "This value is fixed by Setup and must be preserved.")
                else None)

    let validateFixedSettings = fixedSettings true

    let validateInput profile (context: runParametersPipelineContext) (parameters: runParameters) =
        let supplied = ParameterValidation.validateSpans "Execution" (ParameterValidation.ofMap parameters.ParamMap)
        let inherited = context.SetupParameters |> Option.map (fun setup ->
            ParameterValidation.validateSpans "Setup" (ParameterValidation.ofMap setup.ParamMap)) |> Option.defaultValue [||]
        let forbidden = profile.ForbiddenKeys |> List.choose (fun key ->
            if parameters.ParamMap.ContainsKey key || (context.SetupParameters |> Option.exists (fun setup -> setup.ParamMap.ContainsKey key)) then
                Some (validationIssue.create key "This setting belongs to another model family.") else None) |> List.toArray
        let prefix =
            match context.SetupParameters with
            | Some setup when profile.RequiresPrefixSetup ->
                ParameterValidation.requireKeys "Setup" RunParametersEnhancers.prefixSetupKeys (ParameterValidation.ofMap setup.ParamMap)
            | _ -> [||]
        Array.concat [| supplied; inherited; forbidden; prefix; fixedSettings false context parameters |]

    let validateEffective profile (context: runParametersPipelineContext) (parameters: runParameters) =
        Array.concat [|
            ParameterValidation.validateSpans "Execution" (ParameterValidation.ofMap parameters.ParamMap)
            ParameterValidation.requireKeys "Execution" profile.RequiredKeys (ParameterValidation.ofMap parameters.ParamMap)
            ParameterValidation.poolSizes "Execution" (ParameterValidation.ofMap parameters.ParamMap)
            ParameterValidation.resolvedValues "Execution" parameters.ParamMap
            profile.ForbiddenKeys |> List.choose (fun key ->
                if parameters.ParamMap.ContainsKey key then Some (validationIssue.create key "This setting belongs to another model family.") else None) |> List.toArray
            fixedSettings true context parameters
        |]
