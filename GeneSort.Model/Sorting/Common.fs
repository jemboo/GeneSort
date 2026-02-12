namespace GeneSort.Model.Sorting

open FSharp.UMX
open GeneSort.Core



[<Measure>] type sorterModelID
[<Measure>] type sortingModelID
[<Measure>] type sortingModelSetID
[<Measure>] type sorterModelMakerID
[<Measure>] type sorterPairModelMakerID
[<Measure>] type sortingModelMakerID
[<Measure>] type sortingModelSetMakerID

module Common =

    let makeSorterModelId 
                (id:  Guid<sorterModelMakerID>) 
                (index:int) : Guid<sorterModelID> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

    let makeSortingModelId 
                (id:  Guid<sorterModelMakerID>) 
                (index:int) : Guid<sortingModelID> = 
        [
            id  :> obj
            "SortingModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingModelID>