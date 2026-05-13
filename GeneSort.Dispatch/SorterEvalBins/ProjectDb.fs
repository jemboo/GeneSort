namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1


module ProjectDb =

    let makeStandardQueryParams
                    (repl: int<replNumber>) 
                    (sw: int<sortingWidth>) 
                    (smt: simpleSorterModelType) 
                    (rng: rngType)
                    (odt: outputDataType) : queryParams =
        queryParams.create Yab.projectName (Some repl) odt
            [| (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) 
               (runParameters.rngTypeKey, rng |> RngType.toString) |]


    let makeStandardQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! sw = rp.GetSortingWidth()
            let! smt = rp.GetSimpleSorterModelType()
            let! rng = rp.GetRngType()
            return makeStandardQueryParams repl sw smt rng odt
        }


    let makeMergeQueryParams
                    (repl: int<replNumber>) 
                    (sw: int<sortingWidth>) 
                    (md: int<mergeDimension>) 
                    (mst: mergeSuffixType) 
                    (smt: simpleSorterModelType) 
                    (odt: outputDataType) : queryParams =
        queryParams.create Yab.projectName (Some repl) odt
            [| (runParameters.sortingWidthKey, string %sw ); 
               (runParameters.mergeDimensionKey, string %md);
               (runParameters.mergeSuffixTypeKey, mst |> MergeSuffixType.toString);
               (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) |]  

    let makeMergeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams option =
        maybe {
            let! repl = rp.GetRepl()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
            let! smt = rp.GetSimpleSorterModelType()
            return makeMergeQueryParams repl sw md mst smt odt
        }


