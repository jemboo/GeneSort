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
        // Channel-specific modules share the workflow's library database. Their
        // run names are unique across these sibling modules.
        let declared = method.DeclaringType
        let container =
            if not (isNull declared.DeclaringType) && declared.Name.StartsWith("Channels", System.StringComparison.Ordinal) then
                declared.DeclaringType
            else declared
        // Direct workflow modules are databases in that workflow's project.
        // Deeper configurations retain their enclosing module as the project.
        let isWorkflowDatabase =
            isNull container.DeclaringType
            && List.contains container.Namespace [
                "GeneSort.Dispatch.V1.SortableTests"
                "GeneSort.Dispatch.V1.SorterEval"
                "GeneSort.Dispatch.V1.SorterMutate"
                "GeneSort.Dispatch.V1.SorterSgd" ]
        let hierarchy =
            if isWorkflowDatabase then container.Namespace
            elif isNull container.DeclaringType then container.FullName
            else container.DeclaringType.FullName
        let prefix = "GeneSort.Dispatch.V1."
        let hierarchy = hierarchy.Replace('+', '.')
        if not (hierarchy.StartsWith(prefix, System.StringComparison.Ordinal)) then
            invalidArg (nameof method) "Run presets must belong to GeneSort.Dispatch.V1."
        let name =
            if method.IsSpecialName && method.Name.StartsWith("get_", System.StringComparison.Ordinal) then
                method.Name.Substring(4)
            else method.Name
        {
            _projectName = hierarchy.Substring(prefix.Length) |> UMX.tag<projectName>
            _databaseName = (if isNull container.DeclaringType && not isWorkflowDatabase then name else container.Name) |> UMX.tag<databaseName>
            _runName = name |> UMX.tag<runName>
        }

    /// Call directly in a preset marked NoInlining so its frame is retained
    /// in optimized builds. This helper must also retain its own frame.
    [<System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)>]
    let create () : runNames =
        fromMethod (System.Diagnostics.StackFrame(1, false).GetMethod())
