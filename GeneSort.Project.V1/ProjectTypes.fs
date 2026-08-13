namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core


[<Measure>] type projectName
[<Measure>] type databaseName
[<Measure>] type runName
[<Measure>] type queryParamsId
[<Measure>] type queryName
[<Measure>] type replNumber
[<Measure>] type generationIntervalCount

module ProjectName =
    let toString (w: string<projectName> option) : string =
       match w with
        | Some v -> %v
        | None -> "None"

module DatabaseName =
    let toString (w: string<databaseName> option) : string =
       match w with
        | Some v -> %v
        | None -> "None"

module RunName =
    let toString (w: string<runName> option) : string =
       match w with
        | Some v -> %v
        | None -> "None"

module QueryParamsId =
    let toString (w: string<queryParamsId> option) : string =
       match w with
        | Some v -> %v
        | None -> "None"

module QueryName =
    let toString (w: string<queryName> option) : string =
       match w with
        | Some v -> %v
        | None -> "None"

module ReplNumber =
    let toString (w: int<replNumber> option) : string =
       match w with
        | Some v -> %v |> string
        | None -> "None"

module GenerationIntervalCount =
    let toString (w: int<generationIntervalCount> option) : string =
       match w with
        | Some v -> %v |> string
        | None -> "None"
