namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval

/// Supplies evolution setup inputs from the existing file-backed libraries.
module FileEvolutionSetupSources =
    let sources : evolutionSetupSources = {
        LoadMergeTests = SortableTestsDbs.Merge.getMergeSortableTests
        LoadPrefixTests = SortableTestsDbs.Prefix.getPrefixSortableTests
        LoadStandardSorterEvals = SorterEvalDbs.getStandardSorterEvals
        LoadMergeSorterEvals = SorterEvalDbs.getMergeSorterEvals
        LoadPrefixSorterEvals = SorterEvalDbs.getPrefixSorterEvals
    }
