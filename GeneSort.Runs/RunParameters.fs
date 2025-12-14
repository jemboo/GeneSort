namespace GeneSort.Runs

open System
open GeneSort.Sorter
open FSharp.UMX
open GeneSort.Sorter.Sortable

[<Measure>] type projectName
[<Measure>] type textReportName
[<Measure>] type indexNumber
[<Measure>] type replNumber
[<Measure>] type generationNumber

type runParameters =
        private { mutable paramMap : Map<string, string> }
    with

    static member create (paramMap: Map<string, string>) : runParameters =
        { paramMap = paramMap }

    /// Keys for parameters
    static member indexKey = "Index"
    static member replKey = "Repl"
    static member generationKey = "Generation"
    static member runFinishedKey = "RunFinished"
    static member maxOrbitKey = "MaxOrbit"
    static member sorterCountKey = "SorterCount"
    static member sorterModelTypeKey = "SorterModelType"
    static member sortableArrayTypeKey = "SortableArrayType"
    static member sortingWidthKey = "SortingWidth"
    static member sortableArrayDataTypeKey = "SortingWidthDataType"
    static member mergeDimensionKey = "MergeDimension"
    static member mergeFillTypeKey = "MergeFillType"
    static member stageLengthKey = "StageLength"
    static member ceLengthKey = "CeLength"
    static member projectNameKey = "ProjectName"
    static member textReportNameKey = "ReportName"


    member this.ParamMap with get() = this.paramMap

    member this.toString() =
        this.paramMap
        |> Map.toList
        |> List.map (fun (k, v) -> sprintf "%s: %s" k v)
        |> String.concat ", "


    /// Gets the Index value.
    member this.GetIndex() : int<indexNumber> option =
        match this.paramMap.TryFind runParameters.indexKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<indexNumber>)
            | false, _ -> None
        | None -> None
    
    /// Sets the Index value.
    member this.SetIndex(index: int<indexNumber>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.indexKey, (UMX.untag index).ToString())



    /// Gets the Repl value.
    member this.GetRepl() : int<replNumber> option =
        match this.paramMap.TryFind runParameters.replKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<replNumber>)
            | false, _ -> None
        | None -> None

    member this.GetReplKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.replKey with
        | Some value -> Some (runParameters.replKey, value)
        | None -> None

    /// Sets the Repl value.
    member this.SetRepl(repl: int<replNumber>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.replKey, (UMX.untag repl).ToString())



    /// Gets the Generation value.
    member this.GetGeneration() : int<generationNumber> option =
        match this.paramMap.TryFind runParameters.generationKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<generationNumber>)
            | false, _ -> None
        | None -> None

    /// Sets the Generation value.
    member this.SetGeneration(generation: int<generationNumber>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.generationKey, (UMX.untag generation).ToString())



    member this.IsRunFinished() : bool option =
        match this.paramMap.TryFind runParameters.runFinishedKey with
        | Some value -> 
            match Boolean.TryParse(value) with
            | true, v -> Some v
            | false, _ -> None
        | None -> None

    member this.SetRunFinished(finished: bool) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.runFinishedKey, finished.ToString())



    /// Gets the mergeFillType value.
    member this.GetMergeFillType() : mergeFillType option =
        match this.paramMap.TryFind runParameters.mergeFillTypeKey with
        | Some value -> 
            try
                Some (MergeFillType.fromString value)
            with
            | _ -> None
        | None -> None


    member this.GetMergeFillTypeKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.mergeFillTypeKey with
        | Some value -> Some (runParameters.mergeFillTypeKey, value)
        | None -> None


    /// Sets the SortableArrayType value.
    member this.SetMergeFillType(mergeFillType: mergeFillType option) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.mergeFillTypeKey, MergeFillType.toString mergeFillType)




    /// Gets the SorterModelKey value.
    member this.GetSorterModelKey() : sorterModelKey option =
        match this.paramMap.TryFind runParameters.sorterModelTypeKey with
        | Some value -> 
            try
                Some (SorterModelKey.fromString value)
            with
            | _ -> None
        | None -> None

    member this.GetSorterModelKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.sorterModelTypeKey with
        | Some value -> Some (runParameters.sorterModelTypeKey, value)
        | None -> None

    /// Sets the SorterModelKey value.
    member this.SetSorterModelKey(sorterModelKey: sorterModelKey option) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sorterModelTypeKey, SorterModelKey.toString sorterModelKey)




    /// Gets the mergeDimension value.
    member this.GetMergeDimensionKey() : int<mergeDimension> option =
        match this.paramMap.TryFind runParameters.mergeDimensionKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<mergeDimension>)
            | false, _ -> None
        | None -> None

    member this.GetMergeDimensionKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.mergeDimensionKey with
        | Some value -> Some (runParameters.mergeDimensionKey, value)
        | None -> None

    /// Sets the mergeDimension value.
    member this.SetMergeDimension(mergeDimension: int<mergeDimension> option) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.mergeDimensionKey, (%mergeDimension).ToString())





    /// Gets the GetSortableArrayDataType value.
    member this.GetSortableArrayDataTypeKey() : sortableArrayDataType option =
        match this.paramMap.TryFind runParameters.sortableArrayDataTypeKey with
        | Some value -> 
            try
                Some (SortableArrayDataType.fromString value)
            with
            | _ -> None
        | None -> None

    member this.GetSortableArrayDataTypeKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.sortableArrayDataTypeKey with
        | Some value -> Some (runParameters.sortableArrayDataTypeKey, value)
        | None -> None

    /// Sets the GetSortableArrayDataType value.
    member this.GetSortableArrayDataTypeKey(sortableArrayDataType: sortableArrayDataType option) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sortableArrayDataTypeKey, SortableArrayDataType.toString sortableArrayDataType)






    /// Gets the SorterModelKey value.
    member this.GetSorterModelKey() : sorterModelKey option =
        match this.paramMap.TryFind runParameters.sorterModelTypeKey with
        | Some value -> 
            try
                Some (SorterModelKey.fromString value)
            with
            | _ -> None
        | None -> None

    member this.GetSorterModelKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.sorterModelTypeKey with
        | Some value -> Some (runParameters.sorterModelTypeKey, value)
        | None -> None

    /// Sets the SorterModelKey value.
    member this.SetSorterModelKey(sorterModelKey: sorterModelKey option) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sorterModelTypeKey, SorterModelKey.toString sorterModelKey)





    /// Gets the SortingWidth value.
    member this.GetSortingWidth() : int<sortingWidth> option =
        match this.paramMap.TryFind runParameters.sortingWidthKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<sortingWidth>)
            | false, _ -> None
        | None -> None

    member this.GetSortingWidthKvp() : (string * string) option =
        match this.paramMap.TryFind runParameters.sortingWidthKey with
        | Some value -> Some (runParameters.sortingWidthKey, value)
        | None -> None

    /// Sets the SortingWidth value.
    member this.SetSortingWidth(sortingWidth: int<sortingWidth>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sortingWidthKey, (UMX.untag sortingWidth).ToString())



    /// Gets the MaxOrbit value.
    member this.GetMaxOrbit() : int option =
        match this.paramMap.TryFind runParameters.maxOrbitKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some v
            | false, _ -> None
        | None -> None

    /// Sets the MaxOrbit value.
    member this.SetMaxOrbit(maxOrbit: int) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.maxOrbitKey, maxOrbit.ToString())



    /// Gets the StageLength value.
    member this.GetStageLength() : int<stageLength> option =
        match this.paramMap.TryFind runParameters.stageLengthKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<stageLength>)
            | false, _ -> None
        | None -> None

    /// Sets the StageLength value.
    member this.SetStageLength(stageLength: int<stageLength>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.stageLengthKey, (UMX.untag stageLength).ToString())



    /// Gets the CeLength value.
    member this.GetCeLength() : int<ceLength> option =
        match this.paramMap.TryFind runParameters.ceLengthKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<ceLength>)
            | false, _ -> None
        | None -> None

    /// Sets the CeLength value.
    member this.SetCeLength(ceLength: int<ceLength>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.ceLengthKey, (UMX.untag ceLength).ToString())



    /// Sets the SorterCount value.
    member this.SetSorterCount(sorterCount: int<sorterCount>) : unit =
       this.paramMap <- this.paramMap.Add(runParameters.sorterCountKey, (UMX.untag sorterCount).ToString())

    /// Gets the SorterCount value.
    member this.GetSorterCount() : int<sorterCount> option =
        match this.paramMap.TryFind runParameters.sorterCountKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> Some (LanguagePrimitives.Int32WithMeasure v |> UMX.tag<sorterCount>)
            | false, _ -> None
        | None -> None



    /// Sets the ProjectName value.
    member this.SetProjectName(projectName: string<projectName>) : unit =
       this.paramMap <- this.paramMap.Add(runParameters.projectNameKey, %projectName)

    /// Gets the ProjectName value.
    member this.GetProjectName() : string<projectName> option =
        match (this.paramMap.TryFind runParameters.projectNameKey) with
        | Some value -> Some (UMX.tag<projectName> value)
        | None -> None



    /// Sets the TextReportName value.
    member this.SetTextReportName(reportName: string<textReportName>) : unit =
       this.paramMap <- this.paramMap.Add(runParameters.textReportNameKey, %reportName)

    /// Gets the TextReportName value.
    member this.GetTextReportName() : string<textReportName> option =
        match (this.paramMap.TryFind runParameters.textReportNameKey) with
        | Some value -> Some (UMX.tag<textReportName> value)
        | None -> None



module RunParameters =  
                 
    let filterByParameters 
            (runParametersSet: runParameters array) 
            (filter: (string * string) array) : runParameters [] =
        runParametersSet
        |> Array.filter (
            fun runParameters ->
                filter |> Array.forall (fun (key, value) ->
                    runParameters.ParamMap.ContainsKey key && runParameters.ParamMap.[key] = value
                ))

    let pickByParameters
            (runParametersSet: runParameters array) 
            (filter: (string * string) array) : runParameters option =
        let filtrate = filterByParameters runParametersSet filter
        match filtrate.Length with
        | 1 -> Some filtrate.[0]
        | _ -> None

    let getAllKeys (runParams :runParameters seq) : string[] =
        runParams
        |> Seq.collect (fun rp -> rp.ParamMap |> Map.toSeq |> Seq.map fst)
        |> Seq.distinct
        |> Seq.toArray

    let getIndexAndReplName (runParams: runParameters) : string =
        let indexStr = 
            match runParams.GetIndex() with
            | Some index -> sprintf "Index_%d" (UMX.untag index)
            | None -> "Index_None"
        let replStr = 
            match runParams.GetRepl() with
            | Some repl -> sprintf "Repl_%d" (UMX.untag repl)
            | None -> "Repl_None"
        sprintf "%s_%s" indexStr replStr 


    let tableToTabDelimited (table: string[][]) : string[] =
        table
        |> Array.map (fun row -> String.concat "\t" row)


    //creates a table with the getIndexAndReplName as the row headers, and the array from
    //getAllKeys as the column headers
    let makeIndexAndReplTable(runParams :runParameters seq) : string[][] 
        =
        let keys = getAllKeys runParams
        let headerRow = Array.append [| "Run" |] keys
        let dataRows =
            runParams
            |> Seq.map (fun rp ->
                let rowHeader = getIndexAndReplName rp
                let rowValues =
                    keys
                    |> Array.map (fun key ->
                        match rp.ParamMap.TryFind key with
                        | Some value -> value
                        | None -> "N/A"
                    )
                Array.append [| rowHeader |] rowValues
            )
            |> Seq.toArray
        Array.append [| headerRow |] dataRows
    

    let toStringTable(runParams :runParameters seq) : string =
        let table = makeIndexAndReplTable runParams
        let sb = System.Text.StringBuilder()
        for row in table do
            sb.AppendLine(String.Join("\t", row)) |> ignore
        sb.ToString()