namespace GeneSort.Core.Test

open Xunit
open FsUnit.Xunit
open GeneSort.Core.ArrayProperties
open System

type ArrayPropertiesTests() =

    // Tests for distanceSquared
    [<Fact>]
    let ``distanceSquared with equal float arrays returns zero`` () =
        let a = [|0.0; 0.0; 0.0|]
        let b = [|0.0; 0.0; 0.0|]
        let result = distanceSquared a b
        result |> should equal 0.0

    [<Fact>]
    let ``distanceSquared with different float arrays returns correct sum`` () =
        let a = [|1.0; 2.0; 3.0|]
        let b = [|0.0; 0.0; 0.0|]
        let result = distanceSquared a b
        result |> should equal 14.0  // 1^2 + 2^2 + 3^2 = 1 + 4 + 9 = 14

    [<Fact>]
    let ``distanceSquared with integer arrays returns correct sum`` () =
        let a = [|1; 2; 3|]
        let b = [|0; 0; 0|]
        let result = distanceSquared a b
        result |> should equal 14

    // Tests for distanceSquaredOffset
    [<Fact>]
    let ``distanceSquaredOffset with matching arrays returns zero array`` () =
        let longArray = [|0.0; 0.0; 0.0; 0.0|]
        let shortArray = [|0.0; 0.0|]
        let result = distanceSquaredOffset longArray shortArray
        result |> should equal <| [|0.0; 0.0|]

    [<Fact>]
    let ``distanceSquaredOffset with float arrays returns correct distances`` () =
        let longArray = [|1.0; 0.0; 0.0; 1.0|]
        let shortArray = [|0.0; 0.0|]
        let result = distanceSquaredOffset longArray shortArray
        result |> should equal [|1.0; 1.0|]  // [(1-0)^2 + (0-0)^2; (0-0)^2 + (1-0)^2]

    [<Fact>]
    let ``distanceSquaredOffset with integer arrays returns correct distances`` () =
        let longArray = [|1; 0; 0; 1|]
        let shortArray = [|0; 0|]
        let result = distanceSquaredOffset longArray shortArray
        result |> should equal [|1; 1|]

    // Tests for isSorted
    [<Fact>]
    let ``isSorted with sorted array returns true`` () =
        let values = [|1; 2; 3; 4|]
        isSorted values |> should equal true

    [<Fact>]
    let ``isSorted with unsorted array returns false`` () =
        let values = [|1; 3; 2; 4|]
        isSorted values |> should equal false

    [<Fact>]
    let ``isSorted with empty array returns true`` () =
        let values = [||]
        isSorted values |> should equal true

    [<Fact>]
    let ``isSorted with single element returns true`` () =
        let values = [|1|]
        isSorted values |> should equal true

    [<Fact>]
    let ``isSorted with null array throws exception`` () =
        let values: int[] = null
        (fun () -> isSorted values |> ignore) |> should throw typeof<System.Exception>

    // Tests for isSortedOffset
    [<Fact>]
    let ``isSortedOffset with sorted segment returns true`` () =
        let values = [|5; 1; 2; 3; 4|]
        isSortedOffset values 1 3 |> should equal true

    [<Fact>]
    let ``isSortedOffset with unsorted segment returns false`` () =
        let values = [|5; 3; 2; 1; 4|]
        isSortedOffset values 1 3 |> should equal false

    [<Fact>]
    let ``isSortedOffset with single element returns true`` () =
        let values = [|1; 2; 3|]
        isSortedOffset values 1 1 |> should equal true

    [<Fact>]
    let ``isSortedOffset with zero length returns true`` () =
        let values = [|1; 2; 3|]
        isSortedOffset values 1 0 |> should equal true

    [<Fact>]
    let ``isSortedOffset with negative offset throws exception`` () =
        let values = [|1; 2; 3|]
        (fun () -> isSortedOffset values -1 2 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``isSortedOffset with negative length throws exception`` () =
        let values = [|1; 2; 3|]
        (fun () -> isSortedOffset values 0 -1 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``isSortedOffset with offset plus length exceeding array size throws exception`` () =
        let values = [|1; 2; 3|]
        (fun () -> isSortedOffset values 1 3 |> ignore) |> should throw typeof<System.Exception>

    [<Fact>]
    let ``isSortedOffset with null array throws exception`` () =
        let values: int[] = null
        (fun () -> isSortedOffset values 0 1 |> ignore) |> should throw typeof<System.Exception>

    let breakIntoExponentialSegments (n: int) (rate: float) (intData: int[]) : segment[] =
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



    ////////// Segment Tests //////////

    let getSegmentSums (intData: int[]) (segments: segment[]) : segmentWithPayload<int>[] =
        segments
        |> Array.map (fun seg -> 
            let segSum = if seg.endIndex > seg.start then Array.sum intData.[seg.start .. seg.endIndex - 1] else 0
            { start = seg.start; endIndex = seg.endIndex; payload = segSum })

    [<Fact>]
    let ``breakIntoExponentialSegments throws for n <= 0`` () =
        let action = fun () -> breakIntoExponentialSegments 0 2.0 [|1;2;3|] |> ignore
        action |> should throw typeof<ArgumentException>

    [<Fact>]
    let ``breakIntoExponentialSegments throws for rate <= 1.0`` () =
        let action = fun () -> breakIntoExponentialSegments 3 1.0 [|1;2;3|] |> ignore
        action |> should throw typeof<ArgumentException>

    [<Fact>]
    let ``breakIntoExponentialSegments returns empty for empty array`` () =
        let result = breakIntoExponentialSegments 3 2.0 [||]
        result |> should be Empty

    [<Fact>]
    let ``breakIntoExponentialSegments for n=1 covers whole array`` () =
        let data = [|1..10|]
        let segments = breakIntoExponentialSegments 1 2.0 data
        segments |> should haveLength 1
        segments.[0] |> should equal { start = 0; endIndex = 10 }

    [<Fact>]
    let ``breakIntoExponentialSegments creates exponential segments`` () =
        let data = Array.zeroCreate<int> 10  // Length 10, values irrelevant for bounds
        let segments = breakIntoExponentialSegments 3 2.0 data
        segments |> should haveLength 3
        segments.[0].endIndex - segments.[0].start |> should equal 1  // Approx 10*(2-1)/7 ≈1
        segments.[1].endIndex - segments.[1].start |> should equal 3  // Approx 10*(4-1)/7 ≈4-1=3
        segments.[2].endIndex - segments.[2].start |> should equal 6  // 10-4=6


    [<Fact>]
    let ``breakIntoExponentialSegments segments are contiguous`` () =
        let data = [|1..10|]
        let segments = breakIntoExponentialSegments 4 1.5 data
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