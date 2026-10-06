namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Core

type sortableTestsModelSetGen =
    private
        { 
          id : Guid<sorterTestModelSetGenID>
          sorterTestModelGen : sortableTestsModelGen
          firstIndex : int<sorterTestModelCount>
          count : int<sorterTestModelCount>
        }
    with
    static member create 
                (sorterTestModelGen: sortableTestsModelGen) 
                (firstIndex: int<sorterTestModelCount>) 
                (count: int<sorterTestModelCount>) : sortableTestsModelSetGen =
        
        let identityComponents = seq {
            box "sortableTestsModelSetGen"
            box (sorterTestModelGen |> SortableTestsModelGen.getId |> UMX.untag)
            box (firstIndex |> UMX.untag) 
            box (count |> UMX.untag)
        }

        let id = identityComponents |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelSetGenID>

        { id = id; sorterTestModelGen = sorterTestModelGen; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SorterTestModelGen with get() = this.sorterTestModelGen
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count

    member this.MakeSortableTestsModelSet: sortableTestsModelSet =

        let id = (%this.id) |> UMX.tag<sorterTestModelSetID>

        let sorterTestModels = 
                    this.SorterTestModelGen 
                    |> SortableTestsModelGen.makeSorterTestModels %this.firstIndex %this.count
                    |> Seq.toArray

        { id = id; sorterTestModels = sorterTestModels }
