namespace GeneSort.Project.SorterBins

open System

open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting.Sorter.Rs
open GeneSort.Model.Sorting.Sorter.Uf6
open System.Threading
open GeneSort.Runs
open GeneSort.Db
open GeneSort.Model.Sorting
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.SortingResults.Bins
open GeneSort.FileDb
open GeneSort.Project

//module RandomMergeSorterBins_P1 =

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



//    // --- Parameter Spans ---
    
//    let sortingWidths() : string*string list =
//        let values = [ 16; 32; 48; 64; 96; 128;] |> List.map string
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


//    let sortableDataFormats () : string * string list =
//        let values = [ sortableDataFormat.Int8Vector512; ] |> List.map SortableDataFormat.toString
//        (runParameters.sortableDataFormatKey, values)


//    let mergeDimensions () : string * string list =
//        let values = [2; 3; 4; 6; 8;] |> List.map string
//        (runParameters.mergeDimensionKey, values)


        
//    let mergeFillTypes () : string * string list =
//        let values = [ 
//                        mergeSuffixType.NoSuffix; 
//                        mergeSuffixType.VV_1 
//                     ] |> List.map MergeFillType.toString
//        (runParameters.mergeSuffixTypeKey, values)


//    let sorterCounts () : string * string list =
//        let values = [ 1000;] |> List.map (fun d -> d.ToString())
//        (runParameters.sorterCountKey, values)


//    let parameterSpans = 
//        [ sortingWidths(); 
//          sorterModelTypes(); 
//          sortableDataFormats(); 
//          mergeDimensions(); 
//          mergeFillTypes(); 
//          sorterCounts() ]





module RandomMergeSorterBins_P2 =

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



    // --- Parameter Spans ---
    
    let sortingWidths() : string*string list =
        let values = [ 16; 32; 48; ] |> List.map string
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


    let sortableDataFormats () : string * string list =
        let values = [ sortableDataFormat.Int8Vector512; ] |> List.map SortableDataFormat.toString
        (runParameters.sortableDataFormatKey, values)


    let mergeDimensions () : string * string list =
        let values = [2; 3; 4;] |> List.map string
        (runParameters.mergeDimensionKey, values)

        
    let mergeFillTypes () : string * string list =
        let values = [ 
                        mergeSuffixType.NoSuffix; 
                        mergeSuffixType.VV_1 
                     ] |> List.map MergeSuffixType.toString
        (runParameters.mergeSuffixTypeKey, values)


    let sorterCounts () : string * string list =
        let values = [ 100;] |> List.map (fun d -> d.ToString())
        (runParameters.sorterCountKey, values)


    let parameterSpans = 
        [ sortingWidths(); 
          sorterModelTypes(); 
          sortableDataFormats(); 
          mergeDimensions(); 
          mergeFillTypes(); 
          sorterCounts() ]






module RandomMergeSorterBins =
    
    let projectName = "RandomMergeSorterBins"  |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\RandomMergeSorterBins\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "RandomMergeSorterBins, generated by Msce, Mssi, Msrs, and Msuf"


    // Fixed parameters:
    let rngFactory = rngFactory.LcgFactory
    let excludeSelfCe = true
    let allowOverwrite = false |> UMX.tag<allowOverwrite>
    let collectNewSortableTests = false
    let samplesPerBin = 1

    let makeQueryParams 
            (repl: int<replNumber> option) 
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
                ( runParameters.sortingWidthKey, 
                  sortingWidth |> SortingWidth.toString); 
                ( runParameters.mergeDimensionKey, 
                  mergeDimension |> MergeDimension.toString);
                ( runParameters.mergeSuffixTypeKey, 
                  mergeSuffixType |> Option.map MergeSuffixType.toString |> UmxExt.stringToString);
                ( runParameters.sorterModelTypeKey, 
                  sorterModelType |> Option.map SorterModelType.toString |> UmxExt.stringToString)
            |]


    let makeQueryParamsFromRunParams
            (runParams: runParameters) 
            (outputDataType: outputDataType) =
        makeQueryParams
            (runParams.GetRepl())
            (runParams.GetSortingWidth())
            (runParams.GetMergeDimension())
            (runParams.GetMergeSuffixType())
            (runParams.GetSorterModelType())
            outputDataType


    // Implied Parameters:

    let getStageLengthForSortingWidth 
                        (smt:sorterModelType) 
                        (sortingWidth: int<sortingWidth>) : int<stageLength> =
            match %sortingWidth with
            | 4 -> 5 |> UMX.tag<stageLength>
            | 6 -> 20 |> UMX.tag<stageLength>
            | 8 -> 40 |> UMX.tag<stageLength>
            | 12 -> 80 |> UMX.tag<stageLength>
            | 16 -> 100 |> UMX.tag<stageLength>
            | 18 -> 120 |> UMX.tag<stageLength>
            | 20 -> 130 |> UMX.tag<stageLength>
            | 22 -> 140 |> UMX.tag<stageLength>
            | 24 -> 200 |> UMX.tag<stageLength>
            | 32 ->  match smt with
                        | sorterModelType.Msce -> 300 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 300 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 300 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 600 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt

            | 48 ->  match smt with
                        | sorterModelType.Msce -> 400 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 400 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 400 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 1200 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt

            | 64 -> match smt with
                        | sorterModelType.Msce -> 600 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 600 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 800 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 2000 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt

            | 96 -> match smt with
                        | sorterModelType.Msce -> 800 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 800 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 800 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 2800 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt


            | 128 -> match smt with
                        | sorterModelType.Msce -> 1200 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 1500 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 1800 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 4000 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt

            | 196 -> match smt with
                        | sorterModelType.Msce -> 2000 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 2200 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 2800 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 4800 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt

            | 256 -> match smt with
                        | sorterModelType.Msce -> 3000 |> UMX.tag<stageLength>
                        | sorterModelType.Mssi -> 3000 |> UMX.tag<stageLength>
                        | sorterModelType.Msrs -> 4000 |> UMX.tag<stageLength>
                        | sorterModelType.Msuf4 -> 6000 |> UMX.tag<stageLength>
                        | _ -> failwithf "Unsupported sorter model type: %A" smt
            | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)




     

    // --- Project Refinement ---

    let enhancer (rp : runParameters) : runParameters =
        let sortingWidth = rp.GetSortingWidth().Value
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        let stageLength = getStageLengthForSortingWidth (rp.GetSorterModelType().Value) sortingWidth
        let ceLength = stageLength |> StageLength.toCeLength sortingWidth

        rp.WithProjectName(Some projectName)
            .WithRunFinished(Some false)
            .WithCeLength(Some ceLength)
            .WithStageLength(Some stageLength)
            .WithId (Some qp.Id)


    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        seq {
            for runParameters in runParametersSeq do
                    let filtrate = RandomMergeSorterBins_P2.paramMapFilter runParameters
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
            |]

    let project = 
            project.create 
                projectName 
                projectDesc
                outputDataTypes
                RandomMergeSorterBins_P2.parameterSpans


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
                ProjectOps.report progress (sprintf "%s Starting Run %s" (MathUtils.getTimestampString()) %runId)

                // 2. Safe Parameter Extraction
                let! (  repl, 
                        mergeDimension, 
                        mergeSuffixType, 
                        sorterModelType, 
                        sortingWidth, 
                        stageLength, 
                        ceLength, 
                        sorterCount, 
                        sortableDataFormat) = 
                    maybe {
                        let! r = runParameters.GetRepl()
                        let! md = runParameters.GetMergeDimension()
                        let! mst = runParameters.GetMergeSuffixType()
                        let! smt = runParameters.GetSorterModelType()
                        let! sw = runParameters.GetSortingWidth()
                        let! sl = runParameters.GetStageLength()
                        let! cl = runParameters.GetCeLength()
                        let! sc = runParameters.GetSorterCount()
                        let! sdf = runParameters.GetSortableDataFormat()
                        return (r, md, mst, smt, sw, sl, cl, sc, sdf)
                    } |> Result.ofOption (sprintf "Run %s: Missing required parameters" %runId) |> asAsync



                // 3. Create sorting set from run parameters
                let sorterModelGen =
                    match sorterModelType with
                    | sorterModelType.Msce -> 
                        msceRandGen.create rngFactory sortingWidth excludeSelfCe ceLength 
                        |> sorterModelGen.SmmMsceRandGen
                    | sorterModelType.Mssi -> 
                        mssiRandGen.create rngFactory sortingWidth stageLength 
                        |> sorterModelGen.SmmMssiRandGen
                    | sorterModelType.Msrs -> 
                        let opsGenRatesArray = OpsGenRatesArray.createUniform %stageLength
                        msrsRandGen.create rngFactory sortingWidth opsGenRatesArray 
                        |> sorterModelGen.SmmMsrsRandGen
                    | sorterModelType.Msuf4 -> 
                        let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageLength %sortingWidth
                        msuf4RandGen.create rngFactory sortingWidth stageLength uf4GenRatesArray 
                        |> sorterModelGen.SmmMsuf4RandGen
                    | sorterModelType.Msuf6 -> 
                        let uf6GenRatesArray = Uf6GenRatesArray.createUniform %stageLength %sortingWidth
                        msuf6RandGen.create rngFactory sortingWidth stageLength uf6GenRatesArray 
                        |> sorterModelGen.SmmMsuf6RandGen

                let firstIndex = (%repl * %sorterCount) |> UMX.tag<sorterCount>
                let sortingGenSegment = sortingGenSegment.create 
                                                (sorterModelGen |> sortingGen.Single)
                                                firstIndex 
                                                sorterCount
                let qpSortingSet = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "") 
                let fullSortingSet = sortingGenSegment.MakeSortingSet (%qpSortingSet.Id |> UMX.tag<sortingSetId>)
                let fullSorterSet = fullSortingSet |> SortingSet.makeSorterSet


                // 4. Load the sortable tests
                let! _ = checkCancellation cts.Token
                let qpSortableTests = SortableMergeTests.makeQueryParams 
                                            (Some (0 |> UMX.tag<replNumber>)) 
                                            (Some sortingWidth) 
                                            (Some mergeDimension) 
                                            (Some mergeSuffixType) 
                                            (Some sortableDataFormat) 
                                            (outputDataType.SortableTest "")
                                            
                let dbSortableMergeTests = new GeneSortDbMp(SortableMergeTests.projectFolder) :> IGeneSortDb
                let! rawTestData = dbSortableMergeTests.loadAsync qpSortableTests 
                let! sortableTests = rawTestData |> OutputData.asSortableTest


                // 5. Evaluate sortings          
                let qpEval = makeQueryParamsFromRunParams 
                                        runParameters 
                                        (outputDataType.SorterSetEval "")
                let sorterSetEval = 
                                SorterSetEval.makeSorterSetEval
                                                (%qpEval.Id |> UMX.tag<sorterSetEvalId>) 
                                                fullSorterSet 
                                                sortableTests 
                                                collectNewSortableTests

                // 6. Map sorterEvals to sortings
                let sortingResultSetMap = SortingResultSetMap.fromSortingSet fullSortingSet
                sortingResultSetMap.UpdateManySortingResults sorterSetEval.SorterEvals


                // 7. Make the evalBins and the sampled sorterSets
                let qpEvalBins = makeQueryParamsFromRunParams 
                                            runParameters 
                                            (outputDataType.SorterEvalBins "")

                let qpEvenSampledSortingSet = makeQueryParamsFromRunParams 
                                                runParameters 
                                                (outputDataType.SortingSet "EvenSampled")

                //let qpHullSampledSortingSet = makeQueryParamsFromRunParams 
                //                                runParameters 
                //                                (outputDataType.SortingSet "HullSampled")

                let sorterEvalBins = sorterEvalBins.create
                                                (%qpEvalBins.Id |> UMX.tag<sorterEvalBinsId>)
                                                sorterSetEval.SorterEvals
                let evenSampledSortingSet = 
                        sortingSet.create
                            (%qpEvenSampledSortingSet.Id |> UMX.tag<sortingSetId>)
                            (SortingSetFilter.sampleBinsEvenly samplesPerBin sorterEvalBins fullSortingSet)

                //let hullSampledSortingSet = 
                //        sortingSet.create
                //            (%qpEvenSampledSortingSet.Id |> UMX.tag<sortingSetId>)
                //            (SortingSetFilter.sampleBinsConvexHull samplesPerBin sorterEvalBins fullSortingSet)

                // 4. Saves
                let! _ = db.saveAsync qpEvalBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
                let! _ = db.saveAsync qpEvenSampledSortingSet (evenSampledSortingSet |> outputData.SortingSet) allowOverwrite
                //let! _ = db.saveAsync projectFolder qpHullSampledSortingSet (hullSampledSortingSet |> outputData.SortingSet) allowOverwrite

            
                progress |> Option.iter (fun p -> 
                    p.Report(sprintf "Saved SortingSet %s for run: %s" (%qpSortingSet.Id.ToString()) %runId))

                // 5. Final Return
                return runParameters.WithRunFinished (Some true)

            with e -> 
                let runId = runParameters |> RunParameters.getIdString
                let msg = sprintf "Unexpected error in run %s: %s" runId e.Message
                return! async { return Error msg }
        }




//module Yab =



//    let executor
//            (db: IGeneSortDb)
//            (runParameters: runParameters) 
//            (allowOverwrite: bool<allowOverwrite>)
//            (cts: CancellationTokenSource) 
//            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

//        asyncResult {
//            try
//                // 1. Setup & ID Extraction
//                let! _ = checkCancellation cts.Token
//                let runId = runParameters |> RunParameters.getIdString
//                ProjectOps.report progress (sprintf "%s Starting Run %s" (MathUtils.getTimestampString()) %runId)

//                // 2. Safe Parameter Extraction
//                let! (  repl, 
//                        mergeDimension, 
//                        mergeSuffixType, 
//                        sorterModelType, 
//                        sortingWidth, 
//                        stageLength, 
//                        ceLength, 
//                        sorterCount, 
//                        sortableDataFormat) = 
//                    maybe {
//                        let! r = runParameters.GetRepl()
//                        let! md = runParameters.GetMergeDimension()
//                        let! mst = runParameters.GetMergeSuffixType()
//                        let! smt = runParameters.GetSorterModelType()
//                        let! sw = runParameters.GetSortingWidth()
//                        let! sl = runParameters.GetStageLength()
//                        let! cl = runParameters.GetCeLength()
//                        let! sc = runParameters.GetSorterCount()
//                        let! sdf = runParameters.GetSortableDataFormat()
//                        return (r, md, mst, smt, sw, sl, cl, sc, sdf)
//                    } |> Result.ofOption (sprintf "Run %s: Missing required parameters" %runId) |> asAsync



//                // 3. Create sorting set from run parameters
//                let sorterModelGen =
//                    match sorterModelType with
//                    | sorterModelType.Msce -> 
//                        msceRandGen.create rngFactory sortingWidth excludeSelfCe ceLength 
//                        |> sorterModelGen.SmmMsceRandGen
//                    | sorterModelType.Mssi -> 
//                        mssiRandGen.create rngFactory sortingWidth stageLength 
//                        |> sorterModelGen.SmmMssiRandGen
//                    | sorterModelType.Msrs -> 
//                        let opsGenRatesArray = OpsGenRatesArray.createUniform %stageLength
//                        msrsRandGen.create rngFactory sortingWidth opsGenRatesArray 
//                        |> sorterModelGen.SmmMsrsRandGen
//                    | sorterModelType.Msuf4 -> 
//                        let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageLength %sortingWidth
//                        msuf4RandGen.create rngFactory sortingWidth stageLength uf4GenRatesArray 
//                        |> sorterModelGen.SmmMsuf4RandGen
//                    | sorterModelType.Msuf6 -> 
//                        let uf6GenRatesArray = Uf6GenRatesArray.createUniform %stageLength %sortingWidth
//                        msuf6RandGen.create rngFactory sortingWidth stageLength uf6GenRatesArray 
//                        |> sorterModelGen.SmmMsuf6RandGen

//                let firstIndex = (%repl * %sorterCount) |> UMX.tag<sorterCount>
//                let sortingGenSegment = sortingGenSegment.create 
//                                                (sorterModelGen |> sortingGen.Single)
//                                                firstIndex 
//                                                sorterCount
//                let qpSortingSet = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "") 
//                let fullSortingSet = sortingGenSegment.MakeSortingSet (%qpSortingSet.Id |> UMX.tag<sortingSetId>)
//                let fullSorterSet = fullSortingSet |> SortingSet.makeSorterSet


//                // 4. Create sortable tests
//                let! _ = checkCancellation cts.Token
//                let qpSortableTests = SortableMergeTests.makeQueryParams 
//                                            (Some (0 |> UMX.tag<replNumber>)) 
//                                            (Some sortingWidth) 
//                                            (Some mergeDimension) 
//                                            (Some mergeSuffixType) 
//                                            (Some sortableDataFormat) 
//                                            (outputDataType.SortableTest "")

//                let! rawTestData = db.loadAsync qpSortableTests 
//                let! sortableTests = rawTestData |> OutputData.asSortableTest


//                // 5. Evaluate sortings          
//                let qpEval = makeQueryParamsFromRunParams 
//                                        runParameters 
//                                        (outputDataType.SorterSetEval "")
//                let sorterSetEval = 
//                                SorterSetEval.makeSorterSetEval
//                                                (%qpEval.Id |> UMX.tag<sorterSetEvalId>) 
//                                                fullSorterSet 
//                                                sortableTests 
//                                                collectNewSortableTests

//                // 6. Map sorterEvals to sortings
//                let sortingResultSetMap = SortingResultSetMap.fromSortingSet fullSortingSet
//                sortingResultSetMap.UpdateManySortingResults sorterSetEval.SorterEvals


//                // 7. Make the evalBins and the sampled sorterSets
//                let qpEvalBins = makeQueryParamsFromRunParams 
//                                            runParameters 
//                                            (outputDataType.SorterEvalBins "")

//                let qpEvenSampledSortingSet = makeQueryParamsFromRunParams 
//                                                runParameters 
//                                                (outputDataType.SortingSet "EvenSampled")

//                //let qpHullSampledSortingSet = makeQueryParamsFromRunParams 
//                //                                runParameters 
//                //                                (outputDataType.SortingSet "HullSampled")

//                let sorterEvalBins = sorterEvalBins.create
//                                                (%qpEvalBins.Id |> UMX.tag<sorterEvalBinsId>)
//                                                sorterSetEval.SorterEvals
//                let evenSampledSortingSet = 
//                        sortingSet.create
//                            (%qpEvenSampledSortingSet.Id |> UMX.tag<sortingSetId>)
//                            (SortingSetFilter.sampleBinsEvenly samplesPerBin sorterEvalBins fullSortingSet)

//                //let hullSampledSortingSet = 
//                //        sortingSet.create
//                //            (%qpEvenSampledSortingSet.Id |> UMX.tag<sortingSetId>)
//                //            (SortingSetFilter.sampleBinsConvexHull samplesPerBin sorterEvalBins fullSortingSet)

//                // 4. Saves
//                let! _ = db.saveAsync qpEvalBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
//                let! _ = db.saveAsync qpEvenSampledSortingSet (evenSampledSortingSet |> outputData.SortingSet) allowOverwrite
//                //let! _ = db.saveAsync projectFolder qpHullSampledSortingSet (hullSampledSortingSet |> outputData.SortingSet) allowOverwrite

            
//                progress |> Option.iter (fun p -> 
//                    p.Report(sprintf "Saved SortingSet %s for run: %s" (%qpSortingSet.Id.ToString()) %runId))

//                // 5. Final Return
//                return runParameters.WithRunFinished (Some true)

//            with e -> 
//                let runId = runParameters |> RunParameters.getIdString
//                let msg = sprintf "Unexpected error in run %s: %s" runId e.Message
//                return! async { return Error msg }
//        }

