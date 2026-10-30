namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6

open GeneSort.Project.V1.Parameters

module Msuf6Params =

    let msuf6SetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.P3bParams.p3bSetupParams
        @ [ParamKeys.simpleSorterModelTypeKey, ["Msuf6"]]
