namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Threading.Tasks
open FSharp.UMX
open GeneSort.Core.Mp
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup.SorterSgd

type preparedRunFiles =
    private {
        root: string<pathToRootFolder>
        prepared: runParameterSet
        definitionPath: string
        parameterFolder: string
        existingCount: int
        requiresReplacement: bool
    }
    member this.Run = this.prepared.Run
    member this.FileCount = this.prepared.FileCount
    member this.DefinitionPath = this.definitionPath
    member this.ParameterFolder = this.parameterFolder
    member this.ExistingFileCount = this.existingCount
    member this.RequiresReplacement = this.requiresReplacement
    member internal this.Root = this.root
    member internal this.Prepared = this.prepared
    static member internal create root (prepared: runParameterSet) =
        let definition, parameters, count, replacement = RunFileStorage.inspect root prepared.Run
        { root = root; prepared = prepared; definitionPath = definition; parameterFolder = parameters
          existingCount = count; requiresReplacement = replacement }

/// Task-based file adapter for the UI. Generation and publication use the same Project service as Dispatch.
module RunParameterFiles =
    let folderFor projectsFolder configured =
        let root = RunFileStorage.dataRoot projectsFolder configured
        RunFileStorage.paths root configured |> snd

    /// Load every saved replica, including sparse ranges, without generating or modifying files.
    let loadAllAsync projectsFolder (configured: run) =
        Task.Run<runParameters[]>(Func<Task<runParameters[]>>(fun () -> task {
            MessagePackSetup.configure ()
            let folder = folderFor projectsFolder configured
            let rows = ResizeArray<runParameters>()
            if Directory.Exists folder then
                let replicas =
                    Directory.GetDirectories folder
                    |> Array.choose (fun path ->
                        match Int32.TryParse(Path.GetFileName path) with
                        | true, replica when replica >= 0 -> Some (replica, path)
                        | _ -> None)
                    |> Array.sortBy fst
                for replica, path in replicas do
                    for file in Directory.GetFiles(path, "*.msgpack") |> Array.sort do
                        let! result = OutputDataFile._getRPFileAsync file None |> Async.StartAsTask
                        let row = result |> Result.defaultWith invalidOp
                        if row.GetRunName() <> Some configured.RunName || row.GetRepl() <> Some (UMX.tag replica) then
                            invalidOp (sprintf "The RunParameter file does not belong to run '%s', replica %d: %s" %configured.RunName replica file)
                        rows.Add row
            return rows.ToArray()
        }))

    let prepareAsync projectsFolder (args: runPreparationArgs) =
        Task.Run(fun () ->
            let root = RunFileStorage.dataRoot projectsFolder args.Run
            let prepared =
                if args.Run.ParameterSource.IsNone then RunPreparation.prepare args
                else
                    let openSource (source: run) : IGeneSortDb =
                        new GeneSortDbMp(RunFileStorage.dataRoot projectsFolder source, source.RunQueryCatalogName)
                    RunPreparation.prepareWith
                        (RunParameterGeneration.generateWithSourceSelection (EvoCondenseSelection.select openSource) args.Run) args
            preparedRunFiles.create root prepared)

    let generateAsync (prepared: preparedRunFiles) (replace: bool) (progress: IProgress<string>) = task {
        let db = new GeneSortDbMp(prepared.Root, prepared.Run.RunQueryCatalogName) :> IGeneSortDb
        let mode = if replace then runParameterWriteMode.Replace else runParameterWriteMode.CreateNew
        let! result = RunPreparation.publishAsync db prepared.Prepared mode (Option.ofObj progress) |> Async.StartAsTask
        match result with
        | Ok () -> return ()
        | Error error -> return raise (InvalidOperationException error)
    }
