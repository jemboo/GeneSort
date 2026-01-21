namespace GeneSort.Sorting.Tests

open System
open Xunit
open FSharp.UMX
open GeneSort.Core.Combinatorics
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open FsUnit.Xunit

type CeTests() =

    [<Fact>]
    let ``generateCeCode with excludeSelfCe false returns indexPicker value`` () =
        let width = 4
        let indices = [|0 .. 20|] // Corresponds to Ces: (0,1), (0,2), (1,2), (0,3), (1,3), (2,3)
        let picker = indexPicker indices
        let result = Ce.generateCeCode false  width picker
        result |> should equal 0 // First index from picker
        let maxIndex = Ce.maxIndexForWdith width
        result |> should be (lessThanOrEqualTo maxIndex)

    [<Fact>]
    let ``generateCeCode with excludeSelfCe true shifts hi index`` () =
        let width = 4
        let indices = [|0; 1; 2|] // Corresponds to Ces: (0,1), (0,2), (1,2)
        let picker = indexPicker indices
        let result = Ce.generateCeCode true width picker
        let expectedCe = ce.create 0 1 // index 0 maps to (0,0), shifted to (0,1)
        let expectedIndex = Ce.toIndex expectedCe
        result |> should equal expectedIndex
        let ce = Ce.fromIndex result
        ce.Low |> should not' (equal ce.Hi)

    [<Fact>]
    let ``generateCeCode with excludeSelfCe true produces valid Ce within width`` () =
        let width = 5
        let indices = [|0; 1; 2; 3; 4; 5|]
        let picker = indexPicker indices
        let results = [ for _ in 1..6 -> Ce.generateCeCode true width picker]
        results |> List.iter (fun idx ->
            let ce = Ce.fromIndex idx
            ce.Low |> should be (lessThanOrEqualTo (width - 1))
            ce.Hi |> should be (lessThanOrEqualTo (width - 1))
            ce.Low |> should not' (equal ce.Hi))

    [<Fact>]
    let ``generateCeCode throws for width less than 1`` () =
        let picker = indexPicker [|0; 1; 2|]
        (fun () -> Ce.generateCeCode false 0 picker |> ignore) 
        |> should throw typeof<Exception>

    [<Fact>]
    let ``generateCeCode with excludeSelfCe true handles edge case width 2`` () =
        let width = 2
        let indices = [|0|] // Maps to (0,1) when excludeSelfCe is true
        let picker = indexPicker indices
        let result = Ce.generateCeCode true width picker
        let cet = Ce.fromIndex result
        cet |> should equal (ce.create 0 1)
        cet.Low |> should not' (equal cet.Hi)


    [<Fact>]
    let ``toIndex and fromIndex round-trip for valid Ce including Low = Hi``() =
        let ces = [ 
                ce.create 0 0; ce.create 0 1; ce.create 0 2; ce.create 0 3; ce.create 0 4; ce.create 0 5; 
                ce.create 1 1; ce.create 1 2; ce.create 1 3; ce.create 1 4; ce.create 1 5; ce.create 1 6; 
                ce.create 2 2; ce.create 2 3; ce.create 2 4; ce.create 2 5; ce.create 2 6; ce.create 2 7; 
                ce.create 3 3; ce.create 3 4; ce.create 3 5; ce.create 3 6; ce.create 3 7; ce.create 3 8; 
                ce.create 4 4; ce.create 4 5; ce.create 4 6; ce.create 4 7; ce.create 4 8; ce.create 4 9; 
                ce.create 5 5; ce.create 5 6; ce.create 5 7; ce.create 5 8; ce.create 5 9; ce.create 5 10; 
            ]

        for ce in ces do
            let index = Ce.toIndex ce
            let ce' = Ce.fromIndex index
            Assert.Equal(ce, ce')

    [<Theory>]
    [<InlineData(-1, 0, "Indices must be non-negative")>]
    [<InlineData(0, -1, "Indices must be non-negative")>]
    [<InlineData(-1, -1, "Indices must be non-negative")>]
    let ``ce.create rejects negative indices`` (low: int, hi: int, expectedMsg: string) =
        let ex = Assert.Throws<Exception>(fun () -> ce.create low hi |> ignore)
        Assert.Equal(expectedMsg, ex.Message)

    [<Theory>]
    [<InlineData(-1, "Index must be non-negative")>]
    [<InlineData(-2, "Index must be non-negative")>]
    let ``fromIndex handles invalid index`` (index: int, expectedMsg: string) =
        let ex = Assert.Throws<Exception>(fun () -> Ce.fromIndex index |> ignore)
        Assert.Equal(expectedMsg, ex.Message)

    [<Fact>]
    let ``generateCes produces valid Ces within width`` () =
        let width = 4
        let indices = [|0; 1; 2; 3; 4; 5|] // Corresponds to Ces: (0,1), (0,2), (1,2), (0,3), (1,3), (2,3)
        let picker = indexPicker indices
        let ces = Ce.generateCes picker width |> Seq.take 6 |> Seq.toList
        let expected = [
            ce.create 0 0
            ce.create 0 1
            ce.create 1 1
            ce.create 0 2
            ce.create 1 2
            ce.create 2 2
        ]
        ces |> should equal expected
        ces |> List.iter (fun ce -> 
            ce.Low |> should be (lessThanOrEqualTo (width - 1))
            ce.Hi |> should be (lessThanOrEqualTo (width - 1)))

    [<Fact>]
    let ``generateCes includes low equals hi when index maps to it`` () =
        let width = 3
        let indices = [|0; 3; 6|] // Indices that could map to low=hi in larger context
        let picker = indexPicker indices
        let ces = Ce.generateCes picker width |> Seq.take 3 |> Seq.toList
        let expected = [
            ce.create 0 0
            ce.create 0 2
            ce.create 0 3
        ]
        ces |> should equal expected

    [<Fact>]
    let ``generateCes throws for invalid width`` () =
        let picker = indexPicker [|0; 1; 2|]
        (fun () -> Ce.generateCes picker 0 |> Seq.take 1 |> ignore) 
        |> should throw typeof<Exception>

    [<Fact>]
    let ``generateCesExcludeSelf produces valid Ces excluding low equals hi`` () =
        let width = 4
        let indices = [|0; 1; 2; 3|] // Corresponds to Ces: (0,1), (0,2), (1,2), (0,3) after offset
        let picker = indexPicker indices
        let ces = Ce.generateCesExcludeSelf picker width |> Seq.take 4 |> Seq.toList
        let expected = [
            ce.create 0 1
            ce.create 0 2
            ce.create 1 2
            ce.create 0 3
        ]
        ces |> should equal expected
        ces |> List.iter (fun ce -> 
            ce.Low |> should not' (equal ce.Hi)
            ce.Low |> should be (lessThanOrEqualTo (width - 1))
            ce.Hi |> should be (lessThanOrEqualTo (width - 1)))

    [<Fact>]
    let ``generateCesExcludeSelf throws for width less than 2`` () =
        let picker = indexPicker [|0; 1; 2|]
        (fun () -> Ce.generateCesExcludeSelf picker 1 |> Seq.take 1 |> ignore) 
        |> should throw typeof<Exception>

    [<Fact>]
    let ``generateCesExcludeSelf produces distinct Ces for different indices`` () =
        let width = 5
        let indices = [|0; 1; 2; 3; 4; 5|]
        let picker = indexPicker indices
        let ces = Ce.generateCesExcludeSelf picker width |> Seq.take 6 |> Seq.toList
        let distinctCes = ces |> List.distinct
        distinctCes.Length |> should equal ces.Length
        ces |> List.iter (fun ce -> ce.Low |> should not' (equal ce.Hi))


