# Setup files and saved inputs

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
contains requested-to-stored aliases. No new files or MessagePack fields are used.

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

SGD configurations use `sgdSetupConfig`: parameter spans, a query catalog, and a
parameter enhancer. There is no `Setup.FunctionName`. The execution family selects
Standard or Soss start creation; a continuation source selects importing instead.
Simple evaluation/mutation workflows retain their explicit setup function choice.
Older saved SGD labels are ignored on read and written as null in the existing DTO field.

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

`EvoStart.attach` records the two references and starting generation on the existing
parameter row. The execution ID is unchanged. Evolution consumes this start through
`evoPath`, regardless of how the files were prepared.

Continuation configurations no longer choose a seed Setup query catalog. They use
`QueryParamsBuildersSetup.ImportedStart.makeQueryParams`, which reads only explicit
Setup file IDs. They retain their explicit Results query catalog.

Old append setup labels, `SourcePoolFileId`/`SourceTestsFileId` aliases, and the old
off-by-one checkpoint generation convention are unsupported. Rerun those sources
and regenerate continuation parameters. Existing fresh-run IDs and files are unchanged.
