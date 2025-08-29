namespace GeneSort.SortingOps

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
