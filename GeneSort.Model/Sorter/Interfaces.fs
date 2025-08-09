namespace GeneSort.Model.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core


[<Measure>] type sorterModelID
[<Measure>] type sorterModelSetID
[<Measure>] type sorterModelMakerID
[<Measure>] type sorterModelSetMakerID

type ISorterModel =
    abstract member Id : Guid<sorterModelID>
    abstract member MakeSorter : unit -> Sorter

type ISorterModelMaker =
    abstract member Id : Guid<sorterModelMakerID>
    abstract member MakeSorterModel : (rngType -> Guid -> IRando) -> int -> ISorterModel


module SorterModelMaker =
    let makeSorterModelId (sorterModelMaker: ISorterModelMaker) (index:int) =
        [
            %sorterModelMaker.Id  :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>