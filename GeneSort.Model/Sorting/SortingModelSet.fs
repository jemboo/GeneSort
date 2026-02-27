namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingModelSet =
      private
        { 
          id : Guid<sortingModelSetId>
          sortingModels : Map<Guid<sortingModelId>, sorting>
        }
    with
    static member create 
            (id : Guid<sortingModelSetId>) 
            (sortingModels : sorting[]) : sortingModelSet =

        // Create map from sorterModels, keyed by their ID
        let modelMap = 
            sortingModels 
            |> Array.map (fun sm -> (Sorting.getId sm, sm))
            |> Map.ofArray
        
        // Check that no duplicates were lost when creating the map
        if modelMap.Count <> sortingModels.Length then
            failwith "All SorterModels must have unique IDs"
        
        { id = id; sortingModels = modelMap }
    
    member this.Count with get() = this.sortingModels.Count
    member this.Id with get() = this.id
    member this.SortingModels with get() = this.sortingModels |> Map.toArray |> Array.map snd
    member this.SortingWidth with get() = 
        if this.Count = 0 then
            failwith "Cannot determine sorting width of an empty sorting model set"
        else
            // Assuming all models in the set have the same sorting width, 
            // return the sorting width of the first model
            this.SortingModels.[0] |> Sorting.getSortingWidth
    member this.StageLength with get() = 
        if this.Count = 0 then
            failwith "Cannot determine stage length of an empty sorting model set"
        else
            // Assuming all models in the set have the same stage length, 
            // return the stage length of the first model
            this.SortingModels.[0] |> Sorting.getStageLength
    member this.tryFind (id: Guid<sortingModelId>) (modelSet: sortingModelSet) : sorting option =
        modelSet.sortingModels |> Map.tryFind id
    
    member this.find (id: Guid<sortingModelId>) (modelSet: sortingModelSet) : sorting =
        match modelSet.sortingModels |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id


module SortingModelSet =

    let makeSorterSet (modelSet: sortingModelSet) : sorterSet =
        // Collect all sorters with their tags
        let sorters = 
            modelSet.SortingModels 
            |> Array.collect (fun sm -> sm |> Sorting.makeSorters)
        
        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters
