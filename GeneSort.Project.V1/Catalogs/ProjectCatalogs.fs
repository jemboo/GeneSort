namespace GeneSort.Project.V1

/// Initializes all built-in catalogs. Safe to call repeatedly or concurrently.
module ProjectCatalogs =
    let registerAll () =
        QueryParamsBuilders.registerAll ()
        RunParamBuilders.registerAll ()
        SorterSgdMakerCatalogs.registerAll ()
