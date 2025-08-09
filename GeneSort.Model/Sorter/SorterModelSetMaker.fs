namespace GeneSort.Model.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core

type SorterModelSetMaker =
    private
        { 
          Id : Guid<sorterModelSetMakerID>
          SorterModelMaker : ISorterModelMaker
          firstIndex : int
          count : int 
        }
    with
    static member create 
                (sorterModelMaker: ISorterModelMaker) 
                (firstIndex: int) 
                (count: int) : SorterModelSetMaker =
        let id = 
            [ 
              sorterModelMaker.Id :> obj
              firstIndex :> obj
              count :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelSetMakerID>

        if count < 1 then
            failwith "Count must be at least 1"

        else
            { Id = id; SorterModelMaker = sorterModelMaker; firstIndex = firstIndex; count = count }


module SorterModelSetMaker =

    let makeSorterModelSet 
                (randoGen: rngType -> Guid -> IRando)
                (sorterModelSetMaker: SorterModelSetMaker) 
          : SorterModelSet =
          {
            Id = %sorterModelSetMaker.Id |> UMX.tag<sorterModelSetID>
            SorterModels =
                Array.init sorterModelSetMaker.count (fun i ->
                    let index = i + sorterModelSetMaker.firstIndex
                    sorterModelSetMaker.SorterModelMaker.MakeSorterModel (randoGen) index)
         }

    let makeSorterSet 
                (randoGen: rngType -> Guid -> IRando)
                (sorterModelSetMaker: SorterModelSetMaker) 
          : SorterSet =
        makeSorterModelSet randoGen sorterModelSetMaker |> SorterModelSet.makeSorterSet