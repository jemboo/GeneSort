namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32.p4a

open GeneSort.Project.V1.Parameters
open GeneSort.SortingLib.Sorter
open GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32

module P4aParams =

    let p4aSetupParams =
        Cc32Params.cc32SetupParams
        @ [
            ParamKeys.prefixLibIdKey,
                [prefixLibId.create Cc32Params.channelCount 4<GeneSort.Sorting.stageLength> prefixLibVariant.PrefixA |> PrefixLibId.toString]
        ]
