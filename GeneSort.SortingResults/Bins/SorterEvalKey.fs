namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting

[<Struct; StructuralEquality; StructuralComparison>]
type sorterEvalKey =
    private {
        ceCount:     int<ceLength>
        stageLength: int<stageLength>
        isSorted:    bool
    }
    static member create (ceCount: int<ceLength>) (stageLength: int<stageLength>) (isSorted: bool) =
        { ceCount = ceCount; stageLength = stageLength; isSorted = isSorted }
    member this.CeCount     with get() : int<ceLength>    = this.ceCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    member this.IsSorted    with get() : bool             = this.isSorted


module SorterEvalKey =

    // No action
    let noAction (key: sorterEvalKey) : float =
        0.0
        
    /// Raw ce count as a float
    let byCeCount (key: sorterEvalKey) : float =
        key.CeCount |> UMX.untag |> float

    /// Raw stage length as a float
    let byStageLength (key: sorterEvalKey) : float =
        key.StageLength |> UMX.untag |> float

    /// Ratio of ces to stages — lower means more ces are packed per stage
    let byCePerStage (key: sorterEvalKey) : float =
        let stages = key.StageLength |> UMX.untag |> float
        if stages = 0.0 then infinity
        else (key.CeCount |> UMX.untag |> float) / stages

    /// Sorted layers first, unsorted last
    let byIsSorted (key: sorterEvalKey) : float =
        if key.IsSorted then 0.0 else 1.0

    /// Primary: isSorted ascending, secondary: ceCount ascending
    let bySortedThenCeCount (key: sorterEvalKey) : float =
        let sortedScore = if key.IsSorted then 0.0 else 1.0e6
        sortedScore + (key.CeCount |> UMX.untag |> float)

    /// Primary: isSorted ascending, secondary: stageLength ascending
    let bySortedThenStageLength (key: sorterEvalKey) : float =
        let sortedScore = if key.IsSorted then 0.0 else 1.0e6
        sortedScore + (key.StageLength |> UMX.untag |> float)

    /// Weighted combination of ceCount and stageLength — lower values sort first
    let byWeighted 
                (ceCountWeight: float) 
                (stageLengthWeight: float) 
                (key: sorterEvalKey) : float =
        ceCountWeight     * (key.CeCount     |> UMX.untag |> float) +
        stageLengthWeight * (key.StageLength |> UMX.untag |> float)

    /// Primary: isSorted ascending; secondary: weighted ceCount and stageLength
    let bySortedThenWeighted 
                    (ceCountWeight: float) 
                    (stageLengthWeight: float) 
                    (key: sorterEvalKey) : float =
        let sortedScore = if key.IsSorted then 0.0 else 1.0e6
        sortedScore + byWeighted ceCountWeight stageLengthWeight key
