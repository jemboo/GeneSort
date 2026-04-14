module Program

open System
open FSharp.UMX

open GeneSort.Db
open GeneSort.FileDb
open GeneSort.Project
open GeneSort.Project.SorterBins
open System.Threading
open GeneSort.Runs



let rootDir = "c:\Projects" |> UMX.tag<pathToProjectFolder>

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


let cts = new CancellationTokenSource()
let allowOverwrite = false |> UMX.tag<allowOverwrite>
let maxParallel = 1 // Set a reasonable limit for your machine


///// **********     RandomSorters   ****************
//let host = RandomSorters.P1.host
//let executor = RandomSorters.executor host
//let project = RandomSorters.project
//let projectFolder = RandomSorters.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = RandomSorters.makeQueryParamsFromRunParams
//let paramRefiner = RandomSorters.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 10<replNumber>


///// **********     SortableMergeTests   ****************
//let host = SortableMergeTests.P1.host
//let project = SortableMergeTests.project
//let executor = SortableMergeTests.executor host
//let projectFolder = SortableMergeTests.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = SortableMergeTests.makeQueryParamsFromRunParams
//let paramRefiner = SortableMergeTests.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>


/// **********     MergeIntEvals   ****************
//let host = MergeIntEvals.P1.host
//let executor = MergeIntEvals.executor host
//let project = MergeIntEvals.project
//let projectFolder = MergeIntEvals.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = MergeIntEvals.makeQueryParamsFromRunParams
//let paramRefiner = MergeIntEvals.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>



/// **********     MergeIntQa   ****************
//let host = MergeIntQa.P1.host
//let project = MergeIntQa.project
//let executor = MergeIntQa.executor host
//let projectFolder = MergeIntQa.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = MergeIntQa.makeQueryParamsFromRunParams
//let paramRefiner = MergeIntQa.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>


///// **********    FullBoolEvals   ****************
//let host = FullBoolEvals.P1.host
//let project = FullBoolEvals.project
//let executor = FullBoolEvals.executor host
//let projectFolder = FullBoolEvals.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = FullBoolEvals.makeQueryParamsFromRunParams
//let paramRefiner = FullBoolEvals.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>



///// **********     RandomSorterBins   ****************
//let host = RandomSorterBins.P1.host
//let executor = RandomSorterBins.executor host
//let project = RandomSorterBins.project
//let projectFolder = RandomSorterBins.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = RandomSorterBins.makeQueryParamsFromRunParams
//let paramRefiner = RandomSorterBins.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 10<replNumber>


///// **********  MergeRandomSorterBins   ****************
//let host = MergeRandomSorterBins.P1.host    
//let executor = MergeRandomSorterBins.executor host
//let project = MergeRandomSorterBins.project
//let projectFolder = MergeRandomSorterBins.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = MergeRandomSorterBins.makeQueryParamsFromRunParams
//let paramRefiner = MergeRandomSorterBins.paramMapRefiner host
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>



/////// **********     RandomMergeSorterBins   ****************
//let executor = RandomMergeSorterBins.executor
//let project = RandomMergeSorterBins.project
//let projectFolder = RandomMergeSorterBins.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = RandomMergeSorterBins.makeQueryParamsFromRunParams
//let paramRefiner = RandomMergeSorterBins.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 4<replNumber>

/////// **********  MergeRandomMergeSorterBins   ****************
//let executor = MergeRandomMergeSorterBins.executor
//let project = MergeRandomMergeSorterBins.project
//let projectFolder = MergeRandomMergeSorterBins.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = MergeRandomMergeSorterBins.makeQueryParamsFromRunParams
//let paramRefiner = MergeRandomMergeSorterBins.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>



///// **********    FullBoolMutate   ****************
//let executor = FullBoolMutate.executor
//let project = FullBoolMutate.project
//let projectFolder = FullBoolMutate.projectFolder
//let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
//let buildQueryParams = FullBoolMutate.makeQueryParamsFromRunParams
//let paramRefiner = FullBoolMutate.paramMapRefiner
//let minReplica = 0<replNumber>
//let maxReplica = 1<replNumber>


/// **********    MutateSortingSet   ****************
let host = MutateSortingSet.mutateSortingSetHost1
let executor = MutateSortingSet.executor host
let project = MutateSortingSet.project
let projectFolder = MutateSortingSet.projectFolder
let geneSortDb = new GeneSortDbMp(projectFolder) :> IGeneSortDb
let buildQueryParams = MutateSortingSet.makeQueryParamsFromRunParams
let paramRefiner = MutateSortingSet.paramMapRefiner
let minReplica = 0<replNumber>
let maxReplica = 1<replNumber>


printfn "Initializing Project..."
let initResult = 
    ProjectOps.initProjectAndRunFiles
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




//printfn "Printing RunParams..."

//ProjectOps.printRunParamsTable geneSortDb minReplica maxReplica cts (Some progress)
//    |> Async.RunSynchronously
//    |> function
//        | Ok ()      -> () // Success, table was printed
//        | Error msg  -> printfn "Failed: %s" msg

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
System.Threading.Thread.Sleep(100)
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
