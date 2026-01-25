namespace GeneSort.Model.Sortable

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

    member this.makeSortableTestSet (sortableDataType:sortableDataFormat) : sortableTestSet =
        let id = (%this.id) |> UMX.tag<sortableTestSetId>

        let sortableTestArray = 
                this.SorterTestModels
                |> Array.map(fun model -> SortableTestModel.makeSortableTests model sortableDataType)

        //match sortableDataType with
        //| sortableDataType.Bools ->
        //    (sortableBoolTestSet.create id sortableTestArray) |> sortableTestSet.Bools
        //| sortableDataType.Ints -> 


        match sortableDataType with
        | sortableDataFormat.BoolArray -> 
            let boolTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SortableTestModel.makeSortableTests model sortableDataType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTest.Bools bt -> bt
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
            sortableTestSet.Bools (sortableBoolTestSet.create id boolTests)

        | sortableDataFormat.IntArray -> 
            let intTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SortableTestModel.makeSortableTests model sortableDataType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTest.Ints it -> it
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
            sortableTestSet.Ints (sortableIntTestSet.create id intTests)