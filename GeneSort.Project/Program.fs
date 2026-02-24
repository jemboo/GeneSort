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
printfn $"**** Q22Q888 ******** {startTime.ToString()}"


let geneSortDb = new GeneSortDbMp(rootDir) :> IGeneSortDb
let cts = new CancellationTokenSource()
let allowOverwrite = false |> UMX.tag<allowOverwrite>
let maxParallel = 1 // Set a reasonable limit for your machine


///// **********     RandomSorters   ****************
//let executor = RandomSorters.executor
//let project = RandomSorters.project
//let projectName = RandomSorters.project.ProjectName
//let projectFolder = RandomSorters.projectFolder
//let buildQueryParams = RandomSorters.makeQueryParamsFromRunParams
//let paramRefiner = RandomSorters.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>


///// **********     SortableMergeTests   ****************
//let project = SortableMergeTests.project
//let executor = SortableMergeTests.executor
//let projectName = SortableMergeTests.projectName
//let projectFolder = SortableMergeTests.projectFolder
//let buildQueryParams = SortableMergeTests.makeQueryParamsFromRunParams
//let paramRefiner = SortableMergeTests.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>


/// **********     MergeIntEvals   ****************
let executor = MergeIntEvals.executor
let project = MergeIntEvals.project
let projectName = MergeIntEvals.project.ProjectName
let projectFolder = MergeIntEvals.projectFolder
let buildQueryParams = MergeIntEvals.makeQueryParamsFromRunParams
let paramRefiner = MergeIntEvals.paramMapRefiner
let minReplica = 0<replNumber>
let maxReplica = 1<replNumber>



/// **********     MergeIntQa   ****************
//let project = MergeIntQa.project
//let executor = MergeIntQa.executor
//let projectName = MergeIntQa.projectName
//let projectFolder = MergeIntQa.projectFolder
//let buildQueryParams = MergeIntQa.makeQueryParamsFromRunParams
//let paramRefiner = MergeIntQa.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>


///// **********    FullBoolEvals   ****************
//let executor = FullBoolEvals.executor
//let project = FullBoolEvals.project
//let projectName = FullBoolEvals.project.ProjectName
//let projectFolder = FullBoolEvals.projectFolder
//let buildQueryParams = FullBoolEvals.makeQueryParamsFromRunParams
//let paramRefiner = FullBoolEvals.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>

///// **********    FullBoolMutate   ****************
//let executor = FullBoolMutate.executor
//let project = FullBoolMutate.project
//let projectName = FullBoolMutate.project.ProjectName
//let projectFolder = FullBoolMutate.projectFolder
//let buildQueryParams = FullBoolMutate.makeQueryParamsFromRunParams
//let paramRefiner = FullBoolMutate.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>

printfn "Initializing Project..."
let initResult = 
    ProjectOps.initProjectFiles geneSortDb projectFolder buildQueryParams cts (Some progress) project minReplica maxReplica allowOverwrite paramRefiner 
    |> Async.RunSynchronously

match initResult with
| Ok () -> printfn "Project files initialized successfully."
| Error e -> printfn "Init Failed: %s" e


printfn "Executing Runs..."
let execResult = 
    ProjectOps.executeRuns geneSortDb projectFolder minReplica maxReplica buildQueryParams projectName allowOverwrite cts (Some progress) executor maxParallel
    |> Async.RunSynchronously

match execResult with
| Ok results -> printfn "Execution complete. %d results processed." results.Length
| Error e -> printfn "Execution Failed: %s" e


//printfn "Printing RunParams..."
//let reportResult = 
//    ProjectOps.printRunParams geneSortDb projectFolder minReplica maxReplica cts (Some progress)
//    |> Async.RunSynchronously

//printfn "Making Use Profile report ..."

//let uPReportResult = 
//    TextReporters.ceUseProfileReportExecutor geneSortDb projectFolder 0<replNumber> maxReplica buildQueryParams allowOverwrite cts (Some progress)
//    |> Async.RunSynchronously


//printfn "Making Use Bin report ..."

//let reportResultBins = 
//    TextReporters.binReportExecutor geneSortDb projectFolder 0<replNumber> maxReplica buildQueryParams allowOverwrite cts (Some progress)
//    |> Async.RunSynchronously


let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
