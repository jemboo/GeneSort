namespace GeneSort.Project.V1

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
    [<Literal>]
    let standard = "seed-sorter-pool-set.standard"
    [<Literal>]
    let merge = "seed-sorter-pool-set.merge"
    [<Literal>]
    let prefix = "seed-sorter-pool-set.prefix"

module EvolutionMakerNames =
    let standard = {
        SortableTests = SortableTestsMakerNames.standard
        SeedPoolSet = SeedSorterPoolSetMakerNames.standard
        Mutator = MutatorMakerNames.simpleSorterModel
    }
    let merge = { standard with SortableTests = SortableTestsMakerNames.merge; SeedPoolSet = SeedSorterPoolSetMakerNames.merge }
    let prefix = { standard with SortableTests = SortableTestsMakerNames.prefix; SeedPoolSet = SeedSorterPoolSetMakerNames.prefix }

    let validate names =
        for field, value in ["SortableTests", names.SortableTests; "SeedPoolSet", names.SeedPoolSet; "Mutator", names.Mutator] do
            if System.String.IsNullOrWhiteSpace value then
                invalidArg field "An evolution maker catalog name cannot be empty."

