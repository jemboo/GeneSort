namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Component
open GeneSort.Component.Sorter
open GeneSort.Component.Sortable


type sorterEval =
    private { 
        sorterId: Guid<sorterId>
        ceBlockEval: ceBlockEval
    }

    static member create 
                (sorterId: Guid<sorterId>) 
                (ceBlockEval: ceBlockEval) =
        { 
            sorterId = sorterId
            ceBlockEval = ceBlockEval
        }


    member this.CeBlockEval with get() = this.ceBlockEval
    member this.SorterId with get() : Guid<sorterId> = this.sorterId


module SorterEval =

    let makeSorterEval
            (sorter: sorter)
            (sortableTests: sortableTest) : sorterEval =
        let ceBlock = ceBlock.create (%sorter.SorterId |> UMX.tag<ceBlockId>) sorter.SortingWidth sorter.Ces 
        let ceBlockEval = CeBlockOps.evalWithSorterTest sortableTests ceBlock
        sorterEval.create
            sorter.SorterId
            ceBlockEval
