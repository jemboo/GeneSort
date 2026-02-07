namespace GeneSort.Model.Sorter

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Core


[<Measure>] type sorterModelID
[<Measure>] type sorterModelSetID
[<Measure>] type sorterModelMakerID
[<Measure>] type sorterModelSetMakerID

module Common =
    let makeSorterModelId 
                (id:  Guid<sorterModelMakerID>) 
                (index:int) : Guid<sorterModelID> = 
        [
            id  :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>