namespace GeneSort.SortingResults.Bins


open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Core


type mutationSegmentEvalBins =
    private {
        parentSortingEval: sortingEval
        mutantSortingEvalBins: sortingEvalBins
    }

    member this.AddMutantSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        SortingEvalBins.addSorterEval this.mutantSortingEvalBins sorterEval modelTag

    member this.AddParentSorterEval (sorterEval: sorterEval) (modelTag:modelTag) =
        SortingEval.addSorterEval modelTag sorterEval this.parentSortingEval

    member this.ParentSortingEval with get() = this.parentSortingEval
    member this.MutantSortingEvalBins with get() = this.mutantSortingEvalBins


module MutationSegmentEvalBins =

    let makeFromStorage 
                (parentSortingResult: sortingEval) 
                (mutantSortingEvalBins: sortingEvalBins) =

        {
            parentSortingEval = parentSortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }

    let makeFromSorting (sorting: sorting) : mutationSegmentEvalBins =
        let mutantSortingEvalBins = SortingEvalBins.makeFromSorting sorting
        let sortingResult = SortingEval.makeFromSorting sorting
        {
            parentSortingEval = sortingResult
            mutantSortingEvalBins = mutantSortingEvalBins
        }

    let makeDataTableRecords (mutationSegmentEvalBins: mutationSegmentEvalBins) = //: dataTableRecord seq =

        let taggedParents = SortingEval.getAllTaggedSorterEvals 
                                mutationSegmentEvalBins.ParentSortingEval
                            |> Seq.toArray

    //    let taggedMutants = SortingEvalBins.getAllTaggedSorterEvalBins 
    //                            mutationSegmentEvalBins.MutantSortingEvalBins
    //                        |> Seq.toArray

    //    let mergedRecords = 
    //        seq {
    //            for (parentEval, parentModelTag) in taggedParents do
    //                let matchingMutant = 
    //                    taggedMutants 
    //                    |> Seq.filter (fun (mutantEval, mutantModelTag) -> mutantModelTag = parentModelTag.ModelTag)
    //                    |> Seq.head |> fst

    //                let modelSetTagRecord = ModelSetTag.toDataTableRecord parentModelTag
    //                let parentRecord = parentEval |> SorterEval.toDataTableRecord
    //                None
    //        }
        None



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
            