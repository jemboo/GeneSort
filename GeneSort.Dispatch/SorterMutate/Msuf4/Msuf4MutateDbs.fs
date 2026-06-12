namespace GeneSort.Dispatch.V1.SorterMutate.Msuf4


open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1

module Muf4MutateDbs =
    
    let projectName = "SorterMutate.Msuf4" |> UMX.tag<projectName>

    module RandomStandard =

        module Uniform =

            let dbName = "Rsu" |> UMX.tag<databaseName>
            let dbFolder = 
                    @$"c:\Projects\{projectName}\{%dbName}\Data" |> UMX.tag<pathToRootFolder>


            let makeQueryParams
                            (rng: rngType)
                            (ses:sorterEvalSelectionType)
                            (sem:sorterEvalMeasure)
                            (repl: int<replNumber>) 
                            (sw: int<sortingWidth>) 
                            (smt: simpleSorterModelType) 
                            (set: sorterEvalType)
                            (mut: float<mutationRate>)
                            (ins: float<insertionRate>)
                            (del: float<deletionRate>)
                            (mdr: float<modificationRate>)
                            (odt: outputDataType) : queryParams =
                queryParams.create dbName (Some repl) odt
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toString)
                       (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
                       (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) 
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString)
                       (runParameters.mutationRateKey, (Some mut) |> MutationRate.toString)
                       (runParameters.insertionRateKey, (Some ins) |> InsertionRate.toString)
                       (runParameters.deletionRateKey, (Some del) |> DeletionRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! repl = rp.GetRepl()
                    let! ses = rp.GetSorterEvalSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! sw = rp.GetSortingWidth()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! set = rp.GetSorterEvalType()
                    let! mut = rp.GetMutationRate()
                    let! ins = rp.GetInsertionRate()
                    let! del = rp.GetDeletionRate()
                    let! mdr = rp.GetModificationRate()
                    return makeQueryParams rng ses sem repl sw smt set mut ins del mdr odt 
                }

            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)



    module RandomMerge =
    
        module Uniform =
            
            let dbName = "Rmu" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let makeQueryParams
                        (rng: rngType)
                        (ses:sorterEvalSelectionType)
                        (sem:sorterEvalMeasure)
                        (repl: int<replNumber>) 
                        (sortingWidth: int<sortingWidth>)
                        (simpleSorterModelType: simpleSorterModelType)
                        (mergeDimension: int<mergeDimension>) 
                        (mergeSuffixType: mergeSuffixType)
                        (sortableDataFormat: sortableDataFormat) 
                        (set: sorterEvalType)
                        (mut: float<mutationRate>)
                        (ins: float<insertionRate>)
                        (del: float<deletionRate>)
                        (mdr: float<modificationRate>)
                        (outputDataType: outputDataType) : queryParams =

                queryParams.create 
                    dbName
                    (Some repl)
                    outputDataType
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toString)
                       (runParameters.sortingWidthKey, string %sortingWidth); 
                       (runParameters.simpleSorterModelTypeKey, simpleSorterModelType |> SimpleSorterModelType.toString );
                       (runParameters.mergeDimensionKey, string %mergeDimension);
                       (runParameters.mergeSuffixTypeKey, mergeSuffixType |> MergeSuffixType.toString);
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString) 
                       (runParameters.mutationRateKey, (Some mut) |> MutationRate.toString)
                       (runParameters.insertionRateKey, (Some ins) |> InsertionRate.toString)
                       (runParameters.deletionRateKey, (Some del) |> DeletionRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                       (runParameters.sortableDataFormatKey, sortableDataFormat |> SortableDataFormat.toString); 
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! rng = rp.GetRngType()
                    let! ses = rp.GetSorterEvalSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! sw = rp.GetSortingWidth()
                    let! md = rp.GetMergeDimension()
                    let! mst = rp.GetMergeSuffixType()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! sdf = rp.GetSortableDataFormat()
                    let! set = rp.GetSorterEvalType()
                    let! mut = rp.GetMutationRate()
                    let! ins = rp.GetInsertionRate()
                    let! del = rp.GetDeletionRate()
                    let! mdr = rp.GetModificationRate()
                    return makeQueryParams rng ses sem repl sw smt md mst sdf set mut ins del mdr odt
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


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.DatabaseName
        let run = run.create spec.DatabaseName spec.RunName spec.RunDescription
        runHost.Create db spec run :> IRunHost

