namespace GeneSort.Dispatch.V1.SorterMutate

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
open GeneSort.SortingLib.Sorter

module SorterMutateDbs =
    
    let projectName = "SorterMutate" |> UMX.tag<projectName>

    module RandomStandard =

        module Uniform =

            let dbName = "Rsu" |> UMX.tag<databaseName>
            let dbFolder = 
                    @$"c:\Projects\{projectName}\{%dbName}\Data" |> UMX.tag<pathToRootFolder>

            let makeQueryParams
                            (repl: int<replNumber>) 
                            (odt: outputDataType) 
                            (rng: rngType)
                            (strSel:sorterSelectionType)
                            (sem:sorterEvalMeasure)
                            (sw: int<sortingWidth>) 
                            (smplSmt: simpleSorterModelType) 
                            (srtableDf: sortableDataFormat) 
                            (set: sorterEvalType)
                            (mutPrams: mutatorParams)
                            (mdr: float<modificationRate>): queryParams =
                queryParams.create dbName projectName (Some repl) None odt
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.seedSorterPoolSelectionTypeKey, strSel |> SorterSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString)
                       (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
                       (runParameters.simpleSorterModelTypeKey, smplSmt |> SimpleSorterModelType.toString) 
                       (runParameters.sortableDataFormatKey, srtableDf |> SortableDataFormat.toString)
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString)
                       (runParameters.mutatorParamsKey, mutPrams |> MutatorParams.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! repl = rp.GetRepl()
                    let! ses = rp.GetSeedSorterPoolSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! sw = rp.GetSortingWidth()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! sdf = rp.GetSortableDataFormat()
                    let! rng = rp.GetRngType()
                    let! set = rp.GetSorterEvalType()
                    let! smps = rp.GetMutatorParams()
                    let! mdr = rp.GetModificationRate()
                    return makeQueryParams repl odt rng ses sem sw smt sdf set smps mdr  
                }

            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)



    module RandomMerge =
    
        module Uniform =
            
            let dbName = "Rmu" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let makeQueryParams
                        (repl: int<replNumber>)
                        (outputDataType: outputDataType)
                        (rng: rngType)
                        (strSel:sorterSelectionType)
                        (sem:sorterEvalMeasure)
                        (mrgLibId: mergeLibId)
                        (smplSmt: simpleSorterModelType)
                        (srtableDf: sortableDataFormat) 
                        (set: sorterEvalType)
                        (mutPrams: mutatorParams)
                        (mdr: float<modificationRate>): queryParams =

                queryParams.create 
                    dbName projectName
                    (Some repl)
                    None
                    outputDataType
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.seedSorterPoolSelectionTypeKey, strSel |> SorterSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString)
                       (runParameters.mergeLibIdKey, mrgLibId |> MergeLibId.toString);
                       (runParameters.simpleSorterModelTypeKey, smplSmt |> SimpleSorterModelType.toString);
                       (runParameters.sortableDataFormatKey, srtableDf |> SortableDataFormat.toString); 
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString) 
                       (runParameters.mutatorParamsKey, mutPrams |> MutatorParams.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! rng = rp.GetRngType()
                    let! strSel = rp.GetSeedSorterPoolSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! mrgLibId = rp.GetMergeLibId()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! sdf = rp.GetSortableDataFormat()
                    let! set = rp.GetSorterEvalType()
                    let! mutPrams = rp.GetMutatorParams()
                    let! mdr = rp.GetModificationRate()
                    return makeQueryParams repl odt rng strSel sem mrgLibId smt sdf set mutPrams mdr
                }

            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)



    module RandomPrefix =
    
        module Uniform =
            
            let dbName = "Rpu" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let makeQueryParams
                        (repl: int<replNumber>)
                        (outputDataType: outputDataType)
                        (rng: rngType)
                        (strSel: sorterSelectionType)
                        (sem:sorterEvalMeasure)
                        (pfxLibId: prefixLibId)
                        (smplSmt: simpleSorterModelType)
                        (srtableDf: sortableDataFormat) 
                        (set: sorterEvalType)
                        (mutPrams: mutatorParams)
                        (mdr: float<modificationRate>) : queryParams =

                queryParams.create 
                    dbName projectName
                    (Some repl)
                    None
                    outputDataType
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.seedSorterPoolSelectionTypeKey, strSel |> SorterSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString)
                       (runParameters.prefixLibIdKey, pfxLibId |> PrefixLibId.toString);
                       (runParameters.simpleSorterModelTypeKey, smplSmt |> SimpleSorterModelType.toString);
                       (runParameters.sortableDataFormatKey, srtableDf |> SortableDataFormat.toString); 
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString) 
                       (runParameters.mutatorParamsKey, mutPrams |> MutatorParams.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! rng = rp.GetRngType()
                    let! strSel = rp.GetSeedSorterPoolSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! repl = rp.GetRepl()
                    let! pfxLibId = rp.GetPrefixLibId()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! sdf = rp.GetSortableDataFormat()
                    let! set = rp.GetSorterEvalType()
                    let! mutPrams = rp.GetMutatorParams()
                    let! mdr = rp.GetModificationRate()
                    return makeQueryParams repl odt rng strSel sem pfxLibId smt sdf set mutPrams mdr
                }

            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)


    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (RandomStandard.Uniform.dbName, RandomStandard.Uniform.db :> IGeneSortDb);
            (RandomMerge.Uniform.dbName, RandomMerge.Uniform.db :> IGeneSortDb) 
            (RandomPrefix.Uniform.dbName, RandomPrefix.Uniform.db :> IGeneSortDb) 
        ]
        |> Map.ofList


    let getDatabaseByName (name: string<databaseName>) : IGeneSortDb =
        match databaseConfigs.TryFind name with
        | Some db -> db
        | None -> failwithf "Database with name %s not found" (UMX.untag name)


    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.databaseName
        let run = run.create spec.databaseName projectName spec.runName spec.runDescription
        runHost.Create db spec run :> IRunHost

