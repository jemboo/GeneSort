// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System
open System.Threading
open GeneSort.Project.OutputData

let startTime = System.DateTime.Now
printfn $"**** QQQ ******** {startTime.ToString()}"

//WorkspaceOps.saveWorkspace FullBoolEvals.workspace
//FullBoolEvals.RunAll()
//FullBoolEvals.RunSorterEvalReport()


//WorkspaceOps.saveWorkspace MergeIntEvals.workspace
//MergeIntEvals.RunAll()
MergeIntEvals.RunSorterEvalReport()


let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

let yab = Console.ReadLine()







//// Test reading run parameters from a workspace and printing them
//let testWorkspace = workspace.Test
    
//// Cancellation token source (press Ctrl+C to cancel)
//let cts = new CancellationTokenSource()
//Console.CancelKeyPress.AddHandler(fun _ _ -> cts.Cancel())
    
//// Progress reporter that prints to console
//let progress = 
//    { new IProgress<string> with
//        member _.Report(msg) = printfn "%s" msg }
    
//// Run the async function synchronously for console (use Async.RunSynchronously)
//try
//    let result = getRunParametersAsync testWorkspace cts.Token progress |> Async.RunSynchronously
//    printfn "Successfully retrieved %d run parameters:" result.Length
//    for param in result do
//        printfn "%A" param  // Print each RunParameters (adjust based on your type)
//    //0  // Success
//with
//| :? OperationCanceledException -> 
//    printfn "Operation canceled."
//    //1
//| ex -> 
//    printfn "Error: %s" ex.Message
//    //1