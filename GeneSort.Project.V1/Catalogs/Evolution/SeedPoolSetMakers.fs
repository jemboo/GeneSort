namespace GeneSort.Project.V1.Catalogs.Evolution

open System
open FSharp.UMX
open FsToolkit.ErrorHandling

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters



module SeedPoolSetMakers =

    let private buildStandard
            (sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sorterPoolSet_Standard, string>> =
        asyncResult {
            let! channelCount =
                    rp.GetChannelCount()
                    |> Result.ofOption "Missing channel count."

            let! poolCount = 
                    rp.GetSorterPoolCount() 
                    |> Result.ofOption "Missing poolCount."

            let! sortersPerPool = 
                    rp.GetSorterCountPerPool() 
                    |> Result.ofOption "Missing sortersPerPool."

            let! sorterEvalMeasureInitial = 
                    rp.GetSorterEvalMeasureInitial() 
                    |> Result.ofOption "Missing sorterEvalMeasureInitial."

            let! rngType = 
                    rp.GetRngType() 
                    |> Result.ofOption "Missing rngType."

            let! simpleSorterModelType = 
                    rp.GetSimpleSorterModelType() 
                    |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterSelectionType) =
                    rp.GetSeedSorterPoolSelectionType() 
                    |> Result.ofOption "Missing sorterEvalSelectionType"

            let! (excludeSelfCe: bool<excludeSelfCe>) =
                    rp.GetExcludeSelfCe()
                    |> Result.ofOption "Missing excludeSelfCe"

            let! (mutationMod: int<mutationMod>) = 
                    rp.GetMutationMod() 
                    |> Result.ofOption "Missing mutationMod in run parameters"

            


            let! (parentSorterSetEval: sorterSetEval) = 
                sources.LoadStandardSorterEvals
                    channelCount
                    simpleSorterModelType 
                    GeneSort.SortingOps.sorterEvalType.V2

            let! seedSorterModelGen =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                    rngType 
                    channelCount
                    simpleSorterModelType
                    excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let sorterEvalSelection = 
                SorterSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag) 
                    seedSorterModelGen

            return SorterPoolSet_Standard.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                (sorterEvalSelection.ToEvalLabelMap())
                seedSorterModelSet
                mutationMod
                (latticeBounds.Dim1 %poolCount)
        }


    let private buildMerge
            (sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sorterPoolSet_Standard, string>> =
        asyncResult {

            let! repl =
                   rp.GetRepl()
                   |> Result.ofOption "Missing Repl"

            let! (mrgLibid: mergeLibId) = 
                        rp.GetMergeLibId() 
                        |> Result.ofOption "Missing merge library ID in run parameters"

            let! poolCount = 
                    rp.GetSorterPoolCount() 
                    |> Result.ofOption "Missing poolCount."

            let! sortersPerPool = 
                    rp.GetSorterCountPerPool() 
                    |> Result.ofOption "Missing sortersPerPool."

            let! sorterEvalMeasureInitial = 
                    rp.GetSorterEvalMeasureInitial() 
                    |> Result.ofOption "Missing sorterEvalMeasureInitial."

            let! rngType = 
                    rp.GetRngType() 
                    |> Result.ofOption "Missing rngType."

            let! simpleSorterModelType = 
                    rp.GetSimpleSorterModelType() 
                    |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterSelectionType) =
                    rp.GetSeedSorterPoolSelectionType() 
                    |> Result.ofOption "Missing sorterEvalSelectionType"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) =
                    rp.GetExcludeSelfCe()
                    |> Result.ofOption "Missing excludeSelfCe"
            
            let! (parentSorterSetEval: sorterSetEval) = 
                sources.LoadMergeSorterEvals
                    mrgLibid
                    repl
                    simpleSorterModelType



            let! seedSorterModelGen =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                    rngType 
                    mrgLibid.ChannelCount
                    simpleSorterModelType
                    excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let sorterEvalSelection = 
                SorterSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag<sorterModelSetId>) 
                    seedSorterModelGen

            return SorterPoolSet_Standard.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                (sorterEvalSelection.ToEvalLabelMap())
                seedSorterModelSet
                mutationMod
                (latticeBounds.Dim1 %poolCount)
        }



    let private buildPrefix
            (sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sorterPoolSet_Standard, string>> =
        asyncResult {

            let! repl =
                   rp.GetRepl()
                   |> Result.ofOption "Missing Repl"

            let! pfxLibId = 
                    rp.GetPrefixLibId() 
                    |> Result.ofOption "Missing prefixLibId."

            let! poolCount = 
                    rp.GetSorterPoolCount() 
                    |> Result.ofOption "Missing poolCount."

            let! sortersPerPool = 
                    rp.GetSorterCountPerPool() 
                    |> Result.ofOption "Missing sortersPerPool."

            let! sorterEvalMeasureInitial = 
                    rp.GetSorterEvalMeasureInitial() 
                    |> Result.ofOption "Missing sorterEvalMeasureInitial."

            let! rngType = 
                    rp.GetRngType() 
                    |> Result.ofOption "Missing rngType."

            let! simpleSorterModelType = 
                    rp.GetSimpleSorterModelType() 
                    |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterSelectionType) =
                    rp.GetSeedSorterPoolSelectionType() 
                    |> Result.ofOption "Missing sorterEvalSelectionType"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) =
                    rp.GetExcludeSelfCe()
                    |> Result.ofOption "Missing excludeSelfCe"
            


            let! (parentSorterSetEval: sorterSetEval) = 
                sources.LoadPrefixSorterEvals
                    pfxLibId
                    repl
                    simpleSorterModelType

            let! seedSorterModelGen =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                    rngType 
                    pfxLibId.ChannelCount
                    simpleSorterModelType
                    excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let sorterEvalSelection =
                SorterSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag<sorterModelSetId>) 
                    seedSorterModelGen

            return SorterPoolSet_Standard.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                (sorterEvalSelection.ToEvalLabelMap())
                seedSorterModelSet
                mutationMod
                (latticeBounds.Dim1 %poolCount)
        }


    let private buildSeed builder sources (rp: runParameters) =
        if Map.containsKey ParamKeys.controlledSeedAMaxKey rp.ParamMap then
            async {
                return SeedSorterModelGeneration.makeControlledModelSet rp |> Result.map (fun models ->
                    let labels = models.SorterModels |> Array.mapi (fun i model -> SorterModel.getId model, evalLabel.RankIndex i) |> Map.ofArray
                    SorterPoolSet_Standard.fromSorterModelSet (UMX.tag (Guid.NewGuid()))
                        1<sorterPoolCount> (rp.GetSorterCountPerPool().Value) 0<generationNumber> labels models
                        (rp.GetMutationMod().Value) (latticeBounds.Dim1 1))
            }
        else builder sources rp

    let private wrapStandard builder sources rp =
        buildSeed builder sources rp |> Async.map (Result.map sorterPoolSet.Standard)

    let private wrapSoss builder sources rp =
        buildSeed builder sources rp
        |> Async.map (Result.map (GeneSort.Eval.V1.Sgd.Soss.SorterPoolSet_Soss.fromStandard >> sorterPoolSet.Soss))

    let createStandardSeedSorterPoolSetStandard sources rp = wrapStandard buildStandard sources rp
    let createStandardSeedSorterPoolSetMerge sources rp = wrapStandard buildMerge sources rp
    let createStandardSeedSorterPoolSetPrefix sources rp = wrapStandard buildPrefix sources rp
    let createSossSeedSorterPoolSetStandard sources rp = wrapSoss buildStandard sources rp
    let createSossSeedSorterPoolSetMerge sources rp = wrapSoss buildMerge sources rp
    let createSossSeedSorterPoolSetPrefix sources rp = wrapSoss buildPrefix sources rp
