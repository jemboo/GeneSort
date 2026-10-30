namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters

module RunParameterGeneration =
    /// SortableTests has one sweep. The reference is retained solely because it
    /// participates in historical Run/RunParameters IDs; no setup is performed.
    let private generateDirect runQuery enhanceRun (configured: run) minReplica maxReplica =
        let context = runParamEnhancerContext.create runQuery configured
        SimpleRun.makeRunParameters minReplica maxReplica id configured.BaseRun
        |> Array.map (fun parameters ->
            let references = SetupOutputs.referencesFor configured (setupParameters.create parameters.ParamMap)
            let enhanced = parameters |> SetupFileReferences.attach references |> enhanceRun context
            let query = runQuery enhanced (geneSortDataType.Run configured.RunName) |> Option.get
            enhanced.WithId(Some query.Id))

    let validateRoles (configured: run) =
        configured.RunParameterSpans |> List.choose (fun (key, _) ->
            if configured.HasSetup && RunParameterRoles.isFixed (Run.parameterRole configured key)
               && not (key.StartsWith("Source")) then
                Some (validationIssue.create ("Execution." + key) "This value is fixed by Setup. Change it in Setup and prepare the inputs again.")
            else None) |> List.toArray

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
        if configured.IsDirectRun then generateDirect runQuery enhanceRun configured minReplica maxReplica
        else
            validateRoles configured |> ParameterValidation.ensureValid "configured"
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
                           let enhanced =
                               parameters
                               |> fun parameters ->
                                   // All executors receive their fixed setup values, including
                                   // older enhancers that used to rely on duplicated spans.
                                   (SetupFileReferences.withoutReferences setup).ParamMap
                                   |> Map.fold (fun map key value ->
                                       if RunParameterRoles.isFixed (Run.parameterRole configured key) then Map.add key value map
                                       else map) parameters.ParamMap
                                   |> fun map ->
                                       if effectiveSpans configured |> Map.tryFind ParamKeys.selectedSorterCountPerPoolKey
                                          |> Option.exists (function [value] -> System.String.Equals(value, "All", System.StringComparison.OrdinalIgnoreCase) | _ -> false) then
                                           Map.add ParamKeys.selectedSorterCountPerPoolKey "All" map
                                       else map
                                   |> ParameterValidation.resolveSelection
                                   |> runParameters.create
                               |> SetupFileReferences.attach references
                               |> enhanceRun context
                               |> fun enhanced ->
                                   if configured.ParameterSource.IsSome then
                                       // Execution spans override source values and enhancer defaults.
                                       // Apply before computing the final identity below.
                                       Map.fold (fun map key value -> Map.add key value map) enhanced.ParamMap parameters.ParamMap
                                       |> ParameterValidation.resolveSelection
                                       |> runParameters.create
                                   else enhanced
                               |> SetupFileReferences.attach references
                           for KeyValue(key, value) in setup.ParamMap do
                               if RunParameterRoles.isFixed (Run.parameterRole configured key)
                                  && Map.tryFind key enhanced.ParamMap <> Some value then
                                   invalidArg "RunEnhancerFunctionName"
                                       (sprintf "Execution changed fixed Setup value '%s'. Its enhancer must preserve the prepared value." key)
                           // Resolve identities from the fully enhanced maker inputs. Execution-only
                           // parameters never enter these identities; continuations record imports in Setup.
                           let enhanced =
                               if configured.ParameterSource.IsSome then enhanced
                               else
                                   SetupInputReferences.recordFiles (SetupOutputs.generatedFiles configured enhanced) enhanced
                           let query =
                               runQuery enhanced (geneSortDataType.Run configured.RunName)
                               |> Option.defaultWith (fun () -> invalidOp (sprintf "Run query '%s' could not build an identity for the enhanced parameters.\n%s"
                                                                               configured.RunQueryCatalogName (RunParameters.reportKvps enhanced)))
                           yield enhanced.WithId(Some query.Id).WithSetupCompleted(Some false) |]

    /// Resolve catalog functions for callers that do not have a run host.
    and generateWithSourceSelection selectSources (configured: run) (minReplica: int<replNumber>) (maxReplica: int<replNumber>) : runParameters[] =
        ProjectCatalogs.registerAll ()
        let runQuery = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        let enhanceRun = RunParamEnhancerBuilders.get configured.RunEnhancerFunctionName
        if configured.IsDirectRun then generateDirect runQuery enhanceRun configured minReplica maxReplica
        else
            let setupQuery = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
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

    let generateWithBindings runQuery setupQuery enhanceRun enhanceSetup (configured: run) minReplica maxReplica =
        if configured.IsDirectRun then generateDirect runQuery enhanceRun configured minReplica maxReplica
        else generateWithBindingsAndSourceSelection unrankedSources runQuery setupQuery enhanceRun enhanceSetup configured minReplica maxReplica

    let generate configured minReplica maxReplica =
        generateWithSourceSelection unrankedSources configured minReplica maxReplica
