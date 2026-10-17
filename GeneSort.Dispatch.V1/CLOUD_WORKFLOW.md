# Local setup and cloud execution

The two phases are independent:

Standalone SortableTests (`tests`) is an exception to input preparation: its setup executor is a no-op. The local `setup tests` command only initializes Project parameters. `execute tests` generates merge or prefix tests and saves them directly as Results; it needs no Setup artifacts. Evaluation, mutation, and SGD still prepare their test inputs in Setup.

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
