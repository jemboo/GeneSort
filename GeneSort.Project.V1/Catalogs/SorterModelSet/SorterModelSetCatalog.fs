namespace GeneSort.Project.V1.Catalogs.SorterModelSet

open GeneSort.Project.V1.Catalogs.Setup

module SorterModelSetCatalogNames =
    [<Literal>]
    let uniform = "sorter-model-set.uniform"

module SorterModelSetCatalog =
    let private registry =
        setupRegistry("sorter model set setup", [
            SorterModelSetCatalogNames.uniform, fun context -> context.MakeUniformSorterModelSet
        ])

    let register name (builder: setupBuilder) = registry.Register name builder
    let get name : setupBuilder = registry.Get name
