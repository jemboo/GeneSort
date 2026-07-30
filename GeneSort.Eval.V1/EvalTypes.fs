namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Core

[<Measure>] type distinctSorterHashes
[<Measure>] type generationNumber
[<Measure>] type sorterPoolMemberId
[<Measure>] type sorterPoolSetId
[<Measure>] type sorterPoolName
[<Measure>] type sorterPoolId
[<Measure>] type sorterCountPerPool
[<Measure>] type sorterCountCycle
[<Measure>] type sorterCountCycleMultiplier
[<Measure>] type sortedFraction
[<Measure>] type sorterPoolCount
[<Measure>] type sorterChildCount

module GenerationNumber =
    let toString (w: int<generationNumber> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SortedFraction =
    let toString (w: float<sortedFraction> option) : string =
        UmxExt.floatOptionToString w

module SorterCountPerPool =
    let toString (w: int<sorterCountPerPool> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterCountCycle =
    let toString (w: int<sorterCountCycle> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterCountCycleMultiplier =
    let toString (w: float<sorterCountCycleMultiplier> option) : string =
        UmxExt.floatOptionToString w

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


