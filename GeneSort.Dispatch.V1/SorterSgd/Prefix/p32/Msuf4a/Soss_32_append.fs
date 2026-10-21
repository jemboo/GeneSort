module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32_append

open FSharp.UMX
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution

let projName = "SorterSgd.Soss.32p4a.Msuf.Append" |> UMX.tag<projectName>

/// Inherit source parameter combinations; append evolution through a later interval.
/// Both interval arguments are zero-based save-interval indices, not generation numbers.
let fromSource (source: run) (sourceInterval: int) (lastInterval: int) : run =
    if source.ExecuteFunctionName <> ExecuteFunctionNames.soss ||
       (source.RunQueryCatalogName <> QueryCatalogNamesRun.sorterSgdMsufSoss &&
        source.RunQueryCatalogName <> QueryCatalogNamesRun.sorterSgdMsufSossAppend) then
        invalidArg (nameof source) "Expected a Soss_32 source run."
    if sourceInterval < 0 || lastInterval <= sourceInterval then
        invalidArg (nameof lastInterval) "The final interval must follow the nonnegative source interval."
    let splitSetup = source.SetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
    let spans =
        source.RunParameterSpans
        |> List.filter (fun (key, _) ->
            not (List.contains key [
                ParamKeys.generationIntervalLastKey
                ParamKeys.sourceProjectNameKey
                ParamKeys.sourceDbNameKey
                ParamKeys.sourceRunNameKey
                ParamKeys.sourceGenerationIntervalKey
                ParamKeys.sourceMutationModKey
                ParamKeys.sourceParameterOverridesKey
                ParamKeys.sourcePoolFileIdKey
                ParamKeys.sourceTestsFileIdKey
            ]))
        |> fun inherited -> inherited @ [
            ParamKeys.sourceProjectNameKey, [%source.ProjectName]
            ParamKeys.sourceDbNameKey, [%source.DatabaseName]
            ParamKeys.sourceRunNameKey, [%source.RunName]
            ParamKeys.sourceGenerationIntervalKey, [string sourceInterval]
            ParamKeys.generationIntervalLastKey, [string lastInterval]
        ]
    SgdRun.create {
        DatabaseName = source.DatabaseName
        ProjectName = projName
        RunName = (UMX.tag (sprintf "%s_Append_%d" %source.RunName sourceInterval))
        Description = (sprintf "Continue %s/%s from interval %d" %source.ProjectName %source.DatabaseName sourceInterval)
        AllowOverwrite = false
        Setup = {
            FunctionName = SetupCatalogNames.sossAppend
            ParameterSpans = if splitSetup then source.SetupParameterSpans else spans
            QueryCatalogName = if splitSetup then source.SetupQueryCatalogName else QueryCatalogNamesSetup.sorterSgdMsufSossAppend
            EnhancerFunctionName = if splitSetup then source.SetupEnhancerFunctionName else SetupEnhancerFunctionNames.sorterSgd
        }
        Execution = {
            FunctionName = ExecuteFunctionNames.soss
            ParameterSpans = spans
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            EnhancerFunctionName = source.RunEnhancerFunctionName
        }
    } {
        MakerNames = source.MakerNames.Value
        GenSaveIntervalsName = source.GenSaveIntervalsName.Value
        GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
    }
    |> run.SgdRun

let Pool_4_Debug () = fromSource (Soss.Pool_32_Debug ()) 1 2
let Pool_32_Gens15 () = fromSource (Soss.Pool_256_Gens15 ()) 14 20
let Pool_4_Debug_Append () = fromSource (Pool_4_Debug ()) 2 3
