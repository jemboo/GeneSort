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
scrolling provides access to every property. Sorting does not modify saved files.

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
