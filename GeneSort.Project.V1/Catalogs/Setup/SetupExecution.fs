namespace GeneSort.Project.V1.Catalogs.Setup

open System
open System.Threading
open GeneSort.Core
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Runtime.OpsUtils

module SetupExecution =
    /// Creates the Setup artifacts without evaluating sorters or completing the run.
    let run
            (setup: setupFunction)
            (rp: runParameters)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let log msg =
            OpsUtils.report progress
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (RunParameters.getIdString rp) msg)
        asyncResult {
            try
                do! checkCancellation cts.Token
                log "Generating and saving evaluation setup..."
                do! setup rp
                do! checkCancellation cts.Token
                log "Setup complete. Ready to run evaluation."
                return rp
            with e ->
                let errorMsg = sprintf "Setup error in %s: %s" (RunParameters.getIdString rp) e.Message
                log errorMsg
                return! Error errorMsg
        } |> Async.map (logResult progress log)

