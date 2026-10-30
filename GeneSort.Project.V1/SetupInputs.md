# Setup files and saved inputs

Architectural constraint: preserve [future single-machine deployment](../GeneSort.Dispatch.V1/CLOUD_WORKFLOW.md#single-machine-deployment-architectural-requirements).
This guides local development; cloud features are explicitly deferred.

Setup constructors return the actual stored file identities directly. There is
no requested-file alias map and no separate canonical-file constructor layer.

For SossS, `QueryParamsBuildersSetup.SossMsuf32Seed` contains:

- `makePrefixSeedPoolQueryParams`: starting pools, including MutationMod and
  ActivationProbability but excluding the execution ActivatorMutationRate.
- `makePrefixTestsQueryParams`: tests and prefix, independent of both mutation
  settings and activator mutation rate.

The Setup catalog selects these constructors. Identical setup inputs therefore
produce identical filenames without redirecting any query. Run constructors
continue to produce separate Results IDs for different activator mutation rates.

`RunParameterGeneration` records these Setup IDs in the existing `Setup*Id`
parameter fields. The prepared parameter set's `SetupFiles` lists the unique input files;
`evoPath.SetupInputs` binds each input output-type to its concrete file. Neither
contains requested-to-stored aliases.

## Existing data and imports

`SetupInputReferences` contains only reference and compatibility operations:

- `tryRecordedInput` reads explicit Setup IDs.
- `resolveRecorded` uses a recorded ID, otherwise retaining the supplied address.
- `resolveSavedInput` handles older datasets without references: pools keep their
  execution query at the origin, while tests/models use Setup constructors.
- `copyPool` and `copyTests` retain source IDs in the destination database.
- `convertedPool` preserves the existing family-conversion ID formula.
- `recordFiles` records concrete input IDs without changing other parameters.

Results never pass through input-reference resolution. Imports remain unresolved
until their checkpoint has been validated during Setup.

## Evolution starts

`evoStart` contains `Tests`, `Pool`, and `FirstGeneration`. `evoPath.Start` exposes
that value. It contains no recipes, aliases, execution progress, or source lookup.

SGD configurations use the standalone `sgdSetupConfig` record:

- `FunctionName` selects `setup.sgd-standard` or `setup.sgd-soss` directly.
- `SortableTestsModule` selects the input database under `SortableTests`.
- `SorterEvalModule` selects the seed evaluation database under `SorterEval`.
- `ParameterSpans`, `QueryCatalogName`, and `ParametersEnhancerName` describe the setup inputs.

The run-level `DatabaseName` remains the destination for prepared inputs and Results.
`preparedSetupConfig` is a separate record for evaluation/mutation setup;
`directRunConfig` describes SortableTests runs without a Setup phase.
The setup function and source database names are serialized explicitly. Old SGD
definitions without these fields are unsupported; recreate their definitions and parameters.
Source database names are recorded before setup enhancement. Test-file constructors
include the tests database, and seed-pool constructors include both databases, preventing
reuse of inputs created from a different source. Activator-rate sweeps still share inputs.

Fresh runs call `EvoStartCreation.createStandardStart` or `createSossStart`.
The caller supplies the run's project and database. `EvoStartCreation.files`
reads `SetupSortableTestsWithPrefixId` and `SetupSorterPoolSetId` directly,
preserving those IDs without calling a Run query builder. Missing or invalid
references fail before file access; regenerate run parameters to supply them.
Each function saves the tests, creates and evaluates the seed pool if absent,
and returns the two file references with generation zero. Shared file IDs still
come from the existing Setup constructors.

Continuations first call `EvoCheckpoint.load` to load one selected saved execution's
checkpoint and tests. Then `EvoStartImport.importStandardStart` or `importSossStart`
copies the inputs into the destination Setup database, converting the pool when
the destination uses a different execution family. These functions return a start;
they do not mutate run parameters or run evolution.

Fresh runs keep the Setup IDs already recorded by `RunParameterGeneration`.
Only the import branches in `SetupExecutorCatalog` record the imported file IDs,
after the checkpoint has been validated and its files prepared.
`EvoStart.initializeExecution` sets `GenerationFirst`, `GenerationCurrent`, and
`RunFinished`; it does not rewrite references or the execution ID. Evolution
consumes this start through `evoPath`, regardless of how the files were prepared.

`SetupExecutorCatalog.prepare` constructs the path with `EvoPath.fromStart` and
returns `Evolution (path, progress)`. Construction checks the run, recorded IDs,
and starting generation while retaining the exact start object. The UI and Dispatch
retain only `(evoPath, evoProgress)` pairs and pass the same path and progress to
`ExecuteFunctionCatalog.bindPath`. Execution returns new progress; the path stays
unchanged. The UI opens paths when loading saved rows and then updates the selected
items directly after each phase. Dispatch reopens through `ProjectOps.reopenPaths`
only when it has no live paths. Execution never matches rows back to paths or
reconstructs them. Saving uses `EvoPath.toParameters` and the existing DTO.

Standard and Soss executors receive `evoPath` and `evoProgress` directly, reading
fixed settings from the path and returning only progress. They load the start's
`Tests` reference from Setup and load its `Pool` reference when no Results
checkpoint exists. Execution does not build or redirect Setup queries; Results
lookup and saving continue to use the existing Run constructors through
`Storage/EvolutionFiles.fs`. Conversion to a parameter row happens at that file
boundary, not in `bindPath`, mutator makers, or generation loops.

Continuation configurations no longer choose a seed Setup query catalog. They use
`QueryParamsBuildersSetup.ImportedStart.makeQueryParams`, which reads only explicit
Setup file IDs. They retain their explicit Results query catalog.

Old append setup labels, `SourcePoolFileId`/`SourceTestsFileId` aliases, and the old
off-by-one checkpoint generation convention are unsupported. Rerun those sources
and regenerate continuation parameters. Fresh SGD file identities now include their source databases.
