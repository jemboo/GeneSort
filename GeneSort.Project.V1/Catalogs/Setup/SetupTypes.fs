namespace GeneSort.Project.V1.Catalogs.Setup

open GeneSort.Project.V1

type setupFunction = runParameters -> Async<Result<unit, string>>

/// Per-run storage bindings. Catalogs never retain a host or overwrite policy.
type setupContext = {
    MakeUniformSorterModelSet: setupFunction
    MakeStandardTestsWithPrefix: setupFunction
    MakeMergeTestsWithPrefix: setupFunction
    MakePrefixTestsWithPrefix: setupFunction
}

type setupBuilder = setupContext -> setupFunction

/// Shared registration rules for setup catalogs, including built-in entries.
type internal setupRegistry(catalogName: string, builtIns: (string * setupBuilder) list) =
    let syncRoot = obj ()
    let mutable entries = Map.ofList builtIns

    member _.Register name builder =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) (sprintf "A %s name cannot be empty." catalogName)
        lock syncRoot (fun () ->
            if Map.containsKey name entries then
                invalidOp (sprintf "A %s named '%s' is already registered." catalogName name)
            entries <- Map.add name builder entries)

    member _.Get name =
        lock syncRoot (fun () ->
            match Map.tryFind name entries with
            | Some builder -> builder
            | None -> invalidOp (sprintf "No %s named '%s' is registered." catalogName name))
