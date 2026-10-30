namespace GeneSort.Dispatch.V1.SorterSgd.cc24

open GeneSort.Project.V1.Parameters

module Cc24Params =

    let channelCount = 24<GeneSort.Sorting.channelCount>

    let cc24SetupParams =
        GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.sorterSgdSetupParams
        @ [
            ParamKeys.channelCountKey, [string (int channelCount)]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
        ]
