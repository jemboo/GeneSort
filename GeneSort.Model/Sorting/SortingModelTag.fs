namespace GeneSort.Model.Sorting

open FSharp.UMX
open System


type prefixOrSuffix =
     | Prefix
     | Suffix


module PrefixOrSuffix =
    let toString (prefixOrSuffix: prefixOrSuffix) : string =
        match prefixOrSuffix with
        | Prefix -> "Prefix"
        | Suffix -> "Suffix"

    let fromString (str: string) : prefixOrSuffix =
        match str with
        | "Prefix" -> Prefix
        | "Suffix" -> Suffix
        | _ -> failwithf "Invalid prefixOrSuffix value: %s" str

// used to track how a sorter was made from a sortingModel
type modelTag =
     | Single
     | SplitPair of prefixOrSuffix*prefixOrSuffix


module ModelTag =
    let toString (modelTag: modelTag) : string =
        match modelTag with
        | Single -> "Single"
        | SplitPair (p, s) -> sprintf "SplitPair(%s, %s)" (PrefixOrSuffix.toString p) (PrefixOrSuffix.toString s)

    let fromString (str: string) : modelTag =
        if str = "Single" then
            Single
        else if str.StartsWith("SplitPair") then
            let inner = str.Substring("SplitPair(".Length, str.Length - "SplitPair(".Length - 1)
            let parts = inner.Split(',')
            if parts.Length <> 2 then
                failwithf "Invalid SplitPair format: %s" str
            let p = PrefixOrSuffix.fromString (parts.[0].Trim())
            let s = PrefixOrSuffix.fromString (parts.[1].Trim())
            SplitPair (p, s)
        else
            failwithf "Invalid modelTag format: %s" str

            
type sortingModelParentId = Guid<sortingModelID>

// used to track a sorter back to it's parent sortingModel, and it gives it's position 
// within it's family
type sortingModelTag = sortingModelParentId * modelTag

module SortingModelTag =

    let create (id: Guid) (tag: modelTag) : sortingModelTag =
        (id |> UMX.tag<sortingModelID>, tag)

    let getSortingModelParentId (sorterModelTag: sortingModelTag) : sortingModelParentId =
        let (modelId, _) = sorterModelTag
        modelId

    let getSortingModelTag (sorterModelTag: sortingModelTag) : modelTag =
        let (_, tag) = sorterModelTag
        tag

    let toString (sorterModelTag: sortingModelTag) : string =
        let (modelId, tag) = sorterModelTag
        sprintf "%s\t%s" ((%modelId).ToString()) (ModelTag.toString tag)

    let fromString (str: string) : sortingModelTag =
        let parts = str.Split('\t')
        if parts.Length <> 2 then
            failwithf "Invalid sorterModelTag format: %s" str
        let modelId = Guid.Parse(parts.[0])
        let tag = ModelTag.fromString parts.[1]
        (modelId |> UMX.tag<sortingModelID>, tag)

