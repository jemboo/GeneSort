namespace GeneSort.Model.Sorting.SorterPair

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair.SplitPairs


type sorterPairModelMutator =
     | SplitPairs of msSplitPairsMutator
     | SplitPairs2 of msSplitPairsMutator



module SorterPairModelMutator =

    let getId (model: sorterPairModelMutator) : Guid<sorterPairModelMutatorId> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> mspg.Id
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg.Id

    let getSortingModelSeedId (model: sorterPairModelMutator) : Guid<sortingModelId> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> mspg.SortingModelSeedId
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg.SortingModelSeedId

    let getCeLength (model: sorterPairModelMutator) : int<ceLength> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getCeLength mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getCeLength mspg


    let getSortingWidth (model: sorterPairModelMutator) : int<sortingWidth> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getSortingWidth mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getSortingWidth mspg


    let makeSorterPairModel
                (index: int)  
                (model: sorterPairModelMutator) : sorterPairModel =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> 
                mspg |> MsSplitPairsMutator.makeMsSplitPairs index 
                     |> sorterPairModel.SplitPairs
        | sorterPairModelMutator.SplitPairs2 mspg -> 
                mspg |> MsSplitPairsMutator.makeMsSplitPairs index 
                     |> sorterPairModel.SplitPairs2


    let makeSorterIdsWithTags (index: int) (model: sorterPairModelMutator) : (Guid<sorterId> * modelTag) [] =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> mspg |> MsSplitPairsMutator.makeSorterIdsWithTags index
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg |> MsSplitPairsMutator.makeSorterIdsWithTags index




