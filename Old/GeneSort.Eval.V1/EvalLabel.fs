namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Core


type evalLabel = 
    | Tmb of tmbGroup
    | RankIndex of int
    | ValueIndex of int
    | Rank of int


module EvalLabel =
    // Helper to format integer-based metrics consistently
    let private formatIntGroup prefix (values: int list) =
        match values with
        | [] -> ""
        | [single] -> sprintf "%s:1[%d]" prefix single
        | xs -> sprintf "%s:%d[%d-%d]" prefix xs.Length (List.min xs) (List.max xs)

    let toString (evalLabels: evalLabel seq) : string =
        // State tracking for our single-pass fold
        let tmbs, rankIndices, valueIndices, ranks =
            (([], [], [], []), evalLabels)
            ||> Seq.fold (fun (t, ri, vi, r) -> function
                | Tmb g        -> (g :: t, ri, vi, r)
                | RankIndex i  -> (t, i :: ri, vi, r)
                | ValueIndex i -> (t, ri, i :: vi, r)
                | Rank i       -> (t, ri, vi, i :: r))

        // 1. Process TMB counts
        let tmbStr = 
            match tmbs with
            | [] -> ""
            | xs -> 
                let counts = 
                    xs 
                    |> List.countBy id 
                    |> List.map (fun (g, count) -> sprintf "%A(%d)" g count)
                    |> String.concat ","
                sprintf "tmb:%s" counts

        // 2. Process integer ranges
        let riStr = formatIntGroup "ri" rankIndices
        let viStr = formatIntGroup "vi" valueIndices
        let rStr  = formatIntGroup "r" ranks

        // Combine only the active segments
        [ tmbStr; riStr; viStr; rStr ]
        |> List.filter (fun s -> not (System.String.IsNullOrEmpty s))
        |> String.concat ","


