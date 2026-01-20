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
        sorterTestsId: Guid<sorterTestId>
        sorterEvals: sorterEval[]
        ceLength: int<ceLength>
    }

    static member create 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sorterTestId>) 
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
    member this.SorterTestsId with get() : Guid<sorterTestId> = this.sorterTestsId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals
    member this.CeLength with get() : int<ceLength> = this.ceLength

 

module SorterSetEval =

    let makeSorterSetEval
            (sorterSet: sorterSet)
            (sortableTest: sortableTest) : sorterSetEval =

        let ceBlockEvals : (sorter * ceBlockEval) array = 
                sorterSet.Sorters 
                |> Array.map (fun s -> (s,  CeBlockOps.evalWithSorterTest sortableTest  (ceBlock.create (%s.SorterId |> UMX.tag<ceBlockId>) (s.Ces) )))

        let sorterEvals = 
            ceBlockEvals |> Array.map (
                fun (sorter, cdBlockEval ) -> 
                        sorterEval.create 
                            sorter.SorterId 
                            (sortableTest |> SortableTests.getId ) 
                            sorter.SortingWidth 
                            cdBlockEval.CeBlockWithUsage
                            (cdBlockEval.SortableTests |> SortableTests.getUnsortedCount)
                )

        sorterSetEval.create sorterSet.Id (sortableTest |> SortableTests.getId ) sorterEvals (sorterSet.CeLength)