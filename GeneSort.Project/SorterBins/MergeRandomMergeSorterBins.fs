namespace GeneSort.Project.SorterBins

open System

open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open System.Threading
open GeneSort.Runs
open GeneSort.Db
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.SortingResults.Bins
open GeneSort.Project
open GeneSort.FileDb


//module MergeRandomMergeSorterBins_P1 =
    
//    let maxCenterSampledSetSize = 200

//    // --- Filters ---

//    let sorterModelTypeForSortingWidth (rp: runParameters) =
//        let sorterModelType = rp.GetSorterModelType().Value
//        let sortingWidth = rp.GetSortingWidth().Value
//        let has2factor = (%sortingWidth % 2 = 0)
//        let isMuf4able = (MathUtils.isAPowerOfTwo %sortingWidth)
//        let isMuf6able = (%sortingWidth % 3 = 0) && (MathUtils.isAPowerOfTwo (%sortingWidth / 3))

//        match sorterModelType with
//        | sorterModelType.Msce -> 
//                Some rp
//        | sorterModelType.Mssi
//        | sorterModelType.Msrs -> 
//                if has2factor then Some rp else None
//        | sorterModelType.Msuf4 ->
//                if isMuf4able then Some rp else None
//        | sorterModelType.Msuf6 -> 
//                if isMuf6able then Some rp else None
                

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
//        |> Option.bind sorterModelTypeForSortingWidth
//        |> Option.bind mergeDimensionDividesSortingWidth
//        |> Option.bind limitForMergeFillType
//        |> Option.bind limitForMergeDimension


//    let sortingWidths() : string*string list =
//    //    let values = [ 16; 32; 48; 64; 96; 128;] |> List.map(fun d -> d.ToString())
//        let values = [ 16; 32; 48; ] |> List.map string
//        (runParameters.sortingWidthKey, values)


//    let sorterModelTypes () : string*string list =
//        let values =         
//            [ 
//              sorterModelType.Msce; 
//              sorterModelType.Mssi;
//              sorterModelType.Msrs; 
//              sorterModelType.Msuf4;
//              ]  |> List.map(SorterModelType.toString)
//        (runParameters.sorterModelTypeKey, values )


//    let mergeDimensions () : string * string list =
//    //  let values = [2; 3; 4; 6; 8;] |> List.map string
//        let values = [ 2; 3; 4; ] |> List.map string
//        (runParameters.mergeDimensionKey, values)

//    let mergeSuffixTypes () : string * string list =
//        let values = [ 
//                        mergeSuffixType.NoSuffix; 
//                        mergeSuffixType.VV_1 
//                     ] |> List.map MergeSuffixType.toString
//        (runParameters.mergeSuffixTypeKey, values)


//    let startingRepls () : string * string list =
//        let values = [ 0; ] |> List.map (fun d -> d.ToString())
//        (runParameters.startingReplKey, values)


//    let replSpans () : string * string list =
//        let values = [ 4 ] |> List.map (fun d -> d.ToString())
//        (runParameters.replSpanKey, values)


//    let parameterSpans = 
//        [ 
//            sortingWidths(); 
//            sorterModelTypes();
//            mergeDimensions();
//            mergeSuffixTypes(); 
//            startingRepls();
//            replSpans();
//        ]




module MergeRandomMergeSorterBins_P2 =
    
    let maxCenterSampledSetSize = 200

    // --- Filters ---

    let sorterModelTypeForSortingWidth (rp: runParameters) =
        let sorterModelType = rp.GetSorterModelType().Value
        let sortingWidth = rp.GetSortingWidth().Value
        let has2factor = (%sortingWidth % 2 = 0)
        let isMuf4able = (MathUtils.isAPowerOfTwo %sortingWidth)
        let isMuf6able = (%sortingWidth % 3 = 0) && (MathUtils.isAPowerOfTwo (%sortingWidth / 3))

        match sorterModelType with
        | sorterModelType.Msce -> 
                Some rp
        | sorterModelType.Mssi
        | sorterModelType.Msrs -> 
                if has2factor then Some rp else None
        | sorterModelType.Msuf4 ->
                if isMuf4able then Some rp else None
        | sorterModelType.Msuf6 -> 
                if isMuf6able then Some rp else None
                

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
        |> Option.bind sorterModelTypeForSortingWidth
        |> Option.bind mergeDimensionDividesSortingWidth
        |> Option.bind limitForMergeFillType
        |> Option.bind limitForMergeDimension


    let sortingWidths() : string*string list =
    //    let values = [ 16; 32; 48; ] |> List.map(fun d -> d.ToString())
        let values = [ 16; 32; 48; 64; 96; 128 ] |> List.map string
        (runParameters.sortingWidthKey, values)


    let sorterModelTypes () : string*string list =
        let values =         
            [ 
              sorterModelType.Msce; 
              sorterModelType.Mssi;
              sorterModelType.Msrs; 
              sorterModelType.Msuf4;
              ]  |> List.map(SorterModelType.toString)
        (runParameters.sorterModelTypeKey, values )


    let mergeDimensions () : string * string list =
        let values = [ 8;] |> List.map string
        (runParameters.mergeDimensionKey, values)

    let mergeSuffixTypes () : string * string list =
        let values = [ 
                        mergeSuffixType.NoSuffix; 
                        mergeSuffixType.VV_1 
                     ] |> List.map MergeSuffixType.toString
        (runParameters.mergeSuffixTypeKey, values)


    let startingRepls () : string * string list =
        let values = [ 0; ] |> List.map (fun d -> d.ToString())
        (runParameters.startingReplKey, values)


    let replSpans () : string * string list =
        let values = [ 85 ] |> List.map (fun d -> d.ToString())
        (runParameters.replSpanKey, values)


    let parameterSpans = 
        [ 
            sortingWidths(); 
            sorterModelTypes();
            mergeDimensions();
            mergeSuffixTypes(); 
            startingRepls();
            replSpans();
        ]




module MergeRandomMergeSorterBins =

    let projectName = "MergeRandomMergeSorterBins"  |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\RandomMergeSorterBins\\Merge\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "Merge RandomMergeSorterBins, generated by Msce, Mssi, Msrs, and Msuf"
    let collectNewSortableTests = false
    let samplesPerBin = 1


    // Fixed parameters:
    let rngFactory = rngFactory.LcgFactory
    let excludeSelfCe = true
    let allowOverwrite = false |> UMX.tag<allowOverwrite>


    let makeQueryParams 
            (repl: int<replNumber> option) 
            (startingRepl: int<replNumber> option)
            (replSpan: int<replNumber> option)
            (sortingWidth:int<sortingWidth> option)            
            (mergeDimension: int<mergeDimension> option)
            (mergeSuffixType: mergeSuffixType option)
            (sorterModelType:sorterModelType option)
            (outputDataType: outputDataType) =
             
        queryParams.create
            (Some projectName)
            repl
            outputDataType
            [|
                ( runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
                ( runParameters.mergeDimensionKey, 
                  mergeDimension |> MergeDimension.toString);
                ( runParameters.mergeSuffixTypeKey, 
                  mergeSuffixType |> Option.map MergeSuffixType.toString |> UmxExt.stringToString);
                ( runParameters.sorterModelTypeKey, sorterModelType |> Option.map SorterModelType.toString |> UmxExt.stringToString)
                ( runParameters.startingReplKey, startingRepl |> UmxExt.intToString);
                ( runParameters.replSpanKey, replSpan |> UmxExt.intToString)
            |]


    let makeQueryParamsFromRunParams
            (runParams: runParameters) 
            (outputDataType: outputDataType) =
        makeQueryParams
            (runParams.GetRepl())
            (runParams.GetStartingRepl())
            (runParams.GetReplSpan())
            (runParams.GetSortingWidth())
            (runParams.GetMergeDimension())
            (runParams.GetMergeSuffixType())
            (runParams.GetSorterModelType())
            outputDataType



    // --- Project Refinement ---

    let enhancer (rp : runParameters) : runParameters =
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)

        rp.WithProjectName(Some projectName)
            .WithRunFinished(Some false)
            .WithId (Some qp.Id)


    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        seq {
            for runParameters in runParametersSeq do
                    let filtrate = MergeRandomMergeSorterBins_P2.paramMapFilter runParameters
                    if filtrate.IsSome then
                        let retVal = filtrate.Value |> enhancer
                        yield retVal
        }


    let outputDataTypes = 
            [|
                outputDataType.RunParameters;
                outputDataType.SorterEvalBins "";
                outputDataType.SortingSet "EvenSampled";
                outputDataType.SortingSet "HullSampled";
                outputDataType.SortingSet "CenterSampled";
            |]

    let project = 
            project.create 
                projectName 
                projectDesc
                outputDataTypes




    let executor
            (db: IGeneSortDb)
            (runParameters: runParameters) 
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                // 1. Setup & ID Extraction
                let! _ = checkCancellation cts.Token
                let runId = runParameters |> RunParameters.getIdString
                let repl = runParameters.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
                ProjectOps.report progress (sprintf "%s Starting Run %s repl %d" (MathUtils.getTimestampString()) %runId %repl)

                // 2. Safe Parameter Extraction
                let! (mergeDimension, mergeSuffixType, sorterModelType, sortingWidth, startingRepl, replSpan) = 
                    maybe {
                        let! md = runParameters.GetMergeDimension()
                        let! mst = runParameters.GetMergeSuffixType()
                        let! smt = runParameters.GetSorterModelType()
                        let! sw = runParameters.GetSortingWidth()
                        let! srp = runParameters.GetStartingRepl()
                        let! rsp = runParameters.GetReplSpan()
                        return (md, mst, smt, sw, srp, rsp)
                    } |> Result.ofOption (sprintf "Run %s: Missing required parameters" %runId) |> asAsync


                // 3. Make query params for the output data, which is also used to generate the IDs
                let qpSorterEvalBins = makeQueryParamsFromRunParams 
                                            runParameters 
                                            (outputDataType.SorterEvalBins "")


                let qpEvenSortingSet = makeQueryParamsFromRunParams 
                                                    runParameters 
                                                    (outputDataType.SortingSet "EvenSampled")


                let qpHullSortingSet = makeQueryParamsFromRunParams 
                                                    runParameters 
                                                    (outputDataType.SortingSet "HullSampled")


                let qpCenterSortingSet = makeQueryParamsFromRunParams 
                                                    runParameters 
                                                    (outputDataType.SortingSet "CenterSampled")


                let qpWinningSortingSet = makeQueryParamsFromRunParams 
                                                    runParameters 
                                                    (outputDataType.SortingSet "WinningSampled")


                // 4. create the repl collectors 
                let mutable mergedSorterEvalBins = sorterEvalBins.createEmpty
                                                        (%qpSorterEvalBins.Id |> UMX.tag<sorterEvalBinsId>)

                // This is repeatedly merged with the per-repl sorting sets, and then sampled down to create the even sampled set. 
                // The hull sampled set is a subset of the even sampled set, so it doesn't need its own merged set - 
                // the sampling can be done directly on the mergedSorterEvalBins and mergedSortingSet at the end.
                let mutable mergedEvenSet = sortingSet.create
                                                    (%qpEvenSortingSet.Id |> UMX.tag<sortingSetId>)
                                                    [||]

                let mutable mergedCenterSet = sortingSet.create
                                                    (%qpCenterSortingSet.Id |> UMX.tag<sortingSetId>)
                                                    [||]

                let mutable mergedWinningSet = sortingSet.create
                                                    (%qpWinningSortingSet.Id |> UMX.tag<sortingSetId>)
                                                    [||]

                                            
                // 5.  Get the database for the RandomSorterBins project - this is where the per-repl data will be loaded from
                let dbRandomMergeSorterBins = new GeneSortDbMp(RandomMergeSorterBins.projectFolder) :> IGeneSortDb

                // 6. Loop through the repls, loading and merging the data as we go
                for r in 0 .. (%replSpan - 1) do
                        let currentRepl = %startingRepl + r
                        let qpCurrentEvalBins = RandomMergeSorterBins.makeQueryParams  
                                                    (Some (currentRepl |> UMX.tag<replNumber>))
                                                    (Some sortingWidth) 
                                                    (Some mergeDimension)
                                                    (Some mergeSuffixType)
                                                    (Some sorterModelType) 
                                                    (outputDataType.SorterEvalBins "")

    
                        let! currentEvalBins = dbRandomMergeSorterBins.loadAsync qpCurrentEvalBins
                                                |> AsyncResult.bind (OutputData.asSorterEvalBins >> AsyncResult.ofResult)


                        let qpCurrentSortingSet = RandomMergeSorterBins.makeQueryParams  
                                                    (Some (currentRepl |> UMX.tag<replNumber>))
                                                    (Some sortingWidth) 
                                                    (Some mergeDimension)
                                                    (Some mergeSuffixType)
                                                    (Some sorterModelType) 
                                                    (outputDataType.SortingSet "EvenSampled")


                        let! currentSortingSet = dbRandomMergeSorterBins.loadAsync qpCurrentSortingSet
                                                |> AsyncResult.bind (OutputData.asSortingSet >> AsyncResult.ofResult)

                        // merge with the latest repl's data
                        mergedEvenSet <- SortingSet.merge mergedEvenSet currentSortingSet
                        mergedCenterSet <- SortingSet.merge mergedCenterSet currentSortingSet
                        mergedWinningSet <- SortingSet.merge mergedWinningSet currentSortingSet

                        // merge the eval bins 
                        mergedSorterEvalBins <- SorterEvalBins.merge mergedSorterEvalBins currentEvalBins

                        // sample down to samplesPerBin per bin
                        mergedEvenSet <-
                            sortingSet.create
                                (%qpEvenSortingSet.Id |> UMX.tag<sortingSetId>)
                                (SortingSetFilter.sampleBinsEvenly samplesPerBin mergedSorterEvalBins mergedEvenSet)

                        mergedCenterSet <-
                            sortingSet.create
                                (%qpCenterSortingSet.Id |> UMX.tag<sortingSetId>)
                                (SortingSetFilter.sampleTheCenterBins 
                                        MergeRandomSorterBins_P1.maxCenterSampledSetSize 
                                        mergedSorterEvalBins 
                                        mergedCenterSet )

                        mergedWinningSet <-
                            sortingSet.create
                                (%qpWinningSortingSet.Id |> UMX.tag<sortingSetId>)
                                (SortingSetFilter.sampleWinningBins 
                                        MergeRandomSorterBins_P1.maxCenterSampledSetSize 
                                        mergedSorterEvalBins 
                                        mergedWinningSet )


                        None |> ignore // placeholder for potential per-repl processing

                // 7. Get the convex hull of mergedSortingSet
                let hullSampledSet = 
                            sortingSet.create
                                (%qpHullSortingSet.Id |> UMX.tag<sortingSetId>)
                                (SortingSetFilter.sampleBinsConvexHull samplesPerBin mergedSorterEvalBins mergedEvenSet)


                // 8. Make Report
                let reportName = $"MergeReport_{%sortingWidth}_{sorterModelType |> SorterModelType.toString}_{mergeDimension}_{mergeSuffixType}" 
                                    |> UMX.tag<textReportName>

                let qpMergeReport = makeQueryParamsFromRunParams 
                                            runParameters 
                                            (outputDataType.TextReport reportName)
                let mergeProperties =
                    [ 
                        ("sortingWidth", (Some sortingWidth) |> SortingWidth.toString); 
                        ("sorterModelType", sorterModelType |> SorterModelType.toString)
                        ("mergeDimension", Some mergeDimension |> MergeDimension.toString)
                        ("mergeSuffixType", mergeSuffixType |> MergeSuffixType.toString)
                    ] |> Map.ofList

                let keyFormatter (key: ((int * string) * sorterEvalKey)) =
                        sprintf "%d_%s_%s" (fst (fst key)) (snd (fst key)) ((snd key).AsString())
                        

                let tableMap = SorterEvalBins.getPropertyMaps 
                                    mergedSorterEvalBins 
                                    (%sortingWidth, sorterModelType |> SorterModelType.toString)
                                    mergeProperties
                                |> Map.ofSeq

                let headers, rows = DataTableReport.mapToTabDelimitedStrings keyFormatter tableMap
                let dtReport = DataTableReport.create %reportName headers
                dtReport.AppendDataRows (rows |> Array.toSeq)



                //// 9. Saves

                let! _ = db.saveAsync 
                                qpSorterEvalBins 
                                (mergedSorterEvalBins |> outputData.SorterEvalBins) 
                                allowOverwrite

                let! _ = db.saveAsync 
                                qpEvenSortingSet 
                                (mergedEvenSet |> outputData.SortingSet) 
                                allowOverwrite

                let! _ = db.saveAsync 
                                qpHullSortingSet 
                                (hullSampledSet |> outputData.SortingSet) 
                                allowOverwrite

                let! _ = db.saveAsync 
                                qpCenterSortingSet 
                                (mergedCenterSet |> outputData.SortingSet) 
                                allowOverwrite

                let! _ = db.saveAsync 
                                qpWinningSortingSet 
                                (mergedWinningSet |> outputData.SortingSet) 
                                allowOverwrite

                let! _ = db.saveAsync 
                                qpMergeReport 
                                (dtReport |> outputData.TextReport) 
                                allowOverwrite



                // 10. Final Return
                return runParameters.WithRunFinished (Some true)

            with e -> 
                let runId = runParameters |> RunParameters.getIdString
                let msg = sprintf "Unexpected error in run %s: %s" runId e.Message
                return! async { return Error msg }
        }




