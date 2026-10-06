namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

type sortableTestsModelSet =
    private
        { 
          id : Guid<sorterTestModelSetID>
          sorterTestModels : sortableTestsModel array
        }
    with
    static member create 
                (id : Guid<sorterTestModelSetID>)
                (sorterTestModels : sortableTestsModel array) =

        { id = id; sorterTestModels = sorterTestModels; }

    member this.Id with get() = this.id
    member this.SorterTestModels with get() = this.sorterTestModels

    member this.makeSortableTestsSet 
                    (sorterTestId: Guid<sortableTestsId>)
                    (sortableDataType:sortableDataFormat) : sortableTestsSet =
        let id = (%this.id) |> UMX.tag<sortableTestsSetId>

        let sortableTestsArray = 
                this.SorterTestModels
                |> Array.map(fun model -> SortableTestsModel.makeSortableTests sorterTestId model sortableDataType)

        //match sortableDataType with
        //| sortableDataType.Bools ->
        //    (sortableBoolTestsSet.create id sortableTestsArray) |> sortableTestsSet.Bools
        //| sortableDataType.Ints -> 


        match sortableDataType with
        | sortableDataFormat.BoolArray -> 
            let boolTests = 
                this.SorterTestModels 
                |> Array.map (fun model ->
                                    SortableTestsModel.makeSortableTests 
                                        (UMX.tag<sortableTestsId> (Guid.NewGuid()))
                                        model 
                                        sortableDataType )
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Bools bt -> bt
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
            sortableTestsSet.Bools (sortableBoolTestsSet.create id boolTests)

        | sortableDataFormat.IntArray -> 
            let intTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> 
                            SortableTestsModel.makeSortableTests 
                                (UMX.tag<sortableTestsId> (Guid.NewGuid()))
                                model 
                                sortableDataType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Ints it -> it
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
            sortableTestsSet.Ints (sortableIntTestsSet.create id intTests)