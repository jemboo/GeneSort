namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps


[<Measure>] type generationNumber
[<Measure>] type sorterPoolMemberId
[<Measure>] type sorterPoolSetId
[<Measure>] type sorterPoolId
[<Measure>] type sorterCountPerPool
[<Measure>] type sorterPoolCount
[<Measure>] type sorterChildCount

module GenerationNumber =
    let toString (w: int<generationNumber> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterCountPerPool =
    let toString (w: int<sorterCountPerPool> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterPoolCount =
    let toString (w: int<sorterPoolCount> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterChildCount =
    let toString (w: int<sorterChildCount> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module Common = ()

