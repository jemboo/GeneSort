namespace GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a.Common
open GeneSort.Dispatch.V1.SorterSgd


module OrthoPara =

    let globalSorterCount = 8192 |> UMX.tag<sorterCount>
    let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>
    let dbFolderOrthoPara32 = @$"c:\Projects\{%projName}\{%dbOrthoPara32Name}\Data" |> UMX.tag<pathToRootFolder>


    let makeQueryParams
            (repl: int<replNumber>)
            (codeMod: string<codeModKey>)
            (genCurrent: int<generationNumber>)
            (sorterCtPerPool: int<sorterCountPerPool>)
            (sorterPoolCt: int<sorterPoolCount>)
            (para: float<paraRate>)
            (selfSym: float<selfSymRate>)
            (mmod: int<mutationMod>)
            (outDt: outputDataType) : queryParams =

        queryParams.create 
            dbOrthoPara32Name 
            projName
            (Some repl)
            (Some genCurrent)
            outDt
            [|
                (runParameters.codeModKey, (Some codeMod) |> CodeModKey.toString)
                (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                (runParameters.paraRateKey, (Some para) |> ParaRate.toString)
                (runParameters.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString)
                (runParameters.mutationModKey, (Some %mmod) |> MutationMod.toString)
            |]


    let queryParamsFromRunParams
                    (rp: runParameters)
                    (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! codeMod = rp.GetCodeModKey()
            let! curGen = rp.GetGenerationCurrent()
            let! scPP = rp.GetSorterCountPerPool()
            let! spc = rp.GetSorterPoolCount()
            let! mmod = rp.GetMutationMod()
            let! para = rp.GetParaRate()
            let! self = rp.GetSelfSymRate()
            return makeQueryParams repl codeMod curGen scPP spc para self mmod odt
        }


    let private withLocalParams (rp:runParameters) =
        let rpn = standardPoolSzParams rp
        rpn.WithOrthoRate(Some 4.001<orthoRate>)


    let private paramMapFilter (rp: runParameters) =
        Some rp

    let private finishRunParams (host: IRunHost) (rp:runParameters) =
        let rp2 = withLocalParams rp
        let scpp = rp.GetSorterCountPerPool().Value
        let selScpp = scpp
        let spc = (%globalSorterCount / %scpp) |> UMX.tag<sorterPoolCount> |> Option.Some
        let rp3 = rp2.WithSorterPoolCount(spc)
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp3 (outputDataType.Run host.Run.RunName)

        rp3.WithRunFinished(Some false)
                .WithId(Some qp.Value.Id)
                .WithRunName(Some host.Run.RunName)
                .WithModificationRate(Some 0.99<modificationRate>)
                .WithSelectedSorterCountPerPool(Some selScpp)


    let saveIntervals = SampleRegistry.samplingConfigsDict["expInterval100_L50ss"]
    let saveSubIntervals = SampleRegistry.samplingConfigsDict["summaryInterval_C.1p5C"]

    let dbOrthoPara = new GeneSortGenDbMp(dbFolderOrthoPara32, queryParamsFromRunParams, saveIntervals, saveSubIntervals)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (dbOrthoPara32Name, dbOrthoPara :> IGeneSortDb);
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

        let PickMode2_2 (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbOrthoPara32Name
            runName = sprintf @"PickMode2_2_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for 24pfx3a Msrs, PickMode2_2"
            spans = [
                (runParameters.codeModKey, ["PickMode2_2"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [5] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }

        let NoMods (executorType: sorterSgdExecutorType)  : runHostSpec = {
            databaseName = dbOrthoPara32Name
            runName = sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag
            runDescription = "OrthroPara rate comp for 24pfx3a Msrs, NoMods"
            spans = [
                (runParameters.codeModKey, ["NoMods"] |> List.map string)
                (runParameters.generationCurrentKey, [0] |> List.map string)
                (runParameters.generationIntervalCountKey, [7] |> List.map string)
                (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
                (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
                (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
                (runParameters.mutationModKey, [0] |> List.map string)
                (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
            ]
            filter = paramMapFilter
            enhancer = finishRunParams
            allowOverwrite = false |> UMX.tag
            maxParallel = 8
        }