namespace GeneSort.Dispatch.V1.SorterMutate


open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open CommonSorterMutate

module SorterMutateDbs =


    let makeStandardQueryParams
                    (rng: rngType)
                    (repl: int<replNumber>) 
                    (sw: int<sortingWidth>) 
                    (smt: simpleSorterModelType) 
                    (odt: outputDataType) : queryParams =
        queryParams.create queryName (Some repl) odt
            [| 
               (runParameters.rngTypeKey, rng |> RngType.toString)
               (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) 
            |]


    let makeStandardQueryParamsFromRunParams 
                            (rp: runParameters) 
                            (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! sw = rp.GetSortingWidth()
            let! smt = rp.GetSimpleSorterModelType()
            let! rng = rp.GetRngType()
            return makeStandardQueryParams rng repl sw smt odt
        }


    let makeQueryParamsForMerge
                (rng: rngType)
                (repl: int<replNumber>) 
                (sortingWidth: int<sortingWidth>)
                (simpleSorterModelType: simpleSorterModelType)
                (mergeDimension: int<mergeDimension>) 
                (mergeSuffixType: mergeSuffixType)
                (sortableDataFormat: sortableDataFormat) 
                (outputDataType: outputDataType) : queryParams =

        queryParams.create 
            queryName
            (Some repl)
            outputDataType
            [| 
               (runParameters.rngTypeKey, rng |> RngType.toString)
               (runParameters.sortingWidthKey, string %sortingWidth); 
               (runParameters.simpleSorterModelTypeKey, simpleSorterModelType |> SimpleSorterModelType.toString );
               (runParameters.mergeDimensionKey, string %mergeDimension);
               (runParameters.mergeSuffixTypeKey, mergeSuffixType |> MergeSuffixType.toString);
               (runParameters.sortableDataFormatKey, sortableDataFormat |> SortableDataFormat.toString); 
            |]


    let makeMergeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams option =
        maybe {
            let! rng = rp.GetRngType()
            let! repl = rp.GetRepl()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
            let! smt = rp.GetSimpleSorterModelType()
            let! sdf = rp.GetSortableDataFormat()
            return makeQueryParamsForMerge rng repl sw smt md mst sdf odt
        }


    let randomStandardDb = new GeneSortDbMp(randomStandardDatabaseFolder, makeStandardQueryParamsFromRunParams)

    let randomMergeDb = new GeneSortDbMp(randomMergeDatabaseFolder, makeMergeQueryParamsFromRunParams)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (randomStandardDatabaseName, randomStandardDb :> IGeneSortDb);
          (randomMergeDatabaseName, randomMergeDb :> IGeneSortDb) ]
        |> Map.ofList


    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)





