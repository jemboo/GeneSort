namespace GeneSort.Project.V1.Catalogs.Setup

open GeneSort.Project.V1

type setupFunction = runParameters -> Async<Result<unit, string>>

/// Per-run storage bindings. Catalogs never retain a host or overwrite policy.
type setupContext =
    private {
        makeSorterModelSets: Map<string, setupFunction>
        makeStandardTestsWithPrefix: setupFunction
        makeMergeTestsWithPrefix: setupFunction
        makePrefixTestsWithPrefix: setupFunction
    }

    static member create
            (makeSorterModelSets: Map<string, setupFunction>)
            (makeStandardTestsWithPrefix: setupFunction)
            (makeMergeTestsWithPrefix: setupFunction)
            (makePrefixTestsWithPrefix: setupFunction)
            : setupContext =
        {
            makeSorterModelSets = makeSorterModelSets
            makeStandardTestsWithPrefix = makeStandardTestsWithPrefix
            makeMergeTestsWithPrefix = makeMergeTestsWithPrefix
            makePrefixTestsWithPrefix = makePrefixTestsWithPrefix
        }

    member this.MakeSorterModelSets with get() = this.makeSorterModelSets
    member this.MakeStandardTestsWithPrefix with get() = this.makeStandardTestsWithPrefix
    member this.MakeMergeTestsWithPrefix with get() = this.makeMergeTestsWithPrefix
    member this.MakePrefixTestsWithPrefix with get() = this.makePrefixTestsWithPrefix

    member this.WithMakeSorterModelSets(value) = { this with makeSorterModelSets = value }
    member this.WithMakeStandardTestsWithPrefix(value) = { this with makeStandardTestsWithPrefix = value }
    member this.WithMakeMergeTestsWithPrefix(value) = { this with makeMergeTestsWithPrefix = value }
    member this.WithMakePrefixTestsWithPrefix(value) = { this with makePrefixTestsWithPrefix = value }

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
