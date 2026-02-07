namespace GeneSort.Model.Sorter

open FSharp.UMX
open GeneSort.Core


[<Measure>] type sortingModelID
[<Measure>] type sortingModelSetID
[<Measure>] type sorterModelMakerID
[<Measure>] type sorterModelSetMakerID

module Common =
    let makeSortingModelId 
                (id:  Guid<sorterModelMakerID>) 
                (index:int) : Guid<sortingModelID> = 
        [
            id  :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingModelID>