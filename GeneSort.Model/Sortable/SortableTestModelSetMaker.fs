namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Core

type sortableTestModelSetMaker =
    private
        { 
          id : Guid<sorterTestModelSetMakerID>
          sorterTestModelGen : sortableTestModelGen
          firstIndex : int<sorterTestModelCount>
          count : int<sorterTestModelCount>
        }
    with
    static member create 
                (sorterTestModelGen: sortableTestModelGen) 
                (firstIndex: int<sorterTestModelCount>) 
                (count: int<sorterTestModelCount>) : sortableTestModelSetMaker =
        let id = 
            // Generate a unique ID based on the SorterModelMaker and indices
            GuidUtils.guidFromObjs [
                    sorterTestModelGen :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sorterTestModelSetMakerID>

        { id = id; sorterTestModelGen = sorterTestModelGen; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SorterTestModelGen with get() = this.sorterTestModelGen
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MakeSortableTestModelSet: sortableTestModelSet =

        let id = (%this.id) |> UMX.tag<sorterTestModelSetID>

        let sorterTestModels = 
                    this.SorterTestModelGen 
                    |> SortableTestModelGen.makeSorterTestModels %this.firstIndex %this.count
                    |> Seq.toArray

        { id = id; sorterTestModels = sorterTestModels }