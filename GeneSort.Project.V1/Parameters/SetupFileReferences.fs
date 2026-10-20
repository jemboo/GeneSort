namespace GeneSort.Project.V1.Parameters

open GeneSort.Project.V1

module SetupFileReferences =

    let keyForOutputType = function
        | geneSortDataType.SortableTests _ -> ParamKeys.setupSortableTestsIdKey
        | geneSortDataType.SortableTestsWithPrefix _ -> ParamKeys.setupSortableTestsWithPrefixIdKey
        | geneSortDataType.SorterPoolSet _ -> ParamKeys.setupSorterPoolSetIdKey
        | geneSortDataType.SorterPoolSetSummarySet _ -> ParamKeys.setupSorterPoolSetSummarySetIdKey
        | geneSortDataType.SorterSet _ -> ParamKeys.setupSorterSetIdKey
        | geneSortDataType.SorterModelSet _ -> ParamKeys.setupSorterModelSetIdKey
        | geneSortDataType.SorterSetEval _ -> ParamKeys.setupSorterSetEvalIdKey
        | geneSortDataType.SorterPoolBinsSetSeries _ -> ParamKeys.setupSorterPoolBinsSetSeriesIdKey
        | geneSortDataType.SorterPoolSetHistory _ -> ParamKeys.setupSorterPoolSetHistoryIdKey
        | other -> invalidArg "outputType" (sprintf "%A cannot be linked as a setup output." other)

    let keys = [
        ParamKeys.setupParametersIdKey
        ParamKeys.setupSortableTestsIdKey
        ParamKeys.setupSortableTestsWithPrefixIdKey
        ParamKeys.setupSorterPoolSetIdKey
        ParamKeys.setupSorterPoolSetSummarySetIdKey
        ParamKeys.setupSorterSetIdKey
        ParamKeys.setupSorterModelSetIdKey
        ParamKeys.setupSorterSetEvalIdKey
        ParamKeys.setupSorterPoolBinsSetSeriesIdKey
        ParamKeys.setupSorterPoolSetHistoryIdKey
    ]

    let fromRunParameters (runParams: runParameters) : (string * string) list =
        keys |> List.choose (fun key -> runParams.ParamMap.TryFind key |> Option.map (fun value -> key, value))

    let fromSetupParameters (setupParams: setupParameters) : (string * string) list =
        keys |> List.choose (fun key -> setupParams.ParamMap.TryFind key |> Option.map (fun value -> key, value))

    /// Source file references are metadata on setup parameters, not part of setup identities.
    let withoutReferences (parameters: setupParameters) =
        keys |> List.fold (fun map key -> Map.remove key map) parameters.ParamMap |> setupParameters.create

    /// Replace links as a unit so stale links cannot survive pairing with a new setup.
    let attach references (parameters: runParameters) =
        let cleared = keys |> List.fold (fun map key -> Map.remove key map) parameters.ParamMap
        references |> List.fold (fun map (key, value) -> Map.add key value map) cleared |> runParameters.create
