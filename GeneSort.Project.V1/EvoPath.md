# One evolutionary path

`run` describes a workflow and its alternatives. `evoPath` describes one selected
execution: one replica, one value for each parameter, and concrete setup inputs.

For `Soss.SossS`, one replica expands to eight paths: four `MutationMod` values
times two activator mutation rates. Each path has its own existing execution ID.
The two rates for a given `MutationMod` share the same initial pool, and all eight
paths share the tests. Their combined SetupFiles inventory contains five stored files.

## Specification and progress

`evoPath` has a private constructor. Use `EvoPath.fromParameters` to resolve a
saved or generated row against its owning `run`. It contains:

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
an `EvoStartImport` function prepares its inputs, and `EvoStart.attach` records the
pool/test IDs and starting generation. Resolve
the updated row with `EvoPath.fromParameters`; the earlier preparation snapshot
does not mutate itself when setup completes.

## Execution

`ExecuteFunctionCatalog.bindPath storage` returns an `evoPathFunction`:

```fsharp
let execute = ExecuteFunctionCatalog.bindPath storage
let! result = execute path progress allowOverwrite cancellation log
// result: Result<evoProgress, string>
```

The path supplies the workflow and interval configuration. `RunStorage.forPath`
binds Setup reads to its start's two files. Results queries continue to use the
existing query builders, so checkpoints and reports keep their identities.

The existing evolution algorithms still use `runParameters` internally. The
adapter joins specification and progress before calling them, then accepts only
progress changes from their returned parameters. A changed rate, identity,
origin, or setup reference is an error rather than a silent change to the path.
The existing `ExecuteFunctionCatalog.bind` API uses this same typed entry point,
so both Dispatch and WPF use it without changing their saved-file interfaces.

## Saved-file compatibility

`EvoPath.toParameters path progress` reconstructs the existing parameter map.
The adapter preserves missing optional fields, original string representations,
unknown settings, and IDs. It does not hash a new identity or write a new schema.
The existing `RunParametersDto` and MessagePack files remain the storage format.

```fsharp
let path, progress = EvoPath.fromParameters configured savedParameters
let unchanged = EvoPath.toParameters path progress
// unchanged = savedParameters, including the original progress strings.
```

See [SetupInputs.md](SetupInputs.md) for direct Setup identities and saved references.

