namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable

type sorterModelSet =
    private 
        { id : Guid<sorterModelSetID>
          sorterModels : sorterModel[] 
          ceLength: int<ceLength>  }
    with
    static member create 
            (id : Guid<sorterModelSetID>) 
            (ceLength: int<ceLength>)
            (sorterModels : sorterModel[]) : sorterModelSet =
        if sorterModels.Length < 1 then
            failwith "Must have at least 1 SorterModel"
        if sorterModels |> Array.exists (fun sm -> (SorterModel.getCeLength sm ) <> ceLength) then
            failwith "All SorterModels must have the same CeLength"
        else
            { id = id; ceLength = ceLength; sorterModels = sorterModels }

    member this.Id with get() = this.id
    member this.CeLength with get() = this.ceLength
    member this.SorterModels with get() = this.sorterModels



module SorterModelSet =

    let makeSorterSet (modelSet: sorterModelSet) : sorterSet =
        let sorters = modelSet.SorterModels 
                        |> Array.map (fun sm -> sm |> SorterModel.makeSorter)
        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) modelSet.CeLength sorters