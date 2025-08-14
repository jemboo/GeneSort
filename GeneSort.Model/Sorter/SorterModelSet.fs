namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorter

type SorterModelSet =
    { Id : Guid<sorterModelSetID>
      SorterModels : SorterModel[] }

module SorterModelSet =
    let makeSorterSet (modelSet: SorterModelSet) : SorterSet =
        let sorters = modelSet.SorterModels 
                        |> Array.map (fun sm -> sm |> SorterModel.makeSorter)
        SorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters