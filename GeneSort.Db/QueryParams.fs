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
        idCache: Lazy<System.Guid> 
    }

    /// Gets the project name, defaulting to "NoName" if none.
    member this.ProjectName =
        match this.projectName with
        | Some name -> %name
        | None -> "NoName"

    member this.Repl = this.repl

    member this.OutputDataType = this.outputDataType

    member this.Properties = this.properties

    member this.Id : System.Guid = this.idCache.Value

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
                    box projectName; box repl; box outputDataType
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