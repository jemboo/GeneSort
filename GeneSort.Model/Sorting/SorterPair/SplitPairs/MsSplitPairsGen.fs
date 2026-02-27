namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

type msSplitPairsGen = 
    private 
        { id: Guid<sorterPairModelMakerId>
          sortingWidth: int<sortingWidth>
          firstPrefixMaker: sorterModelMaker
          firstSuffixMaker: sorterModelMaker
          secondPrefixMaker: sorterModelMaker
          secondSuffixMaker: sorterModelMaker} 
    with
    /// Creates an msSplitPairsGen instance with the specified sorting width and four sorter model makers.
    /// The ID is deterministically calculated from the constituent maker IDs.
    /// Throws an exception if:
    /// - width is less than 1
    /// - any sorter model maker has a mismatched sorting width
    /// - the two prefix makers don't have the same ceLength
    /// - the two suffix makers don't have the same ceLength
    static member create 
            (sortingWidth: int<sortingWidth>)
            (firstPrefixMaker: sorterModelMaker)
            (firstSuffixMaker: sorterModelMaker)
            (secondPrefixMaker: sorterModelMaker)
            (secondSuffixMaker: sorterModelMaker) : msSplitPairsGen =
        
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        
        // Validate that all sorter model makers have the same sorting width
        let makers = [firstPrefixMaker; firstSuffixMaker; secondPrefixMaker; secondSuffixMaker]
        let mismatchedMakers = 
            makers 
            |> List.filter (fun maker -> SorterModelMaker.getSortingWidth maker <> sortingWidth)
        
        if not (List.isEmpty mismatchedMakers) then
            failwith $"All sorter model makers must have sortingWidth {%sortingWidth}"
        
        // Validate that the two prefix makers have the same ceLength
        let firstPrefixCeLength = SorterModelMaker.getCeLength firstPrefixMaker
        let secondPrefixCeLength = SorterModelMaker.getCeLength secondPrefixMaker
        
        if firstPrefixCeLength <> secondPrefixCeLength then
            failwith $"Both prefix makers must have the same ceLength. FirstPrefix: {%firstPrefixCeLength}, SecondPrefix: {%secondPrefixCeLength}"
        
        // Validate that the two suffix makers have the same ceLength
        let firstSuffixCeLength = SorterModelMaker.getCeLength firstSuffixMaker
        let secondSuffixCeLength = SorterModelMaker.getCeLength secondSuffixMaker
        
        if firstSuffixCeLength <> secondSuffixCeLength then
            failwith $"Both suffix makers must have the same ceLength. FirstSuffix: {%firstSuffixCeLength}, SecondSuffix: {%secondSuffixCeLength}"
        
        // Calculate ID deterministically from the constituent maker IDs
        let id =
            [
                "msSplitPairsGen" :> obj
                sortingWidth :> obj
                %SorterModelMaker.getId firstPrefixMaker :> obj
                %SorterModelMaker.getId firstSuffixMaker :> obj
                %SorterModelMaker.getId secondPrefixMaker :> obj
                %SorterModelMaker.getId secondSuffixMaker :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterPairModelMakerId>
        
        { id = id
          sortingWidth = sortingWidth
          firstPrefixMaker = firstPrefixMaker
          firstSuffixMaker = firstSuffixMaker
          secondPrefixMaker = secondPrefixMaker
          secondSuffixMaker = secondSuffixMaker }
    
    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.FirstPrefixMaker with get () = this.firstPrefixMaker
    member this.FirstSuffixMaker with get () = this.firstSuffixMaker
    member this.SecondPrefixMaker with get () = this.secondPrefixMaker
    member this.SecondSuffixMaker with get () = this.secondSuffixMaker


module MsSplitPairsGen =

    let getSortingWidth (gen: msSplitPairsGen) : int<sortingWidth> =
        gen.SortingWidth
    
    let getPrefixCeLength (gen: msSplitPairsGen) : int<ceLength> =
        SorterModelMaker.getCeLength gen.FirstPrefixMaker
    
    let getSuffixCeLength (gen: msSplitPairsGen) : int<ceLength> =
        SorterModelMaker.getCeLength gen.FirstSuffixMaker

    let getCeLength (gen: msSplitPairsGen) : int<ceLength> =
        getPrefixCeLength gen + getSuffixCeLength gen


    /// Generates an msSplitPairs instance by making sorter models from each maker
    let makeMsSplitPairs (index: int) (gen: msSplitPairsGen) : msSplitPairs =

        let firstPrefix = SorterModelMaker.makeSorterModel index gen.FirstPrefixMaker 
        let firstSuffix = SorterModelMaker.makeSorterModel index gen.FirstSuffixMaker
        let secondPrefix = SorterModelMaker.makeSorterModel index gen.SecondPrefixMaker
        let secondSuffix = SorterModelMaker.makeSorterModel index gen.SecondSuffixMaker
        
        // Calculate ID for the msSplitPairs from the generator ID and seed
        let splitPairsId =
            [
                %gen.Id :> obj
                index :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>
        
        msSplitPairs.create
            splitPairsId
            gen.SortingWidth
            firstPrefix
            firstSuffix
            secondPrefix
            secondSuffix


    let makeSorterIdsWithTags (index: int) (gen: msSplitPairsGen) 
                                    : (Guid<sorterId> * modelTag) [] =
        let splitPairs = makeMsSplitPairs index gen
        splitPairs |> MsSplitPairs.getSorterIdsWithTags