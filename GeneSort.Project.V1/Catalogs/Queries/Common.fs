namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

/// A normalized adapter for turning run parameters into the storage path metadata.
type setupQueryParamsBuilder = setupParameters -> geneSortDataType -> queryParams option
type setupQueryParamsCatalogBuilder = string<projectName> -> string<databaseName> -> setupQueryParamsBuilder
type runQueryParamsBuilder = runParameters -> geneSortDataType -> queryParams option
type runQueryParamsCatalogBuilder = string<projectName> -> string<databaseName> -> runQueryParamsBuilder