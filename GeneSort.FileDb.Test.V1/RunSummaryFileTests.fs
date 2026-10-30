namespace GeneSort.FileDb.Test.V1

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Editing
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.Mp.V1
open GeneSort.FileDb.V1

module RunSummaryFileTests =
    let private withReports test = task {
        let folder = Path.Combine(Path.GetTempPath(), "GeneSort-SummaryViewer-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory folder |> ignore
        try
            let dto = RunEditor.newDraft() |> RunEditor.build |> Result.defaultWith (fun errors -> failwithf "%A" errors) |> RunDto.fromDomain
            let configured =
                { dto with
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval2
                    SetupParameterSpans = dto.SetupParameterSpans |> Array.map (fun span ->
                        if span.Key = ParamKeys.generationIntervalLastKey then {span with Values = [|"1"|]} else span) }
                |> RunDto.toDomain
            let! prepared = RunParameterFiles.prepareAsync folder {Run = configured; FirstReplica = 0; ReplicaCount = 2}
            do! RunParameterFiles.generateAsync prepared false null
            let! rows = RunParameterFiles.loadAllAsync folder configured
            let rows = rows |> Array.sortBy (fun row -> row.GetRepl(), row.GetMutationMod())
            let root = DatabasePaths.dataRoot folder (string configured.ProjectName) (string configured.DatabaseName) |> UMX.tag
            let queryFor = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
            let write (row: runParameters) (generation: int) (text: string) =
                let query = queryFor (row.WithGenerationCurrent(Some (UMX.tag<generationNumber> generation))) (geneSortDataType.TextReport (UMX.tag "SummaryReport")) |> Option.get
                let path = OutputDataFile.getFullOutputDataFilePath root geneSortDataMode.Results query |> string
                Directory.CreateDirectory(Path.GetDirectoryName path) |> ignore
                File.WriteAllText(path, text)
                path
            do! test folder configured rows write
        finally Directory.Delete(folder, true)
    }

    [<Fact>]
    let ``viewer loads only latest matching reports within the selected horizons without writes`` () = withReports (fun folder configured rows write -> task {
        let contents value = sprintf "RunName\tMetric\tOptional\n%s\t%s\t\n" (string configured.RunName) value
        let intervals = RunIntervals.forRun configured |> Option.get
        let gens = [|0; 1; 2|] |> Array.map (GenIntervalConfig.getSampleAtIndex intervals >> Option.get)
        write rows[0] gens[0] (contents "old") |> ignore
        let latest = write rows[0] gens[1] (contents "latest")
        write rows[0] gens[2] (contents "outside horizon") |> ignore
        write rows[1] gens[1] (contents "unselected") |> ignore
        let replica = write rows[4] gens[0] (contents "other replica")
        let files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
        let before = files |> Array.map (fun path -> path, File.ReadAllBytes path, File.GetLastWriteTimeUtc path)
        // Completed state and current generation do not identify a saved report. Duplicate selections load once.
        let selected = rows[0].WithGenerationCurrent(Some 99<generationNumber>).WithRunFinished(Some true)
        let! result = RunSummaryFiles.loadAsync folder configured [|selected; rows[4]; rows[2]; selected|] CancellationToken.None
        Assert.Equal<string>([|latest; replica|], result.Reports |> Array.map (fun report -> report.Path))
        Assert.Equal<int>([|gens[1]; gens[0]|], result.Reports |> Array.map (fun report -> report.Generation))
        Assert.Equal("latest", result.Reports[0].Rows[0][1])
        Assert.Equal("", result.Reports[0].Rows[0][2])
        Assert.Single(result.Errors) |> ignore
        Assert.Contains("No saved Summary Report", result.Errors[0])
        Assert.Equal<string>(files, Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
        for path, bytes, modified in before do
            Assert.Equal<byte>(bytes, File.ReadAllBytes path)
            Assert.Equal(modified, File.GetLastWriteTimeUtc path)
        let! continuation = RunSummaryFiles.loadAsync folder configured [|rows[4].WithGenerationFirst(Some (UMX.tag gens[0]))|] CancellationToken.None
        Assert.Empty(continuation.Reports) // The saved report predates this continuation's start.
        Assert.Single(continuation.Errors) |> ignore
    })

    [<Theory>]
    [<InlineData("duplicate headers")>]
    [<InlineData("missing RunName")>]
    [<InlineData("wrong run")>]
    [<InlineData("short row")>]
    [<InlineData("empty file")>]
    [<InlineData("header only")>]
    let ``viewer reports malformed files without displaying partial rows and continues loading`` problem = withReports (fun folder configured rows write -> task {
        let name = string configured.RunName
        let invalid =
            match problem with
            | "duplicate headers" -> sprintf "RunName\tMetric\tMetric\n%s\t1\t2\n" name
            | "missing RunName" -> "Metric\n1\n"
            | "wrong run" -> "RunName\tMetric\nOtherRun\t1\n"
            | "short row" -> sprintf "RunName\tMetric\n%s\t1\n%s\n" name name
            | "empty file" -> ""
            | _ -> "RunName\tMetric\n"
        let generation = GenIntervalConfig.getSampleAtIndex (RunIntervals.forRun configured |> Option.get) 0 |> Option.get
        let broken = write rows[0] generation invalid
        let valid = write rows[1] generation (sprintf "Metric\tRunName\n2\t%s\n" name)
        let! result = RunSummaryFiles.loadAsync folder configured rows[..1] CancellationToken.None
        Assert.Single(result.Reports) |> ignore
        Assert.Equal(valid, result.Reports[0].Path)
        Assert.Single(result.Errors) |> ignore
        Assert.Contains(broken, result.Errors[0])
    })

    [<Fact>]
    let ``viewer rejects foreign selections and honors cancellation`` () = withReports (fun folder configured rows _ -> task {
        let! empty = RunSummaryFiles.loadAsync folder configured [||] CancellationToken.None
        Assert.Empty(empty.Reports)
        Assert.Empty(empty.Errors)
        let! foreign = RunSummaryFiles.loadAsync folder configured [|rows[0].WithRunName(Some (UMX.tag "OtherRun"))|] CancellationToken.None
        Assert.Empty(foreign.Reports)
        Assert.Contains("another run", Assert.Single(foreign.Errors))
        use cancellation = new CancellationTokenSource()
        cancellation.Cancel()
        let! _ = Assert.ThrowsAnyAsync<OperationCanceledException>(fun () -> RunSummaryFiles.loadAsync folder configured rows cancellation.Token :> Task)
        Assert.Empty(Directory.GetDirectories(folder, "Results", SearchOption.AllDirectories))
    })

    [<Fact>]
    let ``combined export preserves header row order and literal values`` () = withReports (fun folder _ _ _ -> task {
        let report = DataTableReport.create "Combined" [|"Score"; "RunName"; "Seed"; "Optional"|]
        report.AppendDataRow [|"2"; "Essai-é"; "18446744073709551615"; ""|]
        report.AppendDataRow [|"10"; "quoted \"name\", with comma"; "1"; "present"|]
        let path = Path.Combine(folder, "combined.tsv")
        let! saved = RunSummaryFiles.saveCombinedAsync path report false
        Assert.Equal(path, saved)
        Assert.Equal<string>([|"Score\tRunName\tSeed\tOptional"; "2\tEssai-é\t18446744073709551615\t";
                              "10\tquoted \"name\", with comma\t1\tpresent"|], File.ReadAllLines path)
        Assert.Empty(Directory.GetFiles(folder, ".summary-*.tmp"))
    })

    [<Theory>]
    [<InlineData(false)>]
    [<InlineData(true)>]
    let ``combined export replaces files only when requested and cleans up temporary output`` overwrite = withReports (fun folder _ _ _ -> task {
        let report = DataTableReport.create "Combined" [|"RunName"|]
        report.AppendDataRow [|"NewRun"|]
        let path = Path.Combine(folder, "combined.txt")
        File.WriteAllText(path, "Original report")
        if overwrite then
            let! _ = RunSummaryFiles.saveCombinedAsync path report true
            Assert.Equal<string>([|"RunName"; "NewRun"|], File.ReadAllLines path)
        else
            let! _ = Assert.ThrowsAsync<IOException>(fun () -> RunSummaryFiles.saveCombinedAsync path report false :> Task)
            Assert.Equal("Original report", File.ReadAllText path)
        Assert.Empty(Directory.GetFiles(folder, ".summary-*.tmp"))
    })
