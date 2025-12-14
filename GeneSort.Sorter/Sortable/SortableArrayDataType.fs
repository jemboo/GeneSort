
namespace GeneSort.Sorter.Sortable

type sortableArrayDataType = 
    | Ints
    | Bools

module SortableArrayDataType =

    let toString (t:sortableArrayDataType option) : string =
        match t with
        | Some Ints -> "Ints"
        | Some Bools -> "Bools"
        | None -> "None"

    let fromString (s:string) : sortableArrayDataType =
        match s with
        | "Ints" -> Ints
        | "Bools" -> Bools
        | _ -> failwithf "Unknown SortableArrayType: %s" s
    let all () : sortableArrayDataType list =
        [ Ints; Bools ]
 

