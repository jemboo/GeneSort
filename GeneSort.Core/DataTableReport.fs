namespace GeneSort.Core
open System
open System.IO
open System.Text

type dataTableReport = 
    private 
        { name: string
          timeStamp: string
          dataHeaders: string[]
          dataRows: ResizeArray<string> }
    
    member this.Name with get() = this.name
    member this.TimeStamp with get() = this.timeStamp
    member this.DataHeaders with get() = this.dataHeaders
    member this.DataRows with get() = this.dataRows

    static member Empty(name:string) : dataTableReport =
        { name = name
          timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
          dataHeaders = [||]
          dataRows = ResizeArray<string>() }

    /// Helper to sanitize row data to prevent column shifting
    static member private Sanitize (row: string[]) : string [] =
        row |> Array.map (fun s -> if s = null then "" else s.Replace("\t", " "))


    member this.AppendDataRow (rowArray: string []) = 
        let cleanRow = String.Join("\t", dataTableReport.Sanitize rowArray)
        this.dataRows.Add(cleanRow)

    member this.AppendDataRows (rows: string [] seq) =
        for r in rows do
            this.AppendDataRow(r)


    member this.SaveToStream (stream: Stream) =
        use writer = new StreamWriter(stream, Encoding.UTF8, 1024, true)
    
        //// First line: Name and timestamp
        //writer.WriteLine($"{this.name}\t{this.timeStamp}")
    
        //// Second line: Blank line
        //writer.WriteLine()
    
        //// Next line: Blank line
        //writer.WriteLine()
    
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
          dataHeaders = cleanHeaders
          dataRows = ResizeArray() }


    let fromDataTableRecords (records: dataTableRecord seq) : dataTableReport =
        if (records |> Seq.isEmpty) then 
            dataTableReport.Empty("no records")
        else
            let (headers, rows) = dataTableRecord.createTable records
            let report = create "DataTableReport" headers
            report.AppendDataRows rows
            report


    let saveToPath (filePath: string) (rows: string [][]) =
        let append = false // overwrite existing file
        use writer = new StreamWriter(filePath, append, Encoding.UTF8)
        for row in rows do
            writer.WriteLine(String.Join("\t", row))


    let concatenateAllFiles (rootPath: string) (outputPath: string) =
        // 1. Get all .txt files in the root and all subdirectories
        let files = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories)
    
        // 2. Open a FileStream for writing the combined result
        using (new StreamWriter(outputPath, false, Encoding.UTF8)) (fun writer ->
            for file in files do
                // Read the content of each file and write it to the output
                let content = File.ReadAllText(file)
                writer.WriteLine(content)
            
                // Optional: Add a separator or newline between files
                writer.WriteLine("---") 
        )