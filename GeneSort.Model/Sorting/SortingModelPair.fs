namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting


type sortingModelPair =
    private 
        { id: Guid<sortingModelID>
          sorterPairModel: sorterPairModel } 

    static member create 
            (id: Guid<sortingModelID>) 
            (sorterPairModel: sorterPairModel) : sortingModelPair =
       { id = id; sorterPairModel = sorterPairModel }

    member this.Id with get () = this.id
    member this.SorterPairModel with get () = this.sorterPairModel


module SortingModelPair =

   let getChildSorterIds (model: sortingModelPair) : Guid<sorterId> array =
        failwith "Not implemented yet"

   let hasChild (sorterId: Guid<sorterId>) (sortingModelPair:sortingModelPair) : bool =
       sortingModelPair |> getChildSorterIds |> Array.exists (fun childId -> %childId = %sorterId)
        

