
namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Core
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
                    stages: ResizeArray<stageBuilder>;  
                } with

        static member create (sortingWidth: int<sortingWidth>) : stageBuilderSequence =
            if sortingWidth <= 1<sortingWidth> then
                invalidArg "sortingWidth" "Sorting width must be greater than 1."
            { sortingWidth = sortingWidth; stages = ResizeArray<stageBuilder>() }

        member this.SortingWidth with get() = this.sortingWidth

        member this.Stages with get() = this.stages.ToArray()

        member this.StageLength with get() = this.stages.Count |> UMX.tag<stageLength>

        member this.AddCe (ce: ce) =
            if ce.Low >= (this.sortingWidth |> int) || ce.Hi >= (this.sortingWidth |> int) then
                invalidArg "ce" $"CE {ce} has indices out of bounds for sorting width {this.sortingWidth}."

            let mutable targetStageIndex = -1
            // Start from the last stage and work backwards
            for i = this.stages.Count - 1 downto 0 do
                if this.stages.[i].CanAddCe(ce) then
                    targetStageIndex <- i

            // If no stage can accept the CE, create a new stage
            if targetStageIndex = -1 then
                let newStage = 
                    { 
                        sortingWidth = this.sortingWidth
                        ces = ResizeArray<ce>()
                        occupied = Array.create (this.sortingWidth |> int) false
                    }
                newStage.AddCe(ce)
                this.stages.Add(newStage)
            else
                // Add to the earliest stage that can accept the CE
                this.stages.[targetStageIndex].AddCe(ce)


module StageBuilderSequence =

    let fromCes (sortingWidth : int<sortingWidth>) (ces: ce array) : stageBuilderSequence =
        let stageSeq = stageBuilderSequence.create(sortingWidth)
        ces |> Array.iter stageSeq.AddCe
        stageSeq

