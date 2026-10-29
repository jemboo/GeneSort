namespace GeneSort.Project.V1.Catalogs.Evolution

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Project.V1

/// Loaders for the saved tests and evaluations used to prepare evolution inputs.
type evolutionSetupSources = {
    LoadMergeTests: int<replNumber> -> mergeLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>
    LoadPrefixTests: int<replNumber> -> prefixLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>
    LoadStandardSorterEvals: int<channelCount> -> simpleSorterModelType -> sorterEvalType -> Async<Result<sorterSetEval, string>>
    LoadMergeSorterEvals: mergeLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>
    LoadPrefixSorterEvals: prefixLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>
}
