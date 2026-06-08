namespace GeneSort.SortingOps
open System
open GeneSort.Core


type sorterEvalMeasure =
    | CeLength of float
    | StageLength of float
    | UnsortedCount of float
    | CeSt of float * float
    | CeStUc of float * float * float


module SorterEvalMeasure =

    let toString (measure: sorterEvalMeasure) =
        match measure with
        | CeLength v -> sprintf "CeLength: %.2f" v
        | StageLength v -> sprintf "StageLength: %.2f" v
        | UnsortedCount v -> sprintf "UnsortedCount: %.2f" v
        | CeSt (st, uc) -> sprintf "CeSt: ST=%.2f, UC=%.2f" st uc
        | CeStUc (st, uc, ceLen) -> sprintf "CeStUc: ST=%.2f, UC=%.2f, CeLen=%.2f" st uc ceLen

    let fromString (s: string) : sorterEvalMeasure =
        let parseValue (prefix:string) : float =
            if s.StartsWith(prefix) then
                let valueStr = s.Substring(prefix.Length).Trim()
                match Double.TryParse(valueStr) with
                | (true, v) -> v
                | _ -> failwithf "Invalid number format in '%s'" s
            else failwithf "Expected prefix '%s' not found in '%s'" prefix s
        if s.StartsWith("CeLength:") then
            CeLength (parseValue "CeLength:")
        elif s.StartsWith("StageLength:") then
            StageLength (parseValue "StageLength:")
        elif s.StartsWith("UnsortedCount:") then
            UnsortedCount (parseValue "UnsortedCount:")
        elif s.StartsWith("CeSt:") then
            let parts = s.Substring("CeSt:".Length).Split(',')
            if parts.Length = 2 then
                let st = parseValue "ST="
                let uc = parseValue "UC="
                CeSt (st, uc)
            else failwithf "Expected 2 values for CeSt in '%s'" s
        elif s.StartsWith("CeStUc:") then
            let parts = s.Substring("CeStUc:".Length).Split(',')
            if parts.Length = 3 then
                let st = parseValue "ST="
                let uc = parseValue "UC="
                let ceLen = parseValue "CeLen="
                CeStUc (st, uc, ceLen)
            else failwithf "Expected 3 values for CeStUc in '%s'" s
        else failwithf "Unknown measure type in '%s'" s




    let fromStringOpt (s: string) : sorterEvalMeasure option =
        let parseValue (prefix: string) (input: string) =
            if input.StartsWith(prefix) then
                let valuePart = input.Substring(prefix.Length).Trim()
                match Double.TryParse(valuePart) with
                | (true, v) -> Some v
                | _ -> None
            else None
        if s.StartsWith("CeLength:") then parseValue "CeLength:" s |> Option.map CeLength
        elif s.StartsWith("StageLength:") then parseValue "StageLength:" s |> Option.map StageLength
        elif s.StartsWith("UnsortedCount:") then parseValue "UnsortedCount:" s |> Option.map UnsortedCount
        elif s.StartsWith("CeSt:") then
            let parts = s.Substring("CeSt:".Length).Split(',')
            if parts.Length = 2 then
                let stPart = parts.[0].Trim()
                let ucPart = parts.[1].Trim()
                match Double.TryParse(stPart.Substring(3).Trim()) with
                | (true, st) ->
                    match Double.TryParse(ucPart.Substring(3).Trim()) with
                    | (true, uc) -> Some (CeSt(st, uc))
                    | _ -> None
                | _ -> None
            else None
        elif s.StartsWith("CeStUc:") then
            let parts = s.Substring("CeStUc:".Length).Split(',')
            if parts.Length = 3 then
                let stPart = parts.[0].Trim()
                let ucPart = parts.[1].Trim()
                let ceLenPart = parts.[2].Trim()
                match Double.TryParse(stPart.Substring(3).Trim()) with
                | (true, st) ->
                    match Double.TryParse(ucPart.Substring(3).Trim()) with
                    | (true, uc) ->
                        match Double.TryParse(ceLenPart.Substring(6).Trim()) with
                        | (true, ceLen) -> Some (CeStUc(st, uc, ceLen))
                        | _ -> None
                    | _ -> None
                | _ -> None
            else None
        else None



    let toDataTableRecord (measure: sorterEvalMeasure) : dataTableRecord =
        match measure with
        | CeLength len ->
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "MeasureType" "CeLength"
            |> dataTableRecord.addData "M_CeLength" (len |> string)
        | StageLength len ->
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "MeasureType" "StageLength"
            |> dataTableRecord.addData "M_StageLength" (len |> string)
        | UnsortedCount count ->
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "MeasureType" "UnsortedCount"
            |> dataTableRecord.addData "M_UnsortedCount" (count |> string)
        | CeSt (ce, st) ->
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "MeasureType" "CeSt"
            |> dataTableRecord.addData "M_CeLength" (ce |> string)
            |> dataTableRecord.addData "M_Stagelength" (st |> string)
        | CeStUc (ce, st, uc) ->
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "MeasureType" "CeStUc"
            |> dataTableRecord.addData "M_Stagelength" (st |> string)
            |> dataTableRecord.addData "M_UnsortedCount" (uc |> string)
            |> dataTableRecord.addData "M_CeLength" (ce |> string)

module SorterEvalFunctions =

    // lower ceCounts and stageLengths have a lower value, and thus sort first ascending.
    let byTwoWeighted (ceCountWeight: float) (stageLengthWeight: float) (eval: sorterEval) = 
        let ceCount = float (SorterEval.getCeLength eval)
        let stageLength = float (SorterEval.getStageLength eval)
        (ceCount * ceCountWeight) + (stageLength * stageLengthWeight)

    let byEqualTwoWeighted (eval: sorterEval) = 
        byTwoWeighted 1.0 1.0 eval

    let byUnsortedCount (m:float) (eval: sorterEval) =
        m * Math.Log (float (SorterEval.getUnsortedCount eval))

    let getFunctionForMeasure (measure: sorterEvalMeasure) : (sorterEval -> float) =
        match measure with
        | CeLength m -> SorterEval.getCeLength >> float >> (*) m
        | StageLength m -> SorterEval.getStageLength >> float >> (*) m  
        | UnsortedCount m -> byUnsortedCount m
        | CeSt (ceWeight, stageWeight) -> byTwoWeighted ceWeight stageWeight
        | CeStUc (ceWeight, stageWeight, unsortedWeight) ->
            let ceFunc = SorterEval.getCeLength >> float
            let stageFunc = SorterEval.getStageLength >> float
            let unsortedFunc =  byUnsortedCount unsortedWeight
            fun eval ->
                let cePart = ceFunc eval * ceWeight
                let stagePart = stageFunc eval * stageWeight
                let unsortedPart = unsortedFunc eval * unsortedWeight
                cePart + stagePart + unsortedPart



      
