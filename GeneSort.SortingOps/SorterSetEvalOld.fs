namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable

// This type represents the evaluation of a set of sorters (sorterSet) against a 
// specific set of sortable tests (sortableTest).
type sorterSetEvalOld =

    private { 
        sorterSetEvalId: Guid<sorterSetEvalId>
        sorterSetId: Guid<sorterSetId>
        sorterTestId: Guid<sortableTestId>
        sorterEvals: Map<Guid<sorterId>, sorterEvalOld>
    }

    static member create 
                (sorterSetEvalId: Guid<sorterSetEvalId>) 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sortableTestId>) 
                (sorterEvals: sorterEvalOld[]) =
        { 
            sorterSetEvalId = sorterSetEvalId
            sorterSetId = sorterSetId
            sorterTestId = sorterTestsId
            sorterEvals = sorterEvals |> Array.map (fun se -> (se.SorterId, se)) |> Map.ofArray
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestId with get() : Guid<sortableTestId> = this.sorterTestId
    member this.SorterEvals with get() : sorterEvalOld[] = this.sorterEvals.Values |> Seq.toArray

 

module SorterSetEvalOld =

    let getCeLength (sorterSetEval: sorterSetEvalOld) : int<ceLength> =
        match sorterSetEval.SorterEvals |> Array.tryHead with
        | Some firstEval -> firstEval.CeBlockEval.CeBlock.CeLength
        | None -> failwith "SorterSetEval contains no SorterEvals"


    let makeSorterEvals 
                (sorters: sorter array) 
                (sortableTest: sortableTest) 
                (collectNewSortableTests: bool) :sorterEvalOld array =
        let ceBlocks = 
                sorters 
                |> Array.map (fun sorter ->
                        ceBlock.create 
                            (%sorter.SorterId |> UMX.tag<ceBlockId>) 
                            sorter.SortingWidth sorter.Ces )

        let ceBlockEvals : ceBlockEval array =
                CeBlockOps.evalWithSorterTests 
                        sortableTest 
                        ceBlocks 
                        collectNewSortableTests  

        Array.zip sorters ceBlockEvals
        |> Array.map (
                fun (sorter, ceBlockEval ) -> 
                        sorterEvalOld.create 
                            sorter.SorterId
                            ceBlockEval
                )


    let makeSorterSetEval
            (sorterSetEvalId: Guid<sorterSetEvalId>)
            (sorterSet: sorterSet)
            (sortableTest: sortableTest) 
            (collectNewSortableTests: bool) : sorterSetEvalOld =

        let sorterEvals = makeSorterEvals sorterSet.Sorters sortableTest collectNewSortableTests
        sorterSetEvalOld.create 
                    sorterSetEvalId 
                    sorterSet.Id 
                    (sortableTest |> SortableTests.getId ) 
                    sorterEvals
