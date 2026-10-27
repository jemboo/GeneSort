namespace GeneSort.Project.V1.Storage

open System
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

type runParameterWriteMode =
    | CreateNew = 0
    | Replace = 1
    | Resume = 2

type runParameterSetArgs = {
    Run: run
    Entries: (queryParams * runParameters)[]
    SetupFiles: queryParams[]
    EvolutionPaths: evoPathPreparation[]
}

/// A complete, validated set of parameter files for one run, ready for publication.
type runParameterSet =
    private { configured: run; entries: (queryParams * runParameters)[]; setupFiles: queryParams[]; evolutionPaths: evoPathPreparation[] }
    member this.Run = this.configured
    member this.Entries = Array.copy this.entries
    member this.FileCount = this.entries.Length
    /// Inspect unique, concrete Setup files without IO.
    member this.SetupFiles = Array.copy this.setupFiles
    member this.EvolutionPaths = Array.copy this.evolutionPaths
    static member create (args: runParameterSetArgs) =
        if Array.isEmpty args.Entries then invalidArg "Entries" "The configuration produces no RunParameter files."
        let seen = Collections.Generic.HashSet<_>()
        for query, parameters in args.Entries do
            if query.ProjectName <> args.Run.ProjectName || query.DbName <> args.Run.DatabaseName
               || query.OutputDataType <> geneSortDataType.RunParameters args.Run.RunName
               || parameters.GetRunName() <> Some args.Run.RunName
               || query.Repl <> parameters.GetRepl() || (query.Repl |> Option.forall (fun repl -> int repl < 0)) then
                invalidArg "Entries" "RunParameter files must belong to the configured run and a nonnegative replica."
            if not (seen.Add(query.Repl, query.Id)) then
                invalidArg "Entries" "Multiple parameter combinations produce the same RunParameter file. Remove duplicate combinations."
        { configured = args.Run; entries = Array.copy args.Entries; setupFiles = Array.copy args.SetupFiles; evolutionPaths = Array.copy args.EvolutionPaths }

/// Storage capability for publishing a definition together with its complete parameter set.
/// Implementations stage writes and restore the previous set if publication fails.
type IRunParameterStore =
    abstract saveRunParameterSetAsync:
        runParameterSet -> runParameterWriteMode -> IProgress<string> option -> Async<Result<unit, string>>
