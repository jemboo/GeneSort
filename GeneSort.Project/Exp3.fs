
namespace GeneSort.Project

open System
open System.IO

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Project.Params
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs


open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.SortingResults.Mp

module Exp3 = 

    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let excludeSelfCe = true
    let sortableArrayType = SortableArrayType.Bools
    //let testModelCount = 10<sorterTestModelCount>
    let parameterSet = 
        [ SwFull.practicalFullTestVals(); SorterModelKey.allButMusf6Kvps() ]

    let workspace = Workspace.create "Exp3" "Exp3" projectDir parameterSet


    let executor (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d  Cycle %d  %A" run.Index %cycle run.Parameters)

            let sorterModelKey = (run.Parameters["SorterModel"]) |> SorterModelKey.fromString
            let swFull = (run.Parameters["SortingWidth"]) |> SwFull.fromString
            let sortingWidth = swFull |> SwFull.toSortingWidth
            let ceCount = SortingSuccess.getCeCountForFull sortingSuccess.P999 sortingWidth

            let stageCount = SortingSuccess.getStageCountForFull sortingSuccess.P999 sortingWidth
            let opsGenRatesArray = OpsGenRatesArray.createUniform %stageCount
            let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageCount %sortingWidth

            let modelMaker =
                match sorterModelKey with
                | SorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceCount) |> SorterModelMaker.SmmMsceRandGen
                | SorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageCount) |> SorterModelMaker.SmmMssiRandGen
                | SorterModelKey.Msrs -> (MsrsRandGen.create randomType sortingWidth opsGenRatesArray) |> SorterModelMaker.SmmMsrsRandGen
                | SorterModelKey.Msuf4 -> (Msuf4RandGen.create randomType sortingWidth stageCount uf4GenRatesArray) |> SorterModelMaker.SmmMsuf4RandGen
                | SorterModelKey.Msuf6 -> failwith "Msuf6 not supported in this experiment"

            let sorterCount = swFull |> SorterCount.getSorterCountForSwFull
            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create modelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            let sorterTestModel = MsasF.create sortingWidth |> SorterTestModel.MsasF
            let sorterTest = SorterTestModel.makeSorterTest sorterTestModel sortableArrayType
            let sorterSetEval = SorterSetEval.makeSorterSetEval sorterSet sorterTest
            let sorterSetEvalSamples = SorterSetEvalSamples.create 10 sorterSetEval

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> OutputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> OutputData.SorterModelSetMaker)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSetEvalSamples |> OutputData.SorterSetEvalSamples)

            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }

    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let sorterEvalReportExecutor (workspace: Workspace) : Async<unit> =
        async {
            try
                Console.WriteLine(sprintf "Generating SorterEval report in workspace %s"  workspace.WorkspaceFolder)
                let sorterSetEvalSamplesFolder = OutputData.getOutputDataFolder workspace "SorterSetEvalSamples"
                if not (Directory.Exists sorterSetEvalSamplesFolder) then
                    failwith (sprintf "Output folder %s does not exist" sorterSetEvalSamplesFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(sorterSetEvalSamplesFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


                // Process each file and collect data for each SorterTest
                let (summaries) = // : (string*string*string) array) =
                    files
                    |> Seq.map (fun ssEvalPath ->
                        async {
                            try
                                use ssEvalStream = new FileStream(ssEvalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let! ssEvalDto = MessagePackSerializer.DeserializeAsync<sorterSetEvalSamplesDto>(ssEvalStream, options).AsTask() |> Async.AwaitTask
                                let ssEvalSampls = SorterSetEvalSamplesDto.toDomain ssEvalDto
                                let runPath = OutputData.getRunFileNameForOutputName workspace.WorkspaceFolder (Path.GetFileNameWithoutExtension ssEvalPath)
                                if not (File.Exists runPath) then
                                    failwith (sprintf "Expected Run file %s to exist" runPath)
                                let runStream = new FileStream(runPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let! runDto = MessagePackSerializer.DeserializeAsync<RunDto>(runStream, options).AsTask() |> Async.AwaitTask
                                let run = RunDto.fromDto runDto
                                let sorterModelKey = (run.Parameters["SorterModel"])
                                let swFull = (run.Parameters["SortingWidth"])
                                let prpt = SorterSetEvalSamples.getBinCountReport ssEvalSampls

                                let appended = prpt |> Array.map(fun aa -> (swFull, sorterModelKey, aa.[0], aa.[1], aa.[2]))
                                return Some appended
                            with e ->
                                printfn "Error processing file %s: %s" ssEvalPath e.Message
                                return None
                        }
                    )
                    |> Seq.toList
                    |> Async.Parallel
                    |> Async.RunSynchronously
                    |> Array.choose id
                    |> Array.concat


                // Generate the Markdown report, one line per SorterTest
                let reportContent =
                    [ "# sorterEval Report"
                      sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                      sprintf "Workspace: %s" workspace.WorkspaceFolder
                      ""
                      "| Sorting Width | SorterModelKey | Count | stageCount | binCount |"
                    ]
                    @ (summaries
                       |> Array.map (
                            fun (sortingWidth, sorterModelKey, ceCount, stageCount, binCount) ->
                                    sprintf "| %s | %s | %s | %s | %s |" sortingWidth sorterModelKey ceCount stageCount binCount)
                       |> Array.toList)
                    |> String.concat "\n"


                // Save the report to a file
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_SorterEvalReport_%s.md" "SorterSetEvalSamples" (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                do! File.WriteAllTextAsync(reportFilePath, reportContent) |> Async.AwaitTask

                Console.WriteLine(sprintf "SorterTest count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating SorterTest count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex
        }

    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let sorterEvalReportExecutor2 (workspace: Workspace) =
            try
                Console.WriteLine(sprintf "Generating SorterEval report in workspace %s"  workspace.WorkspaceFolder)
                let sorterSetEvalSamplesFolder = OutputData.getOutputDataFolder workspace "SorterSetEvalSamples"
                if not (Directory.Exists sorterSetEvalSamplesFolder) then
                    failwith (sprintf "Output folder %s does not exist" sorterSetEvalSamplesFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(sorterSetEvalSamplesFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


                // Process each file and collect data for each SorterTest
                let summaries = // : (string*string*string) array) =
                    files
                    |> Seq.map (
                        fun ssEvalPath ->
                            try
                                use ssEvalStream = new FileStream(ssEvalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let ssEvalDto = MessagePackSerializer.Deserialize<sorterSetEvalSamplesDto>(ssEvalStream, options)
                                let ssEvalSampls = SorterSetEvalSamplesDto.toDomain ssEvalDto
                                let runPath = OutputData.getRunFileNameForOutputName workspace.WorkspaceFolder (Path.GetFileNameWithoutExtension ssEvalPath)
                                if not (File.Exists runPath) then
                                    failwith (sprintf "Expected Run file %s to exist" runPath)
                                let runStream = new FileStream(runPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let runDto = MessagePackSerializer.Deserialize<RunDto>(runStream, options)
                                let run = RunDto.fromDto runDto
                                let sorterModelKey = (run.Parameters["SorterModel"])
                                let swFull = (run.Parameters["SortingWidth"])
                                let prpt = SorterSetEvalSamples.getBinCountReport ssEvalSampls

                                let appended = prpt |> Array.map(fun aa -> (swFull, sorterModelKey, aa.[0], aa.[1], aa.[2]))
                                appended
                            with e ->
                                failwith (sprintf "Error processing file %s: %s" ssEvalPath e.Message)
                    )
                    
                    |> Array.concat
                    |> Seq.toList


                // Generate the Markdown report, one line per SorterTest
                let reportContent =
                    [ "# sorterEval Report"
                      sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                      sprintf "Workspace: %s" workspace.WorkspaceFolder
                      ""
                      "Sorting Width\t SorterModel\t ceCount\t stageCount\t binCount"
                    ]
                    @ (summaries
                       |> List.map (
                            fun (sortingWidth, sorterModelKey, ceCount, stageCount, binCount) ->
                                    sprintf "%s \t %s \t %s \t %s \t %s \t" sortingWidth sorterModelKey ceCount stageCount binCount))
                    |> String.concat "\n"


                // Save the report to a file
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_SorterEvalReport_%s.txt" "SorterSetEvalSamples" (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                File.WriteAllText(reportFilePath, reportContent)

                Console.WriteLine(sprintf "SorterTest count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating SorterTest count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex



    let RunAll() =
        for i in 50 .. 499 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 4 executor


    let RunSorterEvalReport() =
        //Async.RunSynchronously (sorterEvalReportExecutor workspace)
        (sorterEvalReportExecutor2 workspace)









