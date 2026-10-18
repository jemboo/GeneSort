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
    let sorterMutate = "setup-enhancer.sorter-mutate"
    [<Literal>]
    let sorterSgd = "setup-enhancer.sorter-sgd"

/// Setup enhancers are independent of run enhancers. Built-ins currently preserve all parameters.
module SetupEnhancerCatalog =
    let noOp : setupEnhancerFunction = fun _ parameters -> parameters
    let sortableTests : setupEnhancerFunction = noOp
    let sorterEval : setupEnhancerFunction = noOp
    let sorterMutate : setupEnhancerFunction = noOp
    let sorterSgd : setupEnhancerFunction = noOp

    let private syncRoot = obj ()
    let mutable private functions =
        Map.ofList [
            SetupEnhancerFunctionNames.noOp, noOp
            SetupEnhancerFunctionNames.sortableTests, sortableTests
            SetupEnhancerFunctionNames.sorterEval, sorterEval
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
