namespace GeneSort.Model.Sorting.SorterPair.SplitPairs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open System

type msSplitPairsMutator = 
    private 
        { id: Guid<sorterPairModelMutatorId>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          parentModel: msSplitPairs
          firstPrefixMutator: sorterModelMutator
          firstSuffixMutator: sorterModelMutator
          secondPrefixMutator: sorterModelMutator
          secondSuffixMutator: sorterModelMutator} 
    with
    /// Creates an msSplitPairsGen instance with the specified sorting width and four sorter model mutators.
    /// The ID is deterministically calculated from the constituent mutator IDs.
    /// Throws an exception if:
    /// - width is less than 1
    /// - any sorter model mutator has a mismatched sorting width
    /// - the two prefix mutators don't have the same ceLength
    /// - the two suffix mutators don't have the same ceLength
    static member create 
            (sortingWidth: int<sortingWidth>)
            (rngFactory: rngFactory)
            (firstPrefixMutator: sorterModelMutator)
            (firstSuffixMutator: sorterModelMutator)
            (secondPrefixMutator: sorterModelMutator)
            (secondSuffixMutator: sorterModelMutator) : msSplitPairsMutator =
        
        if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        
        // Validate that all sorter model mutators have the same sorting width
        let makers = [firstPrefixMutator; firstSuffixMutator; secondPrefixMutator; secondSuffixMutator]
        let mismatchedMutators = 
            makers 
            |> List.filter (fun mutator -> SorterModelMutator.getSortingWidth mutator <> sortingWidth)
        
        if not (List.isEmpty mismatchedMutators) then
            failwith $"All sorter model mutators must have sortingWidth {%sortingWidth}"
        
        // Validate that the two prefix mutators have the same ceLength
        let firstPrefixCeLength = SorterModelMutator.getCeLength firstPrefixMutator
        let secondPrefixCeLength = SorterModelMutator.getCeLength secondPrefixMutator
        
        if firstPrefixCeLength <> secondPrefixCeLength then
            failwith $"Both prefix mutators must have the same ceLength. FirstPrefix: {%firstPrefixCeLength}, SecondPrefix: {%secondPrefixCeLength}"
        
        // Validate that the two suffix mutator have the same ceLength
        let firstSuffixCeLength = SorterModelMutator.getCeLength firstSuffixMutator
        let secondSuffixCeLength = SorterModelMutator.getCeLength secondSuffixMutator
        
        if firstSuffixCeLength <> secondSuffixCeLength then
            failwith $"Both suffix mutators must have the same ceLength. FirstSuffix: {%firstSuffixCeLength}, SecondSuffix: {%secondSuffixCeLength}"
        
        // Calculate ID deterministically from the constituent mutator IDs
        let id =
            [
                "msSplitPairsGen" :> obj
                sortingWidth :> obj
                %SorterModelMutator.getId firstPrefixMutator :> obj
                %SorterModelMutator.getId firstSuffixMutator :> obj
                %SorterModelMutator.getId secondPrefixMutator :> obj
                %SorterModelMutator.getId secondSuffixMutator :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterPairModelMutatorId>


        let parentModel = msSplitPairs.create
                            (Guid.NewGuid() |> UMX.tag<sorterPairModelId>)
                            sortingWidth
                            (firstPrefixMutator |> SorterModelMutator.getParentSorterModel)
                            (firstSuffixMutator |> SorterModelMutator.getParentSorterModel)
                            (secondPrefixMutator |> SorterModelMutator.getParentSorterModel)
                            (secondSuffixMutator |> SorterModelMutator.getParentSorterModel)
        
        { id = id
          sortingWidth = sortingWidth
          rngFactory = rngFactory
          parentModel = parentModel
          firstPrefixMutator = firstPrefixMutator
          firstSuffixMutator = firstSuffixMutator
          secondPrefixMutator = secondPrefixMutator
          secondSuffixMutator = secondSuffixMutator }
    
    member this.Id with get () = this.id
    member this.RngFactory with get() = this.rngFactory
    member this.SortingWidth with get () = this.sortingWidth
    member this.ParentModel with get () = this.parentModel  
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

    let getMutantSortingId (index: int) (mutator: msSplitPairsMutator) : Guid<sortingId> =
            [
                %mutator.Id :> obj
                index :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingId>

    /// Generates an msSplitPairs instance based on sortingId
    let makeMsSplitPairsFromId
                (id: Guid<sortingId>) 
                (mutator: msSplitPairsMutator) : msSplitPairs =

        let rng = %id |> RngFactory.createRng mutator.RngFactory
        let makePrefixId () = (rng.NextGuid()) |> UMX.tag<sortingId>

        let firstPrefix = SorterModelMutator.makeMutantSorterModelFromId (makePrefixId()) mutator.FirstPrefixMutator 
        let firstSuffix = SorterModelMutator.makeMutantSorterModelFromId (makePrefixId()) mutator.FirstSuffixMutator
        let secondPrefix = SorterModelMutator.makeMutantSorterModelFromId (makePrefixId()) mutator.SecondPrefixMutator
        let secondSuffix = SorterModelMutator.makeMutantSorterModelFromId (makePrefixId()) mutator.SecondSuffixMutator
        
        msSplitPairs.create
                        (%id |> UMX.tag<sorterPairModelId>)
                        mutator.SortingWidth
                        firstPrefix
                        firstSuffix
                        secondPrefix
                        secondSuffix


    /// Generates an msSplitPairs instance based on index
    let makeMsSplitPairsFromIndex 
                (index: int) 
                (mutator: msSplitPairsMutator) : msSplitPairs =

        let id = getMutantSortingId index mutator
        makeMsSplitPairsFromId id mutator


    let makeSorterIdsWithTags (index: int) (mutator: msSplitPairsMutator) 
                                    : (Guid<sorterId> * modelTag) [] =
        let splitPairs = makeMsSplitPairsFromIndex index mutator
        splitPairs |> MsSplitPairs.getSorterIdsWithTags