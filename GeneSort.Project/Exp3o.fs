
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

module Exp3 = 

    let projectDir = "c:\Projects"

    let experimentName4 = "RandomSorterEvals4"
    let experimentDesc4 = "RandomSorters with SortingWidth divisibe by 4, with full (0,1)^n evaluations"

    let experimentName6 = "RandomSorterEvals6"
    let experimentDesc6 = "RandomSorters with SortingWidth divisibe by 6, with full (0,1)^n evaluations"


    let randomType = rngType.Lcg
    let excludeSelfCe = true


    let getSorterCountForSortingWidth (factor:int) (sortingWidth: int<sortingWidth>) : int<sorterCount> =
        match %sortingWidth with
        | 4 -> (10 * factor) |> UMX.tag<sorterCount>
        | 6 -> (10 * factor) |> UMX.tag<sorterCount>
        | 8 -> (10 * factor) |> UMX.tag<sorterCount>
        | 12 -> (10 * factor) |> UMX.tag<sorterCount>
        | 16 -> (50 * factor) |> UMX.tag<sorterCount>
        | 24 -> (50 * factor) |> UMX.tag<sorterCount>
        | 32 -> (50 * factor) |> UMX.tag<sorterCount>
        | 48 -> (10 * factor) |> UMX.tag<sorterCount>
        | 64 -> (10 * factor) |> UMX.tag<sorterCount>
        | 96 -> (10 * factor) |> UMX.tag<sorterCount>
        | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)


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



    let sortableArrayType = sortableArrayType.Bools
  


    let sortingWidthValues4 = 
        [4; 8; 16; 32; 64] |> List.map(fun d -> d.ToString()) 

    let sortingWidths4() : string*string list =
        (runParameters.sortingWidthKey , sortingWidthValues4)
        
    let sortingWidthValues6 = 
        [6; 12; 24; 48; 96] |> List.map(fun d -> d.ToString())

    let sortingWidths6() : string*string list =
        (runParameters.sortingWidthKey, sortingWidthValues6)


    let sorterModelKeyValues4 () : string list =
        [ sorterModelKey.Mcse; 
          sorterModelKey.Mssi;
          sorterModelKey.Msrs; 
          sorterModelKey.Msuf4; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys4 () : string*string list =
        (runParameters.sorterModelTypeKey, sorterModelKeyValues4() )

    let sorterModelKeyValues6 () : string list =
        [ sorterModelKey.Mcse; 
          sorterModelKey.Mssi;
          sorterModelKey.Msrs; 
          sorterModelKey.Msuf6; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys6 () : string*string list =
        (runParameters.sorterModelTypeKey, sorterModelKeyValues6() )


    let parameterSet4 = 
        [ sortingWidths4(); sorterModelKeys4() ]

    let parameterSet6 = 
        [ sortingWidths6(); sorterModelKeys6() ]

    let workspace4 = 
            Workspace.create 
                "Exp3a" 
                "Exp3 descr" 
                projectDir 
                parameterSet4
                (fun s -> Some s)

    let workspace6 = Workspace.create experimentName6 experimentDesc6 projectDir parameterSet6


    let executor (workspace: workspace) (cycle: int<cycleNumber>) (run: run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d  Cycle %d  %s" run.Index %cycle (run.RunParameters.toString()))
            run.RunParameters.SetCycle cycle

            let sorterModelKey = run.RunParameters.GetSorterModelKey()
            let sortingWidth = run.RunParameters.GetSortingWidth()
            let ceLength = getCeLengthForSortingWidth sortingWidth
            
            let stageLength = getStageLengthForSortingWidth sortingWidth
            let opsGenRatesArray = OpsGenRatesArray.createUniform %stageLength
            let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageLength %sortingWidth

            let modelMaker =
                match sorterModelKey with
                | sorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceLength) |> sorterModelMaker.SmmMsceRandGen
                | sorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageLength) |> sorterModelMaker.SmmMssiRandGen
                | sorterModelKey.Msrs -> (msrsRandGen.create randomType sortingWidth opsGenRatesArray) |> sorterModelMaker.SmmMsrsRandGen
                | sorterModelKey.Msuf4 -> (msuf4RandGen.create randomType sortingWidth stageLength uf4GenRatesArray) |> sorterModelMaker.SmmMsuf4RandGen
                | sorterModelKey.Msuf6 -> failwith "Msuf6 not supported in this experiment"

            let cycleFactor = if (%cycle = 0) then 1 else 10
            let sorterCount = getSorterCountForSortingWidth cycleFactor sortingWidth
            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create modelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            let sorterTestModel = MsasF.create sortingWidth |> sortableTestModel.MsasF
            let sorterTest = SortableTestModel.makeSortableTests sorterTestModel sortableArrayType
            let sorterSetEval = SorterSetEval.makeSorterSetEval sorterSet sorterTest

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSetEval |> outputData.SorterSetEval)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)

            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }


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
                let summaries : (string*string*string*string*string*string) list =
                    files
                    |> Seq.map (
                        fun ssEvalPath ->
                            try
                                use ssEvalStream = new FileStream(ssEvalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                                let ssEvalDto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(ssEvalStream, options)
                                let sorterSetEval = SorterSetEvalDto.toDomain ssEvalDto          
                                let sorterSetEvalBins = SorterSetEvalBins.create 1 sorterSetEval

                                let runParams = OutputData.getRunParametersForOutputDataPath ssEvalPath
                                let sorterModelKey = runParams.GetSorterModelKey() |> SorterModelKey.toString
                                let swFull = (%runParams.GetSortingWidth()).ToString()

                                let prpt = SorterSetEvalBins.getBinCountReport sorterSetEvalBins
                                let appended = prpt |> Array.map(fun aa -> (swFull, sorterModelKey, aa.[0], aa.[1], aa.[2], aa.[3]))
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
                      "Sorting Width\t SorterModel\t ceLength\t stageLength\t binCount\t unsortedReport"
                    ]
                    @ (summaries
                       |> List.map (
                            fun (sortingWidth, sorterModelKey, ceLength, stageLength, binCount, unsortedReport) ->
                                    sprintf "%s \t %s \t %s \t %s \t %s \t %s " sortingWidth sorterModelKey ceLength stageLength binCount unsortedReport))
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
                                let sorterModelKey =   runParams.GetSorterModelKey() 
                                let swFull = runParams.GetSortingWidth() 

                                let linePrefix = sprintf "%s \t %s" (%swFull.ToString()) (sorterModelKey |> SorterModelKey.toString)
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
                let reportFilePath = Path.Combine(workspace.WorkspaceFolder, sprintf "SorterCeUseReport_%s.txt" (DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                File.WriteAllText(reportFilePath, reportContent)

                Console.WriteLine(sprintf "Ce Profile report saved to %s" reportFilePath)
            with ex ->
                Console.WriteLine(sprintf "Error generating Ce Profile report for %s: %s" "SorterTestSet" ex.Message)
                raise ex






    let RunAll() =
        for i in 0 .. 10 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace4 cycle 8 executor


    let RunSorterEvalReport() =
         (binReportExecutor workspace4)
    //    (ceUseProfileReportExecutor workspace)









