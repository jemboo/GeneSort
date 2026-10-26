namespace GeneSort.Project.V1.Runtime

open System
open System.Numerics
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage

[<CLIMutable>]
type runPreparationArgs = {
    Run: run
    FirstReplica: int
    ReplicaCount: int
}

module RunPreparation =
    let private range (args: runPreparationArgs) =
        if args.FirstReplica < 0 then invalidArg "FirstReplica" "First replica must be zero or greater."
        if args.ReplicaCount <= 0 then invalidArg "ReplicaCount" "Replica count must be greater than zero."
        let lastExclusive = int64 args.FirstReplica + int64 args.ReplicaCount
        if lastExclusive > int64 Int32.MaxValue then invalidArg "ReplicaCount" "The replica range is too large."
        UMX.tag<replNumber> args.FirstReplica, UMX.tag<replNumber> (int lastExclusive)

    /// Lightweight preview for independent runs; does not expand combinations or access storage.
    let count (args: runPreparationArgs) =
        range args |> ignore
        if args.Run.ParameterSource.IsSome then invalidArg "Run" "Continuation counts require source selection."
        let product spans = spans |> List.fold (fun (total: BigInteger) (_, values) -> total * bigint (List.length values)) BigInteger.One
        product args.Run.SetupParameterSpans * product args.Run.RunParameterSpans * bigint args.ReplicaCount

    let prepareWith makeRunParameters (args: runPreparationArgs) =
        let first, lastExclusive = range args
        ProjectCatalogs.registerAll ()
        let parameters: runParameters[] = makeRunParameters first lastExclusive
        if parameters |> Array.exists (fun parameter -> parameter.GetRepl() |> Option.forall (fun replica -> replica < first || replica >= lastExclusive)) then
            invalidArg "makeRunParameters" "The generated parameters must belong to the requested replica range."
        let queryFor = QueryParamsCatalogRun.get args.Run.RunQueryCatalogName args.Run.ProjectName args.Run.DatabaseName
        let entries = parameters |> Array.map (fun parameter ->
            let query = queryFor parameter (geneSortDataType.RunParameters args.Run.RunName)
                        |> Option.defaultWith (fun () -> invalidOp "Could not build a RunParameter file identity.")
            query, parameter)
        runParameterSet.create { Run = args.Run; Entries = entries }

    let prepare args =
        if args.Run.ParameterSource.IsNone then
            let size = count args
            if size <= BigInteger.Zero || size > bigint Int32.MaxValue then
                invalidArg "Run" "The configuration must produce a nonempty parameter set that fits in memory. Reduce oversized sweeps before generating."
        prepareWith (RunParameterGeneration.generate args.Run) args

    let publishAsync (db: IGeneSortDb) (prepared: runParameterSet) mode (progress: IProgress<string> option) = async {
        match db with
        | :? IRunParameterStore as store -> return! store.saveRunParameterSetAsync prepared mode progress
        | _ -> return Error "This database does not support publishing RunParameter sets."
    }

    let initializeAsync db progress args mode makeRunParameters = async {
        try
            let prepared = prepareWith makeRunParameters args
            return! publishAsync db prepared mode progress
        with error -> return Error (sprintf "Could not prepare RunParameter files: %s" error.Message)
    }
