namespace GeneSort.Runs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter

[<Measure>] type projectName
[<Measure>] type textReportName
[<Measure>] type idValue
[<Measure>] type replNumber
[<Measure>] type generationNumber

type runParameters =
    private { paramMap : Map<string, string> }

    static member create (paramMap: Map<string, string>) : runParameters =
        { paramMap = paramMap }

    // Constants for consistency
    static member idKey = "Id"
    static member replKey = "Repl"
    static member generationKey = "Generation"
    static member runFinishedKey = "RunFinished"
    static member maxOrbitKey = "MaxOrbit"
    static member sorterCountKey = "SorterCount"
    static member sorterModelTypeKey = "SorterModelType"
    static member sortableArrayTypeKey = "SortableArrayType"
    static member sortingWidthKey = "SortingWidth"
    static member sortableCountKey = "SortableCount"
    static member sortableDataTypeKey = "SortingWidthDataType"
    static member mergeDimensionKey = "MergeDimension"
    static member mergeFillTypeKey = "MergeFillType"
    static member stageLengthKey = "StageLength"
    static member ceLengthKey = "CeLength"
    static member projectNameKey = "ProjectName"
    static member textReportNameKey = "ReportName"

    // Private Parsing Helpers
    static member private tryGetGuid (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Guid.TryParse v with true, g -> Some g | _ -> None)

    static member private tryGetInt (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Int32.TryParse v with true, i -> Some i | _ -> None)
    
    static member private tryGetBool (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Boolean.TryParse v with true, b -> Some b | _ -> None)

    //static member private addIfSome key valueOpt map =
    //    valueOpt |> Option.fold (fun m v -> Map.add key v m) map

    static member private addOrRemove key valueOpt map =
        match valueOpt with
        | Some v -> Map.add key v map
        | None   -> Map.remove key map

    member this.ParamMap = this.paramMap

    // --- Getters ---
    member this.GetId() = runParameters.tryGetGuid runParameters.idKey this.paramMap |> Option.map UMX.tag<idValue>
    member this.GetRepl() = runParameters.tryGetInt runParameters.replKey this.paramMap |> Option.map UMX.tag<replNumber>
    member this.GetGeneration() = runParameters.tryGetInt runParameters.generationKey this.paramMap |> Option.map UMX.tag<generationNumber>
    member this.IsRunFinished() = runParameters.tryGetBool runParameters.runFinishedKey this.paramMap
    member this.GetSortingWidth() = runParameters.tryGetInt runParameters.sortingWidthKey this.paramMap |> Option.map UMX.tag<sortingWidth>
    member this.GetMergeDimension() = runParameters.tryGetInt runParameters.mergeDimensionKey this.paramMap |> Option.map UMX.tag<mergeDimension>
    member this.GetMaxOrbit() = runParameters.tryGetInt runParameters.maxOrbitKey this.paramMap
    member this.GetStageLength() = runParameters.tryGetInt runParameters.stageLengthKey this.paramMap |> Option.map UMX.tag<stageLength>
    member this.GetCeLength() = runParameters.tryGetInt runParameters.ceLengthKey this.paramMap |> Option.map UMX.tag<ceLength>
    member this.GetSortableCount() = runParameters.tryGetInt runParameters.sortableCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    member this.GetSorterCount() = runParameters.tryGetInt runParameters.sorterCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    
    member this.GetProjectName() = this.paramMap.TryFind runParameters.projectNameKey |> Option.map UMX.tag<projectName>
    member this.GetTextReportName() = this.paramMap.TryFind runParameters.textReportNameKey |> Option.map UMX.tag<textReportName>

    member this.GetSorterModelType() = 
        this.paramMap.TryFind runParameters.sorterModelTypeKey |> Option.bind (fun v -> try Some (SorterModelType.fromString v) with _ -> None)
    
    member this.GetMergeFillType() = 
        this.paramMap.TryFind runParameters.mergeFillTypeKey |> Option.bind (fun v -> try Some (MergeFillType.fromString v) with _ -> None)

    member this.GetSortableDataType() = 
        this.paramMap.TryFind runParameters.sortableDataTypeKey |> Option.bind (fun v -> try Some (SortableDataType.fromString v) with _ -> None)

// --- Functional Updates (Fluent API) ---
    member this.WithMergeDimension(md: int<mergeDimension> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeDimensionKey (md |> Option.map UmxExt.intToRaw) }

    member this.WithMergeFillType(mft: mergeFillType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeFillTypeKey (mft |> Option.map MergeFillType.toString) }

    member this.WithStageLength(sl: int<stageLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.stageLengthKey (sl |> Option.map UmxExt.intToRaw) }

    member this.WithCeLength(cl: int<ceLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.ceLengthKey (cl |> Option.map UmxExt.intToRaw) }

    member this.WithProjectName(pn: string<projectName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.projectNameKey (pn |> Option.map UmxExt.stringToRaw) }

    member this.WithId(id: Guid<idValue> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.idKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithRepl(repl: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.replKey (repl |> Option.map UmxExt.intToRaw) }

    member this.WithGeneration(gen: int<generationNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithRunFinished(fin: bool option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.runFinishedKey (fin |> Option.map string) }

    member this.WithMaxOrbit(mo: int option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.maxOrbitKey (mo |> Option.map string) }

    member this.WithSortableCount(sc: int<sortableCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortableCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSortableDataType(sdt: sortableDataType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortableDataTypeKey (sdt |> Option.map SortableDataType.toString) }

    member this.WithSorterCount(sc: int<sorterCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterModelType(smt: sorterModelType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterModelTypeKey (smt |> Option.map SorterModelType.toString) }

    member this.WithSortingWidth(w: int<sortingWidth> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortingWidthKey (w |> Option.map UmxExt.intToRaw) }

// --- 3. The Logic Module ---
module RunParameters =

    /// Filters a set of parameters based on a list of key-value pairs.
    let filterByParameters (runParametersSet: runParameters array) (filter: (string * string) array) =
        runParametersSet
        |> Array.filter (fun rp ->
            filter |> Array.forall (fun (k, v) -> rp.ParamMap.TryFind k = Some v))

    let pickByParameters (runParametersSet: runParameters array) (filter: (string * string) array) =
        let results = filterByParameters runParametersSet filter
        if results.Length = 1 then Some results.[0] else None

    let getAllKeys (runParams: runParameters seq) =
        runParams |> Seq.collect (fun rp -> rp.ParamMap.Keys) |> Seq.distinct |> Seq.toArray

    let reportKvps (runParameters: runParameters) : string =
        runParameters.ParamMap.Keys |> Seq.map(fun k -> sprintf "%s: %s" k (runParameters.ParamMap.[k])) |> String.concat "\n"

    /// Generates a structured table where rows represent individual runs and columns represent parameter keys.
    let makeIndexAndReplTable (runParams: runParameters seq) : string[][] =
        let keys = getAllKeys runParams
        let headerRow = Array.append [| "Run_Index" |] keys
        let dataRows =
            runParams
            |> Seq.mapi (fun i rp ->
                Array.append 
                    [| string i |] 
                    (keys |> Array.map (fun k -> rp.ParamMap.TryFind k |> Option.defaultValue "N/A")))
            |> Seq.toArray
        Array.append [| headerRow |] dataRows

    let toStringTable (runParams: runParameters seq) : string =
        makeIndexAndReplTable runParams
        |> Array.map (String.concat "\t")
        |> String.concat Environment.NewLine

    let getIdString (runParameters:runParameters) =
        runParameters.GetId() |> UmxExt.guidToString