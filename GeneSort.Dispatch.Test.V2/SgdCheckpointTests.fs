module GeneSort.Dispatch.Test.V2.SgdCheckpointTests

open System
open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Dispatch.V1.SorterSgd

[<Theory>]
[<InlineData(false, -1)>]
[<InlineData(true, -1)>]
[<InlineData(false, 0)>]
[<InlineData(true, 0)>]
[<InlineData(false, 1)>]
[<InlineData(true, 1)>]
[<InlineData(false, 2)>]
[<InlineData(true, 2)>]
let ``checkpoint search supports Setup-only seeds and legacy generation zero Results`` soss highest =
    let parameters = runParameters.create Map.empty
    let config = { Name = "test"; Min = 0; MaxCount = Some (if highest = 1 then 2 else 4); Scale = 1.0; Method = Constant 1 }
    let exists generation =
        if highest = 0 then generation = 0
        else generation > 0 && generation <= highest
    let db =
        { new IGeneSortDb with
            member _.databaseName = UMX.tag "test"
            member _.makeQueryParamsFromRunParams rp dataType =
                Some (queryParams.create (UMX.tag "test") (UMX.tag "test") None (rp.GetGenerationCurrent()) dataType [||])
            member _.doesOutPutDataExist mode qp = async {
                Assert.Equal(geneSortDataMode.Results, mode)
                return exists (int qp.Generation.Value)
            }
            member _.loadIfFoundAsync mode qp = async {
                Assert.Equal(geneSortDataMode.Results, mode)
                Assert.True(exists (int qp.Generation.Value))
                let standard = sorterPoolSet_Standard.create (UMX.tag Guid.Empty) qp.Generation.Value (Dim1 1) None
                let pool =
                    if soss then SorterPoolSet_Soss.fromStandard standard |> sorterPoolSet.Soss
                    else sorterPoolSet.Standard standard
                return Some (outputData.SorterPoolSet pool)
            }
            member _.saveAsync _ _ _ _ = failwith "Search must not save"
            member _.loadAsync _ _ = failwith "Unexpected load"
            member _.getRunParameters _ _ _ _ _ = failwith "Unexpected run parameters load" }
    let actual =
        if soss then
            Utils.loadHighestGenSorterPoolSetSoss config db parameters
            |> Async.RunSynchronously |> Result.map (Option.map (fun pool -> int pool.GenerationNumber))
        else
            Utils.loadHighestGenSorterPoolSet config db parameters
            |> Async.RunSynchronously |> Result.map (Option.map (fun pool -> int pool.GenerationNumber))
    Assert.Equal(Ok (if highest < 0 then None else Some highest), actual)
