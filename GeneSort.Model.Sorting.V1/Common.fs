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

[<Measure>] type mutationRate

module CommonGen =

    let makeSorterModelId 
                (id:  Guid<sorterModelGenId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>

    let makeSorterModelIdWithMix
                (genId: Guid<sorterModelGenId>)
                (index: int) : Guid<sorterModelId> =
            let baseSeed = RandomSeed.fromGuid %genId
            let mixed = Rando.mix (baseSeed |> UMX.untag) (uint64 index)
            // Convert back to Guid if needed, or keep as seed directly
            // For now, stick close to your original style
            makeSorterModelId genId index


module CommonMutator =

    let makeSorterModelId 
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>
