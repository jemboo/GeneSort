namespace GeneSort.Project.V1.Catalogs.SorterModelSet

open GeneSort.Project.V1.Catalogs.Setup

module SorterModelSetCatalog =
    let private bind name : setupBuilder =
        fun context ->
            match Map.tryFind name context.MakeSorterModelSets with
            | Some setup -> setup
            | None -> fun _ -> async { return Error (sprintf "Missing sorter model set setup binding '%s'." name) }

    let private registry =
        setupRegistry("sorter model set setup", [
            SorterModelSetCatalogNames.uniform, bind SorterModelSetCatalogNames.uniform
            SorterModelSetCatalogNames.mutantStandard, bind SorterModelSetCatalogNames.mutantStandard
            SorterModelSetCatalogNames.mutantMerge, bind SorterModelSetCatalogNames.mutantMerge
            SorterModelSetCatalogNames.mutantPrefix, bind SorterModelSetCatalogNames.mutantPrefix
        ])

    let register name (builder: setupBuilder) = registry.Register name builder
    let get name : setupBuilder = registry.Get name
