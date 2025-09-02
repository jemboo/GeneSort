namespace GeneSort.SortingOps

open GeneSort.Sorter.Sortable

type ceBlockEval =

    private { 
        ceBlockWithUsage: ceBlockWithUsage
        sorterTests: sorterTests
    }

    static member create (ceBlockUsage: ceBlockWithUsage) (sorterTests: sorterTests) =
        { 
            ceBlockWithUsage = ceBlockUsage; 
            sorterTests = sorterTests; 
        }

    member this.CeBlock with get() = this.ceBlockWithUsage.ceBlock
    member this.SorterTests with get() = this.sorterTests
    member this.CeUseCounts with get() : int[] = this.ceBlockWithUsage.useCounts

