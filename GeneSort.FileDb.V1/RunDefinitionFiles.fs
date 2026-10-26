namespace GeneSort.FileDb.V1

open System
open System.IO
open FSharp.UMX
open MessagePack
open GeneSort.Core.Mp
open GeneSort.Project.V1
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.Mp.V1

/// Read/write run definitions using the same DTO and database layout as dispatch.
/// Saving a definition never generates parameters or starts setup/execution.
module RunDefinitionFiles =
    let private checkIdentity (configured: run) =
        let issues =
            [| RunIdentity.validate "ProjectName" %configured.ProjectName
               RunIdentity.validate "DatabaseName" %configured.DatabaseName
               RunIdentity.validate "RunName" %configured.RunName |]
            |> Array.concat
        if issues.Length > 0 then
            invalidArg "configured" (issues |> Array.map (fun issue -> issue.Field + ": " + issue.Message) |> String.concat "\n")

    let pathFor (projectsFolder: string) (configured: run) =
        checkIdentity configured
        if String.IsNullOrWhiteSpace projectsFolder || not (Path.IsPathFullyQualified projectsFolder) then
            invalidArg "projectsFolder" "Choose an absolute projects folder."
        let root = Path.Combine(Path.GetFullPath projectsFolder, %configured.ProjectName, %configured.DatabaseName, "Data")
        OutputDataFile.getFullOutputDataFilePath (UMX.tag root) geneSortDataMode.Project
            (queryParams.createForRun configured.DatabaseName configured.ProjectName configured.RunName) |> UMX.untag

    let loadAsync (path: string) = task {
        MessagePackSetup.configure ()
        use stream = File.OpenRead path
        let! dto = MessagePackSerializer.DeserializeAsync<runDto>(stream).AsTask()
        return RunDto.toDomain dto
    }

    /// Infer the projects folder only when the opened file follows the canonical layout.
    let tryProjectsFolder (path: string) (configured: run) =
        let file = FileInfo(Path.GetFullPath path)
        let mutable parent = file.Directory
        for _ in 1 .. 6 do
            if not (isNull parent) then parent <- parent.Parent
        if isNull parent then None
        else
            try
                let expected = pathFor parent.FullName configured
                if String.Equals(expected, file.FullName, StringComparison.OrdinalIgnoreCase) then Some parent.FullName else None
            with :? ArgumentException -> None

    let saveAsync (projectsFolder: string) (configured: run) (overwrite: bool) = task {
        let path = pathFor projectsFolder configured
        MessagePackSetup.configure ()
        // Serialize before touching the destination, then publish a complete file atomically.
        let bytes = MessagePackSerializer.Serialize(RunDto.fromDomain configured)
        let folder = Path.GetDirectoryName path
        Directory.CreateDirectory folder |> ignore
        let temporary = Path.Combine(folder, ".run-" + Guid.NewGuid().ToString("N") + ".tmp")
        try
            do! File.WriteAllBytesAsync(temporary, bytes)
            File.Move(temporary, path, overwrite)
            return path
        finally
            if File.Exists temporary then File.Delete temporary
    }
