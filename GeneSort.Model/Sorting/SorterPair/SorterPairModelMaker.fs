namespace GeneSort.Model.Sorting.SorterPair

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.SorterPair.SplitPairs


type sorterPairModelMaker =
     | SplitPairs of msSplitPairsGen
     | SplitPairs2 of msSplitPairsGen



module SorterPairModelMaker =

    let getCeLength (model: sorterPairModelMaker) : int<ceLength> =
        match model with
        | sorterPairModelMaker.SplitPairs mspg -> MsSplitPairsGen.getCeLength mspg
        | sorterPairModelMaker.SplitPairs2 mspg -> MsSplitPairsGen.getCeLength mspg


    let getId (model: sorterPairModelMaker) : Guid<sorterPairModelMakerId> =
        match model with
        | SplitPairs mspg -> mspg.Id
        | SplitPairs2 mspg -> mspg.Id

    let getSortingWidth (model: sorterPairModelMaker) : int<sortingWidth> =
        match model with
        | SplitPairs mspg -> MsSplitPairsGen.getSortingWidth mspg
        | SplitPairs2 mspg -> MsSplitPairsGen.getSortingWidth mspg



    let makeSorterPairModelFromId
                (sortingId: Guid<sortingId>) 
                (model: sorterPairModelMaker) : sorterPairModel =

        match model with
        | SplitPairs mspg -> mspg |> MsSplitPairsGen.makeMsSplitPairsFromId sortingId 
                                  |> sorterPairModel.SplitPairs
        | SplitPairs2 mspg -> mspg |> MsSplitPairsGen.makeMsSplitPairsFromId sortingId 
                                   |> sorterPairModel.SplitPairs2

    let makeSorterPairModelFromIndex
                (index: int)  
                (model: sorterPairModelMaker) : sorterPairModel =

        let id = [
                        model |> getId :> obj
                        index :> obj
                 ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingId>

        makeSorterPairModelFromId id model 


    let makeSorterIdsWithTags (index:int) (model:sorterPairModelMaker)
                                    : (Guid<sorterId> * modelTag) [] =
        match model with
        | SplitPairs mspg -> mspg |> MsSplitPairsGen.makeSorterIdsWithTags index 

        | SplitPairs2 mspg -> mspg |> MsSplitPairsGen.makeSorterIdsWithTags index 





