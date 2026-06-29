namespace GeneSort.Dispatch.V1.SorterSgd.Msuf4


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

module Msuf4SgdDbs =
    
    let projectName = "SorterSgd.Msuf4" |> UMX.tag<projectName>

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
                            (seedMdr:float<seedModificationRate>)
                            (orthoRate: float<orthoRate>)
                            (paraRate: float<paraRate>)
                            (selfSymRate: float<selfSymRate>)
                            (mdr: float<modificationRate>)
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
                       (runParameters.seedModificationRateKey, (Some seedMdr) |> SeedModificationRate.toString)
                       (runParameters.orthoRateKey, (Some orthoRate) |> OrthoRate.toString)
                       (runParameters.paraRateKey, (Some paraRate) |> ParaRate.toString)
                       (runParameters.selfSymRateKey, (Some selfSymRate) |> SelfSymRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
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
                    let! sdMdr = rp.GetSeedModificationRate()
                    let! ortho = rp.GetOrthoRate()
                    let! para = rp.GetParaRate()
                    let! sym = rp.GetSelfSymRate()
                    let! mdr = rp.GetModificationRate()
                    return if genQf then
                            makeQueryParams rng genFirst scPP spc scc ses sem semi repl sw smt set sdMdr ortho para sym mdr odt 
                            else
                            makeQueryParams rng genLast scPP spc scc ses sem semi repl sw smt set sdMdr ortho para sym mdr odt 
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
                        (seedMdr:float<seedModificationRate>)
                        (orthoRate: float<orthoRate>)
                        (paraRate: float<paraRate>)
                        (selfSymRate: float<selfSymRate>)
                        (mdr: float<modificationRate>)
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
                       (runParameters.seedModificationRateKey, (Some seedMdr) |> SeedModificationRate.toString)
                       (runParameters.orthoRateKey, (Some orthoRate) |> OrthoRate.toString)
                       (runParameters.paraRateKey, (Some paraRate) |> ParaRate.toString)
                       (runParameters.selfSymRateKey, (Some selfSymRate) |> SelfSymRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                       (runParameters.sortableDataFormatKey, sortableDataFormat |> SortableDataFormat.toString); 
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
                    let! sdMdr = rp.GetSeedModificationRate()
                    let! ortho = rp.GetOrthoRate()
                    let! para = rp.GetParaRate()
                    let! sym = rp.GetSelfSymRate()
                    let! mdr = rp.GetModificationRate()
                    return if genQf then
                            makeQueryParams rng genFirst scPP spc scc ses sem semi repl sw smt md mst sdf set sdMdr ortho para sym mdr odt
                            else
                            makeQueryParams rng genLast scPP spc scc ses sem semi repl sw smt md mst sdf set sdMdr ortho para sym mdr odt
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

