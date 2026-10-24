namespace GeneSort.Model.Sorting.Mp.V1.Simple

open GeneSort.Model.Sorting.V1.Simple

type simpleSorterModelSDto = {
    Model: simpleSorterModelDto
    Activators: bool[]
}

module SimpleSorterModelSDto =
    let fromDomain (model: simpleSorterModelS) : simpleSorterModelSDto =
        { Model = SimpleSorterModelDto.fromDomain model.Model
          Activators = model.Activators }

    let toDomain (dto: simpleSorterModelSDto) : simpleSorterModelS =
        simpleSorterModelS.create (SimpleSorterModelDto.toDomain dto.Model) dto.Activators
