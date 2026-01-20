namespace GeneSort.SortingOps

open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open FSharp.UMX

type ceBlockWithUsage =

    private { 
        ceBlock: ceBlock
        useCounts: ceUseCounts
        usedCes: Lazy<ce array>
        unsortedCount: int<sortableCount>
    }

    static member create 
            (ceBlock: ceBlock) 
            (ceUseCounts: ceUseCounts) 
            (unsortedCount: int<sortableCount>) =
        { 
            ceBlock = ceBlock; 
            useCounts = ceUseCounts;
            usedCes = Lazy<ce[]>(ceBlockWithUsage.getUsedCes ceBlock ceUseCounts)
            unsortedCount = unsortedCount
        }

    member this.CeBlock with get() = this.ceBlock

    member this.UseCounts with get() = this.useCounts

    member this.UnsortedCount with get() = this.unsortedCount

    member this.CeLength with get() = this.useCounts.Length |> UMX.tag<ceLength>

    member this.UsedCes with get() = this.usedCes.Value

    static member getUsedCes (ceb: ceBlock) (useCounts: ceUseCounts) : ce[] =
        let usedCes = ResizeArray<ce>()

        for i in 0 .. (%ceb.Length - 1) do
            if useCounts.[i |> UMX.tag<ceIndex>] > 0 then
                usedCes.Add(ceb.getCe i)
        usedCes.ToArray()


//module CeBlockUsage = ()