namespace GeneSort.Sorter.Tests

open Xunit
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open FsUnit.Xunit

type StageSequenceTests() =

    [<Fact>]
    let ``AddCe to empty sequence creates new stage`` () =
        let sortingWidth = 4<sortingWidth>
        let stageSeq = stageSequence.create(sortingWidth)
        let ce = ce.create 0 1
        
        stageSeq.AddCe(ce)
        
        stageSeq.StageLength |> should equal 1
        stageSeq.Stages.[0].CeCount |> should equal 1
        stageSeq.Stages.[0].Ces.[0] |> should equal ce
        stageSeq.Stages.[0].IsOccupied(0) |> should equal true
        stageSeq.Stages.[0].IsOccupied(1) |> should equal true

    [<Fact>]
    let ``AddCe to existing stage when possible`` () =
        let sortingWidth = 4<sortingWidth>
        let stageSeq = stageSequence.create(sortingWidth)
        let firstCe = ce.create 0 1
        let secondCe = ce.create 2 3
        stageSeq.AddCe(firstCe)
        
        stageSeq.AddCe(secondCe)
        
        stageSeq.StageLength |> should equal 1
        stageSeq.Stages.[0].CeCount |> should equal 2
        stageSeq.Stages.[0].Ces |> should contain firstCe
        stageSeq.Stages.[0].Ces |> should contain secondCe
        stageSeq.Stages.[0].IsOccupied(0) |> should equal true
        stageSeq.Stages.[0].IsOccupied(1) |> should equal true
        stageSeq.Stages.[0].IsOccupied(2) |> should equal true
        stageSeq.Stages.[0].IsOccupied(3) |> should equal true

    [<Fact>]
    let ``AddCe creates new stage when existing stage is occupied`` () =
        let sortingWidth = 4<sortingWidth>
        let stageSeq = stageSequence.create(sortingWidth)
        let firstCe = ce.create 0 1
        let secondCe = ce.create 0 2 // Overlaps with first CE at index 0
        stageSeq.AddCe(firstCe)
        
        stageSeq.AddCe(secondCe)
        
        stageSeq.StageLength |> should equal 2
        stageSeq.Stages.[0].CeCount |> should equal 1
        stageSeq.Stages.[0].Ces.[0] |> should equal firstCe
        stageSeq.Stages.[1].CeCount |> should equal 1
        stageSeq.Stages.[1].Ces.[0] |> should equal secondCe

    [<Fact>]
    let ``AddCe adds to earliest possible stage`` () =
        let sortingWidth = 4<sortingWidth>
        let stageSeq = stageSequence.create(sortingWidth)
        let ce1 = ce.create 0 1 // Stage 0
        let ce2 = ce.create 2 3 // Stage 0
        let ce3 = ce.create 0 2 // Stage 1 (overlaps with ce1)
        let ce4 = ce.create 1 3 // Stage 1 (overlaps with ce2)
        
        stageSeq.AddCe(ce1)
        stageSeq.AddCe(ce2)
        stageSeq.AddCe(ce3)
        stageSeq.AddCe(ce4)
        
        stageSeq.StageLength |> should equal 2
        stageSeq.Stages.[0].CeCount |> should equal 2
        stageSeq.Stages.[0].Ces |> should contain ce1
        stageSeq.Stages.[0].Ces |> should contain ce2
        stageSeq.Stages.[1].CeCount |> should equal 2
        stageSeq.Stages.[1].Ces |> should contain ce3
        stageSeq.Stages.[1].Ces |> should contain ce4

    [<Fact>]
    let ``AddCe adds to earliest possible stage v2`` () =
        let sortingWidth = 6<sortingWidth>
        let stageSeq = stageSequence.create(sortingWidth)
        let ce1 = ce.create 0 1 // Stage 0
        let ce2 = ce.create 2 3 // Stage 0
        let ce3 = ce.create 0 2 // Stage 1 (overlaps with ce1)
        let ce4 = ce.create 4 5 // Stage 0 
        
        stageSeq.AddCe(ce1)
        stageSeq.AddCe(ce2)
        stageSeq.AddCe(ce3)
        stageSeq.AddCe(ce4)
        
        stageSeq.StageLength |> should equal 2
        stageSeq.Stages.[0].CeCount |> should equal 3
        stageSeq.Stages.[0].Ces |> should contain ce1
        stageSeq.Stages.[0].Ces |> should contain ce2
        stageSeq.Stages.[0].Ces |> should contain ce4
        stageSeq.Stages.[1].CeCount |> should equal 1
        stageSeq.Stages.[1].Ces |> should contain ce3

    [<Fact>]
    let ``AddCe with out-of-bounds indices throws exception`` () =
        let sortingWidth = 4<sortingWidth>
        let stageSeq = stageSequence.create(sortingWidth)
        let invalidCe = ce.create 3 4 // 4 is out of bounds
        
        (fun () -> stageSeq.AddCe(invalidCe)) |> should throw typeof<System.ArgumentException>