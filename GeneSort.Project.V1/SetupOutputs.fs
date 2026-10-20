namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.SetupParameters

module SetupOutputs =
    /// Carry enhancer-supplied file references alongside the identity of the setup settings.
    let referencesFor (configured: run) (parameters: setupParameters) =
        let settings = SetupFileReferences.withoutReferences parameters
        let identity =
            queryParams.create configured.DatabaseName configured.ProjectName (parameters.GetRepl()) None
                (geneSortDataType.Run configured.RunName) (Map.toArray settings.ParamMap)
        let files = SetupFileReferences.fromSetupParameters parameters |> List.filter (fun (key, _) -> key <> ParamKeys.setupParametersIdKey)
        (ParamKeys.setupParametersIdKey, string identity.Id) :: files

    /// Predicts one output ID per enhanced setup parameter combination for a single replica.
    /// Preserves order and duplicate IDs. Does not execute setup or access storage.
    let getIds
            (simple: simpleRun)
            (repl: int<replNumber>)
            (outputType: geneSortDataType) : Result<Guid<queryParamsId> array, string> =
        try
            ProjectCatalogs.registerAll ()
            let configured = run.SimpleRun simple
            let query =
                QueryParamsCatalogSetup.get configured.SetupQueryCatalogName
                    configured.ProjectName configured.DatabaseName
            let enhance = SetupEnhancerCatalog.get configured.SetupEnhancerFunctionName
            let context = { Run = configured; QueryParamsFromSetupParams = query }
            let parameters =
                SimpleRun.makeSetupParameters repl (Checked.(+) repl 1<replNumber>)
                    (Seq.map (enhance context)) simple
            parameters
            |> Array.mapi (fun index parameters ->
                match query parameters outputType with
                | Some queryParams -> queryParams.Id
                | None ->
                    invalidOp (sprintf "Setup query '%s' could not build %A for run '%s', replica %d, combination %d.\n%s"
                        configured.SetupQueryCatalogName outputType (%configured.RunName) (%repl) index
                        (SetupParameters.reportKvps parameters)))
            |> Ok
        with error -> Error error.Message
