# Run editor

Open the **Run Editor** tab in GeneSort.WPF. The first supported workflow is
Standard SGD with Simple Msuf32p4a models and the prefix makers.

1. Create a new definition or open an existing `Run_*.msgpack` file.
2. Edit the project, database and run names, description, parameter spans and
   save intervals. Enter comma-separated parameter values; quote text containing
   commas, and double any quotes inside a quoted value. The catalog tab describes the compatible entries supplied by
   this workflow. Code presets retain their existing automatic naming.
3. Choose the folder containing your projects. **Validate** reports problems by
   field, including missing setup values, invalid spans and incompatible pool sizes.
4. **Save** writes the existing MessagePack run format to
   `<projects>/<project>/<database>/Data/Project/Run/<run>/Run_<run>.msgpack`.
   Changing the names or projects folder saves a separate definition. Replacing
   an existing definition asks for confirmation, independently of the run's
   `AllowOverwrite` setting.

Saving does not generate run parameters, prepare sources, or execute anything.
Unsupported workflows and continuations are rejected when opened; they are not
converted to Standard. Unknown parameter keys in a supported run remain visible
and must be corrected before saving.

The editor API and workflow descriptions live in `GeneSort.Project.V1/Editing`.
File persistence lives in `GeneSort.FileDb.V1/RunDefinitionFiles.fs` and reuses
`RunDto` and `OutputDataFile` naming. WPF references these projects without a
dependency on Dispatch. This keeps the reusable control independent of dispatch
hosts and leaves additional workflow profiles for a later step.

Run the offscreen WPF smoke checks on Windows with:

```powershell
dotnet run --project scripts/RunEditorSmoke/RunEditorSmoke.csproj
```

Pass an output folder after `--` to render each editor tab for visual inspection.
Project validation tests are in `RunEditorTests.fs`; file round-trip, overwrite
and concurrent-save tests are in `RunDefinitionFileTests.fs`.
