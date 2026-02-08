namespace GeneSort.Model.Sorter

open FSharp.UMX


type sortingModelSingle =
    private 
        { id: Guid<sortingModelID>
          sorterModel: sorterModel } 

    static member create 
            (id: Guid<sortingModelID>)
            (sorterModel: sorterModel) : sortingModelSingle =
       { id = id; sorterModel = sorterModel }

    member this.Id with get () = this.id
    member this.SorterModel with get () = this.sorterModel



module SortingModelSingle = ()

