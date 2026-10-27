namespace GeneSort.Project.V1.Storage

open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution

/// The existing file-query interface uses saved parameter rows. Convert only here,
/// preserving every identity field and the original checkpoint lookup behavior.
module EvolutionFiles =
    let queryAtGeneration (db: IGeneSortDb) (path: evoPath) (progress: evoProgress) generation output =
        let saved = EvoPath.toParameters path (progress.WithCurrentGeneration(Some generation))
        db.makeQueryParamsFromRunParams saved output

    let loadHighestStandard intervals db path progress =
        Utils.loadHighestGenSorterPoolSet intervals db (EvoPath.toParameters path progress)

    let loadHighestSoss intervals db path progress =
        Utils.loadHighestGenSorterPoolSetSoss intervals db (EvoPath.toParameters path progress)
