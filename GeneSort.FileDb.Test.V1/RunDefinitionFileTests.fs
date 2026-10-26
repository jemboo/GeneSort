namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Editing
open GeneSort.Project.Mp.V1
open GeneSort.FileDb.V1

module RunDefinitionFileTests =
    let private configured description =
        let args = RunEditor.newDraft().ToArgs()
        match runDraft.create { args with Description = description; AllowOverwrite = true } |> RunEditor.build with
        | Ok configured -> configured
        | Error issues -> failwithf "%A" issues

    let private withFolder test = task {
        let root = Path.Combine(Path.GetTempPath(), "GeneSort-RunEditor-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory root |> ignore
        try do! test root
        finally Directory.Delete(root, true)
    }

    [<Fact>]
    let ``save and open preserve a run and create only its definition`` () = withFolder (fun root -> task {
        let original = configured "Round trip"
        let! path = RunDefinitionFiles.saveAsync root original false
        let! opened = RunDefinitionFiles.loadAsync path
        Assert.True(RunDto.fromDomain original = RunDto.fromDomain opened)
        Assert.Equal(Some root, RunDefinitionFiles.tryProjectsFolder path opened)
        Assert.Equal(Path.Combine(root, string original.ProjectName, string original.DatabaseName, "Data", "Project", "Run", string original.RunName, "Run_" + string original.RunName + ".msgpack"), path)
        Assert.Equal<string>([|path|], Directory.GetFiles(root, "*", SearchOption.AllDirectories))
    })

    [<Fact>]
    let ``definition overwrite needs its own permission even when workflow overwrite is enabled`` () = withFolder (fun root -> task {
        let! path = RunDefinitionFiles.saveAsync root (configured "Original") false
        let before = File.ReadAllBytes path
        let! _ = Assert.ThrowsAsync<IOException>(fun () -> RunDefinitionFiles.saveAsync root (configured "Replacement") false :> Threading.Tasks.Task)
        Assert.Equal<byte>(before, File.ReadAllBytes path)
        Assert.Single(Directory.GetFiles(root, "*", SearchOption.AllDirectories)) |> ignore
        let! _ = RunDefinitionFiles.saveAsync root (configured "Replacement") true
        let! opened = RunDefinitionFiles.loadAsync path
        Assert.Equal("Replacement", opened.Description)
    })

    [<Fact>]
    let ``concurrent non-overwriting saves publish exactly one complete definition`` () = withFolder (fun root -> task {
        let attempt description = task {
            try
                let! _ = RunDefinitionFiles.saveAsync root (configured description) false
                return true
            with :? IOException -> return false
        }
        let! results = Threading.Tasks.Task.WhenAll(attempt "First", attempt "Second")
        Assert.Equal(1, results |> Array.filter id |> Array.length)
        let files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        let path = Assert.Single files
        let! opened = RunDefinitionFiles.loadAsync path
        Assert.Contains(opened.Description, [|"First"; "Second"|])
    })

    [<Fact>]
    let ``an exported definition does not infer an unrelated projects folder`` () = withFolder (fun root -> task {
        let original = configured "Exported"
        let! path = RunDefinitionFiles.saveAsync root original false
        let exported = Path.Combine(root, "copy.msgpack")
        File.Copy(path, exported)
        Assert.Equal(None, RunDefinitionFiles.tryProjectsFolder exported original)
    })

    [<Fact>]
    let ``unsafe identities and relative roots cannot write files`` () = withFolder (fun root -> task {
        let original = configured "Invalid"
        let unsafeRun = { RunDto.fromDomain original with RunName = "../outside" } |> RunDto.toDomain
        Assert.Equal(None, RunDefinitionFiles.tryProjectsFolder (RunDefinitionFiles.pathFor root original) unsafeRun)
        let! _ = Assert.ThrowsAsync<ArgumentException>(fun () -> RunDefinitionFiles.saveAsync root unsafeRun false :> Threading.Tasks.Task)
        Assert.Throws<ArgumentException>(fun () -> RunDefinitionFiles.pathFor "." original |> ignore) |> ignore
        Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories))
    })
