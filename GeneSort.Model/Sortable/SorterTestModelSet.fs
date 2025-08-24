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
