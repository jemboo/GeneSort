
namespace GeneSort.Project

open System
open System.IO
open System.Threading
open System.Threading.Tasks

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model
open GeneSort.Model.Sortable
open GeneSort.Project.Params
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sortable
open GeneSort.Model.Mp.Sortable
open GeneSort.Sorter.Mp.Sortable


module Exp2 = 

    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let sortableArrayType = SortableArrayType.Ints
    let testModelCount = 10<sorterTestModelCount>
    let parameterSet = 
        [ SwFull.standardMapVals(); SorterTestModels.maxOrbit() ]

    let workspace = Workspace.create "Exp2" "Exp2" projectDir parameterSet

    let executor (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {
            try
                Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)

                let sortingWidth =
                    match run.Parameters.TryGetValue("SortingWidth") with
                    | true, value -> value |> SwFull.fromString |> SwFull.toSortingWidth
                    | false, _ -> failwith "SortingWidth parameter not found"
                let maxOrbiit =
                    match Int32.TryParse(run.Parameters["MaxOrbiit"]) with
                    | true, value -> value
                    | false, _ -> failwith "Invalid MaxOrbiit value"

                let firstIndex = (%cycle * %testModelCount) |> UMX.tag<sorterTestModelCount>
                let sorterTestModelGen = MsasORandGen.create randomType sortingWidth maxOrbiit |> SorterTestModelGen.MsasORandGen
                let sorterTestModelSetMaker = SorterTestModelSetMaker.create sorterTestModelGen firstIndex testModelCount
                let sorterTestModelSet = sorterTestModelSetMaker.MakeSorterTestModelSet
                let sorterTestSet = sorterTestModelSet.makeSorterTestSet sortableArrayType
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestSet |> OutputData.SorterTestSet)
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestModelSet |> OutputData.SorterTestModelSet)
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestModelSetMaker |> OutputData.SorterTestModelSetMaker)

                Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
            with ex ->
                Console.WriteLine(sprintf "Error in Run %d, Cycle %d: %s" run.Index %cycle ex.Message)
                raise ex
        }


    let RunAll() =
        for i in 0 .. 20 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 6 executor


    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let sorterTestCountReportExecutor (workspace: Workspace) : Async<unit> =
        async {
            try
                let outputDataType = "SorterTestSet"
                Console.WriteLine(sprintf "Generating SorterTest count report for %s in workspace %s" outputDataType workspace.WorkspaceFolder)

                // Get the folder for SorterTestSet
                let outputFolder = OutputData.getOutputDataFolder workspace outputDataType
                if not (Directory.Exists outputFolder) then
                    failwith (sprintf "Output folder %s does not exist" outputFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(outputFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

                // Process each file and collect data for each SorterTest
                let summaries =
                    files
                    |> Seq.map (fun filePath ->
                        async {
                            try
                                use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let! dto = MessagePackSerializer.DeserializeAsync<sorterTestSetDto>(stream, options).AsTask() |> Async.AwaitTask
                                let sorterTestSet = SorterTestSetDto.fromDto dto
                                let sorterIntTestSet = 
                                    match sorterTestSet with
                                    | sorterTestSet.Ints intTestSet -> intTestSet
                                    | sorterTestSet.Bools _ -> failwith "Expected Ints sorterTestSet"

                                let sorterTestData =
                                    sorterIntTestSet.sorterTests
                                    |> Array.map (fun sorterTest ->
                                        (%sorterTest.Id, %(sorterTestSet |> SorterTestset.getSortingWidth), sorterTest.Count))
                                return Some sorterTestData
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
                    [ "# SorterTest Count Report"
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
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_SorterTestCountReport_%s.md" outputDataType (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                do! File.WriteAllTextAsync(reportFilePath, reportContent) |> Async.AwaitTask

                Console.WriteLine(sprintf "SorterTest count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating SorterTest count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex
        }



    //// New executor to generate a length report for SorterTestSet
    //let lengthReportExecutor (workspace: Workspace) : Async<unit> =
    //    async {
    //        try
    //            let outputDataType = "SorterTestSet"
    //            Console.WriteLine(sprintf "Generating length report for %s in workspace %s" outputDataType workspace.WorkspaceFolder)

    //            // Get the folder for SorterTestSet
    //            let outputFolder = OutputData.getOutputDataFolder workspace outputDataType
    //            if not (Directory.Exists outputFolder) then
    //                failwith (sprintf "Output folder %s does not exist" outputFolder)

    //            // Find all .msgpack files in the output folder
    //            let files = Directory.GetFiles(outputFolder, "*.msgpack")

    //            // Initialize the MessagePack resolver
    //            let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    //            let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    //            // Process each file and collect length data
    //            let summaries =
    //                files
    //                |> Seq.map (fun filePath ->
    //                    async {
    //                        try
    //                            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
    //                            let! dto = MessagePackSerializer.DeserializeAsync<SorterTestSetDto>(stream, options).AsTask() |> Async.AwaitTask
    //                            let sorterTestSet = SorterTestSetDto.fromDto dto
    //                            let id = %sorterTestSet.Id
    //                            let sortingWidth = %sorterTestSet.SortingWidth
    //                            let count = sorterTestSet.sorterTests.Length
    //                            return Some (id, sortingWidth, count)
    //                        with e ->
    //                            printfn "Error processing file %s: %s" filePath e.Message
    //                            return None
    //                    }
    //                )
    //                |> Seq.toList
    //                |> Async.Parallel
    //                |> Async.RunSynchronously
    //                |> Array.choose id // Filter out None values

    //            // Generate the Markdown report
    //            let reportContent =
    //                [ "# SorterTestSet Length Report"
    //                  sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
    //                  sprintf "Workspace: %s" workspace.WorkspaceFolder
    //                  ""
    //                  "| Id | Sorting Width | Count |"
    //                  "|----|---------------|-------|"
    //                ]
    //                @ (summaries
    //                   |> Array.map (fun (id, sortingWidth, count) ->
    //                       sprintf "| %s | %d | %d |" (id.ToString()) %sortingWidth count)
    //                   |> Array.toList)
    //                |> String.concat "\n"

    //            // Save the report to a file
    //            let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_LengthReport_%s.md" outputDataType (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
    //            do! File.WriteAllTextAsync(reportFilePath, reportContent) |> Async.AwaitTask

    //            Console.WriteLine(sprintf "Length report saved to %s" reportFilePath)
    //        with ex ->
    //            Console.WriteLine(sprintf "Error generating length report for %s: %s" "SorterTestSet" ex.Message)
    //            raise ex
    //    }



    // Function to run the SorterTest count report executor
    let RunSorterTestCountReport() =
        Async.RunSynchronously (sorterTestCountReportExecutor workspace)
















