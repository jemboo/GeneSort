namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable


type sorterSetEval =

    private { 
        sorterSetEvalId: Guid<sorterSetEvalId>
        sorterSetId: Guid<sorterSetId>
        sorterTestId: Guid<sortableTestsId>
        sorterEvals: Map<Guid<sorterId>, sorterEval>
    }

    static member create 
                (sorterSetEvalId: Guid<sorterSetEvalId>) 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sortableTestsId>) 
                (sorterEvals: sorterEval[]) =
        { 
            sorterSetEvalId = sorterSetEvalId
            sorterSetId = sorterSetId
            sorterTestId = sorterTestsId
            sorterEvals = sorterEvals |> Array.map (fun se -> (se |> SorterEval.getSorterId, se)) |> Map.ofArray
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestId with get() : Guid<sortableTestsId> = this.sorterTestId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals.Values |> Seq.toArray

 

module SorterSetEval =

    let makeSorterEvals
                (sorters: sorter array) 
                (prefix: ceBlock)
                (sortableTests: sortableTests) 
                (sorterEvalType:sorterEvalType) 
                (collectNewSortableTests: bool<collectNewSortableTests>) :sorterEval array =
        let ceBlocks = 
                sorters 
                |> Array.map (fun sorter ->
                        ceBlock.create 
                            (%sorter.SorterId |> UMX.tag<ceBlockId>) 
                            sorter.SortingWidth sorter.Ces )

        let ceBlockEvals : ceBlockEval array =
                CeBlockOps.evalWithSorterTests 
                        sortableTests 
                        prefix
                        ceBlocks 
                        %collectNewSortableTests  

        Array.zip sorters ceBlockEvals
        |> Array.map (
                fun (sorter, ceBlockEval ) -> 
                        SorterEval.create 
                            sorterEvalType
                            sorter.SorterId
                            ceBlockEval
                )


    let makeSorterSetEval
            (sorterSetEvalId: Guid<sorterSetEvalId>)
            (sorterSet: sorterSet)
            (prefix: ceBlock)
            (sortableTests: sortableTests) 
            (sorterEvalType:sorterEvalType) 
            (collectNewSortableTests: bool<collectNewSortableTests>) : sorterSetEval =

        let sorterEvals = makeSorterEvals 
                            sorterSet.Sorters 
                            prefix
                            sortableTests 
                            sorterEvalType 
                            collectNewSortableTests

        sorterSetEval.create 
                    sorterSetEvalId 
                    sorterSet.Id 
                    (sortableTests |> SortableTests.getId ) 
                    sorterEvals




    let makeFullDataTableRecords (source: sorterSetEval) : GeneSort.Core.dataTableRecord seq =
        source.SorterEvals
        |> Seq.map (SorterEval.toDataTableRecord)