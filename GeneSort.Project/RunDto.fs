
namespace GeneSort.Project


open FSharp.UMX

open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers


[<MessagePackObject>]
type RunDto = 
    { 
      [<Key("0")>] Index: int
      [<Key("1")>] Cycle: int
      [<Key("2")>] Properties: Map<string, string>
    }

module RunDto =
    // MessagePack options for serialization
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    // Convert Run to a Dto for serialization
    let toRunDto (run: Run) : RunDto =
        { Index = run.Index
          Cycle = %run.Cycle
          Properties = run.Parameters }

    let fromDto (dto: RunDto) : Run =
        { Index = dto.Index
          Cycle = dto.Cycle |> UMX.tag<cycleNumber>
          Parameters = dto.Properties }
