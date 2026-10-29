namespace GeneSort.Dispatch.V1

open GeneSort.Project.V1
open GeneSort.Project.V1.Runtime
open FSharp.UMX

/// A code-defined example supplies the module's selections and editable defaults.
/// Keep the constructor lazy: listing run types does not construct or execute runs.
type runExample(name: string, create: unit -> run) =
    let configured = lazy (create ())
    member _.Name = name
    member _.Run = configured.Value

/// One module containing run definitions with a fixed, positive channel count.
type runType(name: string, description: string, channelCount: int<GeneSort.Sorting.channelCount>, executionQueryCatalogName: string, examples: runExample[]) =
    do if int channelCount <= 0 then invalidArg (nameof channelCount) "A run module requires a positive channel count."
    do if System.String.IsNullOrWhiteSpace description then invalidArg (nameof description) "A run module requires a description."
    let source = {
        ProjectName = UMX.tag<projectName> name
        DatabaseName = UMX.tag<databaseName> (name.Split('.') |> Array.last)
        ExecutionQueryCatalogName = executionQueryCatalogName
    }
    do RunModuleCatalog.register name source
    member _.Source = source
    member _.ExecutionQueryCatalogName = executionQueryCatalogName
    member _.Name = name
    member _.ChannelCount = int channelCount
    member _.Description = description
    member _.Examples = Array.copy examples
