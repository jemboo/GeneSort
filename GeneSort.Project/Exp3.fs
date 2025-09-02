
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

            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)

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
                let outputDataType = "SorterSetEvalSamples"
                Console.WriteLine(sprintf "Generating SorterTest count report for %s in workspace %s" outputDataType workspace.WorkspaceFolder)
                let outputFolder = OutputData.getOutputDataFolder workspace outputDataType
                if not (Directory.Exists outputFolder) then
                    failwith (sprintf "Output folder %s does not exist" outputFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(outputFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


                // Process each file and collect data for each SorterTest
                let (summaries : (Guid*int*int) array) =
                    files
                    |> Seq.map (fun filePath ->
                        async {
                            try
                                use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let! dto = MessagePackSerializer.DeserializeAsync<sorterSetEvalSamplesDto>(stream, options).AsTask() |> Async.AwaitTask
                                //let sorterTestSet = SorterSetEvalSamplesDto.fromSorterSetEvalSamplesDto dto
                                //let sorterIntTestSet = 
                                //    match sorterTestSet with
                                //    | sorterTestSet.Ints intTestSet -> intTestSet
                                //    | sorterTestSet.Bools _ -> failwith "Expected Ints sorterTestSet"

                                //let sorterTestData =
                                //    sorterIntTestSet.sorterTests
                                //    |> Array.map (fun sorterTest ->
                                //        (%sorterTest.Id, %(sorterTestSet |> SorterTestset.getSortingWidth), sorterTest.Count))
                                //return Some sorterTestData
                                return Some [| (%dto.SorterSetEvalId, dto.TotalSampleCount, dto.MaxBinCount) |]
                            with e ->
                                printfn "Error processing file %s: %s" filePath e.Message
                                return None
                        }
                    )
                    |> Seq.toList
                    |> Async.Parallel
                    |> Async.RunSynchronously
                    |> Array.choose id
                    |> Array.concat // Combine all SorterTest data from all SorterTestSets





                // Generate the Markdown report, one line per SorterTest
                let reportContent =
                    [ "# sorterEval Report"
                      sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                      sprintf "Workspace: %s" workspace.WorkspaceFolder
                      ""
                      "| Id | Sorting Width | Count |"
                      "|----|---------------|-------|"
                    ]
                    @ (summaries
                       |> Array.map (fun (id, sortingWidth, count) ->
                           sprintf "| %s | %d | %d |" (id.ToString()) sortingWidth count)
                       |> Array.toList)
                    |> String.concat "\n"


                // Save the report to a file
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_SorterEvalReport_%s.md" outputDataType (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                do! File.WriteAllTextAsync(reportFilePath, reportContent) |> Async.AwaitTask

                Console.WriteLine(sprintf "SorterTest count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating SorterTest count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex
        }



    let RunAll() =
        for i in 0 .. 2 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 6 executor


    let RunSorterEvalReport() =
        Async.RunSynchronously (sorterEvalReportExecutor workspace)









