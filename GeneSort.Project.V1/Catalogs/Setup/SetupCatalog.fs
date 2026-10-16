namespace GeneSort.Project.V1.Catalogs.Setup

open GeneSort.Core
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.Catalogs.SortableTestsWithPrefix

module SetupCatalogNames =
    [<Literal>]
    let standard = "setup.standard"
    [<Literal>]
    let merge = "setup.merge"
    [<Literal>]
    let prefix = "setup.prefix"

module SetupFunctions =
    /// Models are persisted first; a failure prevents tests setup from starting.
    let runBoth (makeSorterModelSet: setupFunction) (makeSortableTests: setupFunction) : setupFunction =
        fun rp -> asyncResult {
            do! makeSorterModelSet rp
            do! makeSortableTests rp
        }

    let fromCatalogs modelSetName testsName : setupBuilder =
        fun context ->
            runBoth (SorterModelSetCatalog.get modelSetName context)
                (SortableTestsWithPrefixCatalog.get testsName context)

module SetupCatalog =
    let private registry =
        setupRegistry("combined setup", [
            SetupCatalogNames.standard, SetupFunctions.fromCatalogs SorterModelSetCatalogNames.uniform SortableTestsWithPrefixCatalogNames.standard
            SetupCatalogNames.merge, SetupFunctions.fromCatalogs SorterModelSetCatalogNames.uniform SortableTestsWithPrefixCatalogNames.merge
            SetupCatalogNames.prefix, SetupFunctions.fromCatalogs SorterModelSetCatalogNames.uniform SortableTestsWithPrefixCatalogNames.prefix
        ])

    let register name (builder: setupBuilder) = registry.Register name builder
    let get name : setupBuilder = registry.Get name
