namespace GeneSort.Model.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

type SorterModelSetMaker =
    private
        { 
          id : Guid<sorterModelSetMakerID>
          sorterModelMaker : SorterModelMaker
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sorterModelMaker: SorterModelMaker) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : SorterModelSetMaker =
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

    member this.MakeSorterModelSet (rngFactory: rngType -> Guid -> IRando) : SorterModelSet =
        if %this.count <= 0 then
            failwith "Count must be greater than 0"
        if %this.firstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sorterModels = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                SorterModelMaker.makeSorterModel rngFactory index this.sorterModelMaker |]
        let id = (%this.id) |> UMX.tag<sorterModelSetID>
        { Id = id; SorterModels = sorterModels }