namespace GeneSort.Model.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Core

type SorterModelSetMaker =
    private
        { 
          id : Guid<sorterModelSetMakerID>
          sorterModelMaker : SorterModelMaker
          firstIndex : int
          count : int 
        }
    with
    static member create 
                (sorterModelMaker: SorterModelMaker) 
                (firstIndex: int) 
                (count: int) : SorterModelSetMaker =
        let id = 
            // Generate a unique ID based on the SorterModelMaker and indices
            GuidUtils.guidFromObjs [
                    sorterModelMaker :> obj
                    firstIndex :> obj
                    count :> obj
                ] |> UMX.tag<sorterModelSetMakerID>

        { id = id; sorterModelMaker = sorterModelMaker; firstIndex = firstIndex; count = count }

    member this.Id with get() = this.id
    member this.SorterModelMaker with get() = this.sorterModelMaker
    member this.FirstIndex with get() = this.firstIndex
    member this.Count with get() = this.count