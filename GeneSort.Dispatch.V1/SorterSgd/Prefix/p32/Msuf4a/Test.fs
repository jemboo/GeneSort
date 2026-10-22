namespace GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1

module Test =

    module Standard =

        let Test ()  : run =
            let names = RunNaming.create (System.Reflection.MethodBase.GetCurrentMethod())
            let parameterSpans =
                [
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                    (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                    (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
                    (ParamKeys.mutationModKey, [0 .. 7] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020;] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035;]  |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
                ]
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Rate comp for 32p4a Msuf4"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
                        QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                        EnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
                    }
                    Execution = {
                        FunctionName = ExecuteFunctionNames.standard
                        ParameterSpans = []
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = "uniformInterval10"
                    GenSaveSubIntervalsName = "uniformInterval2"
                }
            )