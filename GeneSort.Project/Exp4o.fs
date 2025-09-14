
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
open GeneSort.SortingOps.Mp

module Exp4 = 

    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let excludeSelfCe = true
  


    let getCeLengthForSortingWidth (sortingWidth: int<sortingWidth>) : int<ceLength> =
        match %sortingWidth with
        | 4 -> 300 |> UMX.tag<ceLength>
        | 6 -> 600 |> UMX.tag<ceLength>
        | 8 -> 16 |> UMX.tag<ceLength>
        | 12 -> 24 |> UMX.tag<ceLength>
        | 16 -> 32 |> UMX.tag<ceLength>
        | 24 -> 48 |> UMX.tag<ceLength>
        | 32 -> 64 |> UMX.tag<ceLength>
        | 48 -> 96 |> UMX.tag<ceLength>
        | 64 -> 128 |> UMX.tag<ceLength>
        | 96 -> 192 |> UMX.tag<ceLength>
        | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)


    let getStageLengthForSortingWidth (sortingWidth: int<sortingWidth>) : int<stageLength> =
        match %sortingWidth with
        | 4 -> 5 |> UMX.tag<stageLength>
        | 6 -> 10 |> UMX.tag<stageLength>
        | 8 -> 20 |> UMX.tag<stageLength>
        | 12 -> 30 |> UMX.tag<stageLength>
        | 16 -> 100 |> UMX.tag<stageLength>
        | 24 -> 150 |> UMX.tag<stageLength>
        | 32 -> 200 |> UMX.tag<stageLength>
        | 48 -> 300 |> UMX.tag<stageLength>
        | 64 -> 400 |> UMX.tag<stageLength>
        | 96 -> 600 |> UMX.tag<stageLength>
        | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)

    let parameterSet = 
        [ SwMerge.exp4Vals(); SorterModelKey.allButMusf6Kvps(); ("SortableArrayType", ["Ints"]) ]

    let workspace = Workspace.create "Exp4a" "Exp4 descr" projectDir parameterSet


    let executor (workspace: workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {
            Console.WriteLine(sprintf "Executing Run %d  Cycle %d  %A" run.Index %cycle run.Parameters)
            Run.setCycle run cycle

            let sorterModelKey = (run.Parameters["SorterModel"]) |> SorterModelKey.fromString
            let swMerge = (run.Parameters["SortingWidth"]) |> SwMerge.fromString
            let sortableArrayType = (run.Parameters["SortableArrayType"]) |> SortableArrayType.fromString

            let sortingWidth = swMerge |> SwMerge.toSortingWidth
            let ceLength = getCeLengthForSortingWidth sortingWidth

            let stageCount = getStageLengthForSortingWidth sortingWidth
            let opsGenRatesArray = OpsGenRatesArray.createUniform %stageCount
            let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageCount %sortingWidth

            let modelMaker =
                match sorterModelKey with
                | sorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceLength) |> SorterModelMaker.SmmMsceRandGen
                | sorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageCount) |> SorterModelMaker.SmmMssiRandGen
                | sorterModelKey.Msrs -> (MsrsRandGen.create randomType sortingWidth opsGenRatesArray) |> SorterModelMaker.SmmMsrsRandGen
                | sorterModelKey.Msuf4 -> (Msuf4RandGen.create randomType sortingWidth stageCount uf4GenRatesArray) |> SorterModelMaker.SmmMsuf4RandGen
                | sorterModelKey.Msuf6 -> failwith "Msuf6 not supported in this experiment"

            let sorterCount = swMerge |> SorterCount.getSorterCountForSwMerge
            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create modelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            let sortableTests = SortableTestModel.makeSortableTestsForMerge sortableArrayType sortingWidth
            let sorterSetEval = SorterSetEval.makeSorterSetEval sorterSet sortableTests

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSetEval |> outputData.SorterSetEval)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)

            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }


    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let evalReportExecutor (workspace: workspace) =
            try
                Console.WriteLine(sprintf "Generating SorterEval report in workspace %s"  workspace.WorkspaceFolder)
                let sorterSetEvalSamplesFolder = OutputData.getOutputDataFolder workspace outputDataType.SorterSetEval
                if not (Directory.Exists sorterSetEvalSamplesFolder) then
                    failwith (sprintf "Output folder %s does not exist" sorterSetEvalSamplesFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(sorterSetEvalSamplesFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


                // Process each file and collect data for each SorterTest
                let summaries : (string*string*string*string*string) list =
                    files
                    |> Seq.map (
                        fun ssEvalPath ->
                            try
                                use ssEvalStream = new FileStream(ssEvalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let ssEvalDto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(ssEvalStream, options)
                                let sorterSetEval = SorterSetEvalDto.toDomain ssEvalDto  
                                                                
                                let runParams = OutputData.getRunParametersForOutputDataPath ssEvalPath
                                let sorterModelKey = runParams[Run.sorterModelTypeKey]
                                let swFull = runParams[Run.sortingWidthKey]
                                let cycle = runParams[Run.cycleKey]
                                let sortableArrayType = runParams[Run.sortableArrayTypeKey]

                                let prpt = sorterSetEval.SorterEvals |> Array.map (fun se -> SorterEval.reportLine se)
                                let appended = prpt |> Array.map(fun aa -> (cycle, swFull, sorterModelKey, sortableArrayType, aa))
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
                      sprintf "Cycle\t Sorting Width\t SorterModel\t SortableArrayType\t %s" SorterEval.reportHeader
                    ]
                    @ (summaries
                       |> List.map (
                            fun (cycle, sortingWidth, sorterModelKey, sortableArrayType, evalReport) ->
                                    sprintf "%s \t%s \t%s \t%s \t%s" cycle sortingWidth sorterModelKey sortableArrayType evalReport))
                    |> String.concat "\n"


                // Save the report to a file
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_%s.txt" "SorterEvalSamples" (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                File.WriteAllText(reportFilePath, reportContent)

                Console.WriteLine(sprintf "SorterTest count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating SorterTest count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex




    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let binReportExecutor (workspace: workspace) =
            try
                Console.WriteLine(sprintf "Generating SorterEval report in workspace %s"  workspace.WorkspaceFolder)
                let sorterSetEvalSamplesFolder = OutputData.getOutputDataFolder workspace outputDataType.SorterSetEval
                if not (Directory.Exists sorterSetEvalSamplesFolder) then
                    failwith (sprintf "Output folder %s does not exist" sorterSetEvalSamplesFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(sorterSetEvalSamplesFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


                // Process each file and collect data for each SorterTest
                let summaries : (string*string*string*string*string*string*string*string) list =
                    files
                    |> Seq.map (
                        fun ssEvalPath ->
                            try
                                use ssEvalStream = new FileStream(ssEvalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let ssEvalDto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(ssEvalStream, options)
                                let sorterSetEval = SorterSetEvalDto.toDomain ssEvalDto          
                                let sorterSetEvalBins = SorterSetEvalBins.create 1 sorterSetEval

                                let runParams = OutputData.getRunParametersForOutputDataPath ssEvalPath
                                let sorterModelKey = runParams["SorterModel"]
                                let swFull = runParams["SortingWidth"]
                                let cycle = runParams[Run.cycleKey]
                                let sortableArrayType = runParams["SortableArrayType"]

                                let prpt = SorterSetEvalBins.getBinCountReport sorterSetEvalBins
                                let appended = prpt |> Array.map(fun aa -> (cycle, swFull, sorterModelKey, sortableArrayType, aa.[0], aa.[1], aa.[2], aa.[3]))
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
                      "Cycle\t Sorting Width\t SorterModel\t SortableArrayType\t  ceLength\t stageCount\t binCount\t unsortedReport"
                    ]
                    @ (summaries
                       |> List.map (
                            fun (cycle, sortingWidth, sorterModelKey, sortableArrayType, ceLength, stageCount, binCount, unsortedReport) ->
                                    sprintf "%s \t %s \t %s \t  %s \t %s \t %s \t %s \t %s " cycle sortingWidth sorterModelKey sortableArrayType ceLength stageCount binCount unsortedReport))
                    |> String.concat "\n"

                // Save the report to a file
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "%s_SorterEvalReport_%s.txt" "SorterSetEvalSamples" (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                File.WriteAllText(reportFilePath, reportContent)

                Console.WriteLine(sprintf "SorterTest count report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating SorterTest count report for %s: %s" "SorterTestSet" ex.Message)
                raise ex



    // Executor to generate a report for each SorterTest across all SorterTestSets, one line per SorterTest
    let ceUseProfileReportExecutor (workspace: workspace) =
            try
                let binCount = 20
                let blockGrowthRate = 1.2
                Console.WriteLine(sprintf "Generating Ce Profile report in workspace %s"  workspace.WorkspaceFolder)
                let sorterSetEvalSamplesFolder = OutputData.getOutputDataFolder workspace outputDataType.SorterSetEval
                if not (Directory.Exists sorterSetEvalSamplesFolder) then
                    failwith (sprintf "Output folder %s does not exist" sorterSetEvalSamplesFolder)

                // Find all .msgpack files in the output folder
                let files = Directory.GetFiles(sorterSetEvalSamplesFolder, "*.msgpack")

                // Initialize the MessagePack resolver
                let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
                let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
                
                let summaries : string list =
                    files
                    |> Seq.map (
                        fun ssEvalPath ->
                            try
                                use ssEvalStream = new FileStream(ssEvalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let ssEvalDto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(ssEvalStream, options)
                                let sorterSetEval = SorterSetEvalDto.toDomain ssEvalDto     
                                let sorterSetCeUseProfile = SorterSetCeUseProfile.makeSorterSetCeUseProfile binCount blockGrowthRate sorterSetEval
  
                                let runParams = OutputData.getRunParametersForOutputDataPath ssEvalPath
                                let sorterModelKey = runParams[Run.sorterModelTypeKey]
                                let swFull = runParams[Run.sortingWidthKey]

                                let linePrefix = sprintf "%s \t %s" swFull sorterModelKey
                                SorterSetCeUseProfile.makeCsvLines linePrefix sorterSetCeUseProfile
                            with e ->
                                failwith (sprintf "Error processing file %s: %s" ssEvalPath e.Message)
                    )
                    |> Array.concat
                    |> Seq.toList

                // Generate the Markdown report, one line per SorterTest
                let reportContent =
                    [ "# sorterCeProfile Report"
                      sprintf "Generated on %s" (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                      sprintf "Workspace: %s" workspace.WorkspaceFolder
                      ""
                      "Sorting Width\t SorterModel\t lastCe"
                    ]
                    @ summaries
                    |> String.concat "\n"


                // Save the report to a file
                let reportFilePath = Path.Combine( workspace.WorkspaceFolder, 
                                                   sprintf "SorterCeUseReport_%s.txt" 
                                                  (DateTime.Now.ToString("yyyyMMdd_HHmmss")))

                File.WriteAllText(reportFilePath, reportContent)

                Console.WriteLine(sprintf "Ce Profile report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating Ce Profile report for %s: %s" "SorterTestSet" ex.Message)
                raise ex






    let RunAll() =
        for i in 0 .. 10 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 8 executor
        Console.WriteLine(sprintf "*****************************************************************")
        Console.WriteLine(sprintf "*****************************************************************")
        Console.WriteLine(sprintf "*****************************************************************")


    let RunSorterEvalReport() =
        (evalReportExecutor workspace)
    //(binReportExecutor workspace)
    //    (ceUseProfileReportExecutor workspace)









