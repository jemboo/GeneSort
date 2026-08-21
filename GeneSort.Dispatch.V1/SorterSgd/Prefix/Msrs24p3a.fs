namespace GeneSort.Dispatch.V1.SorterSgd

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter
open GeneSort.Eval.V1.Sgd
open GeneSort.Dispatch.V1

module Msrs24p3a =

    let projName = "SorterSgd.Prfefix.Msrs24p3a" |> UMX.tag<projectName>

    let standardParams (rp:runParameters) =
        let spp = rp.GetSorterCountPerPool().Value |> UMX.untag
        let pc = rp.GetSorterPoolCount().Value |> UMX.untag
        let sorterEvalSelectionType = sorterEvalSelectionType.GuidOrder ((spp * pc) |> UMX.tag<sorterCount>)
        let stf = SorterLibId.create (24<sortingWidth>) sorterVariant.Prefix3a

        rp.WithRngType(Some rngType.Lcg)
          .WithCollectNewSortableTests(false |> UMX.tag<collectNewSortableTests> |> Some)
          .WithExcludeSelfCe(true |> UMX.tag<excludeSelfCe> |> Some)
          .WithSorterChildCount(Some 1<sorterChildCount>)
          .WithSimpleSorterModelType(Some simpleSorterModelType.Msrs)
          .WithOrthoRate(Some 4.001<orthoRate>)
          .WithParaRate(Some 0.4<paraRate>)
          .WithSelfSymRate(Some 2.001<selfSymRate>)
          .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
          .WithDistinctSorterHashes(Some true)
          .WithPrioritizeNewMutants(Some true)
          .WithSortedFraction(Some 0.99<sortedFraction>)
          .WithSorterEvalMeasureInitial(Some SorterEvalMeasure.stageBiased)
          .WithSorterEvalMeasure(Some SorterEvalMeasure.stageBiased)
          .WithSeedPoolSorterEvalSelectionType(Some sorterEvalSelectionType)
          .WithSortableTestFilter(Some stf)
          .WithSortingWidth(Some stf.sortingWidth)


    module PoolSzComp =

        let dbNameTest = "PoolSzTest" |> UMX.tag<databaseName>
        let dbNamePoolSz32 = "PoolSz32" |> UMX.tag<databaseName>
        let dbNamePoolSz128 = "PoolSz128" |> UMX.tag<databaseName>
        let dbFolderTest = @$"c:\Projects\{%projName}\{%dbNameTest}\Data" |> UMX.tag<pathToRootFolder>
        let dbFolderPoolSz32 = @$"c:\Projects\{%projName}\{%dbNamePoolSz32}\Data" |> UMX.tag<pathToRootFolder>
        let dbFolderPoolSz128 = @$"c:\Projects\{%projName}\{%dbNamePoolSz128}\Data" |> UMX.tag<pathToRootFolder>

        let makeQueryParams
                (repl: int<replNumber>)
                (genCurrent: int<generationNumber>)
                (sorterCtPerPool: int<sorterCountPerPool>)
                (sorterPoolCt: int<sorterPoolCount>)
                (ses:sorterEvalSelectionType)
                (sper: int<sorterPoolExpansionRate>)
                (mmod: int<mutationMod>)
                (spm: sorterPoolMeasure)
                (spsi: samplingConfig)
                (outDt: outputDataType) : queryParams =

            match outDt with
            | outputDataType.RunParameters _ ->
                queryParams.create 
                    dbNameTest projName
                    (Some repl)
                    outDt
                    [|
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.seedPoolSorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                        (runParameters.sorterPoolExpansionRateKey, (Some %sper) |> string)
                        (runParameters.mutationModKey, (Some %mmod) |> string)
                        (runParameters.sorterPoolSelectionIntervalsKey, spsi |> SamplingConfig.toString)
                        (runParameters.sorterPoolMeasureKey, spm |> SorterPoolMeasure.toCompactString)
                    |]
            | _ ->
                 queryParams.create 
                    dbNameTest projName
                    (Some repl)
                    outDt
                    [| 
                        (runParameters.generationCurrentKey, (Some genCurrent) |> GenerationNumber.toString)
                        (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                        (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                        (runParameters.seedPoolSorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                        (runParameters.sorterPoolExpansionRateKey, (Some %sper) |> string)
                        (runParameters.mutationModKey, (Some %mmod) |> string)
                        (runParameters.sorterPoolSelectionIntervalsKey, spsi |> SamplingConfig.toString)
                        (runParameters.sorterPoolMeasureKey, spm |> SorterPoolMeasure.toCompactString)
                    |]


        let queryParamsFromRunParams 
                        (rp: runParameters) 
                        (odt: outputDataType) : queryParams option =
            maybe {
                let! repl = rp.GetRepl()
                let! curGen = rp.GetGenerationCurrent()
                let! scPP = rp.GetSorterCountPerPool()
                let! spc = rp.GetSorterPoolCount()
                let! spsev = rp.GetSeedPoolSorterEvalSelectionType()
                let! sper = rp.GetSorterPoolExpansionRate()
                let! mmod = rp.GetMutationMod()
                let! spsi = rp.GetSorterPoolSelectionIntervals()
                let! spm = rp.GetSorterPoolMeasure()
                return makeQueryParams repl curGen scPP spc spsev 
                                       sper mmod spm spsi odt
            }


        let private withLocalParams (rp:runParameters) =
            let rpn = standardParams rp
            rpn.WithSeedModificationRate(Some 0.02<seedModificationRate>)
               .WithModificationRate(Some 0.06<modificationRate>)


        let private paramMapFilter (rp: runParameters) =
            Some rp


        let private finishRunParams (host: IRunHost) (rp:runParameters) =
            let newRp = withLocalParams rp
            let qp = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.Run host.Run.RunName)
            newRp.WithRunFinished(Some false)
                 .WithId(Some qp.Value.Id)
                 .WithRunName(Some host.Run.RunName)


        let saveIntervals = SampleRegistry.samplingConfigsDict["uniformInterval500"]
        let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.K"]

        let dbTest = new GeneSortGenDbMp(dbFolderTest, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
        let dbPoolSz32 = new GeneSortGenDbMp(dbFolderPoolSz32, queryParamsFromRunParams, saveIntervals, saveSubIntervals)
        let dbPoolSz128 = new GeneSortGenDbMp(dbFolderPoolSz128, queryParamsFromRunParams, saveIntervals, saveSubIntervals)

        let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
            [ 
                (dbNameTest, dbTest :> IGeneSortDb);
                (dbNamePoolSz32, dbPoolSz32 :> IGeneSortDb);
                (dbNamePoolSz128, dbPoolSz128 :> IGeneSortDb);
            ]
            |> Map.ofList

        let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
            match databaseConfigs.TryFind name with
            | Some db -> db
            | None -> failwithf "Database with name %s not found" (UMX.untag name)


        let createRunHost (spec: runHostSpec) : IRunHost =
            let db = getDatabaseByName spec.databaseName
            let run = run.create spec.databaseName projName spec.runName spec.runDescription
            runHost.Create db spec run :> IRunHost


        module Specs =

            let TestSpec (executorType: sorterSgdExecutorType)  : runHostSpec = {
                databaseName = dbNameTest
                runName = sprintf @"Test_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
                runDescription = "Mutation analysis for 24pfx Msrs"
                spans = [
                    (runParameters.generationCurrentKey, [0] |> List.map string)
                    (runParameters.generationIntervalCountKey, [1] |> List.map string)
                    (runParameters.sorterCountPerPoolKey, ["16";])
                    (runParameters.sorterPoolCountKey, ["16";] )
                    (runParameters.sorterPoolExpansionRateKey, ["2";] )
                    (runParameters.mutationModKey, [3;] |> List.map string)
                    (runParameters.sorterPoolSelectionIntervalsKey, [ SampleRegistry.samplingConfigsDict["emptyInterval"] ] |> List.map SamplingConfig.toString)
                    (runParameters.sorterPoolMeasureKey, [ SorterPoolMeasure.noStdev; SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)
                ]
                filter = paramMapFilter
                enhancer = finishRunParams
                allowOverwrite = false |> UMX.tag
                maxParallel = 8
            }


            let PoolSz32Spec (executorType: sorterSgdExecutorType)  : runHostSpec = {
                databaseName = dbNamePoolSz32
                runName = sprintf @"PoolSz32_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
                runDescription = "Mutation analysis for 24pfx Msrs"
                spans = [
                    (runParameters.generationCurrentKey, [0] |> List.map string)
                    (runParameters.generationIntervalCountKey, [3] |> List.map string)
                    (runParameters.sorterCountPerPoolKey, ["32";])
                    (runParameters.sorterPoolCountKey, ["128";] )
                    (runParameters.sorterPoolExpansionRateKey, ["2";] )
                    (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
                    (runParameters.sorterPoolSelectionIntervalsKey, [ SampleRegistry.samplingConfigsDict["uniformInterval5_L5"] ] |> List.map SamplingConfig.toString)
                    (runParameters.sorterPoolMeasureKey, [ SorterPoolMeasure.noStdev; SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)
                ]
                filter = paramMapFilter
                enhancer = finishRunParams
                allowOverwrite = false |> UMX.tag
                maxParallel = 16
            }


            let PoolSz128Spec (executorType: sorterSgdExecutorType)  : runHostSpec = {
                databaseName = dbNamePoolSz128
                runName = sprintf @"PoolSz128_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
                runDescription = "Mutation analysis for 24pfx Msrs"
                spans = [
                    (runParameters.generationCurrentKey, [0] |> List.map string)
                    (runParameters.generationIntervalCountKey, [3] |> List.map string)
                    (runParameters.sorterCountPerPoolKey, ["128";])
                    (runParameters.sorterPoolCountKey, ["32";] )
                    (runParameters.sorterPoolExpansionRateKey, ["2";] )
                    (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
                    (runParameters.sorterPoolSelectionIntervalsKey, [ SampleRegistry.samplingConfigsDict["uniformInterval5_L5"] ] |> List.map SamplingConfig.toString)
                    (runParameters.sorterPoolMeasureKey, [ SorterPoolMeasure.noStdev; SorterPoolMeasure.stdev;] |> List.map SorterPoolMeasure.toCompactString)
                ]
                filter = paramMapFilter
                enhancer = finishRunParams
                allowOverwrite = false |> UMX.tag
                maxParallel = 16
            }



