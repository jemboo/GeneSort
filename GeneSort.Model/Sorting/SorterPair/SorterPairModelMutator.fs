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

    let getId (mutator: sorterPairModelMutator) : Guid<sorterPairModelMutatorId> =
        match mutator with
        | sorterPairModelMutator.SplitPairs mspg -> mspg.Id
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg.Id


    let getParentSorterPair (mutator: sorterPairModelMutator) : sorterPairModel =
        match mutator with
        | sorterPairModelMutator.SplitPairs mspg -> mspg.ParentModel |> sorterPairModel.SplitPairs
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg.ParentModel |> sorterPairModel.SplitPairs2


    let getParentSorterPairId (mutator: sorterPairModelMutator) : Guid<sorterPairModelId> =
        match mutator with
        | sorterPairModelMutator.SplitPairs mspg -> mspg.ParentModel.Id
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg.ParentModel.Id


    let getParentSorterIdsWithTags (mutator: sorterPairModelMutator) : (Guid<sorterId> * modelTag) []  =
        match mutator with
        | sorterPairModelMutator.SplitPairs mspg -> 
                mspg.ParentModel |> MsSplitPairs.getSorterIdsWithTags

        | sorterPairModelMutator.SplitPairs2 mspg -> 
                mspg.ParentModel |> MsSplitPairs.getSorterIdsWithTags


    let getCeLength (mutator: sorterPairModelMutator) : int<ceLength> =
        match mutator with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getCeLength mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getCeLength mspg


    let getMutantSortingId (index: int) (mutator: sorterPairModelMutator) : Guid<sortingId> =
        match mutator with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getMutantSortingId index mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getMutantSortingId index mspg


    let getSortingWidth (model: sorterPairModelMutator) : int<sortingWidth> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getSortingWidth mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getSortingWidth mspg


    let makeSorterPairModelFromId
                (sortingId: Guid<sortingId>) 
                (model: sorterPairModelMutator) : sorterPairModel =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> 
                mspg |> MsSplitPairsMutator.makeMsSplitPairsFromId sortingId 
                     |> sorterPairModel.SplitPairs
        | sorterPairModelMutator.SplitPairs2 mspg -> 
                mspg |> MsSplitPairsMutator.makeMsSplitPairsFromId sortingId 
                     |> sorterPairModel.SplitPairs2


    let makeSorterPairModelFromIndex
                (index: int)  
                (model: sorterPairModelMutator) : sorterPairModel =
        let id = [
                        model |> getId :> obj
                        index :> obj
                 ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingId>

        model |> makeSorterPairModelFromId id


    let makeMutantSorterIdsWithTags (index: int) (model: sorterPairModelMutator) : (Guid<sorterId> * modelTag) [] =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> mspg |> MsSplitPairsMutator.makeSorterIdsWithTags index
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg |> MsSplitPairsMutator.makeSorterIdsWithTags index




