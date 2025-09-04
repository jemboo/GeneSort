namespace GeneSort.Core
open LanguagePrimitives
open System

module ArrayProperties =

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
        if isNull values then 
            failwith "Array cannot be null"
        elif values.Length <= 1 then true
        else
            let mutable i = 1
            let mutable isSorted = true
            while (i < values.Length && isSorted) do
                isSorted <- (values.[i - 1] <= values.[i])
                i <- i + 1
            isSorted


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



    type segment = {
        start: int
        endIndex: int
    }
    /// Breaks the input array into n segments with exponentially increasing lengths.
    /// The segments are shortest at the beginning and grow longer based on the rate parameter.
    /// Returns an array of Segment records containing the bounds for each segment.
    /// 
    /// Parameters:
    /// - result: The input integer array to segment.
    /// - n: The number of segments (must be positive).
    /// - rate: The exponential growth rate (must be greater than 1.0).
    /// 
    /// Throws:
    /// - ArgumentException if n <= 0 or rate <= 1.0.
    /// 
    /// Returns:
    /// - An array of Segment records. If the input array is empty, returns an empty array.
    let breakIntoExponentialSegments (n: int) (rate: float) (intData: int[])  : segment[] =
        if n <= 0 then invalidArg "n" "Number of segments must be positive"
        if rate <= 1.0 then invalidArg "rate" "Rate must be greater than 1.0"
        let len = intData.Length
        if len = 0 then [||]
        else
            let rn = Math.Pow(rate, float n)
            let denom = rn - 1.0
            let mutable prev = 0
            let segments = ResizeArray<segment>()
            for k = 1 to n do
                let cumFloat = if k = n then float len else float len * (Math.Pow(rate, float k) - 1.0) / denom
                let current = int (Math.Round cumFloat)
                let clamped = max prev (min current len)
                segments.Add { start = prev; endIndex = clamped; }
                prev <- clamped
            segments.ToArray()

    let getSegmentReportHeader (segments: segment[]) : string =
        segments
        |> Array.sortBy(fun seg -> seg.start)
        |> Array.mapi (fun i seg -> seg.endIndex.ToString())
                        |> String.concat "\t"
                        |> sprintf "[%s]"


    type segmentWithPayload<'u> = {
        start: int
        endIndex: int
        payload: 'u
    }

    /// Given an array of integers and an array of segments (with start and end indices),
    /// computes the sum of integers within each segment and returns an array of SegmentWithSum records.
    let getSegmentSums (intData: int[]) (segments: segment[]) : segmentWithPayload<int>[] =
        segments
        |> Array.map (fun seg -> 
            let segSum = if seg.endIndex > seg.start then Array.sum intData.[seg.start .. seg.endIndex - 1] else 0
            { start = seg.start; endIndex = seg.endIndex; payload = segSum })


    let getSegmentPayloadReportData<'u> (formatter: 'u -> string) (segWithPayload: segmentWithPayload<'u>[]) : string =
        segWithPayload
        |> Array.sortBy(fun seg -> seg.start)
        |> Array.map (fun seg -> formatter seg.payload)
                        |> String.concat "\t"
                        |> sprintf "[%s]"