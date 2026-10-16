namespace GeneSort.Project.V1.Catalogs.Evolution

open GeneSort.Project.V1
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter

/// Storage capabilities supplied by the host; makers do not depend on a database implementation.
type evolutionMakerContext =
    private {
        loadMergeTests: int<replNumber> -> mergeLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>
        loadPrefixTests: int<replNumber> -> prefixLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>
        loadStandardSorterEvals: int<sortingWidth> -> simpleSorterModelType -> sorterEvalType -> Async<Result<sorterSetEval, string>>
        loadMergeSorterEvals: mergeLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>
        loadPrefixSorterEvals: prefixLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>
    }

    static member create
            (loadMergeTests: int<replNumber> -> mergeLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>)
            (loadPrefixTests: int<replNumber> -> prefixLibId -> sortableDataFormat -> Async<Result<sortableTests, string>>)
            (loadStandardSorterEvals: int<sortingWidth> -> simpleSorterModelType -> sorterEvalType -> Async<Result<sorterSetEval, string>>)
            (loadMergeSorterEvals: mergeLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>)
            (loadPrefixSorterEvals: prefixLibId -> int<replNumber> -> simpleSorterModelType -> Async<Result<sorterSetEval, string>>)
            : evolutionMakerContext =
        {
            loadMergeTests = loadMergeTests
            loadPrefixTests = loadPrefixTests
            loadStandardSorterEvals = loadStandardSorterEvals
            loadMergeSorterEvals = loadMergeSorterEvals
            loadPrefixSorterEvals = loadPrefixSorterEvals
        }

    member this.LoadMergeTests with get() = this.loadMergeTests
    member this.LoadPrefixTests with get() = this.loadPrefixTests
    member this.LoadStandardSorterEvals with get() = this.loadStandardSorterEvals
    member this.LoadMergeSorterEvals with get() = this.loadMergeSorterEvals
    member this.LoadPrefixSorterEvals with get() = this.loadPrefixSorterEvals

    member this.WithLoadMergeTests(value) = { this with loadMergeTests = value }
    member this.WithLoadPrefixTests(value) = { this with loadPrefixTests = value }
    member this.WithLoadStandardSorterEvals(value) = { this with loadStandardSorterEvals = value }
    member this.WithLoadMergeSorterEvals(value) = { this with loadMergeSorterEvals = value }
    member this.WithLoadPrefixSorterEvals(value) = { this with loadPrefixSorterEvals = value }
