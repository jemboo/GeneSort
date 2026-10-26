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
    let pathFor (projectsFolder: string) (configured: run) =
        RunFileStorage.dataRoot projectsFolder configured |> fun root -> RunFileStorage.paths root configured |> fst

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
        let root = RunFileStorage.dataRoot projectsFolder configured
        return! RunFileStorage.saveDefinitionAsync root configured overwrite
    }
