# Project function catalogs

All workflow catalogs and their built-in implementations live under
`GeneSort.Project.V1.Catalogs`. Each catalog folder contains its implementations,
grouped by workflow where needed. Project depends on storage contracts, never
Dispatch, FileDb, or application filesystem paths.

| Folder | Purpose | Entry points |
| --- | --- | --- |
| `Queries` | Build setup and execution storage queries | `QueryParamsCatalogSetup`, `QueryParamsCatalogRun` |
| `RunParameters` | Enhance run parameters | `RunParamBuilderNames`, `RunParamEnhancerBuilders` |
| `SetupParameters` | Enhance setup parameters and record source references | `SetupEnhancerCatalog` |
| `Evolution` | Build tests, seed pools, and mutators | `SortableTestsMakerCatalog`, `SeedSorterPoolSetMakerCatalog`, `MutatorMakerCatalog` |
| `Execution` | Execute prepared runs, including Standard and Soss evolution | `ExecuteFunctionCatalog` |
| `Setup` | Generate or copy inputs and select continuation sources | `SetupCatalog`, `SetupExecutorCatalog` |
| `SorterModelSet` | Select model-set setup functions | `SorterModelSetCatalog` |
| `SortableTestsWithPrefix` | Select test-set setup functions | `SortableTestsWithPrefixCatalog` |
| `Reports` | Build evaluation, mutation, and evolution reports | `ReportCatalog` |

Call `ProjectCatalogs.registerAll()` at startup to register query, parameter, and
evolution makers. This is safe to repeat. Setup, execution, and report registries
contain their built-ins when first accessed. Individual registration entry points remain available.
Register additional implementations under distinct names, then resolve them with
the relevant catalog's `get` function. Query and evolution catalogs reject
duplicate registrations; run-parameter catalogs retain their existing replacement
behavior.

Run parameter combinations pass directly to the selected enhancer. Specify the
desired combinations in the run's parameter spans; refinement retains every
combination.

Setup enhancers place required source file IDs on `setupParameters`. Use
`SetupEnhancerCatalog.withFileReferences` to resolve IDs from the setup query
builder, or set the IDs directly for external sources. Run generation copies
these references into each paired `runParameters`. Setup queries exclude the
references from identity; run queries include them. Keep source ID metadata out
of setup identity calculations when adding new query entry points.

Prefix evaluation uses settings directly instead of setup reference IDs. Its
setup enhancer preserves the setup settings; its run enhancer copies RNG type,
prefix library, model type, data format, and sorter count from the paired setup.
Run generation omits setup linkage for the prefix evaluation query catalog.
Setup file queries exclude the evaluation method, while result queries include
it. Both include sorter count, so changing the input count changes identity.

## Evolution makers

`EvolutionTestMakers`, `SeedPoolSetMakers`, and `MutatorMakers` contain the built-in
functions. `SeedSorterModelGeneration` holds the shared seed-generation rules.
`EvolutionMakerNames.fs` defines stable names and the `EvolutionMakerNames.standard`,
`merge`, and `prefix` presets. `MakerCatalogs.fs` defines function types,
registries, and built-in registration.

Test and seed-pool makers take an `evolutionSetupSources` before `runParameters`.
The plain record in `Evolution/EvolutionSetupSources.fs` supplies five named loader functions.
Construct it with named fields; replace individual loaders with F# record updates.
This keeps database implementations,
filesystem paths, and host configuration out of Project, avoids a dependency cycle,
and allows tests or other hosts to provide alternative storage.

Dispatch binds its existing library databases in `WorkflowSources.fs`.
`SgdRun.create` takes maker names in its named SGD configuration record.
`sgdRunHost` resolves all three maker functions when created, so unknown names fail
before execution. Setup supplies `workflowSources.Evolution`; mutator makers
require only run parameters.

## Storage and phase functions

`GeneSort.Project.V1.Storage` owns `IGeneSortDb`, `allowOverwrite`, and
`sourceDatabaseOpener`. FileDb implements the interface. Project does not reference
FileDb or Dispatch. The former contract-only `GeneSort.Db.V1` project has been removed.

`GeneSort.Project.V1.Runtime` defines:

- `runFunction`: a bound setup, execution, or report phase, accepting run parameters,
  overwrite policy, cancellation, and progress.
- `runStorage`: the destination database and its run-query builder.
- `sgdExecution`: the required mutator maker and generation save intervals.
- `executionBinding`: either a simple storage binding or an SGD binding that also
  requires `sgdExecution`.
- `workflowSources`: the source database opener and saved-library loaders used by
  setup and source-dependent reports.

Execution catalogs register `SimpleExecution` or `SgdExecution` builders. A host
binds its execution function once. A simple run cannot bind an SGD entry;
`WithRunDb` creates a new binding while previously obtained functions retain their
original database. The batch runner invokes `runFunction` directly, without an
executor interface or a host parameter.

Setup and report factories accept source services, a run configuration, and its
`runStorage`; they never receive `runHost`. Dispatch supplies its concrete
`WorkflowSources.defaults` when binding phases:

```fsharp
let setup = SetupExecutorCatalog.get sources configured storage
let report = ReportCatalog.get ReportCatalog.evolutionSummary sources configured storage
```

Source opening and library access occur when a phase needs them. Execution bindings
contain no source loaders. Continuation ranking and copying remain setup work, so
execution can use only the prepared destination data. Shared immutable setup files
are coordinated by `Storage/SetupArtifacts.fs`.

Dispatch retains run configurations, database creation, file-backed library loaders,
batch scheduling, and command-line entry points. It resolves Project catalog entries
and invokes the returned functions. The setup runner and report implementations are
reusable without the Dispatch host.

Maker names are saved with the run. SGD DTOs must provide all three nonblank maker
names and an explicit execution function name. Project and run labels never infer
or rewrite maker selections.

Seed makers return the `sorterPoolSet` union. Seed catalog keys identify both the
output variant and input source: `seed-pool.standard.standard`,
`seed-pool.standard.merge`, `seed-pool.standard.prefix`, and the corresponding
`seed-pool.soss.*` keys. Use `EvolutionMakerNames.sossStandard`, `sossMerge`, or
`sossPrefix` with `ExecuteFunctionNames.soss`. The original presets produce Standard pools
for `ExecuteFunctionNames.standard`.

Each workflow unwraps its expected variant before evaluation or saving and returns
an error for an incompatible maker. The Soss makers perform seed conversion;
generation pipelines remain strongly typed. Only current variant-specific keys
and explicitly registered custom keys are supported.

```fsharp
ProjectCatalogs.registerAll ()
let names = EvolutionMakerNames.standard
let makeTests = SortableTestsMakerCatalog.get names.SortableTests sources.Evolution
let makeSeedPool = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet sources.Evolution
let makeMutator = MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel
```

Use a new stable name when an implementation needs a distinct identity. Existing
names remain unchanged by this reorganization.
