# Run editor

Open the **Run Editor** tab in GeneSort.WPF. The first supported workflow is
Standard SGD with Simple Msuf32p4a models and the prefix makers.

1. Create a new definition or open an existing `Run_*.msgpack` file.
2. Edit the project, database and run names, description, parameter spans and
   save intervals. Parameter names are fixed once added. Edit values in their
   numeric, choice or text controls, using **+ value** for a sweep. Each field
   holds one value; commas and quotes in text labels are literal characters.
   The catalog tab describes the compatible entries supplied by this workflow.
   Code presets retain their existing automatic naming.
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
to save the current definition together with the generated set. Setup and execution
remain separate operations. Replica controls describe this generation operation;
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

The **Run parameters** tab displays the saved parameter files for the current run,
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

Both tables save their column order, widths and sorting when you close the application,
and restore them the next time you open it. Each table has its own settings, shared
across runs for your Windows user, in `%LOCALAPPDATA%\GeneSort\table-layouts.json`.
Settings include the `#` column and columns that are temporarily absent. Missing or
unreadable settings use the default layout. A failed settings save is reported when
closing, and leaves the previous settings file intact.

Select rows with Ctrl+click or Shift+click and choose **Run Setup**, **Run Execution**,
or **Generate Summary Reports**. All three require a saved definition and at least
one selected row. Each action processes those rows one at a time on a background
thread. A progress bar shows the number processed,
and the status shows the current activity. The final counts distinguish successes,
failures and rows skipped because execution has already finished; individual failures
appear below the editor. Failed rows do not stop the remaining selection. The saved
parameter list refreshes afterward and clears the selection.

Setup reuses existing shared inputs and records its results in the selected parameter
files. It does not start execution. Execution reads the prepared inputs, resumes saved
checkpoints through the workflow executor, and saves the resulting generation and
completion state. Missing prepared inputs are reported as failures; use **Run Setup**
to prepare them before retrying Execution. Other rows in the selection still run.

**Generate Summary Reports** uses the existing report catalog to summarize saved
generation slices, including finished runs. It writes one tab-separated SummaryReport
per selected row under the database's `Data/Results/Report` folder, with the
usual `RunName` column. Missing or empty results are reported per row. Reports honor
**Allow workflow output overwrite** and leave every RunParameter file and its saved
execution state unchanged. Reporting does not require the original setup libraries.

The **Summary Reports** tab automatically loads reports for the rows selected in
**Run parameters**. It combines their saved rows into one virtualized, read-only
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
tab-separated `.tsv` or `.txt` file. The export has one header row and preserves
the current column order (including the displayed `#` column), sorting and blank
cells. It includes all displayed report rows, including those outside the visible
viewport. Missing reports contribute no rows. The button is disabled while loading,
when the table is empty, or while a save is in progress. The save dialog lets you
choose the destination and confirms replacing an existing file. Saving takes a
snapshot, so later selection, sorting or column changes cannot alter that export.
The complete file is written before replacing the destination, and a status message
reports success or failure.

Before any action starts, it checks that the saved definition and selected rows
still match what was displayed. The shared phase runner holds the publication lock
so another editor phase, save or regeneration cannot change this run at the same time.
Editing and other editor operations are disabled while it runs.

Source libraries are read from the selected projects folder. For the supported prefix
workflow these are `SortableTests.RunCfg/Prefix/Width32` and
`SorterEval.RunCfg/Prefix/Width32`. Their saved definitions determine the queries and
library settings; both the definitions and corresponding Results must exist. Missing
inputs are reported with their source location. This binding lives in
`GeneSort.FileDb.V1/WorkflowSourceFiles.fs`, alongside the selected-row setup adapter
`RunSetupFiles.fs`. `RunExecutionFiles.fs` binds the shared Project execution catalog
without accessing source libraries. `RunSummaryFiles.fs` binds the Summary Report
catalog entry. All three use `RunPhaseFiles.fs` for selection checks, progress and
locking; only Setup and Execution persist returned parameters.

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

Pass an output folder after `--` to render each editor tab for visual inspection.
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
