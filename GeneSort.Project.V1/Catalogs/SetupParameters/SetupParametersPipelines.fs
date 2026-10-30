namespace GeneSort.Project.V1.Catalogs.SetupParameters

/// Each catalog entry binds the validators to the transformation they describe.
module SetupParametersPipelines =
    let private create profile enhance finalize : setupParametersPipeline = {
        ValidateInput = SetupParametersValidators.validateInput profile
        Enhance = fun context parameters -> enhance context parameters |> SetupParametersEnhancers.resolve
        ValidateEffective = SetupParametersValidators.validateEffective profile
        Finalize = finalize
    }

    let noOp = create SetupParametersValidators.general SetupParametersEnhancers.noOp SetupParametersFinalizers.noOp
    let sortableTests = noOp
    let sorterEval = { noOp with Finalize = SetupParametersFinalizers.sorterEval }
    let sorterEvalPrefix = sorterEval
    let sorterMutate = sorterEval
    let sorterSgd = { noOp with Finalize = SetupParametersFinalizers.sorterSgd }

    let private sgd profile finalize = create profile SetupParametersEnhancers.initializeGeneration finalize
    let sossMsuf32 = sgd SetupParametersValidators.seedPool SetupParametersFinalizers.noOp
    let standardMsuf32 = sgd SetupParametersValidators.msuf SetupParametersFinalizers.sorterSgd
    let standardMsuf32R = sgd (SetupParametersValidators.simpleR SetupParametersValidators.msuf) SetupParametersFinalizers.sorterSgd
    let sossMsuf32R = sgd (SetupParametersValidators.simpleR SetupParametersValidators.seedPool) SetupParametersFinalizers.noOp
    let standardMsuf32S = sgd (SetupParametersValidators.simpleS SetupParametersValidators.msuf) SetupParametersFinalizers.sorterSgd
    let sossMsuf32S = sgd (SetupParametersValidators.simpleS SetupParametersValidators.seedPool) SetupParametersFinalizers.noOp
