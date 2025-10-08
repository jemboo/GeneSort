
namespace GeneSort.Project


open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

[<MessagePackObject>]
type run2Dto = 
    { 
      [<Key("0")>] index: int
      [<Key("1")>] repl: int
      [<Key("2")>] runParametersDto: runParametersDto
    }

module Run2Dto =
    // MessagePack options for serialization
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    // Convert Run to a Dto for serialization
    let toRunDto (run: run2) : run2Dto =
        { index = %run.Index
          repl = %run.Repl
          runParametersDto = run.RunParameters |> RunParametersDto.toRunParametersDto }

    let fromDto (dto: run2Dto) : run2 =
        run2.create 
            (dto.index |> UMX.tag<indexNumber>)
            (RunParametersDto.fromDto dto.runParametersDto)