namespace GeneSort.Runs
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


type sorterModelKey = 
    | Mcse
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SorterModelKey =

    let toString (model:sorterModelKey option) : string =
        match model with
        | Some Mcse -> "Mcse"
        | Some Mssi -> "Mssi"
        | Some Msrs -> "Msrs"
        | Some Msuf4 -> "Msuf4"
        | Some Msuf6 -> "Msuf6"
        | None -> "None"

    let fromString (s:string) : sorterModelKey =
        match s with
        | "Mcse" -> Mcse
        | "Mssi" -> Mssi
        | "Msrs" -> Msrs
        | "Msuf4" -> Msuf4
        | "Msuf6" -> Msuf6
        | _ -> failwithf "Unknown SorterModelKey: %s" s

    let all () : sorterModelKey list =
        [ Mcse; Mssi; Msrs; Msuf4; Msuf6 ]

    let allButMusf6 () : sorterModelKey option list =
        [ Some Mcse; Some Mssi; Some Msrs; Some Msuf4; ]

    let allButMusf6Kvps () : string*string list =
        ("SorterModel", allButMusf6() |> List.map(toString))




