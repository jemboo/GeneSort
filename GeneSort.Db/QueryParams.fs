namespace GeneSort.Db

open System
open FSharp.UMX
open GeneSort.Runs
open GeneSort.Core

type queryParams =
    private {
        projectName: string<projectName> option
        repl: int<replNumber> option
        outputDataType: outputDataType
        properties: Map<string, string>
        idCache: Lazy<System.Guid> 
    }

    /// Gets the project name, defaulting to "NoName" if none.
    static member ProjectNameString (projectName:string<projectName> option) = 
           match projectName with
              | Some pn -> %pn
              | None -> "None"

    static member ReplString (repl:int<replNumber> option) = 
            match repl with
               | Some r -> (%r).ToString()
               | None -> "None"

    member this.OutputDataType = this.outputDataType

    member this.Properties = this.properties

    member this.Id : Guid<idValue> = this.idCache.Value |> UMX.tag<idValue>

    /// Creates a new queryParams instance.
    static member create (
        projectName: string<projectName> option,
        repl: int<replNumber> option,
        outputDataType: outputDataType,
        properties: (string*string) []) : queryParams =
        let props = properties |> Array.filter (fst >> isNull >> not) |> Map.ofArray
        {   // Added validation: no null keys in properties.
            projectName = projectName
            repl = repl
            outputDataType = outputDataType
            properties = props
            idCache = lazy (
                GuidUtils.guidFromObjs [
                    box (projectName |> queryParams.ProjectNameString); 
                    box (repl |> queryParams.ReplString); 
                    box (outputDataType |> OutputDataType.toFolderName);
                    box (props |> Map.toSeq |> Seq.sortBy fst |> Seq.toArray)
                ])
        }

    /// Creates queryParams for a project.
    static member createForProject(projectName: string<projectName>) : queryParams =
        queryParams.create(Some projectName, None, outputDataType.Project, [||])


    /// Creates queryParams for a text report.
    static member createForTextReport
        (projectName: string<projectName>)
        (textReportName: string<textReportName>) : queryParams =
        queryParams.create(Some projectName, None, outputDataType.TextReport textReportName, [||])