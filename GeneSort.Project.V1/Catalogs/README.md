# Project function catalogs

All public catalog APIs and their built-in implementations use the namespace
`GeneSort.Project.V1`. The folders group responsibilities without adding namespace
depth or changing persisted catalog keys.

| Folder | Purpose | Entry points |
| --- | --- | --- |
| `Queries` | Build storage query metadata from run parameters | `QueryCatalogNames`, `QueryParamsCatalog`, `QueryParamsBuilders` |
| `RunParameters` | Filter and enhance run parameters | `RunParamBuilderNames`, `RunParamFilterBuilders`, `RunParamEnhancerBuilders` |
| `Evolution` | Build tests, seed pools, and mutators | `SortableTestsMakerCatalog`, `SeedSorterPoolSetMakerCatalog`, `MutatorMakerCatalog` |

Call `ProjectCatalogs.registerAll()` at startup to register all built-ins. This is
safe to repeat. The individual registration entry points remain available.
Register additional implementations under distinct names, then resolve them with
the relevant catalog's `get` function. Query and evolution catalogs reject
duplicate registrations; run-parameter catalogs retain their existing replacement
behavior.

## Evolution makers

`EvolutionTestMakers`, `SeedPoolSetMakers`, and `MutatorMakers` contain the built-in
functions. `SeedSorterModelGeneration` holds the shared seed-generation rules.
`MakerNames.fs` defines stable names and the `EvolutionMakerNames.standard`,
`merge`, and `prefix` presets. `MakerCatalogs.fs` defines function types,
registries, and built-in registration.

Test and seed-pool makers take an `evolutionMakerContext` before `runParameters`.
The context supplies typed library loaders. This keeps database implementations,
filesystem paths, and host configuration out of Project, avoids a dependency cycle,
and allows tests or other hosts to provide alternative storage.

Dispatch binds its existing library databases in
`SorterSgd/MakerContext.fs`. `SgdRun.create` takes an `evolutionMakerNames` record
as its first argument. `sgdRunHost` resolves all three functions when created,
so unknown names fail before execution. The executor supplies the storage context.
Mutator makers require only run parameters.

The generation strategy is independent: use `GenStandard` or `GenSoss` with any
appropriate maker preset. The former `GenMerge` and `GenPrefix` SGD cases are
replaced by `GenStandard` plus the merge or prefix preset. Reporting cases are
unchanged.

Maker names are saved with the run. For older saved SGD runs without maker fields,
the DTO reader recovers prefix/merge presets from project segments or legacy
generation suffixes (including Soss's former prefix default); other legacy runs
use standard. Explicit names always take precedence, and partially populated
maker fields are rejected.

Seed makers return the `sorterPoolSet` union. Seed catalog keys identify both the
output variant and input source: `seed-pool.standard.standard`,
`seed-pool.standard.merge`, `seed-pool.standard.prefix`, and the corresponding
`seed-pool.soss.*` keys. Use `EvolutionMakerNames.sossStandard`, `sossMerge`, or
`sossPrefix` with `GenSoss`. The original presets produce Standard pools.

Each workflow unwraps its expected variant before evaluation or saving and returns
an error for an incompatible maker. The Soss makers perform seed conversion;
generation pipelines remain strongly typed. Legacy `seed-sorter-pool-set.*` keys
remain Standard aliases. The DTO reader upgrades these legacy keys for saved runs
ending in `GenSoss` to the matching Soss key; custom and explicit variant keys are
preserved.

```fsharp
ProjectCatalogs.registerAll ()
let makeTests = SortableTestsMakerCatalog.get SortableTestsMakerNames.prefix context
let makeSeedPool = SeedSorterPoolSetMakerCatalog.get SeedSorterPoolSetMakerNames.prefix context
let makeMutator = MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel
```

Use a new stable name when an implementation needs a distinct identity. Existing
names remain unchanged by this reorganization.
