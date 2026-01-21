namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable


[<Measure>] type sorterSetEvalId

type sorterSetEval =

    private { 
        sorterSetEvalId: Guid<sorterSetEvalId>
        sorterSetId: Guid<sorterSetId>
        sorterTestId: Guid<sorterTestId>
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
            sorterTestId = sorterTestsId
            sorterEvals = sorterEval
            ceLength = ceLength
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestId with get() : Guid<sorterTestId> = this.sorterTestId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals
    member this.CeLength with get() : int<ceLength> = this.ceLength

 

module SorterSetEval =

    let makeSorterSetEval
            (sorterSet: sorterSet)
            (sortableTest: sortableTest) : sorterSetEval =


        let ceBlockAs = 
                sorterSet.Sorters 
                |> Array.map (fun sorter ->
                        ceBlock.create (%sorter.SorterId |> UMX.tag<ceBlockId>) sorter.SortingWidth sorter.Ces )

        let ceBlockEvals : ceBlockEval array = CeBlockOps.evalWithSorterTests sortableTest ceBlockAs    

        let zipped = Array.zip sorterSet.Sorters ceBlockEvals

        let sorterEvals = 
            zipped |> Array.map (
                fun (sorter, ceBlockEval ) -> 
                        sorterEval.create 
                            sorter.SorterId
                            ceBlockEval
                )
        sorterSetEval.create sorterSet.Id (sortableTest |> SortableTests.getId ) sorterEvals (sorterSet.CeLength)



    let makeSorterSetEval0
            (sorterSet: sorterSet)
            (sortableTest: sortableTest) : sorterSetEval =

        let ceBlockEvals : (sorter * ceBlockEval) array = 
                sorterSet.Sorters 
                |> Array.map (fun sorter ->
                        let ceBlock = ceBlock.create (%sorter.SorterId |> UMX.tag<ceBlockId>) sorter.SortingWidth sorter.Ces 
                        (
                            sorter,  
                            CeBlockOps.evalWithSorterTest sortableTest ceBlock
                        ))

        let sorterEvals = 
            ceBlockEvals |> Array.map (
                fun (sorter, ceBlockEval ) -> 
                        sorterEval.create 
                            sorter.SorterId
                            ceBlockEval
                )

        sorterSetEval.create sorterSet.Id (sortableTest |> SortableTests.getId ) sorterEvals (sorterSet.CeLength)