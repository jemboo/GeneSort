namespace GeneSort.Model.Sortable.V1

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableTestModelSet =
    private
        { 
          id : Guid<sorterTestModelSetID>
          sorterTestModels : sortableTestModel array
        }
    with
    static member create 
                (id : Guid<sorterTestModelSetID>)
                (sorterTestModels : sortableTestModel array) =

        { id = id; sorterTestModels = sorterTestModels; }

    member this.Id with get() = this.id
    member this.SorterTestModels with get() = this.sorterTestModels

    member this.makeSortableTestSet 
                    (sorterTestId: Guid<sortableTestId>)
                    (sortableDataType:sortableDataFormat) : sortableTestSet =
        let id = (%this.id) |> UMX.tag<sortableTestSetId>

        let sortableTestArray = 
                this.SorterTestModels
                |> Array.map(fun model -> SortableTestModel.makeSortableTest sorterTestId model sortableDataType)

        match sortableDataType with
        | sortableDataFormat.BoolArray -> 
            let boolTests = 
                this.SorterTestModels 
                |> Array.map (fun model ->
                                    SortableTestModel.makeSortableTest 
                                        (UMX.tag<sortableTestId> (Guid.NewGuid()))
                                        model 
                                        sortableDataType )
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Bools bt -> bt
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
            sortableTestSet.Bools (sortableBoolTestSet.create id boolTests)

        | sortableDataFormat.IntArray -> 
            let intTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> 
                            SortableTestModel.makeSortableTest 
                                (UMX.tag<sortableTestId> (Guid.NewGuid()))
                                model 
                                sortableDataType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Ints it -> it
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
            sortableTestSet.Ints (sortableIntTestSet.create id intTests)

        | _ -> failwith "Unsupported sortableDataType for SortableTestModelSet"