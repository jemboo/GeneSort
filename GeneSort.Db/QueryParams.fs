
namespace GeneSort.Db

open FSharp.UMX
open GeneSort.Runs.Params

type queryParams =
    private {
        projectName: string<projectName> option
        textReportName: string<textReportName> option
        index: int<indexNumber> option
        repl: int<replNumber> option
        generation: int<generationNumber> option
        outputDataType: outputDataType
        outputDataSubType: string option
    }
    member this.TextReportName with get() = 
                    match this.textReportName with
                        | Some name -> %name 
                        | None -> "NoName"

    member this.ProjectName with get() = 
                    match this.projectName with
                        | Some name -> %name
                        | None -> "NoName"

    member this.Index with get() = this.index
    member this.Repl with get() = this.repl
    member this.Generation with get() = this.generation
    member this.OutputDataType with get() = this.outputDataType
    
    static member create(
            projectName: string<projectName> option, 
            textReportName: string<textReportName> option,
            index: int<indexNumber> option, 
            repl: int<replNumber> option, 
            generation: int<generationNumber> option, 
            outputDataType: outputDataType,
            outputDataSubType: string option) : queryParams =
        {
            projectName = projectName
            textReportName = textReportName
            index = index
            repl = repl
            generation = generation
            outputDataType = outputDataType
            outputDataSubType = outputDataSubType
        }
    
    static member createForProject(projectName: string<projectName>) : queryParams =
        {
            projectName = (Some projectName)
            textReportName = None
            index = None
            repl = None
            generation = None
            outputDataType = outputDataType.Project
            outputDataSubType = None
        }
    
    static member createForTextReport
            (projectName: string<projectName>) 
            (textReportName: string<textReportName>) : queryParams =
        {
            projectName = (Some projectName)
            textReportName = (Some textReportName)
            index = None
            repl = None
            generation = None
            outputDataType = outputDataType.TextReport
            outputDataSubType = None
        }

    static member createFromRunParams 
                (outputDataType:outputDataType) 
                (outputDataSubType:string option) 
                (runParams: runParameters) : queryParams =
        {
            projectName = runParams.GetProjectName()
            textReportName = None
            index = runParams.GetIndex()
            repl = runParams.GetRepl()
            generation = runParams.GetGeneration()
            outputDataType = outputDataType
            outputDataSubType = outputDataSubType
        }