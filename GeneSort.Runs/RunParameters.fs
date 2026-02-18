namespace GeneSort.Runs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

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
    static member ceLengthKey = "CeLength"
    static member elapsedTimeKey = "ElapsedTime"
    static member generationKey = "Generation"
    static member idKey = "Id"
    static member latticeDistanceKey = "LatticeDistance"
    static member maxOrbitKey = "MaxOrbit"
    static member mergeDimensionKey = "MergeDimension"
    static member mergeSuffixTypeKey = "MergeSuffixType"
    static member messageKey = "Message"
    static member mutationRateKey = "MutationRate"
    static member projectNameKey = "ProjectName"
    static member replKey = "Repl"
    static member runFinishedKey = "RunFinished"
    static member sorterCountKey = "SorterCount"
    static member sorterModelTypeKey = "SorterModelType"
    static member sortingWidthKey = "SortingWidth"
    static member sortableCountKey = "SortableCount"
    static member sortableDataFormatKey = "SortableDataFormat"
    static member stageLengthKey = "StageLength"
    static member textReportNameKey = "TextReportName"


    // Private Parsing Helpers
    static member private tryGetGuid (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Guid.TryParse v with true, g -> Some g | _ -> None)

    static member private tryGetInt (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Int32.TryParse v with true, i -> Some i | _ -> None)
    
    static member private tryGetBool (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Boolean.TryParse v with true, b -> Some b | _ -> None)

    static member private tryGetFloat (key: string) (map: Map<string, string>) =
        map.TryFind key |> Option.bind (fun v -> match Double.TryParse v with true, f -> Some f | _ -> None)

    static member private addOrRemove key valueOpt map =
        match valueOpt with
        | Some v -> Map.add key v map
        | None   -> Map.remove key map

    member this.ParamMap with get() = this.paramMap

    // --- Getters ---
    member this.GetCeLength() = runParameters.tryGetInt runParameters.ceLengthKey this.paramMap |> Option.map UMX.tag<ceLength>
    member this.GetGeneration() = runParameters.tryGetInt runParameters.generationKey this.paramMap |> Option.map UMX.tag<generationNumber>
    member this.GetId() = runParameters.tryGetGuid runParameters.idKey this.paramMap |> Option.map UMX.tag<idValue>
    member this.GetLatticeDistance() = runParameters.tryGetInt runParameters.latticeDistanceKey this.paramMap |> Option.map UMX.tag<latticeDistance>
    member this.GetMaxOrbit() = runParameters.tryGetInt runParameters.maxOrbitKey this.paramMap
    member this.GetMergeDimension() = runParameters.tryGetInt runParameters.mergeDimensionKey this.paramMap |> Option.map UMX.tag<mergeDimension>
    
    member this.GetMergeSuffixType() = 
        this.paramMap.TryFind runParameters.mergeSuffixTypeKey |> Option.bind (fun v -> try Some (MergeFillType.fromString v) with _ -> None)

    member this.GetMutationRate() = runParameters.tryGetFloat runParameters.mutationRateKey this.paramMap |> Option.map UMX.tag<mutationRate>
    member this.GetProjectName() = this.paramMap.TryFind runParameters.projectNameKey |> Option.map UMX.tag<projectName>
    member this.GetRepl() = runParameters.tryGetInt runParameters.replKey this.paramMap |> Option.map UMX.tag<replNumber>
    member this.GetRunFinished() = runParameters.tryGetBool runParameters.runFinishedKey this.paramMap
    member this.GetSortingWidth() = runParameters.tryGetInt runParameters.sortingWidthKey this.paramMap |> Option.map UMX.tag<sortingWidth>
    member this.GetStageLength() = runParameters.tryGetInt runParameters.stageLengthKey this.paramMap |> Option.map UMX.tag<stageLength>
    member this.GetSortableCount() = runParameters.tryGetInt runParameters.sortableCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    member this.GetSorterCount() = runParameters.tryGetInt runParameters.sorterCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    
    member this.GetTextReportName() = this.paramMap.TryFind runParameters.textReportNameKey |> Option.map UMX.tag<textReportName>

    member this.GetSorterModelType() = 
        this.paramMap.TryFind runParameters.sorterModelTypeKey |> Option.bind (fun v -> try Some (SorterModelType.fromString v) with _ -> None)


    member this.GetSortableDataFormat() = 
        this.paramMap.TryFind runParameters.sortableDataFormatKey |> Option.bind (fun v -> try Some (SortableDataFormat.fromString v) with _ -> None)

// --- Functional Updates (Fluent API) ---

    member this.WithCeLength(cl: int<ceLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.ceLengthKey (cl |> Option.map UmxExt.intToRaw) }

    member this.WithElapsedTime(time: string option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.elapsedTimeKey time }

    member this.WithGeneration(gen: int<generationNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithId(id: Guid<idValue> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.idKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithLatticeDistance(ld: int<latticeDistance> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.latticeDistanceKey (ld |> Option.map UmxExt.intToRaw) }

    member this.WithMergeDimension(md: int<mergeDimension> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeDimensionKey (md |> Option.map UmxExt.intToRaw) }

    member this.WithMergeSuffixType(mft: mergeSuffixType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeSuffixTypeKey (mft |> Option.map MergeFillType.toString) }

    member this.WithMessage(message: string option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.messageKey message }

    member this.WithMutationRate(mr: float<mutationRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mutationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithProjectName(pn: string<projectName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.projectNameKey (pn |> Option.map UmxExt.stringToRaw) }

    member this.WithRepl(repl: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.replKey (repl |> Option.map UmxExt.intToRaw) }

    member this.WithRunFinished(fin: bool option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.runFinishedKey (fin |> Option.map string) }

    member this.WithMaxOrbit(mo: int option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.maxOrbitKey (mo |> Option.map string) }

    member this.WithSortableCount(sc: int<sortableCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortableCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSortableDataFormat(sdt: sortableDataFormat option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortableDataFormatKey (sdt |> Option.map SortableDataFormat.toString) }

    member this.WithSorterCount(sc: int<sorterCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterCountKey (sc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterModelType(smt: sorterModelType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterModelTypeKey (smt |> Option.map SorterModelType.toString) }

    member this.WithSortingWidth(w: int<sortingWidth> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sortingWidthKey (w |> Option.map UmxExt.intToRaw) }

    member this.WithStageLength(sl: int<stageLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.stageLengthKey (sl |> Option.map UmxExt.intToRaw) }


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