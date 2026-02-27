namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

type msSplitPairsMutator = 
    private 
        { id: Guid<sorterPairModelMutatorId>
          sortingWidth: int<sortingWidth>
          firstPrefixMutator: sorterModelMutator
          firstSuffixMutator: sorterModelMutator
          secondPrefixMutator: sorterModelMutator
          secondSuffixMutator: sorterModelMutator} 
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
            (firstPrefixMutator: sorterModelMutator)
            (firstSuffixMutator: sorterModelMutator)
            (secondPrefixMutator: sorterModelMutator)
            (secondSuffixMutator: sorterModelMutator) : msSplitPairsMutator =
        
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        
        // Validate that all sorter model makers have the same sorting width
        let makers = [firstPrefixMutator; firstSuffixMutator; secondPrefixMutator; secondSuffixMutator]
        let mismatchedMakers = 
            makers 
            |> List.filter (fun maker -> SorterModelMutator.getSortingWidth maker <> sortingWidth)
        
        if not (List.isEmpty mismatchedMakers) then
            failwith $"All sorter model makers must have sortingWidth {%sortingWidth}"
        
        // Validate that the two prefix makers have the same ceLength
        let firstPrefixCeLength = SorterModelMutator.getCeLength firstPrefixMutator
        let secondPrefixCeLength = SorterModelMutator.getCeLength secondPrefixMutator
        
        if firstPrefixCeLength <> secondPrefixCeLength then
            failwith $"Both prefix makers must have the same ceLength. FirstPrefix: {%firstPrefixCeLength}, SecondPrefix: {%secondPrefixCeLength}"
        
        // Validate that the two suffix makers have the same ceLength
        let firstSuffixCeLength = SorterModelMutator.getCeLength firstSuffixMutator
        let secondSuffixCeLength = SorterModelMutator.getCeLength secondSuffixMutator
        
        if firstSuffixCeLength <> secondSuffixCeLength then
            failwith $"Both suffix makers must have the same ceLength. FirstSuffix: {%firstSuffixCeLength}, SecondSuffix: {%secondSuffixCeLength}"
        
        // Calculate ID deterministically from the constituent maker IDs
        let id =
            [
                "msSplitPairsGen" :> obj
                sortingWidth :> obj
                %SorterModelMutator.getId firstPrefixMutator :> obj
                %SorterModelMutator.getId firstSuffixMutator :> obj
                %SorterModelMutator.getId secondPrefixMutator :> obj
                %SorterModelMutator.getId secondSuffixMutator :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterPairModelMutatorId>
        
        { id = id
          sortingWidth = sortingWidth
          firstPrefixMutator = firstPrefixMutator
          firstSuffixMutator = firstSuffixMutator
          secondPrefixMutator = secondPrefixMutator
          secondSuffixMutator = secondSuffixMutator }
    
    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.FirstPrefixMutator with get () = this.firstPrefixMutator
    member this.FirstSuffixMutator with get () = this.firstSuffixMutator
    member this.SecondPrefixMutator with get () = this.secondPrefixMutator
    member this.SecondSuffixMutator with get () = this.secondSuffixMutator
    member this.SortingSeedId with get() : Guid<sortingId> = failwith "not implemented."

module MsSplitPairsMutator =

    let getSortingWidth (gen: msSplitPairsMutator) : int<sortingWidth> =
        gen.SortingWidth
    
    let getPrefixCeLength (gen: msSplitPairsMutator) : int<ceLength> =
        SorterModelMutator.getCeLength gen.FirstPrefixMutator
    
    let getSuffixCeLength (gen: msSplitPairsMutator) : int<ceLength> =
        SorterModelMutator.getCeLength gen.FirstSuffixMutator

    let getCeLength (gen: msSplitPairsMutator) : int<ceLength> =
        getPrefixCeLength gen + getSuffixCeLength gen

    /// Generates an msSplitPairs instance by making sorter models from each maker
    let makeMsSplitPairs 
                (index: int) 
                (gen: msSplitPairsMutator) : msSplitPairs =
        let firstPrefix = SorterModelMutator.makeSorterModel index gen.FirstPrefixMutator 
        let firstSuffix = SorterModelMutator.makeSorterModel index gen.FirstSuffixMutator
        let secondPrefix = SorterModelMutator.makeSorterModel index gen.SecondPrefixMutator
        let secondSuffix = SorterModelMutator.makeSorterModel index gen.SecondSuffixMutator
        
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


    let makeSorterIdsWithTags (index: int) (mutator: msSplitPairsMutator) 
                                    : (Guid<sorterId> * modelTag) [] =
        let splitPairs = makeMsSplitPairs index mutator
        splitPairs |> MsSplitPairs.getSorterIdsWithTags