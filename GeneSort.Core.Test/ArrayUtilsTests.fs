namespace GeneSort.Core.Test

open Xunit
open FsUnit.Xunit
open GeneSort.Core

type ArrayUtilsTests() =

    ////////// Segment Tests //////////
    [<Fact>]
    let ``breakIntoExponentialSegments for segmentCt=2 covers whole array`` () =
        let seqmentCt = 2
        let growthRate = 2.0
        let lastIndex = 10
        let lastLength = 3
        let segments = Segment.breakIntoExponentialSegments seqmentCt growthRate lastIndex lastLength
        segments |> should haveLength 2

    [<Fact>]
    let ``getSegmentSums computes correct sums`` () =
        let data = [|1;2;3;4;5;6;7;8;9;10|]
        let segments = [| {start=0; endIndex=3}; {start=3; endIndex=6}; {start=6; endIndex=10} |]
        let result = SegmentWithPayload.getSegmentSums data segments
        result |> should haveLength 3
        result.[0].payload |> should equal (1+2+3)  // 6
        result.[1].payload |> should equal (4+5+6)  // 15
        result.[2].payload |> should equal (7+8+9+10)  // 34

    [<Fact>]
    let ``getSegmentSums handles empty segment`` () =
        let data = [|1;2;3|]
        let segments = [| {start=0; endIndex=0}; {start=0; endIndex=3} |]  // First empty
        let result = SegmentWithPayload.getSegmentSums data segments
        result.[0].payload |> should equal 0
        result.[1].payload |> should equal (1+2+3)  // 6

    [<Fact>]
    let ``getSegmentSums for empty array and segments`` () =
        let data = [||]
        let segments = [||]
        let result = SegmentWithPayload.getSegmentSums data segments
        result |> should be Empty

    [<Fact>]
    let ``getSegmentSums preserves segment bounds`` () =
        let data = [|1..5|]
        let segments = [| {start=1; endIndex=4} |]
        let result = SegmentWithPayload.getSegmentSums data segments
        result.[0].start |> should equal 1
        result.[0].endIndex |> should equal 4
        result.[0].payload |> should equal (2+3+4)  //