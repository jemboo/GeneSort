namespace GeneSort.Model.Sorter

open FSharp.UMX


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

   let getChildIds (model: sortingModelPair) : Guid<sorterModelID> array =
        failwith "Not implemented yet"


   let hasChild (id: Guid<sorterModelID>) (sortingModelPair:sortingModelPair) : bool =
       sortingModelPair |> getChildIds |> Array.exists (fun childId -> %childId = %id)
        

