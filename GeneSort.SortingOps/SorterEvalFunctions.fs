namespace GeneSort.SortingOps

open System
open GeneSort.Core
open GeneSort.Sorting

type sorterEvalMeasure =
    | CeLength of filterUnsorted: bool
    | StageLength of filterUnsorted: bool
    | UnsortedCount
    | CeSt of stageWeight: float * filterUnsorted: bool
    | CeStUc of stageWeight: float * unsortedWeight: float

module SorterEvalMeasure =

    let toString (measure: sorterEvalMeasure) =
        match measure with
        | CeLength f -> sprintf "CeLength: FilterUnsorted=%b" f
        | StageLength f -> sprintf "StageLength: FilterUnsorted=%b" f
        | UnsortedCount -> "UnsortedCount"
        | CeSt (stW, f) -> sprintf "CeSt: ST_Weight=%.2f, FilterUnsorted=%b" stW f
        | CeStUc (stW, ucW) -> sprintf "CeStUc: ST_Weight=%.2f, UC_Weight=%.2f" stW ucW

    let fromString (s: string) : sorterEvalMeasure =
        let parseValue (prefix: string) (input: string) : float =
            if input.Contains(prefix) then
                let startIdx = input.IndexOf(prefix) + prefix.Length
                let remainder = input.Substring(startIdx).Split(',').[0].Trim()
                match Double.TryParse(remainder) with
                | true, v -> v
                | _ -> failwithf "Invalid number format in '%s'" input
            else failwithf "Expected prefix '%s' not found in '%s'" prefix input

        let parseBool (prefix: string) (input: string) : bool =
            if input.Contains(prefix) then
                let startIdx = input.IndexOf(prefix) + prefix.Length
                let remainder = input.Substring(startIdx).Split(',').[0].Trim()
                match Boolean.TryParse(remainder) with
                | true, b -> b
                | _ -> failwithf "Invalid boolean format in '%s'" input
            else false

        if s.StartsWith("CeLength") then
            CeLength (parseBool "FilterUnsorted=" s)
        elif s.StartsWith("StageLength") then
            StageLength (parseBool "FilterUnsorted=" s)
        elif s.StartsWith("UnsortedCount") then
            UnsortedCount
        elif s.StartsWith("CeSt:") then
            CeSt (parseValue "ST_Weight=" s, parseBool "FilterUnsorted=" s)
        elif s.StartsWith("CeStUc:") then
            CeStUc (parseValue "ST_Weight=" s, parseValue "UC_Weight=" s)
        else 
            failwithf "Unknown measure type in '%s'" s

    let fromStringOpt (s: string) : sorterEvalMeasure option =
        try Some (fromString s) with _ -> None

    let toDataTableRecord (measure: sorterEvalMeasure) : dataTableRecord =
        let baseRecord = dataTableRecord.createEmpty()
        match measure with
        | CeLength f ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "CeLength"
            |> dataTableRecord.addData "M_FilterUnsorted" (string f)
        | StageLength f ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "StageLength"
            |> dataTableRecord.addData "M_FilterUnsorted" (string f)
        | UnsortedCount ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "UnsortedCount"
            |> dataTableRecord.addData "M_FilterUnsorted" "false"
        | CeSt (stW, f) ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "CeSt"
            |> dataTableRecord.addData "M_StageLengthWeight" (string stW)
            |> dataTableRecord.addData "M_FilterUnsorted" (string f)
        | CeStUc (stW, ucW) ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "CeStUc"
            |> dataTableRecord.addData "M_StageLengthWeight" (string stW)
            |> dataTableRecord.addData "M_UnsortedCountWeight" (string ucW)
            |> dataTableRecord.addData "M_FilterUnsorted" "false"

module SorterEvalFunctions =

    let byTwoWeighted (ceCountWeight: float) (stageLengthWeight: float) (eval: sorterEval) = 
        let ceCount = float (SorterEval.getCeLength eval)
        let stageLength = float (SorterEval.getStageLength eval)
        (ceCount * ceCountWeight) + (stageLength * stageLengthWeight)

    let byEqualTwoWeighted (eval: sorterEval) = 
        byTwoWeighted 1.0 1.0 eval

    let byUnsortedCount (m: float) (eval: sorterEval) =
        let uc = float (SorterEval.getUnsortedCount eval)
        if uc <= 0.0 then 0.0 else m * Math.Log uc

    let getFunctionForMeasure (measure: sorterEvalMeasure) : (sorterEval -> float) =
        match measure with
        | CeLength _ -> 
            SorterEval.getCeLength >> float
        | StageLength _ -> 
            SorterEval.getStageLength >> float
        | UnsortedCount -> 
            byUnsortedCount 1.0
        | CeSt (stageWeight, _) -> 
            // ceWeight defaults implicitly to 1.0
            byTwoWeighted 1.0 stageWeight
        | CeStUc (stageWeight, unsortedWeight) ->
            let ceFunc = SorterEval.getCeLength >> float
            let stageFunc = SorterEval.getStageLength >> float
            let unsortedFunc = byUnsortedCount unsortedWeight
            fun eval ->
                let cePart = ceFunc eval * 1.0
                let stagePart = stageFunc eval * stageWeight
                let unsortedPart = unsortedFunc eval
                cePart + stagePart + unsortedPart

    let getFilterUnsortedFlag (measure: sorterEvalMeasure) : bool =
        match measure with
        | CeLength f -> f
        | StageLength f -> f
        | CeSt (_, f) -> f
        | UnsortedCount -> false
        | CeStUc _ -> false

    let filterEvaluations (measure: sorterEvalMeasure) (items: seq<sorterEval>) : seq<sorterEval> =
        if getFilterUnsortedFlag measure then
            items |> Seq.filter (fun se -> SorterEval.getUnsortedCount se <= 0<sortableCount>)
        else
            items