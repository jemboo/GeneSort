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
        sorterTestId: Guid<sorterTestId>
        sorterEvals: sorterEvalOld[]
        ceLength: int<ceLength>
    }

    static member create 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sorterTestId>) 
                (sorterEval: sorterEvalOld[])
                (ceLength: int<ceLength>) =
        let id =
            [
                sorterSetId :> obj
                sorterTestsId :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterSetEvalId>

        { 
            sorterSetEvalId = id
            sorterSetId = sorterSetId
            sorterTestId = sorterTestsId
            sorterEvals = sorterEval
            ceLength = ceLength
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestId with get() : Guid<sorterTestId> = this.sorterTestId
    member this.SorterEvals with get() : sorterEvalOld[] = this.sorterEvals
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
                fun (sorter, ceBlockEval ) -> 
                        sorterEvalOld.create 
                            sorter.SorterId 
                            (sortableTest |> SortableTests.getId ) 
                            sorter.SortingWidth 
                            ceBlockEval.CeBlockWithUsage
                            (ceBlockEval.SortableTests |> SortableTests.getUnsortedCount)
                )

        sorterSetEval.create sorterSet.Id (sortableTest |> SortableTests.getId ) sorterEvals (sorterSet.CeLength)