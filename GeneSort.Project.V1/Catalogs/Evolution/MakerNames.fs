namespace GeneSort.Project.V1.Catalogs.Evolution

/// Serializable selections, independent of the evolution strategy and storage host.
type evolutionMakerNames = {
    SortableTests: string
    SeedPoolSet: string
    Mutator: string
}

module MutatorMakerNames =
    [<Literal>]
    let simpleSorterModel = "sorter-model-mutator.simple"

module SortableTestsMakerNames =
    [<Literal>]
    let standard = "sortable-tests.standard"
    [<Literal>]
    let merge = "sortable-tests.merge"
    [<Literal>]
    let prefix = "sortable-tests.prefix"

module SeedSorterPoolSetMakerNames =
    module Standard =
        [<Literal>]
        let standard = "seed-pool.standard.standard"
        [<Literal>]
        let merge = "seed-pool.standard.merge"
        [<Literal>]
        let prefix = "seed-pool.standard.prefix"

    module Soss =
        [<Literal>]
        let standard = "seed-pool.soss.standard"
        [<Literal>]
        let merge = "seed-pool.soss.merge"
        [<Literal>]
        let prefix = "seed-pool.soss.prefix"

    // Legacy keys retained for saved configurations.
    [<Literal>]
    let standard = "seed-sorter-pool-set.standard"
    [<Literal>]
    let merge = "seed-sorter-pool-set.merge"
    [<Literal>]
    let prefix = "seed-sorter-pool-set.prefix"

module EvolutionMakerNames =
    let standard = {
        SortableTests = SortableTestsMakerNames.standard
        SeedPoolSet = SeedSorterPoolSetMakerNames.Standard.standard
        Mutator = MutatorMakerNames.simpleSorterModel
    }
    let merge = { standard with SortableTests = SortableTestsMakerNames.merge; SeedPoolSet = SeedSorterPoolSetMakerNames.Standard.merge }
    let prefix = { standard with SortableTests = SortableTestsMakerNames.prefix; SeedPoolSet = SeedSorterPoolSetMakerNames.Standard.prefix }
    let sossStandard = { standard with SeedPoolSet = SeedSorterPoolSetMakerNames.Soss.standard }
    let sossMerge = { merge with SeedPoolSet = SeedSorterPoolSetMakerNames.Soss.merge }
    let sossPrefix = { prefix with SeedPoolSet = SeedSorterPoolSetMakerNames.Soss.prefix }

    let validate names =
        for field, value in ["SortableTests", names.SortableTests; "SeedPoolSet", names.SeedPoolSet; "Mutator", names.Mutator] do
            if System.String.IsNullOrWhiteSpace value then
                invalidArg field "An evolution maker catalog name cannot be empty."

