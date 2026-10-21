module GeneSort.Dispatch.V1.SorterSgd.Prefix.p32.Msuf4a.Soss_32_append2

open FSharp.UMX
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution

let projName = "SorterSgd.Soss.32p4a.Msuf.Append2" |> UMX.tag<projectName>

/// Inherit source parameter combinations; append evolution through a later interval.
/// Both interval arguments are zero-based save-interval indices, not generation numbers.
/// Every selected source modifier is paired with every execution modifier.
let fromSourceModifiers (source: run) (sourceInterval: int) (lastInterval: int)
                        (sourceMutationMods: int list) (mutationMods: int list) : run =
    if List.isEmpty sourceMutationMods then
        invalidArg (nameof sourceMutationMods) "At least one source mutation modifier is required."
    let sourceMutationMod = List.head sourceMutationMods
    if List.isEmpty mutationMods then
        invalidArg (nameof mutationMods) "At least one execution mutation modifier is required."
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
                ParamKeys.mutationModKey
            ]))
        |> fun inherited -> inherited @ [
            ParamKeys.sourceProjectNameKey, [%source.ProjectName]
            ParamKeys.sourceDbNameKey, [%source.DatabaseName]
            ParamKeys.sourceRunNameKey, [%source.RunName]
            ParamKeys.sourceGenerationIntervalKey, [string sourceInterval]
            ParamKeys.generationIntervalLastKey, [string lastInterval]
            ParamKeys.sourceParameterOverridesKey,
                (sourceMutationMods |> List.distinct |> List.map (fun modifier ->
                    System.Text.Json.JsonSerializer.Serialize([| [|ParamKeys.mutationModKey; string modifier|] |])))
            ParamKeys.mutationModKey, [string sourceMutationMod]
        ]
    let executionSpans =
        spans
        |> List.map (fun (key, values) ->
            if key = ParamKeys.mutationModKey then key, mutationMods |> List.distinct |> List.map string
            else key, values)
    // Source selection varies in executionSpans. Do not duplicate it in the setup Cartesian product.
    let setupSpans =
        if splitSetup then
            source.SetupParameterSpans
            |> List.map (fun (key, values) ->
                if key = ParamKeys.mutationModKey then key, [string sourceMutationMod]
                else key, values)
        else spans |> List.map (fun (key, values) ->
            if key = ParamKeys.sourceParameterOverridesKey then key, [List.head values] else key, values)
    SgdRun.create {
        DatabaseName = source.DatabaseName
        ProjectName = projName
        RunName = (UMX.tag (sprintf "%s_Append2_%d" %source.RunName sourceInterval))
        Description = (sprintf "Continue %s/%s from interval %d" %source.ProjectName %source.DatabaseName sourceInterval)
        AllowOverwrite = false
        Setup = {
            FunctionName = SetupCatalogNames.sossAppend
            ParameterSpans = setupSpans
            QueryCatalogName = if splitSetup then source.SetupQueryCatalogName else QueryCatalogNamesSetup.sorterSgdMsufSossAppend
            EnhancerFunctionName = if splitSetup then source.SetupEnhancerFunctionName else SetupEnhancerFunctionNames.sorterSgd
        }
        Execution = {
            FunctionName = ExecuteFunctionNames.soss
            ParameterSpans = executionSpans
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            EnhancerFunctionName = source.RunEnhancerFunctionName
        }
    } {
        MakerNames = source.MakerNames.Value
        GenSaveIntervalsName = source.GenSaveIntervalsName.Value
        GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
    }
    |> run.SgdRun

/// Explicitly select one source modifier when a single checkpoint is desired.
let fromSource source sourceInterval lastInterval sourceMutationMod mutationMods =
    fromSourceModifiers source sourceInterval lastInterval [sourceMutationMod] mutationMods

/// Execution spans override setup spans when the source defines MutationMod in both.
let fromAllMutationMods (source: run) sourceInterval lastInterval mutationMods =
    let sourceMods =
        source.RunParameterSpans @ source.SetupParameterSpans
        |> List.tryFind (fun (key, _) -> key = ParamKeys.mutationModKey)
        |> Option.map (snd >> List.map int)
        |> Option.defaultWith (fun () -> invalidArg (nameof source) "The source must define a MutationMod span.")
    fromSourceModifiers source sourceInterval lastInterval sourceMods mutationMods

let Pool_4_Debug () = fromAllMutationMods (Soss.Pool_32_Debug ()) 1 2 [0 .. 15]
let Pool_32_Gens15 () = fromAllMutationMods (Soss.Pool_256_Gens15 ()) 14 20 [0 .. 3]
let Pool_4_Debug_Append () = fromSource (Pool_4_Debug ()) 2 3 0 [0 .. 3]
