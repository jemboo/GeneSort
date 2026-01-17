namespace GeneSort.SortingOps

open GeneSort.Sorter.Sortable

type ceBlockEval =

    private { 
        ceBlockWithUsage: ceBlockWithUsage
        sortableTests: sortableTests
    }

    static member create (ceBlockUsage: ceBlockWithUsage) (sortableTests: sortableTests) =
        { 
            ceBlockWithUsage = ceBlockUsage; 
            sortableTests = sortableTests; 
        }

    member this.CeBlockWithUsage with get() = this.ceBlockWithUsage
    member this.SortableTests with get() = this.sortableTests

