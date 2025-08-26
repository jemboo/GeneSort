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
    member this.makeSorterTestSet (sortableArrayType:SortableArrayType) : SorterTestSet =
        let id = (%this.id) |> UMX.tag<sorterTestSetId>
        let sorterTests = 
                    this.SorterTestModels 
                    |> Array.map (fun model -> SorterTestModel.makeSorterTest model sortableArrayType)
        SorterTestSet.create id sorterTests


    //member this.MakeSorterTestSet (sortableArrayType:SortableArrayType) : SorterTestSet =
    //    let id = (%this.id) |> UMX.tag<sorterTestSetId>
    //    let sorterTests = 
    //                this.SorterTestModels 
    //                |> Array.map (fun model -> SorterTestModel.makeSorterTest model sortableArrayType)
    //    match sortableArrayType with
    //    | SortableArrayType.Bools -> 
    //        let boolTests = sorterTests |> Array.map (fun st -> 
    //            match st with
    //            | SorterTest.Bools bt -> bt
    //            | _ -> failwith "Inconsistent SorterTestModelSet: expected Bools")
    //        SorterTestSet.Bools (SorterBoolTestSet.create id boolTests)
    //    | SortableArrayType.Ints ->
    //        let intTests = sorterTests |> Array.map (fun st -> 
    //            match st with
    //            | SorterTest.Ints it -> it
    //            | _ -> failwith "Inconsistent SorterTestModelSet: expected Ints")
    //        SorterTestSet.Ints (SorterIntTestSet.create id intTests)