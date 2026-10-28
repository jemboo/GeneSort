module GeneSort.Dispatch.Test.V1.OnePoolAuditTests

open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Dispatch.V1
open OnePoolAuditVerification

let private headers = [| "RunName"; "SourceExecutionId"; "SourceMutationMod"; "MutationMod" |]
let private row source sourceMutation mutation =
    Map.ofArray (Array.zip headers [| "Condense"; source; sourceMutation; mutation |])
let private expectedRows =
    [| for source, mutation in ["source-a", "0"; "source-b", "3"] do
           for destination in ["0"; "1"; "2"; "3"] do
               yield row source mutation destination |]
let private lines rows =
    Array.append [| String.concat "\t" headers |]
        (rows |> Array.map (fun row -> headers |> Array.map (fun key -> Map.tryFind key row |> Option.defaultValue "") |> String.concat "\t"))
let private rejects fragment action =
    let error = Assert.Throws<System.Exception>(System.Action action)
    Assert.Contains(fragment, error.Message)

[<Fact>]
let ``Controlled seeds pair model arrays and masks across replicas and workflows`` () =
    let configs = [|
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.StandardSimple()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.StandardR()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.CompareStandardS()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.SossSimple()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.SossR()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.CompareSossS()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.MatchedStandardS()
        GeneSort.Dispatch.V1.SorterSgd.p32.Test.OnePool.Controlled.MatchedSossS()
    |]
    let make (rp: runParameters) =
        SeedSorterModelGeneration.makeControlledModelSet rp |> Result.defaultWith failwith |> fun models -> models.SorterModels
    let parameters = configs |> Array.map (fun configured -> (runHost.Create configured 1).MakeRunParameters (UMX.tag 0) (UMX.tag 5))
    Assert.All(parameters, fun rows -> Assert.Equal(10, rows.Length))
    for replica in 0 .. 4 do
        let group = parameters |> Array.map (Array.find (fun rp -> int (rp.GetRepl().Value) = replica && int (rp.GetMutationMod().Value) = 0))
        let models = group |> Array.map make
        verifyControlledSeeds models.[0] models.[1] models.[2]
        verifyControlledSeeds models.[3] models.[4] models.[5]
        verifyControlledSeeds models.[0] models.[4] models.[5]
        verifyControlledSeeds models.[0] models.[1] models.[6]
        verifyControlledSeeds models.[3] models.[4] models.[7]
        let rateR = group.[1].GetActivatorMutationRate().Value
        let aMax = group.[1].GetAMax().Value
        let expectedFlipRate =
            [for value in -aMax .. aMax do
                for delta in [-1; 1] do
                    let mutated = max -aMax (min aMax (value + delta))
                    yield if (value > 0) <> (mutated > 0) then rateR else 0.0]
            |> List.average
        Assert.Equal(expectedFlipRate, group.[6].GetActivatorMutationRate().Value, 12)
        Assert.Equal(expectedFlipRate, group.[7].GetActivatorMutationRate().Value, 12)
        Assert.All(models, fun values -> Assert.Equal(32, values.Length))
        Assert.All(group, fun rp -> Assert.Equal(Some (UMX.tag 9), rp.GetGenerationIntervalLast()))
        Assert.Equal<Guid<sorterModelId>>(models.[0] |> Array.map SorterModel.getId,
            make (group.[0].WithMutationMod(Some (UMX.tag 1))) |> Array.map SorterModel.getId)
        let other = make (group.[0].WithRepl(Some (UMX.tag (replica + 1))))
        Assert.NotEqual(SorterModel.getId models.[0].[0], SorterModel.getId other.[0])
        let bad = group.[2].WithActivationProbability(Some 0.5)
        Assert.True(SeedSorterModelGeneration.makeControlledModelSet bad |> Result.isError)

[<Fact>]
let ``Controlled audit rejects changed arrays masks IDs and missing models`` () =
    let id = UMX.tag (System.Guid.NewGuid())
    let source = msce.create id 4<sortingWidth> [|0; 1; 2|] |> simpleSorterModel.Msce
    let plain = [|sorterModel.Simple source|]
    let bounded = [|simpleSorterModelR.create source 3 [|-3; 0; 2|] |> sorterModel.SimpleR|]
    let boolean = [|simpleSorterModelS.create source [|false; false; true|] |> sorterModel.SimpleS|]
    verifyControlledSeeds plain bounded boolean
    rejects "masks differ" (fun () -> verifyControlledSeeds plain bounded [|simpleSorterModelS.create source [|true; false; true|] |> sorterModel.SimpleS|])
    let changed = msce.create id 4<sortingWidth> [|2; 1; 0|] |> simpleSorterModel.Msce
    rejects "arrays differ" (fun () -> verifyControlledSeeds [|sorterModel.Simple changed|] bounded boolean)
    let other = msce.create (UMX.tag (System.Guid.NewGuid())) 4<sortingWidth> [|0; 1; 2|] |> simpleSorterModel.Msce
    rejects "IDs differ" (fun () -> verifyControlledSeeds [|sorterModel.Simple other|] bounded boolean)
    rejects "no models" (fun () -> verifyControlledSeeds plain bounded [||])

[<Theory>]
[<InlineData("AMax")>]
[<InlineData("ActivationProbability")>]
[<InlineData("ActivatorMutationRate")>]
let ``Reports preserve configured activator settings`` key =
    let parameters = Map.ofList [ParamKeys.aMaxKey, "3"; ParamKeys.activationProbabilityKey, "0.5"; ParamKeys.activatorMutationRateKey, "0.1"]
    verifyActivatorSettings "report.txt" parameters [|parameters|]
    for invalid in [Map.add key "99" parameters; Map.remove key parameters] do
        rejects key (fun () -> verifyActivatorSettings "report.txt" parameters [|invalid|])
    rejects key (fun () -> verifyActivatorSettings "report.txt" Map.empty [|Map.ofList [key, "3"]|])
    verifyActivatorSettings "report.txt" Map.empty [|Map.empty|]

[<Fact>]
let ``Saved models must match configured SimpleR bounds and model type`` () =
    let source = msce.create (UMX.tag (System.Guid.NewGuid())) 4<sortingWidth> [|0; 1; 2|] |> simpleSorterModel.Msce
    let paired = simpleSorterModelR.create source 3 [|-3; 0; 3|] |> sorterModel.SimpleR
    let plain = sorterModel.Simple source
    verifyModelActivators (Some 3) [paired]
    verifyModelActivators None [plain]
    verifyModelActivators (Some 0) [simpleSorterModelR.create source 0 [|0; 0; 0|] |> sorterModel.SimpleR]
    rejects "AMax differs" (fun () -> verifyModelActivators (Some 2) [paired])
    rejects "Expected a SimpleR" (fun () -> verifyModelActivators (Some 3) [plain])
    rejects "Unexpected SimpleR" (fun () -> verifyModelActivators None [paired])

[<Fact>]
let ``Audit rejects incorrect SimpleS family and conflicting activation settings`` () =
    let source = msce.create (UMX.tag (System.Guid.NewGuid())) 4<sortingWidth> [|0; 1; 2|] |> simpleSorterModel.Msce
    let paired = simpleSorterModelS.create source [|true; false; true|] |> sorterModel.SimpleS
    verifyModelActivationParameters None (Some 0.5) [paired]
    rejects "Unexpected SimpleS" (fun () -> verifyModelActivationParameters None None [paired])
    rejects "Expected a SimpleS" (fun () -> verifyModelActivationParameters None (Some 0.5) [sorterModel.Simple source])
    rejects "no AMax" (fun () -> verifyModelActivationParameters (Some 3) (Some 0.5) [paired])

[<Fact>]
let ``Combined report accepts all eight executions in any order`` () =
    verifyCombinedRows headers expectedRows (lines (Array.rev expectedRows))

[<Theory>]
[<InlineData("SourceMutationMod")>]
[<InlineData("MutationMod")>]
let ``Combined report rejects incorrect mutation values`` key =
    let changed = Array.copy expectedRows
    changed.[0] <- Map.add key "99" changed.[0]
    rejects "Combined summary differs" (fun () -> verifyCombinedRows headers expectedRows (lines changed))

[<Fact>]
let ``Combined report rejects missing execution rows`` () =
    rejects "Combined summary differs" (fun () -> verifyCombinedRows headers expectedRows (lines expectedRows.[1..]))

[<Fact>]
let ``Combined report rejects extra execution rows`` () =
    rejects "Combined summary differs" (fun () ->
        verifyCombinedRows headers expectedRows (lines (Array.append expectedRows [|expectedRows.[0]|])))

[<Fact>]
let ``Combined report rejects a duplicate replacing a missing execution at the same row count`` () =
    let changed = Array.copy expectedRows
    changed.[1] <- changed.[0]
    rejects "Combined summary differs" (fun () -> verifyCombinedRows headers expectedRows (lines changed))

[<Fact>]
let ``Combined report preserves legitimate duplicate row multiplicity`` () =
    let expected = Array.append expectedRows [|expectedRows.[0]|]
    verifyCombinedRows headers expected (lines expected)
    rejects "Combined summary differs" (fun () -> verifyCombinedRows headers expected (lines expectedRows))

[<Theory>]
[<InlineData("SourceMutationMod")>]
[<InlineData("MutationMod")>]
let ``Individual report rejects incorrect or missing mutation values`` key =
    let valid = row "source-b" "3" "1"
    verifyMutations "report.txt" "1" (Some "3") [|valid|]
    for invalid in [Map.add key "99" valid; Map.remove key valid] do
        rejects key (fun () -> verifyMutations "report.txt" "1" (Some "3") [|invalid|])

[<Fact>]
let ``Base reports require blank source mutation`` () =
    let valid = row "" "" "2"
    verifyMutations "base.txt" "2" None [|valid; Map.remove ParamKeys.sourceMutationModKey valid|]
    rejects "SourceMutationMod" (fun () -> verifyMutations "base.txt" "2" None [|Map.add ParamKeys.sourceMutationModKey "3" valid|])

let private spans = [ParamKeys.mutationModKey, ["0"; "1"; "2"; "3"]]
let private sources = [| "source-a", "0"; "source-b", "3" |]
let private executions =
    expectedRows |> Array.map (fun row ->
        row.["SourceExecutionId"], row.["SourceMutationMod"], row.["MutationMod"],
        Map.ofList [ParamKeys.mutationModKey, row.["MutationMod"]])

[<Fact>]
let ``Execution expansion requires four mutations for each of two sources`` () =
    let combinations, count = verifyExpansion "Condense" spans sources (Array.rev executions)
    Assert.Equal(4, combinations)
    Assert.Equal(8, count)

[<Theory>]
[<InlineData("source")>]
[<InlineData("destination")>]
[<InlineData("missing")>]
[<InlineData("extra")>]
[<InlineData("replacement")>]
let ``Execution expansion rejects corrupt values and multiplicities`` corruption =
    let changed = Array.copy executions
    let id, source, destination, parameters = changed.[0]
    let invalid =
        match corruption with
        | "source" -> changed.[0] <- id, "99", destination, parameters; changed
        | "destination" -> changed.[0] <- id, source, "99", parameters; changed
        | "missing" -> changed.[1..]
        | "extra" -> Array.append changed [|changed.[0]|]
        | "replacement" -> changed.[1] <- changed.[0]; changed
        | _ -> failwith "Unknown test case"
    rejects "execution expansion" (fun () -> verifyExpansion "Condense" spans sources invalid |> ignore)

[<Fact>]
let ``Execution expansion inherits source mutations and expands other spans`` () =
    let otherSpans = ["GenerationIntervalLast", ["2"; "3"]]
    let actual =
        [| "source-a", "0", "0", Map.ofList ["GenerationIntervalLast", "2"]
           "source-a", "0", "0", Map.ofList ["GenerationIntervalLast", "3"]
           "source-b", "3", "3", Map.ofList ["GenerationIntervalLast", "2"]
           "source-b", "3", "3", Map.ofList ["GenerationIntervalLast", "3"] |]
    let combinations, count = verifyExpansion "Append" otherSpans sources actual
    Assert.Equal(2, combinations)
    Assert.Equal(4, count)
