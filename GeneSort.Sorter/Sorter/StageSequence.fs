
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

        member this.SortingWidth with get() = this.sortingWidth

        member this.Stages with get() = this.stages.ToArray()

        member this.StageCount with get() = this.stages.Count

       // member this.AddCe (ce:Ce)

// Core module for Stage operations
module StageSequence = ()

