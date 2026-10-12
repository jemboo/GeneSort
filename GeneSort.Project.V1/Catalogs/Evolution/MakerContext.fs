namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter

/// Storage capabilities supplied by the host; makers do not depend on a database implementation.
type evolutionMakerContext = {
    LoadMergeTests: int<replNumber> -> mergeLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>
    LoadPrefixTests: int<replNumber> -> prefixLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>
    LoadStandardSorterEvals: int<sortingWidth> -> simpleSorterModelType -> sorterEvalType -> Async<Result<sorterSetEval, string>>
    LoadMergeSorterEvals: mergeLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>
    LoadPrefixSorterEvals: prefixLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>
}
