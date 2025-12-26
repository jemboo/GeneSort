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

///// **********     RandomSorters4to64   ****************
//let executor = RandomSorters4to64.executor
//let project = RandomSorters4to64.project
//let projectName = RandomSorters4to64.project.ProjectName



/// **********     MergeIntEvals   ****************
//let executor = MergeIntEvals.executor
//let project = MergeIntEvals.project
//let projectName = MergeIntEvals.project.ProjectName



/// **********     SortableIntMerges   ****************
let project = SortableIntMerges.project
let executor = SortableIntMerges.executor
let projectName = SortableIntMerges.projectName
let yab = SortableIntMerges.makeQueryParamsFromRunParams
let paramRefiner = SortableIntMerges.paramMapRefiner
let minReplica = 0<replNumber>
let maxReplica = 1<replNumber>


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



ProjectOps.initProjectFiles geneSortDb yab cts (Some progress) project minReplica maxReplica allowOverwrite paramRefiner |> Async.RunSynchronously
ProjectOps.executeRuns geneSortDb yab projectName allowOverwrite cts (Some progress) executor |> Async.RunSynchronously
ProjectOps.reportRuns geneSortDb projectName cts (Some progress) |> Async.RunSynchronously
//TextReporters.ceUseProfileReportExecutor geneSortDb projectName yab allowOverwrite cts (Some progress) |> Async.RunSynchronously
//TextReporters.binReportExecutor geneSortDb projectName yab allowOverwrite cts (Some progress) |> Async.RunSynchronously


let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
