namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable


type sorterEvalOld =

    private { 
        sorterId: Guid<sorterId>
        sorterTestsId: Guid<sorterTestId>
        sortingWidth: int<sortingWidth>
        ceBlockWithUsage: ceBlockWithUsage
        stageSequence: Lazy<stageSequence>
        unsortedCount: int<sortableCount>
    }

    static member create 
                (sorterId: Guid<sorterId>) 
                (sorterTestId: Guid<sorterTestId>)
                (sortingWidth: int<sortingWidth>)  
                (ceBlockWithUsage: ceBlockWithUsage)
                (unsortedCount: int<sortableCount>) =
        { 
            sorterId = sorterId
            sorterTestsId = sorterTestId
            sortingWidth = sortingWidth
            ceBlockWithUsage = ceBlockWithUsage
            stageSequence = Lazy<stageSequence>(StageSequence.fromCes sortingWidth ceBlockWithUsage.UsedCes)
            unsortedCount = unsortedCount
        }


    member this.getRefinedSorter() : sorter =
             sorter.create
                    (Guid.NewGuid() |> UMX.tag<sorterId>) 
                    this.sortingWidth 
                    (this.ceBlockWithUsage.UsedCes) 

                    
    member this.CeBlockUsage with get() = this.ceBlockWithUsage
    member this.CeLength with get() = this.ceBlockWithUsage.CeLength
    member this.SorterId with get() : Guid<sorterId> = this.sorterId
    member this.SorterTestId with get() : Guid<sorterTestId> = this.sorterTestsId
    member this.SortingWidth with get() : int<sortingWidth> =  this.sortingWidth

    member this.getUsedCeCount() : int<ceLength> =
        this.ceBlockWithUsage.UsedCes.Length |> UMX.tag<ceLength>

    member this.getLastUsedCeIndex : int = 
            this.ceBlockWithUsage.UseCounts.LastUsedCeIndex
        
    member this.UnsortedCount with get() = this.unsortedCount

    member this.getStageLength() : int<stageLength> =
        this.getStageSequence().StageLength 

    member this.getStageSequence() : stageSequence =
        this.stageSequence.Value
 

module SorterEvalOld =

    let makeSorterEval
            (sorter: sorter)
            (sortableTests: sortableTest) : sorterEvalOld =

        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTests (ceBlock.create (Guid.NewGuid() |> UMX.tag<ceBlockId>) (sorter.Ces))
        sorterEvalOld.create 
            sorter.SorterId 
            (sortableTests |> SortableTests.getId ) 
            sorter.SortingWidth 
            ceBlockEval.CeBlockWithUsage
            (sortableTests |> SortableTests.getUnsortedCount)
