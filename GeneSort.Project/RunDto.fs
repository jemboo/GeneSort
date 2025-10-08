
namespace GeneSort.Project


open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

[<MessagePackObject>]
type runDto = 
    { 
      [<Key("0")>] index: int
      [<Key("1")>] repl: int
      [<Key("2")>] runParametersDto: runParametersDto
    }

module RunDto =
    // MessagePack options for serialization
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    // Convert Run to a Dto for serialization
    let toRunDto (run: run) : runDto =
        { index = %run.Index
          repl = %run.Repl
          runParametersDto = run.RunParameters |> RunParametersDto.toRunParametersDto }

    let fromDto (dto: runDto) : run =
        run.create 
            (dto.index |> UMX.tag<indexNumber>)
            (RunParametersDto.fromDto dto.runParametersDto)