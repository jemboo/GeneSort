# Enhancer parameter inventory

Setup and execution enhancers validate configured settings, derive dependent values, attach file references, and initialize execution identity/state. `ChannelCount` belongs to the run's module: modules place their fixed value in parameter spans, and continuations inherit it from their source. Modules also construct prefix IDs from their channel count, stage length, and variant. Enhancers do not choose these values.

This inventory covers the built-in functions in [SetupParametersEnhancers.fs](SetupParameters/SetupParametersEnhancers.fs) and [RunParametersEnhancers.fs](RunParameters/RunParametersEnhancers.fs). Their registrations live in [SetupParametersEnhancerCatalog.fs](SetupParameters/SetupParametersEnhancerCatalog.fs) and [RunParametersEnhancerCatalog.fs](RunParameters/RunParametersEnhancerCatalog.fs). Values below overwrite an existing value unless explicitly described as a missing-value default or an inheritance rule.

## Setup enhancers

Registration names in this section have the prefix `setup-parameters-enhancer.`.

| Registration | Parameters assigned |
| --- | --- |
| `no-op`, `sortable-tests` | None. |
| `sorter-eval`, `sorter-eval-prefix`, `sorter-mutate` | `SetupSorterModelSetId`, `SetupSortableTestsWithPrefixId`, computed by the selected setup query catalog. |
| `sorter-sgd` | `SetupSorterPoolSetId`, `SetupSortableTestsWithPrefixId`, computed by the selected setup query catalog. |
| `soss-msuf32` | The seed calculations and initial state listed below. |
| `standard-msuf32` | Seed calculations and initial state, then the two `sorter-sgd` file references. |
| `standard-msuf32-r`, `soss-msuf32-r` | Corresponding base behavior; requires explicit `AMax` and rejects `ActivationProbability`. |
| `standard-msuf32-s`, `soss-msuf32-s` | Corresponding base behavior; requires explicit `ActivationProbability` and rejects `AMax`. |

The shared seed calculations and initial state are:

| Parameter | Value |
| --- | --- |
| `SorterPoolCount` | Explicit positive setup value, preserved by enhancers. |
| `GenerationCurrent` | `0` |

The R/S wrappers validate activation settings before invoking their base enhancer. `standard-msuf32` also validates the full setup settings; `soss-msuf32` validates seed-pool settings.

## Run parameter enhancers for testing and evaluation

Registration names in the run parameter tables have the prefix `run-parameters-enhancer.`.

Every enhancer in this table assigns `DatabaseName` and `RunName` from the run, `RunFinished = false`, and `Id` from the selected run query catalog.

| Registration | Additional assignments |
| --- | --- |
| `sortable-tests` | None. |
| `sorter-eval-standard` | None. |
| `sorter-eval-merge` | None. |
| `sorter-eval-prefix` | None. |
| `sorter-mutate-standard` | None. |
| `sorter-mutate-standard-format` | None. |

The standard evaluation, both mutation, and merge evaluation enhancers copy missing setup values into execution parameters, excluding setup file references. They preserve values already present in execution parameters.

Prefix evaluation explicitly copies `RngType`, `PrefixLibId`, `SimpleSorterModelType`, `SortableDataFormat`, and `SorterCount` from setup. It also copies `AMax`, `ActivationProbability`, and `ActivatorMutationRate` when present. Fixed module settings, including `ChannelCount`, are already carried into execution by `RunParameterGeneration`.

## Explicit run settings

Mutation rates, population sizes, and selection settings are now declared in module parameter spans. Custom drafts can edit these values within the source's declared phases. Enhancers do not supply defaults or overwrite them.

| Parameter | Module defaults and behavior |
| --- | --- |
| `OrthoRate` | `4.001`; now editable and included in execution identities. |
| `ModificationRate` | Existing module sweeps; modules whose enhancer formerly forced `0.99` now explicitly declare `0.99`. |
| `AMax` | Explicit Setup parameter for SimpleR; defaults to 3 in module presets, preserving existing sweeps. |
| `ActivationProbability` | Explicit Setup parameter for SimpleS; defaults to 0.5 in module presets, preserving existing sweeps. |
| `ActivatorMutationRate` | `0.1` where previously defaulted for R/S runs, preserving existing explicit sweeps. |
| `SorterPoolCount` | Explicit number of pools. Together with `SorterCountPerPool`, defines population size without an inferred total. |
| `SorterCountPerPool` | Existing explicit pool-capacity values and sweeps. |
| `SelectedSorterCountPerPool` | A positive count or `All`. Modules that formerly selected the full capacity now declare `All`; pool-comparison modules retain their explicit selected counts. `All` resolves against each paired pool size, including resized continuations. |
| `SeedPoolSorterSelectionType` | `GuidOrder:512`, now an explicit Setup parameter. |
| `SorterChildCount` | `1` |
| `DistinctSorterHashes`, `PrioritizeNewMutants` | `true` |
| `SortedFraction` | `0.99` |

Changing the new execution controls changes run and evolutionary-result identities. Seed selection remains a Setup choice and contributes to setup identities. `SorterPoolCount` and `SorterCountPerPool` are explicit positive values. Enhancers preserve both; continuations inherit each value unless explicitly overridden.

## Module owned settings

Each SGD module declares a `moduleParameters` list containing its RNG type, model type, data format, prefix identity, evaluation measures, and fixed policies `ExcludeSelfCe = true` and `CollectNewSortableTests = false` (stored as `CollectSortableTests`). Evaluation and mutation modules also explicitly declare these policies. These values are saved in the Setup spans and inherited by execution and continuation runs. The UI shows them read-only in custom drafts; backend validation also rejects changes or removal.

| Module family | Model | Prefix stage / variant | RNG | Data format |
| --- | --- | --- | --- | --- |
| Msuf4 and test modules | Msuf4 | 4 / A | Lcg | BitVector512 |
| Msrs32 and Msrs24 3a | Msrs | 4 / A | Lcg | BitVector512 |
| Msrs24 3b | Msrs | 4 / B | Lcg | BitVector512 |
| Mssi24 3b | Mssi | 4 / B | Lcg | BitVector512 |
| Msuf6 24p3b | Msuf6 | 3 / B | Lcg | BitVector512 |

Most SGD modules declare both evaluation measures as `stageBiasedFilterUnsorted`. Msuf6 retains its existing evaluation-measure comparison sweep. Its module declares `SorterEvalMeasureInitial = SameAsEvaluation`; parameter generation resolves that reference for each Setup combination, so the initial and evolution measures stay paired. The measure sweep remains editable in custom drafts of those comparison runs.

Existing non-SGD model/prefix comparison parameters remain configurable. Evaluation and mutation modules now declare their data format explicitly where an enhancer previously supplied it. Single-valued RNG and data-format settings are read-only module settings across workflows; explicit comparison sweeps remain configurable.

The `24p3a` and `24p3b` Msrs/Mssi names still use stage length 4, preserving their former behavior. Only the Msuf6 `24p3b` module uses stage length 3.

## Remaining SGD enhancer assignments

Execution enhancers still set `CollectNewSortableTests = false` and None.. The Msuf32 family initializes `GenerationCurrent = 0`; the other families preserve it. Model, prefix, evaluation measure, RNG, and format defaults are no longer supplied by any registered enhancer.

All preserve the explicit `SorterPoolCount`, set `RunFinished = false`, copy `RunName`, and calculate `Id`. The `evo` and `soss` Msuf32 wrappers merge setup and execution maps with execution winning. R/S wrappers validate explicit activator mutation rates and reject activation parameters from the other family; they no longer invent mutation-rate defaults.

## Assignments outside enhancers

`RunParameterGeneration` pairs setup and execution combinations, copies fixed setup values, attaches setup references, records generated input-file identities, and calculates the final execution identity. It initializes `SetupCompleted = false`. Continuations import source settings and apply their execution overrides after enhancement. These lifecycle assignments are separate from the enhancer defaults listed above.

Registered enhancers no longer supply configurable defaults or fixed experimental policies. Activation settings must be declared by the source run and cannot be removed from a custom draft; their values remain editable during Setup. Changing either activation setting changes seed/setup identity.

## Consolidated implementations

Existing catalog names remain available, but equivalent registrations share one implementation:

- Setup evaluation, prefix evaluation, and mutation share the model-set and prefixed-test file-reference enhancer.
- Execution standard evaluation, merge evaluation, and both standard mutation registrations share setup inheritance and run initialization.
- The seven MSRS/MSUF6/MSSI continuation registrations share pool-size validation and run initialization without resetting the supplied generation.

Direct sortable-test execution shares run initialization but does not inherit setup. Prefix evaluation retains its required-field checks and setup-wins precedence. SGD MSUF initialization still validates execution settings and resets generation to zero; seed-only versus full setup validation, file-reference handling, and SimpleR/S validation remain distinct. Query builders and identity inputs are unchanged.

Pool configuration now requires `SorterPoolCount` and `SorterCountPerPool`.
Previously saved definitions using a total-population field must be updated to
these two explicit values; no pool count or capacity is inferred from a total.
