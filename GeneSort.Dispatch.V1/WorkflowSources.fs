namespace GeneSort.Dispatch.V1

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Runtime
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval

/// Supplies evolution setup inputs from the existing file-backed libraries.
module WorkflowSources =
    let private evolution : evolutionSetupSources = {
        LoadMergeTests = SortableTestsDbs.Merge.getMergeSortableTests
        LoadPrefixTests = SortableTestsDbs.Prefix.getPrefixSortableTests
        LoadStandardSorterEvals = SorterEvalDbs.getStandardSorterEvals
        LoadMergeSorterEvals = SorterEvalDbs.getMergeSorterEvals
        LoadPrefixSorterEvals = SorterEvalDbs.getPrefixSorterEvals
    }

    let defaults = workflowSources.create {
        OpenDatabase = RunHost.openDatabase
        Evolution = evolution
    }
