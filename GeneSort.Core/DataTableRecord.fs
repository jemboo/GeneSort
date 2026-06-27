namespace GeneSort.Core

type dataTableRecord = 
    private 
        { keys: Map<string, string>
          data: Map<string, string> }
    
    member this.Keys with get() = this.keys
    member this.Data with get() = this.data
    
    static member createEmpty() : dataTableRecord =
        { keys = Map.empty; data = Map.empty }

    static member create ( keys: Map<string, string> ) 
                         ( data: Map<string, string> ) : dataTableRecord =
        { keys = keys; data = data }

    /// Helper to sanitize row data to prevent column shifting
    static member addKeyAndData (key: string) (value: string) (dataTableRecord: dataTableRecord) : dataTableRecord =
        let cleanedKey = if key = null then "" else key.Replace("\t", " ")
        let cleanedValue = if value = null then "" else value.Replace("\t", " ")
        { keys = dataTableRecord.Keys.Add(cleanedKey, cleanedValue); data = dataTableRecord.Data.Add(cleanedKey, cleanedValue) }

    static member addData (key: string) (value: string) (dataTableRecord: dataTableRecord) : dataTableRecord =
        let cleanedKey = if key = null then "" else key.Replace("\t", " ")
        let cleanedValue = if value = null then "" else value.Replace("\t", " ")
        { keys = dataTableRecord.Keys; data = dataTableRecord.Data.Add(cleanedKey, cleanedValue) }

    // Merges keys and data from both.
    static member combine (record1: dataTableRecord) (record2: dataTableRecord) : dataTableRecord =
        let mergedKeys = record1.Keys |> Map.fold (fun (acc: Map<string,string>) k v -> acc.Add(k, v)) record2.Keys
        let mergedData = record1.Data |> Map.fold (fun (acc: Map<string,string>) k v -> acc.Add(k, v)) record2.Data
        { keys = mergedKeys; data = mergedData }

    //// Combines a root record with all records, merging keys and data from both.
    static member combineWithMany (records: dataTableRecord seq) (rootRecord: dataTableRecord)  : dataTableRecord seq =
        records |> Seq.map (fun r -> dataTableRecord.combine rootRecord r)

    static member createWithKeyAndData (key: string) (value: string) : dataTableRecord =
        let cleanedKey = if key = null then "" else key.Replace("\t", " ")
        let cleanedValue = if value = null then "" else value.Replace("\t", " ")
        { keys = Map.ofList [(cleanedKey, cleanedValue)]; data = Map.ofList [(cleanedKey, cleanedValue)] }

    static member createTable (records: dataTableRecord seq) : (string []) * (string [][]) =
        let allKeys = records |> Seq.collect (fun r -> r.Data |> Map.toSeq) |> Seq.map fst |> Seq.distinct
        let header = allKeys |> Seq.toArray
        let rows = 
            records 
            |> Seq.map (fun r -> 
                header 
                |> Array.map (fun k -> if r.Data.ContainsKey(k) then r.Data.[k] else ""))
            |> Seq.toArray
        (header, rows)




open System.IO

module DataTableIO =

    /// Writes a sequence of dataTableRecords to a tab-delimited file.
    /// It dynamically extracts all unique headers across all records to guarantee alignment.
    let writeToFile (filePath: string) (records: dataTableRecord seq) : unit =
        if Seq.isEmpty records then
            // If there's nothing to write, we can create an empty file or just return.
            File.WriteAllText(filePath, "")
        else
            let headers, rows = dataTableRecord.createTable records
            
            // Open a stream writer to write lines efficiently
            use writer = new StreamWriter(filePath)
            
            // 1. Write Header Row
            let headerLine = String.concat "\t" headers
            writer.WriteLine(headerLine)
            
            // 2. Write Data Rows
            for row in rows do
                let rowLine = String.concat "\t" row
                writer.WriteLine(rowLine)


    /// Reads a tab-delimited file and reconstructs a sequence of dataTableRecords.
    /// It treats all column values as 'data' and leaves 'keys' empty, or you can adjust 
    /// if specific column headers represent keys.
    let readFromFile (filePath: string) : dataTableRecord seq =
        seq {
            if File.Exists(filePath) then
                use reader = new StreamReader(filePath)
                
                // Read the header row first
                let headerLine = reader.ReadLine()
                if not (System.String.IsNullOrWhiteSpace(headerLine)) then
                    let headers = headerLine.Split('\t')
                    
                    // Process subsequent data rows
                    while not reader.EndOfStream do
                        let line = reader.ReadLine()
                        if not (System.String.IsNullOrWhiteSpace(line)) then
                            let values = line.Split('\t')
                            
                            // Map headers to row values zip style, handling potential length mismatches Safely
                            let rowData = 
                                headers 
                                |> Array.mapi (fun i header -> 
                                    let value = if i < values.Length then values.[i] else ""
                                    (header, value))
                                |> Map.ofArray
                                
                            // Reconstitute the record (assuming everything read back goes into data)
                            yield dataTableRecord.create Map.empty rowData
        }

    // reads all the dataTableRecords from all the files, combines them, and writes them to outputPath
    let concatenateAllFiles (rootPath: string) (outputPath: string) =
        // 1. Get all .txt files in the root and all subdirectories
        let files = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories)
        // 2. Read and flat-map records from all found files lazily
        let allRecords = 
            files 
            |> Seq.ofArray 
            |> Seq.collect readFromFile

        // 3. Re-serialize all records into a single consolidated layout
        writeToFile outputPath allRecords