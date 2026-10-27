namespace GeneSort.Project.V1.Catalogs.Setup.SorterSgd

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage


/// Select source checkpoints before execution spans are expanded or parameters are saved.
module EvoCondenseSelection =
    let private require message = Result.ofOption message

    let private rank (db: IGeneSortDb) (configured: run) (source: run) count (candidates: runParameters[]) =
        asyncResult {
            do! if source.ExecuteFunctionName = ExecuteFunctionNames.standard || source.ExecuteFunctionName = ExecuteFunctionNames.soss then Ok ()
                else Error "Condense requires a Standard or Soss source."
            do! if count > 0 && candidates.Length >= count then Ok ()
                else Error "Condense requires at least SelectedSorterPoolCount source mutation variants."
            let! repl = candidates.[0].GetRepl() |> require "Missing source replica."
            do! if candidates |> Array.forall (fun rp -> rp.GetRepl() = Some repl) then Ok ()
                else Error "Condense selects sources separately for each replica."
            let! interval =
                configured.RunParameterSpans |> List.tryFind (fst >> (=) ParamKeys.sourceGenerationIntervalKey)
                |> Option.bind (fun (_, values) -> match values with [value] -> Some value | _ -> None)
                |> require "Missing source generation interval."
                |> Result.bind (fun value -> match Int32.TryParse value with true, n when n >= 0 -> Ok n | _ -> Error "Invalid source generation interval.")
            let! sourceData = db.loadAsync geneSortDataMode.Project (queryParams.createForRun source.DatabaseName source.ProjectName source.RunName)
            let! savedRun = OutputData.asRun sourceData
            do! if savedRun.ProjectName = source.ProjectName && savedRun.DatabaseName = source.DatabaseName && savedRun.RunName = source.RunName
                       && savedRun.ExecuteFunctionName = source.ExecuteFunctionName then Ok ()
                else Error "Saved condense source does not match the configured run."
            let! intervalName = savedRun.GenSaveIntervalsName |> require "Source has no generation intervals."
            let! intervals =
                match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue intervalName with
                | true, value -> Ok value
                | _ -> Error (sprintf "Unknown source generation intervals '%s'." intervalName)
            let! generation = GenIntervalConfig.getSampleAtIndex intervals interval |> require "Source generation interval is unavailable."
            let! saved = db.getRunParameters source.RunName (Some repl) (Some (repl + 1<replNumber>)) None None
            let query = QueryParamsCatalogRun.get savedRun.RunQueryCatalogName source.ProjectName source.DatabaseName
            let scores = ResizeArray<float * Guid * runParameters>()
            let mutable commonMeasure = None
            let mutable commonSubset = None
            for candidate in candidates do
                let! id = candidate.GetId() |> require "Missing source execution ID."
                let! parameters =
                    match saved |> Array.filter (fun (rp: runParameters) -> rp.GetId() = Some id && rp.GetRepl() = Some repl) with
                    | [|parameters|] -> Ok parameters
                    | _ -> Error (sprintf "Expected exactly one saved source execution %O for replica %d." id (int repl))
                let! measure = parameters.GetSorterEvalMeasure() |> require "Source has no sorter evaluation measure."
                do! if commonMeasure |> Option.forall ((=) measure) then Ok ()
                    else Error "Condense source evaluation measures must match."
                commonMeasure <- Some measure
                let! poolQuery = query (parameters.WithGenerationCurrent(Some (UMX.tag generation))) (geneSortDataType.SorterPoolSet "")
                                 |> require "Cannot build source checkpoint query."
                let! data = db.loadAsync geneSortDataMode.Results poolQuery
                let! checkpointGeneration, score = asyncResult {
                    if source.ExecuteFunctionName = ExecuteFunctionNames.soss then
                        let! poolSet = OutputData.asSorterPoolSetSoss data
                        let! pool =
                            match poolSet.SorterPools |> Map.values |> Seq.toArray with
                            | [|pool|] -> Ok pool
                            | _ -> Error "Condense requires exactly one sorter pool in every source checkpoint."
                        let members = pool.SorterPoolMembers |> Seq.toArray
                        do! if members.Length > 0 && (members |> Array.forall (fun m -> m.SorterEvalForSelection.IsSome)) then Ok ()
                            else Error "Condense requires nonempty, fully evaluated source pools."
                        let subset = members.[0].SubsetIdForSelection
                        do! if (members |> Array.forall (fun m -> m.SubsetIdForSelection = subset))
                                   && (commonSubset |> Option.forall ((=) subset)) then Ok ()
                            else Error "Soss condense source selection subsets must match."
                        commonSubset <- Some subset
                        return poolSet.GenerationNumber, float (SorterPool_Soss.getAverageScore measure pool)
                    else
                        let! poolSet = OutputData.asSorterPoolSetStandard data
                        let! pool =
                            match poolSet.SorterPools |> Map.values |> Seq.toArray with
                            | [|pool|] -> Ok pool
                            | _ -> Error "Condense requires exactly one sorter pool in every source checkpoint."
                        let members = pool.SorterPoolMembers |> Seq.toArray
                        do! if members.Length > 0 && (members |> Array.forall (fun m -> m.SorterEval.IsSome)) then Ok ()
                            else Error "Condense requires nonempty, fully evaluated source pools."
                        return poolSet.GenerationNumber, float (SorterPool_Standard.getAverageScore measure pool)
                }
                do! if int checkpointGeneration = generation then Ok ()
                    else Error "Source checkpoint generation does not match the requested interval."
                do! if Double.IsFinite score then Ok () else Error "Condense requires finite average evaluation scores."
                scores.Add(score, %id, candidate)
            // Lower is better. Source execution ID makes tied selections repeatable.
            return scores |> Seq.sortBy (fun (score, id, _) -> score, id)
                          |> Seq.take count |> Seq.map (fun (_, _, candidate) -> candidate) |> Seq.toArray
        }

    /// The callback is also used recursively when appending to a condensed run.
    let select (makeSourceDb: run -> IGeneSortDb) (configured: run) source (candidates: runParameters[]) =
        let spans = configured.ParameterSource |> Option.map snd |> Option.defaultValue []
        match spans |> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey) with
        | None -> candidates
        | Some (_, [value]) ->
            match Int32.TryParse value with
            | true, count when count > 0 ->
                let db = makeSourceDb source
                // Keep all source settings together; only mutation and ancestry vary.
                candidates
                |> Array.groupBy (RunParameterGeneration.sourceCombinationKey source)
                |> Array.collect (fun (combination, group) ->
                    (if count = group.Length then Ok group else
                        rank db configured source count group |> Async.RunSynchronously)
                    |> Result.defaultWith (fun error ->
                        invalidOp (sprintf "Condense source combination %A: %s" combination error)))
            | _ -> invalidArg "SelectedSorterPoolCount" "SelectedSorterPoolCount must be positive."
        | _ -> invalidArg "SelectedSorterPoolCount" "Specify exactly one selection count."
