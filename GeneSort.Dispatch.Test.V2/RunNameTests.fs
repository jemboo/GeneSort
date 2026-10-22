module GeneSort.Dispatch.Test.V2.RunNameTests

open System.Reflection
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Dispatch.V1

let private allPresets () =
    typeof<runHost>.Assembly.GetTypes()
    |> Array.filter (fun t -> t.FullName.Contains("Specs") || t.FullName.Contains(".SorterSgd.Prefix."))
    |> Array.collect (fun t -> t.GetMethods(BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly))
    |> Array.filter (fun m -> m.ReturnType = typeof<run> && m.GetParameters().Length = 0)

[<Fact>]
let ``Project names follow the hierarchy above the database module`` () =
    let presets =
        [ "SorterSgd.Prefix.p32.Msuf4a.Standard", SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_32.Test
          "SorterSgd.Prefix.p32.Msuf4a.Standard_append", SorterSgd.Prefix.p32.Msuf4a.Standard_append.Test
          "SorterSgd.Prefix.p32.Msuf4a.Standard_append", SorterSgd.Prefix.p32.Msuf4a.Standard_append.Pool_128.V1
          "SorterSgd.Prefix.p32.Msuf4a.Soss_condense", SorterSgd.Prefix.p32.Msuf4a.Soss_condense.Pool_128_condense.Test
          "SorterSgd.Prefix.p32.Msrs4a.MutationRate", SorterSgd.Prefix.p32.Msrs4a.MutationRate.VarModR_32.Test
          "SorterEval.SorterEvalSpecsRs", SorterEval.SorterEvalSpecsRs.Standard.Rand_Test
          "SorterMutate.MutateSpecsRp", SorterMutate.MutateSpecsRp.RandomPrefix.Test_Mssi
          "SortableTests.SortableTestsSpecsPrefix", SortableTests.SortableTestsSpecsPrefix.Prefix.Prefix_32 ]
    for expected, preset in presets do
        Assert.Equal(expected, %(preset ()).ProjectName)

[<Fact>]
let ``Shared source readers use the projects of their producing presets`` () =
    let readers =
        [ SortableTests.SortableTestsDbs.Merge.projectName, SortableTests.SortableTestsSpecsMerge.Merge.Merge_Test
          SortableTests.SortableTestsDbs.Prefix.projectName, SortableTests.SortableTestsSpecsPrefix.Prefix.Prefix_32
          SorterEval.SorterEvalDbs.Standard.projectName, SorterEval.SorterEvalSpecsRs.Standard.Rand_Test
          SorterEval.SorterEvalDbs.Merge.projectName, SorterEval.SorterEvalSpecsRm.Merge.Rand_MergeTest_Test
          SorterEval.SorterEvalDbs.Prefix.projectName, SorterEval.SorterEvalSpecsTestPrefix.Prefix.Prefix_32
          SorterMutate.SorterMutateDbs.RandomStandard.projectName, SorterMutate.MutateSpecsRs.RandomStandard.Test_Msrs
          SorterMutate.SorterMutateDbs.RandomMerge.projectName, SorterMutate.MutateSpecsRm.RandomMerge.Test_Msce
          SorterMutate.SorterMutateDbs.RandomPrefix.projectName, SorterMutate.MutateSpecsRp.RandomPrefix.Test_Msce ]
    for readerProject, preset in readers do
        Assert.Equal(%(preset ()).ProjectName, %readerProject)

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
        [ "Test", SorterSgd.Prefix.p32.Msuf4a.Standard_append.Test
          "Pool_128", SorterSgd.Prefix.p32.Msuf4a.Standard_append.Pool_128.V1
          "Pool_512", SorterSgd.Prefix.p32.Msuf4a.Standard_append.Pool_512.V1 ]
    for expected, preset in presets do
        Assert.Equal(expected, %(preset ()).DatabaseName)

[<Fact>]
let ``Caller name is the defining function even when invoked through another function`` () =
    let invoke () = SorterSgd.Prefix.p32.Msuf4a.Standard.Pool_32.Test ()
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
