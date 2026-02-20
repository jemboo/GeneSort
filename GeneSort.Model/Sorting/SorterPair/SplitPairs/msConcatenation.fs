namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Sorting.Sorter

type msConcatenation = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          prefix: sorterModel
          suffix: sorterModel } 
    with
    /// Creates an msConcatenation instance with the specified prefix and suffix sorter models.
    /// The ID is deterministically calculated from the prefix and suffix IDs.
    /// Throws an exception if the models have mismatched sorting widths.
    static member create 
            (prefix: sorterModel)
            (suffix: sorterModel) : msConcatenation =
        
        let prefixWidth = SorterModel.getSortingWidth prefix
        let suffixWidth = SorterModel.getSortingWidth suffix
        
        if prefixWidth <> suffixWidth then
            failwith $"Prefix and suffix must have the same sortingWidth. Prefix: {%prefixWidth}, Suffix: {%suffixWidth}"
        
        let id =
            [
                "Concatenation" :> obj
                %SorterModel.getId prefix :> obj
                %SorterModel.getId suffix :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>
        
        { id = id
          sortingWidth = prefixWidth
          prefix = prefix
          suffix = suffix }
    
    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.Prefix with get () = this.prefix
    member this.Suffix with get () = this.suffix



module MsConcatenation =
    
    let getChildIds (model: msConcatenation) : Guid<sorterModelID> array =
        [| 
            SorterModel.getId model.Prefix
            SorterModel.getId model.Suffix
        |]
    
    let getCeLength (model: msConcatenation) : int<ceLength> =
        let prefixLength = SorterModel.getCeLength model.Prefix
        let suffixLength = SorterModel.getCeLength model.Suffix
        UMX.tag<ceLength> (%prefixLength + %suffixLength)
    
    let hasChild (id: Guid<sorterModelID>) (model: msConcatenation) : bool =
        model |> getChildIds |> Array.exists (fun childId -> %childId = %id)

    let makeSorter (concat: msConcatenation) : sorter =
        let prefixSorter, _ = SorterModel.makeSorter concat.Prefix
        let suffixSorter, _ = SorterModel.makeSorter concat.Suffix
        let sorterId = %concat.Id |> UMX.tag<sorterId>
        Sorter.concatSorters prefixSorter suffixSorter sorterId
