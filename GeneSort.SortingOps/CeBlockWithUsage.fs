namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorter.Sorter

type ceBlockWithUsage =

    private { 
        ceBlock: ceBlock
        useCounts: int array
        usedCes: Lazy<ce array>
    }

    static member create (ceBlock: ceBlock) (useCounts: int[]) =
        { 
            ceBlock = ceBlock; 
            useCounts = useCounts;
            usedCes = Lazy<ce[]>(ceBlockWithUsage.getUsedCes ceBlock useCounts)
        }

    member this.useCount (dex:int) = 
        if dex < 0 || dex >= this.useCounts.Length then
            invalidArg "dex" $"Index {dex} is out of bounds for uses array of length {this.useCounts.Length}."
        this.useCounts.[dex]

    member this.CeBlock with get() = this.ceBlock

    member this.UseCounts with get() = Array.copy this.useCounts

    member this.BlockLength with get() = this.useCounts.Length |> UMX.tag<ceBlockLength>

    member this.UsedCes with get() = this.usedCes.Value

    static member getUsedCes (ceb: ceBlock) (useCounts: int[]) : ce[] =
        let usedCes = ResizeArray<ce>()

        for i in 0 .. (%ceb.Length - 1) do
            if useCounts.[i] > 0 then
                usedCes.Add(ceb.getCe i)
        usedCes.ToArray()


//module CeBlockUsage = ()