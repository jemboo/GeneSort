namespace GeneSort.Dispatch.V1

open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1

/// Shared setup files are immutable. Bound the lock table while coordinating hosts in this process.
module SetupArtifacts =
    let private gates = Array.init 64 (fun _ -> new SemaphoreSlim(1, 1))

    let getOrCreate (db: IGeneSortDb) (query: queryParams) (create: unit -> Async<Result<outputData, string>>) =
        async {
            let! token = Async.CancellationToken
            let gate = gates.[(query.Id.GetHashCode() &&& System.Int32.MaxValue) % gates.Length]
            do! gate.WaitAsync(token) |> Async.AwaitTask
            try
                return! asyncResult {
                    let! exists = db.doesOutPutDataExist geneSortDataMode.Setup query
                    if exists then return! db.loadAsync geneSortDataMode.Setup query
                    else
                        let! data = create ()
                        do! db.saveAsync geneSortDataMode.Setup query data (UMX.tag false)
                        // Another process may have published the same identity first.
                        return! db.loadAsync geneSortDataMode.Setup query
                }
            finally
                gate.Release() |> ignore
        }

    let saveOnce db query create = getOrCreate db query create |> Async.map (Result.map ignore)
