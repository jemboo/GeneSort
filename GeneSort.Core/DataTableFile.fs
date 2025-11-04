namespace GeneSort.Core

open System
open System.Text

/// Represents a tab-delimited table with metadata
type dataTableFile private (name: string, sources: string [], headers: string[], rows: string[][]) =
    
    member _.Name = name
    member _.Sources = sources
    member _.Headers = headers
    member _.Rows = rows
    member _.RowCount = rows.Length
    member _.ColumnCount = headers.Length
    
    /// Create a new TextFile with a name and headers
    static member create (name: string) (headers: string[]) : dataTableFile =
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "Name cannot be null or whitespace"
        if headers.Length = 0 then
            invalidArg "headers" "Headers array cannot be empty"
        if headers |> Array.exists String.IsNullOrWhiteSpace then
            invalidArg "headers" "Headers cannot contain null or whitespace strings"
        dataTableFile(name, [||], headers, [||])
    
    /// Create a new TextFile with a name and headers from a list
    static member createFromList (name: string) (headers: string []) : dataTableFile =
        dataTableFile.create name headers
    
    /// Change the name of the TextFile
    member this.WithName (newName: string) : dataTableFile =
        if String.IsNullOrWhiteSpace(newName) then
            invalidArg "newName" "Name cannot be null or whitespace"
        dataTableFile(newName, this.Sources, this.Headers, this.Rows)
    
    /// Add a source line to the metadata section
    member this.AddSource (source: string) : dataTableFile =
        dataTableFile(this.Name, Array.append this.Sources [|source|], this.Headers, this.Rows)
    
    /// Add multiple source lines to the metadata section
    member this.AddSources (sources: string []) : dataTableFile =
        dataTableFile(this.Name, Array.append this.Sources sources, this.Headers, this.Rows)
    
    /// Add a data row (validates column count)
    member this.AddRow (row: string[]) : dataTableFile =
        if row.Length <> this.ColumnCount then
            invalidArg "row" (sprintf "Row has %d columns but header has %d columns" row.Length this.ColumnCount)
        dataTableFile(this.Name, this.Sources, this.Headers, Array.append this.Rows [|row|])
    
    /// Add a data row from a list
    member this.AddRowFromList (row: string list) : dataTableFile =
        this.AddRow (row |> List.toArray)
    
    /// Add multiple data rows
    member this.AddRows (rows: string[][] ) : dataTableFile =
        let mutable result = this
        for row in rows do
            result <- result.AddRow row
        result
    
    /// Add multiple data rows from lists
    member this.AddRowsFromLists (rows: string list list) : dataTableFile =
        this.AddRows (rows |> List.map List.toArray |> List.toArray)
    
    /// Get the full text representation with tab delimiters
    member this.ToText () : string =
        let sb = StringBuilder()
        
        // Add sources section
        if this.Sources.Length > 0 then
            for source in this.Sources do
                sb.AppendLine(source) |> ignore
            sb.AppendLine() |> ignore // Blank line after sources
        
        // Add header row
        sb.AppendLine(String.Join("\t", this.Headers)) |> ignore
        
        // Add data rows
        for row in this.Rows do
            sb.AppendLine(String.Join("\t", row)) |> ignore
        
        sb.ToString()
    
    /// Get only the table portion (headers + rows) without sources
    member this.ToTableText () : string =
        let sb = StringBuilder()
        sb.AppendLine(String.Join("\t", this.Headers)) |> ignore
        for row in this.Rows do
            sb.AppendLine(String.Join("\t", row)) |> ignore
        sb.ToString()
    
    /// Get only the sources section
    member this.ToSourcesText () : string =
        if this.Sources.Length = 0 then
            ""
        else
            let sb = StringBuilder()
            for source in this.Sources do
                sb.AppendLine(source) |> ignore
            sb.ToString()
    
    /// Get a specific row by index (0-based, from first added)
    member this.GetRow (index: int) : string[] option =
        if index >= 0 && index < this.Rows.Length then
            Some this.Rows.[index]
        else
            None
    
    /// Get all rows in insertion order
    member this.GetAllRows () : string[][] =
        this.Rows
    
    /// Clear all data rows but keep name, headers and sources
    member this.ClearRows () : dataTableFile =
        dataTableFile(this.Name, this.Sources, this.Headers, [||])
    
    /// Clear all sources but keep name, headers and rows
    member this.ClearSources () : dataTableFile =
        dataTableFile(this.Name, [||], this.Headers, this.Rows)
    
    /// Create a new TextFile with a transformed row added
    member this.AddRowWith (values: 'a[]) (formatter: 'a -> string) : dataTableFile =
        let row = values |> Array.map formatter
        this.AddRow row
    
    /// Filter rows based on a predicate
    member this.FilterRows (predicate: string[] -> bool) : dataTableFile =
        let filteredRows = 
            this.Rows 
            |> Array.filter predicate
        dataTableFile(this.Name, this.Sources, this.Headers, filteredRows)


/// Builder for fluent construction of TextFile
type dataTableFileBuilder(name: string, headers: string[]) =
    let mutable dataTableFile = dataTableFile.create name headers
    
    member _.Yield(_) = dataTableFile
    
    member _.AddSource(source: string) =
        dataTableFile <- dataTableFile.AddSource source
        dataTableFile
    
    member _.AddRow(row: string[]) =
        dataTableFile <- dataTableFile.AddRow row
        dataTableFile
    
    member _.Build() = dataTableFile


/// Module with helper functions for TextFile
module DataTableFile =
    
    /// Create a new TextFile with a name and headers
    let create (name: string) (headers: string[]) : dataTableFile =
        dataTableFile.create name headers
    
    /// Create a new TextFile with a name and headers from a list
    let createFromList (name: string) (headers: string list) : dataTableFile =
        dataTableFile.createFromList name (headers |> List.toArray)
    
    /// Change the name of a TextFile
    let withName (newName: string) (dataTableFile: dataTableFile) : dataTableFile =
        dataTableFile.WithName newName
    
    /// Add a source line
    let addSource (source: string) (dataTableFile: dataTableFile) : dataTableFile =
        dataTableFile.AddSource source
    
    /// Add multiple source lines
    let addSources (sources: string []) (dataTableFile: dataTableFile) : dataTableFile =
        dataTableFile.AddSources sources
    
    /// Add a data row
    let addRow (row: string[]) (dataTableFile: dataTableFile) : dataTableFile =
        dataTableFile.AddRow row
    
    /// Add multiple data rows
    let addRows (rows: string[][]) (dataTableFile: dataTableFile) : dataTableFile =
        dataTableFile.AddRows rows
    
    /// Convert to full text
    let toText (dataTableFile: dataTableFile) : string =
        dataTableFile.ToText()
    
    /// Convert to table only (no sources)
    let toTableText (dataTableFile: dataTableFile) : string =
        dataTableFile.ToTableText()
    
    /// Get the name
    let name (dataTableFile: dataTableFile) : string =
        dataTableFile.Name
    
    /// Get row count
    let rowCount (dataTableFile: dataTableFile) : int =
        dataTableFile.RowCount
    
    /// Get column count
    let columnCount (dataTableFile: dataTableFile) : int =
        dataTableFile.ColumnCount
    
    /// Pipeline-friendly row addition with type conversion
    let addRowWith (formatter: 'a -> string) (values: 'a[]) (dataTableFile: dataTableFile) : dataTableFile =
        dataTableFile.AddRowWith values formatter
    
    /// Example: Add a row of integers
    let addIntRow (values: int[]) (dataTableFile: dataTableFile) : dataTableFile =
        addRowWith string values dataTableFile