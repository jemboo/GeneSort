namespace GeneSort.Core.Test

open Xunit
open FsUnit.Xunit
open GeneSort.Core.ArrayUtils
open System

type ArrayUtilsTests() =

    ////////// Segment Tests //////////

    let getSegmentSums (intData: int[]) (segments: segment[]) : segmentWithPayload<int>[] =
        segments
        |> Array.map (fun seg -> 
            let segSum = if seg.endIndex > seg.start then Array.sum intData.[seg.start .. seg.endIndex - 1] else 0
            { start = seg.start; endIndex = seg.endIndex; payload = segSum })

    [<Fact>]
    let ``breakIntoExponentialSegments throws for segmentCt <= 1`` () =
        let action = fun () -> breakIntoExponentialSegments 0 2.0 3 |> ignore
        action |> should throw typeof<ArgumentException>

    [<Fact>]
    let ``breakIntoExponentialSegments throws for rate <= 1.0`` () =
        let action = fun () -> breakIntoExponentialSegments 3 1.0 3 |> ignore
        action |> should throw typeof<ArgumentException>

    [<Fact>]
    let ``breakIntoExponentialSegments for segmentCt=1 covers whole array`` () =
        let data = [|1..10|]
        let segments = breakIntoExponentialSegments 1 2.0 data.Length
        segments |> should haveLength 1
        segments.[0] |> should equal { start = 0; endIndex = 10 }

    [<Fact>]
    let ``breakIntoExponentialSegments creates exponential segments`` () =
        let data = Array.zeroCreate<int> 10  // Length 10, values irrelevant for bounds
        let segments = breakIntoExponentialSegments 3 2.0 data.Length
        segments |> should haveLength 3
        segments.[0].endIndex - segments.[0].start |> should equal 1  // Approx 10*(2-1)/7 ≈1
        segments.[1].endIndex - segments.[1].start |> should equal 3  // Approx 10*(4-1)/7 ≈4-1=3
        segments.[2].endIndex - segments.[2].start |> should equal 6  // 10-4=6


    [<Fact>]
    let ``breakIntoExponentialSegments2 creates exponential segments`` () =
        let data = Array.zeroCreate<int> 10  // Length 10, values irrelevant for bounds
        let segments = breakIntoExponentialSegments2 3 2.0 data.Length
        segments |> should haveLength 3


    [<Fact>]
    let ``breakIntoExponentialSegments segments are contiguous`` () =
        let data = [|1..10|]
        let segments = breakIntoExponentialSegments 4 1.5 data.Length
        for i in 1 .. segments.Length - 1 do
            segments.[i].start |> should equal segments.[i-1].endIndex
        segments.[segments.Length-1].endIndex |> should equal 10

    [<Fact>]
    let ``getSegmentSums computes correct sums`` () =
        let data = [|1;2;3;4;5;6;7;8;9;10|]
        let segments = [| {start=0; endIndex=3}; {start=3; endIndex=6}; {start=6; endIndex=10} |]
        let result = getSegmentSums data segments
        result |> should haveLength 3
        result.[0].payload |> should equal (1+2+3)  // 6
        result.[1].payload |> should equal (4+5+6)  // 15
        result.[2].payload |> should equal (7+8+9+10)  // 34

    [<Fact>]
    let ``getSegmentSums handles empty segment`` () =
        let data = [|1;2;3|]
        let segments = [| {start=0; endIndex=0}; {start=0; endIndex=3} |]  // First empty
        let result = getSegmentSums data segments
        result.[0].payload |> should equal 0
        result.[1].payload |> should equal (1+2+3)  // 6

    [<Fact>]
    let ``getSegmentSums for empty array and segments`` () =
        let data = [||]
        let segments = [||]
        let result = getSegmentSums data segments
        result |> should be Empty

    [<Fact>]
    let ``getSegmentSums preserves segment bounds`` () =
        let data = [|1..5|]
        let segments = [| {start=1; endIndex=4} |]
        let result = getSegmentSums data segments
        result.[0].start |> should equal 1
        result.[0].endIndex |> should equal 4
        result.[0].payload |> should equal (2+3+4)  //