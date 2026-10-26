namespace GeneSort.Project.V1.Runtime

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1

module RunIntervals =
    let resolve runName name =
        match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue name with
        | true, interval -> interval
        | _ -> invalidArg (nameof name) (sprintf "Sampling interval '%s' is not registered for run '%s'." name (%runName))

    let forRun (configured: run) =
        configured.GenSaveIntervalsName |> Option.map (resolve configured.RunName)
