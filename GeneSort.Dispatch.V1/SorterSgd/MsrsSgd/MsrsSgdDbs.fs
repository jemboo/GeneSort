namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

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

module MsrsSgdDbs =
    
    let projectName = "SorterSgd.Msrs" |> UMX.tag<projectName>

    module RandomStandard =

        module Uniform =

            let dbName = "Rsu" |> UMX.tag<databaseName>
            let dbFolder = 
                    @$"c:\Projects\{projectName}\{%dbName}\Data" |> UMX.tag<pathToRootFolder>



            let makeQueryParams
                            (rng: rngType)
                            (genLast: int<generationNumber>)
                            (sorterCtPerPool: int<sorterCountPerPool>)
                            (sorterPoolCt: int<sorterPoolCount>)
                            (childCt: int<sorterChildCount>)
                            (ses:sorterEvalSelectionType)
                            (sem:sorterEvalMeasure)
                            (semInitial:sorterEvalMeasure)
                            (repl: int<replNumber>) 
                            (sw: int<sortingWidth>)
                            (smt: simpleSorterModelType) 
                            (set: sorterEvalType)
                            (orthoRate: float<orthoRate>)
                            (paraRate: float<paraRate>)
                            (selfSymRate: float<selfSymRate>)
                            (mdr: float<modificationRate>)
                            (dsh: bool<distinctSorterHashes>)
                            (pNm: bool<prioritizeNewMutants>)
                            (odt: outputDataType) : queryParams =
                queryParams.create dbName (Some repl) odt
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.generationLastKey, (Some genLast) |> GenerationNumber.toString)
                       (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                       (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                       (runParameters.sorterChildCountKey, (Some childCt) |> SorterChildCount.toString)
                       (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toString)
                       (runParameters.sorterEvalMeasureInitialKey, semInitial |> SorterEvalMeasure.toString)
                       (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
                       (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) 
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString)
                       (runParameters.orthoRateKey, (Some orthoRate) |> OrthoRate.toString)
                       (runParameters.paraRateKey, (Some paraRate) |> ParaRate.toString)
                       (runParameters.selfSymRateKey, (Some selfSymRate) |> SelfSymRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                       (runParameters.distinctSorterHashesKey, (Some %dsh) |> string)
                       (runParameters.prioritizeNewMutantsKey, (Some %pNm) |> string)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! repl = rp.GetRepl()
                    let! genFirst = rp.GetGenerationFirst()
                    let! genLast = rp.GetGenerationLast()
                    let! genQf = rp.GetQueryWithGenFirst()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! scc = rp.GetSorterChildCount()
                    let! ses = rp.GetSorterEvalSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! sw = rp.GetSortingWidth()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! rng = rp.GetRngType()
                    let! set = rp.GetSorterEvalType()
                    let! ortho = rp.GetOrthoRate()
                    let! para = rp.GetParaRate()
                    let! sym = rp.GetSelfSymRate()
                    let! mdr = rp.GetModificationRate()
                    let! dsh = rp.GetDistinctSorterHashes()
                    let! pNm = rp.GetPrioritizeNewMutants()
                    return if genQf then
                            makeQueryParams rng genFirst scPP spc scc ses sem semi 
                                            repl sw smt set ortho para sym mdr dsh pNm odt 
                            else
                            makeQueryParams rng genLast scPP spc scc ses sem semi 
                                            repl sw smt set ortho para sym mdr dsh pNm odt 
                }
                
            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)


    module RandomMerge =
    
        module Uniform =
            
            let dbName = "Rmu" |> UMX.tag<databaseName>
            let dbFolder = 
                    $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>

            let makeQueryParams
                        (rng: rngType)
                        (genLast: int<generationNumber>)
                        (sorterCtPerPool: int<sorterCountPerPool>)
                        (sorterPoolCt: int<sorterPoolCount>)
                        (childCt: int<sorterChildCount>)
                        (ses:sorterEvalSelectionType)
                        (sem:sorterEvalMeasure)
                        (semInitial:sorterEvalMeasure)
                        (repl: int<replNumber>) 
                        (sortingWidth: int<sortingWidth>)
                        (simpleSorterModelType: simpleSorterModelType)
                        (mergeDimension: int<mergeDimension>) 
                        (mergeSuffixType: mergeSuffixType)
                        (sortableDataFormat: sortableDataFormat) 
                        (set: sorterEvalType)
                        (orthoRate: float<orthoRate>)
                        (paraRate: float<paraRate>)
                        (selfSymRate: float<selfSymRate>)
                        (mdr: float<modificationRate>)
                        (dsh: bool<distinctSorterHashes>)
                        (outputDataType: outputDataType) : queryParams =

                queryParams.create 
                    dbName
                    (Some repl)
                    outputDataType
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.generationLastKey, (Some genLast) |> GenerationNumber.toString)
                       (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                       (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                       (runParameters.sorterChildCountKey, (Some childCt) |> SorterChildCount.toString)
                       (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toString)
                       (runParameters.sorterEvalMeasureInitialKey, semInitial |> SorterEvalMeasure.toString)
                       (runParameters.sortingWidthKey, string %sortingWidth); 
                       (runParameters.simpleSorterModelTypeKey, simpleSorterModelType |> SimpleSorterModelType.toString );
                       (runParameters.mergeDimensionKey, string %mergeDimension);
                       (runParameters.mergeSuffixTypeKey, mergeSuffixType |> MergeSuffixType.toString);
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString) 
                       (runParameters.orthoRateKey, (Some orthoRate) |> OrthoRate.toString)
                       (runParameters.paraRateKey, (Some paraRate) |> ParaRate.toString)
                       (runParameters.selfSymRateKey, (Some selfSymRate) |> SelfSymRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                       (runParameters.sortableDataFormatKey, sortableDataFormat |> SortableDataFormat.toString);
                       (runParameters.distinctSorterHashesKey, (Some %dsh) |> string)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! rng = rp.GetRngType()
                    let! genFirst = rp.GetGenerationFirst()
                    let! genLast = rp.GetGenerationLast()
                    let! genQf = rp.GetQueryWithGenFirst()
                    let! scPP = rp.GetSorterCountPerPool()
                    let! spc = rp.GetSorterPoolCount()
                    let! scc = rp.GetSorterChildCount()
                    let! ses = rp.GetSorterEvalSelectionType()
                    let! sem = rp.GetSorterEvalMeasure()
                    let! semi = rp.GetSorterEvalMeasureInitial()
                    let! repl = rp.GetRepl()
                    let! sw = rp.GetSortingWidth()
                    let! md = rp.GetMergeDimension()
                    let! mst = rp.GetMergeSuffixType()
                    let! smt = rp.GetSimpleSorterModelType()
                    let! sdf = rp.GetSortableDataFormat()
                    let! set = rp.GetSorterEvalType()
                    let! ortho = rp.GetOrthoRate()
                    let! para = rp.GetParaRate()
                    let! sym = rp.GetSelfSymRate()
                    let! mdr = rp.GetModificationRate()
                    let! dsh = rp.GetDistinctSorterHashes()
                    return if genQf then
                            makeQueryParams rng genFirst scPP spc scc ses sem semi repl sw smt md mst sdf set ortho para sym mdr dsh odt
                            else
                            makeQueryParams rng genLast scPP spc scc ses sem semi repl sw smt md mst sdf set ortho para sym mdr dsh odt
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
        let db = getDatabaseByName spec.databaseName
        let run = run.create spec.databaseName spec.runName spec.runDescription
        runHost.Create db spec run :> IRunHost

