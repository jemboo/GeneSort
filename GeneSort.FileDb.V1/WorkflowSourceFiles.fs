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
    let private loadTests projectsFolder database request query = async {
        let root = Path.Combine(projectsFolder, "SortableTests", database, "Data") |> UMX.tag<pathToRootFolder>
        let file = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results query |> UMX.untag
        if not (File.Exists file) then
            return Error (sprintf "Missing source library SortableTests/%s: %s. Expected result: %s. Generate and execute SortableTests.%s with these values in %s (any run name), then rerun Setup." database request file database projectsFolder)
        else
            let! result = OutputDataFile.getOutputDataAsync root geneSortDataMode.Results query None
            return result |> Result.bind OutputData.asSortableTests
                          |> Result.mapError (fun error -> sprintf "Source library SortableTests/%s (%s), file %s: %s" database request file error)
    }

    let private load projectsFolder project database (name: string) replica matches outputType convert = async {
        let root = Path.Combine(projectsFolder, project, database, "Data") |> UMX.tag<pathToRootFolder>
        let definition =
            queryParams.createForRun (UMX.tag database) (UMX.tag project) (UMX.tag name)
            |> OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Project |> UMX.untag
        if not (File.Exists definition) then
            return Error (sprintf "Missing source library %s/%s/%s. Expected definition: %s. Prepare and execute this library in the selected projects folder (%s), or copy its Project and Results folders there. This prerequisite is separate from the run being edited." project database name definition projectsFolder)
        else
            try
                let! configured = RunDefinitionFiles.loadAsync definition |> Async.AwaitTask
                if string configured.ProjectName <> project || string configured.DatabaseName <> database || string configured.RunName <> name then
                    invalidOp "The saved library definition has an unexpected identity."
                let queryFor = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
                let queries =
                    RunParameterGeneration.generate configured replica (replica + 1<replNumber>)
                    |> Array.filter matches
                    |> Array.map (fun row -> queryFor row outputType |> Option.defaultWith (fun () -> invalidOp "Cannot build the library query."))
                    |> Array.distinctBy (fun query -> query.Id)
                if queries.Length <> 1 then
                    invalidOp (sprintf "Expected one matching library input, found %d. Check the library configuration." queries.Length)
                let db = new GeneSortDbMp(root, configured.RunQueryCatalogName) :> IGeneSortDb
                let! result = db.loadAsync geneSortDataMode.Results queries[0]
                return result |> Result.bind convert |> Result.mapError (fun error -> sprintf "Source library %s/%s/%s: %s" project database name error)
            with error -> return Error (sprintf "Cannot load source library %s/%s/%s: %s" project database name (error.GetBaseException().Message))
    }

    let create (projectsFolder: string) =
        if String.IsNullOrWhiteSpace projectsFolder || not (Path.IsPathFullyQualified projectsFolder) then
            invalidArg "projectsFolder" "Choose an absolute projects folder."
        ProjectCatalogs.registerAll ()
        let openDatabase project database catalog =
            new GeneSortDbMp(Path.Combine(projectsFolder, string project, string database, "Data") |> UMX.tag, catalog) :> IGeneSortDb
        let evals database name replica matches =
            load projectsFolder "SorterEval" database name replica matches (geneSortDataType.SorterSetEval "") OutputData.asSorterSetEval
        workflowSources.create {
            OpenDatabase = openDatabase
            Evolution = {
                LoadMergeTests = fun replica library format ->
                    let row = (runParameters.create Map.empty).WithRepl(Some replica).WithMergeLibId(Some library).WithSortableDataFormat(Some format)
                    let query = QueryParamsBuildersRun.SortableTests.Merge.makeSortableTestsQueryParams (UMX.tag "SortableTests") (UMX.tag "Merge") row "" |> Option.get
                    let request = sprintf "MergeLibId=%s, SortableDataFormat=%s, Repl=%d" (MergeLibId.toString library) (SortableDataFormat.toString format) (int replica)
                    loadTests projectsFolder "Merge" request query
                LoadPrefixTests = fun replica library format ->
                    let row = (runParameters.create Map.empty).WithRepl(Some replica).WithPrefixLibId(Some library).WithSortableDataFormat(Some format)
                    let query = QueryParamsBuildersRun.SortableTests.Prefix.makeSortableTestsQueryParams (UMX.tag "SortableTests") (UMX.tag "Prefix") row "" |> Option.get
                    let request = sprintf "PrefixLibId=%s, SortableDataFormat=%s, Repl=%d" (PrefixLibId.toString library) (SortableDataFormat.toString format) (int replica)
                    loadTests projectsFolder "Prefix" request query
                LoadStandardSorterEvals = fun width model evalType ->
                    evals "Standard" "Test" 0<replNumber> (fun row ->
                        row.GetSortingWidth() = Some width && row.GetSimpleSorterModelType() = Some model
                        && row.GetRngType() = Some rngType.Lcg && row.GetSorterEvalType() = Some evalType)
                LoadMergeSorterEvals = fun library replica model ->
                    evals "Merge" "Test" replica (fun row ->
                        row.GetMergeLibId() = Some library && row.GetSimpleSorterModelType() = Some model
                        && row.GetRngType() = Some rngType.Lcg && row.GetSorterEvalType() = Some sorterEvalType.V2
                        && row.GetSortableDataFormat() = Some sortableDataFormat.Int8Vector512)
                LoadPrefixSorterEvals = fun library replica model ->
                    evals "Prefix" "Width32" replica (fun row ->
                        row.GetPrefixLibId() = Some library && row.GetSimpleSorterModelType() = Some model
                        && row.GetRngType() = Some rngType.Lcg && row.GetSorterEvalType() = Some sorterEvalType.V2
                        && row.GetSortableDataFormat() = Some sortableDataFormat.BitVector512)
            }
        }
