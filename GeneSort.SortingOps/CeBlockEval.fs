namespace GeneSort.SortingOps

open FSharp.UMX

open GeneSort.Sorting.Sortable
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type ceBlockEval =

    private { 
        ceBlock: ceBlock
        ceUseCounts: ceUseCounts
        usedCes: Lazy<ce array>
        unsortedCount: int<sortableCount>
        sortableTest: sortableTest option
    }

    static member create 
            (ceBlock: ceBlock) 
            (ceUseCounts: ceUseCounts) 
            (unsortedCount: int<sortableCount>)
            (sortableTest: sortableTest option) =
        { 
            ceBlock = ceBlock; 
            ceUseCounts = ceUseCounts;
            usedCes = Lazy<ce[]>(ceBlockEval.getUsedCes ceBlock ceUseCounts)
            unsortedCount = unsortedCount
            sortableTest = sortableTest; 
        }

    member this.CeBlock with get() = this.ceBlock

    member this.CeUseCounts with get() = this.ceUseCounts

    static member getUsedCes (ceb: ceBlock) (useCounts: ceUseCounts) : ce[] =
        let usedCes = ResizeArray<ce>()

        for i in 0 .. (%ceb.CeLength - 1) do
            if useCounts.[i |> UMX.tag<ceIndex>] > 0 then
                usedCes.Add(ceb.getCe i)
        usedCes.ToArray()

    member this.SortableTest with get() = this.sortableTest

    member this.UnsortedCount with get() = this.unsortedCount

    member this.UsedCes with get() = this.usedCes.Value


