namespace GeneSort.Core
open System


/// Represents the tier assignment for each categorized block of items
type rankedGroup =
    | Top
    | Middle
    | Bottom

module RankedGroup =

    let toDataTableRecord (group: rankedGroup) : dataTableRecord =
        let groupStr = 
            match group with
            | Top -> "Top"
            | Middle -> "Middle"
            | Bottom -> "Bottom"
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData "RankedGroup" groupStr

    /// Extracts three groups of items, each of size min(groupSize, items.Length/3)
    /// from the input sequence based on their rank as determined by the ranker function.
    let splitIntoRankedGroups<'a> 
                    (ranker: 'a -> float) 
                    (groupSize: int) 
                    (items: array<'a>) : array<dataTableRecord * ('a [])> =
        
        // 1. Determine safe target slice size bounded by 1/3rd of the collection
        let targetSize = Math.Min(groupSize, items.Length / 3)
        
        // Return an empty sequence safely if the bounds allow zero item captures
        if targetSize <= 0 then 
            Array.empty
        else
            // 2. Sort all items descending by rank once to establish array positions
            let sortedItems = items |> Array.sortByDescending ranker

            // 3. Extract exact chunks using array slicing
            let topGroup = sortedItems.[0 .. targetSize - 1]
            
            // Middle group starts at the midpoint index of the total array
            let midStart = items.Length / 2 - (targetSize / 2)
            let midGroup = sortedItems.[midStart .. midStart + targetSize - 1]
            
            // Bottom group grabs the absolute lowest ranking tail items
            let botGroup = sortedItems.[items.Length - targetSize .. items.Length - 1]

            [| 
                (Top |> toDataTableRecord, topGroup)
                (Middle |> toDataTableRecord, midGroup)
                (Bottom |> toDataTableRecord, botGroup)
            |]
