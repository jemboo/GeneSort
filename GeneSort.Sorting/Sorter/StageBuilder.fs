
namespace GeneSort.Sorting.Sorter

open FSharp.UMX
open GeneSort.Sorting


type stageBuilder = 
        private { 
                    sortingWidth: int<sortingWidth>; 
                    ces: ResizeArray<ce>;  
                    occupied: bool[] 
                } with

        member this.SortingWidth with get() = this.sortingWidth

        member this.Ces with get() = this.ces.ToArray()

        member this.CeCount with get() = this.ces.Count

        member this.IsOccupied(index: int) = 
            if index < 0 || index >= %this.sortingWidth then
                invalidArg "index" $"Index {index} is out of bounds for Ce array of length {this.ces.Count}."
            this.occupied.[index]

        member this.AddCe(ce: ce) =
            if this.occupied.[ce.Low] then
                invalidArg "ce" $"Cannot add CE {ce} because position {ce.Low} is already occupied."
            if this.occupied.[ce.Hi] then 
                invalidArg "ce" $"Cannot add CE {ce} because position {ce.Hi} is already occupied."
            this.ces.Add(ce)
            this.occupied.[ce.Low] <- true
            this.occupied.[ce.Hi] <- true

        member this.CanAddCe(ce: ce) =
            not (this.occupied.[ce.Low] || this.occupied.[ce.Hi])

// Core module for Stage operations
module StageBuilder = ()



type stageBuilderSequence = 
        private { 
                    sortingWidth: int<sortingWidth>; 
                    stageBuilders: ResizeArray<stageBuilder>;  
                } with

        static member create (sortingWidth: int<sortingWidth>) : stageBuilderSequence =
            if sortingWidth <= 1<sortingWidth> then
                invalidArg "sortingWidth" "Sorting width must be greater than 1."
            { sortingWidth = sortingWidth; stageBuilders = ResizeArray<stageBuilder>() }

        member this.SortingWidth with get() = this.sortingWidth

        member this.StageBuilders with get() = this.stageBuilders.ToArray()

        member this.StageLength with get() = this.stageBuilders.Count |> UMX.tag<stageLength>

        member this.AddCe (ce: ce) =
            if ce.Low >= (this.sortingWidth |> int) || ce.Hi >= (this.sortingWidth |> int) then
                invalidArg "ce" $"CE {ce} has indices out of bounds for sorting width {this.sortingWidth}."

            // Find the last stage that CANNOT accept the CE
            let firstBlockedIndex = 
                this.stageBuilders 
                |> Seq.tryFindIndexBack (fun sb -> not (sb.CanAddCe(ce)))

            // If a stage is blocked at index `i`, the target is the one right after it (`i + 1`)
            // If no stages are blocked, it means every stage from the end back to 0 can accept it, 
            // so the earliest available stage is index 0.
            let targetStageIndex = 
                match firstBlockedIndex with
                | Some i -> i + 1
                | None   -> 0

            // If the calculated index is beyond our existing stages, it means we need a new stage
            if targetStageIndex >= this.stageBuilders.Count then
                let newStage = 
                    { 
                        sortingWidth = this.sortingWidth
                        ces = ResizeArray<ce>()
                        occupied = Array.create (this.sortingWidth |> int) false
                    }
                newStage.AddCe(ce)
                this.stageBuilders.Add(newStage)
            else
                // Add to the earliest stage that can accept the CE
                this.stageBuilders.[targetStageIndex].AddCe(ce)



        //member this.AddCe (ce: ce) =
        //    if ce.Low >= (this.sortingWidth |> int) || ce.Hi >= (this.sortingWidth |> int) then
        //        invalidArg "ce" $"CE {ce} has indices out of bounds for sorting width {this.sortingWidth}."

        //    let mutable targetStageIndex = -1
        //    // Start from the last stage and work backwards
        //    for i = this.stageBuilders.Count - 1 downto 0 do
        //        if this.stageBuilders.[i].CanAddCe(ce) then
        //            targetStageIndex <- i

        //    // If no stage can accept the CE, create a new stage
        //    if targetStageIndex = -1 then
        //        let newStage = 
        //            { 
        //                sortingWidth = this.sortingWidth
        //                ces = ResizeArray<ce>()
        //                occupied = Array.create (this.sortingWidth |> int) false
        //            }
        //        newStage.AddCe(ce)
        //        this.stageBuilders.Add(newStage)
        //    else
        //        // Add to the earliest stage that can accept the CE
        //        this.stageBuilders.[targetStageIndex].AddCe(ce)


module StageBuilderSequence =

    let fromCes (sortingWidth : int<sortingWidth>) (ces: ce array) : stageBuilderSequence =
        let stageBuilderSeq = stageBuilderSequence.create(sortingWidth)
        ces |> Array.iter stageBuilderSeq.AddCe
        stageBuilderSeq


    let toStageSequence  (sortingWidth : int<sortingWidth>) (ces: ce array)  : stageSequence =
        let stageBuilderSeq = fromCes sortingWidth ces
        let stages = stageBuilderSeq.StageBuilders |> Array.map(fun st -> stage.create st.Ces)
        stageSequence.create stages sortingWidth

