namespace GeneSort.Project.V1.Parameters

open System

module ParameterResolution =
    /// Resolve an explicit All choice only once the paired pool capacity is available.
    let resolveSelection parameters =
        match Map.tryFind ParamKeys.selectedSorterCountPerPoolKey parameters, Map.tryFind ParamKeys.sorterCountPerPoolKey parameters with
        | Some selected, Some capacity when System.String.Equals(selected, "All", StringComparison.OrdinalIgnoreCase) ->
            Map.add ParamKeys.selectedSorterCountPerPoolKey capacity parameters
        | _ -> parameters

    /// Keep a declared initial/evolution measure pair correlated within each sweep row.
    let resolveEvaluationMeasures parameters =
        match Map.tryFind ParamKeys.sorterEvalMeasureInitialKey parameters with
        | Some "SameAsEvaluation" ->
            match Map.tryFind ParamKeys.sorterEvalMeasureKey parameters with
            | Some value -> Map.add ParamKeys.sorterEvalMeasureInitialKey value parameters
            | None -> parameters
        | _ -> parameters

