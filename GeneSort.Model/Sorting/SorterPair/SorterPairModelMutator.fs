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

    let makeSorterPairModel 
                (rngFactory: rngType -> Guid -> IRando) 
                (index: int)  
                (model: sorterPairModelMutator) : sorterPairModel =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> 
                mspg |> MsSplitPairsMutator.makeMsSplitPairs rngFactory index 
                     |> sorterPairModel.SplitPairs
        | sorterPairModelMutator.SplitPairs2 mspg -> 
                mspg |> MsSplitPairsMutator.makeMsSplitPairs rngFactory index 
                     |> sorterPairModel.SplitPairs2


    let getCeLength (model: sorterPairModelMutator) : int<ceLength> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getCeLength mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getCeLength mspg


    let getId (model: sorterPairModelMutator) : Guid<sorterPairModelMakerID> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> mspg.Id
        | sorterPairModelMutator.SplitPairs2 mspg -> mspg.Id

    let getSortingWidth (model: sorterPairModelMutator) : int<sortingWidth> =
        match model with
        | sorterPairModelMutator.SplitPairs mspg -> MsSplitPairsMutator.getSortingWidth mspg
        | sorterPairModelMutator.SplitPairs2 mspg -> MsSplitPairsMutator.getSortingWidth mspg



