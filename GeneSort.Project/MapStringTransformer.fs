namespace GeneSort.Project
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

open System
open System.Collections.Generic

type MapStringTransformer(transforms: (string * (string -> (string * obj) array)) list) =
    let transformMap = transforms |> Map.ofList

    member _.TransformMap = transformMap

    static member ToStringObjMap (input: Map<string, string>) (transformer: MapStringTransformer) : Map<string, obj> =
        input |> Map.fold (fun acc key value ->
            match transformer.TransformMap.TryFind key with
            | None -> 
                raise (KeyNotFoundException(sprintf "No transformation function found for key '%s'" key))
            | Some transformFunc ->
                let keyValuePairs = transformFunc value
                keyValuePairs |> Array.fold (fun accMap (newKey, newValue) -> accMap.Add(newKey, newValue)) acc
        ) Map.empty
