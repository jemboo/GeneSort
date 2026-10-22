namespace GeneSort.Core.Mp

open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

module MessagePackSetup =

    let mutable private isConfigured = false

    let configure () =
        if not isConfigured then
            let resolver = 
                CompositeResolver.Create(
                    FSharpResolver.Instance,                // Handles DUs, Options, F# Lists
                    ContractlessStandardResolver.Instance   // Handles unannotated Records/Classes
                )
            
            MessagePackSerializer.DefaultOptions <- 
                MessagePackSerializerOptions.Standard.WithResolver(resolver)
                
            isConfigured <- true