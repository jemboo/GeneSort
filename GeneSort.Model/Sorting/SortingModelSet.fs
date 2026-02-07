namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type sortingModelSet =
      private
        { id : Guid<sortingModelSetID>
          sorterModels : Map<Guid<sortingModelID>, sorterModel>
          ceLength: int<ceLength> }
    with
    static member create (id : Guid<sortingModelSetID>) (ceLength: int<ceLength>) (sorterModels : sorterModel[]) : sortingModelSet =
        if sorterModels.Length < 1 then
            failwith "Must have at least 1 SorterModel"
        
        if sorterModels |> Array.exists (fun sm -> (SorterModel.getCeLength sm ) <> ceLength) then
            failwith "All SorterModels must have the same CeLength"
        
        // Create map from sorterModels, keyed by their ID
        let modelMap = 
            sorterModels 
            |> Array.map (fun sm -> (SorterModel.getId sm, sm))
            |> Map.ofArray
        
        // Check that no duplicates were lost when creating the map
        if modelMap.Count <> sorterModels.Length then
            failwith "All SorterModels must have unique IDs"
        
        { id = id; ceLength = ceLength; sorterModels = modelMap }
    
    member this.Id with get() = this.id
    member this.CeLength with get() = this.ceLength
    member this.SorterModels with get() = this.sorterModels |> Map.toArray |> Array.map snd

    member this.tryFind (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sorterModel option =
        modelSet.sorterModels |> Map.tryFind id
    
    member this.find (id: Guid<sortingModelID>) (modelSet: sortingModelSet) : sorterModel =
        match modelSet.sorterModels |> Map.tryFind id with
        | Some model -> model
        | None -> failwithf "SorterModel with ID %A not found" id


module SortingModelSet =

    let makeSorterSet (modelSet: sortingModelSet) : sorterSet =
        let sorters = modelSet.SorterModels 
                        |> Array.map (fun sm -> sm |> SorterModel.makeSorter)
        sorterSet.create (%modelSet.Id |> UMX.tag<sorterSetId>) modelSet.CeLength sorters