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
