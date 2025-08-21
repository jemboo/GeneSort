
namespace GeneSort.Model.Sortable
open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core

[<Measure>] type sortableSetModelID
[<Measure>] type sortableModelSetID
[<Measure>] type sortableModelMakerID
[<Measure>] type sortableModelSetMakerID

module Common =
    let makeSorterModelId 
                (id:  Guid<sortableModelMakerID>) 
                (index:int): Guid<sortableSetModelID> = 
        [
            id  :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sortableSetModelID>