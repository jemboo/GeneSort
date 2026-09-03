namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting.Sortable
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type ceBlockEval = 
    private { 
        prefix: ceBlock
        ceBlock: ceBlock
        ceUseCounts: ceUseCounts // Does not track the prefix CEs
        usedCes: Lazy<ce array>
        unsortedCount: int<sortableCount>
        sortableTest: sortableTest option
    }

    static member create 
            (prefix: ceBlock)
            (ceBlock: ceBlock) 
            (ceUseCts: ceUseCounts) 
            (unsortedCount: int<sortableCount>)
            (sortableTest: sortableTest option) =
        { 
            prefix = prefix
            ceBlock = ceBlock 
            ceUseCounts = ceUseCts
            usedCes = Lazy<ce[]>(fun () -> ceBlockEval.getUsedCes prefix ceBlock ceUseCts)
            unsortedCount = unsortedCount
            sortableTest = sortableTest 
        }

    member this.Prefix with get() = this.prefix

    member this.CeBlock with get() = this.ceBlock

    member this.LastUsedIndex with get() : int<ceIndex> =
        this.ceUseCounts.LastUsedCeIndex

    member this.UseCountArray with get() : int array = this.ceUseCounts.ToArray()

    // Includes the prefix CEs with all the used CEs from the CE block
    member this.CeLength with get() : int<ceLength> =
        this.prefix.CeLength + this.ceUseCounts.UsedCeCount

    // Includes the prefix CEs
    static member getUsedCes (prefix: ceBlock) (ceb: ceBlock) (useCounts: ceUseCounts) : ce[] =
        let blockCes = 
            let used = ResizeArray<ce>()
            for i in 0 .. (%ceb.CeLength - 1) do
                if useCounts.[i |> UMX.tag<ceIndex>] <> 0 then
                    used.Add(ceb.getCe i)
            used.ToArray()
        Array.append prefix.CeArray blockCes

    member this.SortableTest with get() = this.sortableTest

    member this.UnsortedCount with get() = this.unsortedCount

    member this.UsedCes with get() : ce array = this.usedCes.Value

    // Includes the prefix CEs with all the used CEs from the CE block
    member this.extractCeUseArray : ceUse array =
        let results = ResizeArray<ceUse>()
        let prefixLength = this.Prefix.CeLength
        for i in 0 .. (%prefixLength - 1) do
            let idx = i |> UMX.tag<ceIndex>
            let count = -1 // Prefix CEs are always considered used
            results.Add(ceUse.create idx count (this.Prefix.getCe i))

        for i in 0 .. (%this.CeBlock.CeLength - 1) do
            let idx = i |> UMX.tag<ceIndex>
            let count = this.ceUseCounts.[idx]
            if count > 0 then
                let offsetPos = idx + (prefixLength |> UMX.cast<ceLength,ceIndex>)
                results.Add(ceUse.create offsetPos count (this.CeBlock.getCe i))
        results.ToArray()




    //let private extractCeUseArray (ceb: ceBlock) (useCounts: ceUseCounts) : ceUse array =
    //    let results = ResizeArray<ceUse>()
    //    for i in 0 .. (%ceb.CeLength - 1) do
    //        let idx = i |> UMX.tag<ceIndex>
    //        let count = useCounts.[idx]
    //        if count > 0 then
    //            results.Add(ceUse.create idx count (ceb.getCe i))
    //    results.ToArray()