
namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter


type stage = 
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
module Stage = ()

