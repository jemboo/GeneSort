namespace GeneSort.Core
open System
open System.IO
open System.Text

type dataTableReport = 
    private 
        { name: string
          timeStamp: string
          mutable sourceRows: ResizeArray<string>
          mutable errorRows: ResizeArray<string>
          dataHeaders: string[]
          dataRows: ResizeArray<string> }
    
    member this.Name with get() = this.name
    member this.TimeStamp with get() = this.timeStamp
    member this.SourceRows 
        with get() = this.sourceRows
    member this.ErrorRows 
        with get() = this.errorRows
    member this.DataHeaders with get() = this.dataHeaders
    member this.DataRows with get() = this.dataRows

    /// Helper to sanitize row data to prevent column shifting
    static member private Sanitize (row: string[]) : string [] =
        row |> Array.map (fun s -> if s = null then "" else s.Replace("\t", " "))


    member this.AppendDataRow (rowArray: string []) = 
        let cleanRow = String.Join("\t", dataTableReport.Sanitize rowArray)
        this.dataRows.Add(cleanRow)

    member this.AppendDataRows (rows: string [] seq) =
        for r in rows do
            this.AppendDataRow(r)

    member this.AddSources (data: string []) =
        let cleanedData = dataTableReport.Sanitize data
        this.sourceRows.AddRange(cleanedData)

    member this.AddErrors (data: string []) =
        let cleanedData = dataTableReport.Sanitize data
        this.errorRows.AddRange(cleanedData)

    member this.AddSource (source: string) =
        let cleaned = if source = null then "" else source.Replace("\t", " ")
        this.sourceRows.Add(cleaned)

    member this.AddError (error: string) =
        let cleaned = if error = null then "" else error.Replace("\t", " ")
        this.errorRows.Add(cleaned)



    // First line: Name and timestamp
    // Second line: Blank line
    // Next lines: Source rows (if any)
    // Next line: Blank line
    // Next lines: Error rows (if any)
    // Next line: Blank line
    // Next line: Headers
    // Next lines: Data rows
    member this.SaveToPath (filePath: string) =
        use writer = new StreamWriter(filePath, false, Encoding.UTF8)
    
        // First line: Name and timestamp
        writer.WriteLine($"{this.name}\t{this.timeStamp}")
    
        // Second line: Blank line
        writer.WriteLine()
    
        // Next lines: Source rows (if any)
        if this.sourceRows.Count > 0 then
            for row in this.sourceRows do
                writer.WriteLine(row)
    
        // Next line: Blank line
        writer.WriteLine()
    
        // Next lines: Error rows (if any)
        if this.errorRows.Count > 0 then
            for row in this.errorRows do
                writer.WriteLine(row)
    
        // Next line: Blank line
        writer.WriteLine()
    
        // Next line: Headers
        writer.WriteLine(String.Join("\t", this.dataHeaders))
    
        // Next lines: Data rows
        for row in this.dataRows do
            writer.WriteLine(row)


    member this.SaveToStream (stream: Stream) =
        use writer = new StreamWriter(stream, Encoding.UTF8, 1024, true)
    
        // First line: Name and timestamp
        writer.WriteLine($"{this.name}\t{this.timeStamp}")
    
        // Second line: Blank line
        writer.WriteLine()
    
        // Next lines: Source rows (if any)
        if this.sourceRows.Count > 0 then
            for row in this.sourceRows do
                writer.WriteLine(row)
    
        // Next line: Blank line
        writer.WriteLine()
    
        // Next lines: Error rows (if any)
        if this.errorRows.Count > 0 then
            for row in this.errorRows do
                writer.WriteLine(row)
    
        // Next line: Blank line
        writer.WriteLine()
    
        // Next line: Headers
        writer.WriteLine(String.Join("\t", this.dataHeaders))
    
        // Next lines: Data rows
        for row in this.dataRows do
            writer.WriteLine(row)



module DataTableReport =

    let create name (headers: string []) : dataTableReport = 
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "Name cannot be null or whitespace"
        if headers.Length = 0 then
            invalidArg "headers" "Headers array cannot be empty"
        
        // Sanitize headers to ensure no tabs break the format
        let cleanHeaders = headers |> Array.map (fun h -> if h = null then "" else h.Replace("\t", " "))
        { name = name
          timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
          sourceRows = ResizeArray()
          errorRows = ResizeArray()
          dataHeaders = cleanHeaders
          dataRows = ResizeArray() }



///// Represents a tab-delimited table with metadata.
///// Internally uses lists for O(1) prepend performance to handle large datasets.
//type dataTableFile private (name: string, sources: string list, headers: string[], rows: string[] list) =
    
//    member _.Name = name
//    /// Returns sources in the correct chronological order (original list is reversed)
//    member _.Sources = sources |> List.rev
//    member _.Headers = headers
//    member _.Rows = rows
//    member _.RowCount = rows.Length
//    member _.ColumnCount = headers.Length
    
//    static member create (name: string) (headers: string[]) : dataTableFile =
//        if String.IsNullOrWhiteSpace(name) then
//            invalidArg "name" "Name cannot be null or whitespace"
//        if headers.Length = 0 then
//            invalidArg "headers" "Headers array cannot be empty"
        
//        // Sanitize headers to ensure no tabs break the format
//        let cleanHeaders = headers |> Array.map (fun h -> if h = null then "" else h.Replace("\t", " "))
//        dataTableFile(name, [], cleanHeaders, [])

//    /// Create a new TextFile with a name and headers from a list
//    static member createFromList (name: string) (headers: string []) : dataTableFile =
//        dataTableFile.create name headers

//    /// Helper to sanitize row data to prevent column shifting
//    static member private Sanitize (row: string[]) : string [] =
//        row |> Array.map (fun s -> if s = null then "" else s.Replace("\t", " "))

//    member this.AddSource (source: string) : dataTableFile =
//        dataTableFile(this.Name, source :: this.Sources, this.Headers, this.Rows)

//    member this.AddSources (newSources: string seq) : dataTableFile =
//        let mutable currentSources = this.Sources
//        let rev = newSources |> Seq.toArray |> Array.rev
//        for s in newSources do
//            currentSources <- s :: currentSources
//        dataTableFile(this.Name, currentSources, this.Headers, this.Rows)

//    member this.AddRow (row: string[]) : dataTableFile =
//        if row.Length <> this.ColumnCount then
//            invalidArg "row" (sprintf "Row has %d columns but header has %d" row.Length this.ColumnCount)
//        let cleanRow = dataTableFile.Sanitize row
//        dataTableFile(this.Name, this.Sources, this.Headers, cleanRow :: rows)

//    member this.AddRows (newRows: string[][] ) : dataTableFile =
//        let mutable currentRows = this.Rows
//        for r in newRows do
//            if r.Length <> this.ColumnCount then
//                invalidArg "newRows" "One or more rows have incorrect column counts"
//            currentRows <- (dataTableFile.Sanitize r) :: currentRows
//        dataTableFile(this.Name, this.Sources, this.Headers, currentRows)

//    /// Efficiently saves the data directly to a file stream.
//    /// This avoids OutOfMemoryExceptions on very large reports.
//    member this.Save (filePath: string) =
//        use writer = new StreamWriter(filePath, false, Encoding.UTF8)
        
//        // 1. Write Sources
//        if not this.Sources.IsEmpty then
//            for source in (this.Sources |> List.rev) do
//                writer.WriteLine(source)
//            writer.WriteLine() // Blank line separator

//        // 2. Write Headers
//        writer.WriteLine(String.Join("\t", this.Headers))

//        // 3. Write Rows
//        for row in (this.Rows |> List.rev) do
//            writer.WriteLine(String.Join("\t", row))

//    /// Returns the full text as a string (use for smaller reports)
//    member this.ToText () =
//        let sb = StringBuilder()
//        let srcArr = this.Sources
//        if srcArr.Length > 0 then
//            srcArr |> List.iter (sb.AppendLine >> ignore)
//            sb.AppendLine() |> ignore
        
//        sb.AppendLine(String.Join("\t", this.Headers)) |> ignore
//        this.Rows |> List.rev |> List.iter (fun r -> 
//            sb.AppendLine(String.Join("\t", r)) |> ignore)
//        sb.ToString()


//module DataTableFile =
    
//    let create name headers : dataTableFile = 
//        dataTableFile.create name headers
    
//    let createFromList name (headers: string seq) : dataTableFile = 
//        dataTableFile.create name (headers |> Seq.toArray)

//    let addSource (source : string) (dt: dataTableFile) : dataTableFile = 
//        dt.AddSource source

//    let addSources (sources: string seq) (dt: dataTableFile) : dataTableFile =
//        dt.AddSources sources

//    let addRow row (dt: dataTableFile) : dataTableFile = 
//        dt.AddRow row

//    let addRows rows (dt: dataTableFile) : dataTableFile = 
//        dt.AddRows rows

//    let toText (dt: dataTableFile) = 
//        dt.ToText()

//    /// Saves the dataTableFile to the specified path
//    let save (path: string) (dt: dataTableFile) = 
//        dt.Save(path)