namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval

/// Binds project-level makers to the existing file-backed libraries.
module EvolutionMakerContext =
    let fileDb : evolutionMakerContext =
        evolutionMakerContext.create
            (SortableTestsDbs.Merge.getMergeSortableTests)
            (SortableTestsDbs.Prefix.getPrefixSortableTests)
            (SorterEvalDbs.getStandardSorterEvals)
            (SorterEvalDbs.getMergeSorterEvals)
            (SorterEvalDbs.getPrefixSorterEvals)
