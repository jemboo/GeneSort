namespace GeneSort.Project.Mp.V1


open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open GeneSort.Project.V1
open MessagePack.FSharp


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

    let toDomain (dto: runParametersDto) : runParameters =
        runParameters.create dto.paramMap
