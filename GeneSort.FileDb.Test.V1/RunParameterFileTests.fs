namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open System.Threading.Tasks
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1
open GeneSort.FileDb.V1

module RunParameterFileTests =
    let private configured mods =
        let args = RunEditor.newDraft().ToArgs()
        let spans = args.SetupParameters |> Array.map (fun row ->
            if row.Key = ParamKeys.mutationModKey then parameterDraft.create row.Key mods else row)
        runDraft.create { args with SetupParameters = spans }
        |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors)

    let private request configured first count = { Run = configured; FirstReplica = first; ReplicaCount = count }
    let private rootFor root (configured: run) = Path.Combine(root, string configured.ProjectName, string configured.DatabaseName, "Data") |> UMX.tag<pathToRootFolder>
    let private database root configured = new GeneSortDbMp(rootFor root configured, configured.RunQueryCatalogName) :> IGeneSortDb
    let private snapshot root =
        Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        |> Array.map (fun path -> Path.GetRelativePath(root, path), Convert.ToBase64String(File.ReadAllBytes path)) |> Map.ofArray
    let private withFolder test = task {
        let root = Path.Combine(Path.GetTempPath(), "GeneSort-ParameterFiles-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory root |> ignore
        try do! test root
        finally Directory.Delete(root, true)
    }
    let private publish root configured first count replace = task {
        let! prepared = RunParameterFiles.prepareAsync root (request configured first count)
        do! RunParameterFiles.generateAsync prepared replace null
        return prepared
    }

    [<Fact>]
    let ``preview is read only and generation saves the exact product and definition`` () = withFolder (fun root -> task {
        let original = configured [|"0"; "1"; "2"; "3"|]
        let dto = RunDto.fromDomain original
        let configured = { dto with RunParameterSpans = [| { Key = ParamKeys.modificationRateKey; Values = [|"0.05"; "0.1"|] } |] } |> RunDto.toDomain
        let args = request configured 2 3
        let! prepared = RunParameterFiles.prepareAsync root args
        Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        Assert.Equal(24, prepared.FileCount)
        Assert.Equal(bigint prepared.FileCount, RunPreparation.count args)
        Assert.False(prepared.RequiresReplacement)
        do! RunParameterFiles.generateAsync prepared false null
        let! opened = RunDefinitionFiles.loadAsync prepared.DefinitionPath
        Assert.Equal(RunDto.fromDomain configured, RunDto.fromDomain opened)
        let! loaded = (database root configured).getRunParameters configured.RunName (Some 2<replNumber>) (Some 5<replNumber>) None None |> Async.StartAsTask
        let rows = loaded |> Result.defaultWith failwith
        Assert.Equal(24, rows.Length)
        Assert.Equal<int>([|2; 3; 4|], rows |> Array.map (fun row -> int (row.GetRepl().Value)) |> Array.distinct |> Array.sort)
        let expected = RunPreparation.prepare args
        let files = Directory.GetFiles(prepared.ParameterFolder, "*.msgpack", SearchOption.AllDirectories) |> Array.sort
        let expectedFiles = expected.Entries |> Array.map (fun (query, _) -> OutputDataFile.getFullOutputDataFilePath (rootFor root configured) geneSortDataMode.Project query |> UMX.untag) |> Array.sort
        Assert.Equal<string>(expectedFiles, files)
        Assert.Equal(25, (snapshot root).Count)
        Assert.False(Directory.Exists(Path.Combine(string (rootFor root configured), "Setup")))
    })

    [<Fact>]
    let ``replacement removes obsolete combinations and replicas only for the selected run`` () = withFolder (fun root -> task {
        let original = configured [|"0"; "1"; "2"; "3"|]
        let! first = publish root original 0 2 false
        let sibling = { RunDto.fromDomain original with RunName = "Sibling" } |> RunDto.toDomain
        let! other = publish root sibling 0 1 false
        let otherBefore = File.ReadAllBytes(Directory.GetFiles(other.ParameterFolder, "*.msgpack", SearchOption.AllDirectories)[0])
        let updated = configured [|"7"|]
        let! prepared = RunParameterFiles.prepareAsync root (request updated 5 1)
        Assert.True(prepared.RequiresReplacement)
        Assert.Equal(8, prepared.ExistingFileCount)
        let before = snapshot root
        let! _ = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunParameterFiles.generateAsync prepared false null :> Task)
        Assert.Equal<Map<string, string>>(before, snapshot root)
        do! RunParameterFiles.generateAsync prepared true null
        Assert.Single(Directory.GetFiles(first.ParameterFolder, "*.msgpack", SearchOption.AllDirectories)) |> ignore
        Assert.Equal<string>([|"5"|], Directory.GetDirectories(first.ParameterFolder) |> Array.map Path.GetFileName)
        Assert.Equal(4, Directory.GetFiles(other.ParameterFolder, "*.msgpack", SearchOption.AllDirectories).Length)
        Assert.Equal<byte>(otherBefore, File.ReadAllBytes(Directory.GetFiles(other.ParameterFolder, "*.msgpack", SearchOption.AllDirectories)[0]))
        let! opened = RunDefinitionFiles.loadAsync prepared.DefinitionPath
        Assert.Equal(RunDto.fromDomain updated, RunDto.fromDomain opened)
    })

    [<Fact>]
    let ``an identical saved definition can generate but later edits must regenerate`` () = withFolder (fun root -> task {
        let original = configured [|"0"|]
        let! _ = RunDefinitionFiles.saveAsync root original false
        let! prepared = publish root original 0 1 false
        let updated = configured [|"1"|]
        let before = snapshot root
        let! _ = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunDefinitionFiles.saveAsync root updated true :> Task)
        Assert.Equal<Map<string, string>>(before, snapshot root)
        Assert.Single(Directory.GetFiles(prepared.ParameterFolder, "*.msgpack", SearchOption.AllDirectories)) |> ignore
    })

    [<Fact>]
    let ``resume preserves checkpoint bytes while filling missing parameter files`` () = withFolder (fun root -> task {
        let configured = configured [|"0"; "1"; "2"|]
        let! _ = publish root configured 0 1 false
        let prepared = RunPreparation.prepare (request configured 0 1)
        let query, row = prepared.Entries[0]
        let db = database root configured
        let! changed = db.saveAsync geneSortDataMode.Project query (outputData.RunParameters (row.WithMessage(Some "completed checkpoint"))) (UMX.tag true) |> Async.StartAsTask
        Assert.Equal(Ok (), changed)
        let checkpoint = OutputDataFile.getFullOutputDataFilePath (rootFor root configured) geneSortDataMode.Project query |> UMX.untag
        let bytes = File.ReadAllBytes checkpoint
        let missing = OutputDataFile.getFullOutputDataFilePath (rootFor root configured) geneSortDataMode.Project (fst prepared.Entries[1]) |> UMX.untag
        File.Delete missing
        let! resumed = RunPreparation.publishAsync db prepared runParameterWriteMode.Resume None |> Async.StartAsTask
        Assert.Equal(Ok (), resumed)
        Assert.Equal<byte>(bytes, File.ReadAllBytes checkpoint)
        Assert.True(File.Exists missing)
        let before = snapshot root
        let different = RunPreparation.prepare (request (configured |> RunDto.fromDomain |> fun dto -> RunDto.toDomain { dto with Description = "changed" }) 0 1)
        let! rejected = RunPreparation.publishAsync db different runParameterWriteMode.Resume None |> Async.StartAsTask
        Assert.True(Result.isError rejected)
        Assert.Equal<Map<string, string>>(before, snapshot root)
    })

    [<Fact>]
    let ``failed publication restores the original definition and keeps the obstruction`` () = withFolder (fun root -> task {
        let original = configured [|"0"|]
        let! _ = RunDefinitionFiles.saveAsync root original false
        // A file where the parameter directory should go forces a failure after the definition is published.
        let folder = RunParameterFiles.folderFor root original
        File.WriteAllText(folder, "keep this file")
        let before = snapshot root
        let updated = configured [|"1"|]
        let! prepared = RunParameterFiles.prepareAsync root (request updated 0 1)
        let! _ = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunParameterFiles.generateAsync prepared true null :> Task)
        Assert.Equal<Map<string, string>>(before, snapshot root)
    })

    [<Fact>]
    let ``concurrent generation publishes one complete set`` () = withFolder (fun root -> task {
        let original = configured [|"0"; "1"|]
        let! first = RunParameterFiles.prepareAsync root (request original 0 2)
        let! second = RunParameterFiles.prepareAsync root (request original 3 1)
        let attempt prepared = task {
            try
                do! RunParameterFiles.generateAsync prepared false null
                return Some prepared.FileCount
            with :? InvalidOperationException -> return None
        }
        let! results = Task.WhenAll(attempt first, attempt second)
        let count = results |> Array.choose id |> Assert.Single
        Assert.Equal(count, Directory.GetFiles(first.ParameterFolder, "*.msgpack", SearchOption.AllDirectories).Length)
        Assert.Equal(count + 1, (snapshot root).Count)
    })

    [<Theory>]
    [<InlineData(-1, 1)>]
    [<InlineData(0, 0)>]
    [<InlineData(Int32.MaxValue, 1)>]
    let ``invalid replica ranges cannot write files`` first count = withFolder (fun root -> task {
        let! _ = Assert.ThrowsAsync<ArgumentException>(fun () -> RunParameterFiles.prepareAsync root (request (configured [|"0"|]) first count) :> Task)
        Assert.Empty(snapshot root)
    })

    [<Fact>]
    let ``duplicate file identities are rejected before writing`` () = withFolder (fun root -> task {
        let original = configured [|"0"|] |> RunDto.fromDomain
        let spans = original.SetupParameterSpans |> Array.map (fun span ->
            if span.Key = ParamKeys.mutationModKey then { span with Values = [|"0"; "0"|] } else span)
        let duplicate = { original with SetupParameterSpans = spans } |> RunDto.toDomain
        let! _ = Assert.ThrowsAsync<ArgumentException>(fun () -> RunParameterFiles.prepareAsync root (request duplicate 0 1) :> Task)
        Assert.Empty(snapshot root)
    })

    [<Fact>]
    let ``reserved run names cannot overlap another run's parameter folder`` () = withFolder (fun root -> task {
        let original = configured [|"0"|]
        let reserved = { RunDto.fromDomain original with RunName = "RunParameters_NewRun" } |> RunDto.toDomain
        let! _ = Assert.ThrowsAsync<ArgumentException>(fun () -> RunParameterFiles.prepareAsync root (request reserved 0 1) :> Task)
        Assert.Empty(snapshot root)
    })

    [<Fact>]
    let ``replacement refuses unrecognized files in the parameter directory`` () = withFolder (fun root -> task {
        let configured = configured [|"0"|]
        let! prepared = publish root configured 0 1 false
        File.WriteAllText(Path.Combine(prepared.ParameterFolder, "notes.txt"), "keep these notes")
        let before = snapshot root
        let! _ = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunParameterFiles.generateAsync prepared true null :> Task)
        Assert.Equal<Map<string, string>>(before, snapshot root)
    })

    [<Fact>]
    let ``browse loads all sparse replicas and preserves their saved values without writing`` () = withFolder (fun root -> task {
        let configured = configured [|"2"; "10"|]
        let! prepared = publish root configured 2 1 false
        let db = database root configured
        for query, row in (RunPreparation.prepare (request configured 10 1)).Entries do
            let! saved = db.saveAsync geneSortDataMode.Project query (outputData.RunParameters (row.WithMessage(Some "checkpoint"))) (UMX.tag false) |> Async.StartAsTask
            Assert.Equal(Ok (), saved)
        File.WriteAllText(Path.Combine(prepared.ParameterFolder, "notes.txt"), "not a parameter")
        let before = snapshot root
        let! rows = RunParameterFiles.loadAllAsync root configured
        Assert.Equal(4, rows.Length)
        Assert.Equal<int>([|2; 10|], rows |> Array.map (fun row -> int (row.GetRepl().Value)) |> Array.distinct)
        Assert.Equal<string>([|"checkpoint"|], rows |> Array.choose (fun row -> Map.tryFind ParamKeys.messageKey row.ParamMap) |> Array.distinct)
        Assert.Equal<Map<string, string>>(before, snapshot root)
    })

    [<Fact>]
    let ``browse before generation is empty and does not create directories`` () = withFolder (fun root -> task {
        let! rows = RunParameterFiles.loadAllAsync root (configured [|"0"|])
        Assert.Empty rows
        Assert.Empty(Directory.GetFileSystemEntries root)
    })

    [<Theory>]
    [<InlineData("corrupt")>]
    [<InlineData("wrong-run")>]
    [<InlineData("wrong-replica")>]
    let ``browse reports unreadable or misplaced files instead of silently omitting rows`` fault = withFolder (fun root -> task {
        let configured = configured [|"0"; "1"|]
        let! prepared = publish root configured 0 1 false
        let query, row = (RunPreparation.prepare (request configured 0 1)).Entries[0]
        let path = OutputDataFile.getFullOutputDataFilePath (rootFor root configured) geneSortDataMode.Project query |> UMX.untag
        if fault = "corrupt" then File.WriteAllText(path, "invalid messagepack")
        else
            let changed = if fault = "wrong-run" then row.WithRunName(Some (UMX.tag "OtherRun")) else row.WithRepl(Some 5<replNumber>)
            let! saved = (database root configured).saveAsync geneSortDataMode.Project query (outputData.RunParameters changed) (UMX.tag true) |> Async.StartAsTask
            Assert.Equal(Ok (), saved)
        let before = snapshot root
        let! error = Assert.ThrowsAsync<InvalidOperationException>(fun () -> RunParameterFiles.loadAllAsync root configured :> Task)
        Assert.Contains(Path.GetFileName path, error.Message)
        Assert.Equal<Map<string, string>>(before, snapshot root)
    })
