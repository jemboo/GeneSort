module GeneSort.Dispatch.Test.V2.MakerCatalogTests

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs
open System
open Xunit
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SorterSgd
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Dispatch.V1
open GeneSort.Project.Mp.V1
open GeneSort.Core.Mp
open MessagePack
open GeneSort.Core
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss

let private emptyParameters = runParameters.create Map.empty

let private context : evolutionMakerContext =
    evolutionMakerContext.create
        (fun _ _ _ -> async { return Error "merge tests loader" })
        (fun _ _ _ -> async { return Error "prefix tests loader" })
        (fun _ _ _ -> async { return Error "standard eval loader" })
        (fun _ _ _ -> async { return Error "merge eval loader" })
        (fun _ _ _ -> async { return Error "prefix eval loader" })

let private errorOf result =
    match result with
    | Error message -> message
    | Ok _ -> failwith "Expected missing run parameters to fail."

let private makeRun names (projectName: string) (runName: string) =
    SgdRun.create names (UMX.tag "CatalogTests") (UMX.tag projectName) (UMX.tag runName)
        "Catalog configuration test" [] "expInterval100_L50ss" "summaryInterval_C.1p5C"
        QueryCatalogNames.sorterSgdMsufStandard GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.standard RunParamBuilderNames.Filter.identity
        RunParamBuilderNames.Enhancer.msuf32MutationRate false
    |> run.SgdRun

[<Fact>]
let ``SGD host resolves custom configured makers`` () =
    let key = Guid.NewGuid().ToString()
    let names = evolutionMakerNames.create (key) (key) (key)
    SortableTestsMakerCatalog.register key (fun _ _ -> async { return Error "host tests" })
    SeedSorterPoolSetMakerCatalog.register key (fun _ _ -> async { return Error "host seed" })
    MutatorMakerCatalog.register key (fun _ -> async { return Error "host mutator" })
    match runHost.Create (makeRun names "CatalogTests" "Custom") 1 with
    | SimpleRunHost _ -> failwith "Expected SGD host"
    | SgdRunHost host ->
        Assert.Equal(names, host.MakerNames)
        Assert.Equal("host tests", host.MakeSortableTests context emptyParameters |> Async.RunSynchronously |> errorOf)
        Assert.Equal("host seed", host.MakeSeedPoolSet context emptyParameters |> Async.RunSynchronously |> errorOf)
        Assert.Equal("host mutator", host.MakeMutator emptyParameters |> Async.RunSynchronously |> errorOf)

[<Theory>]
[<InlineData("tests")>]
[<InlineData("seed")>]
[<InlineData("mutator")>]
let ``SGD host rejects unknown makers before execution`` field =
    let unknown = Guid.NewGuid().ToString()
    let names =
        match field with
        | "tests" -> EvolutionMakerNames.standard.WithSortableTests(unknown)
        | "seed" -> EvolutionMakerNames.standard.WithSeedPoolSet(unknown)
        | _ -> EvolutionMakerNames.standard.WithMutator(unknown)
    let error = Assert.Throws<InvalidOperationException>(fun () -> runHost.Create (makeRun names "Tests" "Unknown") 1 |> ignore)
    Assert.Contains(unknown, error.Message)

[<Fact>]
let ``SGD maker names survive MessagePack round trip`` () =
    MessagePackSetup.configure ()
    let names = evolutionMakerNames.create ("custom.tests") ("custom.seed") ("custom.mutator")
    let dto = makeRun names "Tests" "Custom" |> RunDto.fromDomain
    let restored = MessagePackSerializer.Serialize dto |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    Assert.Equal(Some names, restored.MakerNames)

[<Theory>]
[<InlineData("SorterSgd.Prefix.32p4a.Msuf", "Report", "prefix")>]
[<InlineData("SorterSgd.Merge", "Report", "merge")>]
[<InlineData("Tests", "OldGenPrefix", "prefix")>]
[<InlineData("Tests", "OldGenMerge", "merge")>]
[<InlineData("Tests", "OldGenSoss", "sossPrefix")>]
[<InlineData("Tests", "OldGenStandard", "standard")>]
[<InlineData("SorterSgd.Prefix", "OldGenStandard", "standard")>]
[<InlineData("SorterSgd.Prefix", "OldGenMerge", "merge")>]
let ``legacy saved runs recover maker presets without new fields`` projectName runName expected =
    MessagePackSetup.configure ()
    let dto = makeRun EvolutionMakerNames.standard projectName runName |> RunDto.fromDomain
    let json = MessagePackSerializer.Serialize dto |> MessagePackSerializer.ConvertToJson |> System.Text.Json.Nodes.JsonNode.Parse
    for key in ["SortableTestsMakerName"; "SeedPoolSetMakerName"; "MutatorMakerName"] do
        Assert.True(json.AsObject().Remove key)
    let restored = MessagePackSerializer.ConvertFromJson(json.ToJsonString()) |> MessagePackSerializer.Deserialize<runDto> |> RunDto.toDomain
    let names = match expected with "sossPrefix" -> EvolutionMakerNames.sossPrefix | "prefix" -> EvolutionMakerNames.prefix | "merge" -> EvolutionMakerNames.merge | _ -> EvolutionMakerNames.standard
    Assert.Equal(Some names, restored.MakerNames)

[<Fact>]
let ``partially configured saved runs are rejected`` () =
    let dto = makeRun EvolutionMakerNames.standard "Tests" "Partial" |> RunDto.fromDomain
    Assert.Throws<ArgumentException>(fun () -> RunDto.toDomain { dto with MutatorMakerName = null } |> ignore) |> ignore

[<Theory>]
[<InlineData("seed-sorter-pool-set.standard", "seed-pool.soss.standard")>]
[<InlineData("seed-sorter-pool-set.merge", "seed-pool.soss.merge")>]
[<InlineData("seed-sorter-pool-set.prefix", "seed-pool.soss.prefix")>]
let ``legacy Soss seed names migrate to Soss variants`` oldName expected =
    let names = EvolutionMakerNames.prefix.WithSeedPoolSet(oldName)
    let restored = makeRun names "Tests" "OldGenSoss" |> RunDto.fromDomain |> RunDto.toDomain
    Assert.Equal(expected, restored.MakerNames.Value.SeedPoolSet)

[<Theory>]
[<InlineData("seed-pool.standard.standard", false)>]
[<InlineData("seed-pool.standard.merge", false)>]
[<InlineData("seed-pool.standard.prefix", false)>]
[<InlineData("seed-pool.soss.standard", true)>]
[<InlineData("seed-pool.soss.merge", true)>]
[<InlineData("seed-pool.soss.prefix", true)>]
let ``seed catalogs produce the requested pool variant`` (name: string) (expectSoss: bool) =
    ProjectCatalogs.registerAll ()
    let evaluation =
        sorterEvalV2.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 32<sortingWidth> 3<sortableCount>
            (UMX.tag 7) 2<stageLength> [||] (UMX.tag<isReflectionSymmetric> false)
            (UMX.tag 1) (UMX.tag 0) |> sorterEval.V2
    let evaluations = sorterSetEval.create (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) (UMX.tag (Guid.NewGuid())) [|evaluation|]
    let supplied =
        context.WithLoadStandardSorterEvals(fun _ _ _ -> async { return Ok evaluations })
            .WithLoadMergeSorterEvals(fun _ _ _ -> async { return Ok evaluations })
            .WithLoadPrefixSorterEvals(fun _ _ _ -> async { return Ok evaluations })
    let parameters =
        emptyParameters.WithSortingWidth(Some 32<sortingWidth>)
            .WithRepl(Some 0<replNumber>)
            .WithMergeLibId(Some (mergeLibId.create 32<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA))
            .WithPrefixLibId(Some (prefixLibId.create 32<sortingWidth> 4<stageLength> prefixLibVariant.PrefixA))
            .WithSorterPoolCount(Some 1<sorterPoolCount>)
            .WithSorterCountPerPool(Some 1<sorterCountPerPool>)
            .WithSorterEvalMeasureInitial(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
            .WithRngType(Some rngType.Lcg)
            .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf4)
            .WithSeedSorterPoolSelectionType(Some (sorterSelectionType.GuidOrder 1<sorterCount>))
            .WithExcludeSelfCe(Some (UMX.tag<excludeSelfCe> true))
            .WithMutationMod(Some 0<mutationMod>)
    let poolSet =
        match SeedSorterPoolSetMakerCatalog.get name supplied parameters |> Async.RunSynchronously with
        | Ok value -> value
        | Error error -> failwith error
    match poolSet with
    | sorterPoolSet.Standard value ->
        Assert.False(expectSoss)
        Assert.Single(value.SorterPools) |> ignore
    | sorterPoolSet.Soss value ->
        Assert.True(expectSoss)
        Assert.Single(value.SorterPools) |> ignore

[<Theory>]
[<InlineData(true)>]
[<InlineData(false)>]
let ``workflows reject incompatible seeds before evaluating or saving`` (sossWorkflow: bool) =
    let standard = sorterPoolSet_Standard.create (UMX.tag (Guid.NewGuid())) 0<generationNumber> (Dim1 1) None
    let seed = if sossWorkflow then sorterPoolSet.Standard standard else SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
    let creator _ = async { return Ok seed }
    let result =
        if sossWorkflow then
            GeneSort.Dispatch.V1.SorterSgd.Soss.SgdEx_Soss.initializeAndSaveSeedPoolSet
                creator Unchecked.defaultof<_> Unchecked.defaultof<_> Unchecked.defaultof<_> emptyParameters
                Unchecked.defaultof<_> Unchecked.defaultof<_> ignore
            |> Async.RunSynchronously |> Result.map ignore
        else
            GeneSort.Dispatch.V1.SorterSgd.Standard.SgdEx_Standard.initializeAndSaveSeedPoolSet
                creator Unchecked.defaultof<_> Unchecked.defaultof<_> Unchecked.defaultof<_> emptyParameters
                Unchecked.defaultof<_> Unchecked.defaultof<_> ignore
            |> Async.RunSynchronously |> Result.map ignore
    Assert.Contains((if sossWorkflow then "Expected a Soss" else "Expected a Standard"), errorOf result)

[<Fact>]
let ``project catalogs initialize together without dispatch dependencies`` () =
    ProjectCatalogs.registerAll ()
    ProjectCatalogs.registerAll ()
    QueryParamsCatalog.get QueryCatalogNames.sortableTestMerge |> ignore
    RunParamFilterBuilders.get RunParamBuilderNames.Filter.identity |> ignore
    MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel |> ignore
    let assembly = typeof<evolutionMakerContext>.Assembly
    Assert.Equal("GeneSort.Project.V1", assembly.GetName().Name)
    Assert.DoesNotContain(assembly.GetReferencedAssemblies(), fun reference ->
        reference.Name = "GeneSort.Dispatch.V1" || reference.Name = "GeneSort.FileDb.V1" || reference.Name = "GeneSort.Db.V1")

[<Fact>]
let ``merge test maker uses the supplied storage context`` () =
    let library = mergeLibId.create 16<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA
    let parameters = emptyParameters.WithMergeLibId(Some library).WithSortableDataFormat(Some sortableDataFormat.BitVector512)
    let mutable called = false
    let loadMergeTests (repl: int<replNumber>) actualLibrary format =
        async {
            called <- true
            Assert.Equal(0, UMX.untag repl)
            Assert.Equal(library, actualLibrary)
            Assert.Equal(sortableDataFormat.BitVector512, format)
            return Error "supplied loader result"
        }
    let suppliedContext = context.WithLoadMergeTests(loadMergeTests)
    ProjectCatalogs.registerAll ()
    let actual = SortableTestsMakerCatalog.get SortableTestsMakerNames.merge suppliedContext parameters |> Async.RunSynchronously |> errorOf
    Assert.True(called)
    Assert.Equal("supplied loader result", actual)

[<Theory>]
[<InlineData("standard")>]
[<InlineData("merge")>]
[<InlineData("prefix")>]
let ``built-in names resolve to the corresponding makers`` variant =
    SorterSgdMakerCatalogs.registerAll ()
    SorterSgdMakerCatalogs.registerAll ()
    let testsName, testsMaker, seedName, seedMaker =
        match variant with
        | "standard" -> SortableTestsMakerNames.standard, EvolutionTestMakers.makeStandardTests,
                        SeedSorterPoolSetMakerNames.standard, SeedPoolSetMakers.createSeedSorterPoolSetStandard
        | "merge" -> SortableTestsMakerNames.merge, EvolutionTestMakers.makeMergeTests,
                     SeedSorterPoolSetMakerNames.merge, SeedPoolSetMakers.createSeedSorterPoolSetMerge
        | _ -> SortableTestsMakerNames.prefix, EvolutionTestMakers.makePrefixTests,
               SeedSorterPoolSetMakerNames.prefix, SeedPoolSetMakers.createSeedSorterPoolSetPrefix
    let expectedTests = testsMaker context emptyParameters |> Async.RunSynchronously |> errorOf
    let actualTests = SortableTestsMakerCatalog.get testsName context emptyParameters |> Async.RunSynchronously |> errorOf
    Assert.Equal(expectedTests, actualTests)
    let expectedSeed = seedMaker context emptyParameters |> Async.RunSynchronously |> errorOf
    let actualSeed = SeedSorterPoolSetMakerCatalog.get seedName context emptyParameters |> Async.RunSynchronously |> errorOf
    Assert.Equal(expectedSeed, actualSeed)

[<Fact>]
let ``custom makers can be registered and invoked by name`` () =
    let name = Guid.NewGuid().ToString()
    SortableTestsMakerCatalog.register name (fun _ _ -> async { return Error "custom tests" })
    SeedSorterPoolSetMakerCatalog.register name (fun _ _ -> async { return Error "custom seed" })
    Assert.Equal("custom tests", SortableTestsMakerCatalog.get name context emptyParameters |> Async.RunSynchronously |> errorOf)
    Assert.Equal("custom seed", SeedSorterPoolSetMakerCatalog.get name context emptyParameters |> Async.RunSynchronously |> errorOf)
    Assert.Throws<InvalidOperationException>(fun () ->
        SortableTestsMakerCatalog.register name (fun _ _ -> async { return Error "replacement" })) |> ignore
    Assert.Throws<InvalidOperationException>(fun () ->
        SeedSorterPoolSetMakerCatalog.register name (fun _ _ -> async { return Error "replacement" })) |> ignore

[<Fact>]
let ``unknown names and blank registrations report catalog errors`` () =
    let name = Guid.NewGuid().ToString()
    Assert.Throws<InvalidOperationException>(fun () -> SortableTestsMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> SeedSorterPoolSetMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        SortableTestsMakerCatalog.register " " (fun _ _ -> async { return Error "unused" })) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        SeedSorterPoolSetMakerCatalog.register " " (fun _ _ -> async { return Error "unused" })) |> ignore

[<Fact>]
let ``simple mutator maker resolves by its built-in name`` () =
    SorterSgdMakerCatalogs.registerAll ()
    SorterSgdMakerCatalogs.registerAll ()
    let expected = MutatorMakers.makeSimpleSorterModelMutator emptyParameters |> Async.RunSynchronously |> errorOf
    let actual = MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel emptyParameters |> Async.RunSynchronously |> errorOf
    Assert.Equal(expected, actual)

[<Fact>]
let ``mutator catalog supports custom makers and validates names`` () =
    let name = Guid.NewGuid().ToString()
    Assert.Throws<InvalidOperationException>(fun () -> MutatorMakerCatalog.get name |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () ->
        MutatorMakerCatalog.register " " (fun _ -> async { return Error "unused" })) |> ignore
    MutatorMakerCatalog.register name (fun _ -> async { return Error "custom mutator" })
    Assert.Equal("custom mutator", MutatorMakerCatalog.get name emptyParameters |> Async.RunSynchronously |> errorOf)
    Assert.Throws<InvalidOperationException>(fun () ->
        MutatorMakerCatalog.register name (fun _ -> async { return Error "replacement" })) |> ignore

[<Fact>]
let ``all generation executors initialize the catalogs`` () =
    for executorType in [GenStandard; GenSoss] do
        Assert.NotNull(SorterSgdExecutorType.getExecutor executorType)
    SortableTestsMakerCatalog.get SortableTestsMakerNames.prefix |> ignore
    SeedSorterPoolSetMakerCatalog.get SeedSorterPoolSetMakerNames.prefix |> ignore
    MutatorMakerCatalog.get MutatorMakerNames.simpleSorterModel |> ignore
