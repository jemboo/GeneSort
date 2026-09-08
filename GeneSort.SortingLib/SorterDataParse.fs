namespace GeneSort.SortingLib.Sorter

open System
open System.Text.RegularExpressions
open GeneSort.Sorting.Sorter

module SorterDataParse =

    /// Parses a string containing number pairs—ignoring brackets, newlines, 
    /// and whitespace—into an array of ce structs.
    let parseToFlatCeArray (s: string) : ce[] =
        if String.IsNullOrWhiteSpace(s) then 
            [||]
        else
            let matches = Regex.Matches(s, @"\d+")
            
            if matches.Count % 2 <> 0 then
                failwith "Malformed input string: contains an odd number of indices."

            [| for i in 0 .. 2 .. matches.Count - 1 ->
                let lowVal = Int32.Parse(matches.[i].Value)
                let hiVal  = Int32.Parse(matches.[i + 1].Value)
                ce.create lowVal hiVal |]

    /// Parses a string into a 2D array of ce structs (ce[][]) using square brackets [...] 
    /// to delineate stages, independent of line breaks.
    let parseTo2dCeArray (s: string) : ce[][] =
        if String.IsNullOrWhiteSpace(s) then
            [||]
        else
            // Match contents between square brackets [...]
            let stageMatches = Regex.Matches(s, @"\[(.*?)\]", RegexOptions.Singleline)
            
            if stageMatches.Count = 0 then
                [||]
            else
                [| for m in stageMatches -> parseToFlatCeArray m.Groups.[1].Value |]

    let getCeArrayFromPrefixLib (prefixKey:prefixLibId) : ce array option =
        (PrefixLib.tryGet prefixKey) |> Option.map (parseToFlatCeArray)

    let getCeArrayFromSorterLib (sorterKey:sorterLibId) : ce array option =
        (SorterLib.tryGet sorterKey) |> Option.map (parseToFlatCeArray)

    let get2dCeArrayFromSorterLib (sorterKey:sorterLibId) : ce array array option =
        (SorterLib.tryGet sorterKey) |> Option.map (parseTo2dCeArray)

    let getCeArrayFromMergeLib (mergeKey:mergeLibId) : ce array array option =
        let sorterKey = sorterLibId.create mergeKey.FactorSortingWidth mergeKey.SorterLibVariant
        let ceArrayOpt = (SorterLib.tryGet sorterKey) |> Option.map (parseTo2dCeArray)
        ceArrayOpt |> Option.map (Ce.merge2d mergeKey.MergeDimension mergeKey.SortingWidth)