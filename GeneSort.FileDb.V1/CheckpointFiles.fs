namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Text.Json
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage

module CheckpointFiles =
    /// The observer exposes durable publication boundaries for diagnostics and crash tests.
    let saveAsync root (outputs: (queryParams * outputData) array) (overwrite: bool<allowOverwrite>) (observer: IProgress<string> option) = async {
        try
            if outputs.Length = 0 then invalidArg (nameof outputs) "A checkpoint must contain outputs."
            let paths = outputs |> Array.map (fun (q, _) -> OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results q |> UMX.untag)
            let manifest = paths.[0] + ".commit.json"
            Directory.CreateDirectory(Path.GetDirectoryName manifest) |> ignore
            // Released by the OS even when the writer is forcibly terminated.
            use ownership = new FileStream(manifest + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            let interrupted = File.Exists(paths.[0] + ".checkpoint") && not (CheckpointIntegrity.isReadable paths.[0])
            if not %overwrite && not interrupted && paths |> Array.exists File.Exists then
                return Error "Overwrite not allowed"
            else
                // Invalidate first, before publishing any member, including on retry.
                CheckpointIntegrity.atomicWrite manifest "[]"
                for path in paths do
                    CheckpointIntegrity.atomicWrite (path + ".checkpoint") (Path.GetRelativePath(Path.GetDirectoryName path, manifest))
                observer |> Option.iter (fun p -> p.Report "pending")
                let mutable result = Ok ()
                for index in 0 .. outputs.Length - 1 do
                    if Result.isOk result then
                        let query, data = outputs.[index]
                        let! saved = OutputDataFile.saveToFileAsync root geneSortDataMode.Results query data (UMX.tag true)
                        result <- saved
                        observer |> Option.iter (fun p -> p.Report(sprintf "member-%d" index))
                match result with
                | Error error -> return Error error
                | Ok () ->
                    let entries = paths |> Array.map (fun path -> [| Path.GetRelativePath(Path.GetDirectoryName manifest, path); CheckpointIntegrity.hash path |])
                    observer |> Option.iter (fun p -> p.Report "before-commit")
                    CheckpointIntegrity.atomicWrite manifest (JsonSerializer.Serialize entries)
                    observer |> Option.iter (fun p -> p.Report "committed")
                    return Ok ()
        with error -> return Error ("Checkpoint publication failed: " + error.Message)
    }


    /// Prune only groups explicitly owned by this row; legacy saves remain snapshots.
    let saveRetainedAsync root (owner: Guid<queryParamsId>) snapshot outputs overwrite observer = async {
        try
            if Array.isEmpty outputs then invalidArg (nameof outputs) "A checkpoint must contain outputs."
            let pool = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results (fst outputs.[0]) |> UMX.untag
            let folder = Path.GetDirectoryName pool
            Directory.CreateDirectory folder |> ignore
            use ownership = new FileStream(Path.Combine(folder, string owner + ".retention.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            let metadataPath = pool + ".retention.json"
            let readMetadata path =
                try JsonSerializer.Deserialize<string[]>(File.ReadAllText path) |> Option.ofObj |> Option.defaultValue [||]
                with _ -> [||]
            let previousMetadata = readMetadata metadataPath
            if previousMetadata.Length = 3 && previousMetadata.[0] <> string owner then
                invalidArg (nameof owner) "This checkpoint belongs to another processing row."
            let wasSnapshot = previousMetadata.Length = 3 && previousMetadata.[1] = "snapshot"
            let preserved = wasSnapshot && CheckpointIntegrity.isReadable pool && File.Exists pool
            // Publish the retention intent before commit so a crash cannot demote a snapshot.
            CheckpointIntegrity.atomicWrite metadataPath (JsonSerializer.Serialize [| string owner; (if snapshot || wasSnapshot then "snapshot" else "checkpoint"); string ((fst outputs.[0]).Generation |> Option.defaultWith (fun () -> invalidArg (nameof outputs) "A checkpoint requires a generation.")) |])
            let memberEntries = outputs |> Array.map (fun (query, _) ->
                let path = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results query |> UMX.untag
                [| Path.GetRelativePath(folder, path); "" |])
            CheckpointIntegrity.atomicWrite (pool + ".retention-members.json") (JsonSerializer.Serialize memberEntries)
            let! result = if preserved then async.Return (Ok ()) else saveAsync root outputs overwrite observer
            match result with
            | Error error -> return Error error
            | Ok () ->
                if not (CheckpointIntegrity.isReadable pool) then return Error "New checkpoint failed integrity validation; older checkpoints retained."
                else
                    let rootPath = Path.GetFullPath(UMX.untag root).TrimEnd(Path.DirectorySeparatorChar) + string Path.DirectorySeparatorChar
                    let candidates = Directory.GetFiles(folder, "*.retention.json")
                    let latest =
                        candidates |> Array.choose (fun metadata ->
                            let values = readMetadata metadata
                            if values.Length = 3 && values.[0] = string owner then
                                match Int32.TryParse values.[2] with
                                | true, generation -> Some (generation, metadata.Substring(0, metadata.Length - ".retention.json".Length))
                                | _ -> None
                            else None)
                        |> Array.sortByDescending fst
                        |> Array.find (fun (_, candidatePool) ->
                            candidatePool = pool || (File.Exists candidatePool && CheckpointIntegrity.isReadable candidatePool))
                        |> fst
                    for metadata in candidates do
                        let values = readMetadata metadata
                        if values.Length = 3 && values.[0] = string owner && values.[1] = "checkpoint" && (Int32.TryParse values.[2] |> function true, generation -> generation < latest | _ -> false) then
                            let oldPool = metadata.Substring(0, metadata.Length - ".retention.json".Length)
                            let manifest = oldPool + ".commit.json"
                            // Invalid/pending groups are also safe to retire. Keep the original member
                            // list in a tombstone so interrupted cleanup can finish on a later save.
                            let tombstone = manifest + ".retired"
                            try
                                let saved = if File.Exists tombstone then File.ReadAllText tombstone else File.ReadAllText manifest
                                let contents = if saved.Trim() = "[]" then File.ReadAllText(oldPool + ".retention-members.json") else saved
                                let entries = JsonSerializer.Deserialize<string[][]> contents
                                let paths = entries |> Array.map (fun entry -> Path.GetFullPath(entry.[0], folder))
                                if paths |> Array.forall (fun path -> path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) && path.EndsWith(".msgpack", StringComparison.OrdinalIgnoreCase)) then
                                    CheckpointIntegrity.atomicWrite tombstone contents
                                    CheckpointIntegrity.atomicWrite manifest "[]"
                                    observer |> Option.iter (fun p -> p.Report "retiring")
                                    for path in paths do
                                        File.Delete path
                                        File.Delete(path + ".checkpoint")
                                    File.Delete manifest
                                    File.Delete(oldPool + ".commit.json.lock")
                                    File.Delete metadata
                                    File.Delete(oldPool + ".retention-members.json")
                                    File.Delete tombstone
                            with :? IOException -> () // A busy reader may defer cleanup until the next save.
                    return Ok ()
        with error -> return Error ("Checkpoint retention failed: " + error.Message)
    }
