namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core
open GeneSort.Sorter.Sortable

type SorterTestModelSet =
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

    member this.makeSorterTestSet (sortableArrayType:SortableArrayType) : sorterTestSet =
        let id = (%this.id) |> UMX.tag<sorterTestSetId>
        match sortableArrayType with
        | SortableArrayType.Bools -> 
            let boolTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SorterTestModel.makeSorterTest model sortableArrayType)
                |> Array.map (fun st -> 
                    match st with
                    | sorterTests.Bools bt -> bt
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
            sorterTestSet.Bools (sorterBoolTestSet.create id boolTests)

        | SortableArrayType.Ints -> 
            let intTests = 
                this.SorterTestModels 
                |> Array.map (fun model -> SorterTestModel.makeSorterTest model sortableArrayType)
                |> Array.map (fun st -> 
                    match st with
                    | sorterTests.Ints it -> it
                    | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
            sorterTestSet.Ints (sorterIntTestSet.create id intTests)