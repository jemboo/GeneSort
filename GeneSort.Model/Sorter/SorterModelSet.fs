namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable

type SorterModelSet =
    { Id : Guid<sorterModelSetID>
      SorterModels : SorterModel[] }

module SorterModelSet =
    let makeSorterSet (modelSet: SorterModelSet) : sorterSet =
        let sorters = modelSet.SorterModels 
                        |> Array.map (fun sm -> sm |> SorterModel.makeSorter)
        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters