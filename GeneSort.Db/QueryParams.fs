namespace GeneSort.Db

open FSharp.UMX
open GeneSort.Runs
open GeneSort.Core

type queryParams = 
    private { 
        projectName: string<projectName> option
        repl: int<replNumber> option
        outputDataType: outputDataType
        properties: Map<string, string>
        mutable idCache: System.Guid option
    }
    
    member this.ProjectName 
        with get() = 
            match this.projectName with
            | Some name -> %name
            | None -> "NoName"
    
    member this.Repl 
        with get() = this.repl
    
    member this.OutputDataType 
        with get() = this.outputDataType
    
    member this.Properties 
        with get() = this.properties
    
    member this.Id 
        with get() : System.Guid = 
            match this.idCache with
            | Some guid -> guid
            | None ->
                let guid = 
                    GuidUtils.guidFromObjs [
                        box this.projectName
                        box this.repl
                        box this.outputDataType
                        box (this.properties |> Map.toSeq |> Seq.sortBy fst |> Seq.toArray)
                    ]
                this.idCache <- Some guid
                guid
    
    static member create (
        projectName: string<projectName> option,
        repl: int<replNumber> option,
        outputDataType: outputDataType,
        properties: (string*string) []) : queryParams = 
        { 
            projectName = projectName
            repl = repl
            outputDataType = outputDataType
            properties = properties |> Map.ofArray
            idCache = None
        }
    
    static member createForProject(projectName: string<projectName>) : queryParams = 
        { 
            projectName = (Some projectName)
            repl = None
            outputDataType = outputDataType.Project
            properties = Map.empty
            idCache = None
        }
    
    static member createForTextReport 
        (projectName: string<projectName>) 
        (textReportName: string<textReportName>) : queryParams = 
        { 
            projectName = (Some projectName)
            repl = None
            outputDataType = outputDataType.TextReport textReportName
            properties = Map.empty
            idCache = None
        }