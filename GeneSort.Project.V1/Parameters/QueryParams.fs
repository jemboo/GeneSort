namespace GeneSort.Project.V1.Parameters

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
        outputDataType: geneSortDataType
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
        let outTypeStr = this.outputDataType |> GeneSortDataType.toFolderName
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
            (outputDataType: geneSortDataType)
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

            yield box (outputDataType |> GeneSortDataType.toFolderName)
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



    /// Preserve source identity across copies and continuation chains. Storage is database-local.
    static member createForImportedFile dbName projectName _repl outputType (sourceFileId: Guid<queryParamsId>) =
        let query = queryParams.create dbName projectName None None outputType [| "SourceFileId", string %sourceFileId |]
        { query with id = sourceFileId }

    /// Rehydrate a persisted file identity without recomputing it from current settings.
    static member withId id (query: queryParams) = { query with id = id }

    static member createFromMap
            (dbName:         string<databaseName>)
            (projName:       string<projectName>)
            (repl:           int<replNumber> option)
            (generation:     int<generationNumber> option)
            (outputDataType: geneSortDataType)
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
        queryParams.create databaseName projName None None (geneSortDataType.Run runName) [||]


    static member createForTextReport
            (databaseName:   string<databaseName>)
            (projName:       string<projectName>)
            (textReportName: string<textReportName>) : queryParams =
        queryParams.create databaseName projName None None (geneSortDataType.TextReport textReportName) [||]



module QueryParams = 

    // Creates a dataTableRecord from the Properties only, treating each of them as keys.
    let makeDataTableRecord (qp: queryParams) : GeneSort.Core.dataTableRecord =
        let baseRecord =
            match qp.Repl with
            | Some rv ->
                GeneSort.Core.dataTableRecord.createEmpty()
                         |> dataTableRecord.addKeyAndData "repl" (string %rv)
            | None -> GeneSort.Core.dataTableRecord.createEmpty()
                        
        qp.Properties
        |> Map.toSeq
        |> Seq.fold (fun acc (k, v) -> GeneSort.Core.dataTableRecord.addData k v acc) baseRecord

    /// Report labels come from the owning run without changing query or file identities.
    let makeReportDataTableRecord (runName: string<runName>) (qp: queryParams) =
        makeDataTableRecord qp |> dataTableRecord.addData ParamKeys.runNameKey %runName
