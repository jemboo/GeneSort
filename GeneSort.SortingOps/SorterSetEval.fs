namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable


[<Measure>] type sorterSetEvalId

type sorterSetEval =

    private { 
        sorterSetEvalId: Guid<sorterSetEvalId>
        sorterSetId: Guid<sorterSetId>
        sorterTestsId: Guid<sortableTestsId>
        sorterEvals: sorterEval[]
        ceLength: int<ceLength>
    }

    static member create 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sortableTestsId>) 
                (sorterEval: sorterEval[])
                (ceLength: int<ceLength>) =
        let id =
            [
                sorterSetId :> obj
                sorterTestsId :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterSetEvalId>

        { 
            sorterSetEvalId = id
            sorterSetId = sorterSetId
            sorterTestsId = sorterTestsId
            sorterEvals = sorterEval
            ceLength = ceLength
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestsId with get() : Guid<sortableTestsId> = this.sorterTestsId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals
    member this.CeLength with get() : int<ceLength> = this.ceLength

 

module SorterSetEval =

    let makeSorterSetEval
            (sorterSet: sorterSet)
            (sortableTests: sortableTests) : sorterSetEval =

        let ceBlockEvals : (sorter * ceBlockEval) array = 
                sorterSet.Sorters 
                |> Array.map (fun s -> (s,  CeBlockOps.evalWithSorterTest sortableTests (ceBlock.create(s.Ces))))

        let sorterEvals = 
            ceBlockEvals |> Array.map (
                fun (sorter, ce ) -> 
                        sorterEval.create 
                            sorter.SorterId 
                            (sortableTests |> SortableTests.getId ) 
                            sorter.SortingWidth 
                            ce.CeBlockWithUsage
                            (ce.SortableTests |> SortableTests.getUnsortedCount)
                )

        sorterSetEval.create sorterSet.SorterSetId (sortableTests |> SortableTests.getId ) sorterEvals (sorterSet.CeLength)