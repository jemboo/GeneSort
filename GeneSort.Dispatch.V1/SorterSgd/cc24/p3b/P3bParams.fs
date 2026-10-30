namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p3b

open GeneSort.Project.V1.Parameters

module P3bParams =

    let prefixId =
        GeneSort.SortingLib.Sorter.prefixLibId.create
            GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.channelCount
            3<GeneSort.Sorting.stageLength>
            GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixB

    let p3bSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.Cc24Params.cc24SetupParams
        @ [ParamKeys.prefixLibIdKey, [prefixId |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]]
