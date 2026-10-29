namespace GeneSort.Project.V1.Catalogs.Execution.SortableTests

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Model.Sortable.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Runtime.OpsUtils

module SortableTestsExecution =
    /// Generates standalone tests directly into Results for downstream workflows.
    let _runSortableTests
            (storage: runStorage)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let log msg = OpsUtils.report progress msg
        asyncResult {
            try
                do! checkCancellation cts.Token
                let! dataFormat = rp.GetSortableDataFormat()
                                  |> Result.ofOption "Missing sortable data format required for generation"
                let! model =
                    match rp.GetMergeLibId(), rp.GetPrefixLibId() with
                    | Some merge, None ->
                        msasM.create merge.ChannelCount merge.MergeDimension
                            mergeSuffixType.NoSuffix sorterLibVariant.VariantA
                        |> sortableTestsModel.MsasMi |> Ok
                    | None, Some prefix -> msasPfx.create prefix |> sortableTestsModel.MsasPfx |> Ok
                    | _ -> Error "Sortable test generation requires exactly one MergeLibId or PrefixLibId."
                let! query = storage.QueryParamsFromRunParams rp (geneSortDataType.SortableTests "")
                             |> Result.ofOption "Failed to create query parameters for SortableTests"
                log "Generating sortable tests..."
                let tests = SortableTestsModel.makeSortableTests (%query.Id |> UMX.tag) model dataFormat
                do! checkCancellation cts.Token
                do! storage.RunDb.saveAsync geneSortDataMode.Results query (outputData.SortableTests tests) allowOverwrite
                log "Sortable tests saved as Results. Run complete."
                return rp.WithRunFinished(Some true)
            with e ->
                return! Error (sprintf "Error generating sortable tests: %s" e.Message)
        } |> Async.map (logResult progress log)

