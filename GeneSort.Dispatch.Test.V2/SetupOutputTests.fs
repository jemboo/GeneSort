module GeneSort.Dispatch.Test.V2.SetupOutputTests

open System
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.SetupParameters

let private makeRun spans query enhancer =
    // Execution and run-only catalogs need not exist to predict setup outputs.
    SimpleRun.create {
        DatabaseName = (UMX.tag "Database")
        ProjectName = (UMX.tag "Project")
        RunName = (UMX.tag "Run")
        Description = "Setup output test"
        AllowOverwrite = false
        Execution = {
            FunctionName = "unused.execute"
            ParameterSpans = ["RunOnly", ["unused"]]
            QueryCatalogName = "unused.run-query"
            EnhancerFunctionName = "unused.run-enhancer"
        }
        Setup = {
            FunctionName = "unused.setup-function"
            ParameterSpans = spans
            QueryCatalogName = query
            EnhancerFunctionName = enhancer
        }
    }

let private unwrap = function Ok value -> value | Error error -> failwith error

[<Fact>]
let ``setup prediction uses the setup registry when run and setup names match`` () =
    let name = Guid.NewGuid().ToString("N")
    QueryParamsCatalogRun.register name (fun _ _ _ _ -> failwith "Run builder must not be used for setup")
    QueryParamsCatalogSetup.register name (fun project database sp outputType ->
        Some (queryParams.create database project (sp.GetRepl()) None outputType [|"Source", "setup"|]))
    let configured = makeRun [] name SetupEnhancerFunctionNames.noOp
    let outputType = geneSortDataType.SorterModelSet ""
    let ids = SetupOutputs.getIds configured 0<replNumber> outputType |> unwrap
    let expected = queryParams.create (UMX.tag "Database") (UMX.tag "Project") (Some 0<replNumber>) None outputType [|"Source", "setup"|]
    Assert.Equal<Guid<queryParamsId>>([|expected.Id|], ids)

[<Fact>]
let ``IDs use every enhanced setup combination for exactly the requested replica and output type`` () =
    let name = Guid.NewGuid().ToString("N")
    let mutable enhanced = 0
    SetupEnhancerCatalog.register name (fun context sp ->
        Assert.Equal("Run", %context.Run.RunName)
        enhanced <- enhanced + 1
        sp.WithGenerationCurrent(Some (UMX.tag 12)))
    QueryParamsCatalogSetup.register name (fun project database sp outputType ->
        Assert.False(sp.ParamMap.ContainsKey "RunOnly")
        Some (queryParams.create database project (sp.GetRepl()) (sp.GetGenerationCurrent()) outputType
            [|"A", sp.ParamMap.["A"]; "B", sp.ParamMap.["B"]|]))
    let configured = makeRun ["A", ["a"; "b"]; "B", ["1"; "2"]] name name
    let outputType = geneSortDataType.SorterModelSet "models"
    let ids = SetupOutputs.getIds configured 7<replNumber> outputType |> unwrap
    let expected =
        [| for a in ["a"; "b"] do
               for b in ["1"; "2"] do
                   yield (queryParams.create (UMX.tag "Database") (UMX.tag "Project")
                       (Some 7<replNumber>) (Some (UMX.tag 12)) outputType [|"A", a; "B", b|]).Id |]
    Assert.Equal(4, enhanced)
    Assert.Equal<Guid<queryParamsId>>(expected, ids)

[<Fact>]
let ``duplicate IDs are preserved for different parameter combinations`` () =
    let name = Guid.NewGuid().ToString("N")
    QueryParamsCatalogSetup.register name (fun project database sp outputType ->
        Some (queryParams.create database project (sp.GetRepl()) None outputType [||]))
    let configured = makeRun ["IgnoredByQuery", ["a"; "b"]] name SetupEnhancerFunctionNames.noOp
    let ids = SetupOutputs.getIds configured 0<replNumber> (geneSortDataType.SortableTests "") |> unwrap
    Assert.Equal(2, ids.Length)
    Assert.Equal(ids.[0], ids.[1])

[<Fact>]
let ``missing query returns an error instead of silently dropping a combination`` () =
    let name = Guid.NewGuid().ToString("N")
    QueryParamsCatalogSetup.register name (fun project database sp outputType ->
        if sp.ParamMap.["A"] = "missing" then None
        else Some (queryParams.create database project (sp.GetRepl()) None outputType [||]))
    let configured = makeRun ["A", ["ok"; "missing"]] name SetupEnhancerFunctionNames.noOp
    match SetupOutputs.getIds configured 3<replNumber> (geneSortDataType.SorterModelSet "") with
    | Ok _ -> failwith "Expected incomplete query generation to fail"
    | Error message ->
        Assert.Contains(name, message)
        Assert.Contains("combination 1", message)
        Assert.Contains("missing", message)

[<Fact>]
let ``empty spans produce one ID while an empty span produces none`` () =
    let name = Guid.NewGuid().ToString("N")
    QueryParamsCatalogSetup.register name (fun project database sp outputType ->
        Some (queryParams.create database project (sp.GetRepl()) None outputType [||]))
    let outputType = geneSortDataType.SortableTests ""
    let ids = SetupOutputs.getIds (makeRun [] name SetupEnhancerFunctionNames.noOp) 0<replNumber> outputType |> unwrap
    Assert.Single(ids) |> ignore
    let empty = SetupOutputs.getIds (makeRun ["A", []] name SetupEnhancerFunctionNames.noOp) 0<replNumber> outputType |> unwrap
    Assert.Empty(empty)
