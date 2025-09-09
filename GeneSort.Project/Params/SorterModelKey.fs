namespace GeneSort.Project.Params
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


type SorterModelKey = 
    | Mcse
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SorterModelKey =

    let toString (model:SorterModelKey) : string =
        match model with
        | Mcse -> "Mcse"
        | Mssi -> "Mssi"
        | Msrs -> "Msrs"
        | Msuf4 -> "Msuf4"
        | Msuf6 -> "Msuf6"

    let fromString (s:string) : SorterModelKey =
        match s with
        | "Mcse" -> Mcse
        | "Mssi" -> Mssi
        | "Msrs" -> Msrs
        | "Msuf4" -> Msuf4
        | "Msuf6" -> Msuf6
        | _ -> failwithf "Unknown SorterModelKey: %s" s

    let all () : SorterModelKey list =
        [ Mcse; Mssi; Msrs; Msuf4; Msuf6 ]

    let allButMusf6 () : SorterModelKey list =
        [ Mcse; Mssi; Msrs; Msuf4; ]

    let allButMusf6Kvps () : string*string list =
        ("SorterModel", allButMusf6() |> List.map(toString))




