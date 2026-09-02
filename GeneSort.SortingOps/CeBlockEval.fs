namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting.Sortable
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type ceBlockEval = 
    private { 
        prefix: ceBlock
        ceBlock: ceBlock
        ceUseCounts: ceUseCounts
        usedCes: Lazy<ce array>
        unsortedCount: int<sortableCount>
        sortableTest: sortableTest option
    }

    static member create 
            (prefix: ceBlock)
            (ceBlock: ceBlock) 
            (ceUseCounts: ceUseCounts) 
            (unsortedCount: int<sortableCount>)
            (sortableTest: sortableTest option) =
        { 
            prefix = prefix
            ceBlock = ceBlock 
            ceUseCounts = ceUseCounts
            usedCes = Lazy<ce[]>(fun () -> ceBlockEval.getUsedCes prefix ceBlock ceUseCounts)
            unsortedCount = unsortedCount
            sortableTest = sortableTest 
        }

    member this.Prefix with get() = this.prefix

    member this.CeBlock with get() = this.ceBlock

    /// Returns a new ceUseCounts containing -1 for each prefix CE followed by the original use counts.
    member this.CeUseCounts with get() : ceUseCounts = 
        let prefixLen = %this.prefix.CeLength
        let combined = Array.init (prefixLen + this.ceUseCounts.Length) (fun i ->
            if i < prefixLen then -1
            else this.ceUseCounts.[(i - prefixLen) |> UMX.tag<ceIndex>]
        )
        ceUseCounts.CreateFromArray combined

    static member getUsedCes (prefix: ceBlock) (ceb: ceBlock) (useCounts: ceUseCounts) : ce[] =
        let prefixCes = prefix.CeArray
        
        let blockCes = 
            let used = ResizeArray<ce>()
            for i in 0 .. (%ceb.CeLength - 1) do
                if useCounts.[i |> UMX.tag<ceIndex>] <> 0 then
                    used.Add(ceb.getCe i)
            used.ToArray()

        Array.append prefixCes blockCes

    member this.SortableTest with get() = this.sortableTest

    member this.UnsortedCount with get() = this.unsortedCount

    member this.UsedCes with get() : ce array = this.usedCes.Value