namespace GeneSort.SortingOps

open FSharp.UMX

type ceBlockUsage = 
    private { 
        uses: int array
    }

    static member create(ceBlockLength: int<ceBlockLength>) =
        if %ceBlockLength < 0 then
            invalidArg "ceBlockLength" "ceBlockLength must be non-negative"
        { uses = Array.zeroCreate %ceBlockLength }

    member this.getUse (dex:int) = 
        if dex < 0 || dex >= this.uses.Length then
            invalidArg "dex" $"Index {dex} is out of bounds for uses array of length {this.uses.Length}."
        this.uses.[dex]

    member this.BlockLength with get() = this.uses.Length |> UMX.tag<ceBlockLength>

    member this.IsUsed (index: int) =
        if index < 0 || index >= this.uses.Length then
            invalidArg "index" $"Index {index} is out of bounds for uses array of length {this.uses.Length}."
        this.uses[index] > 0

    member this.Increment (index: int) =
        this.uses[index] <- this.uses[index] + 1


module CeUsage = ()