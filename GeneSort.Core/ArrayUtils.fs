namespace GeneSort.Core
open LanguagePrimitives
open System

module ArrayUtils =


    let arrayPinch (input: 'a[]) (segLen: int) (gen: int) (fsL: int) (ssL: int) :'a[] =
        
        let preSeg = [| for i = 0 to fsL - 1 do input.[i] |]
        let segLa = [| for i = (fsL) to (fsL + segLen - gen - 1) do input.[i] |]
        let segLb = [| for i = (ssL + segLen - gen) to (ssL + segLen - 1) do input.[i] |]
        let j1 = Array.append preSeg (Array.append segLa segLb |> Array.sort)  

        let interSeg = [| for i = (fsL + segLen) to (ssL - 1) do input.[i] |]

        let j2 = Array.append j1 interSeg
        
        let segUa = [| for i = (fsL + segLen - gen) to (fsL + segLen - 1) do input.[i] |]
        let segUb = [| for i = (ssL) to (ssL + segLen - gen - 1) do input.[i] |]
        let j3 = Array.append j2 (Array.append segUa segUb |> Array.sort)

        let postSeg = [| for i = (ssL + segLen) to (input.Length - 1) do input.[i] |]
        let j4 = Array.append  j3 postSeg
        j4



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
    let breakIntoExponentialSegments (segmentCt: int) (rate: float) (unbrokenLength: int)  : segment[] =
        if segmentCt <= 0 then invalidArg "n" "Number of segments must be positive"
        if rate <= 1.0 then invalidArg "rate" "Rate must be greater than 1.0"
        if unbrokenLength = 0 then [||]
        else
            let rn = Math.Pow(rate, float segmentCt)
            let denom = rn - 1.0
            let mutable prev = 0
            let segments = ResizeArray<segment>()
            for k = 1 to segmentCt do
                let cumFloat = if k = segmentCt then float unbrokenLength else float unbrokenLength * (Math.Pow(rate, float k) - 1.0) / denom
                let current = int (Math.Round cumFloat)
                let clamped = max prev (min current unbrokenLength)
                segments.Add { start = prev; endIndex = clamped; }
                prev <- clamped
            segments.ToArray()


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
    /// This version makes the last segment [unbrokenLength - 2, unbrokenLength]
    let breakIntoExponentialSegments2 (segmentCt: int) (rate: float) (unbrokenLength: int)  : segment[] =
        if segmentCt <= 1 then invalidArg "n" "Number of segments must be positive"
        let trimCt = segmentCt - 1
        if rate <= 1.0 then invalidArg "rate" "Rate must be greater than 1.0"
        if unbrokenLength = 0 then [||]
        else
            let rn = Math.Pow(rate, float trimCt)
            let denom = rn - 1.0
            let mutable prev = 0
            let segments = ResizeArray<segment>()
            for k = 1 to trimCt do
                let cumFloat = if k = segmentCt then float unbrokenLength else float unbrokenLength * (Math.Pow(rate, float k) - 1.0) / denom
                let current = int (Math.Round cumFloat)
                let clamped = max prev (min current unbrokenLength)
                segments.Add { start = prev; endIndex = clamped; }
                prev <- clamped
            //segments.[trimCt -1] <- {start = segments.[trimCt -1].start; endIndex = unbrokenLength; }}
            let yab = { start = segments.[(trimCt - 1)].start; endIndex = unbrokenLength - 1; }
            segments.[trimCt - 1] <- yab
            segments.Add { start = unbrokenLength - 1; endIndex = unbrokenLength; }
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


    let getSegmentPayloadReportData<'u> (formatter: 'u -> string) (segWithPayload: segmentWithPayload<'u>[]) : string[] =
        segWithPayload
        |> Array.sortBy(fun seg -> seg.start)
        |> Array.map (fun seg -> formatter seg.payload)



