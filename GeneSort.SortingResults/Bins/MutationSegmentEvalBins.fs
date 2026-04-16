namespace GeneSort.SortingResults.Bins


open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.SortingResults


type mutationSegmentEvalBins =
    private {
        parentSortingResult: sortingResult
        mutantSortingEvalBins: sortingEvalBins
    }

    member this.AddMutantSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        SortingEvalBins.addSorterEval this.mutantSortingEvalBins sorterEval modelTag

    member this.AddParentSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        SortingResult.addSorterEval modelTag sorterEval this.parentSortingResult

    member this.ParentSortingResult with get() = this.parentSortingResult
    member this.MutantSortingEvalBins with get() = this.mutantSortingEvalBins


module MutationSegmentEvalBins =

    let makeFromStorage 
                (parentSortingResult: sortingResult) 
                (mutantSortingEvalBins: sortingEvalBins) =

        {
            parentSortingResult = parentSortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }

    let makeFromSorting (sorting: sorting) : mutationSegmentEvalBins =
        let mutantSortingEvalBins = SortingEvalBins.makeFromSorting sorting
        let sortingResult = SortingResult.makeFromSorting sorting
        {
            parentSortingResult = sortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }

    //let getPropertyMaps<'t>
    //                (mutationSegmentEvalBins:mutationSegmentEvalBins) 
    //                (baseKey:'t) 
    //                (baseProperties: Map<string, string>) = 
    //    match mutationSegmentEvalBins.ParentSortingResult with
    //    | Single ssr -> 
    //        let parentMaps = SortingEvalBins.getPropertyMaps mutationSegmentEvalBins.MutantSortingEvalBins baseKey baseProperties
    //        let parentEval = ssr.SorterEval.Value
    //        let parentMap = baseProperties |> Map.add "modelTag" (modelTag.Single.ToString())
    //        let parentCombinedMap = parentMap |> Map.add "evalType" "parent"
    //        let parentResult = ((baseKey, modelTag.Single, sorterEvalKey.Parent), parentCombinedMap)
    //        Seq.append (Seq.singleton parentResult) parentMaps

    //    | Pairs psr ->
    //        let parentMaps = SortingEvalBins.getPropertyMaps mutationSegmentEvalBins.MutantSortingEvalBins baseKey baseProperties
    //        let parentEvals = psr |> PairsSortingResult.getAllTaggedSorterEvals
    //        let parentResults = 
    //            parentEvals 
    //            |> Seq.map (fun (eval, modelSetTag) -> 
    //                let parentMap = baseProperties |> Map.add "modelTag" (modelSetTag.ModelTag.ToString())
    //                let parentCombinedMap = parentMap |> Map.add "evalType" "parent"
    //                ((baseKey, modelSetTag.ModelTag, sorterEvalKey.Parent), parentCombinedMap))
    //        Seq.append parentResults parentMaps
            