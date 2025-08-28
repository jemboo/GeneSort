namespace GeneSort.Sorter.Sortable
open FSharp.UMX

type ceUsage = 
    private { 
        uses: int array
    }

    static member create(ceBlockLength: int<ceBlockLength>) =
        if %ceBlockLength < 0 then
            invalidArg "ceBlockLength" "ceBlockLength must be non-negative"
        { uses = Array.zeroCreate %ceBlockLength }

    member this.Uses with get() = this.uses

    member this.Increment (index: int) =
        this.uses[index] <- this.uses[index] + 1


module CeUsage = ()