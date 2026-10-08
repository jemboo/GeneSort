namespace GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msuf63b

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.SortingLib.Sorter

module Common =

    let projName = "SorterSgd.Prfefix.Msuf624p3b" |> UMX.tag<projectName>
    let seedSorterCount = 512

    let projectParams (rp: runParameters) =
        let sorterEvalSelectionType = sorterSelectionType.GuidOrder (seedSorterCount |> UMX.tag<sorterCount>)
        let pfxLibId = prefixLibId.create (24<sortingWidth>) (3<stageLength>) prefixLibVariant.PrefixB

        rp.WithRngType(Some rngType.Lcg)
          .WithCollectNewSortableTests(false |> UMX.tag<collectNewSortableTests> |> Some)
          .WithExcludeSelfCe(true |> UMX.tag<excludeSelfCe> |> Some)
          .WithSorterChildCount(Some 1<sorterChildCount>)
          .WithSimpleSorterModelType(Some simpleSorterModelType.Msuf6)
          .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
          .WithDistinctSorterHashes(Some true)
          .WithPrioritizeNewMutants(Some true)
          .WithSortedFraction(Some 0.99<sortedFraction>)
          .WithSeedSorterPoolSelectionType(Some sorterEvalSelectionType)
          .WithPrefixLibId(Some pfxLibId)
          .WithSortingWidth(Some pfxLibId.SortingWidth)
