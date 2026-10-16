namespace GeneSort.Project.V1.Catalogs.SortableTestsWithPrefix

open GeneSort.Project.V1.Catalogs.Setup

module SortableTestsWithPrefixCatalogNames =
    [<Literal>]
    let standard = "sortable-tests-with-prefix.standard"
    [<Literal>]
    let merge = "sortable-tests-with-prefix.merge"
    [<Literal>]
    let prefix = "sortable-tests-with-prefix.prefix"

module SortableTestsWithPrefixCatalog =
    let private registry =
        setupRegistry("sortable tests with prefix setup", [
            SortableTestsWithPrefixCatalogNames.standard, fun context -> context.MakeStandardTestsWithPrefix
            SortableTestsWithPrefixCatalogNames.merge, fun context -> context.MakeMergeTestsWithPrefix
            SortableTestsWithPrefixCatalogNames.prefix, fun context -> context.MakePrefixTestsWithPrefix
        ])

    let register name (builder: setupBuilder) = registry.Register name builder
    let get name : setupBuilder = registry.Get name
