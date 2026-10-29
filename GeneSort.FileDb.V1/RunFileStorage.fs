namespace GeneSort.FileDb.V1

open System
open System.IO
open FSharp.UMX
open MessagePack
open GeneSort.Core.Mp
open GeneSort.Project.V1
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1

module internal RunFileStorage =
    let private comparison = if OperatingSystem.IsWindows() then StringComparison.OrdinalIgnoreCase else StringComparison.Ordinal

    let validateIdentity (configured: run) =
        let issues =
            [| RunIdentity.validate "ProjectName" %configured.ProjectName
               RunIdentity.validate "DatabaseName" %configured.DatabaseName
               RunIdentity.validate "RunName" %configured.RunName |] |> Array.concat
        if issues.Length > 0 then invalidArg "configured" (issues |> Array.map (fun issue -> issue.Field + ": " + issue.Message) |> String.concat "\n")

    let dataRoot projectsFolder (configured: run) =
        validateIdentity configured
        if String.IsNullOrWhiteSpace projectsFolder || not (Path.IsPathFullyQualified projectsFolder) then
            invalidArg "projectsFolder" "Choose an absolute projects folder."
        Path.Combine(Path.GetFullPath projectsFolder, %configured.ProjectName, %configured.DatabaseName, "Data") |> UMX.tag<pathToRootFolder>

    /// Check every path before moving or recursively deleting generated directories.
    let private within (root: string<pathToRootFolder>) path =
        let rootPath = Path.GetFullPath(%root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        let full = Path.GetFullPath path
        if not (full.StartsWith(rootPath + string Path.DirectorySeparatorChar, comparison)) then
            invalidArg "path" "The generated file path is outside the selected database."
        let mutable part = full
        while not (String.Equals(part, rootPath, comparison)) do
            if (File.Exists part || Directory.Exists part) && (File.GetAttributes(part) &&& FileAttributes.ReparsePoint) <> enum 0 then
                invalidArg "path" "Generated run files cannot be published through a linked file or directory."
            part <- Path.GetDirectoryName part
        full

    let paths (root: string<pathToRootFolder>) (configured: run) =
        validateIdentity configured
        let database = DirectoryInfo(%root).Parent
        if isNull database || isNull database.Parent || database.Name <> %configured.DatabaseName || database.Parent.Name <> %configured.ProjectName then
            invalidArg "root" "The destination database does not match this run."
        let definition = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Project
                            (queryParams.createForRun configured.DatabaseName configured.ProjectName configured.RunName) |> UMX.untag |> within root
        let parameters = OutputDataFile.getPathToOutputDataFolder root geneSortDataMode.Project ""
                            (geneSortDataType.RunParameters configured.RunName) |> UMX.untag |> within root
        definition, parameters

    let rec private files root folder =
        if not (Directory.Exists folder) then [||]
        else
            Directory.GetFileSystemEntries folder |> Array.collect (fun entry ->
                let path = within root entry
                if Directory.Exists path then files root path else [|path|])
    let private sameDefinition path configured =
        try File.Exists path &&
            (MessagePackSerializer.Deserialize<runDto>(File.ReadAllBytes path) |> RunDto.toDomain |> RunDto.fromDomain) = RunDto.fromDomain configured
        with :? MessagePackSerializationException -> false

    let inspect root configured =
        MessagePackSetup.configure ()
        let definition, parameters = paths root configured
        let count = (files root parameters).Length
        definition, parameters, count, (count > 0 || (File.Exists definition && not (sameDefinition definition configured)))

    let acquire root (configured: run) =
        let path = within root (Path.Combine(%root, "Project", ".run-" + %configured.RunName + ".lock"))
        Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
        try new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose)
        with :? IOException -> raise (IOException("Another operation is already using this run. Try again when that operation finishes."))

    let saveDefinitionAsync root (configured: run) overwrite = task {
        MessagePackSetup.configure ()
        let path, parameterFolder = paths root configured
        let bytes = MessagePackSerializer.Serialize(RunDto.fromDomain configured)
        use guard = acquire root configured
        if (files root parameterFolder).Length > 0 && not (sameDefinition path configured) then
            invalidOp "This run has generated parameters. Use Generate RunParameters to save these changes and regenerate them, or choose a new run name."
        Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
        let temporary = within root (Path.Combine(Path.GetDirectoryName path, ".run-" + Guid.NewGuid().ToString("N") + ".tmp"))
        try
            do! File.WriteAllBytesAsync(temporary, bytes)
            File.Move(temporary, path, overwrite)
            return path
        finally
            if File.Exists temporary then File.Delete temporary
    }

    let publishAsync root (prepared: runParameterSet) mode (progress: IProgress<string> option) = task {
        // A failing progress display must not change the result of an already published batch.
        let report message = progress |> Option.iter (fun sink -> try sink.Report message with _ -> ())
        try
            MessagePackSetup.configure ()
            let configured = prepared.Run
            let definition, parameters = paths root configured
            use guard = acquire root configured
            let existing = files root parameters
            for path in existing do
                let parts = Path.GetRelativePath(parameters, path).Split(Path.DirectorySeparatorChar)
                let valid =
                    if parts.Length <> 2 then false
                    else
                        let replicaOk, replica = Int32.TryParse parts[0]
                        let idOk, _ = Guid.TryParse(Path.GetFileNameWithoutExtension parts[1])
                        replicaOk && replica >= 0 && idOk && String.Equals(Path.GetExtension parts[1], ".msgpack", StringComparison.OrdinalIgnoreCase)
                if not valid then invalidOp (sprintf "The parameter folder contains an unrecognized file: %s. Move it before regenerating." path)
            match mode with
            | runParameterWriteMode.CreateNew ->
                if existing.Length > 0 || (File.Exists definition && not (sameDefinition definition configured)) then
                    invalidOp "Run files already exist. Choose Replace to regenerate this run, or use a new run name."
            | runParameterWriteMode.Resume
            | runParameterWriteMode.Append ->
                if File.Exists definition && not (sameDefinition definition configured) then
                    invalidOp "The saved run definition differs. Resume requires the original configuration; use Replace to regenerate it."
            | runParameterWriteMode.Replace -> ()
            | _ -> invalidArg "mode" "Unknown RunParameter write mode."

            let entries = prepared.Entries |> Array.map (fun (query, value) ->
                let target = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Project query |> UMX.untag |> within root
                target, value)
            if mode = runParameterWriteMode.Resume then
                let expected = Collections.Generic.HashSet<string>(entries |> Array.map fst,
                    if OperatingSystem.IsWindows() then StringComparer.OrdinalIgnoreCase else StringComparer.Ordinal)
                if existing |> Array.exists (expected.Contains >> not) then
                    invalidOp "Existing RunParameter files are outside the requested set. Resume cannot remove them; use Replace to regenerate the complete set."

            let staging = within root (Path.Combine(%root, "Project", ".parameters-" + Guid.NewGuid().ToString("N")))
            let stagedDefinition = within root (Path.Combine(staging, "definition.msgpack"))
            let stagedParameters = within root (Path.Combine(staging, "parameters"))
            let oldDefinition = within root (Path.Combine(staging, "previous-definition.msgpack"))
            let oldParameters = within root (Path.Combine(staging, "previous-parameters"))
            let mutable preserveRecovery = false
            Directory.CreateDirectory stagedParameters |> ignore
            try
                do! File.WriteAllBytesAsync(stagedDefinition, MessagePackSerializer.Serialize(RunDto.fromDomain configured))
                if mode = runParameterWriteMode.Append then
                    for path in existing do
                        let staged = within root (Path.Combine(stagedParameters, Path.GetRelativePath(parameters, path)))
                        Directory.CreateDirectory(Path.GetDirectoryName staged) |> ignore
                        File.Copy(path, staged)
                for index, (target, value) in Array.indexed entries do
                    let staged = within root (Path.Combine(stagedParameters, Path.GetRelativePath(parameters, target)))
                    Directory.CreateDirectory(Path.GetDirectoryName staged) |> ignore
                    if mode = runParameterWriteMode.Append && File.Exists staged then ()
                    elif mode = runParameterWriteMode.Resume && File.Exists target then File.Copy(target, staged)
                    else do! File.WriteAllBytesAsync(staged, MessagePackSerializer.Serialize(RunParametersDto.fromDomain value))
                    if index = 0 || index + 1 = entries.Length || (index + 1) % 100 = 0 then
                        report (sprintf "Prepared %d of %d RunParameter files." (index + 1) entries.Length)

                // Both outputs are staged before changing either destination. Roll back failed publication.
                let mutable backedDefinition, backedParameters, installedDefinition, installedParameters = false, false, false, false
                try
                    Directory.CreateDirectory(Path.GetDirectoryName definition) |> ignore
                    if File.Exists definition then
                        File.Move(within root definition, oldDefinition)
                        backedDefinition <- true
                    if Directory.Exists parameters then
                        Directory.Move(within root parameters, oldParameters)
                        backedParameters <- true
                    File.Move(stagedDefinition, definition)
                    installedDefinition <- true
                    Directory.Move(stagedParameters, parameters)
                    installedParameters <- true
                with failure ->
                    try
                        if installedParameters then Directory.Move(within root parameters, stagedParameters)
                        if installedDefinition then File.Move(within root definition, stagedDefinition)
                        if backedParameters then Directory.Move(within root oldParameters, parameters)
                        if backedDefinition then File.Move(oldDefinition, definition)
                    with recovery ->
                        preserveRecovery <- true
                        raise (IOException(sprintf "Publication failed: %s. Restoration failed: %s. Recovery files are in %s." failure.Message recovery.Message staging))
                    raise failure
                report (sprintf "Saved the run definition and %d RunParameter files." entries.Length)
                return Ok ()
            finally
                if not preserveRecovery && Directory.Exists staging then
                    try Directory.Delete(within root staging, true)
                    with cleanup -> report (sprintf "Temporary files could not be removed from %s: %s" staging cleanup.Message)
        with error -> return Error error.Message
    }
