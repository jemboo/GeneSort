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
        //let cleanedData = dataTableReport.Sanitize data
        this.sourceRows.AddRange(data)

    member this.AddErrors (data: string []) =
        //let cleanedData = dataTableReport.Sanitize data
        this.errorRows.AddRange(data)

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