namespace GeneSort.Core

open System
open System.IO
open System.Text

/// Represents a tab-delimited table with metadata.
/// Internally uses lists for O(1) prepend performance to handle large datasets.
type dataTableFile0 private (name: string, sources: string list, headers: string[], rows: string[] list) =
    
    member _.Name = name
    /// Returns sources in the correct chronological order (original list is reversed)
    member _.Sources = sources |> List.rev
    member _.Headers = headers
    member _.Rows = rows
    member _.RowCount = rows.Length
    member _.ColumnCount = headers.Length
    
    static member create (name: string) (headers: string[]) : dataTableFile0 =
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "Name cannot be null or whitespace"
        if headers.Length = 0 then
            invalidArg "headers" "Headers array cannot be empty"
        
        // Sanitize headers to ensure no tabs break the format
        let cleanHeaders = headers |> Array.map (fun h -> if h = null then "" else h.Replace("\t", " "))
        dataTableFile0(name, [], cleanHeaders, [])

    /// Create a new TextFile with a name and headers from a list
    static member createFromList (name: string) (headers: string []) : dataTableFile0 =
        dataTableFile0.create name headers

    /// Helper to sanitize row data to prevent column shifting
    static member private Sanitize (row: string[]) : string [] =
        row |> Array.map (fun s -> if s = null then "" else s.Replace("\t", " "))

    member this.AddSource (source: string) : dataTableFile0 =
        dataTableFile0(this.Name, source :: this.Sources, this.Headers, this.Rows)

    member this.AddSources (newSources: string seq) : dataTableFile0 =
        let mutable currentSources = this.Sources
        let rev = newSources |> Seq.toArray |> Array.rev
        for s in newSources do
            currentSources <- s :: currentSources
        dataTableFile0(this.Name, currentSources, this.Headers, this.Rows)

    member this.AddRow (row: string[]) : dataTableFile0 =
        if row.Length <> this.ColumnCount then
            invalidArg "row" (sprintf "Row has %d columns but header has %d" row.Length this.ColumnCount)
        let cleanRow = dataTableFile0.Sanitize row
        dataTableFile0(this.Name, this.Sources, this.Headers, cleanRow :: rows)

    member this.AddRows (newRows: string[][] ) : dataTableFile0 =
        let mutable currentRows = this.Rows
        for r in newRows do
            if r.Length <> this.ColumnCount then
                invalidArg "newRows" "One or more rows have incorrect column counts"
            currentRows <- (dataTableFile0.Sanitize r) :: currentRows
        dataTableFile0(this.Name, this.Sources, this.Headers, currentRows)

    /// Efficiently saves the data directly to a file stream.
    /// This avoids OutOfMemoryExceptions on very large reports.
    member this.Save (filePath: string) =
        use writer = new StreamWriter(filePath, false, Encoding.UTF8)
        
        // 1. Write Sources
        if not this.Sources.IsEmpty then
            for source in (this.Sources |> List.rev) do
                writer.WriteLine(source)
            writer.WriteLine() // Blank line separator

        // 2. Write Headers
        writer.WriteLine(String.Join("\t", this.Headers))

        // 3. Write Rows
        for row in (this.Rows |> List.rev) do
            writer.WriteLine(String.Join("\t", row))



module DataTableFile =
    
    let create name headers : dataTableFile0 = 
        dataTableFile0.create name headers
    
    let createFromList name (headers: string seq) : dataTableFile0 = 
        dataTableFile0.create name (headers |> Seq.toArray)

    let addSource (source : string) (dt: dataTableFile0) : dataTableFile0 = 
        dt.AddSource source

    let addSources (sources: string seq) (dt: dataTableFile0) : dataTableFile0 =
        dt.AddSources sources

    let addRow row (dt: dataTableFile0) : dataTableFile0 = 
        dt.AddRow row

    let addRows rows (dt: dataTableFile0) : dataTableFile0 = 
        dt.AddRows rows

    /// Saves the dataTableFile to the specified path
    let save (path: string) (dt: dataTableFile0) = 
        dt.Save(path)