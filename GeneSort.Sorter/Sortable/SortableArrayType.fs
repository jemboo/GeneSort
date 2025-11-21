
namespace GeneSort.Sorter.Sortable

type sortableArrayType = 
    | Ints
    | Bools

module SortableArrayType =

    let toString (t:sortableArrayType) : string =
        match t with
        | Ints -> "Ints"
        | Bools -> "Bools"

    let fromString (s:string) : sortableArrayType =
        match s with
        | "Ints" -> Ints
        | "Bools" -> Bools
        | _ -> failwithf "Unknown SortableArrayType: %s" s
    let all () : sortableArrayType list =
        [ Ints; Bools ]
 

