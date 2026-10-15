namespace GeneSort.Project.V1.Catalogs

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution

/// Initializes all built-in catalogs. Safe to call repeatedly or concurrently.
module ProjectCatalogs =
    let registerAll () =
        QueryParamsBuilders.registerAll ()
        RunParamBuilders.registerAll ()
        SorterSgdMakerCatalogs.registerAll ()
