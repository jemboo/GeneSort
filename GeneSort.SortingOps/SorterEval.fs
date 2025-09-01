namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable


type sorterEval =

    private { 
        sorterId: Guid<sorterId>
        sorterTestsId: Guid<sorterTestsId>
        sortingWidth: int<sortingWidth>
        ceBlockUsage: ceBlockUsage
        stageSequence: Lazy<stageSequence>
    }

    static member create 
                (sorterId: Guid<sorterId>) 
                (sorterTestsId: Guid<sorterTestsId>)
                (sortingWidth: int<sortingWidth>)  
                (ceBlockUsage: ceBlockUsage) =
        { 
            sorterId = sorterId
            sorterTestsId = sorterTestsId
            sortingWidth = sortingWidth
            ceBlockUsage = ceBlockUsage
            stageSequence = Lazy<stageSequence>(StageSequence.fromCes sortingWidth ceBlockUsage.UsedCes)
        }


    member this.getRefinedSorter() : sorter =
             sorter.create
                    (Guid.NewGuid() |> UMX.tag<sorterId>) this.sortingWidth (this.ceBlockUsage.UsedCes) 

    member this.SorterId with get() : Guid<sorterId> = this.sorterId
    member this.SorterTestsId with get() : Guid<sorterTestsId> = this.sorterTestsId
    member this.SortingWidth with get() : int<sortingWidth> =  this.sortingWidth
    member this.CeBlockUsage with get() = this.ceBlockUsage

    member this.getUsedCeCount() : int<ceCount> =
        this.ceBlockUsage.UsedCes.Length |> UMX.tag<ceCount>

    member this.getStageCount() : int<stageCount> =
        this.getStageSequence().StageCount 

    member this.getStageSequence() : stageSequence =
        this.stageSequence.Value
 

module SorterEval =

    let makeSorterEval
            (sorter: sorter)
            (sorterTests: sorterTests) : sorterEval =

        let ceBlockEval = CeBlockOps.evalWithSorterTest sorterTests (ceBlock.create(sorter.Ces))

        sorterEval.create sorter.SorterId (sorterTests |> SorterTests.getId ) sorter.SortingWidth ceBlockEval.ceBlockUsage


