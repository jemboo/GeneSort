module GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs.Simple.OrthoPara

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs.Simple.Common




module OrthoPara32 =

    let channelCount = GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.channelCount
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters = GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs.Simple.SimpleParams.simpleSetupParams


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.msrs24p3bOrthoPara
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsrsOrthoPara
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsrsOrthoPara

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let NoMods ()  : run =
        let names = RunNaming.create()
        let parameterSpans =
            GeneSort.Dispatch.V1.SorterSgd.cc24.p4b.Msrs.Simple.SimpleParams.orthoParaExecutionDefaults
            @ [
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [5] |> List.map string)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "OrthroPara rate comp for 24pfx3b Msrs, NoMods"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Compare ortho and para mutation settings for 24-input Msrs3b evolution (OrthoPara32)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.cc24.p4b.Msrs.Simple.OrthoPara.OrthoPara32", description, channelCount, executionQueryCatalogName, [|
            runExample("NoMods", NoMods)
        |])
