namespace GeneSort.SortingLib.Sorter

open System
open System.Text.RegularExpressions
open GeneSort.Sorting.Sorter


module SorterDataParse =
    // Parses a string with the following format into an array of ce
    // Ignores the brackets, rather, stage grouping will be done elsewhere.
    //"[(0,2),(1,3)]
    // [(0,1),(2,3)]"
    /// Parses a string containing number pairs—ignoring brackets, newlines, 
    /// and whitespace—into an array of ce structs.
    let parseCeArray (s: string) : ce[] =
        if String.IsNullOrWhiteSpace(s) then 
            [||]
        else
            // Match any sequence of digits, capturing them in pairs
            // This implicitly strips out \r, \n, [, ], (, ), and spacing
            let matches = Regex.Matches(s, @"\d+")
            
            // Ensure we have an even number of integers to make complete pairs
            if matches.Count % 2 <> 0 then
                failwith "Malformed input string: contains an odd number of indices."

            [| for i in 0 .. 2 .. matches.Count - 1 ->
                let lowVal = Int32.Parse(matches.[i].Value)
                let hiVal  = Int32.Parse(matches.[i + 1].Value)
                ce.create lowVal hiVal |]



    let getCeArrayFromLib (sorterKey:sorterLibId) =
        (SorterData.tryGet sorterKey) |> Option.map (parseCeArray)
