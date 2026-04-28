namespace GeneSort.Eval.V1.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps

[<Struct; StructuralEquality; StructuralComparison>]
type sorterEvalKey =
    private {
        ceCount:     int<ceLength>
        stageLength: int<stageLength>
    }
    static member create (ceCount: int<ceLength>) (stageLength: int<stageLength>) =
        { ceCount = ceCount; stageLength = stageLength; }
    
    member this.CeCount     with get() : int<ceLength>    = this.ceCount
    member this.StageLength with get() : int<stageLength> = this.stageLength
    
    member this.AsString() : string =
        sprintf "ceCount: %d, stageLength: %d" (UMX.untag this.ceCount) (UMX.untag this.stageLength)



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

    /// Weighted combination of ceCount and stageLength — lower values sort first
    let byWeighted 
                (ceCountWeight: float) 
                (stageLengthWeight: float) 
                (key: sorterEvalKey) : float =
        ceCountWeight     * (key.CeCount     |> UMX.untag |> float) +
        stageLengthWeight * (key.StageLength |> UMX.untag |> float)

    let fromSorterEval (eval: sorterEval) : sorterEvalKey =
            sorterEvalKey.create
                eval.CeBlockEval.CeUseCounts.UsedCeCount
                eval.CeBlockEval.getStageSequence.StageLength

    let toDataTableRecord (key: sorterEvalKey) : GeneSort.Core.dataTableRecord =
        GeneSort.Core.dataTableRecord.createEmpty()
        |> GeneSort.Core.dataTableRecord.addKeyAndData "CeCount" (key.CeCount |> UMX.untag |> string)
        |> GeneSort.Core.dataTableRecord.addKeyAndData "StageLength" (key.StageLength |> UMX.untag |> string)