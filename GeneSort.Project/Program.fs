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


let geneSortDb = new GeneSortDbMp(rootDir) :> IGeneSortDb
let cts = new CancellationTokenSource()


///// **********     RandomSorters4to64   ****************
//let executor = RandomSorters4to64.executor
//let project = RandomSorters4to64.project
//let projectName = RandomSorters4to64.project.ProjectName



/// **********     MergeIntEvals   ****************
let executor = MergeIntEvals.executor
let project = MergeIntEvals.project
let projectName = MergeIntEvals.project.ProjectName


//ProjectOps.initProjectFiles geneSortDb project cts (Some progress) |> Async.RunSynchronously
//ProjectOps.executeRuns geneSortDb projectName cts (Some progress) MergeIntEvals.executor |> Async.RunSynchronously

MergeIntEvals.binReportExecutor geneSortDb projectName cts (Some progress) |> Async.RunSynchronously


let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
