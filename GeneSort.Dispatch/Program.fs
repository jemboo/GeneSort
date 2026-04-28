module Program

open System
open FSharp.UMX
open GeneSort.Dispatch.V1
open GeneSort.FileDb.V1
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.ParamOps
open System.Threading


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






let cts = new CancellationTokenSource()
let allowOverwrite = false |> UMX.tag<allowOverwrite>
let maxParallel = 1 // Set a reasonable limit for your machine



let startTime = System.DateTime.Now
printfn $"**** Q22Q888 ******** {startTime.ToString()}"







/// **********     RandomSorterBins   ****************
let host = RandomSorterBins.P1.host
let executor = RandomSorterBins.executor host
let project = RandomSorterBins.project
let projectFolder = RandomSorterBins.projectFolder
let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
let buildQueryParams = RandomSorterBins.makeQueryParamsFromRunParams
let paramRefiner = RandomSorterBins.paramMapRefiner host
let minReplica = 0<replNumber>
let maxReplica = 10<replNumber>




printfn "Initializing Project..."
let initResult = 
        initProjectAndRunFiles
                geneSortDb 
                buildQueryParams 
                cts 
                (Some progress) 
                project 
                minReplica 
                maxReplica 
                allowOverwrite 
                paramRefiner
                host.ParameterSpans
        |> Async.RunSynchronously






match initResult with
| Ok () -> printfn "Project files initialized successfully."
| Error e -> printfn "Init Failed: %s" e


printfn "Executing Runs..."
let execResult = 
    ProjectOps.executeRuns 
                geneSortDb 
                minReplica 
                maxReplica 
                buildQueryParams 
                project.ProjectName 
                allowOverwrite 
                cts 
                (Some progress) 
                executor 
                maxParallel
    |> Async.RunSynchronously


match execResult with
| Ok results -> printfn "Execution complete. %d results processed." results.Length
| Error e -> printfn "Execution Failed: %s" e


























let endTime = System.DateTime.Now
let duration = endTime - startTime
System.Threading.Thread.Sleep(100)
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
