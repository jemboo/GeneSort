﻿
namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter


type stageSequence = 
        private { 
                    sortingWidth: int<sortingWidth>; 
                    stages: ResizeArray<stage>;  
                } with

        static member create (sortingWidth: int<sortingWidth>) : stageSequence =
            if sortingWidth <= 1<sortingWidth> then
                invalidArg "sortingWidth" "Sorting width must be greater than 1."
            { sortingWidth = sortingWidth; stages = ResizeArray<stage>() }

        member this.SortingWidth with get() = this.sortingWidth

        member this.Stages with get() = this.stages.ToArray()

        member this.StageCount with get() = this.stages.Count

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

// Core module for Stage operations
module StageSequence =

    let fromSorter (sorter: sorter) : stageSequence =
        let stageSeq = stageSequence.create(sorter.SortingWidth)
        for ce in sorter.Ces do
            stageSeq.AddCe(ce)
        stageSeq

