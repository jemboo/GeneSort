namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingSet =
      private
        { 
          id : Guid<sortingSetId>
          sortingMap : Map<Guid<sortingId>, sorting>
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
        
        { id = id; sortingMap = modelMap }
    
    member this.Count with get() = this.sortingMap.Count
    member this.Id with get() = this.id
    member this.Sortings with get() = this.sortingMap |> Map.toArray |> Array.map snd
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
    
    member this.find (id: Guid<sortingId>) : sorting =
        match this.sortingMap |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id

    member this.SorterIdsWithSortingTags with get () : (Guid<sorterId> * modelSetTag) [] =
        this.Sortings |> Array.collect(Sorting.getSorterIdsWithSortingTags)




module SortingSet =

    let makeSorterSet (sortingSet: sortingSet) : sorterSet =
        let sorters = 
            sortingSet.Sortings 
            |> Array.collect (fun sm -> sm |> Sorting.makeSorters)
        
        sorterSet.create (%sortingSet.Id |> UMX.tag<sorterSetId>) sorters

    // Merges two sorting sets by combining their sortings into a new set with the same ID as 
    // the target set. The source set is not modified.
    let merge (source: sortingSet) (target: sortingSet) : sortingSet =
        let combinedSortings = Array.append target.Sortings source.Sortings
        sortingSet.create target.Id combinedSortings
