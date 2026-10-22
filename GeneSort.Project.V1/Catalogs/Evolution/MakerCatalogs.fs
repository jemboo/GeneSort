namespace GeneSort.Project.V1.Catalogs.Evolution

open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Eval.V1.Sgd
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Model.Sorting.Simple.V1

type sortableTestsMaker = evolutionSetupSources -> runParameters -> Async<Result<sortableTests * ce array, string>>
type seedSorterPoolSetMaker = evolutionSetupSources -> runParameters -> Async<Result<sorterPoolSet, string>>
type sorterModelMutatorMaker = runParameters -> Async<Result<simpleSorterModelMutator, string>>

module private MakerCatalog =
    let register catalogName (name: string) value (catalog: Map<string, 'T> ref) syncRoot =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) (sprintf "A %s catalog name cannot be empty." catalogName)
        lock syncRoot (fun () ->
            if Map.containsKey name catalog.Value then
                invalidOp (sprintf "A %s named '%s' is already registered." catalogName name)
            catalog.Value <- Map.add name value catalog.Value)

    let get catalogName name (catalog: Map<string, 'T> ref) syncRoot =
        lock syncRoot (fun () ->
            match Map.tryFind name catalog.Value with
            | Some maker -> maker
            | None -> invalidOp (sprintf "No %s named '%s' is registered." catalogName name))

module SortableTestsMakerCatalog =
    let private syncRoot = obj ()
    let private makers: Map<string, sortableTestsMaker> ref = ref Map.empty

    let register name maker = MakerCatalog.register "sortable tests maker" name maker makers syncRoot
    let get name = MakerCatalog.get "sortable tests maker" name makers syncRoot

module SeedSorterPoolSetMakerCatalog =
    let private syncRoot = obj ()
    let private makers: Map<string, seedSorterPoolSetMaker> ref = ref Map.empty

    let register name maker = MakerCatalog.register "seed sorter pool set maker" name maker makers syncRoot
    let get name = MakerCatalog.get "seed sorter pool set maker" name makers syncRoot

module MutatorMakerCatalog =
    let private syncRoot = obj ()
    let private makers: Map<string, sorterModelMutatorMaker> ref = ref Map.empty

    let register name maker = MakerCatalog.register "mutator maker" name maker makers syncRoot
    let get name = MakerCatalog.get "mutator maker" name makers syncRoot

/// Registers the built-in makers once, before executor lookup.
module SorterSgdMakerCatalogs =
    let private registrationLock = obj ()
    let mutable private registered = false

    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                SortableTestsMakerCatalog.register SortableTestsMakerNames.standard EvolutionTestMakers.makeStandardTests
                SortableTestsMakerCatalog.register SortableTestsMakerNames.merge EvolutionTestMakers.makeMergeTests
                SortableTestsMakerCatalog.register SortableTestsMakerNames.prefix EvolutionTestMakers.makePrefixTests
                SeedSorterPoolSetMakerCatalog.register SeedSorterPoolSetMakerNames.Standard.standard SeedPoolSetMakers.createStandardSeedSorterPoolSetStandard
                SeedSorterPoolSetMakerCatalog.register SeedSorterPoolSetMakerNames.Standard.merge SeedPoolSetMakers.createStandardSeedSorterPoolSetMerge
                SeedSorterPoolSetMakerCatalog.register SeedSorterPoolSetMakerNames.Standard.prefix SeedPoolSetMakers.createStandardSeedSorterPoolSetPrefix
                SeedSorterPoolSetMakerCatalog.register SeedSorterPoolSetMakerNames.Soss.standard SeedPoolSetMakers.createSossSeedSorterPoolSetStandard
                SeedSorterPoolSetMakerCatalog.register SeedSorterPoolSetMakerNames.Soss.merge SeedPoolSetMakers.createSossSeedSorterPoolSetMerge
                SeedSorterPoolSetMakerCatalog.register SeedSorterPoolSetMakerNames.Soss.prefix SeedPoolSetMakers.createSossSeedSorterPoolSetPrefix
                MutatorMakerCatalog.register MutatorMakerNames.simpleSorterModel MutatorMakers.makeSimpleSorterModelMutator
                registered <- true)
