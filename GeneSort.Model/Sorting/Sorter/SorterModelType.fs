namespace GeneSort.Model.Sorting


type sorterModelType = 
    | Msce
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SorterModelType =

    let toString (model:sorterModelType) : string =
        match model with
        | Msce -> "Msce"
        | Mssi -> "Mssi"
        | Msrs -> "Msrs"
        | Msuf4 -> "Msuf4"
        | Msuf6 -> "Msuf6"

    let fromString (s:string) : sorterModelType =
        match s with
        | "Msce" -> Msce
        | "Mssi" -> Mssi
        | "Msrs" -> Msrs
        | "Msuf4" -> Msuf4
        | "Msuf6" -> Msuf6
        | _ -> failwithf "Unknown SorterModelType: %s" s

    let all () : sorterModelType list =
        [ Msce; Mssi; Msrs; Msuf4; Msuf6 ]




