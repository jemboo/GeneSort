namespace GeneSort.Runs
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


type sorterModelType = 
    | Mcse
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SorterModelType =

    let toString (model:sorterModelType option) : string =
        match model with
        | Some Mcse -> "Mcse"
        | Some Mssi -> "Mssi"
        | Some Msrs -> "Msrs"
        | Some Msuf4 -> "Msuf4"
        | Some Msuf6 -> "Msuf6"
        | None -> "None"

    let fromString (s:string) : sorterModelType =
        match s with
        | "Mcse" -> Mcse
        | "Mssi" -> Mssi
        | "Msrs" -> Msrs
        | "Msuf4" -> Msuf4
        | "Msuf6" -> Msuf6
        | _ -> failwithf "Unknown SorterModelType: %s" s

    let all () : sorterModelType list =
        [ Mcse; Mssi; Msrs; Msuf4; Msuf6 ]

    let allButMusf6 () : sorterModelType option list =
        [ Some Mcse; Some Mssi; Some Msrs; Some Msuf4; ]

    let allButMusf6Kvps () : string*string list =
        ("SorterModel", allButMusf6() |> List.map(toString))




