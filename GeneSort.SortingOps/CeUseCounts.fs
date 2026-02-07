namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Component
open System.Threading


// -----------------------------
// CE use-count container
// -----------------------------

[<CustomEquality; NoComparison>]
type ceUseCounts =
    private {
        useCounts: int array
        // 0 is the sentinel for "needs recomputation"
        mutable mutableHash: int 
    }

    static member Create (ceLength: int<ceLength>) =
        { useCounts = Array.zeroCreate %ceLength; mutableHash = 0 }

    static member CreateFromArray (counts: int[]) =
        { useCounts = Array.copy counts; mutableHash = 0 }

    // ----------------
    // Robust Hashing (Single-Threaded)
    // ----------------

    override this.GetHashCode() =
        if this.mutableHash <> 0 then this.mutableHash
        else
            // Robust FNV-1a Hash + Avalanche
            let mutable h = 2166136261u
            let arr = this.useCounts
            for i = 0 to arr.Length - 1 do
                h <- (h ^^^ uint32 arr.[i]) * 16777619u
            
            // Final mixing (avalanche) to ensure quality
            h <- h ^^^ (h >>> 16)
            h <- h * 0x85ebca6bu
            h <- h ^^^ (h >>> 13)
            h <- h * 0xc2b2ae35u
            h <- h ^^^ (h >>> 16)

            // Ensure we never return 0 so the cache is valid
            let finalHash = if h = 0u then 1 else int h
            this.mutableHash <- finalHash
            finalHash

    override this.Equals(obj) =
        match obj with
        | :? ceUseCounts as other ->
            // Check hash first: extremely fast for rejecting inequality
            if this.GetHashCode() <> other.GetHashCode() then false
            else 
                // Use Span for vectorized hardware-accelerated equality check
                System.ReadOnlySpan<int>(this.useCounts)
                    .SequenceEqual(System.ReadOnlySpan<int>(other.useCounts))
        | _ -> false

    // ----------------
    // Fast Mutations
    // ----------------

    member inline private this.Invalidate() =
        this.mutableHash <- 0

    member this.IncrementBy (index: int<ceIndex>) (amount: int) : unit =
        this.useCounts.[%index] <- this.useCounts.[%index] + amount
        this.Invalidate()

    member this.Increment (index: int<ceIndex>) : unit =
        this.useCounts.[%index] <- this.useCounts.[%index] + 1
        this.Invalidate()

    // ----------------
    // Mutation with Thread-Safe Invalidation
    // ----------------

    member this.IncrementAtomicBy (index: int<ceIndex>) (amount: int) : unit =
        Interlocked.Add(&this.useCounts.[%index], amount) |> ignore
        this.Invalidate()

    member this.IncrementAtomic (index: int<ceIndex>) : unit =
        Interlocked.Increment(&this.useCounts.[%index]) |> ignore
        this.Invalidate()


    // ----------------
    // Properties
    // ----------------

    member this.Length = this.useCounts.Length
    
    member this.Item with get (index: int<ceIndex>) = this.useCounts.[%index]

    /// Number of CEs that have been used at least once
    member this.UsedCeCount : int<ceLength> =
        (Array.fold (fun acc count -> if count > 0 then acc + 1 else acc) 0 this.useCounts)
        |> UMX.tag<ceLength>
 
    member this.LastUsedCeIndex : int<ceIndex> =
        ArrayUtils.lastNonZeroIndex this.useCounts
        |> UMX.tag<ceIndex>

    /// Safe snapshot (no external mutation)
    member this.ToArray() : int[] =
        Array.copy this.useCounts


