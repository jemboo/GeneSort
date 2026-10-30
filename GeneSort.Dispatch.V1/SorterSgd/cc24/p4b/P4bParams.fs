namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p4b

open GeneSort.Project.V1.Parameters

module P4bParams =

    let prefixId =
        GeneSort.SortingLib.Sorter.prefixLibId.create
            GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.channelCount
            4<GeneSort.Sorting.stageLength>
            GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixB

    let p4bSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.cc24SetupParams
        @ [ParamKeys.prefixLibIdKey, [prefixId |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]]
