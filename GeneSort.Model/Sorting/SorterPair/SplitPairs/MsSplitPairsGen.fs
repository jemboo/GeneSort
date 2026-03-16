namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

type msSplitPairsGen = 
    private 
        { id: Guid<sorterPairModelMakerId>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          firstPrefixMaker: sorterModelGen
          firstSuffixMaker: sorterModelGen
          secondPrefixMaker: sorterModelGen
          secondSuffixMaker: sorterModelGen} 
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
            (rngFactory: rngFactory)
            (firstPrefixMaker: sorterModelGen)
            (firstSuffixMaker: sorterModelGen)
            (secondPrefixMaker: sorterModelGen)
            (secondSuffixMaker: sorterModelGen) : msSplitPairsGen =
        
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        
        // Validate that all sorter model makers have the same sorting width
        let makers = [firstPrefixMaker; firstSuffixMaker; secondPrefixMaker; secondSuffixMaker]
        let mismatchedMakers = 
            makers 
            |> List.filter (fun maker -> SorterModelGen.getSortingWidth maker <> sortingWidth)
        
        if not (List.isEmpty mismatchedMakers) then
            failwith $"All sorter model makers must have sortingWidth {%sortingWidth}"
        
        // Validate that the two prefix makers have the same ceLength
        let firstPrefixCeLength = SorterModelGen.getCeLength firstPrefixMaker
        let secondPrefixCeLength = SorterModelGen.getCeLength secondPrefixMaker
        
        if firstPrefixCeLength <> secondPrefixCeLength then
            failwith $"Both prefix makers must have the same ceLength. FirstPrefix: {%firstPrefixCeLength}, SecondPrefix: {%secondPrefixCeLength}"
        
        // Validate that the two suffix makers have the same ceLength
        let firstSuffixCeLength = SorterModelGen.getCeLength firstSuffixMaker
        let secondSuffixCeLength = SorterModelGen.getCeLength secondSuffixMaker
        
        if firstSuffixCeLength <> secondSuffixCeLength then
            failwith $"Both suffix makers must have the same ceLength. FirstSuffix: {%firstSuffixCeLength}, SecondSuffix: {%secondSuffixCeLength}"
        
        // Calculate ID deterministically from the constituent maker IDs
        let id =
            [
                "msSplitPairsGen" :> obj
                sortingWidth :> obj
                %SorterModelGen.getId firstPrefixMaker :> obj
                %SorterModelGen.getId firstSuffixMaker :> obj
                %SorterModelGen.getId secondPrefixMaker :> obj
                %SorterModelGen.getId secondSuffixMaker :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterPairModelMakerId>
        
        { id = id
          sortingWidth = sortingWidth
          rngFactory = rngFactory
          firstPrefixMaker = firstPrefixMaker
          firstSuffixMaker = firstSuffixMaker
          secondPrefixMaker = secondPrefixMaker
          secondSuffixMaker = secondSuffixMaker }
    
    member this.Id with get () = this.id
    member this.RngFactory with get() = this.rngFactory
    member this.SortingWidth with get () = this.sortingWidth
    member this.FirstPrefixMaker with get () = this.firstPrefixMaker
    member this.FirstSuffixMaker with get () = this.firstSuffixMaker
    member this.SecondPrefixMaker with get () = this.secondPrefixMaker
    member this.SecondSuffixMaker with get () = this.secondSuffixMaker


module MsSplitPairsGen =

    let getSortingWidth (gen: msSplitPairsGen) : int<sortingWidth> =
        gen.SortingWidth
    
    let getPrefixCeLength (gen: msSplitPairsGen) : int<ceLength> =
        SorterModelGen.getCeLength gen.FirstPrefixMaker
    
    let getSuffixCeLength (gen: msSplitPairsGen) : int<ceLength> =
        SorterModelGen.getCeLength gen.FirstSuffixMaker

    let getCeLength (gen: msSplitPairsGen) : int<ceLength> =
        getPrefixCeLength gen + getSuffixCeLength gen


    /// Generates an msSplitPairs instance by making sorter models from each maker
    let makeMsSplitPairsFromId
                (id: Guid<sortingId>)
                (gen: msSplitPairsGen) : msSplitPairs =

        let rng = %id |> RngFactory.createRng gen.RngFactory
        let makePrefixId () = (rng.NextGuid()) |> UMX.tag<sortingId>

        let firstPrefix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.FirstPrefixMaker 
        let firstSuffix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.FirstSuffixMaker
        let secondPrefix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.SecondPrefixMaker
        let secondSuffix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.SecondSuffixMaker
        
        msSplitPairs.create
            (%id |> UMX.tag<sorterPairModelId>)
            gen.SortingWidth
            firstPrefix
            firstSuffix
            secondPrefix
            secondSuffix


    /// Generates an msSplitPairs instance by making sorter models from each maker
    let makeMsSplitPairsFromIndex 
                (index: int) 
                (gen: msSplitPairsGen) : msSplitPairs =
        let id =
            [
                index :> obj
                gen.Id :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingId>

        makeMsSplitPairsFromId id gen


    let makeSorterIdsWithTags (index: int) (gen: msSplitPairsGen) 
                                    : (Guid<sorterId> * modelTag) [] =
        let splitPairs = makeMsSplitPairsFromIndex index gen
        splitPairs |> MsSplitPairs.getSorterIdsWithTags