namespace GeneSort.Model.Sorting.V1

open FSharp.UMX
open GeneSort.Core



[<Measure>] type sorterModelId
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



module CommonMutator =

    let makeSorterModelId 
                (id:  Guid<sorterModelMutatorId>) 
                (index:int) : Guid<sorterModelId> = 
        [
            id  :> obj
            "SorterModel" :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelId>


        
type simpleSorterModelType = 
    | Msce
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SorterModelType =

    let toString (model:simpleSorterModelType) : string =
        match model with
        | Msce -> "Msce"
        | Mssi -> "Mssi"
        | Msrs -> "Msrs"
        | Msuf4 -> "Msuf4"
        | Msuf6 -> "Msuf6"

    let fromString (s:string) : simpleSorterModelType =
        match s with
        | "Msce" -> Msce
        | "Mssi" -> Mssi
        | "Msrs" -> Msrs
        | "Msuf4" -> Msuf4
        | "Msuf6" -> Msuf6
        | _ -> failwithf "Unknown SorterModelType: %s" s

    let all () : simpleSorterModelType list =
        [ Msce; Mssi; Msrs; Msuf4; Msuf6 ]
