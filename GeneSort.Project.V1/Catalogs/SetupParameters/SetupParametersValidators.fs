namespace GeneSort.Project.V1.Catalogs.SetupParameters

open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd

type setupParametersValidation = {
    RequiredKeys: string list
    ForbiddenKeys: string list
}

module SetupParametersValidators =
    let general = { RequiredKeys = []; ForbiddenKeys = [] }
    let seedPool = { general with RequiredKeys = SgdParameterValidation.seedPoolKeys }
    let msuf = { general with RequiredKeys = SgdParameterValidation.msufQueryKeys }
    let simpleR profile = { profile with RequiredKeys = ParamKeys.aMaxKey :: profile.RequiredKeys; ForbiddenKeys = [ParamKeys.activationProbabilityKey] }
    let simpleS profile = { profile with RequiredKeys = ParamKeys.activationProbabilityKey :: profile.RequiredKeys; ForbiddenKeys = [ParamKeys.aMaxKey] }

    let validateInput profile (_: setupParametersPipelineContext) (parameters: setupParameters) =
        Array.append
            (ParameterValidation.validateSpans "Setup" (ParameterValidation.ofMap parameters.ParamMap))
            (profile.ForbiddenKeys |> List.choose (fun key ->
                if parameters.ParamMap.ContainsKey key then Some (validationIssue.create key "This setting belongs to another model family.") else None) |> List.toArray)

    let validateEffective profile (context: setupParametersPipelineContext) (parameters: setupParameters) =
        Array.concat [|
            validateInput profile context parameters
            ParameterValidation.requireKeys "Setup" profile.RequiredKeys (ParameterValidation.ofMap parameters.ParamMap)
            ParameterValidation.poolSizes "Setup" (ParameterValidation.ofMap parameters.ParamMap)
            ParameterValidation.resolvedValues "Setup" parameters.ParamMap
        |]
