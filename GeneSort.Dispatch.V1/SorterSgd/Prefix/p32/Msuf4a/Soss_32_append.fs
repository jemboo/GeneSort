module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32_append

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution

let projName = "SorterSgd.Soss.32p4a.Msuf.Append" |> UMX.tag<projectName>

/// Inherit source parameter combinations; append evolution through a later interval.
/// Both interval arguments are zero-based save-interval indices, not generation numbers.
let fromSource (source: run) (sourceInterval: int) (lastInterval: int) : run =
    if source.ExecuteFunctionName <> ExecuteFunctionNames.soss || source.QueryCatalogName <> QueryCatalogNames.sorterSgdMsufSoss then
        invalidArg (nameof source) "Expected a Soss_32 source run."
    if sourceInterval < 0 || lastInterval <= sourceInterval then
        invalidArg (nameof lastInterval) "The final interval must follow the nonnegative source interval."
    let spans =
        source.ParameterSpans
        |> List.filter (fun (key, _) -> key <> runParameters.generationIntervalLastKey)
        |> fun inherited -> inherited @ [
            runParameters.sourceProjectNameKey, [%source.ProjectName]
            runParameters.sourceDbNameKey, [%source.DatabaseName]
            runParameters.sourceRunNameKey, [%source.RunName]
            runParameters.sourceGenerationIntervalKey, [string sourceInterval]
            runParameters.generationIntervalLastKey, [string lastInterval]
        ]
    SgdRun.create source.MakerNames.Value ExecuteFunctionNames.soss
        source.DatabaseName projName (UMX.tag (sprintf "%s_Append_%d" %source.RunName sourceInterval))
        (sprintf "Continue %s/%s from interval %d" %source.ProjectName %source.DatabaseName sourceInterval)
        spans source.GenSaveIntervalsName.Value source.GenSaveSubIntervalsName.Value
        QueryCatalogNames.sorterSgdMsufSossAppend SetupCatalogNames.sossAppend
        source.EnhancerCatalogName false
    |> run.SgdRun

let Pool_4_Debug () = fromSource (Soss_32.Pool_4_Debug ()) 1 2
let Pool_32_Gens15 () = fromSource (Soss_32.Pool_32_Gens15 ()) 14 20
