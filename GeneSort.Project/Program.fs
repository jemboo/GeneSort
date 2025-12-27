module Program

open System
open FSharp.UMX

open GeneSort.Db
open GeneSort.FileDb
open GeneSort.Project
open System.Threading
open GeneSort.Runs



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
printfn $"**** Q22Qr ******** {startTime.ToString()}"


let geneSortDb = new GeneSortDbMp(rootDir) :> IGeneSortDb
let cts = new CancellationTokenSource()
let allowOverwrite = false |> UMX.tag<allowOverwrite>
let maxParallel = 4 // Set a reasonable limit for your machine

///// **********     RandomSorters4to64   ****************
let executor = RandomSorters4to64.executor
let project = RandomSorters4to64.project
let projectName = RandomSorters4to64.project.ProjectName
let yab = RandomSorters4to64.makeQueryParamsFromRunParams
let paramRefiner = RandomSorters4to64.paramMapRefiner
let minReplica = 0<replNumber>
let maxReplica = 1<replNumber>


/// **********     MergeIntEvals   ****************
//let executor = MergeIntEvals.executor
//let project = MergeIntEvals.project
//let projectName = MergeIntEvals.project.ProjectName



///// **********     SortableIntMerges   ****************
//let project = SortableIntMerges.project
//let executor = SortableIntMerges.executor
//let projectName = SortableIntMerges.projectName
//let yab = SortableIntMerges.makeQueryParamsFromRunParams
//let paramRefiner = SortableIntMerges.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>

/// **********     MergeIntQa   ****************
//let project = MergeIntQa.project
//let executor = MergeIntQa.executor
//let projectName = MergeIntQa.projectName
//let yab = MergeIntQa.makeQueryParamsFromRunParams
//let paramRefiner = MergeIntQa.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>




/// **********    FullBoolEvals   ****************
//let executor = FullBoolEvals.executor
//let project = FullBoolEvals.project
//let projectName = FullBoolEvals.project.ProjectName


printfn "Initializing Project..."
let initResult = 
    ProjectOps.initProjectFiles geneSortDb yab cts (Some progress) project minReplica maxReplica allowOverwrite paramRefiner 
    |> Async.RunSynchronously

match initResult with
| Ok () -> printfn "Project files initialized successfully."
| Error e -> printfn "Init Failed: %s" e


printfn "Executing Runs..."
let execResult = 
    ProjectOps.executeRuns geneSortDb yab projectName allowOverwrite cts (Some progress) executor maxParallel
    |> Async.RunSynchronously

match execResult with
| Ok results -> printfn "Execution complete. %d results processed." results.Length
| Error e -> printfn "Execution Failed: %s" e



let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
