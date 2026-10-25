namespace GeneSort.Project.V1.Catalogs.SetupParameters

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters

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
    [<Literal>]
    let sossMsuf32 = "setup-enhancer.soss-msuf32"
    [<Literal>]
    let standardMsuf32 = "setup-enhancer.standard-msuf32"
    [<Literal>]
    let standardMsuf32R = "setup-enhancer.standard-msuf32-r"
    [<Literal>]
    let sossMsuf32R = "setup-enhancer.soss-msuf32-r"
    [<Literal>]
    let standardMsuf32S = "setup-enhancer.standard-msuf32-s"
    [<Literal>]
    let sossMsuf32S = "setup-enhancer.soss-msuf32-s"

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

    /// Seed-pool settings need no execution-only mutation or partition settings.
    let sossMsuf32 : setupEnhancerFunction = fun _ parameters ->
        let perPool = parameters.GetSorterCountPerPool()
                      |> Option.defaultWith (fun () -> invalidOp "Soss setup requires SorterCountPerPool.")
        let total = parameters.GetSorterCountPerPoolSet()
                    |> Option.defaultWith (fun () -> invalidOp "Soss setup requires SorterCountPerPoolSet.")
        if int perPool <= 0 || int total <= 0 || int total % int perPool <> 0 then
            invalidArg "parameters" "The total sorter count must be positive and divisible by the positive per-pool count."
        let prefix = prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA
        parameters.WithSorterPoolCount(Some (UMX.tag (int total / int perPool)))
                  .WithGenerationCurrent(Some 0<generationNumber>)
                  .WithRngType(Some rngType.Lcg)
                  .WithPrefixLibId(Some prefix)
                  .WithSortingWidth(Some prefix.SortingWidth)
                  .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf4)
                  .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
                  .WithExcludeSelfCe(Some (UMX.tag true))
                  .WithSeedSorterPoolSelectionType(Some (sorterSelectionType.GuidOrder 512<sorterCount>))
                  .WithSorterEvalMeasureInitial(Some SorterEvalMeasure.stageBiasedFilterUnsorted)
                  .WithSorterEvalMeasure(Some SorterEvalMeasure.stageBiasedFilterUnsorted)

    /// Standard seed queries include the full parameter settings and their setup references.
    let standardMsuf32 : setupEnhancerFunction = fun context parameters ->
        sossMsuf32 context parameters |> sorterSgd context

    /// SimpleR defaults apply before queries are built, so AMax participates in seed identity.
    let private withSimpleR (enhancer: setupEnhancerFunction) : setupEnhancerFunction = fun context parameters ->
        if Map.containsKey ParamKeys.activationProbabilityKey parameters.ParamMap then
            invalidArg "parameters" "SimpleR setup uses AMax; ActivationProbability belongs to SimpleS."
        let parameters =
            if Map.containsKey ParamKeys.aMaxKey parameters.ParamMap then parameters
            else parameters.WithAMax(Some 3)
        match parameters.GetAMax() with
        | Some maximum when maximum >= 0 -> enhancer context parameters
        | _ -> invalidArg "parameters" "SimpleR setup requires a nonnegative integer AMax."

    let standardMsuf32R = withSimpleR standardMsuf32
    let sossMsuf32R = withSimpleR sossMsuf32

    /// Apply the boolean activation default before creating seed file references.
    let private withSimpleS (enhancer: setupEnhancerFunction) : setupEnhancerFunction = fun context parameters ->
        if Map.containsKey ParamKeys.aMaxKey parameters.ParamMap then
            invalidArg "parameters" "SimpleS setup uses ActivationProbability; AMax belongs to SimpleR."
        let parameters =
            if Map.containsKey ParamKeys.activationProbabilityKey parameters.ParamMap then parameters
            else parameters.WithActivationProbability(Some 0.5)
        match parameters.GetActivationProbability() with
        | Some probability when System.Double.IsFinite probability && probability >= 0.0 && probability <= 1.0 ->
            enhancer context parameters
        | _ -> invalidArg "parameters" "SimpleS setup requires a finite ActivationProbability between 0 and 1."

    let standardMsuf32S = withSimpleS standardMsuf32
    let sossMsuf32S = withSimpleS sossMsuf32

    let private syncRoot = obj ()
    let mutable private functions =
        Map.ofList [
            SetupEnhancerFunctionNames.noOp, noOp
            SetupEnhancerFunctionNames.sortableTests, sortableTests
            SetupEnhancerFunctionNames.sorterEval, sorterEval
            SetupEnhancerFunctionNames.sorterEvalPrefix, sorterEvalPrefix
            SetupEnhancerFunctionNames.sorterMutate, sorterMutate
            SetupEnhancerFunctionNames.sorterSgd, sorterSgd
            SetupEnhancerFunctionNames.sossMsuf32, sossMsuf32
            SetupEnhancerFunctionNames.standardMsuf32, standardMsuf32
            SetupEnhancerFunctionNames.standardMsuf32R, standardMsuf32R
            SetupEnhancerFunctionNames.sossMsuf32R, sossMsuf32R
            SetupEnhancerFunctionNames.standardMsuf32S, standardMsuf32S
            SetupEnhancerFunctionNames.sossMsuf32S, sossMsuf32S
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
