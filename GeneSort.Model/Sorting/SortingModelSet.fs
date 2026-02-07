namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingModelSet =
      private
        { id : Guid<sortingModelSetID>
          sortingModels : Map<Guid<sortingModelID>, sortingModel>
          ceLength: int<ceLength> }
    with
    static member create (id : Guid<sortingModelSetID>) (ceLength: int<ceLength>) (sortingModels : sortingModel[]) : sortingModelSet =
        if sortingModels.Length < 1 then
            failwith "Must have at least 1 SorterModel"
        
        if sortingModels |> Array.exists (fun sm -> (SortingModel.getCeLength sm ) <> ceLength) then
            failwith "All SorterModels must have the same CeLength"
        
        // Create map from sorterModels, keyed by their ID
        let modelMap = 
            sortingModels 
            |> Array.map (fun sm -> (SortingModel.getId sm, sm))
            |> Map.ofArray
        
        // Check that no duplicates were lost when creating the map
        if modelMap.Count <> sortingModels.Length then
            failwith "All SorterModels must have unique IDs"
        
        { id = id; ceLength = ceLength; sortingModels = modelMap }
    
    member this.Id with get() = this.id
    member this.CeLength with get() = this.ceLength
    member this.SorterModels with get() = this.sortingModels |> Map.toArray |> Array.map snd

    member this.tryFind (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sortingModel option =
        modelSet.sortingModels |> Map.tryFind id
    
    member this.find (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sortingModel =
        match modelSet.sortingModels |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id


module SortingModelSet =

    let makeSorterSet (modelSet: sortingModelSet) : sorterSet =
        let sorters = modelSet.SorterModels 
                        |> Array.map (fun sm -> sm |> SortingModel.makeSorter)
        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) modelSet.CeLength sorters