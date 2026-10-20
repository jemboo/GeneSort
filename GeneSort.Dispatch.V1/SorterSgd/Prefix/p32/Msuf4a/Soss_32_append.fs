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
    if source.ExecuteFunctionName <> ExecuteFunctionNames.soss || source.RunQueryCatalogName <> QueryCatalogNamesRun.sorterSgdMsufSoss then
        invalidArg (nameof source) "Expected a Soss_32 source run."
    if sourceInterval < 0 || lastInterval <= sourceInterval then
        invalidArg (nameof lastInterval) "The final interval must follow the nonnegative source interval."
    let spans =
        source.RunParameterSpans
        |> List.filter (fun (key, _) -> key <> ParamKeys.generationIntervalLastKey)
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
        Execution = {
            FunctionName = ExecuteFunctionNames.soss
            ParameterSpans = spans
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            EnhancerFunctionName = source.RunEnhancerFunctionName
        }
        Setup = {
            FunctionName = SetupCatalogNames.sossAppend
            ParameterSpans = spans
            QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufSossAppend
            EnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
        }
    } {
        MakerNames = source.MakerNames.Value
        GenSaveIntervalsName = source.GenSaveIntervalsName.Value
        GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
    }
    |> run.SgdRun

let Pool_4_Debug () = fromSource (Soss_32.Pool_4_Debug ()) 1 2
let Pool_32_Gens15 () = fromSource (Soss_32.Pool_32_Gens15 ()) 14 20
