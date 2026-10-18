namespace GeneSort.Project.Mp.V1

open GeneSort.Project.V1.Parameters

type setupParametersDto =
    { paramMap: Map<string, string> }

module SetupParametersDto =

    /// Convert setup parameters to their MessagePack representation.
    let fromDomain (parameters: setupParameters) : setupParametersDto =
        { paramMap = parameters.ParamMap }

    let toDomain (dto: setupParametersDto) : setupParameters =
        setupParameters.create dto.paramMap
