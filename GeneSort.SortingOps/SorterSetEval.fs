namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting

[<Measure>] type sorterModelSetEvalId

type sorterModelSetEval =

    private { 
        sorterModelSetEvalId: Guid<sorterModelSetEvalId>
        sorterSetId: Guid<sorterSetId>
        sorterTestId: Guid<sorterTestId>
        sorterEvals: sorterEval[]
    }

    static member create 
                (sorterSetId: Guid<sorterSetId>) 
                (sorterTestsId: Guid<sorterTestId>) 
                (sorterEval: sorterEval[]) =
        let id =
            [
                sorterSetId :> obj
                sorterTestsId :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelSetEvalId>

        { 
            sorterModelSetEvalId = id
            sorterSetId = sorterSetId
            sorterTestId = sorterTestsId
            sorterEvals = sorterEval
        }

    member this.SorterModelSetEvalId with get() : Guid<sorterModelSetEvalId> = this.sorterModelSetEvalId
    member this.SorterSetId with get() : Guid<sorterSetId> = this.sorterSetId
    member this.SorterTestId with get() : Guid<sorterTestId> = this.sorterTestId
    member this.SorterEvals with get() : sorterEval[] = this.sorterEvals

 

module SorterModelSetEval =

    let getCeLength (sorterModelSetEval: sorterModelSetEval) : int<ceLength> =
        match sorterModelSetEval.SorterEvals |> Array.tryHead with
        | Some firstEval -> firstEval.CeBlockEval.CeBlock.CeLength
        | None -> failwith "SorterSetEval contains no SorterEvals"

    let makeSorterSetEval
            (sorterSet: sorterSet)
            (sortableTest: sortableTest) 
            (collectResults: bool) : sorterModelSetEval =


        let ceBlockAs = 
                sorterSet.Sorters 
                |> Array.map (fun sorter ->
                        ceBlock.create (%sorter.SorterId |> UMX.tag<ceBlockId>) sorter.SortingWidth sorter.Ces )

        let ceBlockEvals : ceBlockEval array = CeBlockOps.evalWithSorterTests sortableTest ceBlockAs collectResults  

        let zipped = Array.zip sorterSet.Sorters ceBlockEvals

        let sorterEvals = 
            zipped |> Array.map (
                fun (sorter, ceBlockEval ) -> 
                        sorterEval.create 
                            sorter.SorterId
                            ceBlockEval
                )
        sorterModelSetEval.create sorterSet.Id (sortableTest |> SortableTests.getId ) sorterEvals


    /// For the sorterSet and its corresponding sorterSetEval, this creates a subset 
    /// that consists of all the sorters with an UnsortedCount = 0
    let makePassingSorterSet
            (sorterSet: sorterSet)
            (sorterSetEval: sorterModelSetEval) : sorterSet =
        
        // 1. Identify the IDs of the sorters that passed (UnsortedCount = 0)
        let passingIds = 
            sorterSetEval.SorterEvals 
            |> Array.filter (fun se -> se.CeBlockEval.UnsortedCount = 0<sortableCount>)
            |> Array.map (fun se -> se.SorterId)
            |> System.Collections.Generic.HashSet

        // 2. Filter the original sorter collection based on the passing IDs
        let passingSorters = 
            sorterSet.Sorters 
            |> Array.filter (fun s -> passingIds.Contains(s.SorterId))

        // 3. Create a new SorterSet with a unique ID derived from the passing subset
        // Note: Using the original set ID and evaluation ID to maintain lineage
        let newSetId = 
            [ 
                sorterSet.Id :> obj
                sorterSetEval.SorterModelSetEvalId :> obj
                "PassingSubset" :> obj 
            ] 
            |> GuidUtils.guidFromObjs 
            |> UMX.tag<sorterSetId>

        GeneSort.Sorting.Sorter.sorterSet.create 
            newSetId
            passingSorters



    /// For the sorterSet and its corresponding sorterSetEval, this creates a subset 
    /// that consists of all the sorters with an UnsortedCount = 0
    let makePassingSortingModelSet
            (sms: sortingModelSet)
            (sorterSetEval: sorterModelSetEval) :sortingModelSet =
        
        // 1. Identify the IDs of the sorters that passed (UnsortedCount = 0)
        let passingIds = 
            sorterSetEval.SorterEvals 
            |> Array.filter (fun se -> se.CeBlockEval.UnsortedCount = 0<sortableCount>)
            |> Array.map (fun se -> se.SorterId)

        // 2. Filter the original sorter collection based on the passing IDs
        let passingSorterModels = 
            sms.SortingModels 
            |> Array.filter (fun stm -> SortingModel.containsAnySorter passingIds stm)

        sortingModelSet.create 
            (Guid.NewGuid() |> UMX.tag<sortingModelSetID>) 
            passingSorterModels

