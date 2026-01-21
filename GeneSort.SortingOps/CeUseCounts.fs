namespace GeneSort.SortingOps

open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting


// -----------------------------
// CE use-count container
// -----------------------------

type ceUseCounts =
    private {
        useCounts: int array
    }

    // --------
    // Factory
    // --------

    static member Create (ceBlockLength: int<ceBlockLength>) =
        let len = %ceBlockLength
        if len < 0 then
            invalidArg "ceBlockLength" "ceBlockLength must be non-negative"
        { useCounts = Array.zeroCreate len }


    static member CreateFromArray (counts: int[]) =
        if counts = null then
            invalidArg "counts" "counts array cannot be null"
        { useCounts = Array.copy counts }

    // ----------------
    // Basic properties
    // ----------------

    /// Number of CEs that have been used at least once
    member this.UsedCeCount : int<ceLength> =
        (Array.fold (fun acc count -> if count > 0 then acc + 1 else acc) 0 this.useCounts)
        |> UMX.tag<ceLength>
 

    /// Number of CEs tracked
    member this.Length : int =
        this.useCounts.Length

    /// Safe snapshot (no external mutation)
    member this.ToArray() : int[] =
        Array.copy this.useCounts

    // ----------------
    // Indexer
    // ----------------

    member this.Item
        with get (index: int<ceIndex>) : int =
            this.useCounts.[%index]

    // ----------------
    // Mutations
    // ----------------

    /// Increment by a given amount (non-atomic)
    member this.IncrementBy (index: int<ceIndex>) (amount: int) : unit =
        this.useCounts.[%index] <- this.useCounts.[%index] + amount

    /// Increment by 1 (non-atomic)
    member this.Increment (index: int<ceIndex>) : unit =
        this.useCounts.[%index] <- this.useCounts.[%index] + 1

    /// Atomic increment (for parallel analysis passes)
    member this.IncrementAtomicBy (index: int<ceIndex>) (amount: int) : unit =
        Interlocked.Add(&this.useCounts.[%index], amount) |> ignore

    /// Atomic increment by 1
    member this.IncrementAtomic (index: int<ceIndex>) : unit =
        Interlocked.Increment(&this.useCounts.[%index]) |> ignore

    member this.LastUsedCeIndex : int<ceIndex> =
        ArrayUtils.lastNonZeroIndex this.useCounts
        |> UMX.tag<ceIndex>


