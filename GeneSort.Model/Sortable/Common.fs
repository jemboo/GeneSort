
namespace GeneSort.Model.Sortable
open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Core

[<Measure>] type sorterTestModelID
[<Measure>] type sorterTestModelCount

[<Measure>] type sorterTestModelSetID
[<Measure>] type sorterTestModelGenID
[<Measure>] type sorterTestModelSetGenID

//module Common =
//    let makeSorterModelId 
//                (id:  Guid<sortableModelMakerID>) 
//                (index:int): Guid<sorterTestModelID> = 
//        [
//            id  :> obj
//            index :> obj
//        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>