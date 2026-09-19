namespace GeneSort.Project.Mp.V1

open FSharp.UMX
open MessagePack
open MessagePack.Resolvers
open GeneSort.Project.V1
open MessagePack.FSharp

type runParametersDto = 
    { 
      paramMap: Map<string, string>
    }

module RunParametersDto =
    
    // Convert Run to a Dto for serialization
    let fromDomain (runParameters: runParameters) : runParametersDto =
        { paramMap = runParameters.ParamMap }

    let toDomain (dto: runParametersDto) : runParameters =
        runParameters.create dto.paramMap