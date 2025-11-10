
namespace GeneSort.Runs.Mp

open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open GeneSort.Runs


[<MessagePackObject>]
type runParametersDto = 
    { 
      [<Key("0")>] paramMap: Map<string, string>
    }

module RunParametersDto =
    // MessagePack options for serialization
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    // Convert Run to a Dto for serialization
    let fromDomain (runParameters: runParameters) : runParametersDto =
        { paramMap = runParameters.ParamMap }

    let fromDto (dto: runParametersDto) : runParameters =
        runParameters.create dto.paramMap
