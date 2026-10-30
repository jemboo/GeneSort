namespace GeneSort.Project.V1.Catalogs.RunParameters

/// Validation is selected together with enhancement, so callers cannot mismatch them.
module RunParametersPipelines =
    let private create profile enhance finalize : runParametersPipeline = {
        ValidateInput = RunParametersValidators.validateInput profile
        Enhance = fun context parameters -> enhance context parameters |> RunParametersEnhancers.resolve
        ValidateEffective = RunParametersValidators.validateEffective profile
        Finalize = finalize
    }

    let sortableTests = create RunParametersValidators.general RunParametersEnhancers.noOp RunParametersFinalizers.standard
    let sorterEvalStandard = create RunParametersValidators.general RunParametersEnhancers.inheritSetup RunParametersFinalizers.standard
    let sorterEvalMerge = sorterEvalStandard
    let sorterEvalPrefix = create RunParametersValidators.prefix RunParametersEnhancers.sorterEvalPrefix RunParametersFinalizers.standard
    let sorterMutateStandard = sorterEvalStandard
    let sorterMutateStandardFormat = sorterEvalStandard

    module Sgd =
        let msuf32MutationRate = create RunParametersValidators.msuf RunParametersEnhancers.initializeSgd RunParametersFinalizers.sgd
        let msuf32FromSetup = create RunParametersValidators.msuf RunParametersEnhancers.msuf32FromSetup RunParametersFinalizers.sgd
        let msuf32RFromSetup = create RunParametersValidators.simpleR RunParametersEnhancers.msuf32FromSetup RunParametersFinalizers.sgd
        let msuf32SFromSetup = create RunParametersValidators.simpleS RunParametersEnhancers.msuf32FromSetup RunParametersFinalizers.sgd
        let msrs32MutationRate = create RunParametersValidators.seedPool RunParametersEnhancers.continueSgd RunParametersFinalizers.sgd
        let msrs32MutationRateMax = msrs32MutationRate
        let msuf624p3bMutationRate = msrs32MutationRate
        let msrs24p3aOrthoPara = msrs32MutationRate
        let msrs24p3aPoolModComp = msrs32MutationRate
        let msrs24p3bOrthoPara = msrs32MutationRate
        let mssi24p3bOrthoPara = msrs32MutationRate
