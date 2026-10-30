namespace GeneSort.FileDb.V1

open System
open System.IO
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage

/// Library locations used by the desktop workflows, relative to the selected projects folder.
/// Test libraries are addressed by their input parameters; evaluation libraries use saved definitions.
module WorkflowSourceFiles =
    let private loadTests projectsFolder database workflow request query = async {
        let root = DatabasePaths.dataRoot projectsFolder "SortableTests" database |> UMX.tag<pathToRootFolder>
        let file = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results query |> UMX.untag
        if not (File.Exists file) then
            return Error (sprintf "Missing source library SortableTests/%s: %s. Expected result: %s. Generate and execute SortableTests.%s with these values in database '%s' under %s (any run name), then rerun Setup." database request file workflow database projectsFolder)
        else
            let! result = OutputDataFile.getOutputDataAsync root geneSortDataMode.Results query None
            return result |> Result.bind OutputData.asSortableTests
                          |> Result.mapError (fun error -> sprintf "Source library SortableTests/%s (%s), file %s: %s" database request file error)
    }

    let private load projectsFolder project database queryCatalog replica matches outputType convert = async {
        let root = DatabasePaths.dataRoot projectsFolder project database |> UMX.tag<pathToRootFolder>
        let definitionsFolder = Path.Combine(%root, "Project", "Run")
        let definitions =
            if not (Directory.Exists definitionsFolder) then [||]
            else
                Directory.GetDirectories definitionsFolder
                |> Array.map (fun directory -> Path.Combine(directory, "Run_" + Path.GetFileName(directory) + ".msgpack"))
                |> Array.filter File.Exists
                |> Array.sort
        if definitions.Length = 0 then
            return Error (sprintf "Missing source library %s/%s. Expected definitions under: %s. Prepare and execute this library in the selected projects folder (%s)." project database definitionsFolder projectsFolder)
        else
            try
                let candidates = ResizeArray<queryParams>()
                for definition in definitions do
                    let! configured = RunDefinitionFiles.loadAsync definition |> Async.AwaitTask
                    if string configured.ProjectName <> project || string configured.DatabaseName <> database then
                        invalidOp (sprintf "The saved library definition has an unexpected identity: %s" definition)
                    if configured.RunQueryCatalogName = queryCatalog then
                        let queryFor = QueryParamsCatalogRun.get queryCatalog configured.ProjectName configured.DatabaseName
                        RunParameterGeneration.generate configured replica (replica + 1<replNumber>)
                        |> Array.filter matches
                        |> Array.iter (fun row ->
                            queryFor row outputType |> Option.defaultWith (fun () -> invalidOp "Cannot build the library query.")
                            |> candidates.Add)
                let queries =
                    candidates.ToArray()
                    |> Array.distinctBy (fun query -> query.Id)
                if queries.Length <> 1 then
                    invalidOp (sprintf "Expected one matching library input across the database's run definitions, found %d. Choose a database containing one matching evaluation." queries.Length)
                let! result = OutputDataFile.getOutputDataAsync root geneSortDataMode.Results queries[0] None
                return result |> Result.bind convert |> Result.mapError (fun error -> sprintf "Source library %s/%s: %s" project database error)
            with error -> return Error (sprintf "Cannot load source library %s/%s: %s" project database (error.GetBaseException().Message))
    }

    let create (projectsFolder: string) =
        if String.IsNullOrWhiteSpace projectsFolder || not (Path.IsPathFullyQualified projectsFolder) then
            invalidArg "projectsFolder" "Choose an absolute projects folder."
        ProjectCatalogs.registerAll ()
        let openDatabase project database catalog =
            new GeneSortDbMp(DatabasePaths.dataRoot projectsFolder (string project) (string database) |> UMX.tag, catalog, project) :> IGeneSortDb
        let evals database queryCatalog replica matches =
            load projectsFolder "SorterEval" database queryCatalog replica matches (geneSortDataType.SorterSetEval "") OutputData.asSorterSetEval
        let resolveSource key (parameters: setupParameters) =
            parameters.ParamMap.TryFind key
            |> Result.ofOption (sprintf "Setup requires %s." key)
            |> Result.bind (ParameterValidation.validateValue key)
            |> Result.bind RunModuleCatalog.resolve
        let moduleTests (parameters: setupParameters) (row: runParameters) = asyncResult {
            let! source = resolveSource ParamKeys.sortableTestsModuleKey parameters
            let db = openDatabase source.ProjectName source.DatabaseName source.ExecutionQueryCatalogName
            let! query = db.makeQueryParamsFromRunParams row (geneSortDataType.SortableTests "")
                         |> Result.ofOption "The SortableTestsModule catalog cannot query tests with these setup parameters."
            let! data = db.loadAsync geneSortDataMode.Results query
            return! OutputData.asSortableTests data
        }
        let moduleEvals (parameters: setupParameters) replica matches = asyncResult {
            let! source = resolveSource ParamKeys.sorterEvalModuleKey parameters
            let matching (row: runParameters) =
                matches row
                && (parameters.GetRngType() |> Option.forall (fun value -> row.GetRngType() = Some value))
                && (parameters.GetSortableDataFormat() |> Option.forall (fun value -> row.GetSortableDataFormat() = Some value))
                && (parameters.GetSorterEvalType() |> Option.forall (fun value -> row.GetSorterEvalType() = Some value))
            return! load projectsFolder (string source.ProjectName) (string source.DatabaseName)
                        source.ExecutionQueryCatalogName replica matching (geneSortDataType.SorterSetEval "") OutputData.asSorterSetEval
        }
        workflowSources.create {
            EvolutionModules = fun parameters -> {
                LoadMergeTests = fun replica library format ->
                    let row = (runParameters.create parameters.ParamMap).WithRepl(Some replica).WithMergeLibId(Some library).WithSortableDataFormat(Some format)
                    moduleTests parameters row
                LoadPrefixTests = fun replica library format ->
                    let row = (runParameters.create parameters.ParamMap).WithRepl(Some replica).WithPrefixLibId(Some library).WithSortableDataFormat(Some format)
                    moduleTests parameters row
                LoadStandardSorterEvals = fun width model evalType -> asyncResult {
                    let! replica = parameters.GetRepl() |> Result.ofOption "Setup requires Repl."
                    return! moduleEvals parameters replica (fun row ->
                        row.GetChannelCount() = Some width && row.GetSimpleSorterModelType() = Some model
                        && row.GetSorterEvalType() = Some evalType)
                }
                LoadMergeSorterEvals = fun library replica model ->
                    moduleEvals parameters replica (fun row -> row.GetMergeLibId() = Some library && row.GetSimpleSorterModelType() = Some model)
                LoadPrefixSorterEvals = fun library replica model ->
                    moduleEvals parameters replica (fun row -> row.GetPrefixLibId() = Some library && row.GetSimpleSorterModelType() = Some model)
            }
            OpenDatabase = openDatabase
            Evolution = fun testsDatabase evalsDatabase -> {
                LoadMergeTests = fun replica library format ->
                    let row = (runParameters.create Map.empty).WithRepl(Some replica).WithMergeLibId(Some library).WithSortableDataFormat(Some format)
                    let query = QueryParamsBuildersRun.SortableTests.Merge.makeSortableTestsQueryParams (UMX.tag "SortableTests") testsDatabase row "" |> Option.get
                    let request = sprintf "MergeLibId=%s, SortableDataFormat=%s, Repl=%d" (MergeLibId.toString library) (SortableDataFormat.toString format) (int replica)
                    loadTests projectsFolder %testsDatabase "Merge" request query
                LoadPrefixTests = fun replica library format ->
                    let row = (runParameters.create Map.empty).WithRepl(Some replica).WithPrefixLibId(Some library).WithSortableDataFormat(Some format)
                    let query = QueryParamsBuildersRun.SortableTests.Prefix.makeSortableTestsQueryParams (UMX.tag "SortableTests") testsDatabase row "" |> Option.get
                    let request = sprintf "PrefixLibId=%s, SortableDataFormat=%s, Repl=%d" (PrefixLibId.toString library) (SortableDataFormat.toString format) (int replica)
                    loadTests projectsFolder %testsDatabase "Prefix" request query
                LoadStandardSorterEvals = fun width model evalType ->
                    evals %evalsDatabase QueryCatalogNamesRun.sorterEvalStandard 0<replNumber> (fun row ->
                        row.GetChannelCount() = Some width && row.GetSimpleSorterModelType() = Some model
                        && row.GetRngType() = Some rngType.Lcg && row.GetSorterEvalType() = Some evalType)
                LoadMergeSorterEvals = fun library replica model ->
                    evals %evalsDatabase QueryCatalogNamesRun.sorterEvalMerge replica (fun row ->
                        row.GetMergeLibId() = Some library && row.GetSimpleSorterModelType() = Some model
                        && row.GetRngType() = Some rngType.Lcg && row.GetSorterEvalType() = Some sorterEvalType.V2
                        && row.GetSortableDataFormat() = Some sortableDataFormat.Int8Vector512)
                LoadPrefixSorterEvals = fun library replica model ->
                    evals %evalsDatabase QueryCatalogNamesRun.sorterEvalPrefix replica (fun row ->
                        row.GetPrefixLibId() = Some library && row.GetSimpleSorterModelType() = Some model
                        && row.GetRngType() = Some rngType.Lcg && row.GetSorterEvalType() = Some sorterEvalType.V2
                        && row.GetSortableDataFormat() = Some sortableDataFormat.BitVector512)
            }
        }
