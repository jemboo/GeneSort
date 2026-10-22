namespace GeneSort.Dispatch.V1

open System.Reflection
open FSharp.UMX
open GeneSort.Project.V1

type runNames =
    private {
        _projectName: string<projectName>
        _databaseName: string<databaseName>
        _runName: string<runName>
    }
    member this.ProjectName = this._projectName
    member this.DatabaseName = this._databaseName
    member this.RunName = this._runName

module RunNaming =
    let private fromMethod (method: MethodBase) : runNames =
        let container = method.DeclaringType
        // A top-level preset (such as Standard_append.Test) uses its function
        // name as the database, leaving its entire module as the project.
        let projectModule =
            if isNull container.DeclaringType then container else container.DeclaringType
        let prefix = "GeneSort.Dispatch.V1."
        let hierarchy = projectModule.FullName.Replace('+', '.')
        if not (hierarchy.StartsWith(prefix, System.StringComparison.Ordinal)) then
            invalidArg (nameof method) "Run presets must belong to GeneSort.Dispatch.V1."
        let name =
            if method.IsSpecialName && method.Name.StartsWith("get_", System.StringComparison.Ordinal) then
                method.Name.Substring(4)
            else method.Name
        {
            _projectName = hierarchy.Substring(prefix.Length) |> UMX.tag<projectName>
            _databaseName = (if isNull container.DeclaringType then name else container.Name) |> UMX.tag<databaseName>
            _runName = name |> UMX.tag<runName>
        }

    /// Call directly in a preset marked NoInlining so its frame is retained
    /// in optimized builds. This helper must also retain its own frame.
    [<System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)>]
    let create () : runNames =
        fromMethod (System.Diagnostics.StackFrame(1, false).GetMethod())
