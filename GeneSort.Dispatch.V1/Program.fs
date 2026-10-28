module Program

open System
open GeneSort.Core.Mp
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Reports
open GeneSort.Dispatch.V1

[<EntryPoint>]
let main argv =
    MessagePackSetup.configure ()
    ProjectCatalogs.registerAll ()

    // Each command runs exactly one phase, using the spec configured in its dispatch module.
    //let yab = [| "generate"; "tests" |]
    //let yab = [| "execute"; "tests" |]
    //let yab = [| "setup"; "eval" |]
    //let yab = [| "execute"; "eval" |]
    //let yab = [| "setup"; "sgd" |]
    let yab = [| "execute"; "sgd" |]
    //let yab = [| "report"; "sgd"; "summary" |]

    let command =
        match yab with
        | [| "report"; "sgd"; "summary" |] -> Some (fun () -> DispatchSorterSgd.runReport ReportCatalog.evolutionSummary)
        | [| "setup"; "sgd" |] -> Some DispatchSorterSgd.prepareRun
        | [| "execute"; "sgd" |] -> Some DispatchSorterSgd.runRunParams
        | [| "setup"; "eval" |] -> Some DispatchSorterEval.prepareRun
        | [| "execute"; "eval" |] -> Some DispatchSorterEval.runRunParams
        | [| "setup"; "mutate" |] -> Some DispatchSorterMutate.prepareRun
        | [| "execute"; "mutate" |] -> Some DispatchSorterMutate.runRunParams
        | [| "generate"; "tests" |] -> Some DispatchSortableTests.makeRunParams
        | [| "execute"; "tests" |] -> Some DispatchSortableTests.runRunParams
        | _ -> None

    match command with
    | None ->
        eprintfn "Usage: GeneSort.Dispatch.V1 <setup|execute> <sgd|eval|mutate>, or <generate|execute> tests"
        eprintfn "Set GENESORT_PROJECTS_ROOT to the parent of your project folders."
        2
    | Some action ->
        try
            action ()
            Console.ReadKey() |> ignore
            0
        with error ->
            // Unwrap initialization errors without showing runtime wrappers or stack traces.
            eprintfn "Error: %s" (error.GetBaseException().Message)
            
            Console.ReadKey() |> ignore
            1
