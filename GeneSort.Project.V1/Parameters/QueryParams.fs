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

    /// A saved setup reference is sufficient to locate a file in the owning database.
    let trySetupReference database project (parameters: runParameters) outputType =
        let key =
            match outputType with
            | geneSortDataType.SorterModelSet _ | geneSortDataType.SortableTestsWithPrefix _
            | geneSortDataType.SorterPoolSet _ -> Some (SetupFileReferences.keyForOutputType outputType)
            | _ -> None
        key |> Option.bind (fun key ->
            parameters.ParamMap.TryFind key |> Option.map (fun value ->
                let query = queryParams.create database project None None outputType [||]
                { query with id = System.Guid.Parse value |> UMX.tag }))

    /// Resolve imported Setup independently of the current execution settings and generation.
    let forSetup (parameters: runParameters) (query: queryParams) =
        match trySetupReference query.DbName query.ProjectName parameters query.OutputDataType with
        | Some referenced -> referenced
        | None ->
            let sourceKey =
                match query.OutputDataType with
                | geneSortDataType.SorterPoolSet _ -> Some ParamKeys.sourcePoolFileIdKey
                | geneSortDataType.SortableTestsWithPrefix _ -> Some ParamKeys.sourceTestsFileIdKey
                | _ -> None
            match sourceKey |> Option.bind (fun key -> Map.tryFind key parameters.ParamMap) with
            | None -> query
            | Some value ->
                let imported = queryParams.createForImportedFile query.DbName query.ProjectName (parameters.GetRepl()) query.OutputDataType
                                    (System.Guid.Parse value |> UMX.tag)
                // Converted pools have their own Setup identity; provenance still records the original source file.
                let referenceKey = SetupFileReferences.keyForOutputType query.OutputDataType
                match Map.tryFind referenceKey parameters.ParamMap with
                | Some id -> { imported with id = System.Guid.Parse id |> UMX.tag }
                | None -> imported

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
