module Program

open System
open System.IO
open FSharp.UMX

open GeneSort.Db
open GeneSort.Core
open GeneSort.FileDb
open GeneSort.Project
open GeneSort.Project.Old
open System.Threading



let rootDir = "c:\Projects" |> UMX.tag<pathToRootFolder>

// Progress reporter that prints to console
let createThreadSafeProgress() =
    let agent = MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()
                printfn "%s" msg
        })
    
    { new IProgress<string> with
        member _.Report(msg) = agent.Post(msg) }

// Usage:
let progress = createThreadSafeProgress()

let startTime = System.DateTime.Now
printfn $"**** QQQ ******** {startTime.ToString()}"




//OutputDataFile.saveToFileAsync
//                    (Path.Combine(rootDir, FullBoolEvals.project.ProjectName))
//                    None 
//                    (FullBoolEvals.project |> outputData.Project)
//               |> Async.RunSynchronously       


//FullBoolEvals.RunAll rootDir progress |> Async.RunSynchronously
//FullBoolEvals.RunSorterEvalReport (Path.Combine(rootDir, FullBoolEvals.project.ProjectName)) progress




//OutputDataFile.saveToFileAsync 
//            (Path.Combine(rootDir, MergeIntEvals.project.ProjectName))
//            None 
//            (MergeIntEvals.project |> outputData.Project)
//        |> Async.RunSynchronously

//MergeIntEvals.RunAll rootDir progress |> Async.RunSynchronously
//MergeIntEvals.RunSorterEvalReport (Path.Combine(rootDir, MergeIntEvals.project.ProjectName)) progress





//let geneSortDb = new GeneSortDbMp(Path.Combine(rootDir, PermutationOrbits.project.ProjectName)) :> IGeneSortDb
//let projParams = queryParams.CreateForProject PermutationOrbits.project.ProjectName
//geneSortDb.saveAsync projParams (PermutationOrbits.project |> outputData.Project) |> Async.RunSynchronously
//PermutationOrbits.RunAll progress |> Async.RunSynchronously



//////// New code for GeneSortDbMp usage


/// **********     RandomSorters4to64   ****************
let geneSortDb = new GeneSortDbMp(rootDir) :> IGeneSortDb
let cts = new CancellationTokenSource()

let queryParams = queryParams.CreateForProject RandomSorters4to64.project.ProjectName
RandomSorters4to64.InitProjectFiles geneSortDb queryParams cts (Some progress) |> Async.RunSynchronously
RandomSorters4to64.ExecuteRuns geneSortDb cts (Some progress) |> Async.RunSynchronously



let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore







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