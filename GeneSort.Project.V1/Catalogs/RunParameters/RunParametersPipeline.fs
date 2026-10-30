namespace GeneSort.Project.V1.Catalogs.RunParameters

open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters

/// Context available to a run parameter pipeline without depending on dispatch infrastructure.
type runParametersPipelineContext =
    private {
        queryParamsFromRunParams: runQueryParamsBuilder
        run: run
        setupParameters: setupParameters option
    }

    static member create
            (queryParamsFromRunParams: runQueryParamsBuilder)
            (run: run)
            : runParametersPipelineContext =
        {
            queryParamsFromRunParams = queryParamsFromRunParams
            run = run
            setupParameters = None
        }

    static member createForSetup query run setup =
        { queryParamsFromRunParams = query; run = run; setupParameters = Some setup }

    member this.QueryParamsFromRunParams with get() = this.queryParamsFromRunParams
    member this.Run with get() = this.run
    member this.SetupParameters = this.setupParameters


type runParametersTransform = runParametersPipelineContext -> runParameters -> runParameters

type runParametersPipeline = parameterPipeline<runParametersPipelineContext, runParameters>

