namespace GeneSort.Core

open LanguagePrimitives
open System
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop
open System.Runtime.InteropServices
open System.Runtime.Intrinsics


module ArrayUtils =

    let inline distanceSquared< ^a when ^a: (static member Zero: ^a)
                                        and ^a: (static member (+): ^a * ^a -> ^a)
                                        and ^a: (static member (-): ^a * ^a -> ^a)
                                        and ^a: (static member (*): ^a * ^a -> ^a)>
                    (a: ^a[]) (b: ^a[]) : ^a =
        let mutable acc = GenericZero<^a>
        let mutable i = 0
        while i < a.Length do
            acc <- acc + (a.[i] - b.[i]) * (a.[i] - b.[i])
            i <- i + 1
        acc


    let inline distanceSquaredOffset< ^a when ^a: (static member Zero: ^a)
                                        and ^a: (static member (+): ^a * ^a -> ^a)
                                        and ^a: (static member (-): ^a * ^a -> ^a)
                                        and ^a: (static member (*): ^a * ^a -> ^a)>
            (longArray: ^a[]) (shortArray: ^a[]) : ^a[] =
        let n = shortArray.Length
        let m = longArray.Length / n
        let result = Array.zeroCreate m
        for i = 0 to m - 1 do
            let mutable acc = GenericZero<^a>
            for j = 0 to n - 1 do
                let diff = longArray.[i * n + j] - shortArray.[j]
                acc <- acc + diff * diff
            result.[i] <- acc
        result


    let inline isSorted< ^a when ^a: comparison> (values: ^a[]) : bool =
        let len = values.Length
        let mutable ok = true
        let mutable i = 0
        while i < len - 1 && ok do
            if values.[i] > values.[i+1] then 
                ok <- false
            else 
                i <- i + 1
        ok


    let inline isSortedOffset< ^a when ^a: comparison> 
                    (values: ^a[]) 
                    (offset:int) 
                    (length:int) : bool =
        if isNull values then failwith "Array cannot be null"
        elif offset < 0 then failwithf "Invalid offset: %d" offset
        elif length < 0 then failwithf "Invalid length: %d" length
        elif offset + length > values.Length then 
            failwithf "Offset plus length exceeds array size: offset=%d, length=%d, array size=%d" offset length values.Length
        elif length <= 1 then true
        else
            let mutable i = 1
            let mutable isSorted = true
            while (i < length && isSorted) do
                isSorted <- (values.[i + offset - 1] <= values.[i + offset])
                i <- i + 1
            isSorted


    let lastNonZeroIndex (arr: int array) : int =
        let rec loop i =
            if i < 0 then
                -1  // Return -1 if no non-zero value is found
            elif arr.[i] <> 0 then
                i
            else
                loop (i - 1)
        loop (arr.Length - 1)

    /// Finds the first index at which two arrays differ. 
    /// Throws an exception if the arrays are identical or of different lengths.
    let firstDiffIndexOrThrow (a: 'T[]) (b: 'T[]) =
        if a.Length <> b.Length then
            invalidArg "b" "Arrays must have the same length"

        let mutable i = 0
        let mutable found = -1

        while i < a.Length && found = -1 do
            if a.[i] <> b.[i] then
                found <- i
            i <- i + 1

        if found >= 0 then
            found
        else
            invalidOp "Arrays are identical; no differing index found"


    let stackAndBlock (data: 'a[] seq) (blockWidth: int) : ('a[][] seq) =
        if Seq.isEmpty data then 
            Seq.empty 
        else
            // 1. Group the sequence into chunks of size blockWidth
            data
            |> Seq.chunkBySize blockWidth
            |> Seq.map (fun chunk ->
                let k = chunk.[0].Length
            
                // 2. Prepare the output: k rows, each of length blockWidth
                Array.init k (fun rowIdx ->
                    Array.init blockWidth (fun colIdx ->
                        // 3. Fill from chunk if data exists, otherwise use 'zero' (default)
                        if colIdx < chunk.Length then
                            chunk.[colIdx].[rowIdx]
                        else
                            Unchecked.defaultof<'a>
                    )
                )
            )

    let unstackAndBlock (blockedData: 'a[][] seq) (originalCount: int option) : ('a[] seq) =
        blockedData
        |> Seq.collect (fun block ->
            // 1. Determine dimensions
            let k = block.Length          // The length of original arrays
            let blockWidth = block.[0].Length // The width of the block
        
            // 2. Transpose the block back to the original array format
            Array.init blockWidth (fun colIdx ->
                Array.init k (fun rowIdx ->
                    block.[rowIdx].[colIdx]
                )
            )
        )
        // 3. If we know the original count (m), truncate the padding
        |> (fun fullSeq -> 
            match originalCount with
            | Some m -> Seq.truncate m fullSeq
            | None   -> fullSeq)


    // You must use the 'unmanaged' constraint for NativePtr operations
    let fastGenericCopyToBuffer0 (source: 'T[]) (dest: 'T[]) : unit when 'T : unmanaged =
        let byteCount = uint32 (source.Length * Unsafe.SizeOf<'T>())
        
        // Pin the arrays
        use pSrc = fixed source
        use pDest = fixed dest
        
        // Convert nativeptr<'T> to void* 
        let srcVoidPtr = NativePtr.toVoidPtr pSrc
        let destVoidPtr = NativePtr.toVoidPtr pDest
        
        Unsafe.CopyBlock(destVoidPtr, srcVoidPtr, byteCount)


    /// High-performance generic copy using Span logic.
    /// This is safer than raw pointers but usually just as fast.
    let fastGenericCopyToBuffer (source: 'T[]) (dest: 'T[]) =
        // source.AsSpan() creates a span of exact length.
        // dest.AsSpan(0, source.Length) slices the pool buffer to match.
        source.AsSpan().CopyTo(dest.AsSpan(0, source.Length))


    /// The "Nuclear" version if Span.CopyTo isn't fast enough for your specific CPU.
    let ultraFastCopy (source: 'T[]) (dest: 'T[]) : unit when 'T : unmanaged =
        let count = source.Length
        let byteCount = uint32 (count * Unsafe.SizeOf<'T>())
        
        // Get refs to the actual data start points
        let mutable srcRef = MemoryMarshal.GetArrayDataReference(source)
        let mutable destRef = MemoryMarshal.GetArrayDataReference(dest)
        
        // This emits the same IL as a native memcpy
        Unsafe.CopyBlock(Unsafe.AsPointer(&destRef), Unsafe.AsPointer(&srcRef), byteCount)








