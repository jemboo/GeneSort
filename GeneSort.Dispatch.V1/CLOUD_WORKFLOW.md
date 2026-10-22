# Local setup and cloud execution

Run configuration modules live under `RunCfg` in each workflow's folder and
namespace: `SorterSgd.RunCfg`, `SorterEval.RunCfg`, `SorterMutate.RunCfg`, and
`SortableTests.RunCfg`. Execution code and shared database readers stay in their
workflow folders. Project names derived from these modules include `RunCfg`.

`SorterSgd.RunCfg.p32.Test.OnePool` provides `TestStandard` and `TestSoss` base
runs with one pool of 32 sorters for each mutation value 0–3. Run the desired base through
setup and execution before using its interval-0 checkpoint in `TestStandardAppend`,
`TestSossAppend`, `TestStandardCondense`, or `TestSossCondense`.
Append tests use source mutation 0 and branch into mutation values 0–2 at capacities
32 and 64, preserving one pool in each of six runs. Condense tests rank all four
source mutation values and select the best two per replica. Both continuation workflows finish at interval 1, using the base's
save schedule; selection and copying happen before execution as usual.

Each family also has `TestStandardAppendCondense` / `TestSossAppendCondense`
and `TestStandardCondenseAppend` / `TestSossCondenseAppend`:

- Append → Condense selects the best two of the three size-64 Append candidates.
- Condense → Append branches both selected winners into sizes 64 and 128,
  preserving their mutation values and one pool per execution.

These chains import their immediate source's interval-1 checkpoint and finish at
interval 2. Standard sources stay Standard and Soss sources stay Soss. The suite
now has ten configurations and 36 executions for replica 0. The runner discovers
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
After verification, the script combines the current configurations' summary
reports into `Data/Results/Report/OnePool_SummaryReport.txt` under the OnePool
database. This tab-delimited file has one header with `RunName` first. Columns
shared by Standard and Soss are aligned by name; unavailable fields are blank.
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
