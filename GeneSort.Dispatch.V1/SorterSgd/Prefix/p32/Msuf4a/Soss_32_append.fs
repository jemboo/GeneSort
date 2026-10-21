module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32_append

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1.SorterSgd

let projName = "SorterSgd.Soss_append.32p4a.Msuf" |> UMX.tag<projectName>

let Pool_4_Debug () : run =
    let sourceParameterSpans =
        [
            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string
        ]
    let executionParameterSpans =
        [
            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [32; 64] |> List.map string
            ParamKeys.generationIntervalLastKey, [2] |> List.map string
        ]
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = Soss.dbPool4name
        RunName = "Pool_4_DebugGenSoss_Append" |> UMX.tag<runName>
        Source = {
            Run = Soss.Pool_32_Debug ()
            GenerationInterval = 1
            ParameterSpans = sourceParameterSpans
        }
        ExecutionParameterSpans = executionParameterSpans
    }

let Pool_32_Gens15 () : run =
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = Soss.dbPool32name
        RunName = "Pool_32_Gens15_Append" |> UMX.tag<runName>
        Source = {
            Run = Soss.Pool_256_Gens15 ()
            GenerationInterval = 14
            ParameterSpans = [ParamKeys.mutationModKey, [0 .. 3] |> List.map string]
        }
        ExecutionParameterSpans = [
            ParamKeys.mutationModKey, [0 .. 3] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, [256; 512] |> List.map string
            ParamKeys.generationIntervalLastKey, [20] |> List.map string
        ]
    }

let Pool_4_Debug_Append () : run =
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = Soss.dbPool4name
        RunName = "Pool_4_DebugGenSoss_Append_Again" |> UMX.tag<runName>
        Source = {
            Run = Pool_4_Debug ()
            GenerationInterval = 2
            // Select modifier zero at size 64 from each original source lineage.
            ParameterSpans = [
                ParamKeys.mutationModKey, ["0"]
                ParamKeys.sorterCountPerPoolSetKey, ["64"]
            ]
        }
        ExecutionParameterSpans = [
            ParamKeys.mutationModKey, [0 .. 3] |> List.map string
            ParamKeys.sorterCountPerPoolSetKey, ["64"]
            ParamKeys.generationIntervalLastKey, ["3"]
        ]
    }
