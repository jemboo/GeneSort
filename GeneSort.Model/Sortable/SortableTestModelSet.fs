namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

type sortableTestModelSet =
    private
        { 
          id : Guid<sorterTestModelSetID>
          sorterTestModels : SorterTestModel array
        }
    with
    static member create 
                (id : Guid<sorterTestModelSetID>)
                (sorterTestModels : SorterTestModel array) =

        { id = id; sorterTestModels = sorterTestModels; }

    member this.Id with get() = this.id
    member this.SorterTestModels with get() = this.sorterTestModels

    member this.makeSortableTestSet (sortableArrayType:SortableArrayType) : sortableTestSet =
        let id = (%this.id) |> UMX.tag<sortableTestSetId>
        match sortableArrayType with
        | SortableArrayType.Bools -> 
            let boolTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SorterTestModel.makeSorterTest model sortableArrayType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Bools bt -> bt
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
            sortableTestSet.Bools (sortableBoolTestSet.create id boolTests)

        | SortableArrayType.Ints -> 
            let intTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SorterTestModel.makeSorterTest model sortableArrayType)
                |> Array.map (fun st -> 
                    match st with
                    | sortableTests.Ints it -> it
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
            sortableTestSet.Ints (sortableIntTestSet.create id intTests)