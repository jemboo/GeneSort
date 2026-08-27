namespace GeneSort.Eval.Mp.V1.Sgd

open MessagePack
open GeneSort.Eval.V1.Sgd


[<MessagePackObject>]
type sorterPoolSetSummariesDto = {
    [<Key(0)>] spsDescriptionDtos: sorterPoolSetSummaryDto array
}


module SorterPoolSetSummariesDto =

    let fromDomain (domain: sorterPoolSetSummary array) : sorterPoolSetSummariesDto =
        let retVal =
            {
                spsDescriptionDtos = domain |> Array.map SorterPoolSetSummaryDto.toDto
            }
        retVal

    let toDomain (dto: sorterPoolSetSummariesDto) : sorterPoolSetSummary array =
        dto.spsDescriptionDtos |> Array.map SorterPoolSetSummaryDto.fromDto