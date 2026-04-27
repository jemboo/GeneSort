namespace GeneSort.Model.Sorting.V1


type simpleSorterModelType = 
    | Msce
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SimpleSorterModelType =

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




type sorterModelType = 
    | Simple of simpleSorterModelType
    | Unknown


module SorterModelType =

    let toString (model:sorterModelType) : string =
        match model with
        | Simple sms -> sms |> SimpleSorterModelType.toString
        | Unknown -> "Unknown"

    let fromString (s:string) : sorterModelType =
        match s with
        | "Msce" -> Simple Msce
        | "Mssi" -> Simple Mssi
        | "Msrs" -> Simple Msrs
        | "Msuf4" -> Simple Msuf4
        | "Msuf6" -> Simple Msuf6
        | _ -> failwithf "Unknown SorterModelType: %s" s