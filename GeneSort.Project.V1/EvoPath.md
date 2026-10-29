# One evolutionary path

Architectural constraint: preserve [future single-machine deployment](../GeneSort.Dispatch.V1/CLOUD_WORKFLOW.md#single-machine-deployment-architectural-requirements).
This guides local development; cloud features are explicitly deferred.

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


## Crash-safe checkpoint recovery

Standard and Soss publish each Results checkpoint as one group: the pool,
summaries, and bin series. FileDb writes each payload to a temporary sibling,
flushes it to disk, and atomically renames it. Parameter files use the same atomic
publication, so an interrupted replacement retains the previous file.

A pending commit manifest is installed before checkpoint payloads are changed.
Each member has a `.checkpoint` link to the manifest. Only after every payload is
flushed does an atomic manifest replacement publish its relative path and SHA-256
checksum. Readers reject pending groups, missing members, and checksum mismatches.
Recovery searches backwards for the newest readable checkpoint; it does not trust
saved GenerationCurrent or the presence of the pool alone. Retrying an interrupted
group can replace its incomplete files even when ordinary overwrite is disabled.
Earlier snapshots are retained; ordinary recovery checkpoints are pruned after a newer group commits. A per-group file lock prevents two
writers from publishing the same checkpoint concurrently and is released on death.

Legacy files without sidecars remain readable but do not acquire group integrity
guarantees retroactively. Copy sidecars along with data when moving a database;
all references are relative. Orphan `.tmp` files are ignored. Retention only prunes
explicitly classified checkpoints; no cloud infrastructure is introduced.

CheckpointFileTests kills a separate worker process at pending, partial-member,
pre-commit, and post-commit boundaries for Standard/Soss payloads, reopens the
database, checks fallback, replays incomplete work, and corrupts a companion to
check integrity validation. This verifies abrupt process termination on the local
filesystem; it is not a hardware power-loss or remote filesystem durability test.

## Checkpoint and snapshot schedules

The editor's Snapshot schedule is the existing `GenSaveIntervalsName` setting.
`GenerationIntervalLast` continues to select its zero-based endpoint, preserving
existing run lengths. `CheckpointIntervalsName` independently selects recovery
save generations. The execution loop saves on the union of both schedules and
always saves the endpoint as a snapshot. A generation on both schedules is one
snapshot group, not duplicated data. Summary sampling remains independent.

An absent checkpoint schedule in older run files means the snapshot schedule
also supplies recovery points. Existing unclassified files are preserved, never
retroactively deleted. New checkpoint schedule settings round-trip through the
run editor and MessagePack run definition.

FileDb retention is scoped to each processing-row ID (including its replica),
not the shared run name. It keeps the highest valid generation plus all snapshots.
Snapshot groups are immutable under retention, including when ordinary overwrite
is enabled. A successful save validates the new group before retiring older
ordinary checkpoints. Cleanup first invalidates an old group and records its
member list, then deletes its files. Interrupted cleanup can finish on a later
save; a busy reader can temporarily defer deletion. Never manually remove sidecars.

Interim checkpoints include the summary/bin segment since the last snapshot.
Resume restores that segment and snapshots reset it. Deleting interim checkpoints
therefore does not remove samples needed for permanent timeline analysis. Local
cancellation uses both schedules when restoring GenerationCurrent. Cloud storage,
uploads, and provisioning remain deferred.
