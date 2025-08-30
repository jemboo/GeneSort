namespace GeneSort.SortingOps

open FSharp.UMX

type ceUseCounts = 
    private { 
        useCounts: int array
    }

    static member create (ceBlockLength: int<ceBlockLength>) =
        if %ceBlockLength < 0 then
            invalidArg "ceBlockLength" "ceBlockLength must be non-negative"
        { useCounts = Array.zeroCreate %ceBlockLength }

    //member this.getUseCount (dex:int) = 
    //    if dex < 0 || dex >= this.useCounts.Length then
    //        invalidArg "dex" $"Index {dex} is out of bounds for uses array of length {this.useCounts.Length}."
    //    this.useCounts.[dex]

    member this.getUseCounts() = Array.copy this.useCounts

    //member this.BlockLength with get() = this.useCounts.Length |> UMX.tag<ceBlockLength>

    //member this.IsUsed (index: int) =
    //    if index < 0 || index >= this.useCounts.Length then
    //        invalidArg "index" $"Index {index} is out of bounds for uses array of length {this.useCounts.Length}."
    //    this.useCounts[index] > 0

    member this.Increment (index: int) =
        this.useCounts[index] <- this.useCounts[index] + 1


module CeUsage = ()