namespace GeneSort.Eval.Mp.V1.Sgd.Soss

open System
open MessagePack
open FSharp.UMX
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1
open GeneSort.Core
open GeneSort.Eval.V1.Sgd.Soss

// ---------------------------------------------------------------------
// Lightweight Summary Snapshot DTOs
// ---------------------------------------------------------------------

type sorterPoolSummaryDto = {
    sorterPoolId: Guid
    sorterPoolName: string
    sortableTestsSubsetId: string
    aveCeLength: float
    minCeLength: int
    minStageLength: int
    aveStageLength: float
    aveStageCrossings: float
    rawCeLength: int
    stdDevCeLength: float
    stdDevStageLength: float
    aveReflectiveCount: float
    averageUnsortedCount: float
}

type sorterPoolSetSummaryDto = {
    sorterPoolSetId: Guid
    generationNumber: int
    sortedSorterEvalPercentage: float
    sorterPoolSummaryDtos: sorterPoolSummaryDto array
}

type sorterPoolSetSummarySetDto_Soss = {
    sorterPoolSetSummarySetId: Guid
    lastGeneration: int
    sorterPoolSetSummaryDtos: sorterPoolSetSummaryDto array
}

// ---------------------------------------------------------------------
// Translation Logic Modules
// ---------------------------------------------------------------------

module SorterPoolSetSummaryDto =

    let toDto (domain: sorterPoolSetSummary_Soss) : sorterPoolSetSummaryDto =
        let poolSummaryDtos =
            domain.SorterPoolSummaries
            |> Array.map (fun p ->
                { 
                    sorterPoolSummaryDto.sorterPoolId = UMX.untag p.SorterPoolId
                    sorterPoolName = UMX.untag p.SorterPoolName
                    sortableTestsSubsetId = UMX.untag p.SortableTestsSubsetId
                    aveCeLength = UMX.untag p.AveCeLength
                    minCeLength = UMX.untag p.MinCeLength
                    minStageLength = UMX.untag p.MinStageLength
                    aveStageLength = UMX.untag p.AveStageLength
                    rawCeLength = UMX.untag p.RawCeLength
                    aveStageCrossings = UMX.untag p.AveStageCrossings
                    stdDevCeLength = UMX.untag p.StdDevCeLength
                    stdDevStageLength = UMX.untag p.StdDevStageLength
                    aveReflectiveCount = UMX.untag p.AveReflectiveCountR
                    averageUnsortedCount = p.AverageUnsortedCount
                }
            )
        {
            sorterPoolSetId = UMX.untag domain.SorterPoolSetId
            generationNumber = UMX.untag domain.GenerationNumber
            sortedSorterEvalPercentage = domain.SortedSorterEvalPercentage
            sorterPoolSummaryDtos = poolSummaryDtos
        }

    let fromDto (dto: sorterPoolSetSummaryDto) : sorterPoolSetSummary_Soss =
        let poolSummaryDomains =
            dto.sorterPoolSummaryDtos
            |> Array.map (fun p ->
                spSummary_Soss.create
                    (p.sorterPoolId |> UMX.tag<sorterPoolId>)
                    (p.sorterPoolName |> UMX.tag<sorterPoolName>)
                    (if String.IsNullOrWhiteSpace p.sortableTestsSubsetId then SortableTestsSubsetId.Default else p.sortableTestsSubsetId |> UMX.tag<sortableTestsSubsetId>)
                    (p.rawCeLength |> UMX.tag<ceLength>)
                    (p.minCeLength |> UMX.tag<ceLength>)
                    (p.aveCeLength |> UMX.tag<ceLength>)
                    (p.stdDevCeLength |> UMX.tag<ceLength>)
                    (p.minStageLength |> UMX.tag<stageLength>)
                    (p.aveStageLength |> UMX.tag<stageLength>)
                    (p.stdDevStageLength |> UMX.tag<stageLength>)
                    (p.aveStageCrossings |> UMX.tag<stageCrossings>)
                    (p.aveReflectiveCount |> UMX.tag<reflectiveCount>)
                    p.averageUnsortedCount
            )
        sorterPoolSetSummary_Soss.Create(
            UMX.tag dto.sorterPoolSetId, 
            UMX.tag dto.generationNumber, 
            dto.sortedSorterEvalPercentage,
            poolSummaryDomains
        )

module SorterPoolSetSummarySetDto_Soss =

    let toDto (domain: spSummarySet_Soss) : sorterPoolSetSummarySetDto_Soss =
        {
            sorterPoolSetSummarySetId = UMX.untag domain.SorterPoolSetSummarySetId
            lastGeneration = UMX.untag domain.LastGeneration
            sorterPoolSetSummaryDtos = 
                domain.SorterPoolSetSummaries 
                |> Array.map SorterPoolSetSummaryDto.toDto
        }

    let fromDto (dto: sorterPoolSetSummarySetDto_Soss) : spSummarySet_Soss =
        let summaries = 
            dto.sorterPoolSetSummaryDtos 
            |> Array.map SorterPoolSetSummaryDto.fromDto
        
        spSummarySet_Soss.create 
            (dto.sorterPoolSetSummarySetId |> UMX.tag<sorterPoolSetSummarySetId>)
            (dto.lastGeneration |> UMX.tag<generationNumber>) 
            summaries
