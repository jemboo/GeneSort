namespace GeneSort.Dispatch.V1.SorterSgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Soss
open GeneSort.Dispatch.V1.SorterSgd.Standard

module SorterSgdSetup =

    let private setupExecutor prepare =
        { new IRunParamsExecutor with
            member _.Execute host rp _ cts progress =
                match host with
                | SgdRunHost sgdHost ->
                    prepare
                        (sgdHost.MakeSortableTests FileEvolutionSetupSources.sources)
                        (sgdHost.MakeSeedPoolSet FileEvolutionSetupSources.sources)
                        host.RunDb sgdHost.GenSaveIntervals sgdHost.GenSaveSubIntervals rp cts progress
                | SimpleRunHost _ ->
                    async { return Error "SGD setup requires an SgdRun with generation save interval names." } }

    let getSetupExecutor (host: runHost) =
        match host.Run.ExecuteFunctionName with
        | (ExecuteFunctionNames.soss | ExecuteFunctionNames.standard)
            when host.Run.SetupFunctionName = SetupCatalogNames.evoAppend || host.Run.SetupFunctionName = SetupCatalogNames.sossAppend ->
            { new IRunParamsExecutor with
                member _.Execute host rp allowOverwrite cts progress = asyncResult {
                    let! project = rp.GetSourceProjectName() |> Result.ofOption "Missing SourceProjectName."
                    let! database = rp.GetSourceDbName() |> Result.ofOption "Missing SourceDbName."
                    let root = System.IO.Path.Combine(RunHost.projectsRoot (), %project, %database, "Data") |> UMX.tag<pathToRootFolder>
                    let source = new GeneSortDbMp(root, host.Run.RunQueryCatalogName)
                    return! EvoAppendSetup.prepare source host rp allowOverwrite cts progress
                } }
        | ExecuteFunctionNames.standard -> setupExecutor SgdEx_Standard.setupEvolutionRunStandard
        | ExecuteFunctionNames.soss -> setupExecutor SgdEx_Soss.setupEvolutionRunSoss
        | name -> invalidArg (nameof host) (sprintf "No evolution setup is configured for execute function %A." name)
