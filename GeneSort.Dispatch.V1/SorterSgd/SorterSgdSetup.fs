namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Dispatch.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SorterSgd.Standard
open GeneSort.Dispatch.V1.SorterSgd.Soss

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
        | Some ExecuteFunctionNames.standard -> setupExecutor SgdEx_Standard.setupEvolutionRunStandard
        | Some ExecuteFunctionNames.soss -> setupExecutor SgdEx_Soss.setupEvolutionRunSoss
        | name -> invalidArg (nameof host) (sprintf "No evolution setup is configured for execute function %A." name)
