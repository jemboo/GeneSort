namespace GeneSort.Project.V1.Workflows

open System
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup

type catalogChoice =
    private { role: string; name: string; description: string }
    member this.Role = this.role
    member this.Name = this.name
    member this.Description = this.description
    static member create role name description =
        if String.IsNullOrWhiteSpace name then invalidArg "name" "A catalog choice requires a name."
        { role = role; name = name; description = description }

type workflowDefinitionArgs = {
    Name: string
    Description: string
    SetupFunction: catalogChoice
    SetupQuery: catalogChoice
    SetupParametersEnhancer: catalogChoice
    ExecutionFunction: catalogChoice
    ExecutionQuery: catalogChoice
    RunParametersEnhancer: catalogChoice
    TestsMaker: catalogChoice
    SeedMaker: catalogChoice
    Mutator: catalogChoice
}

/// A compatible catalog combination. Parameter values and run identity remain in the run configuration.
type workflowDefinition =
    private { definition: workflowDefinitionArgs }
    static member create args = { definition = args }
    member this.Name = this.definition.Name
    member this.Description = this.definition.Description
    member this.CatalogChoices =
        let d = this.definition
        [| d.SetupFunction; d.SetupQuery; d.SetupParametersEnhancer
           d.ExecutionFunction; d.ExecutionQuery; d.RunParametersEnhancer
           d.TestsMaker; d.SeedMaker; d.Mutator |]
    member this.MakerNames =
        evolutionMakerNames.create {
            SortableTests = this.definition.TestsMaker.Name
            SeedPoolSet = this.definition.SeedMaker.Name
            Mutator = this.definition.Mutator.Name
        }
    member this.SetupConfig testsDatabase evalsDatabase spans : sgdSetupConfig = {
        FunctionName = this.definition.SetupFunction.Name
        SortableTestsModule = testsDatabase
        SorterEvalModule = evalsDatabase
        QueryCatalogName = this.definition.SetupQuery.Name
        ParametersEnhancerName = this.definition.SetupParametersEnhancer.Name
        ParameterSpans = spans
    }
    member this.ExecutionConfig spans : executionConfig = {
        FunctionName = this.definition.ExecutionFunction.Name
        QueryCatalogName = this.definition.ExecutionQuery.Name
        ParametersEnhancerName = this.definition.RunParametersEnhancer.Name
        ParameterSpans = spans
    }
    member this.Matches(configured: run) =
        let d = this.definition
        configured.SetupQueryCatalogName = d.SetupQuery.Name
        && configured.SetupFunctionName = Some d.SetupFunction.Name
        && configured.SetupParametersEnhancerName = d.SetupParametersEnhancer.Name
        && configured.ExecuteFunctionName = d.ExecutionFunction.Name
        && configured.RunQueryCatalogName = d.ExecutionQuery.Name
        && configured.RunParametersEnhancerName = d.RunParametersEnhancer.Name
        && configured.MakerNames = Some this.MakerNames

module WorkflowDefinitions =
    let standardMsuf32 = workflowDefinition.create {
        Name = "Standard · Msuf32p4a"
        Description = "Simple sorter models, width 32, prefix p4a, Standard evolution."
        SetupFunction = catalogChoice.create "Setup.FunctionName" SetupCatalogNames.sgdStandard "Prepare Standard evolution inputs."
        SetupQuery = catalogChoice.create "Setup query" QueryCatalogNamesSetup.sorterSgdMsufStandard "Identify Standard Msuf setup inputs."
        SetupParametersEnhancer = catalogChoice.create "Setup parameters" SetupParametersEnhancerNames.standardMsuf32 "Supply width 32, prefix p4a and initial pool settings."
        ExecutionFunction = catalogChoice.create "Execution" ExecuteFunctionNames.standard "Evolve Standard sorter pools."
        ExecutionQuery = catalogChoice.create "Execution query" QueryCatalogNamesRun.sorterSgdMsufStandard "Identify Standard Msuf execution results."
        RunParametersEnhancer = catalogChoice.create "Execution parameters" RunParametersEnhancerNames.evoMsuf32 "Inherit setup parameters and apply execution overrides."
        TestsMaker = catalogChoice.create "Tests maker" EvolutionMakerNames.prefix.SortableTests "Load sortable tests with the selected prefix."
        SeedMaker = catalogChoice.create "Seed maker" EvolutionMakerNames.prefix.SeedPoolSet "Create Standard pools from the prefix seed library."
        Mutator = catalogChoice.create "Mutator" EvolutionMakerNames.prefix.Mutator "Mutate the underlying simple sorter model."
    }
