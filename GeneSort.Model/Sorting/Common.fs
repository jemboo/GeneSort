namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Core



[<Measure>] type sorterModelId
[<Measure>] type sorterPairModelId
[<Measure>] type sortingId
[<Measure>] type sortingSetId
[<Measure>] type sorterModelGenId
[<Measure>] type sorterModelMutatorId
[<Measure>] type sorterPairModelGenId
[<Measure>] type sorterPairModelMutatorId
[<Measure>] type sortingGenId
[<Measure>] type sortingMutatorId
[<Measure>] type sortingGenSegmentId
[<Measure>] type sortingMutationSegmentId
[<Measure>] type parentSorterSetEvalId
[<Measure>] type sortingParamsId



[<Measure>] type mutationRate

module CommonGen =

    let makeSorterModelId 
                (id:  Guid<sorterModelGenId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>


    let makeSortingId 
                (id:  Guid<sorterModelGenId>) 
                (index:int) : Guid<sortingId> = 
        [
            id  :> obj
            "Sorting" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingId>



module CommonMutator =

    let makeSorterModelId 
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>


    let makeSortingId 
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sortingId> = 
        [
            id  :> obj
            "Sorting" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingId>