namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

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

    member this.makeSortableTestSet (sortableArrayDataType:sortableArrayDataType) : sortableTestSet =
        let id = (%this.id) |> UMX.tag<sortableTestSetId>

        let sortableTestArray = 
                this.SorterTestModels
                |> Array.map(fun model -> SortableTestModel.makeSortableTests model sortableArrayDataType)

        //match sortableArrayDataType with
        //| sortableArrayDataType.Bools ->
        //    (sortableBoolTestSet.create id sortableTestArray) |> sortableTestSet.Bools
        //| sortableArrayDataType.Ints -> 


        match sortableArrayDataType with
        | sortableArrayDataType.Bools -> 
            let boolTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SortableTestModel.makeSortableTests model sortableArrayDataType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Bools bt -> bt
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
            sortableTestSet.Bools (sortableBoolTestSet.create id boolTests)

        | sortableArrayDataType.Ints -> 
            let intTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SortableTestModel.makeSortableTests model sortableArrayDataType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Ints it -> it
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
            sortableTestSet.Ints (sortableIntTestSet.create id intTests)