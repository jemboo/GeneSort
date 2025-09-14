namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable


type sorterEval =

    private { 
        sorterId: Guid<sorterId>
        sorterTestsId: Guid<sortableTestsId>
        sortingWidth: int<sortingWidth>
        ceBlockWithUsage: ceBlockWithUsage
        stageSequence: Lazy<stageSequence>
        unsortedCount: int
    }

    static member create 
                (sorterId: Guid<sorterId>) 
                (sorterTestsId: Guid<sortableTestsId>)
                (sortingWidth: int<sortingWidth>)  
                (ceBlockWithUsage: ceBlockWithUsage)
                (unsortedCount: int) =
        { 
            sorterId = sorterId
            sorterTestsId = sorterTestsId
            sortingWidth = sortingWidth
            ceBlockWithUsage = ceBlockWithUsage
            stageSequence = Lazy<stageSequence>(StageSequence.fromCes sortingWidth ceBlockWithUsage.UsedCes)
            unsortedCount = unsortedCount
        }


    member this.getRefinedSorter() : sorter =
             sorter.create
                    (Guid.NewGuid() |> UMX.tag<sorterId>) this.sortingWidth (this.ceBlockWithUsage.UsedCes) 

                    
    member this.CeBlockUsage with get() = this.ceBlockWithUsage
    member this.CeLength with get() = this.ceBlockWithUsage.CeLength
    member this.SorterId with get() : Guid<sorterId> = this.sorterId
    member this.SorterTestsId with get() : Guid<sortableTestsId> = this.sorterTestsId
    member this.SortingWidth with get() : int<sortingWidth> =  this.sortingWidth

    member this.getUsedCeCount() : int<ceLength> =
        this.ceBlockWithUsage.UsedCes.Length |> UMX.tag<ceLength>

    member this.getLastUsedCeIndex : int = 
            this.ceBlockWithUsage.LastUsedCeIndex
        
    member this.UnsortedCount with get() = this.unsortedCount

    member this.getStageCount() : int<stageLength> =
        this.getStageSequence().StageCount 

    member this.getStageSequence() : stageSequence =
        this.stageSequence.Value
 

module SorterEval =

    let makeSorterEval
            (sorter: sorter)
            (sortableTests: sortableTests) : sorterEval =

        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTests (ceBlock.create(sorter.Ces))
        sorterEval.create 
            sorter.SorterId 
            (sortableTests |> SortableTests.getId ) 
            sorter.SortingWidth 
            ceBlockEval.CeBlockWithUsage
            (sortableTests |> SortableTests.getUnsortedCount)

    let reportHeader : string =
        "SorterId \tSorterTestsId \tSortingWidth \tCeLengthUsed \tCeCount \tLastUsedCeIndex \tStageCount \tUnsortedCount"

    let reportLine (se: sorterEval) : string =
        $"{se.SorterId} \t{se.SorterTestsId} \t{se.SortingWidth} \t{se.getUsedCeCount()} \t{se.CeLength} \t{se.getLastUsedCeIndex} \t{se.getStageCount()} \t{se.UnsortedCount}"

