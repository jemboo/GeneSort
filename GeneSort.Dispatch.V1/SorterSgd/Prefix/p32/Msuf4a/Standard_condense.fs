module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Standard_condense

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Dispatch.V1.SorterSgd

let projName = "SorterSgd.Standard_condense.32p4a.Msuf" |> UMX.tag<projectName>

let dbPool32name = "Pool_32" |> UMX.tag<databaseName>

let Pool_32_Test () : run =
    // Rank only these source MutationMods during preparation.
    let sourceParameterSpans =
        [
            ParamKeys.selectedSorterPoolCountKey, [2] |> List.map string
            ParamKeys.mutationModKey, [0 .. 3] |> List.map string
        ]
    let executionParameterSpans =
        [
            ParamKeys.generationIntervalLastKey, [3] |> List.map string
        ]
    EvoAppend.create {
        ProjectName = projName
        DatabaseName = dbPool32name
        RunName = "Pool_32_TestGenStandard_Condense" |> UMX.tag<runName>
        Source = {
            Run = Standard.Pool_32_Test ()
            GenerationInterval = 2
            ParameterSpans = sourceParameterSpans
        }
        ExecutionParameterSpans = executionParameterSpans
    }
