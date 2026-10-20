namespace GeneSort.Project.V1.Catalogs.SetupParameters

open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Queries

type setupEnhancerContext = {
    Run: run
    QueryParamsFromSetupParams: setupQueryParamsBuilder
}

type setupEnhancerFunction = setupEnhancerContext -> setupParameters -> setupParameters

module SetupEnhancerFunctionNames =
    [<Literal>]
    let noOp = "setup-enhancer.no-op"
    [<Literal>]
    let sortableTests = "setup-enhancer.sortable-tests"
    [<Literal>]
    let sorterEval = "setup-enhancer.sorter-eval"
    [<Literal>]
    let sorterEvalPrefix = "setup-enhancer.sorter-eval-prefix"
    [<Literal>]
    let sorterMutate = "setup-enhancer.sorter-mutate"
    [<Literal>]
    let sorterSgd = "setup-enhancer.sorter-sgd"

/// Setup enhancers supply file references for the associated run parameters.
module SetupEnhancerCatalog =
    /// Resolve required source files after applying setup settings.
    let withFileReferences outputTypes (context: setupEnhancerContext) (parameters: setupParameters) =
        let outputs = outputTypes |> List.map (fun outputType -> SetupFileReferences.keyForOutputType outputType, outputType)
        match outputs |> List.countBy fst |> List.tryFind (fun (_, count) -> count > 1) with
        | Some (key, _) -> invalidArg "outputTypes" (sprintf "Multiple source files use '%s'; only one file per ID key can be linked." key)
        | None -> ()
        let references = outputs |> List.map (fun (key, outputType) ->
            match context.QueryParamsFromSetupParams parameters outputType with
            | Some query -> key, string query.Id
            | None -> invalidOp (sprintf "Setup query '%s' could not build %A for run '%s'.\n%s"
                                    context.Run.SetupQueryCatalogName outputType (string context.Run.RunName)
                                    (SetupParameters.reportKvps parameters)))
        references |> List.fold (fun map (key, value) -> Map.add key value map) parameters.ParamMap |> setupParameters.create

    let noOp : setupEnhancerFunction = fun _ parameters -> parameters
    let sortableTests : setupEnhancerFunction = noOp
    let sorterEvalPrefix : setupEnhancerFunction = noOp
    let sorterEval : setupEnhancerFunction =
        withFileReferences [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
    let sorterMutate : setupEnhancerFunction = sorterEval
    let sorterSgd : setupEnhancerFunction =
        withFileReferences [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]

    let private syncRoot = obj ()
    let mutable private functions =
        Map.ofList [
            SetupEnhancerFunctionNames.noOp, noOp
            SetupEnhancerFunctionNames.sortableTests, sortableTests
            SetupEnhancerFunctionNames.sorterEval, sorterEval
            SetupEnhancerFunctionNames.sorterEvalPrefix, sorterEvalPrefix
            SetupEnhancerFunctionNames.sorterMutate, sorterMutate
            SetupEnhancerFunctionNames.sorterSgd, sorterSgd
        ]

    let register name (enhancer: setupEnhancerFunction) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A setup enhancer function name cannot be empty."
        lock syncRoot (fun () ->
            if functions.ContainsKey name then
                invalidOp (sprintf "Setup enhancer '%s' is already registered." name)
            functions <- functions.Add(name, enhancer))

    let get name : setupEnhancerFunction =
        lock syncRoot (fun () ->
            match functions.TryFind name with
            | Some enhancer -> enhancer
            | None -> invalidOp (sprintf "No setup enhancer named '%s' is registered." name))
