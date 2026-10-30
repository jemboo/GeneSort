namespace GeneSort.Project.V1.Catalogs.SetupParameters

open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters

type setupParametersPipelineContext = {
    Run: run
    QueryParamsFromSetupParams: setupQueryParamsBuilder
}

type setupParametersTransform = setupParametersPipelineContext -> setupParameters -> setupParameters

type setupParametersPipeline = parameterPipeline<setupParametersPipelineContext, setupParameters>

