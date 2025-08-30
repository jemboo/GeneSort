namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable

type ceBlockEval = 
    private { 
        ceBlockUsage: ceBlockUsage
        sorterTests: sorterTests
    }

    static member create (ceBlockUsage: ceBlockUsage) (sorterTests: sorterTests) =
        { 
            ceBlockUsage = ceBlockUsage; 
            sorterTests = sorterTests; 
        }

    member this.CeBlock = this.ceBlockUsage.ceBlock
    member this.SorterTests = this.sorterTests
    member this.CeUseCounts :int[] = this.ceBlockUsage.useCounts

