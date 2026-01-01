namespace GeneSort.Runs
open System
open FSharp.UMX
open GeneSort.Sorter

[<Measure>] type projectName
[<Measure>] type textReportName
[<Measure>] type idValue
[<Measure>] type replNumber
[<Measure>] type generationNumber

module Repl =
    let toString (r: int<replNumber> option) : string =
        match r with
        | Some v -> (UMX.untag v).ToString()
        | None -> "None"

type runParameters =
    private { paramMap : Map<string, string> } // Made immutable.

    static member create (paramMap: Map<string, string>) : runParameters =
        { paramMap = paramMap } // Validation: could add checks for required keys.

    /// Keys for parameters (constants for consistency).
    static member idKey = "Id"
    static member replKey = "Repl"
    static member generationKey = "Generation"
    static member runFinishedKey = "RunFinished"
    static member maxOrbitKey = "MaxOrbit"
    static member sorterCountKey = "SorterCount"
    static member sorterModelTypeKey = "SorterModelType"
    static member sortableArrayTypeKey = "SortableArrayType"
    static member sortingWidthKey = "SortingWidth"
    static member sortableDataTypeKey = "SortingWidthDataType"
    static member mergeDimensionKey = "MergeDimension"
    static member mergeFillTypeKey = "MergeFillType"
    static member stageLengthKey = "StageLength"
    static member ceLengthKey = "CeLength"
    static member projectNameKey = "ProjectName"
    static member textReportNameKey = "ReportName"

    member this.ParamMap = this.paramMap

    member this.toString() =
        this.paramMap
        |> Map.toList
        |> List.map (fun (k, v) -> sprintf "%s: %s" k v)
        |> String.concat ", "

    /// Gets the Id value.
    member this.GetId() : string<idValue> option =
        this.paramMap.TryFind runParameters.idKey |> Option.map UMX.tag<idValue>

    /// Returns a new instance with Id set.
    member this.WithId(id: string<idValue>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.idKey, %id) }

    /// Gets the Repl value.
    member this.GetRepl() : int<replNumber> option =
        this.paramMap.TryFind runParameters.replKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<replNumber> i) | _ -> None)

    member this.GetReplKvp() : (string * string) option =
        this.paramMap.TryFind runParameters.replKey |> Option.map (fun v -> runParameters.replKey, v)

    /// Returns a new instance with Repl set.
    member this.WithRepl(repl: int<replNumber>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.replKey, (UMX.untag repl).ToString()) }

    /// Gets the Generation value.
    member this.GetGeneration() : int<generationNumber> option =
        this.paramMap.TryFind runParameters.generationKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<generationNumber> i) | _ -> None)

    /// Returns a new instance with Generation set.
    member this.WithGeneration(generation: int<generationNumber>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.generationKey, (UMX.untag generation).ToString()) }

    member this.IsRunFinished() : bool option =
        this.paramMap.TryFind runParameters.runFinishedKey
        |> Option.bind (fun v -> Boolean.TryParse v |> function | true, b -> Some b | _ -> None)

    member this.WithRunFinished(finished: bool) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.runFinishedKey, finished.ToString()) }

    /// Gets the mergeFillType value.
    member this.GetMergeFillType() : mergeFillType option =
        this.paramMap.TryFind runParameters.mergeFillTypeKey
        |> Option.bind (fun v -> try Some (MergeFillType.fromString v) with _ -> None)

    member this.GetMergeFillTypeKvp() : (string * string) option =
        this.paramMap.TryFind runParameters.mergeFillTypeKey |> Option.map (fun v -> runParameters.mergeFillTypeKey, v)

    /// Returns a new instance with MergeFillType set.
    member this.WithMergeFillType(mergeFillType: mergeFillType option) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.mergeFillTypeKey, MergeFillType.toString mergeFillType) }

    /// Gets the mergeDimension value.
    member this.GetMergeDimension() : int<mergeDimension> option =
        this.paramMap.TryFind runParameters.mergeDimensionKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<mergeDimension> i) | _ -> None)

    member this.GetMergeDimensionKvp() : (string * string) option =
        this.paramMap.TryFind runParameters.mergeDimensionKey |> Option.map (fun v -> runParameters.mergeDimensionKey, v)

    /// Returns a new instance with MergeDimension set.
    member this.WithMergeDimension(mergeDimension: int<mergeDimension>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.mergeDimensionKey, (UMX.untag mergeDimension).ToString()) }

    /// Gets the SortableDataType value.
    member this.GetSortableDataType() : sortableDataType option =
        this.paramMap.TryFind runParameters.sortableDataTypeKey
        |> Option.bind (fun v -> try Some (SortableDataType.fromString v) with _ -> None)

    member this.GetSortableArrayDataTypeKvp() : (string * string) option =
        this.paramMap.TryFind runParameters.sortableDataTypeKey |> Option.map (fun v -> runParameters.sortableDataTypeKey, v)

    /// Returns a new instance with SortableDataType set.
    member this.WithSortableDataType(sortableDataType: sortableDataType option) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.sortableDataTypeKey, SortableDataType.toString sortableDataType) }

    /// Gets the SorterModelType value.
    member this.GetSorterModelType() : sorterModelType option =
        this.paramMap.TryFind runParameters.sorterModelTypeKey
        |> Option.bind (fun v -> try Some (SorterModelType.fromString v) with _ -> None)

    member this.GetSorterModelKvp() : (string * string) option =
        this.paramMap.TryFind runParameters.sorterModelTypeKey |> Option.map (fun v -> runParameters.sorterModelTypeKey, v)

    /// Returns a new instance with SorterModelType set.
    member this.WithSorterModelType(sorterModelType: sorterModelType option) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.sorterModelTypeKey, SorterModelType.toString sorterModelType) }

    /// Gets the SortingWidth value.
    member this.GetSortingWidth() : int<sortingWidth> option =
        this.paramMap.TryFind runParameters.sortingWidthKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<sortingWidth> i) | _ -> None)

    member this.GetSortingWidthKvp() : (string * string) option =
        this.paramMap.TryFind runParameters.sortingWidthKey |> Option.map (fun v -> runParameters.sortingWidthKey, v)

    /// Returns a new instance with SortingWidth set.
    member this.WithSortingWidth(sortingWidth: int<sortingWidth>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.sortingWidthKey, (UMX.untag sortingWidth).ToString()) }

    /// Gets the MaxOrbit value.
    member this.GetMaxOrbit() : int option =
        this.paramMap.TryFind runParameters.maxOrbitKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some i | _ -> None)

    /// Returns a new instance with MaxOrbit set.
    member this.WithMaxOrbit(maxOrbit: int) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.maxOrbitKey, maxOrbit.ToString()) }

    /// Gets the StageLength value.
    member this.GetStageLength() : int<stageLength> option =
        this.paramMap.TryFind runParameters.stageLengthKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<stageLength> i) | _ -> None)

    /// Returns a new instance with StageLength set.
    member this.WithStageLength(stageLength: int<stageLength>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.stageLengthKey, (UMX.untag stageLength).ToString()) }

    /// Gets the CeLength value.
    member this.GetCeLength() : int<ceLength> option =
        this.paramMap.TryFind runParameters.ceLengthKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<ceLength> i) | _ -> None)

    /// Returns a new instance with CeLength set.
    member this.WithCeLength(ceLength: int<ceLength>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.ceLengthKey, (UMX.untag ceLength).ToString()) }

    /// Gets the SorterCount value.
    member this.GetSorterCount() : int<sorterCount> option =
        this.paramMap.TryFind runParameters.sorterCountKey
        |> Option.bind (fun v -> Int32.TryParse v |> function | true, i -> Some (UMX.tag<sorterCount> i) | _ -> None)

    /// Returns a new instance with SorterCount set.
    member this.WithSorterCount(sorterCount: int<sorterCount>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.sorterCountKey, (UMX.untag sorterCount).ToString()) }

    /// Gets the ProjectName value.
    member this.GetProjectName() : string<projectName> option =
        this.paramMap.TryFind runParameters.projectNameKey |> Option.map UMX.tag<projectName>

    /// Returns a new instance with ProjectName set.
    member this.WithProjectName(projectName: string<projectName>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.projectNameKey, %projectName) }

    /// Gets the TextReportName value.
    member this.GetTextReportName() : string<textReportName> option =
        this.paramMap.TryFind runParameters.textReportNameKey |> Option.map UMX.tag<textReportName>

    /// Returns a new instance with TextReportName set.
    member this.WithTextReportName(reportName: string<textReportName>) : runParameters =
        { paramMap = this.paramMap.Add(runParameters.textReportNameKey, %reportName) }

module RunParameters =

    let filterByParameters
            (runParametersSet: runParameters array)
            (filter: (string * string) array) : runParameters [] =
        runParametersSet
        |> Array.filter (fun rp ->
            filter |> Array.forall (fun (key, value) ->
                rp.ParamMap.ContainsKey key && rp.ParamMap.[key] = value))

    let pickByParameters
            (runParametersSet: runParameters array)
            (filter: (string * string) array) : runParameters option =
        let filtrate = filterByParameters runParametersSet filter
        if filtrate.Length = 1 then Some filtrate.[0] else None

    let getAllKeys (runParams :runParameters seq) : string[] =
        runParams
        |> Seq.collect (fun rp -> rp.ParamMap.Keys)
        |> Seq.distinct
        |> Seq.toArray

    let tableToTabDelimited (table: string[][]) : string[] =
        table |> Array.map (String.concat "\t")

    let makeIndexAndReplTable(runParams : runParameters seq) : string[][]
        =
        let keys = getAllKeys runParams
        let headerRow = Array.append [| "Run" |] keys
        let dataRows =
            runParams
            |> Seq.map (fun rp ->
                keys
                |> Array.map (fun key -> rp.ParamMap.TryFind key |> Option.defaultValue "N/A"))
            |> Seq.toArray
        Array.append [| keys |] dataRows // Fixed: header was wrong.


    let toStringTable(runParams :runParameters seq) : string =
        makeIndexAndReplTable runParams
        |> Array.map (fun row -> String.Join("\t", row))
        |> String.concat "\n"