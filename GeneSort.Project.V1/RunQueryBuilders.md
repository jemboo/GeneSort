# Editing a file's query constructor

Each output has its own named constructor. A constructor reads its required
parameters, lists its filename properties, and calls `queryParams.create` once.
It does not call another output's constructor or add fields after construction.
Optional fields are explicit; original strings are retained where required by
the saved identity format.

The Run and Setup catalogs remain direct name-to-function maps. Their
`makeQueryParams` functions contain only an output-type match selecting a named
constructor. Those selection adapters contain no identity rules.

## Soss execution files

`QueryCatalogNamesRun.sorterSgdMsufSoss` selects
`QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeQueryParams`.

Within that module, edit the constructor for the file you intend to change:

| File | Constructor |
| --- | --- |
| Run definition query | `makeRunQueryParams` |
| Run parameters | `makeRunParametersQueryParams` |
| Pool checkpoint | `makeSorterPoolSetQueryParams` |
| Pool summary | `makeSorterPoolSetSummarySetQueryParams` |
| Bins | `makeSorterPoolBinsSetSeriesQueryParams` |
| Text report | `makeTextReportQueryParams` |
| Requested tests input | `makeSortableTestsWithPrefixQueryParams` |

Each function contains its own `activatorMutationRateKey` entry. Changing the
pool constructor does not change the summary, bins, report, or another catalog's
constructors. The repetition is intentional.

## Stored setup files

Setup file recipes are constructors in `QueryParamsBuildersSetup`.
For SossS, the setup catalog is `sossMsuf32Seed`; its stored-file constructors
are in `QueryParamsBuildersSetup.SossMsuf32Seed`:

- `makePrefixSeedPoolQueryParams`: the shared starting pool.
- `makePrefixTestsQueryParams`: the shared tests and prefix.
- `makePrefixControlledSeedPoolQueryParams`: the separate controlled-seed recipe.

Each contains its literal `SetupRecipe` version and its own property list.
There are no shared model-key lists, family flags, or recipe-name composition.
Other catalogs have independent copies, even where their current fields match.

Setup constructors return stored IDs directly. `SetupInputReferences` handles
saved/imported IDs and legacy addresses. There is no generated-file alias map.
Results keep their Run constructor identities.

## Compatibility

This refactor preserves existing query IDs and saved-file formats. Future edits
to a constructor's fields can intentionally change that file's ID. Check the
query compatibility tests before making such changes. Source-library adapters
retain their historical identities separately from execution constructors.

