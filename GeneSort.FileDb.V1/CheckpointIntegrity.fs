namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Security.Cryptography
open System.Text.Json

/// Sidecars use relative paths so a prepared database remains relocatable.
module CheckpointIntegrity =
    let atomicWrite (path: string) (contents: string) =
        Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
        let temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp"
        try
            use stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
            let bytes = System.Text.Encoding.UTF8.GetBytes contents
            stream.Write(bytes)
            stream.Flush(true)
            stream.Dispose()
            File.Move(temporary, path, true)
        finally
            if File.Exists temporary then File.Delete temporary

    let hash (path: string) =
        use stream = File.OpenRead path
        SHA256.HashData stream |> Convert.ToHexString

    let isReadable (path: string) =
        let link = path + ".checkpoint"
        if not (File.Exists link) then true // Legacy files remain supported.
        else
            try
                let manifest = Path.GetFullPath(File.ReadAllText link, Path.GetDirectoryName link)
                let entries = JsonSerializer.Deserialize<string[][]>(File.ReadAllText manifest)
                entries.Length > 0 &&
                entries |> Array.forall (fun entry ->
                    entry.Length = 2 &&
                    let memberPath = Path.GetFullPath(entry.[0], Path.GetDirectoryName manifest)
                    File.Exists memberPath && hash memberPath = entry.[1])
            with _ -> false
