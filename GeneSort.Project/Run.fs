
namespace GeneSort.Project


open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.IO
open System.Threading
open System.Threading.Tasks


[<MessagePackObject>]
type RunDto = 
    { 
      [<Key("index")>] Index: int
      [<Key("parameters")>] Properties: Map<string, string>
    }

// Event type for Run completion
type RunCompletedEventArgs = { Index: int }

// Run type
type Run = 
    { Index: int
      Parameters: Map<string, string> }

module Run =
    // MessagePack options for serialization
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    // Convert Run to a Dto for serialization
    let toRunDto (run: Run) : RunDto =
        { Index = run.Index
          Properties = run.Parameters }

    let fromDto (dto: RunDto) : Run =
        { Index = dto.Index
          Parameters = dto.Properties }
