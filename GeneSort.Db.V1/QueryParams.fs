namespace GeneSort.Db.V1

open FSharp.UMX
open GeneSort.Core

//type queryParams =
//    private {
//        projectName:    string<projectName> option
//        repl:           int<replNumber> option
//        outputDataType: outputDataType
//        properties:     Map<string, string>
//        id:             Guid<queryParamsId>
//    }

//    static member ProjectNameString (projectName: string<projectName> option) =
//        match projectName with
//        | Some pn -> %pn
//        | None    -> "None"

//    static member ReplString (repl: int<replNumber> option) =
//        match repl with
//        | Some r -> (%r).ToString()
//        | None   -> ""

//    member this.Id             with get() = this.id
//    member this.ProjectName    with get() = this.projectName
//    member this.Repl           with get() = this.repl
//    member this.OutputDataType with get() = this.outputDataType
//    member this.Properties     with get() = this.properties

//    member this.ReplAsString with get() : string =
//        queryParams.ReplString this.repl

//    override this.ToString() : string =
//        let projStr    = this.projectName    |> queryParams.ProjectNameString
//        let replStr    = this.repl           |> queryParams.ReplString
//        let outTypeStr = this.outputDataType |> OutputDataType.toFolderName
//        let propsStr   =
//            this.properties
//            |> Map.toSeq
//            |> Seq.map (fun (k, v) -> $"{k}={v}")
//            |> String.concat ";"
//        $"Project: {projStr}, Repl: {replStr}, OutputType: {outTypeStr}, Properties: [{propsStr}]"

//    static member create
//            (projectName:    string<projectName> option)
//            (repl:           int<replNumber> option)
//            (outputDataType: outputDataType)
//            (properties:     (string * string) []) : queryParams =
//        let props = properties |> Array.filter (fst >> isNull >> not) |> Map.ofArray
//        {
//            projectName    = projectName
//            repl           = repl
//            outputDataType = outputDataType
//            properties     = props
//            id             = GuidUtils.guidFromObjs [
//                                box (projectName    |> queryParams.ProjectNameString)
//                                box (repl           |> queryParams.ReplString)
//                                box (outputDataType |> OutputDataType.toFolderName)
//                                box (props |> Map.toSeq |> Seq.sortBy fst |> Seq.toArray)
//                             ] |> UMX.tag<queryParamsId>
//        }

//    static member createForProject (projectName: string<projectName>) : queryParams =
//        queryParams.create (Some projectName) None outputDataType.Project [||]

//    static member createForTextReport
//            (projectName:    string<projectName>)
//            (textReportName: string<textReportName>) : queryParams =
//        queryParams.create (Some projectName) None (outputDataType.TextReport textReportName) [||]