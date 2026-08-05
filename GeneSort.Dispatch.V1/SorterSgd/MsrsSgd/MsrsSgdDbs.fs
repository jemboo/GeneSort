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
open GeneSort.SortingLib.Sorter

module MsrsSgdDbs =
    
    let projectName = "SorterSgd.Msrs" |> UMX.tag<projectName>

    module Standard =

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
                            (sfrac: float<sortedFraction>)      
                            (sper: int<sorterPoolExpansionRate>)
                            (sli: int<generationNumber>)
                            (mmod: int<mutationMod>)
                            (odt: outputDataType) : queryParams =
                queryParams.create dbName projectName (Some repl) odt
                    [| 
                       (runParameters.rngTypeKey, rng |> RngType.toString)
                       (runParameters.generationLastKey, (Some genLast) |> GenerationNumber.toString)
                       (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                       (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                       (runParameters.sorterChildCountKey, (Some childCt) |> SorterChildCount.toString)
                       (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                       (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString)
                       (runParameters.sorterEvalMeasureInitialKey, semInitial |> SorterEvalMeasure.toCompactString)
                       (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
                       (runParameters.simpleSorterModelTypeKey, smt |> SimpleSorterModelType.toString) 
                       (runParameters.sorterEvalTypeKey, set |> SorterEvalType.toString)
                       (runParameters.orthoRateKey, (Some orthoRate) |> OrthoRate.toString)
                       (runParameters.paraRateKey, (Some paraRate) |> ParaRate.toString)
                       (runParameters.selfSymRateKey, (Some selfSymRate) |> SelfSymRate.toString)
                       (runParameters.modificationRateKey, (Some mdr) |> ModificationRate.toString)
                       (runParameters.distinctSorterHashesKey, (Some %dsh) |> string)
                       (runParameters.prioritizeNewMutantsKey, (Some %pNm) |> string)
                       (runParameters.sortedFractionKey, (Some %sfrac) |> string)
                       (runParameters.sorterPoolExpansionRateKey, (Some %sper) |> string)
                       (runParameters.generationSliceIntervalKey, (Some %sli) |> string)
                       (runParameters.mutationModKey, (Some %mmod) |> string)
                    |]


            let queryParamsFromRunParams 
                                    (rp: runParameters) 
                                    (odt: outputDataType) : queryParams option =
                maybe {
                    let! repl = rp.GetRepl()
                    let! curGen = rp.GetGenerationCurrent()
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
                    let! sfrac = rp.GetSortedFraction()
                    let! sper = rp.GetSorterPoolExpansionRate()
                    let! sli = rp.GetGenerationSliceInterval()
                    let! mmod = rp.GetMutationMod()
                    return makeQueryParams rng curGen scPP spc scc ses
                                           sem semi repl sw smt set ortho 
                                           para sym mdr dsh pNm sfrac sper
                                           sli mmod odt
                }
                
            let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)


    module Merge =
            
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
                    (sfrac: float<sortedFraction>)
                    (sper: int<sorterPoolExpansionRate>)
                    (sli: int<generationNumber>)
                    (mmod: int<mutationMod>)
                    (outputDataType: outputDataType) : queryParams =

            queryParams.create 
                dbName projectName
                (Some repl)
                outputDataType
                [| 
                    (runParameters.rngTypeKey, rng |> RngType.toString)
                    (runParameters.generationLastKey, (Some genLast) |> GenerationNumber.toString)
                    (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                    (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                    (runParameters.sorterChildCountKey, (Some childCt) |> SorterChildCount.toString)
                    (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                    (runParameters.sorterEvalMeasureKey, sem |> SorterEvalMeasure.toCompactString)
                    (runParameters.sorterEvalMeasureInitialKey, semInitial |> SorterEvalMeasure.toCompactString)
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
                    (runParameters.sortedFractionKey, (Some %sfrac) |> string)
                    (runParameters.sorterPoolExpansionRateKey, (Some %sper) |> string)
                    (runParameters.generationSliceIntervalKey, (Some %sli) |> string)
                    (runParameters.mutationModKey, (Some %mmod) |> string)
                |]


        let queryParamsFromRunParams 
                                (rp: runParameters) 
                                (odt: outputDataType) : queryParams option =
            maybe {
                let! rng = rp.GetRngType()
                let! curGen = rp.GetGenerationCurrent()
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
                let! sfrac = rp.GetSortedFraction()
                let! sper = rp.GetSorterPoolExpansionRate()
                let! sli = rp.GetGenerationSliceInterval()
                let! mmod = rp.GetMutationMod()
                return makeQueryParams rng curGen scPP spc scc ses sem
                                        semi repl sw smt md mst sdf set 
                                        ortho para sym mdr dsh sfrac sper
                                        sli mmod odt
                                        
            }

        let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)


    module Prefix =
            
        let dbName = "Prefix" |> UMX.tag<databaseName>
        let dbFolder = 
                $"c:\\Projects\\{projectName}\\{%dbName}\\Data" |> UMX.tag<pathToRootFolder>


        let makeQueryParams
                    (repl: int<replNumber>)
                    (rng: rngType)
                    (genCurrent: int<generationNumber>)
                    (sorterCtPerPool: int<sorterCountPerPool>)
                    (sorterCountCycle: int<sorterCountCycle>)
                    (sorterCountCycleMultiplier: float<sorterCountCycleMultiplier>)
                    (sorterPoolCt: int<sorterPoolCount>)
                    (childCt: int<sorterChildCount>)
                    (ses:sorterEvalSelectionType)
                    (semEvo:sorterEvalMeasure)
                    (semInitial:sorterEvalMeasure)
                    (sorterLibId: sorterLibId)
                    (simpleSorterModelType: simpleSorterModelType)
                    (sortableDataFormat: sortableDataFormat) 
                    (sorterEvalType: sorterEvalType)
                    (seedModRate:float<seedModificationRate>)
                    (orthoRate: float<orthoRate>)
                    (paraRate: float<paraRate>)
                    (selfSymRate: float<selfSymRate>)
                    (modRate: float<modificationRate>)
                    (dsh: bool<distinctSorterHashes>)
                    (pNm: bool<prioritizeNewMutants>)
                    (sfrac: float<sortedFraction>)
                    (sper: int<sorterPoolExpansionRate>)
                    (sli: int<generationNumber>)
                    (mmod: int<mutationMod>)
                    (outputDataType: outputDataType) : queryParams =

            queryParams.create 
                dbName projectName
                (Some repl)
                outputDataType
                [| 
                    (runParameters.rngTypeKey, rng |> RngType.toString)
                    (runParameters.generationCurrentKey, (Some genCurrent) |> GenerationNumber.toString)
                    (runParameters.sorterCountPerPoolKey, (Some sorterCtPerPool) |> SorterCountPerPool.toString)
                    (runParameters.sorterCountCycleKey, (Some sorterCountCycle) |> SorterCountCycle.toString)
                    (runParameters.sorterCountCycleMultiplierKey, (Some sorterCountCycleMultiplier) |> SorterCountCycleMultiplier.toString)
                    (runParameters.sorterPoolCountKey, (Some sorterPoolCt) |> SorterPoolCount.toString)
                    (runParameters.sorterChildCountKey, (Some childCt) |> SorterChildCount.toString)
                    (runParameters.sorterEvalSelectionType, ses |> SorterEvalSelectionType.toString)
                    (runParameters.sorterEvalMeasureKey, semEvo |> SorterEvalMeasure.toCompactString)
                    (runParameters.sorterEvalMeasureInitialKey, semInitial |> SorterEvalMeasure.toCompactString)
                    (runParameters.sortableTestFilterKey, SorterLibId.toString sorterLibId);
                    (runParameters.simpleSorterModelTypeKey, simpleSorterModelType |> SimpleSorterModelType.toString );
                    (runParameters.sorterEvalTypeKey, sorterEvalType |> SorterEvalType.toString) 
                    (runParameters.seedModificationRateKey, (Some seedModRate) |> SeedModificationRate.toString)
                    (runParameters.orthoRateKey, (Some orthoRate) |> OrthoRate.toString)
                    (runParameters.paraRateKey, (Some paraRate) |> ParaRate.toString)
                    (runParameters.selfSymRateKey, (Some selfSymRate) |> SelfSymRate.toString)
                    (runParameters.modificationRateKey, (Some modRate) |> ModificationRate.toString)
                    (runParameters.sortableDataFormatKey, sortableDataFormat |> SortableDataFormat.toString); 
                    (runParameters.distinctSorterHashesKey, (Some %dsh) |> string)
                    (runParameters.prioritizeNewMutantsKey, (Some %pNm) |> string)
                    (runParameters.sortedFractionKey, (Some %sfrac) |> string)
                    (runParameters.sorterPoolExpansionRateKey, (Some %sper) |> string)
                    (runParameters.generationSliceIntervalKey, (Some %sli) |> string)
                    (runParameters.mutationModKey, (Some %mmod) |> string)
                |]


        let queryParamsFromRunParams 
                                (rp: runParameters) 
                                (odt: outputDataType) : queryParams option =
            maybe {
                let! repl = rp.GetRepl()
                let! rng = rp.GetRngType()
                let! curGen = rp.GetGenerationCurrent()
                let! scPP = rp.GetSorterCountPerPool()
                let! sctc = rp.GetSorterCountCycle()
                let! sctm = rp.GetSorterCountCycleMultiplier()
                let! spc = rp.GetSorterPoolCount()
                let! scc = rp.GetSorterChildCount()
                let! ses = rp.GetSorterEvalSelectionType()
                let! sem = rp.GetSorterEvalMeasure()
                let! semi = rp.GetSorterEvalMeasureInitial()
                let! slId = rp.GetSortableTestFilter()
                let! smt = rp.GetSimpleSorterModelType()
                let! sdf = rp.GetSortableDataFormat()
                let! set = rp.GetSorterEvalType()
                let! sdMdr = rp.GetSeedModificationRate()
                let! ortho = rp.GetOrthoRate()
                let! para = rp.GetParaRate()
                let! sym = rp.GetSelfSymRate()
                let! mdr = rp.GetModificationRate()
                let! dsh = rp.GetDistinctSorterHashes()
                let! pNm = rp.GetPrioritizeNewMutants()
                let! sfrac = rp.GetSortedFraction()
                let! sper = rp.GetSorterPoolExpansionRate()
                let! sli = rp.GetGenerationSliceInterval()
                let! mmod = rp.GetMutationMod()
                return makeQueryParams repl rng curGen scPP sctc sctm 
                                       spc scc ses sem semi slId smt 
                                       sdf set sdMdr ortho para sym mdr 
                                       dsh pNm sfrac sper sli mmod
                                       odt

            }


        let db = new GeneSortDbMp(dbFolder, queryParamsFromRunParams)



    let databaseConfigs : Map<string<databaseName>, IGeneSortDb> = 
        [ 
            (Standard.Uniform.dbName, Standard.Uniform.db :> IGeneSortDb);
            (Merge.dbName, Merge.db :> IGeneSortDb) 
            (Prefix.dbName, Prefix.db :> IGeneSortDb)
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

