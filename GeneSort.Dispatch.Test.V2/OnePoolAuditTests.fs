module GeneSort.Dispatch.Test.V2.OnePoolAuditTests

open Xunit
open GeneSort.Project.V1.Parameters
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
