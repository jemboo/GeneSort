namespace GeneSort.Dispatch.V1.SorterSgd.OnePool.cc32

open GeneSort.Project.V1.Parameters

module Cc32Params =

    let channelCount = 32<GeneSort.Sorting.channelCount>

    let cc32SetupParams =
        GeneSort.Dispatch.V1.SorterSgd.OnePool.OnePoolParams.onePoolSetupParams
        @ [
            ParamKeys.channelCountKey, [string (int channelCount)]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
        ]
