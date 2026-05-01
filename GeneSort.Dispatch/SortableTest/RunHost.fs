namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1



type runHostSpec = {
    ProjectName: string<projectName>
    RunName: string<runName>
    ProjectDesc: string
    DataFolder: string
    Spans: (string * string list) list
    // Logic Callbacks
    Filter: runParameters -> runParameters option
    Enhancer: IRunHost -> runParameters -> runParameters
    // Domain Settings
    RngFactory: rngFactory
    CollectNewSortableTests: bool
    AllowOverwrite: bool<allowOverwrite>

}

type runHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runHostSpec
        _project: run
    }
    
    static member Create db spec project =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _project = project }

    member this.Spec = this._spec

    member this.MakeQueryParams 
                (repl: int<replNumber> option) 
                (sortingWidth: int<sortingWidth> option)
                (mergeDimension: int<mergeDimension> option) 
                (mergeFillType: mergeSuffixType option)
                (sortableDataFormat: sortableDataFormat option) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            (Some this._spec.ProjectName) 
            repl 
            outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeFillType 
                    |> Option.map MergeSuffixType.toString |> UmxExt.stringToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat 
                    |> Option.map SortableDataFormat.toString |> UmxExt.stringToString ); |]


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

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.Filter >> Option.map (this._spec.Enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.ProjectDb = this._projectDb
        member this.Project = this._project
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.AllowOverwrite
        member this.MakeQueryParamsFromRunParams rp odt = this.MakeQueryParamsFromRunParams rp odt
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps
        member this.Executor runParameters allowOverwrite cts progress = 
            Executor.makeSortableTest 
                this
                this._spec.CollectNewSortableTests 
                runParameters 
                allowOverwrite 
                cts 
                progress

