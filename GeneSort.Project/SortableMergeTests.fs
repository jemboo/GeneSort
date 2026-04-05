namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Runs
open GeneSort.Db
open GeneSort.Model.Sortable



//module SortableMergeTests_P1 =

//    // --- Filters ---

//    let mergeDimensionDividesSortingWidth (rp: runParameters) =
//        let sw = rp.GetSortingWidth().Value
//        let md = rp.GetMergeDimension().Value
//        if (%sw % %md = 0) then Some rp else None

//    let limitForMergeFillType (rp: runParameters) =
//        let sw = rp.GetSortingWidth().Value
//        let ft = rp.GetMergeSuffixType().Value
//        if (ft.IsNoSuffix && %sw > 64) then None else Some rp

//    let limitForMergeDimension (rp: runParameters) =
//        let sw = rp.GetSortingWidth().Value
//        let md = rp.GetMergeDimension().Value
//        if (%md > 6 && %sw > 144) then None else Some rp

//    let paramMapFilter (rp: runParameters) = 
//        Some rp
//        |> Option.bind mergeDimensionDividesSortingWidth
//        |> Option.bind limitForMergeFillType
//        |> Option.bind limitForMergeDimension



//    // --- Parameter Spans ---

//    let sortableDataFormats () : string * string list =
//        let values = [
//                        sortableDataFormat.Int8Vector512;
//                     ] |> List.map SortableDataFormat.toString
//        (runParameters.sortableDataFormatKey, values)


//    let sortingWidths () : string * string list =
//        let values = [16; 18; 24; 32; 36; 48; 64; 96; 128; 192; 256] |> List.map string
//        (runParameters.sortingWidthKey, values)


//    let mergeDimensions () : string * string list =
//        let values = [2; 3; 4; 6; 8 ] |> List.map string
//        (runParameters.mergeDimensionKey, values)


//    let mergeFillTypes () : string * string list =
//        let values = [ 
//                        mergeSuffixType.NoSuffix; 
//                        mergeSuffixType.VV_1 
//                     ] |> List.map MergeFillType.toString
//        (runParameters.mergeSuffixTypeKey, values)

//    let parameterSpans = 
//        [
//            sortingWidths();
//            sortableDataFormats(); 
//            mergeDimensions(); 
//            mergeFillTypes(); 
//        ]





module SortableMergeTests_P2 =

    // --- Filters ---

    let mergeDimensionDividesSortingWidth (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%sw % %md = 0) then Some rp else None

    let limitForMergeFillType (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let ft = rp.GetMergeSuffixType().Value
        if (ft.IsNoSuffix && %sw > 64) then None else Some rp

    let limitForMergeDimension (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%md > 6 && %sw > 144) then None else Some rp

    let paramMapFilter (rp: runParameters) = 
        Some rp
        |> Option.bind mergeDimensionDividesSortingWidth
        |> Option.bind limitForMergeFillType
        |> Option.bind limitForMergeDimension



    // --- Parameter Spans ---

    let sortableDataFormats () : string * string list =
        let values = [
                        sortableDataFormat.Int8Vector512;
                     ] |> List.map SortableDataFormat.toString
        (runParameters.sortableDataFormatKey, values)


    let sortingWidths () : string * string list =
        let values = [ 16; 32; 48; 64; 96; 128 ] |> List.map string
        (runParameters.sortingWidthKey, values)


    let mergeDimensions () : string * string list =
        let values = [2; 3; 4; 6; 8; ] |> List.map string
        (runParameters.mergeDimensionKey, values)


    let mergeFillTypes () : string * string list =
        let values = [ 
                        mergeSuffixType.NoSuffix; 
                        mergeSuffixType.VV_1 
                     ] |> List.map MergeFillType.toString
        (runParameters.mergeSuffixTypeKey, values)

    let parameterSpans = 
        [
            sortingWidths();
            sortableDataFormats(); 
            mergeDimensions(); 
            mergeFillTypes(); 
        ]



        

module SortableMergeTests =

    let projectName = "SortableMergeTests" |> UMX.tag<projectName>
    let projectFolder = "SortableMergeTests" |> UMX.tag<projectFolder>
    let projectDesc = "Calc and save large SortableMergeTests"

    let makeQueryParams 
            (repl: int<replNumber> option) 
            (sortingWidth: int<sortingWidth> option)
            (mergeDimension: int<mergeDimension> option)
            (mergeFillType: mergeSuffixType option)
            (sortableDataFormat: sortableDataFormat option)
            (outputDataType: outputDataType) : queryParams =
             
        queryParams.create
            (Some projectName)
            repl
            outputDataType
            [|
                (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
                (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
                (runParameters.mergeSuffixTypeKey, mergeFillType |> Option.map MergeFillType.toString |> UmxExt.stringToString );
                (runParameters.sortableDataFormatKey, sortableDataFormat |> Option.map SortableDataFormat.toString |> UmxExt.stringToString );
            |]

    let makeQueryParamsFromRunParams
            (runParams: runParameters) 
            (outputDataType: outputDataType) =

        makeQueryParams
            (runParams.GetRepl())
            (runParams.GetSortingWidth())
            (runParams.GetMergeDimension())
            (runParams.GetMergeSuffixType())
            (runParams.GetSortableDataFormat())
            outputDataType




    // --- Project Refinement ---

    let enhancer (rp : runParameters) =
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        rp.WithProjectName(Some projectName)
            .WithRunFinished(Some false)
            .WithId (Some qp.Id)

    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (SortableMergeTests_P2.paramMapFilter >> Option.map enhancer)


    let outputDataTypes = 
            [|                
                outputDataType.SortableTestSet "";
                outputDataType.RunParameters;
            |]

    let project = 
            project.create 
                projectName 
                projectDesc
                outputDataTypes
                SortableMergeTests_P2.parameterSpans


        // --- Executor ---
    let executor
            (db: IGeneSortDb)
            (projectFolder: string<projectFolder>)
            (runParams: runParameters) 
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                // 1. Setup
                let! _ = checkCancellation cts.Token
                let runId = runParams |> RunParameters.getIdString
                progress |> Option.iter (fun p -> p.Report(sprintf "Starting Run %s (Generation)" %runId))

                // 2. Safe extraction of domain parameters
                let! (sortingWidth, mergeDimension, mergeFillType, sortableDataFormat) = 
                    maybe {
                        let! width = runParams.GetSortingWidth()
                        let! dim = runParams.GetMergeDimension()
                        let! fill = runParams.GetMergeSuffixType()
                        let! dataType = runParams.GetSortableDataFormat()
                        return (width, dim, fill, dataType)
                    } |> Result.ofOption "Missing domain parameters required for generation"

                // 3. Create SortableTestModel
                let sortableTestModel = 
                    msasM.create sortingWidth mergeDimension mergeFillType 
                    |> sortableTestModel.MsasMi
            
                let qpForSortableTest = makeQueryParamsFromRunParams runParams (outputDataType.SortableTest "") 
                let sortableTests = SortableTestModel.makeSortableTest 
                                            (%qpForSortableTest.Id |> UMX.tag<sorterTestId>) 
                                            sortableTestModel 
                                            sortableDataFormat

                // 4. Save
                let! _ = checkCancellation cts.Token
            
                // The builder automatically handles short-circuiting if saveAsync returns Error
                let! _ = db.saveAsync projectFolder qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite

                // 5. Success
                return runParams.WithRunFinished (Some true)

            with e ->
                let runId = runParams |> RunParameters.getIdString
                let msg = sprintf "Fatal error in Generation Run %s: %s" runId e.Message
                return! async { return Error msg }
        }