namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

/// Identities for immutable setup artifacts. Only inputs consumed by their makers belong here.
/// Increment the recipe version when a maker changes the meaning of those inputs.
module SetupArtifactQueries =
    let private evaluation = [QueryCatalogNamesSetup.sorterEvalStandard; QueryCatalogNamesSetup.sorterEvalMerge; QueryCatalogNamesSetup.sorterEvalPrefix]
    let private mutation = [QueryCatalogNamesSetup.sorterMutateStandard; QueryCatalogNamesSetup.sorterMutateMerge; QueryCatalogNamesSetup.sorterMutatePrefix]
    let private evolution = [QueryCatalogNamesSetup.sorterSgdUf6MutationRate; QueryCatalogNamesSetup.sorterSgdMsrsOrthoPara
                             QueryCatalogNamesSetup.sorterSgdMssiOrthoPara; QueryCatalogNamesSetup.sorterSgdMsufStandard
                             QueryCatalogNamesSetup.sorterSgdMsufSoss; QueryCatalogNamesSetup.sossMsuf32Seed
                             QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate; QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp]

    let private modelKeys = [ParamKeys.sortingWidthKey; ParamKeys.rngTypeKey; ParamKeys.simpleSorterModelTypeKey
                             ParamKeys.excludeSelfCeKey; ParamKeys.aMaxKey; ParamKeys.activationProbabilityKey]

    let tryCreate catalog project database (rp: runParameters) outputType =
        let isEval, isMutate, isEvolution = List.contains catalog evaluation, List.contains catalog mutation, List.contains catalog evolution
        if not (isEval || isMutate || isEvolution) then None
        else
            let width =
                match rp.GetPrefixLibId(), rp.GetMergeLibId() with
                | Some prefix, _ -> Some prefix.SortingWidth
                | _, Some merge -> Some merge.SortingWidth
                | _ -> rp.GetSortingWidth()
            let parameters =
                rp.WithSortingWidth(width)
                  .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                  .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
            let libraryKeys =
                if rp.GetPrefixLibId().IsSome then [ParamKeys.prefixLibIdKey]
                elif rp.GetMergeLibId().IsSome then [ParamKeys.mergeLibIdKey]
                else [ParamKeys.sortingWidthKey]
            let make recipe replica required keys =
                let properties =
                    ("SetupRecipe", recipe + "-v1") ::
                    (keys |> List.distinct |> List.choose (fun key -> parameters.ParamMap.TryFind key |> Option.map (fun value -> key, value)))
                if required |> List.forall parameters.ParamMap.ContainsKey then
                    Some (queryParams.create database project replica None outputType (List.toArray properties))
                else None
            let requiredModelKeys = [ParamKeys.sortingWidthKey; ParamKeys.rngTypeKey; ParamKeys.simpleSorterModelTypeKey]
            let testsKeys = libraryKeys @ [ParamKeys.sortableDataFormatKey]
            let testsRecipe =
                if rp.GetPrefixLibId().IsSome then "prefix-tests"
                elif rp.GetMergeLibId().IsSome then
                    // The evolution merge maker omits the prefix; evaluation includes it.
                    if isEvolution then "merge-tests-without-prefix" else "merge-tests-with-prefix"
                else "standard-tests"
            match outputType with
            | geneSortDataType.SortableTestsWithPrefix _ -> make testsRecipe None testsKeys testsKeys
            | geneSortDataType.SorterModelSet _ when isEval ->
                make "uniform-models" (rp.GetRepl()) (requiredModelKeys @ [ParamKeys.replKey; ParamKeys.sorterCountKey]) (modelKeys @ [ParamKeys.sorterCountKey])
            | geneSortDataType.SorterModelSet _ when isMutate ->
                let replica = if libraryKeys = [ParamKeys.sortingWidthKey] then None else rp.GetRepl()
                make "mutant-models" replica (requiredModelKeys @ libraryKeys @ [ParamKeys.sorterChildCountKey; ParamKeys.mutationModKey; ParamKeys.mutatorParamsKey])
                    (modelKeys @ libraryKeys @ [ParamKeys.seedSorterPoolSelectionTypeKey; ParamKeys.sorterEvalMeasureKey;
                                               ParamKeys.sorterChildCountKey; ParamKeys.mutatorParamsKey; ParamKeys.modificationRateKey;
                                               ParamKeys.mutationModKey; ParamKeys.activatorMutationRateKey])
            | geneSortDataType.SorterPoolSet _ when isEvolution ->
                let family = if catalog = QueryCatalogNamesSetup.sorterSgdMsufSoss || catalog = QueryCatalogNamesSetup.sossMsuf32Seed then "soss" else "standard"
                let controlled = rp.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey
                let sourceKeys = if controlled then [ParamKeys.controlledSeedAMaxKey] else libraryKeys @ [ParamKeys.seedSorterPoolSelectionTypeKey; ParamKeys.sorterEvalMeasureInitialKey]
                let replica = if controlled || libraryKeys <> [ParamKeys.sortingWidthKey] then rp.GetRepl() else None
                make (family + "-seed-pool-" + testsRecipe) replica (requiredModelKeys @ [ParamKeys.sorterPoolCountKey; ParamKeys.sorterCountPerPoolKey; ParamKeys.mutationModKey])
                    (modelKeys @ sourceKeys @ testsKeys @ [ParamKeys.sorterPoolCountKey; ParamKeys.sorterCountPerPoolKey;
                                                         ParamKeys.mutationModKey; ParamKeys.sorterEvalMeasureKey])
            | _ -> None
