namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open System.Linq
open System.Collections.Generic


type sorterEval = 
    private { 
        sorter: sorter
        ceBlockUsage: ceBlockUsage
        stageSequence: Lazy<stageSequence>
    }

    static member create (sorter: sorter) (ceBlockUsage: ceBlockUsage) =
        { 
            sorter = sorter
            ceBlockUsage = ceBlockUsage
            stageSequence = Lazy<stageSequence>(
                    fun () -> 
                        let refined = ceBlockUsage.getUsedCes()
                        StageSequence.fromCes sorter.SortingWidth refined)
        }

    member this.getRefinedSorter() : sorter =
             sorter.create
                    (Guid.NewGuid() |> UMX.tag<sorterId>) this.sorter.SortingWidth (this.ceBlockUsage.getUsedCes()) 

    member this.getSortingWidth() : int<sortingWidth> =
        this.sorter.SortingWidth

    member this.getStageCount() : int<stageCount> =
        this.getStageSequence().StageCount 

    member this.getStageSequence() : stageSequence =
        this.stageSequence.Value
 

module SorterEval =

    let getSorterEval
            (sorter: sorter)
            (sorterTests: sorterTests) : sorterEval =

        let ceBlockEval = CeBlockOps.evalWithSorterTest sorterTests (ceBlock.create(sorter.Ces))

        sorterEval.create sorter ceBlockEval.ceBlockUsage


