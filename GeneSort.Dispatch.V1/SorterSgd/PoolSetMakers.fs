namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.SortingOps
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Sgd



module PoolSetMakers =

    let createSeedSorterPoolSetStandard 
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getStandardSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    GeneSort.SortingOps.sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                (sorterEvalSelection.ToEvalLabelMap())
                seedSorterModelSet
        }


    let createSeedSorterPoolSetMerge
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! mergeDimension = rp.GetMergeDimension() |> Result.ofOption "Missing mergeDimension."
            let! mergeSuffixType = rp.GetMergeSuffixType() |> Result.ofOption "Missing mergeSuffixType."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getMergeSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    mergeDimension
                    mergeSuffixType
                    sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag<sorterModelSetId>) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                (sorterEvalSelection.ToEvalLabelMap())
                seedSorterModelSet
        }




    let createSeedSorterPoolSet32pfx4
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! mergeDimension = rp.GetMergeDimension() |> Result.ofOption "Missing mergeDimension."
            let! mergeSuffixType = rp.GetMergeSuffixType() |> Result.ofOption "Missing mergeSuffixType."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getMergeSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    mergeDimension
                    mergeSuffixType
                    sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag<sorterModelSetId>) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                (sorterEvalSelection.ToEvalLabelMap())
                seedSorterModelSet
        }

