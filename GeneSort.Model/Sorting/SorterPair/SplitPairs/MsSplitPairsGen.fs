namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

type msSplitPairsGen = 
    private 
        { id: Guid<sorterPairModelGenId>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          firstPrefixGen: sorterModelGen
          firstSuffixGen: sorterModelGen
          secondPrefixGen: sorterModelGen
          secondSuffixGen: sorterModelGen} 
    with
    /// Creates an msSplitPairsGen instance with the specified sorting width and four sorter model gens.
    /// The ID is deterministically calculated from the constituent gen IDs.
    /// Throws an exception if:
    /// - width is less than 1
    /// - any sorter model gen has a mismatched sorting width
    /// - the two prefix gens don't have the same ceLength
    /// - the two suffix gens don't have the same ceLength
    static member create 
            (sortingWidth: int<sortingWidth>)
            (rngFactory: rngFactory)
            (firstPrefixGen: sorterModelGen)
            (firstSuffixGen: sorterModelGen)
            (secondPrefixGen: sorterModelGen)
            (secondSuffixGen: sorterModelGen) : msSplitPairsGen =
        
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        
        // Validate that all sorter model makers have the same sorting width
        let makers = [firstPrefixGen; firstSuffixGen; secondPrefixGen; secondSuffixGen]
        let mismatchedMakers = 
            makers 
            |> List.filter (fun mutator -> SorterModelGen.getSortingWidth mutator <> sortingWidth)
        
        if not (List.isEmpty mismatchedMakers) then
            failwith $"All sorter model makers must have sortingWidth {%sortingWidth}"
        
        // Validate that the two prefix makers have the same ceLength
        let firstPrefixCeLength = SorterModelGen.getCeLength firstPrefixGen
        let secondPrefixCeLength = SorterModelGen.getCeLength secondPrefixGen
        
        if firstPrefixCeLength <> secondPrefixCeLength then
            failwith $"Both prefix makers must have the same ceLength. FirstPrefix: {%firstPrefixCeLength}, SecondPrefix: {%secondPrefixCeLength}"
        
        // Validate that the two suffix makers have the same ceLength
        let firstSuffixCeLength = SorterModelGen.getCeLength firstSuffixGen
        let secondSuffixCeLength = SorterModelGen.getCeLength secondSuffixGen
        
        if firstSuffixCeLength <> secondSuffixCeLength then
            failwith $"Both suffix makers must have the same ceLength. FirstSuffix: {%firstSuffixCeLength}, SecondSuffix: {%secondSuffixCeLength}"
        
        // Calculate ID deterministically from the constituent maker IDs
        let id =
            [
                "msSplitPairsGen" :> obj
                sortingWidth :> obj
                %SorterModelGen.getId firstPrefixGen :> obj
                %SorterModelGen.getId firstSuffixGen :> obj
                %SorterModelGen.getId secondPrefixGen :> obj
                %SorterModelGen.getId secondSuffixGen :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterPairModelGenId>
        
        { id = id
          sortingWidth = sortingWidth
          rngFactory = rngFactory
          firstPrefixGen = firstPrefixGen
          firstSuffixGen = firstSuffixGen
          secondPrefixGen = secondPrefixGen
          secondSuffixGen = secondSuffixGen }
    
    member this.Id with get () = this.id
    member this.RngFactory with get() = this.rngFactory
    member this.SortingWidth with get () = this.sortingWidth
    member this.FirstPrefixGen with get () = this.firstPrefixGen
    member this.FirstSuffixGen with get () = this.firstSuffixGen
    member this.SecondPrefixGen with get () = this.secondPrefixGen
    member this.SecondSuffixGen with get () = this.secondSuffixGen


module MsSplitPairsGen =

    let getSortingWidth (gen: msSplitPairsGen) : int<sortingWidth> =
        gen.SortingWidth
    
    let getPrefixCeLength (gen: msSplitPairsGen) : int<ceLength> =
        SorterModelGen.getCeLength gen.FirstPrefixGen
    
    let getSuffixCeLength (gen: msSplitPairsGen) : int<ceLength> =
        SorterModelGen.getCeLength gen.FirstSuffixGen

    let getCeLength (gen: msSplitPairsGen) : int<ceLength> =
        getPrefixCeLength gen + getSuffixCeLength gen


    /// Generates an msSplitPairs instance by making sorter models from each mutator
    let makeMsSplitPairsFromId
                (id: Guid<sortingId>)
                (gen: msSplitPairsGen) : msSplitPairs =

        let rng = %id |> RngFactory.createRng gen.RngFactory
        let makePrefixId () = (rng.NextGuid()) |> UMX.tag<sortingId>

        let firstPrefix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.FirstPrefixGen 
        let firstSuffix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.FirstSuffixGen
        let secondPrefix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.SecondPrefixGen
        let secondSuffix = SorterModelGen.makeSorterModelFromId (makePrefixId()) gen.SecondSuffixGen
        
        msSplitPairs.create
            (%id |> UMX.tag<sorterPairModelId>)
            gen.SortingWidth
            firstPrefix
            firstSuffix
            secondPrefix
            secondSuffix


    /// Generates an msSplitPairs instance by making sorter models from each mutator
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