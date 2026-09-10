namespace GeneSort.Dispatch.V1.SorterSgd.Msrs24p3a

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter

module Common =

    let projName = "SorterSgd.Prfefix.Msrs24p3a" |> UMX.tag<projectName>
    let seedSorterCount = 5000

    let standardPoolSzParams (rp:runParameters) =
        let sorterEvalSelectionType = sorterSelectionType.GuidOrder (seedSorterCount |> UMX.tag<sorterCount>)
        let pfxLibId = prefixLibId.create (24<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA

        rp.WithRngType(Some rngType.Lcg)
          .WithCollectNewSortableTests(false |> UMX.tag<collectNewSortableTests> |> Some)
          .WithExcludeSelfCe(true |> UMX.tag<excludeSelfCe> |> Some)
          .WithSorterChildCount(Some 1<sorterChildCount>)
          .WithSimpleSorterModelType(Some simpleSorterModelType.Msrs)
          .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
          .WithDistinctSorterHashes(Some true)
          .WithPrioritizeNewMutants(Some true)
          .WithSortedFraction(Some 0.99<sortedFraction>)
          .WithSorterEvalMeasureInitial(Some SorterEvalMeasure.stageBiased)
          .WithSorterEvalMeasure(Some SorterEvalMeasure.stageBiased)
          .WithSeedPoolSorterEvalSelectionType(Some sorterEvalSelectionType)
          .WithPrefixLibId(Some pfxLibId)
          .WithSortingWidth(Some pfxLibId.SortingWidth)



    let standardStageCrossingsParams (rp:runParameters) =
        let sorterEvalSelectionType = sorterSelectionType.GuidOrder (seedSorterCount |> UMX.tag<sorterCount>)
        let pfxLibId = prefixLibId.create (24<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA

        rp.WithRngType(Some rngType.Lcg)
          .WithCollectNewSortableTests(false |> UMX.tag<collectNewSortableTests> |> Some)
          .WithExcludeSelfCe(true |> UMX.tag<excludeSelfCe> |> Some)
          .WithSorterChildCount(Some 1<sorterChildCount>)
          .WithSimpleSorterModelType(Some simpleSorterModelType.Msrs)
          .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
          .WithDistinctSorterHashes(Some true)
          .WithPrioritizeNewMutants(Some true)
          .WithSortedFraction(Some 0.99<sortedFraction>)
          .WithSorterEvalMeasureInitial(Some SorterEvalMeasure.stageBiased)
          .WithSeedPoolSorterEvalSelectionType(Some sorterEvalSelectionType)
          .WithPrefixLibId(Some pfxLibId)
          .WithSortingWidth(Some pfxLibId.SortingWidth)