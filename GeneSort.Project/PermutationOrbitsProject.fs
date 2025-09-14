
namespace GeneSort.Project

open System
open System.IO

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Core
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sortable
open GeneSort.Sorter.Mp.Sortable
open GeneSort.Model.Mp.Sortable


module PermutationOrbitsProject = 

    let projectDir = "c:\Projects"
    let experimentName = "PermutationOrbits"
    let experimentDesc = "Count Permutation Orbit lengths"

    let randomType = rngType.Lcg
    let sortableArrayType = sortableArrayType.Ints
    let testModelCount = 20<sorterTestModelCount>
    let maxOrbiit = 200000
    
    let sortingWidthValues = 
        [4; 8; 16; 32; 64; 128; 256; 512; 1024] |> List.map(fun d -> d.ToString())

    let sortingWidths() : string*string list =
        (Run.sortingWidthKey, sortingWidthValues)

    let parameterSet = 
        [ sortingWidths(); ]

    let workspace = Workspace.create experimentName experimentDesc projectDir parameterSet

    let executor (workspace: workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {
            try
                Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)
                Run.setCycle run cycle

                let sortingWidth = run |> Run.getSortingWidth

                let firstIndex = (%cycle * %testModelCount) |> UMX.tag<sorterTestModelCount>
                let sorterTestModelGen = MsasORandGen.create randomType sortingWidth maxOrbiit |> SorterTestModelGen.MsasORandGen
                let sorterTestModelSetMaker = sortableTestModelSetMaker.create sorterTestModelGen firstIndex testModelCount
                let sorterTestModelSet = sorterTestModelSetMaker.MakeSortableTestModelSet
                //let sorterTestSet = sorterTestModelSet.makeSortableTestSet sortableArrayType
                //do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestSet |> outputData.SortableTestSet)
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle 
                                          (sorterTestModelSet |> outputData.SortableTestModelSet)
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle 
                                          (sorterTestModelSetMaker |> outputData.SortableTestModelSetMaker)

                Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
            with ex ->
                Console.WriteLine(sprintf "Error in Run %d, Cycle %d: %s" run.Index %cycle ex.Message)
                raise ex
        }

    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let sorterTestCountReportExecutor (workspace: workspace) =
            try
                Console.WriteLine(sprintf 
                                    "Generating Permutation orbit count report for %s in workspace %s" 
                                    (outputDataType.SortableTestModelSet |> OutputDataType.toString ) 
                                    workspace.WorkspaceFolder)

                // Get the folder for SorterTestSet
                let outputFolder = OutputData.getOutputDataFolder workspace outputDataType.SortableTestModelSet
                if not (Directory.Exists outputFolder) then
                    failwith (sprintf "Output folder %s does not exist" outputFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(outputFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

                // Process each file and collect data for each SorterTest
                let summaries : (Guid*int*string*int) list =
                    files
                    |> Seq.map (
                        fun filePath ->
                            try
                                use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let dto = MessagePackSerializer.Deserialize<sortableTestModelSetDto>(stream, options)
                                let sorterTestModelSet = SortableTestModelSetDto.toDomain dto
                                let sorterTestSet = sorterTestModelSet.makeSortableTestSet sortableArrayType

                                let runParams = OutputData.getRunParametersForOutputDataPath filePath
                                let cycle = runParams[Run.cycleKey]

                                let sortableIntTestSet =
                                    match sorterTestSet with
                                    | sortableTestSet.Ints intTestSet -> intTestSet
                                    | sortableTestSet.Bools _ -> failwith "Expected Ints sorterTestSet"

                                let sortableTestData =
                                    sortableIntTestSet.sortableTests |> Array.map (
                                        fun sorterTest ->
                                            (%sorterTest.Id, 
                                             %(sorterTestSet |> SortableTestset.getSortingWidth), 
                                             cycle, 
                                             sorterTest.Count))

                                sortableTestData
                            with e ->
                                failwith (sprintf "Error processing file %s: %s" filePath e.Message)
                    )
                    |> Array.concat
                    |> Seq.toList

                // Generate the Markdown report, one line per SorterTest
                let reportContent =
                    [ "Permutation orbit count report"
                      sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                      sprintf "Workspace: %s" workspace.WorkspaceFolder
                      ""
                      "Id\t Sorting\t Width\t Cycle\t Count"
                    ]
                    @ (summaries
                       |> List.map (fun (id, sortingWidth, cycle, count) ->
                           sprintf "%s \t%d \t%s \t%d" (id.ToString()) sortingWidth cycle count))
                    |> String.concat "\n"


                // Save the report to a file
                let reportFilePath = Path.Combine(
                        workspace.WorkspaceFolder, 
                        sprintf "%s_PermutationOrbitCountReport_%s.txt" 
                                    (outputDataType.SortableTestSet |> OutputDataType.toString )
                                    (DateTime.Now.ToString("yyyyMMdd_HHmmss"))
                        )
                File.WriteAllText(reportFilePath, reportContent)

                Console.WriteLine(sprintf "Permutation orbit count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating Permutation orbit count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex

        

    let RunAll() =
        for i in 0 .. 0 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 6 executor



    // Function to run the SorterTest count report executor
    let RunPermuationOrbitCountReport() =
        sorterTestCountReportExecutor workspace
















