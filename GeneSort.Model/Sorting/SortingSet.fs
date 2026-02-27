namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingSet =
      private
        { 
          id : Guid<sortingSetId>
          sortings : Map<Guid<sortingId>, sorting>
        }
    with
    static member create 
            (id : Guid<sortingSetId>) 
            (sortings : sorting[]) : sortingSet =

        // Create map from sorterModels, keyed by their ID
        let modelMap = 
            sortings 
            |> Array.map (fun sm -> (Sorting.getId sm, sm))
            |> Map.ofArray
        
        // Check that no duplicates were lost when creating the map
        if modelMap.Count <> sortings.Length then
            failwith "All SorterModels must have unique IDs"
        
        { id = id; sortings = modelMap }
    
    member this.Count with get() = this.sortings.Count
    member this.Id with get() = this.id
    member this.Sortings with get() = this.sortings |> Map.toArray |> Array.map snd
    member this.SortingWidth with get() = 
        if this.Count = 0 then
            failwith "Cannot determine sorting width of an empty sorting model set"
        else
            // Assuming all models in the set have the same sorting width, 
            // return the sorting width of the first model
            this.Sortings.[0] |> Sorting.getSortingWidth
    member this.StageLength with get() = 
        if this.Count = 0 then
            failwith "Cannot determine stage length of an empty sorting model set"
        else
            // Assuming all models in the set have the same stage length, 
            // return the stage length of the first model
            this.Sortings.[0] |> Sorting.getStageLength
    member this.tryFind (id: Guid<sortingId>) (modelSet: sortingSet) : sorting option =
        modelSet.sortings |> Map.tryFind id
    
    member this.find (id: Guid<sortingId>) (modelSet: sortingSet) : sorting =
        match modelSet.sortings |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id


module SortingSet =

    let makeSorterSet (modelSet: sortingSet) : sorterSet =
        // Collect all sorters with their tags
        let sorters = 
            modelSet.Sortings 
            |> Array.collect (fun sm -> sm |> Sorting.makeSorters)
        
        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) sorters
