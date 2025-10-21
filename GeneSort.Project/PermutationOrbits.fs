
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
open System.Threading
open GeneSort.Runs.Params
open GeneSort.Runs
open GeneSort.Db


module PermutationOrbits = 

    let experimentName = "PermutationOrbits"
    let experimentDesc = "Count Permutation Orbit lengths"

    let randomType = rngType.Lcg
    let sortableArrayType = sortableArrayType.Ints
    let testModelCount = 20<sorterTestModelCount>
    let maxOrbiit = 200000
    
    let sortingWidthValues = 
        [4; 8; 16; 32; 64; 128; 256; 512; 1024] |> List.map(fun d -> d.ToString())

    let sortingWidths() : string*string list =
        (runParameters.sortingWidthKey, sortingWidthValues)

    let parameterSet = 
        [ sortingWidths(); ]

    let project = Project.create experimentName experimentDesc [||] parameterSet (fun s -> s)

    let executor 
            (projectFolder: string)
            (runParameters: runParameters) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string>) : Async<unit> =
        async {

            try
                let index = runParameters.GetIndex()
                let repl = runParameters.GetRepl()  
                let sortingWidth = runParameters.GetSortingWidth()

                progress.Report(sprintf "Executing Run %d  %s" index (runParameters.toString()))

                let firstIndex = (%repl * %testModelCount) |> UMX.tag<sorterTestModelCount>
                let sorterTestModelGen = MsasORandGen.create randomType sortingWidth maxOrbiit |> SorterTestModelGen.MsasORandGen
                let sorterTestModelSetMaker = sortableTestModelSetMaker.create sorterTestModelGen firstIndex testModelCount
                let sorterTestModelSet = sorterTestModelSetMaker.MakeSortableTestModelSet
                do! OutputDataFile.saveToFileAsync projectFolder (Some runParameters)
                                          (sorterTestModelSet |> outputData.SortableTestModelSet)
                do! OutputDataFile.saveToFileAsync projectFolder (Some runParameters)
                                          (sorterTestModelSetMaker |> outputData.SortableTestModelSetMaker)

                
                runParameters.SetRunFinished true

                progress.Report(sprintf "Finished executing Run %d  Repl  %d \n" %index %repl)
            with ex ->
                progress.Report(sprintf "Error in Run") // %d, Repl %d: %s" index %repl ex.Message)
                raise ex
        }

    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    //let sorterTestCountReporter (project: project) =
    //        try
    //            Console.WriteLine(sprintf 
    //                                "Generating Permutation orbit count report for %s in project %s" 
    //                                (outputDataType.SortableTestModelSet |> OutputDataType.toString ) 
    //                                project.ProjectFolder)

    //            // Get the folder for SorterTestSet
    //            let outputFolder = OutputData.getOutputDataFolder project outputDataType.SortableTestModelSet
    //            if not (Directory.Exists outputFolder) then
    //                failwith (sprintf "Output folder %s does not exist" outputFolder)

    //            // Find all .msgpack files in the output folder
    //            let files = Directory.GetFiles(outputFolder, "*.msgpack")

    //            // Initialize the MessagePack resolver
    //            let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    //            let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    //            // Process each file and collect data for each SorterTest
    //            let summaries : (Guid*int*string*int) list =
    //                files
    //                |> Seq.map (
    //                    fun filePath ->
    //                        try
    //                            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
    //                            let dto = MessagePackSerializer.Deserialize<sortableTestModelSetDto>(stream, options)
    //                            let sorterTestModelSet = SortableTestModelSetDto.toDomain dto
    //                            let sorterTestSet = sorterTestModelSet.makeSortableTestSet sortableArrayType

    //                            let runParams = OutputData.getRunParametersForOutputDataPath filePath
    //                            let repl = runParams.GetRepl()

    //                            let sortableIntTestSet =
    //                                match sorterTestSet with
    //                                | sortableTestSet.Ints intTestSet -> intTestSet
    //                                | sortableTestSet.Bools _ -> failwith "Expected Ints sorterTestSet"

    //                            let sortableTestData =
    //                                sortableIntTestSet.sortableTests |> Array.map (
    //                                    fun sorterTest ->
    //                                        (%sorterTest.Id, 
    //                                         %(sorterTestSet |> SortableTestset.getSortingWidth), 
    //                                         (%repl.ToString()), 
    //                                         sorterTest.Count))

    //                            sortableTestData
    //                        with e ->
    //                            failwith (sprintf "Error processing file %s: %s" filePath e.Message)
    //                )
    //                |> Array.concat
    //                |> Seq.toList

    //            // Generate the Markdown report, one line per SorterTest
    //            let reportContent =
    //                [ "Permutation orbit count report"
    //                  sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
    //                  sprintf "Project: %s" project.ProjectFolder
    //                  ""
    //                  "Id\t Sorting\t Width\t Repl\t Count"
    //                ]
    //                @ (summaries
    //                   |> List.map (fun (id, sortingWidth, repl, count) ->
    //                       sprintf "%s \t%d \t%s \t%d" (id.ToString()) sortingWidth repl count))
    //                |> String.concat "\n"


    //            // Save the report to a file
    //            let reportFilePath = Path.Combine(
    //                    project.ProjectFolder, 
    //                    sprintf "%s_PermutationOrbitCountReport_%s.txt" 
    //                                (outputDataType.SortableTestSet |> OutputDataType.toString )
    //                                (DateTime.Now.ToString("yyyyMMdd_HHmmss"))
    //                    )
    //            File.WriteAllText(reportFilePath, reportContent)
                
    //            Console.WriteLine(sprintf "Permutation orbit count report saved to %s" reportFilePath)
    //        with ex ->
    //            Console.WriteLine(sprintf "Error generating Permutation orbit count report for %s: %s" "SorterTestSet" ex.Message)
    //            raise ex

        

    //let RunAll() =
    //    for i in 0 .. 0 do
    //        let repl = i |> UMX.tag<replNumber>
    //        ProjectOps.executeProject project repl 6 executor



    // Function to run the SorterTest count report executor
    //let RunPermuationOrbitCountReport() =
    //    sorterTestCountReporter project
















