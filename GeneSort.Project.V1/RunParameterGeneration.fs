namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters

module RunParameterGeneration =
    /// Effective declared options, including settings inherited through continuations.
    let rec effectiveSpans (configured: run) =
        let inherited = configured.ParameterSource |> Option.map (fst >> effectiveSpans) |> Option.defaultValue Map.empty
        configured.SetupParameterSpans @ configured.RunParameterSpans
        |> List.fold (fun spans (key, values) -> Map.add key values spans) inherited

    /// Mutation variants compete within the same source settings, regardless of ancestry.
    let sourceCombinationKey source (parameters: runParameters) =
        effectiveSpans source |> Map.toList
        |> List.filter (fun (key, _) ->
            key <> ParamKeys.mutationModKey && key <> ParamKeys.selectedSorterPoolCountKey
            && key <> ParamKeys.idKey && not (key.StartsWith("Source"))
            && not (List.contains key SetupFileReferences.keys))
        |> List.map (fun (key, _) -> key, Map.tryFind key parameters.ParamMap)

    /// Pair every run span combination with every enhanced setup combination within the same replica.
    let rec generateWithBindingsAndSourceSelection
            (selectSources: run -> run -> runParameters[] -> runParameters[])
            (runQuery: runQueryParamsBuilder)
            (setupQuery: setupQueryParamsBuilder)
            (enhanceRun: runParamEnhancerBuilder)
            (enhanceSetup: setupEnhancerFunction)
            (configured: run) (minReplica: int<replNumber>) (maxReplica: int<replNumber>) : runParameters[] =
        let setupContext = { Run = configured; QueryParamsFromSetupParams = setupQuery }
        [| for replica in int minReplica .. int maxReplica - 1 do
               let repl = UMX.tag<replNumber> replica
               let next = UMX.tag<replNumber> (replica + 1)
               let runs = SimpleRun.makeRunParameters repl next id configured.BaseRun
               let setups =
                   match configured.ParameterSource with
                   | None -> SimpleRun.makeSetupParameters repl next (Seq.map (enhanceSetup setupContext)) configured.BaseRun
                   | Some (source, spans) ->
                       let sourceParameters = generateWithSourceSelection selectSources source repl next
                       let filters = spans |> List.filter (fst >> (<>) ParamKeys.selectedSorterPoolCountKey)
                                           |> List.map (fun (key, values) -> ParamKeys.sourceParameterKey key, values)
                       if spans |> List.exists (fst >> (=) ParamKeys.selectedSorterPoolCountKey) then
                           let requested = filters |> List.find (fst >> (=) ParamKeys.mutationModKey) |> snd
                           let available =
                               effectiveSpans source |> Map.tryFind ParamKeys.mutationModKey
                               |> Option.defaultWith (fun () ->
                                   sourceParameters |> Array.choose (fun rp -> Map.tryFind ParamKeys.mutationModKey rp.ParamMap) |> Array.toList)
                               |> Set.ofList
                           if requested |> List.exists (fun value -> not (Set.contains value available)) then
                               invalidOp (sprintf "Source MutationMod values must be a subset of the available values for replica %d." replica)
                       let selected =
                           sourceParameters
                           |> Array.filter (fun parameters ->
                               filters |> List.forall (fun (key, values) ->
                                   Map.tryFind key parameters.ParamMap |> Option.exists (fun value -> List.contains value values)))
                       if Array.isEmpty selected then invalidOp "No source executions match Source.ParameterSpans."
                       match spans |> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey) with
                       | Some (_, [count]) ->
                           let selectedIds = selected |> Array.map (fun rp -> rp.GetId()) |> Set.ofArray
                           for combination, group in sourceParameters |> Array.groupBy (sourceCombinationKey source) do
                               let matchingCount = group |> Array.filter (fun rp -> Set.contains (rp.GetId()) selectedIds) |> Array.length
                               if matchingCount < int count then
                                   invalidOp (sprintf "Source combination %A has %d matching executions; SelectedSorterPoolCount requires %s." combination matchingCount count)
                       | _ -> ()
                       selectSources configured source selected |> Array.map (fun parameters ->
                           let excluded =
                               [ParamKeys.idKey; ParamKeys.sourceExecutionIdKey
                                ParamKeys.sourcePoolFileIdKey; ParamKeys.sourceTestsFileIdKey
                                ParamKeys.sourceProjectNameKey; ParamKeys.sourceDbNameKey; ParamKeys.sourceRunNameKey; ParamKeys.sourceMutationModKey
                                ParamKeys.sourceGenerationIntervalKey; ParamKeys.generationFirstKey; ParamKeys.selectedSorterPoolCountKey]
                               @ SetupFileReferences.keys
                           let inherited =
                               parameters.ParamMap
                               |> fun map -> List.fold (fun map key -> Map.remove key map) map excluded
                               |> Map.add ParamKeys.sourceExecutionIdKey (string %(parameters.GetId().Value))
                               |> Map.add ParamKeys.sourceMutationModKey (string %(parameters.GetMutationMod().Value))
                           setupParameters.create inherited)
               for setup in setups do
                   let references =
                       if configured.ParameterSource.IsSome
                          || configured.RunEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32 then []
                       else SetupOutputs.referencesFor configured setup
                   let context = runParamEnhancerContext.createForSetup runQuery configured setup
                   for parameters in runs do
                       // A new append total changes capacity, preserving the source pool count.
                       // An explicitly supplied per-pool count still takes precedence.
                       let parameters =
                           if configured.ParameterSource.IsSome
                              && Map.containsKey ParamKeys.sorterCountPerPoolSetKey parameters.ParamMap
                              && not (Map.containsKey ParamKeys.sorterCountPerPoolKey parameters.ParamMap) then
                               match parameters.GetSorterCountPerPoolSet(), setup.GetSorterPoolCount() with
                               | Some total, Some count when int total > 0 && int count > 0 && int total % int count = 0 ->
                                   parameters.WithSorterCountPerPool(Some (UMX.tag (int total / int count)))
                               | _ ->
                                   invalidArg "ExecutionParameterSpans"
                                       "Append SorterCountPerPoolSet must be positive and divisible by the source SorterPoolCount."
                           else parameters
                       let enhanced =
                           parameters
                           |> SetupFileReferences.attach references
                           |> enhanceRun context
                           |> fun enhanced ->
                               if configured.ParameterSource.IsSome then
                                   // Execution spans override source values and enhancer defaults.
                                   // Apply before computing the final identity below.
                                   Map.fold (fun map key value -> Map.add key value map) enhanced.ParamMap parameters.ParamMap
                                   |> runParameters.create
                               else enhanced
                           |> SetupFileReferences.attach references
                       // Resolve identities from the fully enhanced maker inputs. Execution-only
                       // parameters never enter these identities; continuations record imports in Setup.
                       let enhanced =
                           if configured.ParameterSource.IsSome then enhanced
                           else
                               [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""; geneSortDataType.SorterPoolSet ""]
                               |> List.choose (fun output ->
                                   SetupArtifactQueries.tryCreate configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName enhanced output
                                   |> Option.map (fun query -> SetupFileReferences.keyForOutputType output, string query.Id))
                               |> List.fold (fun (rp: runParameters) (key, value) -> runParameters.create (rp.ParamMap.Add(key, value))) enhanced
                       let query =
                           runQuery enhanced (geneSortDataType.Run configured.RunName)
                           |> Option.defaultWith (fun () -> invalidOp (sprintf "Run query '%s' could not build an identity for the enhanced parameters.\n%s"
                                                                           configured.RunQueryCatalogName (RunParameters.reportKvps enhanced)))
                       yield enhanced.WithId(Some query.Id) |]

    /// Resolve catalog functions for callers that do not have a run host.
    and generateWithSourceSelection selectSources (configured: run) (minReplica: int<replNumber>) (maxReplica: int<replNumber>) : runParameters[] =
        ProjectCatalogs.registerAll ()
        let runQuery = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        let setupQuery = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
        let enhanceRun = RunParamEnhancerBuilders.get configured.RunEnhancerFunctionName
        let enhanceSetup = SetupEnhancerCatalog.get configured.SetupEnhancerFunctionName
        generateWithBindingsAndSourceSelection selectSources runQuery setupQuery enhanceRun enhanceSetup configured minReplica maxReplica

    let private unrankedSources (configured: run) source candidates =
        let spans = configured.ParameterSource |> Option.map snd |> Option.defaultValue []
        match spans |> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey) with
        | Some (_, [count]) ->
            for _, group in candidates |> Array.groupBy (sourceCombinationKey source) do
                if group.Length <> int count then
                    invalidOp "Ranked source selection requires source checkpoint access. Use runHost.MakeRunParameters."
        | _ -> ()
        candidates

    let generateWithBindings runQuery setupQuery enhanceRun enhanceSetup configured minReplica maxReplica =
        generateWithBindingsAndSourceSelection unrankedSources runQuery setupQuery enhanceRun enhanceSetup configured minReplica maxReplica

    let generate configured minReplica maxReplica =
        generateWithSourceSelection unrankedSources configured minReplica maxReplica
