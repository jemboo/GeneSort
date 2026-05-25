namespace GeneSort.Db.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1


type queryParams =
    private {
        queryName:    string<queryName>
        repl:           int<replNumber> option
        outputDataType: outputDataType
        properties:     Map<string, string>
        id:             Guid<queryParamsId>
    }

    static member ProjectNameString (projectName: string<projectName> option) =
        match projectName with
        | Some pn -> %pn
        | None    -> "None"

    static member ReplString (repl: int<replNumber> option) =
        match repl with
        | Some r -> (%r).ToString()
        | None   -> ""

    member this.Id             with get() = this.id
    member this.QueryName    with get() = this.queryName
    member this.Repl           with get() = this.repl
    member this.OutputDataType with get() = this.outputDataType
    member this.Properties     with get() = this.properties

    member this.ReplAsString with get() : string =
        queryParams.ReplString this.repl

    override this.ToString() : string =
        let queryStr    = %this.queryName    |> string
        let replStr    = this.repl           |> queryParams.ReplString
        let outTypeStr = this.outputDataType |> OutputDataType.toFolderName
        let propsStr   =
            this.properties
            |> Map.toSeq
            |> Seq.map (fun (k, v) -> $"{k}={v}")
            |> String.concat ";"
        $"Query: {queryStr}, Repl: {replStr}, OutputType: {outTypeStr}, Properties: [{propsStr}]"

    static member create
            (queryName:    string<queryName>)
            (repl:           int<replNumber> option)
            (outputDataType: outputDataType)
            (properties:     (string * string) []) : queryParams =
        let props = properties |> Array.filter (fst >> isNull >> not) |> Map.ofArray
        
        // Build a clean, typed sequential list for Guid generation.
        // We unpack primitives here so they route smoothly into your GuidUtils primitives matcher.
        let structuralIdentityComponents = seq {
            yield box queryName

            match repl with
            | Some r -> yield box true; yield box %r
            | None -> yield box false

            yield box (outputDataType |> OutputDataType.toFolderName)
            yield box props.Count
            
            yield! props 
                   |> Map.toSeq 
                   |> Seq.sortBy fst 
                   |> Seq.collect (fun (k, v) -> [box k; box v])
        }

        {
            queryName    = queryName
            repl           = repl
            outputDataType = outputDataType
            properties     = props
            id             = GuidUtils.guidFromObjs structuralIdentityComponents |> UMX.tag<queryParamsId>
        }

    interface IStableSerializable with
            member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
                let rawGuid = UMX.untag this.id
                writer.Write(rawGuid.ToByteArray())


    static member createForRun 
                    (queryName: string<queryName>) 
                    (runName: string<runName>) 
                    : queryParams =
        queryParams.create queryName None (outputDataType.Run runName) [||]


    static member createForTextReport
            (queryName:    string<queryName>)
            (textReportName: string<textReportName>) : queryParams =
        queryParams.create queryName None (outputDataType.TextReport textReportName) [||]



module QueryParams = 

    // Creates a dataTableRecord from the Properties only, treating each of them as keys.
    let makeDataTableRecord (qp: queryParams) : GeneSort.Core.dataTableRecord =
        let baseRecord = GeneSort.Core.dataTableRecord.createEmpty()
        qp.Properties
        |> Map.toSeq
        |> Seq.fold (fun acc (k, v) -> GeneSort.Core.dataTableRecord.addData k v acc) baseRecord