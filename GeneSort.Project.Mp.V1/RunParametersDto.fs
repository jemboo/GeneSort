namespace GeneSort.Project.Mp.V1

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

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