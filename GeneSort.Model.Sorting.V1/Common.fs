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
