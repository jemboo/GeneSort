namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.SortingOps
open System
open GeneSort.Model.Sorting
open GeneSort.SortingResults


type sortingEvalBins =
     | Single of singleSortingEvalBins
     | Pairs of pairSortingEvalBins

module SortingEvalBins = 

    let addSorterEval 
                (sortingEvalBins:sortingEvalBins) 
                (sorterEval: sorterEval) 
                (modelTag:modelTag) =
        match sortingEvalBins with
        | Single s -> s.AddSorterEval sorterEval modelTag
        | Pairs p -> PairSortingEvalBins.addSorterEval p sorterEval modelTag

    let getId (sortingEvalBins:sortingEvalBins) : Guid<sortingEvalBinsId> = 
        match sortingEvalBins with
        | Single s -> s.SortingEvalBinsId
        | Pairs p -> PairSortingEvalBins.getId p    

    let makeFromSorting (ting: sorting) : sortingEvalBins =
        match ting with
        | sorting.Single _ -> singleSortingEvalBins.create (Guid.NewGuid() |> UMX.tag<sortingEvalBinsId>)
                              |> Single
        | sorting.Pairs spm -> PairSortingEvalBins.create spm |> Pairs

    let getAllTaggedSorterEvalBins (sortingEvalBins:sortingEvalBins) : (sorterEvalBins * modelTag) seq =
        match sortingEvalBins with
        | Single s -> seq { yield (s.SorterEvalBins, modelTag.Single) }
        | Pairs p -> PairSortingEvalBins.getAllTaggedSorterEvalBins p


    let getPropertyMaps<'t>
                    (sortingEvalBins:sortingEvalBins) 
                    (baseKey:'t) 
                    (baseProperties: Map<string, string>) 
                    : (('t * modelTag * sorterEvalKey) * Map<string, string>) seq =
            match sortingEvalBins with
            | Single s -> 
                SorterEvalBins.getPropertyMaps s.SorterEvalBins baseKey baseProperties
                |> Seq.map (fun ((bk, sek), props) -> 
                    let combinedMap = props |> Map.add "modelTag" (modelTag.Single.ToString())
                    ((bk, modelTag.Single, sek), combinedMap))

            | Pairs p -> 
                PairSortingEvalBins.getAllTaggedSorterEvalBins p 
                |> Seq.collect (fun (seb, mt) -> 
                    let combinedMap = baseProperties |> Map.add "modelTag" (mt.ToString())
                    SorterEvalBins.getPropertyMaps seb baseKey combinedMap
                    |> Seq.map (fun ((bk, sek), props) -> 
                        ((bk, mt, sek), props)))



