namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open GeneSort.Sorting.Sortable
open GeneSort.FileDb.V1


type runHostForMergeTest = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runHostSpec
        _run: run
    }
    
    static member Create db spec run =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _run = run }

    member this.Spec = this._spec

    member this.MakeQueryParams 
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeFillType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            this._spec.ProjectName
            repl 
            outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeFillType 
                    |> Option.map MergeSuffixType.toString |> UmxExt.stringOptionToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat 
                    |> Option.map SortableDataFormat.toString |> UmxExt.stringOptionToString ); |]


    member this.MakeQueryParamsFromRunParams 
                (runParams: runParameters) 
                (outputDataType: outputDataType) : queryParams =
            this.MakeQueryParams 
                    (runParams.GetRepl()) 
                    (runParams.GetSortingWidth()) 
                    (runParams.GetMergeDimension())
                    (runParams.GetMergeSuffixType()) 
                    (runParams.GetSortableDataFormat()) 
                    outputDataType


    member this.getSortableMergeTest 
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeFillType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                    : Async<Result<sortableTest, string>> =
            asyncResult {
                try
                    let queryParams = this.MakeQueryParams 
                                                repl 
                                                sortingWidth 
                                                mergeDimension 
                                                mergeFillType 
                                                sortableDataFormat 
                                                (outputDataType.SortableTest "")

                    let! (dataOut : outputData) = this._projectDb.loadAsync queryParams
                    let! sortableTest = dataOut |> OutputData.asSortableTest
                    return sortableTest
                with e -> 
                    return! Error (sprintf "Error in getSortableMergeTest: %s" e.Message) |> async.Return
            }


    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.Filter >> Option.map (this._spec.Enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.ProjectDb = this._projectDb
        member this.Run = this._run
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.AllowOverwrite
        member this.MakeQueryParamsFromRunParams rp odt = this.MakeQueryParamsFromRunParams rp odt
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps



module RunHostForMergeTest =

    let createRunHost (spec: runHostSpec) : IRunHost =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let run = run.create spec.ProjectName spec.RunName spec.RunDescription
        runHost.Create db spec run :> IRunHost
