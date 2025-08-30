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

    member this.UseCounts with get () = Array.copy this.useCounts

    member this.Increment (index: int) =
        this.useCounts[index] <- this.useCounts[index] + 1


module CeUsage = ()