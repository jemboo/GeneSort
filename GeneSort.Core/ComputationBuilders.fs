namespace GeneSort.Core

[<AutoOpen>]
module ComputationBuilders =

    type MaybeBuilder() =
        member _.Bind(x, f) = Option.bind f x
        member _.Return(x) = Some x
        member _.ReturnFrom(x) = x
        member _.Zero() = None

    let maybe = MaybeBuilder()