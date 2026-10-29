namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Bins.Soss
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.FileDb.V1

module CheckpointFileTests =
    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``file database resumes sparse high generations without changing existing checkpoint names`` soss =
        MessagePackSetup.configure()
        let folder = Path.Combine(Path.GetTempPath(), "GeneSort-checkpoint-" + Guid.NewGuid().ToString("N"))
        let get = Result.defaultWith failwith
        try
            let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors)
            let root = Path.Combine(folder, string configured.ProjectName, string configured.DatabaseName, "Data") |> UMX.tag
            let db = GeneSortDbMp(root, configured.RunQueryCatalogName) :> IGeneSortDb
            let rp =
                (RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>).[0]
                    .WithGenerationIntervalLast(Some 1000<generationIntervalLast>)
                    .WithGenerationFirst(Some 0<generationNumber>)
                    .WithGenerationCurrent(Some 0<generationNumber>)
            let config = { Name = "long"; Min = 0; MaxCount = None; Scale = 1000.0; Method = Constant 1 }
            let outputType = geneSortDataType.SorterPoolSet ""
            let query (gen: int) = db.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag gen))) outputType |> Option.get
            let load () =
                if soss then
                    Utils.loadHighestGenSorterPoolSetSoss config db rp |> Async.RunSynchronously
                    |> Result.map (Option.map (fun pool -> int pool.GenerationNumber))
                else
                    Utils.loadHighestGenSorterPoolSet config db rp |> Async.RunSynchronously
                    |> Result.map (Option.map (fun pool -> int pool.GenerationNumber))
            // A new run has no Results directory yet.
            Assert.Equal(Ok None, load())
            let save mode (gen: int) =
                let standard = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag gen) (Dim1 1) None
                let pool = if soss then SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss else sorterPoolSet.Standard standard
                db.saveAsync mode (query gen) (outputData.SorterPoolSet pool) (UMX.tag false) |> Async.RunSynchronously |> get
            save geneSortDataMode.Setup 0
            save geneSortDataMode.Results 350000
            save geneSortDataMode.Results 900000
            save geneSortDataMode.Results 1500000 // Outside this run's endpoint.
            let expectedFile = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results (query 900000) |> UMX.untag
            Assert.Equal((query 900000).Id.ToString() + ".msgpack", Path.GetFileName expectedFile)
            let before = Directory.GetFiles(folder, "*", SearchOption.AllDirectories) |> Array.sort
            Assert.Equal(Ok (Some 900000), load())
            Assert.Equal<string[]>(before, Directory.GetFiles(folder, "*", SearchOption.AllDirectories) |> Array.sort)
            // The listing is refreshed for each resume; no persistent index or stale cache.
            File.Delete expectedFile
            Assert.Equal(Ok (Some 350000), load())
        finally
            if Directory.Exists folder then Directory.Delete(folder, true)


    let private crashFixture folder soss =
        MessagePackSetup.configure()
        let configured = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (sprintf "%A" >> failwith)
        let root = Path.Combine(folder, string configured.ProjectName, string configured.DatabaseName, "Data") |> UMX.tag
        let db = GeneSortDbMp(root, configured.RunQueryCatalogName) :> IGeneSortDb
        let rp = (RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>).[0]
                    .WithGenerationIntervalLast(Some 10<generationIntervalLast>)
                    .WithGenerationFirst(Some 0<generationNumber>)
        let outputs (gen: int) =
            let query kind = db.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some (UMX.tag gen))) kind |> Option.get
            let pool = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag gen) (Dim1 1) None
            let pool = if soss then SorterPoolSet_Soss.fromStandard pool |> sorterPoolSet.Soss else sorterPoolSet.Standard pool
            let summaryQuery = query (geneSortDataType.SorterPoolSetSummarySet "")
            let summaries =
                if soss then spSummarySet_Soss.create (UMX.cast summaryQuery.Id) (UMX.tag gen) [||] |> spSummarySet.Soss
                else spSummarySet_Standard.create (UMX.cast summaryQuery.Id) (UMX.tag gen) [||] |> spSummarySet.Standard
            let bins =
                if soss then sorterPoolBinsSetSeries_Soss.create (UMX.tag Guid.Empty) [] |> sorterPoolBinsSetSeries.Soss
                else sorterPoolBinsSetSeries_Standard.create (UMX.tag Guid.Empty) [] |> sorterPoolBinsSetSeries.Standard
            [| query (geneSortDataType.SorterPoolSet ""), outputData.SorterPoolSet pool
               summaryQuery, outputData.SorterPoolSetSummarySet summaries
               query (geneSortDataType.SorterPoolBinsSetSeries ""), outputData.SorterPoolBinsSetSeries bins |]
        let load () =
            let config = { Name = "test"; Min = 0; MaxCount = None; Scale = 1.0; Method = Constant 1 }
            Utils.loadOutputDataWithHighestGenerationNumber OutputData.asSorterPoolSet (geneSortDataType.SorterPoolSet "") config db rp
            |> Async.RunSynchronously |> Result.defaultWith failwith
            |> Option.map (function sorterPoolSet.Standard p -> int p.GenerationNumber | sorterPoolSet.Soss p -> int p.GenerationNumber)
        root, outputs, load

    [<Fact>]
    let ``checkpoint crash worker`` () =
        let folder = Environment.GetEnvironmentVariable "SORTOPIA_CRASH_FOLDER"
        if not (String.IsNullOrEmpty folder) then
            let root, outputs, _ = crashFixture folder (Environment.GetEnvironmentVariable "SORTOPIA_CRASH_SOSS" = "True")
            let observer = { new IProgress<string> with
                                member _.Report stage =
                                    if stage = Environment.GetEnvironmentVariable "SORTOPIA_CRASH_STAGE" then
                                        File.WriteAllText(Path.Combine(folder, "ready"), stage)
                                        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite) }
            let owner = Environment.GetEnvironmentVariable "SORTOPIA_RETENTION_OWNER"
            let work =
                if String.IsNullOrEmpty owner then CheckpointFiles.saveAsync root (outputs 2) (UMX.tag false) (Some observer)
                else CheckpointFiles.saveRetainedAsync root (UMX.tag (Guid.Parse owner)) false (outputs 2) (UMX.tag false) (Some observer)
            work |> Async.RunSynchronously |> Result.defaultWith failwith

    [<Theory>]
    [<InlineData(false, "pending")>]
    [<InlineData(false, "member-0")>]
    [<InlineData(false, "before-commit")>]
    [<InlineData(false, "committed")>]
    [<InlineData(true, "member-1")>]
    [<InlineData(true, "before-commit")>]
    [<InlineData(true, "committed")>]
    let ``abrupt process death recovers only committed generations and permits replay`` soss stage =
        let folder = Path.Combine(Path.GetTempPath(), "Sortopia-crash-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory folder |> ignore
        use child = new System.Diagnostics.Process()
        try
            let root, outputs, _ = crashFixture folder soss
            CheckpointFiles.saveAsync root (outputs 1) (UMX.tag false) None |> Async.RunSynchronously |> Result.defaultWith failwith
            let start = System.Diagnostics.ProcessStartInfo(Environment.ProcessPath)
            start.ArgumentList.Add "-method"
            start.ArgumentList.Add "*checkpoint crash worker*"
            start.ArgumentList.Add "-noLogo"
            start.UseShellExecute <- false
            start.CreateNoWindow <- true
            start.Environment.["SORTOPIA_CRASH_FOLDER"] <- folder
            start.Environment.["SORTOPIA_CRASH_STAGE"] <- stage
            start.Environment.["SORTOPIA_CRASH_SOSS"] <- string soss
            child.StartInfo <- start
            Assert.True(child.Start())
            let ready = Path.Combine(folder, "ready")
            let clock = System.Diagnostics.Stopwatch.StartNew()
            while not (File.Exists ready) && not child.HasExited && clock.Elapsed.TotalSeconds < 30.0 do
                System.Threading.Thread.Sleep 20
            Assert.True(File.Exists ready, "Child did not reach crash boundary: " + stage)
            child.Kill(true)
            Assert.True(child.WaitForExit(10000))
            let _, _, recovered = crashFixture folder soss // Fresh database, no cached state.
            Assert.Equal(Some (if stage = "committed" then 2 else 1), recovered())
            if stage <> "committed" then
                CheckpointFiles.saveAsync root (outputs 2) (UMX.tag false) None |> Async.RunSynchronously |> Result.defaultWith failwith
            Assert.Equal(Some 2, recovered())
            // Damage a companion: the intact pool alone must not be resumed.
            let companion = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results (fst (outputs 2).[2]) |> UMX.untag
            File.WriteAllText(companion, "truncated")
            Assert.Equal(Some 1, recovered())
        finally
            try
                if child.Id > 0 && not child.HasExited then
                    child.Kill(true)
                    child.WaitForExit(10000) |> ignore
            with :? InvalidOperationException -> ()
            Directory.Delete(folder, true)


    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``retention keeps latest checkpoint and every snapshot without crossing row ownership`` soss =
        let folder = Path.Combine(Path.GetTempPath(), "Sortopia-retention-" + Guid.NewGuid().ToString("N"))
        try
            let root, outputs, recovered = crashFixture folder soss
            let owner = UMX.tag<queryParamsId> (Guid.NewGuid())
            let another = UMX.tag<queryParamsId> (Guid.NewGuid())
            let save owner gen snapshot =
                CheckpointFiles.saveRetainedAsync root owner snapshot (outputs gen) (UMX.tag false) None
                |> Async.RunSynchronously |> Result.defaultWith failwith
            let exists gen =
                outputs gen |> Array.forall (fun (q, _) ->
                    let path = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results q |> UMX.untag
                    File.Exists path && CheckpointIntegrity.isReadable path)
            save owner 1 true
            let wrongOwner = CheckpointFiles.saveRetainedAsync root another false (outputs 1) (UMX.tag true) None |> Async.RunSynchronously
            Assert.True(Result.isError wrongOwner)
            save another 2 false
            save owner 3 false
            save owner 4 false
            Assert.False(exists 3)
            Assert.True(exists 1)
            Assert.True(exists 2)
            Assert.True(exists 4)
            save owner 5 true
            Assert.False(exists 4)
            save owner 6 false
            Assert.True(exists 1)
            Assert.True(exists 5)
            Assert.True(exists 6)
            // Re-executing an earlier snapshot cannot delete a newer recovery point.
            save owner 5 true
            Assert.True(exists 6)
            Assert.Equal(Some 6, recovered())
            // A failed new save never removes the latest valid checkpoint.
            let observer = { new IProgress<string> with member _.Report stage = if stage = "before-commit" then failwith "Injected save failure" }
            let failed = CheckpointFiles.saveRetainedAsync root owner false (outputs 7) (UMX.tag false) (Some observer) |> Async.RunSynchronously
            Assert.True(Result.isError failed)
            Assert.True(exists 6)
            Assert.Equal(Some 6, recovered())
            save owner 7 false
            Assert.False(exists 6)
            Assert.True(exists 7)
            Assert.True(exists 1)
            Assert.True(exists 2)
            Assert.True(exists 5)
        finally
            if Directory.Exists folder then Directory.Delete(folder, true)

    [<Fact>]
    let ``checkpoint schedule round trips independently of snapshot schedule`` () =
        MessagePackSetup.configure()
        let draft = RunEditor.newDraft()
        let configured = RunEditor.build draft |> Result.defaultWith (sprintf "%A" >> failwith)
        let bytes = GeneSort.Project.Mp.V1.RunDto.fromDomain configured |> MessagePack.MessagePackSerializer.Serialize
        let restored = MessagePack.MessagePackSerializer.Deserialize<GeneSort.Project.Mp.V1.runDto>(bytes) |> GeneSort.Project.Mp.V1.RunDto.toDomain
        Assert.Equal(configured.CheckpointIntervalsName, restored.CheckpointIntervalsName)
        Assert.Equal(configured.GenSaveIntervalsName, restored.GenSaveIntervalsName)
        let reopened = RunEditor.fromRun restored |> Result.defaultWith failwith
        Assert.Equal(draft.CheckpointIntervalsName, reopened.CheckpointIntervalsName)
        let legacy = { GeneSort.Project.Mp.V1.RunDto.fromDomain configured with CheckpointIntervalsName = null } |> GeneSort.Project.Mp.V1.RunDto.toDomain
        Assert.True(legacy.CheckpointIntervalsName.IsNone)

    [<Theory>]
    [<InlineData("before-commit")>]
    [<InlineData("committed")>]
    [<InlineData("retiring")>]
    let ``process death during replacement or pruning retains a recoverable checkpoint`` stage =
        let folder = Path.Combine(Path.GetTempPath(), "Sortopia-prune-crash-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory folder |> ignore
        use child = new System.Diagnostics.Process()
        try
            let root, outputs, recovered = crashFixture folder false
            let owner = UMX.tag<queryParamsId> (Guid.NewGuid())
            CheckpointFiles.saveRetainedAsync root owner false (outputs 1) (UMX.tag false) None |> Async.RunSynchronously |> Result.defaultWith failwith
            let start = System.Diagnostics.ProcessStartInfo(Environment.ProcessPath)
            start.ArgumentList.Add "-method"
            start.ArgumentList.Add "*checkpoint crash worker*"
            start.ArgumentList.Add "-noLogo"
            start.UseShellExecute <- false
            start.CreateNoWindow <- true
            start.Environment.["SORTOPIA_CRASH_FOLDER"] <- folder
            start.Environment.["SORTOPIA_CRASH_STAGE"] <- stage
            start.Environment.["SORTOPIA_RETENTION_OWNER"] <- string owner
            child.StartInfo <- start
            Assert.True(child.Start())
            let ready = Path.Combine(folder, "ready")
            let clock = System.Diagnostics.Stopwatch.StartNew()
            while not (File.Exists ready) && not child.HasExited && clock.Elapsed.TotalSeconds < 30.0 do
                System.Threading.Thread.Sleep 20
            Assert.True(File.Exists ready, "Child did not reach crash boundary: " + stage)
            child.Kill(true)
            Assert.True(child.WaitForExit(10000))
            Assert.Equal(Some (if stage = "before-commit" then 1 else 2), recovered())
            CheckpointFiles.saveRetainedAsync root owner false (outputs 3) (UMX.tag false) None |> Async.RunSynchronously |> Result.defaultWith failwith
            Assert.Equal(Some 3, recovered())
            let path gen = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results (fst (outputs gen).[0]) |> UMX.untag
            Assert.False(File.Exists(path 1))
            Assert.False(File.Exists(path 2))
        finally
            try
                if child.Id > 0 && not child.HasExited then
                    child.Kill(true)
                    child.WaitForExit(10000) |> ignore
            with :? InvalidOperationException -> ()
            Directory.Delete(folder, true)
