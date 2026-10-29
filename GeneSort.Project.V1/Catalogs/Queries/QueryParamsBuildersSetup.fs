namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters

/// Setup constructors return the actual stored-file identities directly.
module QueryParamsBuildersSetup =

    module SorterSgd =

        module Uf6MutationRate =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }


            /// Constructs only the Run address for SorterSgd.Uf6MutationRate.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.Uf6MutationRate.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.Uf6MutationRate.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.Uf6MutationRate.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.Uf6MutationRate.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Uf6MutationRate.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterSgd.Uf6MutationRate.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.Uf6MutationRate.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.Uf6MutationRate.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    let! seedModR = rp.GetSeedModificationRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sorterEvalMeasureInitialKey, semi |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module MsrsOrthoPara =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }


            /// Constructs only the Run address for SorterSgd.MsrsOrthoPara.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.MsrsOrthoPara.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.MsrsOrthoPara.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.MsrsOrthoPara.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.MsrsOrthoPara.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.MsrsOrthoPara.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterSgd.MsrsOrthoPara.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.MsrsOrthoPara.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.MsrsOrthoPara.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! para = rp.GetParaRate()
                    let! self = rp.GetSelfSymRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module MssiOrthoPara =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }


            /// Constructs only the Run address for SorterSgd.MssiOrthoPara.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.MssiOrthoPara.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.MssiOrthoPara.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.MssiOrthoPara.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.MssiOrthoPara.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.MssiOrthoPara.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterSgd.MssiOrthoPara.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.MssiOrthoPara.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.MssiOrthoPara.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Msuf =

            module Standard =
                let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let width = rp.GetSortingWidth()
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                        return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield "SetupRecipe", "standard-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                            |]
                    }

                let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let width = rp.GetSortingWidth()
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                    yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureInitialKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let width = rp.GetSortingWidth()
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetMergeLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                        return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield "SetupRecipe", "merge-tests-without-prefix-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                    yield ParamKeys.mergeLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                            |]
                    }

                let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetMergeLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                    yield ParamKeys.mergeLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                    yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureInitialKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetMergeLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                    yield ParamKeys.mergeLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetPrefixLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                        return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield "SetupRecipe", "prefix-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                    yield ParamKeys.prefixLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                            |]
                    }

                let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetPrefixLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                    yield ParamKeys.prefixLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                    yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureInitialKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetPrefixLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                    yield ParamKeys.prefixLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }


                /// Constructs only the Run address for SorterSgd.Msuf.Standard.
                let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the RunParameters address for SorterSgd.Msuf.Standard.
                let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSet address for SorterSgd.Msuf.Standard.
                let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                    match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                    | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                    | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                    | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                    | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                    | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                    | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

                /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msuf.Standard.
                let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSet address for SorterSgd.Msuf.Standard.
                let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterModelSet address for SorterSgd.Msuf.Standard.
                let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTests address for SorterSgd.Msuf.Standard.
                let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msuf.Standard.
                let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                    match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                    | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                    | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                    | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

                /// Constructs only the SorterSetEval address for SorterSgd.Msuf.Standard.
                let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msuf.Standard.
                let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msuf.Standard.
                let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the TextReport address for SorterSgd.Msuf.Standard.
                let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Catalog adapter: selection only; each constructor owns its fields.
                let makeQueryParams projectName dbName parameters = function
                    | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                    | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                    | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            module Soss =
                let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let width = rp.GetSortingWidth()
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                        return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield "SetupRecipe", "standard-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                            |]
                    }

                let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let width = rp.GetSortingWidth()
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "soss-seed-pool-standard-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                    yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureInitialKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let width = rp.GetSortingWidth()
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "soss-seed-pool-standard-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetMergeLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                        return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield "SetupRecipe", "merge-tests-without-prefix-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                    yield ParamKeys.mergeLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                            |]
                    }

                let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetMergeLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "soss-seed-pool-merge-tests-without-prefix-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                    yield ParamKeys.mergeLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                    yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureInitialKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetMergeLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "soss-seed-pool-merge-tests-without-prefix-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                    yield ParamKeys.mergeLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetPrefixLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                        return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                            [|
                                yield "SetupRecipe", "prefix-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                    yield ParamKeys.prefixLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                            |]
                    }

                let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetPrefixLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "soss-seed-pool-prefix-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                    yield ParamKeys.prefixLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                    yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureInitialKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }

                let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                    maybe {
                        let! library = rp.GetPrefixLibId()
                        let width = Some library.SortingWidth
                        let parameters =
                            rp.WithSortingWidth(width)
                              .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                              .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                        let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                        return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                            [|
                                yield "SetupRecipe", "soss-seed-pool-prefix-tests-v1"
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                    yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                    yield ParamKeys.sortingWidthKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                    yield ParamKeys.rngTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                    yield ParamKeys.simpleSorterModelTypeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                    yield ParamKeys.excludeSelfCeKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                    yield ParamKeys.prefixLibIdKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                    yield ParamKeys.sortableDataFormatKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                    yield ParamKeys.sorterPoolCountKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                    yield ParamKeys.sorterCountPerPoolKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                    yield ParamKeys.mutationModKey, value
                                for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                    yield ParamKeys.sorterEvalMeasureKey, value
                            |]
                    }


                /// Constructs only the Run address for SorterSgd.Msuf.Soss.
                let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the RunParameters address for SorterSgd.Msuf.Soss.
                let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSet address for SorterSgd.Msuf.Soss.
                let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                    match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                    | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                    | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                    | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                    | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                    | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                    | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

                /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msuf.Soss.
                let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterSet address for SorterSgd.Msuf.Soss.
                let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterModelSet address for SorterSgd.Msuf.Soss.
                let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTests address for SorterSgd.Msuf.Soss.
                let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msuf.Soss.
                let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                    match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                    | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                    | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                    | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

                /// Constructs only the SorterSetEval address for SorterSgd.Msuf.Soss.
                let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msuf.Soss.
                let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msuf.Soss.
                let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Constructs only the TextReport address for SorterSgd.Msuf.Soss.
                let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                    maybe {
                        let! repl = rp.GetRepl()
                        let! codeMod = rp.GetCodeModKey()
                        let! curGen = rp.GetGenerationCurrent()
                        let! scPP = rp.GetSorterCountPerPool()
                        let! spc = rp.GetSorterPoolCount()
                        let! para = rp.GetParaRate()
                        let! self = rp.GetSelfSymRate()
                        let! seedModR = rp.GetSeedModificationRate()
                        let! modR = rp.GetModificationRate()
                        let! mmod = rp.GetMutationMod()
                        let! mutVar = rp.GetMutatorVariant()
                        let! seedSoss = rp.GetSeedSoss()
                        let! partCt = rp.GetPartitionCount()
                        let! genSoss = rp.GetGenerationsPerSubset()
                        let! genMerged = rp.GetGenerationsPerMergedSet()
                        return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                            [|
                                yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                                yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                                yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                                yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                                yield ParamKeys.selfSymRateKey, (Some self) |> SelfSymRate.toString
                                yield ParamKeys.seedModificationRateKey, (Some seedModR) |> SeedModificationRate.toString
                                yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                                yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                                yield ParamKeys.mutatorVariantKey, mutVar |> MutatorVariant.toString
                                yield ParamKeys.seedSossKey, seedSoss |> string
                                yield ParamKeys.partitionCountKey, string %partCt
                                yield ParamKeys.generationsPerSubsetKey, string %genSoss
                                yield ParamKeys.generationsPerMergedSetKey, (Some genMerged) |> GenerationsPerSubset.toString
                                for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                    yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                                for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                    yield ParamKeys.aMaxKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                    yield ParamKeys.activationProbabilityKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                    yield ParamKeys.activatorMutationRateKey, value
                                for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                    yield ParamKeys.controlledSeedAMaxKey, value
                            |]
                    }

                /// Catalog adapter: selection only; each constructor owns its fields.
                let makeQueryParams projectName dbName parameters = function
                    | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                    | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                    | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                    | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                    | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Msrs32MutationRate =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }


            /// Constructs only the Run address for SorterSgd.Msrs32MutationRate.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.Msrs32MutationRate.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.Msrs32MutationRate.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.Msrs32MutationRate.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.Msrs32MutationRate.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.Msrs32MutationRate.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterSgd.Msrs32MutationRate.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.Msrs32MutationRate.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.Msrs32MutationRate.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! codeMod = rp.GetCodeModKey()
                    let! curGen = rp.GetGenerationCurrent()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! modR = rp.GetModificationRate()
                    let! mmod = rp.GetMutationMod()
                    return queryParams.create dbName projectName (Some repl) (Some curGen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.codeModKey, (Some codeMod) |> CodeModKey.toString
                            yield ParamKeys.sorterCountPerPoolKey, (Some scPP) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.mutationModKey, (Some %mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module MsrsPoolModComp =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-merge-tests-without-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureInitialKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }

            let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield "SetupRecipe", "standard-seed-pool-prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                                yield ParamKeys.sorterPoolCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                                yield ParamKeys.sorterCountPerPoolKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                        |]
                }


            /// Constructs only the Run address for SorterSgd.MsrsPoolModComp.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterSgd.MsrsPoolModComp.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
                | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
                | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
                | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterPoolSetSummarySet address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterSgd.MsrsPoolModComp.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterSgd.MsrsPoolModComp.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SorterSgd.MsrsPoolModComp.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterSgd.MsrsPoolModComp.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterSgd.MsrsPoolModComp.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterSgd.MsrsPoolModComp.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterSgd.MsrsPoolModComp.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! gen = rp.GetGenerationCurrent()
                    let! scpp = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! seedSelection = rp.GetSeedSorterPoolSelectionType()
                    let! mmod = rp.GetMutationMod()
                    let! modR = rp.GetModificationRate()
                    let! para = rp.GetParaRate()
                    let! selfSym = rp.GetSelfSymRate()
                    let! selectedCount = rp.GetSelectedSorterCountPerPool()
                    return queryParams.create dbName projectName (Some repl) (Some gen) (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.sorterCountPerPoolKey, (Some scpp) |> SorterCountPerPool.toString
                            yield ParamKeys.sorterPoolCountKey, (Some spc) |> SorterPoolCount.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, seedSelection |> SorterSelectionType.toString
                            yield ParamKeys.modificationRateKey, (Some modR) |> ModificationRate.toString
                            yield ParamKeys.paraRateKey, (Some para) |> ParaRate.toString
                            yield ParamKeys.selfSymRateKey, (Some selfSym) |> SelfSymRate.toString
                            yield ParamKeys.selectedSorterCountPerPoolKey, (Some selectedCount) |> SorterCountPerPool.toString
                            yield ParamKeys.mutationModKey, (Some mmod) |> MutationMod.toString
                            for frequency in rp.GetAdjustCeLengthFrequency() |> Option.toList do
                                yield ParamKeys.adjustCeLengthFrequencyKey, string %frequency
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


    module SortableTests =

        module Merge =
            /// Constructs only the Run address for SortableTests.Merge.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SortableTests.Merge.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SortableTests.Merge.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SortableTests.Merge.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SortableTests.Merge.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SortableTests.Merge.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SortableTests.Merge.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SortableTests.Merge.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SortableTests.Merge.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SortableTests.Merge.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SortableTests.Merge.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SortableTests.Merge.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl (mergeLib: mergeLibId) (sortableDataFormat: sortableDataFormat) outDt =
                queryParams.create dbName projectName (Some repl) None outDt
                    [|
                        (ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLib)
                        (ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: setupParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeLibraryQueryParams projectName dbName repl mergeLib sdf odt
                }


        module Prefix =
            /// Constructs only the Run address for SortableTests.Prefix.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SortableTests.Prefix.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SortableTests.Prefix.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SortableTests.Prefix.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SortableTests.Prefix.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SortableTests.Prefix.
            let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTests address for SortableTests.Prefix.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SortableTests.Prefix.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSetEval address for SortableTests.Prefix.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SortableTests.Prefix.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SortableTests.Prefix.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SortableTests.Prefix.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib
                            yield ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sdf
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl (prefixLib: prefixLibId) (sortableDataFormat: sortableDataFormat) outDt =
                queryParams.create dbName projectName (Some repl) None outDt
                    [|
                        (ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLib)
                        (ParamKeys.sortableDataFormatKey, SortableDataFormat.toString sortableDataFormat)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: setupParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeLibraryQueryParams projectName dbName repl prefixLib sdf odt
                }


    module SorterEval =

        module Standard =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-with-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }


            /// Constructs only the Run address for SorterEval.Standard.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterEval.Standard.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterEval.Standard.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterEval.Standard.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterEval.Standard.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterEval.Standard.
            let makeSorterModelSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixModelsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeModelsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardModelsQueryParams projectName dbName parameters outputName

            /// Constructs only the SortableTests address for SorterEval.Standard.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterEval.Standard.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterEval.Standard.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterEval.Standard.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterEval.Standard.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterEval.Standard.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl rng sortingWidth simpleModel evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.sortingWidthKey, (Some sortingWidth) |> SortingWidth.toString)
                        (ParamKeys.simpleSorterModelTypeKey, simpleModel |> SimpleSorterModelType.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: setupParameters) odt =
                maybe {
                    let! repl = rp.GetRepl()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    return makeLibraryQueryParams projectName dbName repl rng width model evalType odt
                }


        module Merge =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-with-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }


            /// Constructs only the Run address for SorterEval.Merge.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterEval.Merge.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterEval.Merge.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterEval.Merge.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterEval.Merge.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterEval.Merge.
            let makeSorterModelSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixModelsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeModelsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardModelsQueryParams projectName dbName parameters outputName

            /// Constructs only the SortableTests address for SorterEval.Merge.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterEval.Merge.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterEval.Merge.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterEval.Merge.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterEval.Merge.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterEval.Merge.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl rng mergeLibId model dataFormat evalType odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.mergeLibIdKey, MergeLibId.toString mergeLibId)
                        (ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                        (ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString)
                        (ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: setupParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! mergeLibId = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    return makeLibraryQueryParams projectName dbName repl rng mergeLibId model dataFormat evalType odt
                }


        module Prefix =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-with-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.replKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "uniform-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterCountKey |> Option.toList do
                                yield ParamKeys.sorterCountKey, value
                        |]
                }


            /// Constructs only the Run address for SorterEval.Prefix.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterEval.Prefix.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterEval.Prefix.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterEval.Prefix.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterEval.Prefix.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterEval.Prefix.
            let makeSorterModelSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixModelsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeModelsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardModelsQueryParams projectName dbName parameters outputName

            /// Constructs only the SortableTests address for SorterEval.Prefix.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterEval.Prefix.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterEval.Prefix.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterEval.Prefix.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterEval.Prefix.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterEval.Prefix.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sorterCountKey, string (int sorterCount)
                            yield ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


            let makeLibraryQueryParams projectName dbName repl rng prefixLibId model dataFormat (sorterCount: int<sorterCount>) odt =
                queryParams.create dbName projectName (Some repl) None odt
                    [|
                        (ParamKeys.rngTypeKey, rng |> RngType.toString)
                        (ParamKeys.prefixLibIdKey, PrefixLibId.toString prefixLibId)
                        (ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString)
                        (ParamKeys.sorterCountKey, string (int sorterCount))
                        (ParamKeys.sortableDataFormatKey, dataFormat |> SortableDataFormat.toString)
                    |]

            let libraryQueryFromParameters projectName dbName (rp: setupParameters) odt =
                maybe {
                    let! rng = rp.GetRngType()
                    let! prefixLibId = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! dataFormat = rp.GetSortableDataFormat()
                    let! sorterCount = rp.GetSorterCount()
                    let! repl = rp.GetRepl()
                    return makeLibraryQueryParams projectName dbName repl rng prefixLibId model dataFormat sorterCount odt
                }


    module SorterMutate =

        module Standard =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-with-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }


            /// Constructs only the Run address for SorterMutate.Standard.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterMutate.Standard.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterMutate.Standard.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterMutate.Standard.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterMutate.Standard.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterMutate.Standard.
            let makeSorterModelSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixModelsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeModelsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardModelsQueryParams projectName dbName parameters outputName

            /// Constructs only the SortableTests address for SorterMutate.Standard.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterMutate.Standard.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterMutate.Standard.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterMutate.Standard.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterMutate.Standard.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterMutate.Standard.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! repl = rp.GetRepl()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! width = rp.GetSortingWidth()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.sortingWidthKey, (Some width) |> SortingWidth.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Merge =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-with-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }


            /// Constructs only the Run address for SorterMutate.Merge.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterMutate.Merge.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterMutate.Merge.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterMutate.Merge.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterMutate.Merge.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterMutate.Merge.
            let makeSorterModelSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixModelsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeModelsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardModelsQueryParams projectName dbName parameters outputName

            /// Constructs only the SortableTests address for SorterMutate.Merge.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterMutate.Merge.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterMutate.Merge.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterMutate.Merge.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterMutate.Merge.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterMutate.Merge.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mergeLib = rp.GetMergeLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.mergeLibIdKey, mergeLib |> MergeLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


        module Prefix =
            let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "standard-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeStandardModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let width = rp.GetSortingWidth()
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (None) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }

            let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "merge-tests-with-prefix-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makeMergeModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetMergeLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                                yield ParamKeys.mergeLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }

            let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                    return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                        [|
                            yield "SetupRecipe", "prefix-tests-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                                yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                                yield ParamKeys.sortableDataFormatKey, value
                        |]
                }

            let makePrefixModelsQueryParams project database (rp: setupParameters) outputName =
                maybe {
                    let! library = rp.GetPrefixLibId()
                    let width = Some library.SortingWidth
                    let parameters =
                        rp.WithSortingWidth(width)
                          .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                          .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                    let! _ = parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey
                    return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterModelSet outputName)
                        [|
                            yield "SetupRecipe", "mutant-models-v1"
                            for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                                yield ParamKeys.sortingWidthKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                                yield ParamKeys.rngTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                                yield ParamKeys.simpleSorterModelTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                                yield ParamKeys.excludeSelfCeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                                yield ParamKeys.prefixLibIdKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                                yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                                yield ParamKeys.sorterEvalMeasureKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.sorterChildCountKey |> Option.toList do
                                yield ParamKeys.sorterChildCountKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutatorParamsKey |> Option.toList do
                                yield ParamKeys.mutatorParamsKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.modificationRateKey |> Option.toList do
                                yield ParamKeys.modificationRateKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                                yield ParamKeys.mutationModKey, value
                            for value in parameters.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                        |]
                }


            /// Constructs only the Run address for SorterMutate.Prefix.
            let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.Run outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the RunParameters address for SorterMutate.Prefix.
            let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.RunParameters outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSet address for SorterMutate.Prefix.
            let makeSorterPoolSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetSummarySet address for SorterMutate.Prefix.
            let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetSummarySet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterSet address for SorterMutate.Prefix.
            let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSet outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterModelSet address for SorterMutate.Prefix.
            let makeSorterModelSetQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixModelsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeModelsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardModelsQueryParams projectName dbName parameters outputName

            /// Constructs only the SortableTests address for SorterMutate.Prefix.
            let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SortableTests outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SortableTestsWithPrefix address for SorterMutate.Prefix.
            let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
                match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
                | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
                | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
                | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

            /// Constructs only the SorterSetEval address for SorterMutate.Prefix.
            let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterSetEval outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolBinsSetSeries address for SorterMutate.Prefix.
            let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolBinsSetSeries outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the SorterPoolSetHistory address for SorterMutate.Prefix.
            let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.SorterPoolSetHistory outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Constructs only the TextReport address for SorterMutate.Prefix.
            let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
                maybe {
                    let! rng = rp.GetRngType()
                    let! selection = rp.GetSeedSorterPoolSelectionType()
                    let! measure = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! prefixLib = rp.GetPrefixLibId()
                    let! model = rp.GetSimpleSorterModelType()
                    let! format = rp.GetSortableDataFormat()
                    let! evalType = rp.GetSorterEvalType()
                    let! mutatorParams = rp.GetMutatorParams()
                    let! modRate = rp.GetModificationRate()
                    return queryParams.create dbName projectName (Some repl) None (geneSortDataType.TextReport outputName)
                        [|
                            yield ParamKeys.rngTypeKey, rng |> RngType.toString
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, selection |> SorterSelectionType.toString
                            yield ParamKeys.sorterEvalMeasureKey, measure |> SorterEvalMeasure.toCompactString
                            yield ParamKeys.prefixLibIdKey, prefixLib |> PrefixLibId.toString
                            yield ParamKeys.simpleSorterModelTypeKey, model |> SimpleSorterModelType.toString
                            yield ParamKeys.sortableDataFormatKey, format |> SortableDataFormat.toString
                            yield ParamKeys.sorterEvalTypeKey, evalType |> SorterEvalType.toString
                            yield ParamKeys.mutatorParamsKey, mutatorParams |> MutatorParams.toString
                            yield ParamKeys.modificationRateKey, (Some modRate) |> ModificationRate.toString
                            for value in rp.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                                yield ParamKeys.aMaxKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                                yield ParamKeys.activationProbabilityKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.activatorMutationRateKey |> Option.toList do
                                yield ParamKeys.activatorMutationRateKey, value
                            for value in rp.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                                yield ParamKeys.controlledSeedAMaxKey, value
                        |]
                }

            /// Catalog adapter: selection only; each constructor owns its fields.
            let makeQueryParams projectName dbName parameters = function
                | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
                | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
                | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
                | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
                | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
                | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
                | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


    module SossMsuf32Seed =
        let makeStandardTestsQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let width = rp.GetSortingWidth()
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                    [|
                        yield "SetupRecipe", "standard-tests-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                    |]
            }

        let makeStandardSeedPoolQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let width = rp.GetSortingWidth()
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                return queryParams.create database project (None) None (geneSortDataType.SorterPoolSet outputName)
                    [|
                        yield "SetupRecipe", "soss-seed-pool-standard-tests-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                            yield ParamKeys.rngTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                            yield ParamKeys.simpleSorterModelTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                            yield ParamKeys.excludeSelfCeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                            yield ParamKeys.aMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                            yield ParamKeys.activationProbabilityKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureInitialKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                            yield ParamKeys.sorterPoolCountKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                            yield ParamKeys.sorterCountPerPoolKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                            yield ParamKeys.mutationModKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureKey, value
                    |]
            }

        let makeStandardControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let width = rp.GetSortingWidth()
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                    [|
                        yield "SetupRecipe", "soss-seed-pool-standard-tests-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                            yield ParamKeys.rngTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                            yield ParamKeys.simpleSorterModelTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                            yield ParamKeys.excludeSelfCeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                            yield ParamKeys.aMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                            yield ParamKeys.activationProbabilityKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                            yield ParamKeys.controlledSeedAMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                            yield ParamKeys.sorterPoolCountKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                            yield ParamKeys.sorterCountPerPoolKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                            yield ParamKeys.mutationModKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureKey, value
                    |]
            }

        let makeMergeTestsQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let! library = rp.GetMergeLibId()
                let width = Some library.SortingWidth
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                    [|
                        yield "SetupRecipe", "merge-tests-without-prefix-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                            yield ParamKeys.mergeLibIdKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                    |]
            }

        let makeMergeSeedPoolQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let! library = rp.GetMergeLibId()
                let width = Some library.SortingWidth
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                    [|
                        yield "SetupRecipe", "soss-seed-pool-merge-tests-without-prefix-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                            yield ParamKeys.rngTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                            yield ParamKeys.simpleSorterModelTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                            yield ParamKeys.excludeSelfCeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                            yield ParamKeys.aMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                            yield ParamKeys.activationProbabilityKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                            yield ParamKeys.mergeLibIdKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureInitialKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                            yield ParamKeys.sorterPoolCountKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                            yield ParamKeys.sorterCountPerPoolKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                            yield ParamKeys.mutationModKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureKey, value
                    |]
            }

        let makeMergeControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let! library = rp.GetMergeLibId()
                let width = Some library.SortingWidth
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                    [|
                        yield "SetupRecipe", "soss-seed-pool-merge-tests-without-prefix-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                            yield ParamKeys.rngTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                            yield ParamKeys.simpleSorterModelTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                            yield ParamKeys.excludeSelfCeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                            yield ParamKeys.aMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                            yield ParamKeys.activationProbabilityKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                            yield ParamKeys.controlledSeedAMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mergeLibIdKey |> Option.toList do
                            yield ParamKeys.mergeLibIdKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                            yield ParamKeys.sorterPoolCountKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                            yield ParamKeys.sorterCountPerPoolKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                            yield ParamKeys.mutationModKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureKey, value
                    |]
            }

        let makePrefixTestsQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let! library = rp.GetPrefixLibId()
                let width = Some library.SortingWidth
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey
                return queryParams.create database project (None) None (geneSortDataType.SortableTestsWithPrefix outputName)
                    [|
                        yield "SetupRecipe", "prefix-tests-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                            yield ParamKeys.prefixLibIdKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                    |]
            }

        let makePrefixSeedPoolQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let! library = rp.GetPrefixLibId()
                let width = Some library.SortingWidth
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                    [|
                        yield "SetupRecipe", "soss-seed-pool-prefix-tests-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                            yield ParamKeys.rngTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                            yield ParamKeys.simpleSorterModelTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                            yield ParamKeys.excludeSelfCeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                            yield ParamKeys.aMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                            yield ParamKeys.activationProbabilityKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                            yield ParamKeys.prefixLibIdKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.seedSorterPoolSelectionTypeKey |> Option.toList do
                            yield ParamKeys.seedSorterPoolSelectionTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureInitialKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureInitialKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                            yield ParamKeys.sorterPoolCountKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                            yield ParamKeys.sorterCountPerPoolKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                            yield ParamKeys.mutationModKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureKey, value
                    |]
            }

        let makePrefixControlledSeedPoolQueryParams project database (rp: setupParameters) outputName =
            maybe {
                let! library = rp.GetPrefixLibId()
                let width = Some library.SortingWidth
                let parameters =
                    rp.WithSortingWidth(width)
                      .WithExcludeSelfCe(Some (rp.GetExcludeSelfCe() |> Option.defaultValue (UMX.tag true)))
                      .WithSortableDataFormat(Some (rp.GetSortableDataFormat() |> Option.defaultValue GeneSort.Sorting.sortableDataFormat.BitVector512))
                let! _ = parameters.ParamMap.TryFind ParamKeys.sortingWidthKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.rngTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey
                let! _ = parameters.ParamMap.TryFind ParamKeys.mutationModKey
                return queryParams.create database project (rp.GetRepl()) None (geneSortDataType.SorterPoolSet outputName)
                    [|
                        yield "SetupRecipe", "soss-seed-pool-prefix-tests-v1"
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSortableTestsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSortableTestsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.setupSorterSetEvalsDatabaseNameKey |> Option.toList do
                            yield ParamKeys.setupSorterSetEvalsDatabaseNameKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortingWidthKey |> Option.toList do
                            yield ParamKeys.sortingWidthKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.rngTypeKey |> Option.toList do
                            yield ParamKeys.rngTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.simpleSorterModelTypeKey |> Option.toList do
                            yield ParamKeys.simpleSorterModelTypeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.excludeSelfCeKey |> Option.toList do
                            yield ParamKeys.excludeSelfCeKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.aMaxKey |> Option.toList do
                            yield ParamKeys.aMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.activationProbabilityKey |> Option.toList do
                            yield ParamKeys.activationProbabilityKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.controlledSeedAMaxKey |> Option.toList do
                            yield ParamKeys.controlledSeedAMaxKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.prefixLibIdKey |> Option.toList do
                            yield ParamKeys.prefixLibIdKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sortableDataFormatKey |> Option.toList do
                            yield ParamKeys.sortableDataFormatKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterPoolCountKey |> Option.toList do
                            yield ParamKeys.sorterPoolCountKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterCountPerPoolKey |> Option.toList do
                            yield ParamKeys.sorterCountPerPoolKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.mutationModKey |> Option.toList do
                            yield ParamKeys.mutationModKey, value
                        for value in parameters.ParamMap.TryFind ParamKeys.sorterEvalMeasureKey |> Option.toList do
                            yield ParamKeys.sorterEvalMeasureKey, value
                    |]
            }


        /// Constructs only the Run address for SossMsuf32Seed.
        let makeRunQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.Run outputName) (Map.toArray settings))

        /// Constructs only the RunParameters address for SossMsuf32Seed.
        let makeRunParametersQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.RunParameters outputName) (Map.toArray settings))

        /// Constructs only the SorterPoolSet address for SossMsuf32Seed.
        let makeSorterPoolSetQueryParams projectName dbName (parameters: setupParameters) outputName =
            match parameters.GetPrefixLibId(), parameters.GetMergeLibId(), parameters.ParamMap.ContainsKey ParamKeys.controlledSeedAMaxKey with
            | Some _, _, true -> makePrefixControlledSeedPoolQueryParams projectName dbName parameters outputName
            | Some _, _, false -> makePrefixSeedPoolQueryParams projectName dbName parameters outputName
            | _, Some _, true -> makeMergeControlledSeedPoolQueryParams projectName dbName parameters outputName
            | _, Some _, false -> makeMergeSeedPoolQueryParams projectName dbName parameters outputName
            | _, _, true -> makeStandardControlledSeedPoolQueryParams projectName dbName parameters outputName
            | _, _, false -> makeStandardSeedPoolQueryParams projectName dbName parameters outputName

        /// Constructs only the SorterPoolSetSummarySet address for SossMsuf32Seed.
        let makeSorterPoolSetSummarySetQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SorterPoolSetSummarySet outputName) (Map.toArray settings))

        /// Constructs only the SorterSet address for SossMsuf32Seed.
        let makeSorterSetQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SorterSet outputName) (Map.toArray settings))

        /// Constructs only the SorterModelSet address for SossMsuf32Seed.
        let makeSorterModelSetQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SorterModelSet outputName) (Map.toArray settings))

        /// Constructs only the SortableTests address for SossMsuf32Seed.
        let makeSortableTestsQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SortableTests outputName) (Map.toArray settings))

        /// Constructs only the SortableTestsWithPrefix address for SossMsuf32Seed.
        let makeSortableTestsWithPrefixQueryParams projectName dbName (parameters: setupParameters) outputName =
            match parameters.GetPrefixLibId(), parameters.GetMergeLibId() with
            | Some _, _ -> makePrefixTestsQueryParams projectName dbName parameters outputName
            | _, Some _ -> makeMergeTestsQueryParams projectName dbName parameters outputName
            | _, _ -> makeStandardTestsQueryParams projectName dbName parameters outputName

        /// Constructs only the SorterSetEval address for SossMsuf32Seed.
        let makeSorterSetEvalQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SorterSetEval outputName) (Map.toArray settings))

        /// Constructs only the SorterPoolBinsSetSeries address for SossMsuf32Seed.
        let makeSorterPoolBinsSetSeriesQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SorterPoolBinsSetSeries outputName) (Map.toArray settings))

        /// Constructs only the SorterPoolSetHistory address for SossMsuf32Seed.
        let makeSorterPoolSetHistoryQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.SorterPoolSetHistory outputName) (Map.toArray settings))

        /// Constructs only the TextReport address for SossMsuf32Seed.
        let makeTextReportQueryParams projectName dbName (rp: setupParameters) outputName =
            let settings = rp.ParamMap
                           |> Map.remove ParamKeys.setupParametersIdKey
                           |> Map.remove ParamKeys.setupSortableTestsIdKey
                           |> Map.remove ParamKeys.setupSortableTestsWithPrefixIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetSummarySetIdKey
                           |> Map.remove ParamKeys.setupSorterSetIdKey
                           |> Map.remove ParamKeys.setupSorterModelSetIdKey
                           |> Map.remove ParamKeys.setupSorterSetEvalIdKey
                           |> Map.remove ParamKeys.setupSorterPoolBinsSetSeriesIdKey
                           |> Map.remove ParamKeys.setupSorterPoolSetHistoryIdKey
            Some (queryParams.create dbName projectName (rp.GetRepl()) (rp.GetGenerationCurrent())
                      (geneSortDataType.TextReport outputName) (Map.toArray settings))

        /// Catalog adapter: selection only; each constructor owns its fields.
        let makeQueryParams projectName dbName parameters = function
            | geneSortDataType.Run name -> makeRunQueryParams projectName dbName parameters name
            | geneSortDataType.RunParameters name -> makeRunParametersQueryParams projectName dbName parameters name
            | geneSortDataType.SorterPoolSet name -> makeSorterPoolSetQueryParams projectName dbName parameters name
            | geneSortDataType.SorterPoolSetSummarySet name -> makeSorterPoolSetSummarySetQueryParams projectName dbName parameters name
            | geneSortDataType.SorterSet name -> makeSorterSetQueryParams projectName dbName parameters name
            | geneSortDataType.SorterModelSet name -> makeSorterModelSetQueryParams projectName dbName parameters name
            | geneSortDataType.SortableTests name -> makeSortableTestsQueryParams projectName dbName parameters name
            | geneSortDataType.SortableTestsWithPrefix name -> makeSortableTestsWithPrefixQueryParams projectName dbName parameters name
            | geneSortDataType.SorterSetEval name -> makeSorterSetEvalQueryParams projectName dbName parameters name
            | geneSortDataType.SorterPoolBinsSetSeries name -> makeSorterPoolBinsSetSeriesQueryParams projectName dbName parameters name
            | geneSortDataType.SorterPoolSetHistory name -> makeSorterPoolSetHistoryQueryParams projectName dbName parameters name
            | geneSortDataType.TextReport name -> makeTextReportQueryParams projectName dbName parameters name


    /// Imported starts have explicit file IDs; they never generate seed identities.
    module ImportedStart =
        let makeTestsQueryParams project database (parameters: setupParameters) =
            parameters.ParamMap.TryFind ParamKeys.setupSortableTestsWithPrefixIdKey
            |> Option.map (fun id ->
                queryParams.create database project None None (geneSortDataType.SortableTestsWithPrefix "") [||]
                |> queryParams.withId (System.Guid.Parse id |> UMX.tag<queryParamsId>))

        let makePoolQueryParams project database (parameters: setupParameters) =
            parameters.ParamMap.TryFind ParamKeys.setupSorterPoolSetIdKey
            |> Option.map (fun id ->
                queryParams.create database project None None (geneSortDataType.SorterPoolSet "") [||]
                |> queryParams.withId (System.Guid.Parse id |> UMX.tag<queryParamsId>))

        let makeQueryParams project database parameters = function
            | geneSortDataType.SortableTestsWithPrefix _ -> makeTestsQueryParams project database parameters
            | geneSortDataType.SorterPoolSet _ -> makePoolQueryParams project database parameters
            | _ -> None

    let registrations : (string * setupQueryParamsCatalogBuilder) list = [
        QueryCatalogNamesSetup.importedStart, ImportedStart.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdUf6MutationRate, SorterSgd.Uf6MutationRate.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdMsrsOrthoPara, SorterSgd.MsrsOrthoPara.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdMssiOrthoPara, SorterSgd.MssiOrthoPara.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdMsufStandard, SorterSgd.Msuf.Standard.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdMsufSoss, SorterSgd.Msuf.Soss.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdMsrs32MutationRate, SorterSgd.Msrs32MutationRate.makeQueryParams
        QueryCatalogNamesSetup.sorterSgdMsrsPoolModComp, SorterSgd.MsrsPoolModComp.makeQueryParams
        QueryCatalogNamesSetup.sortableTestMerge, SortableTests.Merge.makeQueryParams
        QueryCatalogNamesSetup.sortableTestPrefix, SortableTests.Prefix.makeQueryParams
        QueryCatalogNamesSetup.sorterEvalStandard, SorterEval.Standard.makeQueryParams
        QueryCatalogNamesSetup.sorterEvalMerge, SorterEval.Merge.makeQueryParams
        QueryCatalogNamesSetup.sorterEvalPrefix, SorterEval.Prefix.makeQueryParams
        QueryCatalogNamesSetup.sorterMutateStandard, SorterMutate.Standard.makeQueryParams
        QueryCatalogNamesSetup.sorterMutateMerge, SorterMutate.Merge.makeQueryParams
        QueryCatalogNamesSetup.sorterMutatePrefix, SorterMutate.Prefix.makeQueryParams
        QueryCatalogNamesSetup.sossMsuf32Seed, SossMsuf32Seed.makeQueryParams
    ]
    let private registrationLock = obj ()
    let mutable private registered = false
    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                registrations |> List.iter (fun (name, make) -> QueryParamsCatalogSetup.register name make)
                registered <- true)
