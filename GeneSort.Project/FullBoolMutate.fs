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


type fullBoolMutateHost = 
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

    /// Encapsulates the specific DB logic for loading parent sorters from the RandomSorters project
    member this.GetRandomSortersDb() = 
        new GeneSortDbMp(RandomSorters.projectFolder) :> IGeneSortDb

    /// Centralized parameter extraction for the mutation pipeline
    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! smt = rp.GetSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! sdt = rp.GetSortableDataFormat()
            let! mr = rp.GetMutationRate()
            return (smt, sw, sdt, mr)
        }



module FullBoolMutate =
    // --- Project Constants ---
    let projectName = "FullBoolMutate" |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\FullBoolMutate\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "FullBoolMutate on RandomSorters with SortingWidth from 4 to 24"
    let rngFactory = rngFactory.LcgFactory
    let firstSortingIndex = 0<sorterCount>

    // --- Query Parameter Helpers ---
    let makeQueryParams (repl: int<replNumber> option) (sortingWidth:int<sortingWidth> option)
                        (sorterModelType:sorterModelType option) (sortableDataFormat:sortableDataFormat option)
                        (outputDataType: outputDataType) : queryParams =
        queryParams.create (Some projectName) repl outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> UmxExt.intOptionToString ); 
               (runParameters.sorterModelTypeKey, sorterModelType |> Option.map SorterModelType.toString |> UmxExt.stringOptionToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat |> Option.map SortableDataFormat.toString |> UmxExt.stringOptionToString ); |]

    let makeQueryParamsFromRunParams (runParams: runParameters) (outputDataType: outputDataType) =
        makeQueryParams (runParams.GetRepl()) (runParams.GetSortingWidth())
                        (runParams.GetSorterModelType()) (runParams.GetSortableDataFormat()) outputDataType

    // --- Filtering & Refinement ---
    let private sorterModelTypeForSortingWidth (rp: runParameters) =
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

    let enhancer (rp : runParameters) : runParameters =
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        rp.WithProjectName(Some projectName).WithRunFinished(Some false).WithId (Some qp.Id)

    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose sorterModelTypeForSortingWidth 
        |> Seq.map enhancer
        |> Seq.toArray |> Array.toSeq

    // --- Internal Logic Helpers ---
    let private performMutation (parent: sortingSet) (smt: sorterModelType) (rate: float<mutationRate>) (childCount: int<sorterCount>) =
        let mutator = SorterModelMutateParams.makeUniformMutatorForSorterModel 
                        smt parent.StageLength parent.SortingWidth rate rngFactory
        let segments = parent.Sortings |> Array.map (fun sm ->
            SorterMutateParamsOps.makeSortingMutationSegmentFromSorting sm mutator firstSortingIndex childCount)
        let mutantSortings = segments |> Array.collect (fun s -> s.MakeMutantSortings)
        let mutantSorters = segments |> Array.collect (fun s -> s.MakeMutantSorters)
        (segments, mutantSortings, mutantSorters)

    let private saveResults (db: IGeneSortDb) allowOverwrite (results: (queryParams * outputData) list) =
        asyncResult {
            for (qp, data) in results do
                let! (_: unit) = db.saveAsync qp data allowOverwrite
                ()
            return ()
        }

    // --- Configuration Spans ---
    let parameterSpans = [
        (runParameters.sortingWidthKey, [ "16" ])
        (runParameters.sorterModelTypeKey, [ sorterModelType.Msce ] |> List.map SorterModelType.toString)
        (runParameters.sortableDataFormatKey, [ sortableDataFormat.BitVector512 ] |> List.map SortableDataFormat.toString)
        (runParameters.mutationRateKey, [ "0.05" ])
    ]

    let outputDataTypes = [|
        outputDataType.RunParameters; outputDataType.SorterSetEval "";
        outputDataType.TextReport ("Bins" |> UMX.tag); outputDataType.TextReport ("Profiles" |> UMX.tag)
    |]

    // --- Host Initialization ---
    let fullBoolMutateHost1 = 
        let db = new GeneSortDbMp(projectFolder) :> IGeneSortDb
        fullBoolMutateHost.Create db parameterSpans 100<sorterCount>

    // --- The Executor ---
    let executor (host: fullBoolMutateHost) (runParameters: runParameters) 
                 (allowOverwrite: bool<allowOverwrite>) (cts: CancellationTokenSource) 
                 (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = runParameters |> RunParameters.getIdString
                let repl = runParameters.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
                report progress (sprintf "%s Starting FullBoolMutate Run %s" (MathUtils.getTimestampString()) %runId)

                // 1. Extract Parameters via Host
                let! (smt, sw, sdt, mr) = host.ExtractDomainParams runParameters 
                                            |> Result.ofOption (sprintf "Run %s: Missing params" %runId)

                // 2. Load Parent Data
                let qpParent = RandomSorters.makeQueryParams (Some repl) (Some sw) (Some smt) (outputDataType.SortingSet "")
                let! loadRes = host.GetRandomSortersDb().loadAsync qpParent
                let! sortingSetParent = loadRes |> OutputData.asSortingSet |> asAsync
                let sorterSetParent = sortingSetParent |> SortingSet.makeSorterSet

                // 3. Setup Tests & Initial Eval
                let sortableTestModel = msasF.create sw |> sortableTestModel.MsasF
                let qpTests = makeQueryParamsFromRunParams runParameters (outputDataType.SortableTest "")
                let sortableTests = SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) sortableTestModel sdt

                let qpEvalParents = makeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "Parents")
                let evalParents = SorterSetEval.makeSorterSetEval (%qpEvalParents.Id |> UMX.tag) sorterSetParent sortableTests host.CollectNewSortableTests

                // 4. Mutation & Mutant Eval
                let (segments, mutantSortings, mutantSorters) = performMutation sortingSetParent smt mr host.ChildSortersPerParent
                let sorterSetMutants = sorterSet.createWithNewId mutantSorters
                let sortingSetMutant = sortingSet.create (Guid.NewGuid() |> UMX.tag) mutantSortings // Internal ID
                
                let qpEvalMutants = makeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "Mutants")
                let evalMutants = SorterSetEval.makeSorterSetEval (%qpEvalMutants.Id |> UMX.tag) sorterSetMutants sortableTests host.CollectNewSortableTests

                // 5. Mapping & Binning
                let segmentEvals = mutationSegmentSetEvals.create segments
                segmentEvals.UpdateAllSortingEvalsParent evalParents.SorterEvals
                segmentEvals.UpdateAllSortingEvalsMutant evalMutants.SorterEvals

                let qpBinsSet = makeQueryParamsFromRunParams runParameters (outputDataType.MutationSegmentEvalBinsSet "")
                let binsSet = MutationSegmentEvalBinsSet.makeFromSortings (%qpBinsSet.Id |> UMX.tag) sortingSetParent.Sortings
                binsSet.AddAllParentSorterEvals (segmentEvals.GetAllParentSorterEvals() |> Seq.toArray)
                binsSet.AddAllMutantSorterEvals (segmentEvals.GetAllMutantSorterEvals() |> Seq.toArray)

                // 6. Persistence
                let qpMutantSet = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "Mutants")
                let results = [
                    qpTests, (sortableTests |> outputData.SortableTest)
                    qpEvalParents, (evalParents |> outputData.SorterSetEval)
                    qpEvalMutants, (evalMutants |> outputData.SorterSetEval)
                    qpMutantSet, (sortingSetMutant |> outputData.SortingSet)
                    qpBinsSet, (binsSet |> outputData.MutationSegmentEvalBinsSet)
                ]
                let! (_: unit) = saveResults host.ProjectDb allowOverwrite results

                return runParameters.WithRunFinished (Some true)

            with (e: exn)  -> 
                return! Error (sprintf "Error in %s: %s" (runParameters |> RunParameters.getIdString) e.Message) |> async.Return
        }