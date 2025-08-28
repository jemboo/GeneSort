namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open System.Linq
open System.Collections.Generic



type ceBlockEval = 
    private { 
        ceBlock: ceBlock
        sorterTests: sorterTests
        ceBlockUsage: ceBlockUsage
    }

    static member create(ceBlock: ceBlock, sorterTests: sorterTests ) =
        { 
            ceBlock = ceBlock; 
            sorterTests = sorterTests; 
            ceBlockUsage = ceBlockUsage.create ceBlock.Length 
        }

    member this.CeBlock = this.ceBlock
    member this.SorterTests = this.sorterTests
    member this.CeBlockUsage = this.ceBlockUsage

    member this.Increment(index: int) =
        this.ceBlockUsage.Increment(index)



module CeBlockEval =

    let evaluateCeBlock 
                (ceBlock: ceBlock) 
                (sorterTests: sorterTests) 
                (useCounter: int[]) 
                : ceBlockEval =
        let ceBlockEval = ceBlockEval.create(ceBlock, sorterTests)
        match sorterTests with
        | sorterTests.Bools boolTests ->
            for sortableArray in boolTests.SortableBoolArrays do
                let sortedArray = sortableArray.SortByCes ceBlock.Ces useCounter
                if not sortedArray.IsSorted then
                    ceBlockEval.Increment(1)
        | sorterTests.Ints intTests ->
            for sortableArray in intTests.SortableIntArrays do
                let sortedArray = sortableArray.SortByCes ceBlock.Ces useCounter
                if not sortedArray.IsSorted then
                    ceBlockEval.Increment(1)
        | _ -> failwith "Unsupported sorterTests type"
        ceBlockEval

