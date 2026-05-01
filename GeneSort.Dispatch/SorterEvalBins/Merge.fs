namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1



type runSpecMerge = {
    ProjectName: string<projectName>
    RunName: string<runName>
    ProjectDesc: string
    DataFolder: string
    Spans: (string * string list) list
    // Logic Callbacks
    GetStageLength: simpleSorterModelType -> int<sortingWidth> -> int<stageLength>
    Filter: runParameters -> runParameters option
    Enhancer: IRunHost -> runParameters -> runParameters
    // Merge Specifics
    ExcludeSelfCe: bool
    CollectNewSortableTests: bool
    SamplesPerBin: int
    AllowOverwrite: bool<allowOverwrite>
    MergeTestsProjectFolder: string
}


// --- 2. Merge Host Implementation ---

type randomSorterBinsMergeHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runSpecMerge
        _project: run
    }
    
    static member Create db spec project =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _project = project }

    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! smk = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! sl = rp.GetStageLength()
            let! cl = rp.GetCeLength()
            let! sc = rp.GetSorterCount()
            let! sdt = rp.GetSortableDataFormat()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
            return (smk, sw, sl, cl, sc, sdt, md, mst)
        }

    member this.MakeQueryParams (repl: int<replNumber> option) (sw: int<sortingWidth> option) 
                                (md: int<mergeDimension> option) (mst: mergeSuffixType option) 
                                (smt: simpleSorterModelType option) (odt: outputDataType) : queryParams =
        queryParams.create (Some this._spec.ProjectName) repl odt
            [| (runParameters.sortingWidthKey, sw |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, md |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mst |> Option.map MergeSuffixType.toString |> UmxExt.stringToString);
               (runParameters.simpleSorterModelTypeKey, smt |>  Option.map SimpleSorterModelType.toString |> UmxExt.stringToString) |]

    interface IRunHost with
        member this.ProjectDb = this._projectDb
        member this.Project = this._project
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.AllowOverwrite
        
        member this.MakeQueryParamsFromRunParams rp odt = 
            this.MakeQueryParams (rp.GetRepl()) (rp.GetSortingWidth()) (rp.GetMergeDimension()) (rp.GetMergeSuffixType()) (rp.GetSimpleSorterModelType()) odt
        
        member this.ParamMapRefiner rps = 
            rps |> Seq.choose (this._spec.Filter >> Option.map (this._spec.Enhancer (this :> IRunHost)))
            
        member this.Executor rp ow cts prog = 
            asyncResult {
                try
                    let! (_: unit) = checkCancellation cts.Token
                    let! (smt, sw, sl, cl, sc, sdt, md, mst) = this.ExtractDomainParams rp |> Result.ofOption "Missing parameters"
                    let repl = rp.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
                    
                    // 1. Sorter Generation
                    let rng = rngFactory.LcgFactory
                    //let gen = 
                    //    match smt with
                    //    | simpleSorterModelType.Msce -> msceRandGen.create rng sw this._spec.ExcludeSelfCe cl |> sorterModelGen.SmmMsceRandGen
                    //    | simpleSorterModelType.Mssi -> mssiRandGen.create rng sw sl |> sorterModelGen.SmmMssiRandGen
                    //    | simpleSorterModelType.Msrs -> msrsRandGen.create rng sw (OpsGenRatesArray.createUniform %sl) |> sorterModelGen.SmmMsrsRandGen
                    //    | simpleSorterModelType.Msuf4 -> msuf4RandGen.create rng sw sl (Uf4GenRatesArray.createUniform %sl %sw) |> sorterModelGen.SmmMsuf4RandGen
                    //    | _ -> failwith "Unsupported model type"

                    //let firstIdx = (%repl * %sc) |> UMX.tag<sorterCount>
                    //let genSeg = sortingGenSegment.create (gen |> sortingGen.Single) firstIdx sc
                    //let qpFullSet = (this :> IRunHost).MakeQueryParamsFromRunParams rp (outputDataType.SortingSet "") 
                    //let fullSortingSet = genSeg.MakeSortingSet (%qpFullSet.Id |> UMX.tag)
                    //let fullSorterSet = fullSortingSet |> SortingSet.makeSorterSet

                    //// 2. Load Merge Tests
                    //let qpTests = this.MakeQueryParams (Some (0 |> UMX.tag)) (Some sw) (Some md) (Some mst) None (outputDataType.SortableTest "")
                    //let dbMergeTests = new GeneSortDbMp(this._spec.MergeTestsProjectFolder |> UMX.tag) :> IGeneSortDb
                    //let! rawTestData = dbMergeTests.loadAsync qpTests 
                    //let! tests = rawTestData |> OutputData.asSortableTest

                    //// 3. Evaluate & Bin
                    //let qpEval = (this :> IRunHost).MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                    //let eval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests this._spec.CollectNewSortableTests

                    //let qpBins = (this :> IRunHost).MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                    //let bins = sorterEvalBins.create (%qpBins.Id |> UMX.tag) eval.SorterEvals

                    //// 4. Sample & Save
                    //let qpEven = (this :> IRunHost).MakeQueryParamsFromRunParams rp (outputDataType.SortingSet "EvenSampled")
                    //let evenSet = sortingSet.create (%qpEven.Id |> UMX.tag) (SortingSetFilter.sampleBinsEvenly this._spec.SamplesPerBin bins fullSortingSet)

                    //let! (_: unit) = this._projectDb.saveAsync qpBins (bins |> outputData.SorterEvalBins) ow
                    //let! (_: unit) = this._projectDb.saveAsync qpEven (evenSet |> outputData.SortingSet) ow

                    return rp.WithRunFinished (Some true)
                with e -> 
                    return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
            }

// --- 3. Logic Module: Merge ---

module RandomSorterBinsMerge =

    let getStageLength (smt: simpleSorterModelType) (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 16 -> 100
        | 32 -> match smt with | Msuf4 -> 600 | _ -> 300
        | 48 -> match smt with | Msuf4 -> 1200 | _ -> 400
        | 64 -> match smt with | Msuf4 -> 2000 | _ -> 600
        | 96 -> match smt with | Msuf4 -> 2800 | _ -> 800
        | 128 -> match smt with | Msuf4 -> 4000 | _ -> 1200
        | 196 -> match smt with | Msuf4 -> 4800 | _ -> 2000
        | 256 -> match smt with | Msuf4 -> 6000 | _ -> 3000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag

    let private mergeEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let smt = rp.GetSimpleSorterModelType().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Project.RunName)
        let sl = getStageLength smt sw
        let cl = sl |> StageLength.toCeLength sw

        rp.WithProjectName(Some host.Project.ProjectName)
          .WithRunName(Some host.Project.RunName)
          .WithRunFinished(Some false)
          .WithCeLength(Some cl)
          .WithStageLength(Some sl)
          .WithId (Some qp.Id)

    let private paramMapFilter (rp: runParameters) = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
        
            let has2factor = (%sw % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            let isMuf6able = (%sw % 3 = 0) && (MathUtils.isAPowerOfTwo (%sw / 3))

            // We bind to unit just to enforce the filter
            let! _ = 
                match smt with
                | simpleSorterModelType.Msce -> Some ()
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> 
                    if has2factor then Some () else None
                | simpleSorterModelType.Msuf4 -> 
                    if isMuf4able then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 64, return None to stop
            if (mst.IsNoSuffix && %sw > 64) then return! None
        
            return rp
        }

    module Specs =
        let P1 = {
            ProjectName = "RandomMergeSorterBins" |> UMX.tag
            RunName = "Merge_P1" |> UMX.tag
            ProjectDesc = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            DataFolder = "c:\\Projects\\RandomMergeSorterBins\\Data"
            MergeTestsProjectFolder = "c:\\Projects\\SortableMergeTests\\Data"
            Spans = [
                (runParameters.sortingWidthKey, [32; 64] |> List.map string)
                (runParameters.simpleSorterModelTypeKey, [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)
                (runParameters.mergeDimensionKey, [2; 4] |> List.map string)
                (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)
                (runParameters.sorterCountKey, ["1000"])
            ]
            GetStageLength = getStageLength
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            ExcludeSelfCe = true
            CollectNewSortableTests = false
            SamplesPerBin = 1
            AllowOverwrite = false |> UMX.tag
        }

    let Configs = Map.ofList [ ("P1", Specs.P1) ]

    let CreateHost (spec: runSpecMerge) =
        let db = new GeneSortDbMp(spec.DataFolder |> UMX.tag) :> IGeneSortDb
        let proj = run.create spec.ProjectName spec.RunName spec.ProjectDesc 
                                [| outputDataType.RunParameters spec.RunName; outputDataType.SorterEvalBins ""; outputDataType.SortingSet "EvenSampled" |]
        randomSorterBinsMergeHost.Create db spec proj :> IRunHost