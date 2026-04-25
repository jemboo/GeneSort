namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.Runs
open GeneSort.Db
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.ModelParams
open GeneSort.SortingResults
open GeneSort.SortingResults.Bins
open GeneSort.FileDb
open OpsUtils

type mutateSortingSetHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _childSortersPerParent : int<sorterCount>
    }
    static member Create (db: IGeneSortDb) (parameterSpans: (string * string list) list) (childSorters: int<sorterCount>) =
        { _projectDb = db; _parameterSpans = parameterSpans; _childSortersPerParent = childSorters }

    member this.ParameterSpans = this._parameterSpans
    member this.ProjectDb = this._projectDb
    member this.ChildSortersPerParent = this._childSortersPerParent
    member this.CollectNewSortableTests = false

    /// Encapsulates the specific DB logic for loading parent sorters
    member this.GetRandomSortersDb() = 
        new GeneSortDbMp(RandomSorters.projectFolder) :> IGeneSortDb

    /// Centralized parameter extraction for better error reporting
    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! smt = rp.GetSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! sdt = rp.GetSortableDataFormat()
            let! mr = rp.GetMutationRate()
            return (smt, sw, sdt, mr)
        }

module MutateSortingSet =
    // --- Project Constants ---
    let projectName = "MutateSortingSet" |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\MutateSortingSet\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "Mutate SortingSets and get bin stats for each member of the set."
    let rngFactory = rngFactory.LcgFactory
    let firstSortingIndex = 0<sorterCount>

    // --- Query Parameter Helpers ---
    let makeQueryParams (repl: int<replNumber> option) (sortingWidth:int<sortingWidth> option)
                        (sorterModelType:sorterModelType option) (sortableDataFormat:sortableDataFormat option)
                        (outputDataType: outputDataType) : queryParams =
        queryParams.create (Some projectName) repl outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> UmxExt.intToString ); 
               (runParameters.sorterModelTypeKey, sorterModelType |> Option.map SorterModelType.toString |> UmxExt.stringToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat |> Option.map SortableDataFormat.toString |> UmxExt.stringToString ); |]

    let makeQueryParamsFromRunParams (runParams: runParameters) (outputDataType: outputDataType) =
        makeQueryParams (runParams.GetRepl()) (runParams.GetSortingWidth())
                        (runParams.GetSorterModelType()) (runParams.GetSortableDataFormat()) outputDataType

    // --- Filtering Logic ---
    let sorterModelTypeForSortingWidth (rp: runParameters) =
        let sorterModelKey = rp.GetSorterModelType().Value
        let sortingWidth = rp.GetSortingWidth().Value
        let has2factor = (%sortingWidth % 2 = 0)
        let isMuf4able = (MathUtils.isAPowerOfTwo %sortingWidth)
        let isMuf6able = (%sortingWidth % 3 = 0) && (MathUtils.isAPowerOfTwo (%sortingWidth / 3))

        match sorterModelKey with
        | sorterModelType.Msce -> Some rp
        | sorterModelType.Mssi | sorterModelType.Msrs -> if has2factor then Some rp else None
        | sorterModelType.Msuf4 -> if isMuf4able then Some rp else None
        | sorterModelType.Msuf6 -> if isMuf6able then Some rp else None
        | _ -> None

    let paramMapFilter (rp: runParameters) = 
        Some rp |> Option.bind sorterModelTypeForSortingWidth

    let enhancer (rp : runParameters) : runParameters =
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        rp.WithProjectName(Some projectName).WithRunFinished(Some false).WithId (Some qp.Id)

    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose paramMapFilter 
        |> Seq.map enhancer
        |> Seq.toArray // Forces evaluation and prevents re-computation
        |> Array.toSeq // Convert back to seq for compatibility with host

    // --- Private Internal Helpers for Executor ---
    let private saveResults (db: IGeneSortDb) allowOverwrite (results: (queryParams * outputData) list) =
        asyncResult {
            for (qp, data) in results do
                let! _ = db.saveAsync qp data allowOverwrite
                ()
            return ()
        }

    let private performMutation 
                    (parent: sortingSet) 
                    (smt: sorterModelType) 
                    (rate: float<mutationRate>) (childCount: int<sorterCount>) : 
                                (sortingMutationSegment[] * sorting[] * sorter[])  =
        let mutator = SorterModelMutateParams.makeUniformMutatorForSorterModel 
                        smt parent.StageLength parent.SortingWidth rate rngFactory
        let segments = parent.Sortings |> Array.map (fun sm ->
            SorterMutateParamsOps.makeSortingMutationSegmentFromSorting 
                sm mutator firstSortingIndex childCount)
        let mutantSortings = segments |> Array.collect (fun s -> s.MakeMutantSortings)
        let mutantSorters = segments |> Array.collect (fun s -> s.MakeMutantSorters)
        (segments, mutantSortings, mutantSorters)


    // --- Config Spans ---
    let mutationRateKeys() = (runParameters.mutationRateKey , [ "0.05" ])
    let sortingWidthKeys() = (runParameters.sortingWidthKey, [ "16" ])
    let sorterModelKeys() = (runParameters.sorterModelTypeKey, [ sorterModelType.Msce ] |> List.map SorterModelType.toString)
    let sortableDataFormatKeys () = (runParameters.sortableDataFormatKey, [ sortableDataFormat.BitVector512 ] |> List.map SortableDataFormat.toString)

    let parameterSpans = [ sortingWidthKeys(); sorterModelKeys(); sortableDataFormatKeys(); mutationRateKeys()]
    let outputDataTypes = 
                [|
                    outputDataType.RunParameters;
                    outputDataType.SorterSetEval "";
                    outputDataType.TextReport ("Bins" |> UMX.tag<textReportName>); 
                    outputDataType.TextReport ("Profiles" |> UMX.tag<textReportName>); 
                |]

    let project = 
            project.create 
                projectName 
                projectDesc
                outputDataTypes

    // --- Host Initialization ---

    /// Default count of mutants to generate for every parent sorting
    let childSortersPerParentDefault = 10<sorterCount>

    /// The primary host instance for this project.
    /// It encapsulates the DB connection and global configuration.
    let mutateSortingSetHost1 = 
        let db = new GeneSortDbMp(projectFolder) :> IGeneSortDb
        mutateSortingSetHost.Create db parameterSpans childSortersPerParentDefault


    // --- The Executor ---
    let executor (host: mutateSortingSetHost) (runParameters: runParameters) 
                 (allowOverwrite: bool<allowOverwrite>) (cts: CancellationTokenSource) 
                 (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                // 1. Setup & ID Extraction
                let! _ = checkCancellation cts.Token
                let runId = runParameters |> RunParameters.getIdString
                let repl = runParameters.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
                report progress (sprintf "%s Starting Run %s repl %d" (MathUtils.getTimestampString()) %runId %repl)

                // 2. Domain Parameter Extraction (Via Host)
                let! (sorterModelType, sortingWidth, sortableDataFormat, mutationRate) = 
                    host.ExtractDomainParams runParameters 
                    |> Result.ofOption (sprintf "Run %s, Repl %d: Missing required parameters" %runId %repl)

                // 3. Load parent SortingSet (Via Host DB resolution)
                let qpSorters = RandomSorters.makeQueryParams (Some repl) (Some sortingWidth) (Some sorterModelType) (outputDataType.SortingSet "")
                let dbParent = host.GetRandomSortersDb()
                let! loadRes = dbParent.loadAsync qpSorters
                let! sortingSetParent = loadRes |> OutputData.asSortingSet |> asAsync
                let sorterSetParent = sortingSetParent |> SortingSet.makeSorterSet

                // 4. Create sortable tests
                let! _ = checkCancellation cts.Token
                let sortableTestModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                let qpSortableTests = makeQueryParamsFromRunParams runParameters (outputDataType.SortableTest "")
                let sortableTest = SortableTestModel.makeSortableTest (%qpSortableTests.Id |> UMX.tag) sortableTestModel sortableDataFormat

                // 5-6. Evaluate parent sortings & Map results
                let qpEvalParents = makeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "Parents")
                let sorterSetEvalParent = SorterSetEval.makeSorterSetEval 
                                                (%qpEvalParents.Id |> UMX.tag) 
                                                sorterSetParent 
                                                sortableTest 
                                                host.CollectNewSortableTests

                let sortingEvalSetMap = SortingEvalSetMap.fromSortingSet sortingSetParent
                sortingEvalSetMap.UpdateManySortingEvals sorterSetEvalParent.SorterEvals

                // 7. Mutate parent sortings
                let (segments, mutantSortings, mutantSorters) = 
                    performMutation sortingSetParent sorterModelType mutationRate host.ChildSortersPerParent

                let sorterSetMutants = sorterSet.createWithNewId mutantSorters
                let qpSortingSetMutants = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "Mutants")
                let sortingSetMutant = sortingSet.create (%qpSortingSetMutants.Id |> UMX.tag) mutantSortings

                // 8-9. Evaluate mutants & Map results
                let qpEvalMutants = makeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "Mutants")
                let sorterSetEvalMutant = SorterSetEval.makeSorterSetEval 
                                                (%qpEvalMutants.Id |> UMX.tag)
                                                sorterSetMutants sortableTest 
                                                host.CollectNewSortableTests
                
                let mutationSegmentSetEvals = mutationSegmentSetEvals.create segments
                mutationSegmentSetEvals.UpdateAllSortingEvalsParent sorterSetEvalParent.SorterEvals
                mutationSegmentSetEvals.UpdateAllSortingEvalsMutant sorterSetEvalMutant.SorterEvals

                // 10. Extract the passing results
                let qpParentPass = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "Parent_Pass")
                let sortingSetParentPass = SorterSetEval.makePassingSortingSet 
                                                            (%qpParentPass.Id |> UMX.tag) 
                                                            sortingSetParent 
                                                            sorterSetEvalParent

                let qpMutantPass = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "Mutant_Pass")
                let sortingSetMutantPass = SorterSetEval.makePassingSortingSet (%qpMutantPass.Id |> UMX.tag) sortingSetMutant sorterSetEvalMutant

                // 11. Make the evaluation bins
                let qpBinsSet = makeQueryParamsFromRunParams runParameters (outputDataType.MutationSegmentEvalBinsSet "")
                let mutationSegmentEvalBinsSet = MutationSegmentEvalBinsSet.makeFromSortings (%qpBinsSet.Id |> UMX.tag) sortingSetParent.Sortings
                mutationSegmentEvalBinsSet.AddAllParentSorterEvals (mutationSegmentSetEvals.GetAllParentSorterEvals() |> Seq.toArray)
                mutationSegmentEvalBinsSet.AddAllMutantSorterEvals (mutationSegmentSetEvals.GetAllMutantSorterEvals() |> Seq.toArray)

                // 12. Save All Results (Using Batch Helper)
                let resultsToSave = [
                    qpSortableTests, (sortableTest |> outputData.SortableTest)
                    qpEvalParents, (sorterSetEvalParent |> outputData.SorterSetEval)
                    qpEvalMutants, (sorterSetEvalMutant |> outputData.SorterSetEval)
                    qpParentPass, (sortingSetParentPass |> outputData.SortingSet)
                    qpSortingSetMutants, (sortingSetMutant |> outputData.SortingSet)
                    qpMutantPass, (sortingSetMutantPass |> outputData.SortingSet)
                    qpBinsSet, (mutationSegmentEvalBinsSet |> outputData.MutationSegmentEvalBinsSet)
                ]
                let! _ = saveResults host.ProjectDb allowOverwrite resultsToSave

                return runParameters.WithRunFinished (Some true)

            with e ->
                let msg = sprintf "Execution exception in Run %s: %s" (runParameters |> RunParameters.getIdString) e.Message
                return! async { return Error msg }
        }