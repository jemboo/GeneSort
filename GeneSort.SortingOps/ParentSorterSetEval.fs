namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting

type parentSorterSetEval =

    private { 
        id: Guid<parentSorterSetEvalId>
        grandParentMap: Map<Guid<sorterId>, parentSortingModelTag>
        sorterEvals: sorterEval[]
    }

    static member create 
                (id: Guid<parentSorterSetEvalId>)
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sorterTestId>) 
                (sorterEvals: sorterEval[]) =
        let id =
            [
                sorterSetId :> obj
                sorterTestsId :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<parentSorterSetEvalId>

        { 
            id = id
            grandParentMap = Map.empty
            sorterEvals = [||]
        }

    member this.Id with get() : Guid<parentSorterSetEvalId> = this.id
    member this.GrandParentMap with get() : Map<Guid<sorterId>, parentSortingModelTag> = this.grandParentMap
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals

 

module ParentSorterSetEval =

    ()