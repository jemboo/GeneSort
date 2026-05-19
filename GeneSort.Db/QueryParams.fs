namespace GeneSort.Db

open FSharp.UMX
open GeneSort.Runs
open GeneSort.Core

type queryParams =
    private {
        projectName:    string<projectName> option
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
    member this.ProjectName    with get() = this.projectName
    member this.Repl           with get() = this.repl
    member this.OutputDataType with get() = this.outputDataType
    member this.Properties     with get() = this.properties

    member this.ReplAsString with get() : string =
        queryParams.ReplString this.repl

    override this.ToString() : string =
        let projStr    = this.projectName    |> queryParams.ProjectNameString
        let replStr    = this.repl           |> queryParams.ReplString
        let outTypeStr = this.outputDataType |> OutputDataType.toFolderName
        let propsStr   =
            this.properties
            |> Map.toSeq
            |> Seq.map (fun (k, v) -> $"{k}={v}")
            |> String.concat ";"
        $"Project: {projStr}, Repl: {replStr}, OutputType: {outTypeStr}, Properties: [{propsStr}]"

    interface IStableSerializable with
        member this.WriteStableBytes writer =
            // 1. Serialize projectName option safely
            match this.projectName with
            | Some pn -> 
                writer.Write(true)
                writer.Write(%pn)
            | None -> 
                writer.Write(false)

            // 2. Serialize repl option safely
            match this.repl with
            | Some r -> 
                writer.Write(true)
                writer.Write(%r)
            | None -> 
                writer.Write(false)

            // 3. Serialize outputDataType
            writer.Write(this.outputDataType |> OutputDataType.toFolderName)

            // 4. Serialize properties sorted by key to maintain hash parity
            writer.Write(this.properties.Count)
            this.properties 
            |> Map.toSeq 
            |> Seq.sortBy fst 
            |> Seq.iter (fun (k, v) -> 
                writer.Write(k)
                writer.Write(v))

    static member create
            (projectName:    string<projectName> option)
            (repl:           int<replNumber> option)
            (outputDataType: outputDataType)
            (properties:     (string * string) []) : queryParams =
        let props = properties |> Array.filter (fst >> isNull >> not) |> Map.ofArray
        
        // Build a clean, typed sequential list for Guid generation.
        // We unpack primitives here so they route smoothly into your GuidUtils primitives matcher.
        let structuralIdentityComponents = seq {
            match projectName with
            | Some pn -> yield box true; yield box %pn
            | None -> yield box false

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
            projectName    = projectName
            repl           = repl
            outputDataType = outputDataType
            properties     = props
            id             = GuidUtils.guidFromObjs structuralIdentityComponents |> UMX.tag<queryParamsId>
        }

    static member createForProject (projectName: string<projectName>) : queryParams =
        queryParams.create (Some projectName) None outputDataType.Project [||]