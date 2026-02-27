namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Core



[<Measure>] type sorterModelId
[<Measure>] type sortingModelId
[<Measure>] type sortingModelSetId
[<Measure>] type sorterModelMakerId
[<Measure>] type sorterModelMutatorId
[<Measure>] type sorterPairModelMakerId
[<Measure>] type sorterPairModelMutatorId
[<Measure>] type sortingModelMakerId
[<Measure>] type sortingModelMutatorId
[<Measure>] type sortingModelSetMakerId
[<Measure>] type sortingModelSetMutatorId
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


    let makeSortingModelId 
                (id:  Guid<sorterModelMakerId>) 
                (index:int) : Guid<sortingModelId> = 
        [
            id  :> obj
            "Sorting" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingModelId>



module CommonMutator =

    let makeSorterModelId 
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>


    let makeSortingModelId 
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sortingModelId> = 
        [
            id  :> obj
            "Sorting" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingModelId>