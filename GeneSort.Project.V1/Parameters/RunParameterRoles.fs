namespace GeneSort.Project.V1.Parameters

open GeneSort.Project.V1.Catalogs.Evolution

/// The meaning of a setting at the boundary between preparation and execution.
[<RequireQualifiedAccess>]
type parameterRole =
    | SetupOnly
    | InheritedReadOnly
    | InheritedOverridable
    | ExecutionOnly

/// Explicit policies for the workflows, independent of presentation and storage.
module RunParameterRoles =
    let sorterEval = Map.ofList [
        ParamKeys.channelCountKey, parameterRole.InheritedReadOnly
        ParamKeys.prefixLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.mergeLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.simpleSorterModelTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.sortableDataFormatKey, parameterRole.InheritedReadOnly
        ParamKeys.rngTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.excludeSelfCeKey, parameterRole.InheritedReadOnly
        ParamKeys.collectSortableTestsKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterCountKey, parameterRole.InheritedReadOnly
        ParamKeys.aMaxKey, parameterRole.InheritedReadOnly
        ParamKeys.activationProbabilityKey, parameterRole.InheritedReadOnly
        ParamKeys.activatorMutationRateKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterEvalTypeKey, parameterRole.InheritedOverridable
    ]

    let sorterMutate = Map.ofList [
        ParamKeys.channelCountKey, parameterRole.InheritedReadOnly
        ParamKeys.prefixLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.mergeLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.simpleSorterModelTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.sortableDataFormatKey, parameterRole.InheritedReadOnly
        ParamKeys.rngTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.excludeSelfCeKey, parameterRole.InheritedReadOnly
        ParamKeys.collectSortableTestsKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterCountKey, parameterRole.InheritedReadOnly
        ParamKeys.aMaxKey, parameterRole.InheritedReadOnly
        ParamKeys.activationProbabilityKey, parameterRole.InheritedReadOnly
        ParamKeys.activatorMutationRateKey, parameterRole.InheritedReadOnly
        ParamKeys.mutatorParamsKey, parameterRole.InheritedReadOnly
        ParamKeys.modificationRateKey, parameterRole.InheritedReadOnly
        ParamKeys.mutatorVariantKey, parameterRole.InheritedReadOnly
        ParamKeys.mutationModKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterChildCountKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterEvalMeasureKey, parameterRole.InheritedReadOnly
        ParamKeys.seedSorterPoolSelectionTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterEvalTypeKey, parameterRole.InheritedOverridable
    ]

    let evolution = Map.ofList [
        ParamKeys.setupSortableTestsDatabaseNameKey, parameterRole.SetupOnly
        ParamKeys.setupSorterSetEvalsDatabaseNameKey, parameterRole.SetupOnly
        ParamKeys.channelCountKey, parameterRole.InheritedReadOnly
        ParamKeys.prefixLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.mergeLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterLibIdKey, parameterRole.InheritedReadOnly
        ParamKeys.simpleSorterModelTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.sortableDataFormatKey, parameterRole.InheritedReadOnly
        ParamKeys.rngTypeKey, parameterRole.InheritedReadOnly
        ParamKeys.excludeSelfCeKey, parameterRole.InheritedReadOnly
        ParamKeys.collectSortableTestsKey, parameterRole.InheritedReadOnly
        ParamKeys.aMaxKey, parameterRole.InheritedReadOnly
        ParamKeys.activationProbabilityKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterCountPerPoolKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterCountPerPoolSetKey, parameterRole.InheritedReadOnly
        ParamKeys.sorterPoolCountKey, parameterRole.InheritedReadOnly
        ParamKeys.mutationModKey, parameterRole.InheritedReadOnly
        ParamKeys.seedSorterPoolSelectionTypeKey, parameterRole.SetupOnly
        ParamKeys.sorterEvalMeasureInitialKey, parameterRole.SetupOnly
        ParamKeys.controlledSeedAMaxKey, parameterRole.SetupOnly
        ParamKeys.modificationRateKey, parameterRole.InheritedOverridable
        ParamKeys.activatorMutationRateKey, parameterRole.InheritedOverridable
        ParamKeys.seedModificationRateKey, parameterRole.InheritedOverridable
        ParamKeys.orthoRateKey, parameterRole.InheritedOverridable
        ParamKeys.paraRateKey, parameterRole.InheritedOverridable
        ParamKeys.selfSymRateKey, parameterRole.InheritedOverridable
        ParamKeys.mutatorVariantKey, parameterRole.InheritedOverridable
        ParamKeys.sorterEvalMeasureKey, parameterRole.InheritedOverridable
        ParamKeys.generationIntervalLastKey, parameterRole.ExecutionOnly
        ParamKeys.seedSossKey, parameterRole.ExecutionOnly
        ParamKeys.partitionCountKey, parameterRole.ExecutionOnly
        ParamKeys.generationsPerSubsetKey, parameterRole.ExecutionOnly
        ParamKeys.generationsPerMergedSetKey, parameterRole.ExecutionOnly
    ]

    // Continuation deliberately permits branching and resizing imported pools.
    let continuation =
        evolution
        |> Map.add ParamKeys.mutationModKey parameterRole.InheritedOverridable
        |> Map.add ParamKeys.sorterCountPerPoolKey parameterRole.InheritedOverridable
        |> Map.add ParamKeys.sorterCountPerPoolSetKey parameterRole.InheritedOverridable
        |> Map.add ParamKeys.sorterPoolCountKey parameterRole.InheritedOverridable
        |> Map.add ParamKeys.sourceProjectNameKey parameterRole.InheritedReadOnly
        |> Map.add ParamKeys.sourceDbNameKey parameterRole.InheritedReadOnly
        |> Map.add ParamKeys.sourceRunNameKey parameterRole.InheritedReadOnly
        |> Map.add ParamKeys.sourceGenerationIntervalKey parameterRole.InheritedReadOnly

    let forWorkflow executionFunction isContinuation key =
        let roles =
            match executionFunction with
            | ExecuteFunctionNames.sorterEval -> sorterEval
            | ExecuteFunctionNames.sorterMutate -> sorterMutate
            | ExecuteFunctionNames.standard | ExecuteFunctionNames.soss -> if isContinuation then continuation else evolution
            | _ -> Map.empty
        Map.tryFind key roles |> Option.defaultValue parameterRole.ExecutionOnly

    let isFixed = function
        | parameterRole.SetupOnly | parameterRole.InheritedReadOnly -> true
        | _ -> false

    /// Old definitions repeated setup spans in Execution. These are inherited,
    /// not another Cartesian dimension. Keep the setup span (and its identity).
    let withoutRepeatedSetup executionFunction isContinuation setup execution =
        execution |> List.filter (fun (key, values) ->
            not (isFixed (forWorkflow executionFunction isContinuation key)
                 && List.tryFind (fst >> (=) key) setup = Some (key, values)))
