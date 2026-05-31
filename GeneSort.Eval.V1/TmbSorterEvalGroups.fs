namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.SortingOps
open System


namespace GeneSort.Core

open System
open GeneSort.SortingOps

type tmbSorterEvalGroups = 
    private {
        top: sorterEval array
        middle: sorterEval array
        bottom: sorterEval array
    }
    
    static member create (top: sorterEval array) (middle: sorterEval array) (bottom: sorterEval array) =
        { top = top; middle = middle; bottom = bottom }

    member this.Top = this.top
    member this.Middle = this.middle
    member this.Bottom = this.bottom

    /// Converts the three explicit ranked groups into an itemized loop context for reports
    member this.ToGroupedArray() : array<rankedGroup * sorterEval array> =
        [|
            (Top, this.top)
            (Middle, this.middle)
            (Bottom, this.bottom)
        |]

module TmbSorterEvalGroups =

    /// Builds a rankedSorterGroups instance from a flat array using the specified ranker and size parameters
    let fromEvaluations 
            (ranker: sorterEval -> float) 
            (groupSize: int) 
            (items: sorterEval array) : tmbSorterEvalGroups =
            
        let targetSize = Math.Min(groupSize, items.Length / 3)
        
        if targetSize <= 0 then 
            tmbSorterEvalGroups.create Array.empty Array.empty Array.empty
        else
            let sortedItems = items |> Array.sortByDescending ranker
            let topGroup = sortedItems.[0 .. targetSize - 1]
            let midStart = items.Length / 2 - (targetSize / 2)
            let midGroup = sortedItems.[midStart .. midStart + targetSize - 1]
            let botGroup = sortedItems.[items.Length - targetSize .. items.Length - 1]
            
            tmbSorterEvalGroups.create topGroup midGroup botGroup


    let toDataTableRecords 
                (leadCols: dataTableRecord) 
                (recordMaker: sorterEval -> dataTableRecord [])
                (groups: tmbSorterEvalGroups) : dataTableRecord array =
        
            groups.ToGroupedArray()
            |> Array.collect (fun (groupTag, evals) -> 
                // 1. Build the shared contextual header for this group tier
                let groupHeaderDtr = 
                    groupTag 
                    |> RankedGroup.toDataTableRecord 
                    |> dataTableRecord.combine leadCols
            
                // 2. Map items via the injected handler function and append the headers
                evals
                |> Array.collect recordMaker
                |> Array.map (fun customDtr -> 
                    customDtr |> dataTableRecord.combine groupHeaderDtr
                )
            )