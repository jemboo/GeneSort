namespace GeneSort.Model.Sorting.V1

open System
open FSharp.UMX
open GeneSort.Core



[<Measure>] type sorterModelId
[<Measure>] type sorterModelSetId
[<Measure>] type sorterPairModelId
[<Measure>] type sorterModelGenId
[<Measure>] type sorterModelMutatorId
[<Measure>] type sortingGenId
[<Measure>] type sortingMutatorId
[<Measure>] type sortingGenSegmentId
[<Measure>] type sortingMutationSegmentId
[<Measure>] type parentSorterSetEvalId
[<Measure>] type sortingParamsId

[<Measure>] type excludeSelfCe
[<Measure>] type modificationRate
[<Measure>] type mutationRate
[<Measure>] type insertionRate
[<Measure>] type deletionRate


module MutationRate =

    let toString (w: float<mutationRate> option) : string =
       match w with
        | Some v -> sprintf "%f" %v
        | None -> "None"


module InsertionRate =

    let toString (w: float<insertionRate> option) : string =
       match w with
        | Some v -> sprintf "%f" %v
        | None -> "None"


module DeletionRate =

    let toString (w: float<deletionRate> option) : string =
       match w with
        | Some v -> sprintf "%f" %v
        | None -> "None"

module ModificationRate =

    let toString (w: float<modificationRate> option) : string =
       match w with
        | Some v -> sprintf "%f" %v
        | None -> "None"


module CommonGen =

    let makeSorterModelId 
                (id:  Guid<sorterModelGenId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            box "CommonGen.makeSorterModelId"
            box (id |> UMX.untag)
            box index
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>



module CommonMutator =

    let makeSorterModelId 
                (parentId: Guid<sorterModelId>)
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            box "CommonMutator.makeSorterModelId"
            box (parentId |> UMX.untag)
            box (id |> UMX.untag)
            box index
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>
