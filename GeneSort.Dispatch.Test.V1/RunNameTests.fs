module GeneSort.Dispatch.Test.V1.RunNameTests

open System.Reflection
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Dispatch.V1

let private allPresets () =
    typeof<runHost>.Assembly.GetTypes()
    |> Array.filter (fun t ->
        ["SortableTests"; "SorterEval"; "SorterMutate"; "SorterSgd"]
        |> List.exists (fun workflow -> t.FullName.StartsWith("GeneSort.Dispatch.V1." + workflow + ".", System.StringComparison.Ordinal)))
    |> Array.collect (fun t -> t.GetMethods(BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly))
    |> Array.filter (fun m -> m.ReturnType = typeof<run> && m.GetParameters().Length = 0)

[<Fact>]
let ``Project names follow the hierarchy above the database module`` () =
    let presets =
        [ "SorterSgd.p32.Msuf4a.Standard", SorterSgd.p32.Msuf4a.Standard.Pool_32.Test
          "SorterSgd.p32.Msuf4a.Standard_append", SorterSgd.p32.Msuf4a.Standard_append.Test
          "SorterSgd.p32.Msuf4a.Standard_append", SorterSgd.p32.Msuf4a.Standard_append.Pool_128.V1
          "SorterSgd.p32.Test.OnePool", SorterSgd.p32.Test.OnePool.Continuation.TestSossCondense
          "SorterSgd.p32.Msrs4a.MutationRate", SorterSgd.p32.Msrs4a.MutationRate.VarModR_32.Test
          "SorterEval", SorterEval.Standard.Test
          "SorterMutate", SorterMutate.Prefix.Mssi
          "SortableTests", SortableTests.Prefix.Width32
          "SortableTests", SortableTests.PrefixTest.Test ]
    for expected, preset in presets do
        Assert.Equal(expected, %(preset ()).ProjectName)

[<Fact>]
let ``Shared source readers use the projects and databases of their producing presets`` () =
    let readers =
        [ SortableTests.SortableTestsDbs.Merge.projectName, SortableTests.SortableTestsDbs.Merge.dbName, SortableTests.Merge.Test
          SortableTests.SortableTestsDbs.Prefix.projectName, SortableTests.SortableTestsDbs.Prefix.dbName, SortableTests.Prefix.Width32
          SorterEval.SorterEvalDbs.Standard.projectName, SorterEval.SorterEvalDbs.Standard.dbName, SorterEval.Standard.Test
          SorterEval.SorterEvalDbs.Merge.projectName, SorterEval.SorterEvalDbs.Merge.dbName, SorterEval.Merge.Test
          SorterEval.SorterEvalDbs.Prefix.projectName, SorterEval.SorterEvalDbs.Prefix.dbName, SorterEval.Prefix.Width32
          SorterMutate.SorterMutateDbs.RandomStandard.projectName, SorterMutate.SorterMutateDbs.RandomStandard.Uniform.dbName, SorterMutate.Standard.Msrs
          SorterMutate.SorterMutateDbs.RandomMerge.projectName, SorterMutate.SorterMutateDbs.RandomMerge.Uniform.dbName, SorterMutate.Merge.Msce
          SorterMutate.SorterMutateDbs.RandomPrefix.projectName, SorterMutate.SorterMutateDbs.RandomPrefix.Uniform.dbName, SorterMutate.Prefix.Msce ]
    for readerProject, readerDatabase, preset in readers do
        Assert.Equal(%(preset ()).ProjectName, %readerProject)
        Assert.Equal(%(preset ()).DatabaseName, %readerDatabase)

[<Fact>]
let ``Database names follow their modules and preset identities are unique`` () =
    let identities = System.Collections.Generic.HashSet<string * string * string>()
    let presets = allPresets ()
    Assert.NotEmpty(presets)
    for preset in presets do
        let configured = preset.Invoke(null, [||]) :?> run
        let expected =
            if preset.DeclaringType.Name = "Standard_append" && preset.Name = "Test" then "Test"
            else preset.DeclaringType.Name
        Assert.Equal(expected, %configured.DatabaseName)
        Assert.True(identities.Add(%configured.ProjectName, %configured.DatabaseName, %configured.RunName),
                    sprintf "Duplicate identity for %s.%s" preset.DeclaringType.FullName preset.Name)

[<Fact>]
let ``Standard append database names follow the test function and pool modules`` () =
    let presets =
        [ "Test", SorterSgd.p32.Msuf4a.Standard_append.Test
          "Pool_128", SorterSgd.p32.Msuf4a.Standard_append.Pool_128.V1
          "Pool_512", SorterSgd.p32.Msuf4a.Standard_append.Pool_512.V1 ]
    for expected, preset in presets do
        Assert.Equal(expected, %(preset ()).DatabaseName)

[<Fact>]
let ``Caller name is the defining function even when invoked through another function`` () =
    let invoke () = SorterSgd.p32.Msuf4a.Standard.Pool_32.Test ()
    Assert.Equal("Test", %(invoke ()).RunName)

[<Fact>]
let ``All dispatch presets use their defining name as RunName`` () =
    let presets = allPresets ()
    Assert.NotEmpty(presets)
    for preset in presets do
        Assert.True(preset.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.NoInlining),
                    sprintf "%s.%s must disable inlining for RunNaming.create()." preset.DeclaringType.FullName preset.Name)
        let configured = preset.Invoke(null, [||]) :?> run
        let expected = if preset.IsSpecialName && preset.Name.StartsWith("get_") then preset.Name.Substring(4) else preset.Name
        Assert.True((expected = %configured.RunName),
                    sprintf "%s.%s has RunName '%s'." preset.DeclaringType.FullName preset.Name %configured.RunName)
