namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1


type runParameters =
    private { paramMap : Map<string, string> }

    static member create (paramMap: Map<string, string>) : runParameters =
        { paramMap = paramMap }

    // Constants for consistency
    static member ceLengthKey = "CeLength"
    static member collectSortableTestsKey = "CollectSortableTests"
    static member databaseNameKey = "DatabaseName"
    static member elapsedTimeKey = "ElapsedTime"
    static member excludeSelfCeKey = "ExcludeSelfCe"
    static member generationKey = "Generation"
    static member idKey = "Id"
    static member latticeDistanceKey = "LatticeDistance"
    static member maxOrbitKey = "MaxOrbit"
    static member mergeDimensionKey = "MergeDimension"
    static member mergeSuffixTypeKey = "MergeSuffixType"
    static member messageKey = "Message"
    static member modificationRateKey = "ModificationRate"
    static member mutationRateKey = "MutationRate"
    static member insertionRateKey = "InsertionRate"
    static member deletionRateKey = "DeletionRate"
    static member projectNameKey = "ProjectName"
    static member queryNameKey = "QueryName"
    static member runNameKey = "RunName"
    static member replKey = "Repl"
    static member rngTypeKey = "RngType"
    static member startingReplKey = "StartingRepl"
    static member replSpanKey = "ReplSpan"
    static member runFinishedKey = "RunFinished"
    static member sorterEvalTypeKey = "SorterEvalType"
    static member sorterEvalSelectionType = "SorterEvalSelectionType"
    static member sorterEvalMeasureKey = "SorterEvalMeasure"
    static member sorterCountKey = "SorterCount"
    static member sorterParentCountKey = "SorterParentCount"
    static member sorterChildCountKey = "SorterChildCount"
    static member simpleSorterModelTypeKey = "SimpleSorterModelType"
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
    member this.GetCollectSortableTests() = runParameters.tryGetBool runParameters.collectSortableTestsKey this.paramMap
    member this.GetDatabaseName() = this.paramMap.TryFind runParameters.databaseNameKey |> Option.map UMX.tag<databaseName>
    member this.GetElapsedTime() = this.paramMap.TryFind runParameters.elapsedTimeKey
    member this.GetExcludeSelfCe() = runParameters.tryGetBool runParameters.excludeSelfCeKey this.paramMap |> Option.map UMX.tag<excludeSelfCe>
    member this.GetGeneration() = runParameters.tryGetInt runParameters.generationKey this.paramMap |> Option.map UMX.tag<generationNumber>
    member this.GetId() = runParameters.tryGetGuid runParameters.idKey this.paramMap |> Option.map UMX.tag<queryParamsId>
    member this.GetLatticeDistance() = runParameters.tryGetInt runParameters.latticeDistanceKey this.paramMap |> Option.map UMX.tag<latticeDistance>
    member this.GetMaxOrbit() = runParameters.tryGetInt runParameters.maxOrbitKey this.paramMap
    member this.GetMergeDimension() = runParameters.tryGetInt runParameters.mergeDimensionKey this.paramMap |> Option.map UMX.tag<mergeDimension>
    member this.GetMergeSuffixType() = 
        this.paramMap.TryFind runParameters.mergeSuffixTypeKey |> Option.bind (fun v -> try Some (MergeSuffixType.fromString v) with _ -> None)
    member this.GetModificationRate() = runParameters.tryGetFloat runParameters.modificationRateKey this.paramMap |> Option.map UMX.tag<modificationRate>
    member this.GetMutationRate() = runParameters.tryGetFloat runParameters.mutationRateKey this.paramMap |> Option.map UMX.tag<mutationRate>
    member this.GetInsertionRate() = runParameters.tryGetFloat runParameters.insertionRateKey this.paramMap |> Option.map UMX.tag<insertionRate>
    member this.GetDeletionRate() = runParameters.tryGetFloat runParameters.deletionRateKey this.paramMap |> Option.map UMX.tag<deletionRate>
    member this.GetProjectName() = this.paramMap.TryFind runParameters.projectNameKey |> Option.map UMX.tag<projectName>
    member this.GetQueryName() = this.paramMap.TryFind runParameters.queryNameKey |> Option.map UMX.tag<queryName>
    member this.GetRngType() = this.paramMap.TryFind runParameters.rngTypeKey |> Option.map RngType.fromString
    member this.GetRunName() = this.paramMap.TryFind runParameters.runNameKey |> Option.map UMX.tag<runName>
    member this.GetRepl() = runParameters.tryGetInt runParameters.replKey this.paramMap |> Option.map UMX.tag<replNumber>
    member this.GetStartingRepl() = runParameters.tryGetInt runParameters.startingReplKey this.paramMap |> Option.map UMX.tag<replNumber>
    member this.GetReplSpan() = runParameters.tryGetInt runParameters.replSpanKey this.paramMap |> Option.map UMX.tag<replNumber>
    member this.GetRunFinished() = runParameters.tryGetBool runParameters.runFinishedKey this.paramMap
    member this.GetSortingWidth() = runParameters.tryGetInt runParameters.sortingWidthKey this.paramMap |> Option.map UMX.tag<sortingWidth>
    member this.GetStageLength() = runParameters.tryGetInt runParameters.stageLengthKey this.paramMap |> Option.map UMX.tag<stageLength>
    member this.GetSortableCount() = runParameters.tryGetInt runParameters.sortableCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    member this.GetSorterCount() = runParameters.tryGetInt runParameters.sorterCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    member this.GetSorterEvalType() = this.paramMap.TryFind runParameters.sorterEvalTypeKey |> Option.map SorterEvalType.fromString
    member this.GetSorterEvalSelectionType() = this.paramMap.TryFind runParameters.sorterEvalSelectionType |> Option.map SorterEvalSelectionType.fromString
    member this.GetSorterEvalMeasure() = this.paramMap.TryFind runParameters.sorterEvalMeasureKey |> Option.map SorterEvalMeasure.fromString
    member this.GetSorterParentCount() = runParameters.tryGetInt runParameters.sorterParentCountKey this.paramMap |> Option.map UMX.tag<sorterCount>
    member this.GetSorterChildCount() = runParameters.tryGetInt runParameters.sorterChildCountKey this.paramMap |> Option.map UMX.tag<sorterCount>


    member this.GetTextReportName() = this.paramMap.TryFind runParameters.textReportNameKey |> Option.map UMX.tag<textReportName>

    member this.GetSimpleSorterModelType() = 
        this.paramMap.TryFind runParameters.simpleSorterModelTypeKey |> Option.bind (fun v -> try Some (SimpleSorterModelType.fromString v) with _ -> None)


    member this.GetSortableDataFormat() = 
        this.paramMap.TryFind runParameters.sortableDataFormatKey |> Option.bind (fun v -> try Some (SortableDataFormat.fromString v) with _ -> None)

// --- Functional Updates (Fluent API) ---

    member this.WithCeLength(cl: int<ceLength> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.ceLengthKey (cl |> Option.map UmxExt.intToRaw) }

    member this.WithCollectSortableTests(cst: bool option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.collectSortableTestsKey (cst |> Option.map string) }

    member this.WithDatabaseName(dbn: string<databaseName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.databaseNameKey (dbn |> Option.map UmxExt.stringToRaw) }

    member this.WithElapsedTime(time: string option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.elapsedTimeKey time }

    member this.WithExcludeSelfCe(esc: bool<excludeSelfCe> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.excludeSelfCeKey (esc |> Option.map UmxExt.boolToRaw) } 

    member this.WithGeneration(gen: int<generationNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.generationKey (gen |> Option.map UmxExt.intToRaw) }

    member this.WithId(id: Guid<queryParamsId> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.idKey (id |> Option.map UmxExt.guidToRaw) }

    member this.WithLatticeDistance(ld: int<latticeDistance> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.latticeDistanceKey (ld |> Option.map UmxExt.intToRaw) }

    member this.WithMergeDimension(md: int<mergeDimension> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeDimensionKey (md |> Option.map UmxExt.intToRaw) }

    member this.WithMergeSuffixType(mft: mergeSuffixType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mergeSuffixTypeKey (mft |> Option.map MergeSuffixType.toString) }

    member this.WithMessage(message: string option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.messageKey message }

    member this.WithModificationRate(mr: float<modificationRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.modificationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithMutationRate(mr: float<mutationRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.mutationRateKey (mr |> Option.map UmxExt.floatToRaw) }

    member this.WithProjectName(pn: string<projectName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.projectNameKey (pn |> Option.map UmxExt.stringToRaw) }

    member this.WithInsertionRate(ir: float<insertionRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.insertionRateKey (ir |> Option.map UmxExt.floatToRaw) }

    member this.WithDeletionRate(dr: float<deletionRate> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.deletionRateKey (dr |> Option.map UmxExt.floatToRaw) }    

    member this.WithRngType(rng: rngType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.rngTypeKey (rng |> Option.map RngType.toString) }

    member this.WithQueryName(qn: string<queryName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.queryNameKey (qn |> Option.map UmxExt.stringToRaw) }  

    member this.WithRunName(rn: string<runName> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.runNameKey (rn |> Option.map UmxExt.stringToRaw) }

    member this.WithRepl(repl: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.replKey (repl |> Option.map UmxExt.intToRaw) }

    member this.WithStartingRepl(sr: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.startingReplKey (sr |> Option.map UmxExt.intToRaw) }

    member this.WithReplSpan(rs: int<replNumber> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.replSpanKey (rs |> Option.map UmxExt.intToRaw) }  

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

    member this.WithSorterEvalType(set: sorterEvalType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterEvalTypeKey (set |> Option.map SorterEvalType.toString) }

    member this.WithSorterEvalSelectionType(ses: sorterEvalSelectionType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterEvalSelectionType (ses |> Option.map SorterEvalSelectionType.toString) }

    member this.WithSorterEvalMeasure(sem: sorterEvalMeasure option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterEvalMeasureKey (sem |> Option.map SorterEvalMeasure.toString) }

    member this.WithSorterParentCount(spc: int<sorterCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterParentCountKey (spc |> Option.map UmxExt.intToRaw) }

    member this.WithSorterChildCount(scc: int<sorterCount> option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.sorterChildCountKey (scc |> Option.map UmxExt.intToRaw) } 

    member this.WithSimpleSorterModelType(smt: simpleSorterModelType option) = 
        { paramMap = this.paramMap |> runParameters.addOrRemove runParameters.simpleSorterModelTypeKey (smt |> Option.map SimpleSorterModelType.toString) }

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
        runParameters.GetId() |> UmxExt.guidOptionToString