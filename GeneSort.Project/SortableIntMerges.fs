namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Runs
open GeneSort.Db
open GeneSort.Model.Sortable

module SortableIntMerges =

    let projectName = "SortableIntMerges" |> UMX.tag<projectName>
    let projectDesc = "Calc and save large SortableIntMerges"

    let makeQueryParams 
            (repl: int<replNumber> option) 
            (sortingWidth: int<sortingWidth> option)
            (mergeDimension: int<mergeDimension> option)
            (mergeFillType: mergeFillType option)
            (sortableDataType: sortableDataType option)
            (outputDataType: outputDataType) =
             
        queryParams.create(
            Some projectName,
            repl,
            outputDataType,
            [|
                (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
                (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
                (runParameters.mergeFillTypeKey, mergeFillType |> MergeFillType.toString);
                (runParameters.sortableDataTypeKey, sortableDataType |> SortableDataType.toString);
            |])

    let makeQueryParamsFromRunParams
            (runParams: runParameters) 
            (outputDataType: outputDataType) =

        makeQueryParams
            (runParams.GetRepl())
            (runParams.GetSortingWidth())
            (runParams.GetMergeDimension())
            (runParams.GetMergeFillType())
            (runParams.GetSortableDataType())
            outputDataType


    // --- Parameter Spans ---

    let sortableArrayDataTypeKeys () : string * string list =
        let values = [ Some sortableDataType.Ints; Some sortableDataType.Bools ] |> List.map SortableDataType.toString
        (runParameters.sortableDataTypeKey, values)

    let sortingWidths () : string * string list =
        let values = [16; 18; 24; 32; 36; ] |> List.map string
        (runParameters.sortingWidthKey, values)

    let mergeDimensions () : string * string list =
        let values = [2; 3; 4; 6; 8] |> List.map string
        (runParameters.mergeDimensionKey, values)

    let mergeFillTypes () : string * string list =
        let values = [ Some mergeFillType.NoFill; Some mergeFillType.VanVoorhis ] |> List.map MergeFillType.toString
        (runParameters.mergeFillTypeKey, values)


    // --- Filters ---

    let mergeDimensionDividesSortingWidth (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%sw % %md = 0) then Some rp else None

    let limitForBoolenDataType (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let dt = rp.GetSortableDataType().Value
        if (dt.IsBools && %sw > 32) then None else Some rp

    let limitForMergeFillType (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let ft = rp.GetMergeFillType().Value
        if (ft.IsNoFill && %sw > 32) then None else Some rp

    let limitForMergeDimension (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%md > 6 && %sw > 144) then None else Some rp

    let paramMapFilter (rp: runParameters) = 
        Some rp
        |> Option.bind mergeDimensionDividesSortingWidth
        |> Option.bind limitForBoolenDataType
        |> Option.bind limitForMergeFillType
        |> Option.bind limitForMergeDimension



    // --- Project Refinement ---

    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 

        let enhancer (rp : runParameters) =
            // Use empty string for RunParameters case if required by your model
            let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
            rp.WithProjectName(projectName)
              .WithRunFinished(false)
              .WithId (qp.Id.ToString() |> UMX.tag<idValue>)

        runParametersSeq 
        |> Seq.choose (paramMapFilter >> Option.map enhancer)

    let parameterSpans = 
        [
            sortingWidths();
            sortableArrayDataTypeKeys(); 
            mergeDimensions(); 
            mergeFillTypes(); 
        ]
        
    let outputDataTypes = 
            [|                
                outputDataType.SortableTestSet ""; // Adjusted for string param
                outputDataType.RunParameters;
            |]

    let project = 
            project.create 
                projectName 
                projectDesc
                parameterSpans
                outputDataTypes

        // --- Executor ---
    let executor
            (db: IGeneSortDb)
            (runParams: runParameters) 
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        async {
            try
                // 1. Safe extraction of metadata
                let runId = runParams.GetId() |> Option.defaultValue (% "unknown")
                let repl = runParams.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
            
                // 2. Safe extraction of domain parameters using your new 'maybe' builder
                let domainParams = maybe {
                    let! width = runParams.GetSortingWidth()
                    let! dim = runParams.GetMergeDimension()
                    let! fill = runParams.GetMergeFillType()
                    let! dataType = runParams.GetSortableDataType()
                    return (width, dim, fill, dataType)
                }

                match domainParams with
                | None -> 
                    return Error (sprintf "Run %s: Missing required parameters (Width, Dimension, Fill, or DataType)" %runId)
                | Some (sortingWidth, mergeDimension, mergeFillType, sortableDataType) ->

                    cts.Token.ThrowIfCancellationRequested()
            
                    progress |> Option.iter (fun p -> 
                        p.Report(sprintf "Run %s (Repl %s): Creating sortable tests" %runId (Repl.toString (Some repl))))
            
                    // 3. Logic Execution
                    let sortableTestModel = msasM.create sortingWidth mergeDimension mergeFillType |> sortableTestModel.MsasMi
                    let sortableTests = SortableTestModel.makeSortableTests sortableTestModel sortableDataType

                    cts.Token.ThrowIfCancellationRequested()

                    // 4. Save with proper Result propagation
                    let qpForSortableTest = makeQueryParamsFromRunParams runParams (outputDataType.SortableTest "") 
                    let! saveResult = db.saveAsync qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite

                    match saveResult with
                    | Ok () ->
                        progress |> Option.iter (fun p -> p.Report(sprintf "Run %s finished successfully." %runId))
                        return Ok (runParams.WithRunFinished true)
                    | Error err ->
                        let msg = sprintf "Failed to save run %s: %s" %runId err
                        progress |> Option.iter (fun p -> p.Report(msg))
                        return Error msg

            with e -> 
                // Correctly untagging runId for the string format specifier
                let rawId = runParams.GetId() |> Option.map UMX.untag |> Option.defaultValue "unknown"
                return Error (sprintf "Unexpected exception in run %s: %s" rawId e.Message)
        }