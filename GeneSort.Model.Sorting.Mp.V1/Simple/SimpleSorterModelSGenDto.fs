namespace GeneSort.Model.Sorting.Mp.V1.Simple

open GeneSort.Model.Sorting.V1.Simple

type simpleSorterModelSGenDto = {
    ModelGen: simpleSorterModelGenDto
    ActivationProbability: float
}

module SimpleSorterModelSGenDto =
    let fromDomain (generator: simpleSorterModelSGen) =
        { ModelGen = SimpleSorterModelGenDto.fromDomain generator.ModelGen
          ActivationProbability = generator.ActivationProbability }

    let toDomain (dto: simpleSorterModelSGenDto) =
        simpleSorterModelSGen.create (SimpleSorterModelGenDto.toDomain dto.ModelGen) dto.ActivationProbability
