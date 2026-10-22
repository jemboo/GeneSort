namespace GeneSort.Project.V1.Catalogs.Evolution

/// Serializable selections, independent of the evolution strategy and storage host.
type evolutionMakerNamesArgs = {
    SortableTests: string
    SeedPoolSet: string
    Mutator: string
}

type evolutionMakerNames =
    private {
        sortableTests: string
        seedPoolSet: string
        mutator: string
    }

    static member create (args: evolutionMakerNamesArgs) : evolutionMakerNames =
        {
            sortableTests = args.SortableTests
            seedPoolSet = args.SeedPoolSet
            mutator = args.Mutator
        }

    member this.SortableTests with get() = this.sortableTests
    member this.SeedPoolSet with get() = this.seedPoolSet
    member this.Mutator with get() = this.mutator

    member this.WithSortableTests(value) = { this with sortableTests = value }
    member this.WithSeedPoolSet(value) = { this with seedPoolSet = value }
    member this.WithMutator(value) = { this with mutator = value }

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

module EvolutionMakerNames =
    let standard = evolutionMakerNames.create {
        SortableTests = SortableTestsMakerNames.standard
        SeedPoolSet = SeedSorterPoolSetMakerNames.Standard.standard
        Mutator = MutatorMakerNames.simpleSorterModel
    }
    let merge = standard.WithSortableTests(SortableTestsMakerNames.merge).WithSeedPoolSet(SeedSorterPoolSetMakerNames.Standard.merge)
    let prefix = standard.WithSortableTests(SortableTestsMakerNames.prefix).WithSeedPoolSet(SeedSorterPoolSetMakerNames.Standard.prefix)
    let sossStandard = standard.WithSeedPoolSet(SeedSorterPoolSetMakerNames.Soss.standard)
    let sossMerge = merge.WithSeedPoolSet(SeedSorterPoolSetMakerNames.Soss.merge)
    let sossPrefix = prefix.WithSeedPoolSet(SeedSorterPoolSetMakerNames.Soss.prefix)

    let validate (names: evolutionMakerNames) =
        for field, value in ["SortableTests", names.SortableTests; "SeedPoolSet", names.SeedPoolSet; "Mutator", names.Mutator] do
            if System.String.IsNullOrWhiteSpace value then
                invalidArg field "An evolution maker catalog name cannot be empty."

