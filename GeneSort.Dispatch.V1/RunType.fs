namespace GeneSort.Dispatch.V1

open GeneSort.Project.V1

/// A code-defined example supplies the module's selections and editable defaults.
/// Keep the constructor lazy: listing run types does not construct or execute runs.
type runExample(name: string, create: unit -> run) =
    let configured = lazy (create ())
    member _.Name = name
    member _.Run = configured.Value

/// One module containing run definitions. Mixed modules expose their examples explicitly.
type runType(name: string, examples: runExample[]) =
    member _.Name = name
    member _.Examples = Array.copy examples
