namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Sorting.Sorter

type msSplitPairs = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          firstPrefix: sorterModel
          firstSuffix: sorterModel
          secondPrefix: sorterModel
          secondSuffix: sorterModel} 
    with
    /// Creates an msSplitPairs instance with the specified ID, sorting width, and four sorter models.
    /// Throws an exception if:
    /// - width is less than 1
    /// - any sorter model has a mismatched sorting width
    /// - the two prefixes don't have the same ceLength
    /// - the two suffixes don't have the same ceLength
    static member create 
            (id: Guid<sorterModelID>) 
            (sortingWidth: int<sortingWidth>)
            (firstPrefix: sorterModel)
            (firstSuffix: sorterModel)
            (secondPrefix: sorterModel)
            (secondSuffix: sorterModel) : msSplitPairs =
        
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        
        // Validate that all sorter models have the same sorting width
        let models = [firstPrefix; firstSuffix; secondPrefix; secondSuffix]
        let mismatchedModels = 
            models 
            |> List.filter (fun sm -> SorterModel.getSortingWidth sm <> sortingWidth)
        
        if not (List.isEmpty mismatchedModels) then
            failwith $"All sorter models must have sortingWidth {%sortingWidth}"
        
        // Validate that the two prefixes have the same ceLength
        let firstPrefixCeLength = SorterModel.getCeLength firstPrefix
        let secondPrefixCeLength = SorterModel.getCeLength secondPrefix
        
        if firstPrefixCeLength <> secondPrefixCeLength then
            failwith $"Both prefixes must have the same ceLength. FirstPrefix: {%firstPrefixCeLength}, SecondPrefix: {%secondPrefixCeLength}"
        
        // Validate that the two suffixes have the same ceLength
        let firstSuffixCeLength = SorterModel.getCeLength firstSuffix
        let secondSuffixCeLength = SorterModel.getCeLength secondSuffix
        
        if firstSuffixCeLength <> secondSuffixCeLength then
            failwith $"Both suffixes must have the same ceLength. FirstSuffix: {%firstSuffixCeLength}, SecondSuffix: {%secondSuffixCeLength}"
        
        { id = id
          sortingWidth = sortingWidth
          firstPrefix = firstPrefix
          firstSuffix = firstSuffix
          secondPrefix = secondPrefix
          secondSuffix = secondSuffix }
    
    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.FirstPrefix with get () = this.firstPrefix
    member this.FirstSuffix with get () = this.firstSuffix
    member this.SecondPrefix with get () = this.secondPrefix
    member this.SecondSuffix with get () = this.secondSuffix



module MsSplitPairs =
    
    let getCeLength (model: msSplitPairs) : int<ceLength> =
        (SorterModel.getCeLength model.FirstPrefix) + (SorterModel.getCeLength model.FirstSuffix)

    let getStageLength (model: msSplitPairs) : int<stageLength> =
        (SorterModel.getStageLength model.FirstPrefix) + (SorterModel.getStageLength model.FirstSuffix)

    let getPrefixCeLength (model: msSplitPairs) : int<ceLength> =
        SorterModel.getCeLength model.FirstPrefix
    
    let getSuffixCeLength (model: msSplitPairs) : int<ceLength> =
        SorterModel.getCeLength model.FirstSuffix
    
    /// Creates concatenation of FirstPrefix and FirstSuffix
    let makeFirstFirst (model: msSplitPairs) : msConcatenation =
        msConcatenation.create model.FirstPrefix model.FirstSuffix
    
    /// Creates concatenation of FirstPrefix and SecondSuffix
    let makeFirstSecond (model: msSplitPairs) : msConcatenation =
        msConcatenation.create model.FirstPrefix model.SecondSuffix
    
    /// Creates concatenation of SecondPrefix and FirstSuffix
    let makeSecondFirst (model: msSplitPairs) : msConcatenation =
        msConcatenation.create model.SecondPrefix model.FirstSuffix
    
    /// Creates concatenation of SecondPrefix and SecondSuffix
    let makeSecondSecond (model: msSplitPairs) : msConcatenation =
        msConcatenation.create model.SecondPrefix model.SecondSuffix
    
    /// Creates all four possible concatenations
    let makeAllConcatenations (model: msSplitPairs) : (msConcatenation * sortingModelTag) [] =
        [|
            (makeFirstFirst model, modelTag.SplitPair splitJoin.First_First)
            (makeFirstSecond model, modelTag.SplitPair splitJoin.First_Second)
            (makeSecondFirst model, modelTag.SplitPair splitJoin.Second_First)
            (makeSecondSecond model, modelTag.SplitPair splitJoin.Second_Second)
        |] |> Array.map (fun (concat, tag) -> (concat, SortingModelTag.create %model.Id tag))

    let makeAllSorters (model: msSplitPairs) : (sorter * sortingModelTag) [] =
        makeAllConcatenations model
        |> Array.map (fun (s,t) ->  (s |> MsConcatenation.makeSorter, t))

    // checks if childId identifies one of the outputs of makeAllSorters
    let isAChildOf (childId: Guid<sorterId>) (sprs: msSplitPairs) : bool =
        makeAllConcatenations sprs
        |> Array.exists (fun (concat, _) -> %concat.Id = %childId)


    let getSorterModelIdForModelTag (model: msSplitPairs) (tag: modelTag) : Guid<sorterModelID> =
        match tag with
        | modelTag.SplitPair splitJoin.First_First -> 
                msConcatenation.createId model.FirstPrefix model.FirstSuffix
        | modelTag.SplitPair splitJoin.First_Second ->
                msConcatenation.createId model.FirstPrefix model.SecondSuffix
        | modelTag.SplitPair splitJoin.Second_First ->
                msConcatenation.createId model.SecondPrefix model.FirstSuffix
        | modelTag.SplitPair splitJoin.Second_Second ->
                msConcatenation.createId model.SecondPrefix model.SecondSuffix
        | _ -> failwith $"Invalid modelTag for msSplitPairs"


    let getSorterModelIdsWithTags (model: msSplitPairs) : (Guid<sorterModelID> * modelTag) [] =
        [|
            (msConcatenation.createId model.FirstPrefix model.FirstSuffix, modelTag.SplitPair splitJoin.First_First);
            (msConcatenation.createId model.FirstPrefix model.SecondSuffix, modelTag.SplitPair splitJoin.First_Second);
            (msConcatenation.createId model.SecondPrefix model.FirstSuffix, modelTag.SplitPair splitJoin.Second_First);
            (msConcatenation.createId model.SecondPrefix model.SecondSuffix, modelTag.SplitPair splitJoin.Second_Second);
        |]