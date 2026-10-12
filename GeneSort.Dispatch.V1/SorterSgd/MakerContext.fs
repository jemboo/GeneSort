namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval

/// Binds project-level makers to the existing file-backed libraries.
module EvolutionMakerContext =
    let fileDb : evolutionMakerContext = {
        LoadMergeTests = SortableTestsDbs.Merge.getMergeSortableTests
        LoadPrefixTests = SortableTestsDbs.Prefix.getPrefixSortableTests
        LoadStandardSorterEvals = SorterEvalDbs.getStandardSorterEvals
        LoadMergeSorterEvals = SorterEvalDbs.getMergeSorterEvals
        LoadPrefixSorterEvals = SorterEvalDbs.getPrefixSorterEvals
    }
