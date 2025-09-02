namespace GeneSort.SortingOps

open GeneSort.Sorter.Sortable

type ceBlockEval = 
    private { 
        ceBlockUsage: ceBlockWithUsage
        sorterTests: sorterTests
    }

    static member create (ceBlockUsage: ceBlockWithUsage) (sorterTests: sorterTests) =
        { 
            ceBlockUsage = ceBlockUsage; 
            sorterTests = sorterTests; 
        }

    member this.CeBlock = this.ceBlockUsage.ceBlock
    member this.SorterTests = this.sorterTests
    member this.CeUseCounts :int[] = this.ceBlockUsage.useCounts

