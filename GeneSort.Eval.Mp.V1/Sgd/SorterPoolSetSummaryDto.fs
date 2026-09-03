namespace GeneSort.Eval.Mp.V1.Sgd

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Core

// ---------------------------------------------------------------------
// Lightweight Summary Snapshot DTOs
// ---------------------------------------------------------------------

[<MessagePackObject>]
type sorterPoolSummaryDto = {
    [<Key(0)>] sorterPoolId: Guid
    [<Key(1)>] sorterPoolName: string
    [<Key(2)>] aveCeLength: float
    [<Key(3)>] minCeLength: int
    [<Key(4)>] minStageLength: int
    [<Key(5)>] aveStageLength: float
    [<Key(6)>] aveStageCrossings: float
    [<Key(7)>] rawCeLength: int
    [<Key(8)>] stdDevCeLength: float
    [<Key(9)>] stdDevStageLength: float
}

[<MessagePackObject>]
type sorterPoolSetSummaryDto = {
    [<Key(0)>] sorterPoolSetId: Guid
    [<Key(1)>] generationNumber: int
    [<Key(2)>] sorterPoolSummaryDtos: sorterPoolSummaryDto array
}

[<MessagePackObject>]
type sorterPoolSetSummarySetDto = {
    [<Key(0)>] sorterPoolSetSummarySetId: Guid
    [<Key(1)>] lastGeneration: int
    [<Key(2)>] sorterPoolSetSummaryDtos: sorterPoolSetSummaryDto array
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
                    aveStageCrossings = UMX.untag p.AveStageCrossings
                    stdDevCeLength = UMX.untag p.StdDevCeLength
                    stdDevStageLength = UMX.untag p.StdDevStageLength
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
                    (p.stdDevCeLength |> UMX.tag<ceLength>)
                    (p.minStageLength |> UMX.tag<stageLength>)
                    (p.aveStageLength |> UMX.tag<stageLength>)
                    (p.stdDevStageLength |> UMX.tag<stageLength>)
                    (p.aveStageCrossings |> UMX.tag<stageCrossings>)
            )
        sorterPoolSetSummary.Create(
            UMX.tag dto.sorterPoolSetId, 
            UMX.tag dto.generationNumber, 
            poolSummaryDomains
        )


module SorterPoolSetSummarySetDto =

    let toDto (domain: sorterPoolSetSummarySet) : sorterPoolSetSummarySetDto =
        {
            sorterPoolSetSummarySetId = UMX.untag domain.SorterPoolSetSummarySetId
            lastGeneration = UMX.untag domain.LastGeneration
            sorterPoolSetSummaryDtos = 
                domain.SorterPoolSetSummaries 
                |> Array.map SorterPoolSetSummaryDto.toDto
        }

    let fromDto (dto: sorterPoolSetSummarySetDto) : sorterPoolSetSummarySet =
        let summaries = 
            dto.sorterPoolSetSummaryDtos 
            |> Array.map SorterPoolSetSummaryDto.fromDto
        
        sorterPoolSetSummarySet.create 
            (dto.sorterPoolSetSummarySetId |> UMX.tag<sorterPoolSetSummarySetId>)
            (dto.lastGeneration |> UMX.tag<generationNumber>) 
            summaries