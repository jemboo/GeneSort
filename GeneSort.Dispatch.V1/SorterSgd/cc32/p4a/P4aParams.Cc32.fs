namespace GeneSort.Dispatch.V1.SorterSgd.cc32.p4a

open GeneSort.Project.V1.Parameters

module P4aParams =

    let prefixId =
        GeneSort.SortingLib.Sorter.prefixLibId.create
            GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.channelCount
            4<GeneSort.Sorting.stageLength>
            GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA

    let p4aSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc32.Cc32Params.cc32SetupParams
        @ [ParamKeys.prefixLibIdKey, [prefixId |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]]
