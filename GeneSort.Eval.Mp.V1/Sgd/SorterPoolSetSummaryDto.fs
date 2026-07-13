namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting
open GeneSort.Eval.V1

// ---------------------------------------------------------------------
// Lightweight Summary Snapshot DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterPoolSummaryDto = {
    [<Key(0)>] sorterPoolId: Guid
    [<Key(1)>] sorterPoolName: string
    [<Key(2)>] aveCeLength: int
    [<Key(3)>] minCeLength: int
    [<Key(4)>] minStageLength: int
    [<Key(5)>] aveStageLength: int
    [<Key(6)>] rawCeLength: int
}

[<MessagePackObject>]
type sorterPoolSetSummaryDto = {
    [<Key(0)>] sorterPoolSetId: Guid
    [<Key(1)>] generationNumber: int
    [<Key(2)>] sorterPoolSummaryDtos: sorterPoolSummaryDto array
}


// ---------------------------------------------------------------------
// Translation Logic Modules
// ---------------------------------------------------------------------

module SorterPoolSetSummaryDto =

    let toDto (domain: sorterPoolSetSummary) : sorterPoolSetSummaryDto =
        let poolSummaryDtos =
            domain.SorterPoolSummaries
            |> Array.map (fun p ->
                { 
                    sorterPoolSummaryDto.sorterPoolId = UMX.untag p.SorterPoolId
                    sorterPoolName = UMX.untag p.SorterPoolName
                    aveCeLength = UMX.untag p.AveCeLength
                    minCeLength = UMX.untag p.MinCeLength
                    minStageLength = UMX.untag p.MinStageLength
                    aveStageLength = UMX.untag p.AveStageLength
                    rawCeLength = UMX.untag p.RawCeLength
                }
            )
        {
            sorterPoolSetId = UMX.untag domain.SorterPoolSetId
            generationNumber = UMX.untag domain.GenerationNumber
            sorterPoolSummaryDtos = poolSummaryDtos
        }

    let fromDto (dto: sorterPoolSetSummaryDto) : sorterPoolSetSummary =
        let poolSummaryDomains =
            dto.sorterPoolSummaryDtos
            |> Array.map (fun p ->
                sorterPoolSummary.create
                    (p.sorterPoolId |> UMX.tag<sorterPoolId>)
                    (p.sorterPoolName |> UMX.tag<sorterPoolName>)
                    (p.rawCeLength |> UMX.tag<ceLength>)
                    (p.minCeLength |> UMX.tag<ceLength>)
                    (p.aveCeLength |> UMX.tag<ceLength>)
                    (p.minStageLength |> UMX.tag<stageLength>)
                    (p.aveStageLength |> UMX.tag<stageLength>)
            )
        sorterPoolSetSummary.Create(
            UMX.tag dto.sorterPoolSetId, 
            UMX.tag dto.generationNumber, 
            poolSummaryDomains
        )