namespace GeneSort.Dispatch.V1.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1
open GeneSort.Dispatch.V1

module EvoAppendSetup =
    let private copyOnce (db: IGeneSortDb) query data (ct: CancellationToken) =
        async {
            ct.ThrowIfCancellationRequested()
            return! SetupArtifacts.saveOnce db query (fun () -> async { return Ok data })
        }

    let private selectSourceParameters (destination: runParameters) (candidates: runParameters array) =
        match Map.tryFind ParamKeys.sourceExecutionIdKey destination.ParamMap with
        | None -> Error "Missing SourceExecutionId."
        | Some value ->
            match Guid.TryParse value with
            | false, _ -> Error "Invalid SourceExecutionId."
            | true, id ->
                match candidates |> Array.filter (fun candidate ->
                    candidate.GetRepl() = destination.GetRepl() &&
                    candidate.GetId() = Some (UMX.tag id)) with
                | [| candidate |] -> Ok candidate
                | [||] -> Error "The selected source execution has no saved parameters."
                | _ -> Error "Duplicate saved parameters for the selected source execution."

    /// Copy one source checkpoint and its tests, converting families during Setup when needed.
    /// SourceExecutionId selects the immediate saved source execution.
    let prepare (sourceDb: IGeneSortDb) (host: runHost) (rp: runParameters)
                _allowOverwrite (cts: CancellationTokenSource) progress =
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
            do!
                match sourceRun.ExecuteFunctionName, sourceRun.RunQueryCatalogName with
                | ExecuteFunctionNames.soss, (QueryCatalogNamesRun.sorterSgdMsufSoss | QueryCatalogNamesRun.sorterSgdMsufSossAppend)
                | ExecuteFunctionNames.standard, (QueryCatalogNamesRun.sorterSgdMsufStandard | QueryCatalogNamesRun.sorterSgdMsufStandardAppend) -> Ok ()
                | _ -> Error "Append source must use a supported Standard or Soss workflow."
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

            let sourceQuery = QueryParamsCatalogRun.get sourceRun.RunQueryCatalogName sourceProject sourceDatabase
            let! repl = rp.GetRepl() |> Result.ofOption "Missing Repl."
            let! sourceCandidates = sourceDb.getRunParameters sourceRunName (Some repl) (Some (repl + 1<replNumber>)) (Some cts.Token) progress
            let! sourceParameters = selectSourceParameters rp sourceCandidates
            do!
                match rp.GetSorterCountPerPoolSet(), rp.GetSorterCountPerPool(), rp.GetSorterPoolCount() with
                | Some total, Some perPool, Some count
                    when int total > 0 && int perPool > 0 && int count > 0
                         && int total % int perPool = 0 && int total / int perPool = int count -> Ok ()
                | _ -> Error "Append pool capacity must be positive and SorterCountPerPoolSet must equal SorterPoolCount * SorterCountPerPool."
            let atGeneration = sourceParameters.WithGenerationCurrent(Some (UMX.tag generation))
            let! poolQuery = sourceQuery atGeneration (geneSortDataType.SorterPoolSet "") |> Result.ofOption "Cannot build source pool query."
            let! checkpointExists = sourceDb.doesOutPutDataExist geneSortDataMode.Results poolQuery
            do! if checkpointExists then Ok ()
                else Error (sprintf "Source checkpoint not found for %s/%s/%s, replica %d, interval %d (generation %d), file ID %O. Run the source through this interval or select an interval it has saved."
                                %sourceProject %sourceDatabase %sourceRunName %repl interval generation poolQuery.Id)
            let! poolData = sourceDb.loadAsync geneSortDataMode.Results poolQuery
            let! poolGeneration =
                match sourceRun.ExecuteFunctionName with
                | ExecuteFunctionNames.soss -> OutputData.asSorterPoolSetSoss poolData |> Result.map (fun pool -> pool.GenerationNumber)
                | _ -> OutputData.asSorterPoolSetStandard poolData |> Result.map (fun pool -> pool.GenerationNumber)
            let importedGeneration = %poolGeneration
            do! if importedGeneration = generation || importedGeneration = generation + 1 then Ok ()
                else Error (sprintf "Source interval %d resolves to generation %d, but its saved pool contains generation %d." interval generation importedGeneration)
            if importedGeneration <> generation then
                OpsUtils.report progress (sprintf "Source checkpoint is from the previous evolution save convention: interval %d is labeled generation %d but contains generation %d. Preserving pool generation %d." interval generation importedGeneration importedGeneration)
            do! if lastGeneration > importedGeneration then Ok () else Error "Destination final generation must be after the imported pool's actual generation."
            let! testsQuery = sourceQuery (sourceParameters.WithGenerationCurrent(Some (UMX.tag 0))) (geneSortDataType.SortableTestsWithPrefix "")
                              |> Result.ofOption "Cannot build source tests query."
            let testsQuery = QueryParams.forSetup sourceParameters testsQuery
            let! testsData = sourceDb.loadAsync geneSortDataMode.Setup testsQuery
            let! tests, ces = OutputData.asSortableTestsWithPrefix testsData
            let! width = rp.GetSortingWidth() |> Result.ofOption "Missing SortingWidth."
            do! if SortableTests.getSortingWidth tests = width then Ok () else Error "Source test width does not match the destination."
            let converting = sourceRun.ExecuteFunctionName <> host.Run.ExecuteFunctionName
            let destinationPool =
                if converting then
                    queryParams.create host.Run.DatabaseName host.Run.ProjectName None None (geneSortDataType.SorterPoolSet "")
                        [|"SourceFileId", string %poolQuery.Id
                          "SourceTestsFileId", string %testsQuery.Id
                          "ExecutionFunction", host.Run.ExecuteFunctionName|]
                else queryParams.createForImportedFile host.Run.DatabaseName host.Run.ProjectName (Some repl) (geneSortDataType.SorterPoolSet "") poolQuery.Id
            let destinationTests = queryParams.createForImportedFile host.Run.DatabaseName host.Run.ProjectName (Some repl) (geneSortDataType.SortableTestsWithPrefix "") testsQuery.Id
            let updated =
                rp.ParamMap
                |> Map.add ParamKeys.sourcePoolFileIdKey (string %poolQuery.Id)
                |> Map.add ParamKeys.sourceTestsFileIdKey (string %testsQuery.Id)
                |> runParameters.create
                |> SetupFileReferences.attach [ParamKeys.setupSorterPoolSetIdKey, string %destinationPool.Id
                                               ParamKeys.setupSortableTestsWithPrefixIdKey, string %destinationTests.Id]
                |> fun value -> value.WithGenerationCurrent(Some poolGeneration).WithGenerationFirst(Some poolGeneration).WithRunFinished(Some false)
            do! checkCancellation cts.Token
            do! copyOnce host.RunDb destinationTests testsData cts.Token
            let! destinationExists = host.RunDb.doesOutPutDataExist geneSortDataMode.Setup destinationPool
            if not destinationExists then
                let! sourcePool = OutputData.asSorterPoolSet poolData
                let converted = EvoAppendPools.convertForSetup host.Run.ExecuteFunctionName tests
                                    (ceBlock.create (UMX.tag Guid.Empty) width ces) sourcePool
                do! copyOnce host.RunDb destinationPool (outputData.SorterPoolSet converted) cts.Token
            OpsUtils.report progress (sprintf "Imported %s/%s, interval %d (generation %d) into Setup." %sourceProject %sourceDatabase interval importedGeneration)
            return updated
        }
