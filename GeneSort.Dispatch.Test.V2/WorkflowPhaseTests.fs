module GeneSort.Dispatch.Test.V2.WorkflowPhaseTests

open System
open System.Threading
open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Dispatch.V1.SorterSgd.Standard
open GeneSort.Dispatch.V1.SorterSgd.Soss

let private parameters =
    (runParameters.create Map.empty)
        .WithSortingWidth(Some 2<sortingWidth>)
        .WithPartitionCount(Some 1<partitionCount>)
        .WithRngType(Some Lcg)
        .WithSeedSoss(Some (UMX.tag 1UL))
        .WithSorterEvalMeasure(Some (stageLengthMeasure.create false false |> sorterEvalMeasure.StageLength))
        .WithGenerationCurrent(Some 7<generationNumber>)

let private intervals = { Name = "test"; Min = 0; MaxCount = Some 2; Scale = 1.0; Method = Constant 1 }
let private tests = sortableBinaryTests.create (UMX.tag Guid.Empty) 2<sortingWidth> [||] |> sortableTests.Bools
let private pool soss (generation: int) =
    let standard = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) (UMX.tag generation) (Dim1 1) None
    if soss then SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
    else sorterPoolSet.Standard standard

type private MemoryDb() =
    let data = System.Collections.Generic.Dictionary<geneSortDataMode * Guid, outputData>()
    let reads = ResizeArray<geneSortDataMode * geneSortDataType>()
    let writes = ResizeArray<geneSortDataMode * geneSortDataType>()
    member _.Reads = reads
    member _.Writes = writes
    interface IGeneSortDb with
        member _.databaseName = UMX.tag "phase-test"
        member _.makeQueryParamsFromRunParams rp dataType =
            Some (queryParams.create (UMX.tag "test") (UMX.tag "test") None (rp.GetGenerationCurrent()) dataType [||])
        member _.saveAsync mode query value _ = async {
            writes.Add(mode, query.OutputDataType)
            data.[(mode, %query.Id)] <- value
            return Ok ()
        }
        member _.doesOutPutDataExist mode query = async { return data.ContainsKey(mode, %query.Id) }
        member _.loadAsync mode query = async {
            reads.Add(mode, query.OutputDataType)
            return match data.TryGetValue((mode, %query.Id)) with
                   | true, value -> Ok value
                   | _ -> Error "Missing prepared input"
        }
        member _.loadIfFoundAsync mode query = async {
            reads.Add(mode, query.OutputDataType)
            return match data.TryGetValue((mode, %query.Id)) with true, value -> Some value | _ -> None
        }
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected parameter read"

let private save (db: IGeneSortDb) mode (generation: int) dataType value =
    let rp = parameters.WithGenerationCurrent(Some (UMX.tag generation))
    let query = db.makeQueryParamsFromRunParams rp dataType |> Option.get
    db.saveAsync mode query value (UMX.tag false) |> Async.RunSynchronously |> Result.defaultWith failwith

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``evolution setup saves only Setup and reuses it on subsequent calls`` soss =
    let memory = MemoryDb()
    let db = memory :> IGeneSortDb
    let mutable testCalls, poolCalls = 0, 0
    let makeTests (rp: runParameters) = async {
        Assert.Equal(Some 0<generationNumber>, rp.GetGenerationCurrent())
        testCalls <- testCalls + 1
        return Ok (tests, [||])
    }
    let makePool (rp: runParameters) = async {
        Assert.Equal(Some 0<generationNumber>, rp.GetGenerationCurrent())
        poolCalls <- poolCalls + 1
        return Ok (pool soss 0)
    }
    use cts = new CancellationTokenSource()
    let prepare = if soss then SgdEx_Soss.setupEvolutionRunSoss else SgdEx_Standard.setupEvolutionRunStandard
    for _ in 1..2 do
        let result = prepare makeTests makePool db intervals intervals parameters cts None |> Async.RunSynchronously
        match result with
        | Ok updated -> Assert.Same(parameters, updated)
        | Error error -> failwith error
    Assert.Equal(1, testCalls)
    Assert.Equal(1, poolCalls)
    Assert.Equal<list<geneSortDataMode * geneSortDataType>>(
        [geneSortDataMode.Setup, geneSortDataType.SortableTestsWithPrefix ""; geneSortDataMode.Setup, geneSortDataType.SorterPoolSet ""],
        List.ofSeq memory.Writes)

[<Theory>]
[<InlineData(false, "setup")>]
[<InlineData(true, "setup")>]
[<InlineData(false, "results")>]
[<InlineData(true, "results")>]
[<InlineData(false, "missing")>]
[<InlineData(true, "missing")>]
let ``evolution run reads prepared inputs and prioritizes Results without setup writes`` soss source =
    let memory = MemoryDb()
    let db = memory :> IGeneSortDb
    save db geneSortDataMode.Setup 0 (geneSortDataType.SortableTestsWithPrefix "") (outputData.SortableTestsWithPrefix(tests, [||]))
    if source <> "missing" then
        save db geneSortDataMode.Setup 0 (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet(pool soss 0))
    if source = "results" then
        save db geneSortDataMode.Results 1 (geneSortDataType.SorterPoolSet "") (outputData.SorterPoolSet(pool soss 1))
    memory.Writes.Clear()
    let mutable selectedGeneration = None
    let mutator (rp: runParameters) = async {
        selectedGeneration <- rp.GetGenerationCurrent()
        return Error "Stop before evolution"
    }
    use cts = new CancellationTokenSource()
    let run = if soss then SgdEx_Soss.evaluateEvolutionRunSoss else SgdEx_Standard.evaluateEvolutionRunStandard
    let result = run mutator db intervals intervals parameters (UMX.tag false) cts None |> Async.RunSynchronously
    Assert.Equal(Error (if source = "missing" then "Missing prepared input" else "Stop before evolution"), result)
    Assert.Equal((match source with "results" -> Some 1<generationNumber> | "setup" -> Some 0<generationNumber> | _ -> None), selectedGeneration)
    Assert.Empty(memory.Writes)
    if source = "results" then
        Assert.DoesNotContain((geneSortDataMode.Setup, geneSortDataType.SorterPoolSet ""), memory.Reads)
