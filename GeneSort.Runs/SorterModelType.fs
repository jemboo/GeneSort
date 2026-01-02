namespace GeneSort.Runs



type sorterModelType = 
    | Mcse
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6


module SorterModelType =

    let toString (model:sorterModelType) : string =
        match model with
        | Mcse -> "Mcse"
        | Mssi -> "Mssi"
        | Msrs -> "Msrs"
        | Msuf4 -> "Msuf4"
        | Msuf6 -> "Msuf6"

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




