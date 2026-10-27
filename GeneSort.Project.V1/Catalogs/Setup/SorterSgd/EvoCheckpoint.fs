namespace GeneSort.Project.V1.Catalogs.Setup.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1.Sgd
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage

/// Loaded source data. Importing it does not need to rediscover its run or parameters.
type evoCheckpoint = {
    PoolFile: queryParams
    TestsFile: queryParams
    Pool: sorterPoolSet
    Tests: sortableTests
    Prefix: ce array
    Generation: int<generationNumber>
}

module EvoCheckpoint =
    /// Select exactly one saved execution and load the requested generation.
    /// Only current checkpoint conventions and explicit Setup references are accepted.
    let load (db: IGeneSortDb) (rp: runParameters) (ct: CancellationToken) progress = asyncResult {
        do! checkCancellation ct
        let! project = rp.GetSourceProjectName() |> Result.ofOption "Missing SourceProjectName."
        let! database = rp.GetSourceDbName() |> Result.ofOption "Missing SourceDbName."
        let! runName = rp.GetSourceRunName() |> Result.ofOption "Missing SourceRunName."
        let! interval = rp.GetSourceGenerationInterval() |> Result.ofOption "Missing SourceGenerationInterval."
        let! repl = rp.GetRepl() |> Result.ofOption "Missing Repl."
        let! idText = rp.ParamMap.TryFind ParamKeys.sourceExecutionIdKey |> Result.ofOption "Missing SourceExecutionId."
        let! id = match Guid.TryParse idText with true, id -> Ok (UMX.tag<queryParamsId> id) | _ -> Error "Invalid SourceExecutionId."
        let! runData = db.loadAsync geneSortDataMode.Project (queryParams.createForRun database project runName)
        let! source = OutputData.asRun runData
        do! if source.ProjectName = project && source.DatabaseName = database && source.RunName = runName then Ok ()
            else Error "Source run identity does not match the requested source."
        let! intervals = Runtime.RunIntervals.forRun source |> Result.ofOption "Source must be an SGD run."
        let! generation =
            if interval < 0 then Error "SourceGenerationInterval must be nonnegative."
            else GenIntervalConfig.getSampleAtIndex intervals interval |> Result.ofOption "Source generation interval is unavailable."
        let! candidates = db.getRunParameters runName (Some repl) (Some (repl + 1<replNumber>)) (Some ct) progress
        let! parameters =
            match candidates |> Array.filter (fun (candidate: runParameters) -> candidate.GetId() = Some id && candidate.GetRepl() = Some repl) with
            | [| parameters |] -> Ok parameters
            | [||] -> Error "The selected source execution has no saved parameters."
            | _ -> Error "Duplicate saved parameters for the selected source execution."
        let query = QueryParamsCatalogRun.get source.RunQueryCatalogName project database
        let! poolFile = query (parameters.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "")
                        |> Result.ofOption "Cannot build source checkpoint query."
        let! exists = db.doesOutPutDataExist geneSortDataMode.Results poolFile
        do! if exists then Ok () else Error (sprintf "Source checkpoint not found for %s/%s/%s, interval %d (generation %d), file ID %O."
                                                    %project %database %runName interval generation poolFile.Id)
        let! poolData = db.loadAsync geneSortDataMode.Results poolFile
        let! pool = OutputData.asSorterPoolSet poolData
        let! actualGeneration =
            match source.ExecuteFunctionName, pool with
            | ExecuteFunctionNames.standard, sorterPoolSet.Standard pool -> Ok pool.GenerationNumber
            | ExecuteFunctionNames.soss, sorterPoolSet.Soss pool -> Ok pool.GenerationNumber
            | _ -> Error "Source checkpoint type does not match its execution family."
        do! if int actualGeneration = generation then Ok ()
            else Error "Source checkpoint generation does not match the requested interval. Rerun the source with the current save convention."
        let! testsId = parameters.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey
                       |> Result.ofOption "Source has no Setup tests reference. Rerun source Setup."
        let! testsId = match Guid.TryParse testsId with true, id -> Ok (UMX.tag<queryParamsId> id) | _ -> Error "Invalid source Setup tests reference."
        let testsFile = queryParams.create database project None None (geneSortDataType.SortableTestsWithPrefix "") [||] |> queryParams.withId testsId
        do! checkCancellation ct
        let! testsData = db.loadAsync geneSortDataMode.Setup testsFile
        let! tests, prefix = OutputData.asSortableTestsWithPrefix testsData
        return { PoolFile = poolFile; TestsFile = testsFile; Pool = pool; Tests = tests; Prefix = prefix; Generation = actualGeneration }
    }
