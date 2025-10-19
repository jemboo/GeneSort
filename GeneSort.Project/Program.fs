// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System
open System.Threading
open GeneSort.Project.OutputDataFile
open GeneSort.Db

let startTime = System.DateTime.Now
printfn $"**** QQQ ******** {startTime.ToString()}"

//OutputData.saveToFile FullBoolEvals.project.ProjectFolder None (FullBoolEvals.project |> outputData.Project)
//FullBoolEvals.RunAll()
//FullBoolEvals.RunSorterEvalReport()

//let wak = 
OutputDataFile.saveToFile MergeIntEvals.project.ProjectFolder None (MergeIntEvals.project |> outputData.Project)
        |> Async.RunSynchronously
//MergeIntEvals.RunAll()
//MergeIntEvals.RunSorterEvalReport()

//let res = OutputData.saveToFile RandomSorters4to64.project.ProjectFolder None (FullBoolEvals.project |> outputData.Project) |> Async.RunSynchronously
//RandomSorters4to64.RunAll()

let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

let yab = Console.ReadLine()







//// Test reading run parameters from a project and printing them
//let testProject = project.Test
    
//// Cancellation token source (press Ctrl+C to cancel)
//let cts = new CancellationTokenSource()
//Console.CancelKeyPress.AddHandler(fun _ _ -> cts.Cancel())
    
//// Progress reporter that prints to console
//let progress = 
//    { new IProgress<string> with
//        member _.Report(msg) = printfn "%s" msg }
    
//// Run the async function synchronously for console (use Async.RunSynchronously)
//try
//    let result = getRunParametersAsync testProject cts.Token progress |> Async.RunSynchronously
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