namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable

type ceBlockEval = 
    private { 
        ceBlock: ceBlock
        sorterTests: sorterTests
        ceBlockUsage: ceBlockUsage
    }

    static member create(ceBlock: ceBlock, sorterTests: sorterTests, ceBlockUsage: ceBlockUsage) =
        { 
            ceBlock = ceBlock; 
            sorterTests = sorterTests; 
            ceBlockUsage = ceBlockUsage
        }

    member this.CeBlock = this.ceBlock
    member this.SorterTests = this.sorterTests
    member this.CeBlockUsage = this.ceBlockUsage

    member this.Increment(index: int) =
        this.ceBlockUsage.Increment(index)

    member this.getUsedCes() : ce[] =
        let usedCes = ResizeArray<ce>()

        for i in 0 .. (%this.ceBlock.Length - 1) do
            if this.ceBlockUsage.IsUsed(i) then
                usedCes.Add(this.ceBlock.getCe i)

        usedCes.ToArray()
