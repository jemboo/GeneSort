namespace GeneSort.Db.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Project.V1


type queryParams =
    private {
        dbName:         string<databaseName>
        projectName:    string<projectName>
        repl:           int<replNumber> option
        generation:     int<generationNumber> option
        outputDataType: outputDataType
        properties:     Map<string, string>
        id:             Guid<queryParamsId>
    }

    member this.Id             with get() = this.id
    member this.DbName         with get() = this.dbName
    member this.Repl           with get() = this.repl
    member this.Generation     with get() = this.generation
    member this.OutputDataType with get() = this.outputDataType
    member this.ProjectName    with get() = this.projectName
    member this.Properties     with get() = this.properties

    member this.ReplAsString with get() : string =
        UmxExt.intOptionToString this.repl

    override this.ToString() : string =
        let replStr    = this.repl           |> UmxExt.intOptionToString
        let outTypeStr = this.outputDataType |> OutputDataType.toFolderName
        let propsStr   =
            this.properties
            |> Map.toSeq
            |> Seq.map (fun (k, v) -> $"{k}={v}")
            |> String.concat ";"
        $"Db: {%this.dbName}, Project: {%this.projectName}, Repl: {replStr}, 
           OutputType: {outTypeStr}, Properties: [{propsStr}]"

    static member create
            (dbName:         string<databaseName>)
            (projName:       string<projectName>)
            (repl:           int<replNumber> option)
            (generation:     int<generationNumber> option)
            (outputDataType: outputDataType)
            (properties:     (string * string) []) : queryParams =
        let props = properties |> Array.filter (fst >> isNull >> not) |> Map.ofArray
        
        // Build a clean, typed sequential list for Guid generation.
        // We unpack primitives here so they route smoothly into your GuidUtils primitives matcher.
        let structuralIdentityComponents = seq {
            yield box dbName
            yield box projName
            match repl with
            | Some r -> yield box true; yield box %r
            | None -> yield box false

            // Include generation in the structural identity ONLY when outputDataType is NOT RunParameters
            match outputDataType, generation with
            | RunParameters _, _ -> 
                ()
            | _, Some gen -> 
                yield box true
                yield box %gen
            | _, None -> 
                yield box false

            yield box (outputDataType |> OutputDataType.toFolderName)
            yield box props.Count
            
            yield! props 
                   |> Map.toSeq 
                   |> Seq.sortBy fst 
                   |> Seq.collect (fun (k, v) -> [box k; box v])
        }

        {
            dbName         = dbName
            projectName    = projName
            repl           = repl
            generation     = generation
            outputDataType = outputDataType
            properties     = props
            id             = GuidUtils.guidFromObjs structuralIdentityComponents |> UMX.tag<queryParamsId>
        }



    static member createFromMap
            (dbName:         string<databaseName>)
            (projName:       string<projectName>)
            (repl:           int<replNumber> option)
            (generation:     int<generationNumber> option)
            (outputDataType: outputDataType)
            (propertyMap:     Map<string, string>) : queryParams =

       queryParams.create dbName projName repl generation 
                          outputDataType (propertyMap |> Map.toArray)
            

    interface IStableSerializable with
            member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
                let rawGuid = UMX.untag this.id
                writer.Write(rawGuid.ToByteArray())


    static member createForRun 
                    (databaseName: string<databaseName>) 
                    (projName:     string<projectName>)
                    (runName:      string<runName>) 
                    : queryParams =
        queryParams.create databaseName projName None None (outputDataType.Run runName) [||]


    static member createForTextReport
            (databaseName:   string<databaseName>)
            (projName:       string<projectName>)
            (textReportName: string<textReportName>) : queryParams =
        queryParams.create databaseName projName None None (outputDataType.TextReport textReportName) [||]



module QueryParams = 

    // Creates a dataTableRecord from the Properties only, treating each of them as keys.
    let makeDataTableRecord (qp: queryParams) : GeneSort.Core.dataTableRecord =
        let baseRecord = GeneSort.Core.dataTableRecord.createEmpty()
        qp.Properties
        |> Map.toSeq
        |> Seq.fold (fun acc (k, v) -> GeneSort.Core.dataTableRecord.addData k v acc) baseRecord