namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorter.Sorter

type ceBlockUsage = 
    private { 
        ceBlock: ceBlock
        useCounts: int array
    }

    static member create (ceBlock: ceBlock) (useCounts: int[]) =
        { ceBlock = ceBlock; useCounts = useCounts }

    member this.getUseCount (dex:int) = 
        if dex < 0 || dex >= this.useCounts.Length then
            invalidArg "dex" $"Index {dex} is out of bounds for uses array of length {this.useCounts.Length}."
        this.useCounts.[dex]

    member this.BlockLength with get() = this.useCounts.Length |> UMX.tag<ceBlockLength>

    member this.getUsedCes() : ce[] =
        let usedCes = ResizeArray<ce>()

        for i in 0 .. (%this.ceBlock.Length - 1) do
            if this.useCounts.[i] > 0 then
                usedCes.Add(this.ceBlock.getCe i)

        usedCes.ToArray()


//module CeUsage = ()