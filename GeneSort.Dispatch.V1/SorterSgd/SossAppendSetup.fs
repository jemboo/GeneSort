namespace GeneSort.Dispatch.V1.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Eval.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1

module SossAppendSetup =
    /// Copy one source checkpoint and its tests, without regenerating or evaluating either.
    /// Source parameter combinations and replicas must match the destination's inherited spans.
    let prepare (sourceDb: IGeneSortDb) (host: runHost) (rp: runParameters)
                allowOverwrite (cts: CancellationTokenSource) progress =
        asyncResult {
            do! checkCancellation cts.Token
            let! sourceProject = rp.GetSourceProjectName() |> Result.ofOption "Missing SourceProjectName."
            let! sourceDatabase = rp.GetSourceDbName() |> Result.ofOption "Missing SourceDbName."
            let! sourceRunName = rp.GetSourceRunName() |> Result.ofOption "Missing SourceRunName."
            let! interval = rp.GetSourceGenerationInterval() |> Result.ofOption "Missing SourceGenerationInterval."
            do! if interval >= 0 then Ok () else Error "SourceGenerationInterval must be nonnegative."
            let! sourceData = sourceDb.loadAsync geneSortDataMode.Project (queryParams.createForRun sourceDatabase sourceProject sourceRunName)
            let! sourceRun = OutputData.asRun sourceData
            do! if sourceRun.ProjectName = sourceProject && sourceRun.DatabaseName = sourceDatabase && sourceRun.RunName = sourceRunName then Ok ()
                else Error "Source run identity does not match the requested source."
            do! if sourceRun.ExecuteFunctionName = ExecuteFunctionNames.soss && sourceRun.QueryCatalogName = QueryCatalogNames.sorterSgdMsufSoss then Ok ()
                else Error "Append setup currently requires a Soss_32 source run."
            let! intervalName = sourceRun.GenSaveIntervalsName |> Result.ofOption "Source run has no generation interval configuration."
            let! intervals =
                match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue intervalName with
                | true, value -> Ok value
                | _ -> Error (sprintf "Unknown source generation intervals '%s'." intervalName)
            let! generation = GenIntervalConfig.getSampleAtIndex intervals interval
                              |> Result.ofOption (sprintf "Source generation interval %d is unavailable." interval)
            let! lastInterval = rp.GetGenerationIntervalLast() |> Result.ofOption "Missing GenerationIntervalLast."
            let! destinationIntervals = host.GenSaveIntervals |> Result.ofOption "Append setup requires an SGD host."
            let! lastGeneration = GenIntervalConfig.getSampleAtIndex destinationIntervals %lastInterval
                                  |> Result.ofOption "Destination GenerationIntervalLast is unavailable."
            do! if lastGeneration > generation then Ok () else Error "Destination final generation must be after the imported generation."

            let sourceQuery = QueryParamsCatalog.get sourceRun.QueryCatalogName sourceProject sourceDatabase
            let atGeneration = rp.WithGenerationCurrent(Some (UMX.tag generation))
            let! poolQuery = sourceQuery atGeneration (geneSortDataType.SorterPoolSet "") |> Result.ofOption "Cannot build source pool query."
            let! poolData = sourceDb.loadAsync geneSortDataMode.Results poolQuery
            let! pool = OutputData.asSorterPoolSetSoss poolData
            let importedGeneration = %pool.GenerationNumber
            do! if importedGeneration = generation || importedGeneration = generation + 1 then Ok ()
                else Error (sprintf "Source interval %d resolves to generation %d, but its saved pool contains generation %d." interval generation importedGeneration)
            if importedGeneration <> generation then
                OpsUtils.report progress (sprintf "Source checkpoint is from the previous Soss save convention: interval %d is labeled generation %d but contains generation %d. Preserving pool generation %d." interval generation importedGeneration importedGeneration)
            do! if lastGeneration > importedGeneration then Ok () else Error "Destination final generation must be after the imported pool's actual generation."
            let! testsQuery = sourceQuery (rp.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "")
                              |> Result.ofOption "Cannot build source tests query."
            let! testsData = sourceDb.loadAsync geneSortDataMode.Setup testsQuery
            let! tests, _ = OutputData.asSortableTestsWithPrefix testsData
            let! width = rp.GetSortingWidth() |> Result.ofOption "Missing SortingWidth."
            do! if SortableTests.getSortingWidth tests = width then Ok () else Error "Source test width does not match the destination."
            let updated = rp.WithGenerationCurrent(Some pool.GenerationNumber).WithGenerationFirst(Some pool.GenerationNumber).WithRunFinished(Some false)
            let! destinationPool = host.QueryParamsFromRunParams updated (geneSortDataType.SorterPoolSet "")
                                  |> Result.ofOption "Cannot build destination pool query."
            let! destinationTests = host.QueryParamsFromRunParams (updated.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "")
                                   |> Result.ofOption "Cannot build destination tests query."
            do! checkCancellation cts.Token
            do! host.RunDb.saveAsync geneSortDataMode.Setup destinationTests testsData allowOverwrite
            do! host.RunDb.saveAsync geneSortDataMode.Setup destinationPool poolData allowOverwrite
            OpsUtils.report progress (sprintf "Imported %s/%s, interval %d (generation %d) into Setup." %sourceProject %sourceDatabase interval importedGeneration)
            return updated
        }
