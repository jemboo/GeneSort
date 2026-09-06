
namespace GeneSort.Core.Mp

open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MessagePackSetup =

    let mutable private isConfigured = false
    let private lockObj = obj()

    /// Thread-safe configuration for MessagePack DefaultOptions
    let configure () =
        if not isConfigured then
            lock lockObj (fun () ->
                if not isConfigured then
                    let resolver =
                        CompositeResolver.Create(
                            FSharpResolver.Instance,
                            StandardResolver.Instance
                        )
                    MessagePackSerializer.DefaultOptions <-
                        MessagePackSerializerOptions.Standard.WithResolver(resolver)
                    isConfigured <- true)

    // Optional: Alias for C# idiomatic PascalCase naming
    let Configure () = configure ()