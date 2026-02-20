namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingModelSet =
      private
        { 
          id : Guid<sortingModelSetID>
          sortingModels : Map<Guid<sortingModelID>, sortingModel>
        }
    with
    static member create 
            (id : Guid<sortingModelSetID>) 
            (sortingModels : sortingModel[]) : sortingModelSet =

        // Create map from sorterModels, keyed by their ID
        let modelMap = 
            sortingModels 
            |> Array.map (fun sm -> (SortingModel.getId sm, sm))
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
            // Assuming all models in the set have the same sorting width, return the sorting width of the first model
            this.SortingModels.[0] |> SortingModel.getSortingWidth
    member this.StageLength with get() = 
        if this.Count = 0 then
            failwith "Cannot determine stage length of an empty sorting model set"
        else
            // Assuming all models in the set have the same stage length, return the stage length of the first model
            this.SortingModels.[0] |> SortingModel.getStageLength
    member this.tryFind (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sortingModel option =
        modelSet.sortingModels |> Map.tryFind id
    
    member this.find (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sortingModel =
        match modelSet.sortingModels |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id


module SortingModelSet =

    let makeSorterSet (modelSet: sortingModelSet) : sorterSet * Map<Guid<sorterId>, sortingModelTag> =
        // Collect all sorters with their tags
        let sortersWithTags = 
            modelSet.SortingModels 
            |> Array.collect (fun sm -> sm |> SortingModel.makeSorters)
        
        // Extract just the sorters for the sorterSet
        let sorters = sortersWithTags |> Array.map fst
        
        // Create the map of sorterId -> sorterModelTag
        let sorterTagMap = 
            sortersWithTags 
            |> Array.map (fun (sorter, tag) -> (sorter.SorterId, tag))
            |> Map.ofArray
        
        // Validate that all sorterIds are unique
        if sorterTagMap.Count <> sortersWithTags.Length then
            failwith "All sorters must have unique IDs"
        
        let sorterSet = sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters
        
        (sorterSet, sorterTagMap)