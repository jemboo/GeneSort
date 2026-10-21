module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_append

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1.SorterSgd

let projName = "SorterSgd.Standard_append.32p4a.Msuf" |> UMX.tag<projectName>
let dbPool32TestAppendName = "Pool_32_Test_append" |> UMX.tag<databaseName>

let Pool_128_Test () : run =
    let sourceParameterSpans =
        [
            ParamKeys.mutationModKey, [0] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [128] |> List.map string
        ]
    let executionParameterSpans =
        [
            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [128; 256] |> List.map string
            ParamKeys.generationIntervalLastKey, [3] |> List.map string
        ]
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = Standard.dbPool32TestName
        RunName = "Standard_Pool_128-Append" |> UMX.tag<runName>
        Source = {
            Run = Standard.Pool_128_Test ()
            GenerationInterval = 1
            ParameterSpans = sourceParameterSpans
        }
        ExecutionParameterSpans = executionParameterSpans
    }

    
let Pool_128_Test_append () : run =

    let sourceParameterSpans =
        [
            ParamKeys.mutationModKey, [0] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [128] |> List.map string
        ]
    let executionParameterSpans =
        [
            ParamKeys.mutationModKey, [0 .. 7] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [128;] |> List.map string
            ParamKeys.generationIntervalLastKey, [5] |> List.map string
        ]
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = dbPool32TestAppendName
        RunName = "Standard_append_Pool_128_Test-Append" |> UMX.tag<runName>
        Source = {
            Run = Pool_128_Test ()
            GenerationInterval = 3
            ParameterSpans = sourceParameterSpans
        }
        ExecutionParameterSpans = executionParameterSpans
    }

let Pool_512_V1 () : run =
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = Standard.dbPool32name
        RunName = "Pool_512_V1GenStandard_Append" |> UMX.tag<runName>
        Source = { Run = Standard.Pool_512_V1 (); GenerationInterval = 3; ParameterSpans = [] }
        ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["4"]]
    }

let Pool_512_V2a () : run =
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = Standard.dbPool512name
        RunName = "Pool_512_V2aGenStandard_Append" |> UMX.tag<runName>
        Source = { Run = Standard.Pool_512_V2a (); GenerationInterval = 3; ParameterSpans = [] }
        ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["4"]]
    }
