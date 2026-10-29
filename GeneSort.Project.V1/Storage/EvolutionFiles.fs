namespace GeneSort.Project.V1.Storage

open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution

/// The existing file-query interface uses saved parameter rows. Convert only here,
/// preserving every identity field and the original checkpoint lookup behavior.
module EvolutionFiles =
    let queryAtGeneration (db: IGeneSortDb) (path: evoPath) (progress: evoProgress) generation output =
        let saved = EvoPath.toParameters path (progress.WithCurrentGeneration(Some generation))
        db.makeQueryParamsFromRunParams saved output

    let loadHighestStandard intervals db (path: evoPath) progress =
        Utils.loadOutputDataWithSchedules path.CheckpointIntervals OutputData.asSorterPoolSetStandard (geneSortDataType.SorterPoolSet "") intervals db (EvoPath.toParameters path progress)

    let loadHighestSoss intervals db (path: evoPath) progress =
        Utils.loadOutputDataWithSchedules path.CheckpointIntervals OutputData.asSorterPoolSetSoss (geneSortDataType.SorterPoolSet "") intervals db (EvoPath.toParameters path progress)

    let saveCheckpoint (db: IGeneSortDb) outputs overwrite = async {
        match db with
        | :? ICheckpointStore as store -> return! store.saveCheckpointAsync outputs overwrite
        | _ ->
            let mutable result = Ok ()
            for query, data in outputs do
                if Result.isOk result then
                    let! saved = db.saveAsync geneSortDataMode.Results query data overwrite
                    result <- saved
            return result
    }

    let saveRetainedCheckpoint (db: IGeneSortDb) (path: evoPath) snapshot outputs overwrite =
        match db with
        | :? IRetainedCheckpointStore as store -> store.saveRetainedCheckpointAsync path.Id snapshot outputs overwrite
        | _ -> saveCheckpoint db outputs overwrite
