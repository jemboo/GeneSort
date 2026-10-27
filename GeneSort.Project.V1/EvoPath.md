# One evolutionary path

`run` describes a workflow and its alternatives. `evoPath` describes one selected
execution: one replica, one value for each parameter, and concrete setup inputs.

For `Soss.SossS`, one replica expands to eight paths: four `MutationMod` values
times two activator mutation rates. Each path has its own existing execution ID.
The two rates for a given `MutationMod` share the same initial pool, and all eight
paths share the tests. Their combined SetupFiles inventory contains five stored files.

## Specification and progress

`evoPath` has a private constructor. Setup uses `EvoPath.fromStart` with its exact
start; `EvoPath.fromParameters` resolves saved rows and generation previews.
It contains:

- `Id` and `Replica`, taken from the existing row.
- `Workflow`, containing selected function/catalog/maker names and run ownership.
  It contains no parameter spans or alternative source runs.
- `Settings`, the selected parameter values, including provenance and setup IDs.
- `Start`, containing tests, initial pool, and starting generation.
- `SetupInputs`, a view of the start's two file references by output type.
- Resolved generation-save and summary-interval configurations.

`evoProgress` contains `CurrentGeneration`, `Finished`, `ElapsedTime`, and
`Message`. Those fields are absent from `path.Settings`.

`GenerationFirst` belongs to the specification: it identifies the starting
checkpoint of a continuation. Advancing `CurrentGeneration` does not change it.

```fsharp
let path, progress = EvoPath.fromParameters configured savedParameters
let advanced = progress.WithCurrentGeneration(Some nextGeneration)
// path, path.Id, path.Settings and path.SetupInputs are unchanged.
```

Resolving a path is a pure operation. Concrete file bindings do not imply that
the files already exist; Setup materializes them before execution can load them.

## Preparation and continuations

`RunPreparation.prepare` exposes `prepared.EvolutionPaths`. Simple (non-evolution)
runs have an empty array. Evolution entries are:

```fsharp
match entry with
| Ready (path, progress) -> // Fully selected specification and separate state.
    printfn "%O uses %d stored inputs" path.Id path.SetupInputs.Count
| AwaitingSetup id -> // The continuation's source checkpoint still needs binding.
    printfn "%O requires Setup" id
```

A continuation becomes resolvable after `EvoCheckpoint.load` validates its checkpoint,
an `EvoStartImport` function prepares its inputs, and the import branch in
`SetupExecutorCatalog` records the pool/test IDs. `EvoStart.initializeExecution`
then sets the starting generation and initial progress without rewriting the IDs.
`SetupExecutorCatalog.prepare` calls `EvoPath.fromStart` once, then returns
`Evolution (path, progress)`. The path contains the exact `evoStart`; progress
stays separate. The shared phase boundary uses `Parameters row` for simple runs.
There is no extra record holding a parameter row alongside the start.
The earlier preparation snapshot does not mutate itself.

## Execution

`ExecuteFunctionCatalog.bindPath storage` returns an `evoPathFunction`:

```fsharp
let execute = ExecuteFunctionCatalog.bindPath storage
let! result = execute path progress allowOverwrite cancellation log
// result: Result<evoProgress, string>
```

The path supplies the workflow, interval configuration, and `path.Start` directly
to the executor. Standard and Soss execution load `start.Tests` and `start.Pool`
from Setup through the original database; no storage wrapper or Setup query
reconstruction is involved. Results queries continue to use the existing query
builders, so checkpoints and reports keep their identities.

Standard and Soss executors, mutator makers, and generation loops read fixed
settings directly from `evoPath`. Execution returns only `evoProgress`, so its
return value cannot change a rate, identity, origin, or Setup reference.
`bindPath` passes the supplied path and progress directly to the executor.
Execution uses the exact path returned by Setup; it calls neither `fromStart`
nor `fromParameters` for a live path. `RunExecutionFiles.runPathsAsync` and
`ProjectOps.executePaths` accept the selected `(evoPath, evoProgress)` pairs and
call `ExecuteFunctionCatalog.bindPath` directly. There is no row-to-path lookup.

Each UI list item holds its path and progress. Setup and execution results update
the selected items directly, using the original selection indices, even if the
display is sorted. Completing a phase does not reload parameter files or rebuild
paths. Explicitly opening or refreshing saved rows calls `EvoPath.fromParameters`
once for each row. Rows still awaiting Setup remain available for preparation.

Dispatch retains the paths returned by `ProjectOps.prepareRuns`, passes them
directly to `executePaths`, and retains the updated progress. Starting without
live paths calls `ProjectOps.reopenPaths` at the application boundary.

Storage validation compares each supplied path's parameter representation with
its saved file before any execution. A changed file requires reopening; execution
never silently replaces the supplied path. `RunExecutionFiles.runSavedAsync` is
the explicit saved-row entry point for non-UI callers reopening parameter files.
Only the existing parameter representation is written to disk.

## Saved-file compatibility

`EvoPath.toParameters path progress` reconstructs the existing parameter map.
The adapter preserves missing optional fields, original string representations,
unknown settings, and IDs. It does not hash a new identity or write a new schema.
The existing `RunParametersDto` and MessagePack files remain the storage format.
`Storage/EvolutionFiles.fs` also uses this conversion at the file-query boundary:
checkpoint lookup and result queries retain their existing builders and IDs.
Generation loops pass the path, progress, and output generation to that boundary;
they do not reconstruct parameter rows themselves.

```fsharp
let path, progress = EvoPath.fromParameters configured savedParameters
let unchanged = EvoPath.toParameters path progress
// unchanged = savedParameters, including the original progress strings.
```

See [SetupInputs.md](SetupInputs.md) for direct Setup identities and saved references.

