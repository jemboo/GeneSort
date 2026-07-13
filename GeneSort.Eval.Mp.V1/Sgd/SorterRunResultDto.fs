namespace GeneSort.Eval.Mp.V1

open MessagePack
open GeneSort.Eval.V1.Sgd


[<MessagePackObject>]
type sorterRunResultDto = {
    [<Key(0)>] spsDescriptionDtos: sorterPoolSetSummaryDto array
    [<Key(1)>] spsFinalDto: sorterPoolSetDto
}


module SorterRunResultDto =

    let fromDomain (domain: sorterRunResult) : sorterRunResultDto =
        {
            spsDescriptionDtos = domain.IntermediateHistory |> Array.map SorterPoolSetSummaryDto.toDto
            spsFinalDto = SorterPoolSetDto.toDto domain.FinalPoolSet
        }

    let toDomain (dto: sorterRunResultDto) : sorterRunResult =
        let finalPoolSet = SorterPoolSetDto.fromDto dto.spsFinalDto
        let summaries = dto.spsDescriptionDtos |> Array.map SorterPoolSetSummaryDto.fromDto
        sorterRunResult.create finalPoolSet summaries