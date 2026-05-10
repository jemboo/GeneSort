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



    let makeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams option =
        maybe {
            // Common parameters: if these are missing, we definitely want None
            let! repl = rp.GetRepl()
            let! sw = rp.GetSortingWidth()
            let! smt = rp.GetSimpleSorterModelType()

            // Use standard pattern matching for the branching logic
            // We don't use let! on RngType because its absence isn't necessarily a failure
            return! 
                match rp.GetRngType() with
                | Some rng -> 
                    // Standard Path
                    Some (makeStandardQueryParams repl sw smt rng odt)
            
                | None -> 
                    // Merge Path - we nested another maybe block here or just use Option.map
                    maybe {
                        let! md = rp.GetMergeDimension()
                        let! mst = rp.GetMergeSuffixType()
                        return makeMergeQueryParams repl sw md mst smt odt
                    }
        }