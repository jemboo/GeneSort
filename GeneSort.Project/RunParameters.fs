namespace GeneSort.Project

open System
open GeneSort.Sorter
open GeneSort.Project.Params
open FSharp.UMX
open GeneSort.Sorter.Sortable

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
    static member stageLengthKey = "StageLength"
    static member ceLengthKey = "CeLength"


    member this.ParamMap with get() = this.paramMap

    member this.toString() =
        this.paramMap
        |> Map.toList
        |> List.map (fun (k, v) -> sprintf "%s: %s" k v)
        |> String.concat ", "

    /// Gets the Index value.
    member this.GetIndex() : int<indexNumber> =
        match this.paramMap.TryFind runParameters.indexKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<indexNumber>
            | false, _ -> failwith "Invalid Index value"
        | None -> failwith "Index parameter not found"
    /// Sets the Index value.
    member this.SetIndex(index: int<indexNumber>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.indexKey, (UMX.untag index).ToString())



    /// Gets the Repl value.
    member this.GetRepl() : int<replNumber> =
        match this.paramMap.TryFind runParameters.replKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<replNumber>
            | false, _ -> failwith "Invalid Repl value"
        | None -> failwith "Repl parameter not found"

    member this.GetReplKvp() : string*string =
        match this.paramMap.TryFind runParameters.replKey with
        | Some value -> 
            (runParameters.replKey, value)
        | None -> failwith "Repl parameter not found"

    /// Sets the Repl value.
    member this.SetRepl(repl: int<replNumber>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.replKey, (UMX.untag repl).ToString())



    /// Gets the Repl value.
    member this.GetGeneration() : int<generationNumber> =
        match this.paramMap.TryFind runParameters.generationKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<generationNumber>
            | false, _ -> failwith "Invalid Generation value"
        | None -> failwith "Repl parameter not found"

    /// Sets the Repl value.
    member this.SetGeneration(repl: int<generationNumber>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.generationKey, (UMX.untag repl).ToString())



    member this.IsRunFinished() : bool =
        match this.paramMap.TryFind runParameters.runFinishedKey with
        | Some value -> 
            match Boolean.TryParse(value) with
            | true, v -> v
            | false, _ -> failwith "Invalid RunFinished value"
        | None -> false

    member this.SetRunFinished(finished: bool) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.runFinishedKey, finished.ToString())



    /// Gets the SortableArrayType value.
    member this.GetSortableArrayType() : sortableArrayType =
        match this.paramMap.TryFind runParameters.sortableArrayTypeKey with
        | Some value -> SortableArrayType.fromString value
        | None -> failwith "SortableArrayType parameter not found"

    /// Sets the SortableArrayType value.
    member this.SetSortableArrayType(sortableArrayType: sortableArrayType) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sortableArrayTypeKey, SortableArrayType.toString sortableArrayType)



    /// Gets the SorterModelKey value.
    member this.GetSorterModelKey() : sorterModelKey =
        match this.paramMap.TryFind runParameters.sorterModelTypeKey with
        | Some value -> SorterModelKey.fromString value
        | None -> failwith "SorterModel parameter not found"

    member this.GetSorterModelKvp() : string*string =
        match this.paramMap.TryFind runParameters.sorterModelTypeKey with
        | Some value -> 
            (runParameters.sorterModelTypeKey, value)
        | None -> failwith "Repl parameter not found"

    /// Sets the SorterModelKey value.
    member this.SetSorterModelKey(sorterModelKey: sorterModelKey) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sorterModelTypeKey, SorterModelKey.toString sorterModelKey)



    /// Gets the SortingWidth value.
    member this.GetSortingWidth() : int<sortingWidth> =
        match this.paramMap.TryFind runParameters.sortingWidthKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<sortingWidth>
            | false, _ -> failwith "Invalid SortingWidth value"
        | None -> failwith "SortingWidth parameter not found"

    member this.GetSortingWidthKvp() : string*string =
        match this.paramMap.TryFind runParameters.sortingWidthKey with
        | Some value -> 
            (runParameters.sortingWidthKey, value)
        | None -> failwith "Repl parameter not found"

    /// Sets the SortingWidth value.
    member this.SetSortingWidth(sortingWidth: int<sortingWidth>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.sortingWidthKey, (UMX.untag sortingWidth).ToString())



    /// Gets the MaxOrbit value.
    member this.GetMaxOrbit() : int =
        match this.paramMap.TryFind runParameters.maxOrbitKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> v
            | false, _ -> failwith "Invalid MaxOrbit value"
        | None -> failwith "MaxOrbit parameter not found"

    /// Sets the MaxOrbit value.
    member this.SetMaxOrbit(maxOrbit: int) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.maxOrbitKey, maxOrbit.ToString())



    /// Gets the StageLength value.
    member this.GetStageLength() : int<stageLength> =
        match this.paramMap.TryFind runParameters.stageLengthKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<stageLength>
            | false, _ -> failwith "Invalid StageLength value"
        | None -> failwith "StageLength parameter not found"

    /// Sets the StageLength value.
    member this.SetStageLength(stageLength: int<stageLength>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.stageLengthKey, (UMX.untag stageLength).ToString())



    /// Gets the CeLength value.
    member this.GetCeLength() : int<ceLength> =
        match this.paramMap.TryFind runParameters.ceLengthKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<ceLength>
            | false, _ -> failwith "Invalid CeLength value"
        | None -> failwith "CeLength parameter not found"

    /// Sets the CeLength value.
    member this.SetCeLength(ceLength: int<ceLength>) : unit =
        this.paramMap <- this.paramMap.Add(runParameters.ceLengthKey, (UMX.untag ceLength).ToString())



    /// Sets the SorterCount value.
    member this.SetSorterCount(sorterCount: int<sorterCount>) : unit =
       this. paramMap <- this.paramMap.Add(runParameters.sorterCountKey, (UMX.untag sorterCount).ToString())

    /// Gets the SorterCount value.
    member this.GetSorterCount() : int<sorterCount> =
        match this.paramMap.TryFind runParameters.sorterCountKey with
        | Some value -> 
            match Int32.TryParse(value) with
            | true, v -> LanguagePrimitives.Int32WithMeasure v |> UMX.tag<sorterCount>
            | false, _ -> failwith "Invalid SorterCount value"
        | None -> failwith "SorterCount parameter not found"




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
            (filter: (string * string) array) : runParameters =
        let filtrate = filterByParameters runParametersSet filter
        if filtrate.Length = 1 then
            filtrate.[0]
        else
            failwithf "Expected exactly one runParameters to match filter, but found %d" filtrate.Length



        
