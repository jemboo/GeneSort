namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Dispatch.V1.SortableTest
open CommonSorterEval


module SorterEvalDbs =
    
    module RandomStandard =

        module Uniform =

            let dbName = "RandomStandardUniform" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>


            let makeQueryParams
                            (rng: rngType)
                            (repl: int<replNumber>) 
                            (sw: int<sortingWidth>) 
                            (smt: simpleSorterModelType) 
                            (odt: outputDataType) : queryParams =
                queryParams.create dbName (Some repl) odt
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
                       (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) 
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! repl = rp.GetRepl()
                    let! sw = rp.GetSortingWidth()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    return makeQueryParams rng repl sw smt odt
                }
        


            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)





    module RandomMerge =
    
        module Uniform =
            
            let dbName = "RandomMergeUniform" |> UMX.tag<databaseName>
            //let queryName = $"{projectName}\\{%dbName}" |> UMX.tag<queryName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let makeQueryParams
                        (rng: rngType)
                        (repl: int<replNumber>) 
                        (sortingWidth: int<sortingWidth>)
                        (simpleSorterModelType: simpleSorterModelType)
                        (mergeDimension: int<mergeDimension>) 
                        (mergeSuffixType: mergeSuffixType)
                        (sortableDataFormat: sortableDataFormat) 
                        (outputDataType: outputDataType) : queryParams =

                queryParams.create 
                    dbName
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


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! rng = rp.GetRngType()
                    let! repl = rp.GetRepl()
                    let! sw = rp.GetSortingWidth()
                    let! md = rp.GetMergeDimension()
                    let! mst = rp.GetMergeSuffixType()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! sdf = rp.GetSortableDataFormat()
                    return makeQueryParams rng repl sw smt md mst sdf odt
                }

            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)




    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ (RandomStandard.Uniform.dbName, RandomStandard.Uniform.db :> IGeneSortDb);
          (RandomMerge.Uniform.dbName, RandomMerge.Uniform.db :> IGeneSortDb) ]
        |> Map.ofList


    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)



    let getStandardSorterEvalBins
                    (sortingWidth: int<sortingWidth>) 
                    (simpleSorterModelType: simpleSorterModelType) 
                            : Async<Result<sorterEvalBins, string>> =
        let qp = RandomStandard.Uniform.makeQueryParams 
                        CommonSorterEval.projectRngType
                        (0 |> UMX.tag<replNumber>) 
                        sortingWidth simpleSorterModelType 
                        (outputDataType.SorterEvalBins "")
        async {
             let! result = (RandomStandard.Uniform.db :> IGeneSortDb).loadAsync qp
             return  result |> Result.bind OutputData.asSorterEvalBins
        }


    let getMergeSorterEvalBins
                    (sortingWidth: int<sortingWidth>)
                    (simpleSorterModelType: simpleSorterModelType)
                    (mergeDimension: int<mergeDimension>) 
                    (mergeSuffixType: mergeSuffixType)
                            : Async<Result<sorterEvalBins, string>> =
        let qp = RandomMerge.Uniform.makeQueryParams 
                        CommonSorterEval.projectRngType
                        (0 |> UMX.tag<replNumber>) 
                        sortingWidth simpleSorterModelType
                        mergeDimension mergeSuffixType 
                        CommonSortableTest.projectSortableDataFormat
                        (outputDataType.SorterEvalBins "")
        async {
             let! result = (RandomMerge.Uniform.db :> IGeneSortDb).loadAsync qp
             return  result |> Result.bind OutputData.asSorterEvalBins
        }
