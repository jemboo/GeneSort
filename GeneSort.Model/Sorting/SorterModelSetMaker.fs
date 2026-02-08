namespace GeneSort.Model.Sorting

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

type sorterModelSetMaker =
    private
        { 
          id : Guid<sorterModelSetMakerID>
          sorterModelMaker : sorterModelMaker
          firstIndex : int<sorterCount>
          count : int<sorterCount>
        }
    with
    static member create 
                (sorterModelMaker: sorterModelMaker) 
                (firstIndex: int<sorterCount>) 
                (count: int<sorterCount>) : sorterModelSetMaker =
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

    member this.MakeSortingModelSet (rngFactory: rngType -> Guid -> IRando) : sortingModelSet =
        if %this.count <= 0 then
            failwith "Count must be greater than 0"
        if %this.firstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sortingModels = 
            [| for i in 0 .. %this.count - 1 do
                let index = %this.firstIndex + i
                let sorterModel = SorterModelMaker.makeSorterModel rngFactory index this.sorterModelMaker 
                let sortingModelId = Common.makeSortingModelId (this.sorterModelMaker |> SorterModelMaker.getSorterModelMakerId) index
                sortingModelSingle.create sortingModelId sorterModel |> sortingModel.Single |]

        let id = (%this.id) |> UMX.tag<sortingModelSetID>
        sortingModelSet.create id sortingModels