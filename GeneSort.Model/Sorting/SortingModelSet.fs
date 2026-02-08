namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingModelSet =
      private
        { id : Guid<sortingModelSetID>
          sortingModels : Map<Guid<sortingModelID>, sortingModel>
        }
    with
    static member create 
            (id : Guid<sortingModelSetID>) 
            (sortingModels : sortingModel[]) : sortingModelSet =
        if sortingModels.Length < 1 then
            failwith "Must have at least 1 SorterModel"
        
        // Create map from sorterModels, keyed by their ID
        let modelMap = 
            sortingModels 
            |> Array.map (fun sm -> (SortingModel.getId sm, sm))
            |> Map.ofArray
        
        // Check that no duplicates were lost when creating the map
        if modelMap.Count <> sortingModels.Length then
            failwith "All SorterModels must have unique IDs"
        
        { id = id; sortingModels = modelMap }
    
    member this.Id with get() = this.id
    member this.SortingModels with get() = this.sortingModels |> Map.toArray |> Array.map snd

    member this.tryFind (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sortingModel option =
        modelSet.sortingModels |> Map.tryFind id
    
    member this.find (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sortingModel =
        match modelSet.sortingModels |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id


module SortingModelSet =

    let makeSorterSet (modelSet: sortingModelSet) : sorterSet =
        let sorters = modelSet.SortingModels 
                        |> Array.collect (fun sm -> sm |> SortingModel.makeSorters)

        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters