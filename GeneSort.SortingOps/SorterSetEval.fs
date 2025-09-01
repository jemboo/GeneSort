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
        sorterTestsId: Guid<sorterTestsId>
        sorterEvals: sorterEval[]
    }

    static member create 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sorterTestsId>) 
                (sorterEval: sorterEval[]) =
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
        }

    member this.SorterSetEvalId with get() : Guid<sorterSetEvalId> = this.sorterSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestsId with get() : Guid<sorterTestsId> = this.sorterTestsId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals

 

module SorterSetEval =

    let makeSorterSetEval
            (sorterSet: sorterSet)
            (sorterTests: sorterTests) : sorterSetEval =

        let ceBlockEvals = 
                sorterSet.Sorters 
                |> Array.map (fun s -> (s,  CeBlockOps.evalWithSorterTest sorterTests (ceBlock.create(s.Ces))))

        let sorterEvals = 
            ceBlockEvals |> Array.map (
                fun (sorter, ce ) -> 
                        sorterEval.create 
                            sorter.SorterId 
                            (sorterTests |> SorterTests.getId ) 
                            sorter.SortingWidth ce.ceBlockUsage
                )

        sorterSetEval.create sorterSet.SorterSetId (sorterTests |> SorterTests.getId ) sorterEvals