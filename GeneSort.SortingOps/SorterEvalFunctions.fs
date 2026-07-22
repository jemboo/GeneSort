namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

module private CompactStringParser =

    let getValue (key: string) (input: string) : string =
        let prefix = key + "="
        if input.Contains(prefix) then
            let startIdx = input.IndexOf(prefix) + prefix.Length
            let remainder = input.Substring(startIdx)
            // Trim off trailing commas, parentheses, or extra text
            remainder.Split([| ','; ')'; ' ' |], StringSplitOptions.RemoveEmptyEntries).[0].Trim()
        else
            failwithf "Key '%s' not found in compact string '%s'" key input

    let parseBool<[<Measure>] 'm> (key: string) (input: string) : bool<'m> =
        let raw = getValue key input
        match Boolean.TryParse(raw) with
        | true, v -> UMX.tag<'m> v
        | _ -> failwithf "Failed to parse bool for key '%s' in '%s'" key input

    let parseFloat<[<Measure>] 'm> (key: string) (input: string) : float<'m> =
        let raw = getValue key input
        match Double.TryParse(raw) with
        | true, v -> UMX.tag<'m> v
        | _ -> failwithf "Failed to parse float for key '%s' in '%s'" key input



type ceLengthMeasure = private {
    filterUnsorted: bool<filterUnsorted>
    filterReflectionSymmetric: bool<filterReflectionSymmetric>
} with
    static member create (filterUnsorted: bool) (filterReflectionSymmetric: bool) : ceLengthMeasure =
        {
            filterUnsorted = UMX.tag filterUnsorted
            filterReflectionSymmetric = UMX.tag filterReflectionSymmetric
        }

    member this.FilterUnsorted: bool<filterUnsorted> = this.filterUnsorted
    member this.FilterReflectionSymmetric: bool<filterReflectionSymmetric> = this.filterReflectionSymmetric

    member this.ToCompactString() = 
        sprintf "CeLen(fUnsorted=%b, fRefl=%b)" (%this.filterUnsorted) (%this.filterReflectionSymmetric)

    static member FromCompactString(s: string) : ceLengthMeasure =
        let fUnsorted = CompactStringParser.parseBool "fUnsorted" s
        let fRefl = CompactStringParser.parseBool "fRefl" s
        ceLengthMeasure.create fUnsorted fRefl


type stageLengthMeasure = private {
    filterUnsorted: bool<filterUnsorted>
    filterReflectionSymmetric: bool<filterReflectionSymmetric>
} with
    static member create (filterUnsorted: bool) (filterReflectionSymmetric: bool) : stageLengthMeasure =
        {
            filterUnsorted = UMX.tag filterUnsorted
            filterReflectionSymmetric = UMX.tag filterReflectionSymmetric
        }

    member this.FilterUnsorted: bool<filterUnsorted> = this.filterUnsorted
    member this.FilterReflectionSymmetric: bool<filterReflectionSymmetric> = this.filterReflectionSymmetric

    member this.ToCompactString() = 
        sprintf "StgLen(fUnsorted=%b, fRefl=%b)" (%this.filterUnsorted) (%this.filterReflectionSymmetric)

    static member FromCompactString(s: string) : stageLengthMeasure =
        let fUnsorted = CompactStringParser.parseBool "fUnsorted" s
        let fRefl = CompactStringParser.parseBool "fRefl" s
        stageLengthMeasure.create fUnsorted fRefl


type unsortedCountMeasure = private {
    filterReflectionSymmetric: bool<filterReflectionSymmetric>
} with
    static member create (filterReflectionSymmetric: bool) : unsortedCountMeasure =
        {
            filterReflectionSymmetric = UMX.tag filterReflectionSymmetric
        }

    member this.FilterReflectionSymmetric: bool<filterReflectionSymmetric> = this.filterReflectionSymmetric

    member this.ToCompactString() = 
        sprintf "UnsortedCnt(fRefl=%b)" (%this.filterReflectionSymmetric)

    static member FromCompactString(s: string) : unsortedCountMeasure =
        let fRefl = CompactStringParser.parseBool "fRefl" s
        unsortedCountMeasure.create fRefl


type ceStMeasure = private {
    stageWeight: float<stageWeight>
    filterUnsorted: bool<filterUnsorted>
    filterReflectionSymmetric: bool<filterReflectionSymmetric>
} with
    static member create 
                (stageWeight: float<stageWeight>) 
                (filterUnsorted: bool<filterUnsorted>) 
                (filterReflectionSymmetric: bool<filterReflectionSymmetric>) 
                : ceStMeasure =
        {
            stageWeight = stageWeight
            filterUnsorted = filterUnsorted
            filterReflectionSymmetric = filterReflectionSymmetric
        }

    member this.StageWeight: float<stageWeight> = this.stageWeight
    member this.FilterUnsorted: bool<filterUnsorted> = this.filterUnsorted
    member this.FilterReflectionSymmetric: bool<filterReflectionSymmetric> = this.filterReflectionSymmetric

    member this.ToCompactString() = 
        sprintf "CeSt(stW=%.2f, fUnsorted=%b, fRefl=%b)" (%this.stageWeight) (%this.filterUnsorted) (%this.filterReflectionSymmetric)

    static member FromCompactString(s: string) : ceStMeasure =
        let stW = CompactStringParser.parseFloat "stW" s
        let fUnsorted = CompactStringParser.parseBool "fUnsorted" s
        let fRefl = CompactStringParser.parseBool "fRefl" s
        ceStMeasure.create stW fUnsorted fRefl


type ceStUcMeasure = private {
    stageWeight: float<stageWeight>
    unsortedWeight: float<unsortedWeight>
    filterReflectionSymmetric: bool<filterReflectionSymmetric>
} with
    static member create (stageWeight: float<stageWeight>) 
                         (unsortedWeight: float<unsortedWeight>) 
                         (filterReflectionSymmetric: bool<filterReflectionSymmetric>) 
                         : ceStUcMeasure =
        {
            stageWeight = stageWeight
            unsortedWeight = unsortedWeight
            filterReflectionSymmetric = filterReflectionSymmetric
        }

    member this.StageWeight: float<stageWeight> = this.stageWeight
    member this.UnsortedWeight: float<unsortedWeight> = this.unsortedWeight
    member this.FilterReflectionSymmetric: bool<filterReflectionSymmetric> = this.filterReflectionSymmetric

    member this.ToCompactString() = 
        sprintf "CeStUc(stW=%.2f, ucW=%.2f, fRefl=%b)" (%this.stageWeight) (%this.unsortedWeight) (%this.filterReflectionSymmetric)

    static member FromCompactString(s: string) : ceStUcMeasure =
        let stW = CompactStringParser.parseFloat "stW" s
        let ucW = CompactStringParser.parseFloat "ucW" s
        let fRefl = CompactStringParser.parseBool "fRefl" s
        ceStUcMeasure.create stW ucW fRefl


type sorterEvalMeasure =
    | CeLength of ceLengthMeasure
    | StageLength of stageLengthMeasure
    | UnsortedCount of unsortedCountMeasure
    | CeSt of ceStMeasure
    | CeStUc of ceStUcMeasure


module SorterEvalMeasure =

    let toCompactString (measure: sorterEvalMeasure) : string =
        match measure with
        | CeLength m -> m.ToCompactString()
        | StageLength m -> m.ToCompactString()
        | UnsortedCount m -> m.ToCompactString()
        | CeSt m -> m.ToCompactString()
        | CeStUc m -> m.ToCompactString()

    let fromCompactString (s: string) : sorterEvalMeasure =
        let trimmed = s.Trim()
        if trimmed.StartsWith("CeLen") then
            CeLength (ceLengthMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("StgLen") then
            StageLength (stageLengthMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("UnsortedCnt") then
            UnsortedCount (unsortedCountMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("CeStUc") then
            CeStUc (ceStUcMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("CeSt") then
            CeSt (ceStMeasure.FromCompactString trimmed)
        else
            failwithf "Unknown compact measure format in '%s'" s

    let fromCompactStringOpt (s: string) : sorterEvalMeasure option =
        try Some (fromCompactString s) with _ -> None

    let toDataTableRecord (measure: sorterEvalMeasure) : dataTableRecord =
        let baseRecord = dataTableRecord.createEmpty()
        match measure with
        | CeLength m ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "CeLength"
            |> dataTableRecord.addData "M_FilterUnsorted" (string %m.FilterUnsorted)
            |> dataTableRecord.addData "M_FilterReflectionSymmetric" (string %m.FilterReflectionSymmetric)
        | StageLength m ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "StageLength"
            |> dataTableRecord.addData "M_FilterUnsorted" (string %m.FilterUnsorted)
            |> dataTableRecord.addData "M_FilterReflectionSymmetric" (string %m.FilterReflectionSymmetric)
        | UnsortedCount m ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "UnsortedCount"
            |> dataTableRecord.addData "M_FilterUnsorted" "false"
            |> dataTableRecord.addData "M_FilterReflectionSymmetric" (string %m.FilterReflectionSymmetric)
        | CeSt m ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "CeSt"
            |> dataTableRecord.addData "M_StageLengthWeight" (string %m.StageWeight)
            |> dataTableRecord.addData "M_FilterUnsorted" (string %m.FilterUnsorted)
            |> dataTableRecord.addData "M_FilterReflectionSymmetric" (string %m.FilterReflectionSymmetric)
        | CeStUc m ->
            baseRecord
            |> dataTableRecord.addData "MeasureType" "CeStUc"
            |> dataTableRecord.addData "M_StageLengthWeight" (string %m.StageWeight)
            |> dataTableRecord.addData "M_UnsortedCountWeight" (string %m.UnsortedWeight)
            |> dataTableRecord.addData "M_FilterUnsorted" "false"
            |> dataTableRecord.addData "M_FilterReflectionSymmetric" (string %m.FilterReflectionSymmetric)


module SorterEvalFunctions =

    let byTwoWeighted (ceCountWeight: float) (stageLengthWeight: float) (eval: sorterEval) = 
        let ceCount = float (SorterEval.getCeLength eval)
        let stageLength = float (SorterEval.getStageLength eval)
        (ceCount * ceCountWeight) + (stageLength * stageLengthWeight)

    let byUnsortedCount (m: float) (eval: sorterEval) =
        let uc = float (SorterEval.getUnsortedCount eval)
        if uc <= 0.0 then 0.0 else m * Math.Log uc

    let getFunctionForMeasure (measure: sorterEvalMeasure) : (sorterEval -> float) =
        match measure with
        | CeLength _ -> 
            SorterEval.getCeLength >> float
        | StageLength _ -> 
            SorterEval.getStageLength >> float
        | UnsortedCount _ -> 
            byUnsortedCount 1.0
        | CeSt m -> 
            byTwoWeighted 1.0 (%m.StageWeight)
        | CeStUc m ->
            let ceFunc = SorterEval.getCeLength >> float
            let stageFunc = SorterEval.getStageLength >> float
            let unsortedFunc = byUnsortedCount (%m.UnsortedWeight)
            fun eval ->
                let cePart = ceFunc eval * 1.0
                let stagePart = stageFunc eval * (%m.StageWeight)
                let unsortedPart = unsortedFunc eval
                cePart + stagePart + unsortedPart

    let getFilterUnsortedFlag (measure: sorterEvalMeasure) : bool<filterUnsorted> =
        match measure with
        | CeLength m -> m.FilterUnsorted
        | StageLength m -> m.FilterUnsorted
        | CeSt m -> m.FilterUnsorted
        | UnsortedCount _ -> UMX.tag<filterUnsorted> false
        | CeStUc _ -> UMX.tag<filterUnsorted> false

    let getFilterReflectionSymmetricFlag (measure: sorterEvalMeasure) : bool<filterReflectionSymmetric> =
        match measure with
        | CeLength m -> m.FilterReflectionSymmetric
        | StageLength m -> m.FilterReflectionSymmetric
        | UnsortedCount m -> m.FilterReflectionSymmetric
        | CeSt m -> m.FilterReflectionSymmetric
        | CeStUc m -> m.FilterReflectionSymmetric

    let filterEvaluations 
                    (measure: sorterEvalMeasure) 
                    (items: seq<sorterEval>) : seq<sorterEval> =
        items
        |> Seq.filter (fun se -> 
            let passUnsorted = 
                not (%getFilterUnsortedFlag measure) || (SorterEval.getUnsortedCount se <= 0<sortableCount>)
            let passRefl = 
                not (%getFilterReflectionSymmetricFlag measure) || 
                (SorterEval.getIsReflectionSymmetric se |> UMX.untag)
            passUnsorted && passRefl
        )
    let toCompactString (measure: sorterEvalMeasure) : string =
            match measure with
            | CeLength m -> m.ToCompactString()
            | StageLength m -> m.ToCompactString()
            | UnsortedCount m -> m.ToCompactString()
            | CeSt m -> m.ToCompactString()
            | CeStUc m -> m.ToCompactString()


    /// Parses a sorterEvalMeasure from its compact string representation.
    let fromCompactString (s: string) : sorterEvalMeasure =
        let trimmed = s.Trim()
        if trimmed.StartsWith("CeLen") then
            CeLength (ceLengthMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("StgLen") then
            StageLength (stageLengthMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("UnsortedCnt") then
            UnsortedCount (unsortedCountMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("CeStUc") then // Note: check CeStUc before CeSt to prevent false match on prefix
            CeStUc (ceStUcMeasure.FromCompactString trimmed)
        elif trimmed.StartsWith("CeSt") then
            CeSt (ceStMeasure.FromCompactString trimmed)
        else
            failwithf "Unknown compact measure format in '%s'" s

    /// Safe version of fromCompactString returning option.
    let fromCompactStringOpt (s: string) : sorterEvalMeasure option =
        try Some (fromCompactString s) with _ -> None