module GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32.Msuf4a.SimpleS

open System.Runtime.CompilerServices
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1

type private runOptions = {
    Soss: bool
    LastGenerationInterval: int
    SetupParameterSpans: (string * string list) list
    ExecutionParameterSpans: (string * string list) list
}

let private create (names: runNames) (options: runOptions) : run =
    let evolutionParameters = [
        ParamKeys.codeModKey, ["NoMods"]
        ParamKeys.paraRateKey, [1.15] |> List.map string
        ParamKeys.selfSymRateKey, [1.75] |> List.map string
        ParamKeys.seedModificationRateKey, [0.020] |> List.map string
        ParamKeys.modificationRateKey, [0.035] |> List.map string
        ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString
    ]
    SgdRun.create {
        ProjectName = names.ProjectName
        DatabaseName = names.DatabaseName
        RunName = names.RunName
        Description = "SimpleS Msuf4, width 32, four-stage PrefixA; one pool of 32 sorters"
        AllowOverwrite = false
        Setup = {
            FunctionName = SetupCatalogNames.standard
            ParameterSpans = [
                ParamKeys.sorterCountPerPoolKey, ["32"]
                ParamKeys.sorterCountPerPoolSetKey, ["256"]
                ParamKeys.mutationModKey, [0 .. 1] |> List.map string
            ] @ (if options.Soss then [] else evolutionParameters) @ options.SetupParameterSpans
            QueryCatalogName = if options.Soss then QueryCatalogNamesSetup.sossMsuf32Seed else QueryCatalogNamesSetup.sorterSgdMsufStandard
            EnhancerFunctionName = if options.Soss then SetupEnhancerFunctionNames.sossMsuf32S else SetupEnhancerFunctionNames.standardMsuf32S
        }
        Execution = {
            FunctionName = if options.Soss then ExecuteFunctionNames.soss else ExecuteFunctionNames.standard
            ParameterSpans = [
                ParamKeys.generationIntervalLastKey, [string options.LastGenerationInterval]
            ] @ (if options.Soss then evolutionParameters @ [
                    ParamKeys.seedSossKey, [1234UL] |> List.map string
                    ParamKeys.partitionCountKey, ["2"]
                    ParamKeys.generationsPerSubsetKey, ["10"]
                    ParamKeys.generationsPerMergedSetKey, ["20"]
                ] else [ParamKeys.adjustCeLengthFrequencyKey, ["10"]])
              @ options.ExecutionParameterSpans
            QueryCatalogName = if options.Soss then QueryCatalogNamesRun.sorterSgdMsufSoss else QueryCatalogNamesRun.sorterSgdMsufStandard
            EnhancerFunctionName = if options.Soss then RunParamBuilderNames.Enhancer.sossMsuf32S else RunParamBuilderNames.Enhancer.evoMsuf32S
        }
    } {
        MakerNames = if options.Soss then EvolutionMakerNames.sossPrefix else EvolutionMakerNames.prefix
        GenSaveIntervalsName = GenIntervalNames.uniformInterval100
        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
    } |> run.SgdRun


module Standard =

    // Uses the catalog defaults: ActivationProbability = 0.5 and ActivatorMutationRate = 0.1.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () =
        create (RunNaming.create()) {
            Soss = false
            LastGenerationInterval = 0
            SetupParameterSpans = []
            ExecutionParameterSpans = []
        }

    // 100 generations; 3 activation probabilities x 3 activator rates x 4 mutation variants per replica.
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () =
        create (RunNaming.create()) {
            Soss = false
            LastGenerationInterval = 19
            SetupParameterSpans = [ParamKeys.activationProbabilityKey, [0.70; 0.80; 0.90; 0.99] |> List.map string]
            ExecutionParameterSpans = [ParamKeys.activatorMutationRateKey, [0.0; 0.03; 0.06; 0.09] |> List.map string]
        }

module Soss =

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () =
        create (RunNaming.create()) {
            Soss = true
            LastGenerationInterval = 0
            SetupParameterSpans = []
            ExecutionParameterSpans = []
        }

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ActivatorRates () =
        create (RunNaming.create()) {
            Soss = true
            LastGenerationInterval = 9
            SetupParameterSpans = [ParamKeys.activationProbabilityKey, [0.0; 0.25; 0.5; 0.75] |> List.map string]
            ExecutionParameterSpans = [ParamKeys.activatorMutationRateKey, [0.0; 0.05; 0.1; 0.2] |> List.map string]
        }
