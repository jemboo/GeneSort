namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Editing
open GeneSort.Project.Mp.V1
open GeneSort.FileDb.V1
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open MessagePack

module RunDefinitionFileTests =
    [<Theory>]
    [<InlineData("SortableTests.Prefix.Channels32", "Channels32", "SortableTests/Prefix/Channels32")>]
    [<InlineData("SortableTests.Prefix.Channels32", "Custom", "SortableTests/Prefix/Channels32/Custom")>]
    [<InlineData("SortableTests", "Prefix", "SortableTests/Prefix")>]
    let ``database folders expand project hierarchy without duplicating its final module`` project database expected =
        Assert.Equal((expected: string).Replace('/', Path.DirectorySeparatorChar), DatabasePaths.relativeFolder project database)

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
    let ``opening a legacy direct definition retains old redundant parameter files unchanged`` () = withFolder (fun root -> task {
        let configured =
            DirectRun.create {
                ProjectName = UMX.tag "SortableTests"; DatabaseName = UMX.tag "Prefix"; RunName = UMX.tag "Legacy"
                Description = "Legacy duplicate setup sweep"; AllowOverwrite = false
                Execution = {
                    FunctionName = ExecuteFunctionNames.sortableTests
                    QueryCatalogName = QueryCatalogNamesRun.sortableTestPrefix
                    ParametersPipelineName = RunParametersPipelineNames.sortableTests
                    ParameterSpans = [
                        ParamKeys.sortableDataFormatKey, ["BitVector512"]
                        ParamKeys.prefixLibIdKey, ["PrefixLibId(channelCount=24, stageLength=3, Variant=PrefixA)";
                                                  "PrefixLibId(channelCount=24, stageLength=3, Variant=PrefixB)"]
                    ]
                }
            } |> run.DirectRun
        let rows = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
        // This cross-pair was created by the old redundant Cartesian expansion.
        let query = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        let oldRow = rows[0].WithSetupParametersId(rows[1].GetSetupParametersId())
        let oldId = query oldRow (geneSortDataType.Run configured.RunName) |> Option.get
        let oldRow = oldRow.WithId(Some oldId.Id)
        let dataRoot = Path.Combine(root, "SortableTests", "Prefix", "Data") |> UMX.tag<pathToRootFolder>
        let db = new GeneSortDbMp(dataRoot, configured.RunQueryCatalogName) :> IGeneSortDb
        let address = query oldRow (geneSortDataType.RunParameters configured.RunName) |> Option.get
        let! saved = db.saveAsync geneSortDataMode.Project address (outputData.RunParameters oldRow) (UMX.tag false) |> Async.StartAsTask
        saved |> Result.defaultWith failwith
        let dto = RunDto.fromDomain configured
        let legacy = { dto with SetupParameterSpans = dto.RunParameterSpans }
        MessagePackSetup.configure ()
        let path = RunDefinitionFiles.pathFor root configured
        Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
        File.WriteAllBytes(path, MessagePackSerializer.Serialize legacy)
        let before = Directory.GetFiles(root, "*.msgpack", SearchOption.AllDirectories) |> Array.map (fun file -> file, File.ReadAllBytes file)
        let! opened = RunDefinitionFiles.loadAsync path
        Assert.True(opened.IsDirectRun)
        Assert.Empty(opened.SetupParameterSpans)
        let! loaded = RunParameterFiles.loadAllAsync root opened
        Assert.Equal<runParameters>([|oldRow|], loaded)
        for file, bytes in before do Assert.Equal<byte>(bytes, File.ReadAllBytes file)
    })

    [<Fact>]
    let ``save and open preserve a run and create only its definition`` () = withFolder (fun root -> task {
        let original = configured "Round trip"
        let! path = RunDefinitionFiles.saveAsync root original false
        let! opened = RunDefinitionFiles.loadAsync path
        Assert.True(RunDto.fromDomain original = RunDto.fromDomain opened)
        Assert.Equal(Some root, RunDefinitionFiles.tryProjectsFolder path opened)
        Assert.Equal(Path.Combine(DatabasePaths.dataRoot root (string original.ProjectName) (string original.DatabaseName), "Project", "Run", string original.RunName, "Run_" + string original.RunName + ".msgpack"), path)
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
