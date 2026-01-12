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

    member this.Increment (index: int) (amt: int) =
        this.useCounts.[index] <- this.useCounts.[index] + amt
        //// Interlocked.Add returns the new value, but we can ignore it.
        //// It ensures the addition is atomic across all CPU cores.
        //Interlocked.Add(&this.useCounts.[index], amt) |> ignore


module CeUseCounts = ()