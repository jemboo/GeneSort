namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorter

type SorterModelSet =
    { Id : Guid<sorterModelSetID>
      SorterModels : ISorterModel[] }

module SorterModelSet =

    let makeSorterSet (modelSet: SorterModelSet) : SorterSet =
        let sorters = modelSet.SorterModels |> Array.map (fun sm -> sm.MakeSorter())
        SorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters