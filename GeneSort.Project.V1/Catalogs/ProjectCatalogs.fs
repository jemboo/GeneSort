namespace GeneSort.Project.V1.Catalogs

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters

/// Initializes all built-in catalogs. Safe to call repeatedly or concurrently.
module ProjectCatalogs =
    let registerAll () =
        QueryParamsBuildersSetup.registerAll ()
        QueryParamsBuildersRun.registerAll ()
        RunParametersPipelineCatalog.registerAll ()
        SorterSgdMakerCatalogs.registerAll ()
