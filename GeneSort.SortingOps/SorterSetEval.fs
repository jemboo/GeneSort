namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable

// This type represents the evaluation of a set of sorters (sorterSet) against a 
// specific set of sortable tests (sortableTest).
type sorterSetEval =

    private { 
        sorterSetEvalId: Guid<sorterSetEvalId>
        sorterSetId: Guid<sorterSetId>
        sorterTestId: Guid<sortableTestId>
        sorterEvals: Map<Guid<sorterId>, sorterEval>
    }

    static member create 
                (sorterSetEvalId: Guid<sorterSetEvalId>) 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sortableTestId>) 
                (sorterEvals: sorterEval[]) =
        { 
            sorterSetEvalId = sorterSetEvalId
            sorterSetId = sorterSetId
            sorterTestId = sorterTestsId
            sorterEvals = sorterEvals |> Array.map (fun se -> (se.SorterId, se)) |> Map.ofArray
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestId with get() : Guid<sortableTestId> = this.sorterTestId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals.Values |> Seq.toArray

 

module SorterSetEval =

    let getCeLength (sorterSetEval: sorterSetEval) : int<ceLength> =
        match sorterSetEval.SorterEvals |> Array.tryHead with
        | Some firstEval -> firstEval.CeBlockEval.CeBlock.CeLength
        | None -> failwith "SorterSetEval contains no SorterEvals"

    let makeSorterSetEval
            (sorterSetEvalId: Guid<sorterSetEvalId>)
            (sorterSet: sorterSet)
            (sortableTest: sortableTest) 
            (collectNewSortableTests: bool) : sorterSetEval =

        let ceBlockAs = 
                sorterSet.Sorters 
                |> Array.map (fun sorter ->
                        ceBlock.create (%sorter.SorterId |> UMX.tag<ceBlockId>) sorter.SortingWidth sorter.Ces )

        let ceBlockEvals : ceBlockEval array = CeBlockOps.evalWithSorterTests sortableTest ceBlockAs collectNewSortableTests  

        let zipped = Array.zip sorterSet.Sorters ceBlockEvals

        let sorterEvals = 
            zipped |> Array.map (
                fun (sorter, ceBlockEval ) -> 
                        sorterEval.create 
                            sorter.SorterId
                            ceBlockEval
                )
        sorterSetEval.create sorterSetEvalId sorterSet.Id (sortableTest |> SortableTests.getId ) sorterEvals
