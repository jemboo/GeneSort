namespace GeneSort.SortingOps

open FSharp.UMX
open System.Threading

type ceUseCounts = 
    private { 
        useCounts: int array
    }

    static member create (ceBlockLength: int<ceBlockLength>) =
        if %ceBlockLength < 0 then
            invalidArg "ceBlockLength" "ceBlockLength must be non-negative"
        { useCounts = Array.zeroCreate %ceBlockLength }

    member this.UseCounts with get () = Array.copy this.useCounts

/// Thread-safe increment
    member this.Increment (index: int) (amt: int) =
        // Interlocked.Add returns the new value, but we can ignore it.
        // It ensures the addition is atomic across all CPU cores.
        Interlocked.Add(&this.useCounts.[index], amt) |> ignore


module CeUseCounts = ()