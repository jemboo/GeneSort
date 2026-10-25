# Local setup and cloud execution

Run configuration modules live under `RunCfg` in each workflow's folder and
namespace: `SorterSgd.RunCfg`, `SorterEval.RunCfg`, `SorterMutate.RunCfg`, and
`SortableTests.RunCfg`. Execution code and shared database readers stay in their
workflow folders. Project names derived from these modules include `RunCfg`.

`SorterSgd.RunCfg.p32.Test.OnePool` provides `TestStandard` and `TestSoss` base
runs with one pool of 32 sorters for each mutation value 0–3. Run the desired base through
setup and execution before using its interval-0 checkpoint in `TestStandardAppend`,
`TestSossAppend`, `TestStandardCondense`, or `TestSossCondense`.
All OnePool configurations use 32 total sorters and one pool per pool set.

`OnePool.SimpleR` adds `Standard`, `Soss`, `StandardAppend`, and `SossCondense`.
Edit its `aMax` and `activatorMutationRate` settings to configure these tests.
The existing OnePool runner discovers them automatically. StandardAppend continues
all four base executions; SossCondense selects two source pools and expands each
into mutation variants 0 and 1. Each resulting execution has one pool of 32.

For SimpleR in dispatch, add `ParamKeys.aMaxKey` (`AMax`, a nonnegative integer)
to setup spans and `ParamKeys.activatorMutationRateKey` (`ActivatorMutationRate`,
a finite probability from 0 to 1) to execution spans. Keep the underlying
`SimpleSorterModelType` such as `Msuf4`. AMax opts into SimpleR generation;
without either activation setting the existing Simple behavior is unchanged. Seed activators are uniform
in `[-AMax, AMax]`. Each selected mutation steps an activator by +/-1, clamped to
those bounds. Append and Condense inherit both settings; changing AMax requires
new seeds, while the activator mutation rate may be overridden for execution.
The values participate in file identities and appear in summary metadata.

`SorterSgd.RunCfg.p32.Msuf4a.SimpleR` provides `Standard.Test`,
`Standard.ActivatorRates`, `Soss.Test`, and `Soss.ActivatorRates`. Each execution
has one pool of 32, with mutation variants 0–3. Test runs use 10 generations;
the rate sweeps use 100 generations and cross AMax values 1, 3, 7 with activator
mutation rates 0.05, 0.1, 0.2 (36 executions per replica).
The `standardMsuf32R` / `sossMsuf32R` setup enhancer entries default AMax to 3;
the `evoMsuf32R` / `sossMsuf32R` execution enhancers default activator mutation
rate to 0.1. Explicit spans override these defaults. Invalid activation settings
are rejected during parameter preparation. These presets use the existing prefix
seed-evaluation and sortable-test catalogs; prepare `SorterEval.RunCfg.Prefix.Width32`
and `SortableTests.RunCfg.Prefix.Width32` before setup. Automatic naming produces
project `SorterSgd.RunCfg.p32.Msuf4a.SimpleR`, databases `Standard` and `Soss`,
and run names matching the preset functions.
Run all four presets with `scripts/Run-OnePool.ps1 -Msuf4aSimpleR`; it prepares
the prerequisites, runs replica 0, resumes completed work, regenerates reports,
and verifies the sweep, pool sizes, activation settings and every generation.
The aggregate is saved as `Reports/Msuf4a_SimpleR_SummaryReport.txt` under the
SimpleR project folder, with `Workflow` identifying the Standard/Soss databases.
Use `scripts/Compare-Msuf4aSimpleR.ps1 -ReportPath <path>` for endpoint and
trajectory comparisons across AMax and activator rates. The four mutation
variants share a seed replica; these results are exploratory rather than a
comparison across independent seed replicas.

`SorterSgd.RunCfg.p32.Msuf4a.SimpleS` mirrors the four SimpleR presets:
`Standard.Test`, `Standard.ActivatorRates`, `Soss.Test`, and `Soss.ActivatorRates`.
The test presets use 10 generations and catalog defaults of ActivationProbability
0.5 and ActivatorMutationRate 0.1. The 100-generation sweeps cross activation
probabilities 0.25, 0.5, 0.75 with mutation rates 0.05, 0.1, 0.2 and mutation
variants 0–3 (36 executions per replica). Every execution has one pool of 32.
Setup entries `standardMsuf32S` / `sossMsuf32S` and execution entries
`evoMsuf32S` / `sossMsuf32S` supply and validate the boolean activation settings.
Both probabilities accept 0 through 1; AMax is rejected. The project is
`SorterSgd.RunCfg.p32.Msuf4a.SimpleS`, with databases `Standard` and `Soss`.
These use the same prefix prerequisites as SimpleR. Equal mutation-rate values
do not imply equal effective activation flip rates between R and S.
Run all four presets with `scripts/Run-OnePool.ps1 -Msuf4aSimpleS`; it performs
setup, execution, summary reporting, and verification for replica 0. The combined
report is `Reports/Msuf4a_SimpleS_SummaryReport.txt` under the SimpleS project,
with `Workflow` distinguishing Standard and Soss. Compare the completed sweeps
using `scripts/Compare-Msuf4aSimpleS.ps1 -ReportPath <SimpleS report> -SimpleRReportPath <SimpleR report>`.
The comparison includes endpoints, SimpleS trajectories, and the best observed
settings in each family. It uses the four mutation variants of one seed replica;
the activation masks and effective flip rates are not matched between families.
Strict pool pruning normally rejects unsorted candidates. If that would leave no
evaluated candidates, Standard and Soss retain the best imperfect candidates,
ranking unsorted-case count before the configured size score. Strict filtering
resumes as soon as a candidate passes it. This allows sparse initial activation
settings to recover without changing the configured measure. Summary reporting
rejects saved slices that contain no report rows.

`OnePool.SimpleS` provides `StandardS`, `SossS`, `StandardAppendS`, and
`SossCondenseS`, with the same pool counts and continuation structure as SimpleR.
Its `activationProbability` defaults to 0.5 and `activatorMutationRate` to 0.1.
Distinct run names identify these runs in the combined report.

For SimpleS, set `ParamKeys.activationProbabilityKey` (`ActivationProbability`)
in setup instead of `AMax`, and supply `ActivatorMutationRate` in execution.
Both probabilities must be finite and between 0 and 1. Generation initializes
each boolean independently; only true entries contribute to the sorter.
Mutation flips each boolean at the configured rate. CE insertion/deletion keeps
activators aligned with their entries, with inserted entries initially false
before activator mutation. `AMax` and `ActivationProbability` are mutually
exclusive. Continuations inherit the model family and settings; execution uses
the prepared models rather than generating activators again. The OnePool audit
checks model family, activator lengths, and report metadata for both families.

`OnePool.Controlled` compares Simple, SimpleR, and SimpleS under Standard and
Soss, with 5 replicas, mutation variants 0–1, and 100 generations. Run just this
experiment with `scripts/Run-OnePool.ps1 -ControlledOnly`. The ordinary full
OnePool script includes it as well, using its configured replica count.

`ControlledSeedAMax` opts into deterministic, unselected random seeds generated
during setup instead of importing ranked seed evaluations. Underlying generator,
replica, and model index determine seed model IDs, independently of workflow,
mutation variant, and representation. All three families share the underlying
models. R draws uniform integers in [-3,3]; S copies the exact positive-entry mask
from those draws (nominal activation probability 3/7). Both use activator mutation
rate 0.1, which means bounded integer steps for R and flips for S. This controls
initial activity, not the later probability of changing active status.

`MatchedStandardS` and `MatchedSossS` add S comparisons at mutation rate
`0.1 / 7` against the same `StandardR` and `SossR` baselines. For uniformly
distributed R activators, only 0 -> 1 and 1 -> 0 flip activity, each with
probability `0.1 / 2`, giving an expected flip rate of about 1.43% per entry.
These runs reuse the exact seed masks, replicas, mutation variants and schedule.
The match is for the initial uniform distribution (also the neutral random
walk's stationary distribution); selection can change R's boundary occupancy
and therefore its later effective flip rate. It does not guarantee equal
realized flip counts throughout evolution.

The audit compares the saved underlying model DTOs and R/S masks, checks all
100 generations per evaluation subset, and verifies every expected execution.
Controlled-only output is `OnePool_Controlled_SummaryReport.txt` in the base
OnePool report folder. `scripts/Compare-OnePoolControlled.ps1 -ReportPath <path>`
summarizes endpoint metrics and paired differences after averaging the mutation
variants within each replica. Standard reports generations 0–99 and Soss 1–100;
compare model families within each workflow. Prefix test data is shared at
replica zero; the five experimental replicas vary the seed models.

Append tests use source mutation 0 and branch into mutation values 0–2,
preserving one pool of 32 in each of three runs. Condense tests rank all four
source mutation values and select the best two per replica. Both continuation workflows finish at interval 1, using the base's
save schedule; selection and copying happen before execution as usual.

Continuation sources require explicit `SourceMutationMods: int list` and
`SelectedSorterPoolCount: int` arguments, alongside `SourceRun` and `GenerationInterval`.
There is no arbitrary source filter array. The selected count is per source
parameter combination and replica; repeated mutation values from different source
executions remain separate candidates. Selecting every candidate continues them all
without ranking. `SourceRunName` is populated from `SourceRun`. `MutationMod` in execution
spans specifies destination variants. `TestSossCondense` branches each of its two
selected sources into mutations 0–3, producing eight runs with one pool of 32 each.
Continuation parameters and summaries record `SourceMutationMod` separately from
the destination `MutationMod`.

Only `MutationMod` can contain multiple execution values. Other execution overrides
must contain one value and can replace only a single-valued source setting (or add
a new setting, such as the required Standard-to-Soss settings). A source sweep must
be inherited. These rules also apply to settings inherited through earlier steps.
For `Msuf4a.Soss_condense.Pool_X`, both Step1 and Step2 select two candidates and
expand each into two mutations, producing four executions per partition count.

Each family also has `TestStandardAppendCondense` / `TestSossAppendCondense`
and `TestStandardCondenseAppend` / `TestSossCondenseAppend`:

- Append → Condense selects the best two of the three size-32 Append candidates.
- Condense → Append continues all resulting executions at size 32,
  preserving their mutation values and one pool per execution.

These chains import their immediate source's interval-1 checkpoint and finish at
interval 2. `SameFamilyChain` keeps Standard sources Standard and Soss sources Soss.
`MixedFamilyChain` adds `TestStandardAppendSossCondense`, `TestStandardCondenseSossAppend`,
`TestSossAppendStandardCondense`, and `TestSossCondenseStandardAppend`. These select
using the source family's scores, then convert the selected checkpoints during Setup.
Standard to Soss explicitly supplies `SeedSoss`, `PartitionCount`, `GenerationsPerSubset`,
and `GenerationsPerMergedSet` in execution spans. Soss to Standard supplies
`AdjustCeLengthFrequency` and evaluates converted members against the full test set
during Setup, so Standard parent selection never uses partial subset scores.
Converted Setup pool files have separate identities from unchanged copies; execution
needs only destination Setup data. The suite now has fourteen configurations and
52 executions for replica 0. The runner discovers
the dependency order automatically; verification discovers all continuations,
checks their immediate source members, sizes, and selections, and includes their
reports in the combined output. Configuration discovery does not require existing
results; on a fresh data folder, the script builds the sources before their chains.

From the repository root, run the complete OnePool batch with:

```powershell
.\scripts\Run-OnePool.ps1
```

The script builds its runner, prepares the `SortableTests.RunCfg.Prefix.Width32`
and `SorterEval.RunCfg.Prefix.Width32` library prerequisites, discovers all presets in `Test.OnePool` and its nested modules,
and completes setup, execution, and the evolution summary report for each preset.
Source presets finish before any dependent Append or Condense presets. It runs
replica 0, defaults to eight parallel jobs within each phase, respects each preset's
overwrite setting for setup and execution, preserves existing run parameters when
resuming, regenerates summary reports, and stops with a nonzero exit code on failure.
After all summary reports are generated, it runs `OnePoolAudit` to verify the
Append size/mutation combinations against report rows and final checkpoints,
and the Condense selections against source checkpoint scores and imported Setup
members. It also rejects summary rows with stale `Default` subset evaluations;
these evaluations are discarded when Standard seed pools are converted to Soss.
Verification failures also return a nonzero exit code. Expected pool
sizes, pool counts, and execution combinations come from the configured parameter
spans. Condense candidates and selection count come from its source spans, and
the source checkpoint generation is resolved from its source interval and the
source run's save schedule. Candidate scores are ranked independently against
the saved selections, so changing these configurations requires no audit edits.
Condense ranks source pools separately within every combination of source settings
(and replica), excluding mutation and provenance. The selection count applies to
each group; execution mutation variants then expand each selected source while
inheriting its settings. Scores and Soss selection subsets are compared only within
a group.
After verification, the script combines the current configurations' summary
reports into `Data/Results/Report/OnePool_SummaryReport.txt` under the OnePool
database. This tab-delimited file has one header with `RunName` first. Columns
shared by Standard and Soss are aligned by name; unavailable fields are blank.
The audit checks `MutationMod` against each execution and `SourceMutationMod`
against its immediate source (blank for base runs). It independently expands the
execution spans for every selected source, including inherited mutation values.
Before replacing the combined file, it reads it back and compares all rows,
including duplicates, with the verified individual reports. Missing, extra, or
changed rows fail verification.
The file is replaced on each successful run. Inputs are resolved from current run
parameters, so older reports and the combined file itself are never included.
Use `-MaxParallel 4` to change concurrency, `-ListOnly` to preview the order without
running phases or verification, or `-NoBuild` to reuse existing runner and audit builds. Use the default
`C:\Projects` root: the shared library readers currently use that location.
The initial library preparation evaluates four sets of 10,000 sorters and can take
several minutes; completed prerequisite runs are skipped on subsequent invocations.

Run presets capture all three names with one call:

```fsharp
open System.Runtime.CompilerServices

[<MethodImpl(MethodImplOptions.NoInlining)>]
let Test () : run =
    let names = RunNaming.create()
    // Use names.ProjectName, names.DatabaseName, and names.RunName below.
```

The configuration then uses `names.ProjectName`, `names.DatabaseName`, and
`names.RunName`. Keep the call directly inside the preset so it captures that
function, rather than a shared factory. The helper reads its immediate caller's
method. Mark each preset `NoInlining` so optimized builds retain that caller frame;
the helper also disables inlining. The naming tests check this requirement.

The project is the module hierarchy above the database module, with the common
`GeneSort.Dispatch.V1.` prefix removed. For example, `Standard.Pool_32.Test()`
has project `SorterSgd.RunCfg.p32.Msuf4a.Standard`, database `Pool_32`, and run `Test`.
Modules directly under a workflow's `RunCfg` namespace act as databases. For example,
`SorterEval.RunCfg.Standard.Test()` has project `SorterEval.RunCfg`, database
`Standard`, and run `Test`. The distinct sortable-test group is
`SortableTests.RunCfg.PrefixTest.Test()` (database `PrefixTest`).
Top-level presets deeper in the hierarchy, such as `Standard_append.Test()`, use their entire module hierarchy
as the project. Shared database readers obtain their project from a producing preset
so module renames update both writing and reading locations.

Run names follow the preset function's name, and database names follow its
containing module. The top-level `Standard_append.Test()` uses `Test` for both
its database and run name.

Presets are grouped by pool capacity or experiment for SGD, and by sorter family
for sortable tests, evaluation, and mutation. For example:

| Preset | Database | Run |
| --- | --- | --- |
| `Standard.Pool_32.Test()` | `Pool_32` | `Test` |
| `Standard.Pool_512.V2a()` | `Pool_512` | `V2a` |
| `Soss.Pool_4.Debug()` | `Pool_4` | `Debug` |
| `Soss_append.Pool_4_append.Debug()` | `Pool_4_append` | `Debug` |
| `Soss_condense.Pool_128_condense.Test()` | `Pool_128_condense` | `Test` |
| `SorterEval.RunCfg.Standard.Test()` | `Standard` | `Test` |
| `SorterMutate.RunCfg.Prefix.Mssi()` | `Prefix` | `Mssi` |

Seed pool groups describe capacity per pool, not the total pool-set size.
Continuation groups describe the source pool family and retain workflow suffixes
where needed to distinguish their identities from seed runs. Execution spans may
resize those pools. Shared database readers use the corresponding family names.

The two phases are independent:

Standalone SortableTests (`tests`) is an exception to input preparation: its setup executor is a no-op. The local `setup tests` command only initializes Project parameters. `execute tests` generates merge or prefix tests and saves them directly as Results; it needs no Setup artifacts. Evaluation, mutation, and SGD still prepare their test inputs in Setup.

Standalone test Results use only the source database, replica, prefix/merge library and data format for their identity, without setup links. If tests were generated with a setup-linked identity, regenerate them before evaluation or SGD setup: temporarily set `AllowOverwrite = true` in the selected standalone test spec, run `setup tests` to reset its finished parameters, then run `execute tests`. Restore `AllowOverwrite = false` afterward. In `Program.fs`, select each command through `yab` when using the configured local entry point.

- `ProjectOps.executeSetupRuns` resolves `host.SetupExecutor`, prepares Setup artifacts, and saves updated Project run parameters. It never invokes the execution function.
- `ProjectOps.executeRuns` resolves the execution function on the host and loads saved inputs. It never invokes setup or regenerates parameters. Completed runs are skipped; SGD resumes from Results checkpoints when available, otherwise from its initial Setup pool.

Each dispatch module exposes `prepareRun()` (initialize parameters, then setup), `setupRunParams()` (setup existing parameters), and `runRunParams()` (execute existing parameters). The combined `executeSetupAndRuns` and `makeParamsAndRun` entry points have been removed.

## Commands

Build the dispatch project, then prepare locally:

```powershell
$env:GENESORT_PROJECTS_ROOT = 'C:\Projects'
dotnet GeneSort.Dispatch.V1/bin/Debug/net10.0/GeneSort.Dispatch.V1.dll setup sgd
```

On the worker, point `GENESORT_PROJECTS_ROOT` at the transferred project tree and run:

```powershell
$env:GENESORT_PROJECTS_ROOT = 'D:\GeneSortJobs'
dotnet GeneSort.Dispatch.V1/bin/Debug/net10.0/GeneSort.Dispatch.V1.dll execute sgd
```

Supported workflow names are `sgd`, `eval`, `mutate`, and `tests`. These select the run spec, replica range, and parallelism currently configured in the corresponding dispatch module. They do not yet load an arbitrary saved run definition from a command-line path. Use the same configuration and application build on both machines. Run without valid arguments to print usage; it performs no batch work.

Exit codes: 0 for a successful batch (including skipped runs or no work), 1 for failure/cancellation, 2 for invalid arguments. There is no interactive console pause. Session logs go under `GENESORT_PROJECTS_ROOT/Logs`. If the variable is unset, the root defaults to `C:\Projects` on Windows or `./Projects` on other platforms. This override controls the run database; existing library/source database locations used during local setup retain their own configuration.

## Transfer layout

After checking that every setup item succeeded, copy both folders, preserving relative paths:

```text
<projects-root>/<project-name>/<database-name>/Data/
  Project/    # Run definition and prepared run parameters
  Setup/      # Materialized tests, prefixes, models, or initial pools
```

Setup alone is insufficient: execution discovers its work through the Project run parameter files. Source library databases used to prepare these artifacts need not be copied for execution. For SGD, local setup includes evaluating the initial pool, so preparation can involve substantial computation.

When moving an in-progress job, also copy its Results folder and current Project files. After execution, retain both Results and Project: Project records completion. Keep the original prepared bundle separately. Do not run `setup` on the cloud worker or regenerate parameters when resuming.

## Suggested next steps

- Add a bundle manifest containing the selected run, replica range, application revision, and hashes of Project/Setup files. Validate it before starting paid compute and reject incomplete transfers.
- Load the saved run definition directly when creating the cloud host, with command-line replica and parallelism options. This would remove dependence on the dispatch module's compiled spec.
- Assign disjoint replicas to workers and retain checkpoints on durable storage. Avoid concurrent writers for the same run parameters or result files.
- Keep Setup immutable after preparation. Upload a completed bundle before submitting execution jobs, and download Project plus Results afterward.

These commands separate phases but do not upload files or provision cloud resources.

## Continuing an evolution run

Select the append spec when creating the dispatch host:

```fsharp
let host = runHost.Create (Soss_append.Pool_4_append.Debug ()) 8
```

The module is `GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.Soss_append`.
It uses `EvoAppend.createSoss` with separate `Source.ParameterSpans` and
`ExecutionParameterSpans`. Source spans filter configured source executions;
omitted source keys select all their configured values. Execution spans form a
Cartesian product for each selected source. Unspecified execution parameters
inherit that source execution's settings.

The current debug preset selects three source mutation modifiers and branches
each into three modifiers and sorter counts 32 and 64: **18 runs per replica**.
The source's eight pools are preserved, with capacities of four and eight sorters per pool.
Its destination project is `SorterSgd.Soss_append.32p4a.Msuf`.

`Source.GenerationInterval` and `GenerationIntervalLast` are zero-based save-interval
indices, not generation numbers. Each branch carries one immediate
`SourceExecutionId`, resolved against persisted source parameters during setup.
The source parameter-generation configuration is stored with the run, so reloading
or chaining append specs preserves branch identities. Source selection requires
no JSON overrides or source-of-source parameter keys.

Setup reads the saved source run, execution parameters, Results checkpoint, and
Setup tests/prefix. Source and destination roots use `GENESORT_PROJECTS_ROOT`.
Source files are only read. Missing or invalid inputs fail setup without generating
replacement seeds. A checkpoint from the earlier evolution save convention may contain
generation N+1 under label N; setup preserves its actual generation. Other generation
mismatches are rejected.

Each imported Setup file is identified by `hash(destinationProjectName, sourceFileId)`.
It is stored under its replica folder (`0`, `1`, etc.) and shared across execution
branches. Destination parameters record `SourcePoolFileId`, `SourceTestsFileId`,
and their destination Setup references. Results identities use those references
plus execution settings. The imported generation is saved as `GenerationFirst`
and `GenerationCurrent`.

Imported Setup data stays unchanged. At execution startup, shrinking takes whole
pools in ascending ID order; growing cycles over source pools, giving additional
pools and members fresh identities. New members start at mutation index zero.
Changing the mutation modifier also gives affected pools/members new identities.
Pool positions and the one-dimensional lattice size are updated when the pool count changes.
Changing only the total preserves the source pool count and derives the per-pool capacity;
the new total must be positive and divisible by the source pool count. An explicit
per-pool capacity takes precedence. Source pools may be underfilled after pruning.

Results checkpoint resumes bypass resizing and modifier reassignment. Transfer the
destination Project and Setup folders after successful setup; cloud execution does
not need the source project. Retain Project and Results when resuming. Constructing
the spec alone does not copy source data.

### Standard append presets

`Standard_append` uses `EvoAppend.createStandard`; `Soss_append` uses
`EvoAppend.createSoss`; `Standard_condense` uses `EvoCondense.createStandard`.
Each run definition supplies `SetupQueryCatalogName` and `RunQueryCatalogName`
explicitly. Constructors do not infer query catalogs from the source. The two append
entry points reject ranked selection settings; condense owns selection-count validation
and validates the selected execution family. Shared continuation construction and validation live in
the internal `EvoContinuation` module.

`Soss_condense.Pool_128_condense.Test` uses `EvoCondense.createSoss`, explicitly specifying
the Soss seed setup and Soss append execution query catalogs. It ranks mutations
0 through 3 from `Soss.Pool_128.Test` at interval 0, retains the best two per replica,
and continues through interval 1. Each candidate must contain one nonempty pool
with all members evaluated for the same active selection subset. Candidates must
use matching selection subsets and evaluation measures. Lower average scores win;
source execution IDs break ties. Selection and copying finish during local preparation,
so execution needs only the destination Project and Setup files.

The chosen entry point validates the source execution family. Makers and the save
schedule are inherited from the source. Standard
checkpoints remain Standard pools; Soss checkpoints remain Soss pools.

```fsharp
let host = runHost.Create (Standard_append.Test ()) 8
```

`Pool_128_Test` imports `Standard.Pool_32.Test` at interval 0 and continues through
interval 1. It branches modifier 0 into modifiers 0, 1, and 2, each with total sorter
capacities 128 and 256: six runs per replica, preserving one pool per run.
`Pool_512_V1` and `Pool_512_V2a` import all their matching Standard source combinations
at interval 3 and continue through interval 4 without changing their settings.
Each preset uses its source database name within the separate
`SorterSgd.Standard_append.32p4a.Msuf` project.

The Standard source presets apply the Msuf setup defaults before building setup
queries. Their parameter spans occur once in setup and are inherited by execution,
so each source combination produces one run. They explicitly set
`AdjustCeLengthFrequency` to 10; appended runs inherit it unless overridden.
New Standard checkpoints use the produced generation as their file label and stop
exactly at the requested final generation. Setup can still read the earlier
N-labeled checkpoints containing generation N+1.
