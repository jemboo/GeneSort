namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1


type sorterEvalSelectionType = 
    | Tmb of int<sorterCount>
    | ValueSpan of int<sorterCount>
    | RankSpan of int<sorterCount>
    | TopN of int<sorterCount>

module SorterEvalSelectionType =
    
    let toString = function
        | Tmb count       -> sprintf "Tmb:%d" count
        | ValueSpan count -> sprintf "ValueSpan:%d" count
        | RankSpan count -> sprintf "RankSpan:%d" count
        | TopN count      -> sprintf "TopN:%d" count

    let fromString (str: string) = 
        match str.Split(':') with
        | [| caseName; countStr |] ->
            let count = Int32.Parse(countStr) * 1<sorterCount>
            match caseName with
            | "Tmb"       -> Tmb count
            | "ValueSpan" -> ValueSpan count
            | "RankSpan" -> RankSpan count
            | "TopN"      -> TopN count
            | _           -> failwithf "Invalid case name '%s' in string '%s'" caseName str
        | _ -> 
            failwithf "String '%s' is not in the expected format 'Name:Value'" str

    let toStrategyLabel = function
        | Tmb _       -> "Tmb"
        | ValueSpan _ -> "ValueSpan"
        | RankSpan _ -> "RankSpan"
        | TopN _      -> "TopN"


type sorterEvalSelection = 
    private {
        selectionType : sorterEvalSelectionType
        measure       : sorterEvalMeasure
        labeledSorterEvals : (evalLabel * sorterEval) array
        sortableTestId: Guid<sortableTestId>
    }

    static member Empty = 
        { selectionType = Tmb 0<sorterCount>; 
          measure = CeLength false; 
          labeledSorterEvals = [||] 
          sortableTestId = Guid.Empty |> UMX.tag<sortableTestId>}

    static member create 
                    (selType: sorterEvalSelectionType) 
                    (measure: sorterEvalMeasure) 
                    (items: (evalLabel * sorterEval) array)
                    (sortableTestId: Guid<sortableTestId>) =
            { 
                selectionType = selType; 
                measure = measure; 
                labeledSorterEvals = items 
                sortableTestId = sortableTestId
            }

    member this.SelectionType = this.selectionType
    member this.Measure       = this.measure
    member this.LabeledSorterEvals with get() = this.labeledSorterEvals

    member this.ToEvalLabelMap() : Map<Guid<sorterModelId>, evalLabel> =
        this.labeledSorterEvals 
        |> Array.map (fun (evalLabel, se) -> 
                (se |> SorterEval.getSorterId |> UMX.cast<sorterId,sorterModelId>, evalLabel))
        |> Map.ofArray

    member this.MakeSorterModelSet 
                    (sorterModelSetId: Guid<sorterModelSetId>) 
                    (sorterModelGen: sorterModelGen) : sorterModelSet =
            let sorterModelIds = 
                this.labeledSorterEvals 
                |> Array.map(fun (_, se) -> 
                    se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>)

            let sorterModels = SorterModelGen.makeSorterModelsFromIds sorterModelIds sorterModelGen
            sorterModelSet.create sorterModelSetId sorterModels


module SorterEvalSelection =

    let private prepareDistinctSorted 
                        (measure: sorterEvalMeasure) 
                        (items: sorterEval seq): sorterEval array =
        items
        |> SorterEvalFunctions.filterEvaluations measure
        |> Seq.filter SorterEval.getIsSorted
        |> Seq.distinctBy SorterEval.getSequenceHash
        |> Seq.toArray


    let makeSelection 
                (measure: sorterEvalMeasure) 
                (selType: sorterEvalSelectionType) 
                (items: sorterEval seq) 
                (sortableTestId: Guid<sortableTestId>) : sorterEvalSelection =
        let ranker = SorterEvalFunctions.getFunctionForMeasure measure
        let cleanItems = prepareDistinctSorted measure items
        
        match selType with
        | TopN count ->
            let n = %count
            if n <= 0 then failwithf "TopN count must be greater than zero, but was %d" n
            
            let topN = cleanItems |> Array.sortBy ranker |> Array.truncate n
            let labeledItems = topN |> Array.mapi (fun idx se -> evalLabel.Rank idx, se)
            sorterEvalSelection.create selType measure labeledItems sortableTestId

        | Tmb count ->
            let groupSize = %count
            let targetSize = Math.Min(groupSize, cleanItems.Length / 3)
            
            if targetSize <= 0 then 
                sorterEvalSelection.create selType measure Array.empty sortableTestId
            else
                let sortedItems = cleanItems |> Array.sortBy ranker
                
                let topGroup = sortedItems.[0 .. targetSize - 1] 
                               |> Array.map (fun se -> evalLabel.Tmb tmbGroup.Top, se)
                
                let midStart = cleanItems.Length / 2 - (targetSize / 2)
                let midGroup = sortedItems.[midStart .. midStart + targetSize - 1] 
                               |> Array.map (fun se -> evalLabel.Tmb tmbGroup.Middle, se)
                
                let botGroup = sortedItems.[cleanItems.Length - targetSize .. cleanItems.Length - 1] 
                               |> Array.map (fun se -> evalLabel.Tmb tmbGroup.Bottom, se)
                
                let labeledItems = Array.concat [topGroup; midGroup; botGroup]
                sorterEvalSelection.create selType measure labeledItems sortableTestId

        | RankSpan count ->
            let sampleCount = %count
            if sampleCount <= 0 then failwithf "Sample count must be greater than zero, but was %d" sampleCount
            if cleanItems.Length < sampleCount then failwithf "Cannot sample %d items; only %d options available." 
                                                        sampleCount cleanItems.Length

            let sortedItems = cleanItems |> Array.sortBy ranker
            let result = 
                if sampleCount = 1 then [| sortedItems.[0] |]
                else
                    let res = Array.zeroCreate sampleCount
                    res.[0] <- sortedItems.[0]
                    let step = float (cleanItems.Length - 1) / float (sampleCount - 1)
                    for i in 1 .. (sampleCount - 1) do
                        let targetIndex = int (round (float i * step))
                        res.[i] <- sortedItems.[targetIndex]
                    res

            let labeledItems = result |> Array.mapi (fun idx se -> evalLabel.RankIndex idx, se)
            sorterEvalSelection.create selType measure labeledItems sortableTestId

        | ValueSpan count ->
            let sampleCount = %count
            if sampleCount <= 0 then failwithf "Sample count must be greater than zero, but was %d" sampleCount
            if cleanItems.Length < sampleCount then failwithf "Cannot sample %d items; only %d options available." 
                                                        sampleCount cleanItems.Length

            let sortedItems = cleanItems |> Array.sortBy ranker
            let result = 
                if sampleCount = 1 then [| sortedItems.[0] |]
                else
                    let res = Array.zeroCreate sampleCount
                    res.[0] <- sortedItems.[0]
                    res.[sampleCount - 1] <- sortedItems.[cleanItems.Length - 1]

                    let maxVal = ranker sortedItems.[0]
                    let minVal = ranker sortedItems.[cleanItems.Length - 1]
                    let valRange = maxVal - minVal

                    if valRange = 0.0 then
                        let step = float (cleanItems.Length - 1) / float (sampleCount - 1)
                        for i in 1 .. (sampleCount - 1) do
                            let targetIndex = int (round (float i * step))
                            res.[i] <- sortedItems.[targetIndex]
                    else
                        let targetStep = valRange / float (sampleCount - 1)
                        for i in 1 .. (sampleCount - 2) do
                            let targetValue = maxVal - (float i * targetStep)
                            let closestMatch = sortedItems |> Array.minBy (fun se -> Math.Abs(ranker se - targetValue))
                            res.[i] <- closestMatch
                    res

            let labeledItems = result |> Array.mapi (fun idx se -> evalLabel.ValueIndex idx, se)
            sorterEvalSelection.create selType measure labeledItems sortableTestId


module EvalReporting =

    let evalLabelsToDtr (labels: evalLabel seq) : dataTableRecord =
            dataTableRecord.createEmpty() 
            |> dataTableRecord.addData "EvalLabel" (EvalLabel.toString labels)

    let evalLabelToDtr (label: evalLabel) : dataTableRecord =
            seq { yield label } |> evalLabelsToDtr

    let private createContextDtr (leadCols: dataTableRecord) (selection: sorterEvalSelection) =
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData "SelectionType" (SorterEvalSelectionType.toString selection.SelectionType)
        |> dataTableRecord.addData "SelectionStrategy" (SorterEvalSelectionType.toStrategyLabel selection.SelectionType)
        |> dataTableRecord.addData "Measure" (SorterEvalMeasure.toString selection.Measure)
        |> dataTableRecord.combine leadCols
        |> dataTableRecord.combine (SorterEvalMeasure.toDataTableRecord selection.Measure)

    let toManyDataTableRecords
                (leadCols: dataTableRecord) 
                (recordMaker: sorterEval -> dataTableRecord [])
                (selection: sorterEvalSelection) : dataTableRecord array =

        let contextDtr = createContextDtr leadCols selection

        selection.LabeledSorterEvals
        |> Array.collect (fun (label, se) -> 
            let labelDtr = evalLabelToDtr label |> dataTableRecord.combine contextDtr
            se 
            |> recordMaker
            |> Array.map (fun customDtr -> customDtr |> dataTableRecord.combine labelDtr)
        )

    let toDataTableRecords 
        (leadCols: dataTableRecord) 
        (labelPfx: string)
        (selection: sorterEvalSelection) : Map<Guid<sorterId>, dataTableRecord> =

        let contextDtr = createContextDtr leadCols selection

        selection.LabeledSorterEvals
        |> Array.map (fun (label, se) -> 
            let labelDtr = evalLabelToDtr label |> dataTableRecord.combine contextDtr
            (
                se |> SorterEval.getSorterId,
                se |> SorterEval.toDataTableRecordWithPrefix labelPfx
                   |> dataTableRecord.combine labelDtr
            )
        ) 
        |> Map.ofArray