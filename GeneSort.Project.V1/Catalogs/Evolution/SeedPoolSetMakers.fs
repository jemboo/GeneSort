namespace GeneSort.Project.V1.Catalogs.Evolution

open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core

open GeneSort.SortingOps
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Eval.V1.Sgd.Standard



module SeedPoolSetMakers =

    let private buildStandard
            (context: evolutionMakerContext) (rp:runParameters) : Async<Result<sorterPoolSet_Standard, string>> =
        asyncResult {
            let! sortingWidth = 
                    rp.GetSortingWidth() 
                    |> Result.ofOption "Missing sorting width."

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
                context.LoadStandardSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    GeneSort.SortingOps.sorterEvalType.V2

            let seedSorterModelGen = 
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType
                    excludeSelfCe

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
            (context: evolutionMakerContext) (rp:runParameters) : Async<Result<sorterPoolSet_Standard, string>> =
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
                context.LoadMergeSorterEvals
                    mrgLibid
                    repl
                    simpleSorterModelType



            let seedSorterModelGen = 
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                    rngType 
                    mrgLibid.SortingWidth 
                    simpleSorterModelType
                    excludeSelfCe

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
            (context: evolutionMakerContext) (rp:runParameters) : Async<Result<sorterPoolSet_Standard, string>> =
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
                context.LoadPrefixSorterEvals 
                    pfxLibId
                    repl
                    simpleSorterModelType

            let seedSorterModelGen = 
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                    rngType 
                    pfxLibId.SortingWidth 
                    simpleSorterModelType
                    excludeSelfCe

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


    let private wrapStandard builder context rp =
        builder context rp |> Async.map (Result.map sorterPoolSet.Standard)

    let private wrapSoss builder context rp =
        builder context rp
        |> Async.map (Result.map (GeneSort.Eval.V1.Sgd.Soss.SorterPoolSet_Soss.fromStandard >> sorterPoolSet.Soss))

    let createStandardSeedSorterPoolSetStandard context rp = wrapStandard buildStandard context rp
    let createStandardSeedSorterPoolSetMerge context rp = wrapStandard buildMerge context rp
    let createStandardSeedSorterPoolSetPrefix context rp = wrapStandard buildPrefix context rp
    let createSossSeedSorterPoolSetStandard context rp = wrapSoss buildStandard context rp
    let createSossSeedSorterPoolSetMerge context rp = wrapSoss buildMerge context rp
    let createSossSeedSorterPoolSetPrefix context rp = wrapSoss buildPrefix context rp
