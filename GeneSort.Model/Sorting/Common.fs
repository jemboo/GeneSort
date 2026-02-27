namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Core



[<Measure>] type sorterModelId
[<Measure>] type sortingId
[<Measure>] type sortingSetId
[<Measure>] type sorterModelMakerId
[<Measure>] type sorterModelMutatorId
[<Measure>] type sorterPairModelMakerId
[<Measure>] type sorterPairModelMutatorId
[<Measure>] type sortingMakerId
[<Measure>] type sortingMutatorId
[<Measure>] type sortingSetMakerId
[<Measure>] type sortingSetMutatorId
[<Measure>] type parentSorterSetEvalId
[<Measure>] type sortingParamsId



[<Measure>] type mutationRate

module CommonMaker =

    let makeSorterModelId 
                (id:  Guid<sorterModelMakerId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>


    let makeSortingId 
                (id:  Guid<sorterModelMakerId>) 
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