# Run editor

Architectural constraint: preserve [future single-machine deployment](../GeneSort.Dispatch.V1/CLOUD_WORKFLOW.md#single-machine-deployment-architectural-requirements).
This guides local development; cloud features are explicitly deferred.

Open the **Run Editor** tab in Sortopia. The selection wizard first asks for the
folder containing your projects. The last valid Projects folder is remembered
per user and prefilled in the wizard and
editor on startup. When that folder still exists, the wizard opens directly to
the top-level modules. **Change folder** remains available. An unavailable folder remains visible so you can reconnect
it or choose another folder; restoring it does not create unsaved run edits.
Each successive page shows the next module
layer as a wrapping set of icon tiles, with module names underneath. **Back**
returns one layer and **All modules** returns to the root.

At a module's leaves, select a run or **Create your own** and choose a starting
definition. Code-only leaves retain their original run names when selected;
new leaves start with an unused NewRun name. Tiles distinguish **Code only · not
saved**, **Code + saved on disk**, and **Saved on disk**. A saved counterpart
replaces the code leaf in place, and selection loads the saved values. Browsing
does not save files or execute runs. Use **Validate** and **Generate RunParameters** after selecting a leaf; generation saves the definition along with its parameters. Then use Setup and Processing.

**Back** reopens the wizard and refreshes disk state. **Return to editor**
keeps existing edits; selecting another leaf asks before discarding them.
**Change folder** starts folder selection again. **Refresh runs** reloads the
current folder without changing the hierarchy location. Unreadable definitions
are reported while healthy runs remain available.

New saves include a small `Run_*.msgpack.module.json` sidecar recording the
originating module, so a custom run stays in that module even if its project or
database name changes. Older files are associated by project/database identity
when unambiguous; otherwise they appear under **Saved runs**. Discovery reads
canonical definition folders only, avoiding large parameter and result trees.

Project names contain the full namespace and module hierarchy after `GeneSort.Dispatch.V1` (for example, `SortableTests.Prefix.Channels32`). Database names use the immediately containing module (`Channels32` in this example). Existing saved files are not relocated.

The starting definition supplies parameter fields, defaults, catalog selections,
evolution makers and schedules. SortableTests runs have one Processing
parameter sweep and no Setup section or action. Evaluation and mutation runs
prepare their input files before execution. Evolution runs create a starting
population; continuation runs import one from their source.

For Merge and Prefix SorterEval runs, Setup includes **SortableTestsModule**, the producing module name (for example, `SortableTests.Prefix.Channels32`). Defaults are declared as `sortableTestsModule` module values. The module supplies its project, database, and execution query catalog; setup parameters supply the query inputs, including replica. The module name participates in Setup query identities but not processing query properties. Standard evaluation generates its test inputs locally. Each run module declares one execution query catalog. Formerly mixed SGD modules are split into Standard, Soss, StandardAppend, and SossAppend children as applicable. Their project and database identities follow the new hierarchy; existing saved output files are not relocated.

For fresh SGD runs, Setup exposes **SortableTestsModule** and **SorterEvalModule**.
Each names a producing run module whose metadata supplies the project, database,
and execution query catalog. Source defaults are module-level values in the preset
code. Both names are carried in setup parameters and participate in dependent
setup identities, but are excluded from processing query properties. Evaluation
inputs are selected from saved producer definitions so their prepared-input references
remain intact. Unknown modules and ambiguous matching evaluations fail explicitly.
Continuations retain the module selections and import their saved source state.
Saved definitions using the old source-database fields must be recreated with module names.

The domain `run` DU distinguishes `DirectRun`, `PreparedRun`, `EvolutionRun`, and
`ContinuationRun`. `RunParameterRoles.fs` defines the explicit policies for each
workflow: `SetupOnly`, `InheritedReadOnly`, `InheritedOverridable`, and
`ExecutionOnly`. Processing displays fixed Setup values as read-only, including
resolved enhancer defaults. These display rows are not additional sweep dimensions.
The domain also rejects overrides of fixed values, so the same rule applies
outside WPF. Continuations explicitly permit mutation branching and pool resizing.

For Soss.SossS, ActivationProbability, the model/library settings and seed pool
dimensions are fixed. ActivatorMutationRate remains editable: rates 0.0 and 0.01
share the same recorded Setup files for each MutationMod and retain distinct
execution/result IDs.

SortableTests and prepared-run identities retain their current behavior. On import,
legacy SortableTests Setup spans are folded into the one Processing sweep, and
exactly repeated fixed spans in prepared runs are inherited instead of expanded
again. Existing files are neither rewritten nor renamed. The historical
SetupParametersId remains in direct-run bookkeeping queries solely to preserve
saved addresses; it does not represent a Setup action. Previously saved rows
are loaded as saved, including rows from the redundant expansion.

Each defining module owns a `runType` value listing its example constructors;
`RunTypeCatalog.all` explicitly registers these modules. The constructors use
the module-level selection variables, so the editor has no duplicate catalog
settings to maintain. Modules with multiple families keep their distinct examples.
Continuation examples retain their source configuration and selection; generation
reads source checkpoints from the selected projects folder when ranking is needed.
The editor validates source identity fields but does not provide a source picker.

Opening a saved definition preserves its parameter values and continuation source,
normalizing redundant spans as described above. The editor chooses a compatible catalog entry for display,
preferring the recorded module sidecar, then the original project/database.
The MessagePack definition itself does not store a module identifier, so older
renamed definitions without sidecars may match several compatible modules.

1. Create a new definition or open an existing `Run_*.msgpack` file.
2. Code-defined and saved runs are read-only. Choose **Clone** to edit the new
   run's name and its declared Setup/Execution parameter values before saving.
   Each module declares one fixed `channelCount`, displayed above the editor.
   Mixed-channel workflows have `ChannelsN` child modules. Select the appropriate
   module in the wizard rather than changing a run's channel count.
   `ChannelCount`, when needed internally by parameter generation, remains a
   read-only singleton. Prefix, merge and sorter library dropdowns use the module
   count; model-type choices respect generator channel-count constraints.
   Validation rejects mismatched IDs, mutator counts and channel-count sweeps.
   Clones and saved definitions retain their source module's count.
   Long identities
   wrap at narrower window widths. Dropdowns exclude values selected elsewhere in
   the same parameter, and **+ value** chooses an unused option. Duplicate typed
   values, including equivalent numeric spellings, are highlighted and cannot be
   saved or generated. Imported duplicates remain visible for correction.
3. Choose the folder containing your projects. **Validate** reports problems by
   field, including missing setup values, invalid spans and incompatible pool sizes.
   **Save** stays disabled until an existing projects folder is selected and the
   current definition passes validation. Editing the definition requires validation
   again; changing only the destination folder does not invalidate its parameters.
4. **Save** writes the existing MessagePack run format to
   `<projects>/<project>/<database>/Data/Project/Run/<run>/Run_<run>.msgpack`.
   Changing the names or projects folder saves a separate definition. Replacing
   an existing definition asks for confirmation, independently of the run's
   `AllowOverwrite` setting.

Saving does not generate run parameters, prepare sources, or execute anything.
To generate parameter files, open **Generate parameters**, choose **First replica**
and **Replica count**, and review the file count and destination. For example,
first replica 2 and count 3 generate replicas 2, 3 and 4. Click **Generate RunParameters**
to save the current definition together with the generated set. Run Setup for
workflows that require prepared inputs, then Processing. SortableTests goes directly
to Processing. Replica controls describe this generation operation;
they are not stored as run parameters.

Generation previews do not write files. Existing parameter files or a different
saved definition require replacement confirmation, independently of the workflow's
`AllowOverwrite` setting. Replacement publishes the complete set for this run,
removing previous combinations and replicas outside the selected range. Setup and
execution outputs and other runs are preserved. Once parameters exist, edited
definitions must be saved through generation or under a new run name.

Publication stages the definition and parameters before changing the destination,
coordinates concurrent saves/generations, and restores the previous files if
publication fails. Dispatch uses the same Project preparation service and FileDb
publisher. OnePool uses its explicit resume mode to retain saved checkpoint state
and fill missing files; changed definitions or unexpected files require replacement.

Evolution resumes from the highest saved checkpoint through `GenerationIntervalLast`,
the zero-based index in the save schedule. An explicit schedule `MaxCount` still
limits finite schedules; there is no default 200-entry search cutoff. FileDb lists
checkpoint IDs in the output/replica folder once, matches the scheduled queries in
memory, and deserializes only the latest matching file. Missing earlier checkpoints
and stale saved progress are supported. Existing file names and formats are unchanged.

Processing has separate **Setup parameters** and **Processing parameters** tabs.
The Setup tab has one row per shared set of recorded input files, with the input
parameters, file IDs and a `LinkedRuns` count. Processing-only sweep values do not
split these groups: Soss.SossS rates 0.0 and 0.01 share a Setup row when they use
the same inputs. Select Setup rows and click **Run Setup** to prepare their inputs
and update all linked saved run rows. Shared artifacts are reused by the existing
setup executors. Progress counts linked run rows. Continuation requests whose
input IDs are not yet known stay separate until Setup records their imported files.
SortableTests hides this tab because it has no Setup phase.

The **Processing parameters** tab displays the saved parameter files for the current run,
including every saved replica regardless of the generation range controls. Opening
a definition or generating parameters loads this view automatically; **Refresh saved
files** reloads saved values, including execution progress. Unsaved definition edits
are not displayed as generated rows. A failed read reports the offending file and
clears the list rather than showing incomplete results.

The reusable `RunParameterListView` uses a virtualized WPF `ListView`/`GridView`
(an `ItemsControl`) with one row per RunParameters file and a column for every
property. Click a column heading to sort ascending, then again to reverse. Numeric
columns sort numerically, including large seed values; text sorts alphabetically.
Missing values stay at the end. Columns can be resized and reordered, and horizontal
scrolling provides access to every property. Column order, widths and sorting survive
refreshes, including empty results and failed reads. Temporarily absent columns recover
their layout and sorting when they return; new columns are appended. Sorting does
not modify saved files.

The Setup, Processing parameters and Summary Reports tables save their column order, widths and sorting when you close the application,
and restore them the next time you open it. Each table has its own settings, shared
across runs for your Windows user, in `%LOCALAPPDATA%\GeneSort\table-layouts.json`.
Settings include the `#` column and columns that are temporarily absent. Missing or
unreadable settings use the default layout. A failed settings save is reported when
closing, and leaves the previous settings file intact.

Use **Columns…** in either table to check the columns you want to show, including
the `#` column. The chooser scrolls when there are many fields and stays open while
you make multiple choices. Visibility changes save immediately and survive refreshes,
selection changes and application restarts. Hidden columns retain their widths and
positions, and hiding the sort column keeps the existing row order. Columns absent
from the current files keep their visibility settings until they return.

Each table's **Reset layout** button shows all columns and restores its default
order and widths, including `#`, and ascending sorting by its first data column. The reset clears
remembered layouts for absent columns too, and saves immediately. It works with an
empty table, whose default sort is applied when rows load. The other table keeps
its layout. A message below the table confirms the save or reports a save failure.

Select rows with Ctrl+click or Shift+click. Use **Run Setup** on the Setup tab;
use **Run Processing** or **Generate Summary Reports** on the Processing parameters tab.
The tabs have independent selections, so selecting shared inputs does not select
executions. These actions require a saved definition and at least one selected row.
Each action processes the affected saved run rows one at a time on a background
thread. A progress bar shows the number processed,
and the status shows the current activity. The final counts distinguish successes,
failures and rows skipped because execution has already finished; individual failures
appear below the editor. Failed rows do not stop the remaining selection. The saved
parameter list refreshes afterward and clears the selection.

Setup reuses existing shared inputs and records its results in the selected parameter
files. It does not start execution. Processing reads the prepared inputs, resumes saved
checkpoints through the workflow executor, and saves the resulting generation and
completion state. Missing prepared inputs are reported as failures; use **Run Setup**
to prepare them before retrying Processing. Other rows in the selection still run.

**Select Completed** in Processing parameters replaces the current selection with
all completed runs in the table, regardless of sorting or scrolling. It is disabled
while work is running.

**Generate Summary Reports** uses the existing report catalog to summarize saved
generation slices for completed runs only. Select at least one row; every selected
run must have completed. Both the editor and saved-row runner enforce this rule
before any reports are written. It writes one tab-separated SummaryReport
per selected row under the database's `Data/Results/Report` folder, with the
usual `RunName` column. Missing or empty results are reported per row. Reports honor
**Allow workflow output overwrite** and leave every RunParameter file and its saved
execution state unchanged. Reporting does not require the original setup libraries.

The **Summary Reports** tab automatically loads reports for the rows selected in
**Processing parameters**. It combines their saved rows into one virtualized, read-only
table. Each selection uses its latest saved report within the configured generation
range, even if its execution checkpoint has since advanced. Older cumulative reports
are not added again. **Refresh reports** reloads the current selection from disk.

Click any column heading to sort or reverse its order. Numbers sort numerically
(including large seeds), text alphabetically, and missing values and `NaN` stay
last. Reports with different headers share a union of columns, with blank cells
where a column is absent. Column order, widths and sorting are remembered across
application restarts, refreshes and selection changes. Temporarily absent
columns recover their layout when they return; new columns are appended. If the
sort column is absent, rows remain in report order until it returns or you choose
another sort. Each row's tooltip shows its source file. Missing or malformed reports are listed below the table,
while other selected reports remain visible. Changing or clearing the selection
discards earlier loads so stale results cannot replace the current view. Viewing,
refreshing and sorting reports do not generate reports or change any saved files.

Use **Save combined report…** to export the displayed table as one UTF-8,
tab-separated `.tsv` or `.txt` file. The export contains only visible columns, has
one header row, and preserves column order (including `#` when visible), sorting and blank
cells. It includes all displayed report rows, including those outside the visible
viewport. Missing reports contribute no rows. The button is disabled while loading,
when the table is empty, when every column is hidden, or while a save is in progress.
The save dialog lets you choose the destination; a themed confirmation asks before replacing an existing file. Saving takes a
snapshot, so later selection, sorting or column changes cannot alter that export.
The complete file is written before replacing the destination, and a status message
reports success or failure.

Before any action starts, it checks that the saved definition and selected rows
still match what was displayed. The shared phase runner holds the publication lock
so another editor phase, save or regeneration cannot change this run at the same time.
Editing and other editor operations are disabled while it runs.

Error messages have a **Copy errors** button that copies every displayed error,
including full file paths. Errors, status and phase messages are selectable read-only
text: use Ctrl+C or the context menu to copy a selection.

Source libraries are read from the selected projects folder. Prefix and Merge tests
are loaded from `SortableTests/<database>/Data/Results` using the library ID, data
format and replica. Their run name may be anything, including `NewRun`; no source
definition is required to load these results. Missing tests report the requested
values and exact result path. Generate and execute a matching SortableTests run,
then rerun the evaluation's Setup to create `SortableTestsWithPrefix` before Processing.
Evolution workflows use the selected evaluation database; its saved run definitions
supply library sizes and other query settings. The loader accepts any run name and
requires one distinct matching evaluation. If several match, use a database containing
the intended evaluation rather than relying on an arbitrary selection.
This binding lives in
`GeneSort.FileDb.V1/WorkflowSourceFiles.fs`, alongside the selected-row setup adapter
`RunSetupFiles.fs`. `RunExecutionFiles.fs` binds the shared Project execution catalog
without accessing source libraries. `RunSummaryFiles.fs` binds the Summary Report
catalog entry. All three use `RunPhaseFiles.fs` for selection checks, progress and
locking; only Setup and Processing persist returned parameters.

**Add parameter** and **Add override** open an embedded panel. Choose an available
parameter, enter its values, then **Add** or **Cancel**. Parameters already present
in that section are excluded. Adding an override starts from the inherited setup
values and displays them alongside the editable override. Only parameters allowed
in execution appear in that picker. Numeric bounds and choices come from Project's
shared parameter definitions; invalid or duplicate values prevent adding the row.
Use **Remove** to change which parameter a row represents. Removing a required
parameter is allowed while editing, but validation prevents saving until it is
restored. Finish or cancel an Add panel before saving the run.

Unsupported workflows and continuations are rejected when opened; they are not
converted to Standard. Unknown parameter keys in a supported run remain visible
and must be removed before saving. Invalid saved values stay visible for correction.

The editor API lives in `GeneSort.Project.V1/Editing`. Its supported-key and
editing restrictions remain UI policies. `GeneSort.Project.V1/Workflows` defines
the catalog combination used to display, construct and recognize a run.
`Parameters/ParameterValidation.fs` and `SorterSgd/SgdParameterValidation.fs`
provide the value rules and workflow requirements shared with parameter
generation, setup and execution. Code run definitions remain complete and inline.
File persistence lives in `GeneSort.FileDb.V1/RunDefinitionFiles.fs` and reuses
`RunDto` and `OutputDataFile` naming. WPF references these projects without a
dependency on Dispatch. This keeps the reusable control independent of dispatch
hosts and leaves additional workflow profiles for a later step.

Run the offscreen WPF smoke checks on Windows with:

```powershell
dotnet run --project scripts/RunEditorSmoke/RunEditorSmoke.csproj
```

Pass an output folder after `--` to render each editor tab and dark-theme previews
(workspace, browser, selected report rows, dropdown, column chooser, text menus,
tooltips, progress controls and message dialogs).
The shared palette and control styles live in `Themes/Dark.xaml`; views use those
resources for surfaces, text, focus, selection and validation colors.
App confirmations and warnings use `Views/MessageDialog` with the same dark title
bar. Messages are selectable, and long messages scroll. Enter invokes the first
button; Escape or closing the dialog declines a confirmation. Windows file and
folder pickers continue to use the system appearance.
Text fields share a dark editing menu with standard Undo, Redo, Cut, Copy, Paste
and Select All commands. Commands target the field that opened the menu, with
editing commands disabled for read-only paths and message details.
Project validation tests are in `RunEditorTests.fs`; definition persistence tests
are in `RunDefinitionFileTests.fs`. `RunParameterFileTests.fs` covers generation,
replacement, rollback, concurrent publication and checkpoint-preserving resume.
`RunPhaseFileTests.fs` covers selected setup, execution and summary reporting, finished rows, per-row
errors, progress, stale selections, publication conflicts, saved execution state,
SGD bindings, real setup reuse and source-folder resolution. Summary checks verify
saved report contents, overwrite policy, selected rows only, and unchanged parameter files.
`RunSummaryFileTests.fs` covers report discovery, replicas, generation horizons,
malformed files and cancellation. The WPF smoke checks cover the report viewer's
column alignment, sorting, selection changes, refresh and virtualization. Export
checks cover reordered columns, both sort directions, all virtualized rows,
snapshot consistency, overwrite refusal and temporary-file cleanup.

Setup rows include a saved completion status: green Complete, amber Partial,
and brown Not run. Older parameter files without a completion record show Not
recorded. A grouped setup is complete when all linked rows have completed Setup.
The Parallel threads field defaults to 1 and limits concurrent selected rows in
Setup and Processing; shared setup artifacts remain protected against duplicate
creation. Positive whole numbers are required, and the control is disabled while
work is running.

During SGD Processing, GenerationCurrent updates from coarse generation messages
for each selected row. These are display-only updates; saved parameters continue
to describe resumable state. The Cancel footer remains available while the editor
is busy. Cancellation stops queued rows and lets active work reach a cooperative
stop point (between SGD generations, after any checkpoint writes). Cancelled SGD
rows are restored to their highest saved pool checkpoint, or their initial Setup
generation when no Results checkpoint exists. That generation is saved to the
parameter file with RunFinished=false, so the same selection can resume. Completed
rows remain completed, and cancellations are counted separately from failures.

Processing parameters show a Status column: Waiting (gray), Running (blue),
Completed (green), and Failed (red). A colored left stripe
remains visible on selected rows. Status updates target the original selected row
regardless of table order or parallel completion order. Setup and summary report
operations do not change Processing status. Saved finished runs open as Completed;
failed runs restore their Failed status from a .processing-failures.json sidecar beside the
parameter folder. This history does not modify parameter values. Other unfinished
runs open as Waiting. Cancelled rows return to Waiting; a retry replaces the
previous failure status. Replacing generated parameters clears the old failure history.

During execution, parameter tables and the outer editor remain scrollable. Tabs,
row selection, sorting, and column layout controls stay available for inspection;
the active job uses its captured selection. Run navigation, metadata, parameter
editing, parallel-thread settings, and actions that start or reload work remain
locked until the operation finishes.

Generated Setup and Processing values are read-only text cells. To change input
values, edit the run configuration and regenerate parameters. Runtime progress
and status updates do not provide an editing path for generated inputs.

Click the underlined Failed status in Processing parameters to view that row's
error in a scrollable, selectable dialog, with run name, replica, and ID. The
failure history stores each message by run-parameter ID, so it survives reopening
and remains associated with the correct row after sorting or parallel execution.
Older failure files that stored only IDs remain readable and explicitly indicate
that no error message was recorded.

### Checkpoints and snapshots

The Save intervals tab has separate Checkpoint, Snapshot, and Summary sampling
schedules. Checkpoints provide recovery; only the latest valid recovery group per
processing row is retained. Snapshots preserve evolutionary history permanently.
When schedules coincide, one snapshot serves both purposes. GenerationIntervalLast
still indexes the Snapshot schedule; the endpoint is always a snapshot. Old run
files retain their existing save schedule as snapshots. Cloud deployment remains
deferred. See [checkpoint retention](../GeneSort.Project.V1/EvoPath.md).

### Module descriptions and return navigation

Each registered run module declares a `description` alongside its `runType`.
The wizard shows it on the module tile and above the module's run definitions;
the editor shows the module description above the run fields. The editor's Back
button returns to the containing module in the current projects folder and keeps
unsaved editor changes. Return to editor restores the draft without reopening it.

Module tiles use stacked frames around the module-type image to suggest a
container. Run tiles include their descriptions and omit visible status captions.
A run is visually materialized (solid badge and filled tile background) only when it has
saved processing rows and every row has RunFinished=true. Saved-only, partial,
cancelled, failed, and unreadable processing states remain light. Creating a new
run from a completed template also remains light. Completion is refreshed from
parameter files when the wizard loads or refreshes; Results are not scanned.

### Channel terminology

`ChannelCount` is the number of sorting-network channels and is used consistently
in parameter keys, library identities, DTOs, and the editor. Comparator endpoints
are `LowChannel` and `HighChannel` (`int<channelIndex>`); `ChannelSpan` measures
the distance between them. Comparator counts and stage counts remain separate.
This rename intentionally changes stored names without legacy aliases or migration.


### Cloning a run

Open a code-defined or saved run and choose **Clone**. The definition is copied into an unsaved draft named `<source>_clone`; the name remains editable. The clone stays in the source module and project/database. Saving records that module association so the clone appears beside its source in the wizard.

Only the run name and values of parameters declared in the source's Setup and Execution spans are editable. Phase membership remains fixed, and inherited/continuation-source restrictions still apply. Project/database identity, description, source modules, overwrite policy, schedules, functions, catalogs, and makers belong to the module. They remain read-only when opening module runs and clones. Validation enforces these restrictions as well as the controls. Projects folder and execution controls remain operational settings.

Cloning copies no generated rows, statuses, checkpoints, snapshots, or results. Saving a clone does not save or overwrite its source. An existing destination cannot be replaced from the editor; choose a different name for the clone.


Code-defined and saved definitions are immutable in the editor, including their names and parameter spans. Clone to make an editable draft; saving or generating its first parameters makes it read-only. Replica range and parallel-thread controls remain available. Generating replicas adds missing files and preserves existing replicas (including progress and status), even outside the requested range. It refuses a different definition under an existing run name.


### Channel module storage

Channel-specific children of the SortableTests and SorterEval workflow modules
share their parent's project/database library location. This keeps setup readers
and channel-keyed library queries consistent. Presets split from a multi-channel
sweep have unique names such as `Small_16`; each calls `RunNaming.create` itself.
The `.module.json` label records the full channel-specific module for saved clones.
When a label is missing, matching also checks channel count. Every run-bearing
module exposes an explicit positive `channelCount` and registers it in `runType`.

### Evolution module settings

SGD modules declare model type, prefix stage/variant, evaluation measures, RNG type,
and data format in `moduleParameters`. These values appear in Setup and remain
read-only when cloning; validation rejects edits and removal. Continuations inherit
them from their source. Existing evaluation/model/prefix comparison workflows retain
their explicit sweeps. The Msuf6 initial-measure value `SameAsEvaluation` resolves to
each Setup row's evaluation measure without adding another sweep dimension.

`ExcludeSelfCe` and `CollectNewSortableTests` (`CollectSortableTests` in saved spans)
are also fixed module policies. SimpleR's `AMax` and SimpleS's `ActivationProbability`
are explicit Setup run parameters, editable in clones. Their module defaults are 3
and 0.5 respectively; existing sweeps are preserved. Missing activation settings
are validation errors rather than requests for enhancer defaults.

### Live phase status

The parameter table displays `RunStatus` and `SetupStatus` instead of the legacy
boolean completion fields. Pending rows have not completed, Running rows are
active, and Completed rows have finished. Failures retain their error display;
cancelled attempts return to Pending and can resume. Shared setup rows update
live and show Completed only when every linked row has completed setup.

An activity bar stays visible during Setup and Processing with an animated
indicator and an enabled Cancel button. After cancellation is requested the
button disables while active work reaches a safe stopping point. Saved completion
flags remain compatible with existing files; Running is transient UI state.
