namespace GeneSort.Core
open LanguagePrimitives
open System

type segment = {
    start: int
    endIndex: int
}


module Segment =

     //The last segment returned is {start = lastIndex - lastLength - 1; endIndex = lastIndex - 1}.     
     //The first (segmentCt - 1) segments grow exponentially with the specified rate, starting from a     
     //length calculated to fit the last segment at the end     
    let breakIntoExponentialSegments
                (segmentCt: int)
                (rate: float)
                (totalLength: int)
                (lastLength: int) : segment[] =
        // Validate inputs
        if segmentCt < 2 then
            invalidArg "segmentCt" "segmentCt must be 2 or more"
        if rate <= 1.0 then
            invalidArg "rate" "rate must be greater than 1"
        if lastLength < 1 then
            invalidArg "lastLength" "lastLength must be at least 1"
        if totalLength < lastLength - 1 then
            invalidArg "lastIndex" "lastIndex must be at least lastLength - 1"
    

        // Calculate the starting length for the exponential sequence
        // We have (segmentCt - 1) segments growing exponentially
        // Sum of geometric series: firstLen * (rate^(n-1) - 1) / (rate - 1) = totalLengthBeforeLast
        let totalLengthBeforeLast = totalLength - lastLength + 1
        let n = segmentCt - 1
    
        // firstLen * (rate^(n-1) - 1) / (rate - 1) = totalLengthBeforeLast
        // firstLen = totalLengthBeforeLast * (rate - 1) / (rate^(n-1) - 1)
        let firstLen = 
            float totalLengthBeforeLast * (rate - 1.0) / (Math.Pow(rate, float (n - 0)) - 1.0)
    
        // Generate the exponential segments
        let mutable currentStart = 0
        let segments = Array.zeroCreate segmentCt
    
        for i in 0 .. segmentCt - 2 do
            let length = int (Math.Round(firstLen * Math.Pow(rate, float i)))
            let length = max 1 length // Ensure at least length 1
            let endIdx = currentStart + length - 1
            segments.[i] <- { start = currentStart; endIndex = endIdx }
            currentStart <- endIdx + 1
    
        // Set the last segment
        let lastSegment = { start = currentStart; endIndex = totalLength }
        segments.[segmentCt - 1] <- lastSegment
    
        segments



    //The last segment returned is {start = itemsLength - lastLength - 1; endIndex = itemsLength - 1}.
    //The first (segmentCt - 1) segments grow exponentially with the specified rate, starting from a 
    //length calculated to fit the last segment at the end
    let breakIntoExponentialSegments0 
                (segmentCt: int) 
                (rate: float) 
                (itemsLength: int) 
                (lastLength: int) : segment[] =
        // Validate inputs
        if segmentCt < 2 then
            invalidArg "segmentCt" "segmentCt must be 2 or more"
        if rate <= 1.0 then
            invalidArg "rate" "rate must be greater than 1"
        if lastLength < 1 then
            invalidArg "lastLength" "lastLength must be at least 1"
        if itemsLength < lastLength - 1 then
            invalidArg "lastIndex" "lastIndex must be at least lastLength - 1"
    
        // Calculate the first segment length using the geometric series formula
        // Given: last_length = first_length * rate^(n-1)
        // Solving for first_length: first_length = last_length / rate^(n-1)
        let firstLength = (float (itemsLength - lastLength) )/ (rate ** float (segmentCt - 1))
    
        // Generate segment lengths using exponential growth
        // Each segment length = firstLength * rate^i where i is the segment index
        let lengths = 
            [| 0 .. segmentCt - 1 |]
            |> Array.map (fun i -> firstLength * (rate ** float i))
    
        // Round lengths to integers, ensuring we maintain at least length 1
        let intLengths = 
            lengths 
            |> Array.map (fun len -> max 1 (int (round len)))
    
        // Adjust the last length to exactly match the requested lastLength
        // This compensates for rounding errors in earlier segments
        intLengths.[segmentCt - 1] <- lastLength
    
        // Calculate the total length of all segments
        let totalLength = Array.sum intLengths
    
        // Calculate where the first segment must start so that the last segment
        // ends at lastIndex. Working backwards: startIndex = lastIndex - totalLength + 1
        let startIndex = itemsLength - totalLength + 1
    
        // Build segments by computing cumulative positions from startIndex
        // Each segment's start is the sum of all previous segment lengths plus startIndex
        let segments = Array.zeroCreate segmentCt
        let mutable currentStart = startIndex
    
        for i = 0 to segmentCt - 1 do
            let length = intLengths.[i]
            segments.[i] <- {
                start = currentStart
                endIndex = currentStart + length - 1  // endIndex is inclusive
            }
            currentStart <- currentStart + length
    
        segments


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

module SegmentWithPayload =

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
