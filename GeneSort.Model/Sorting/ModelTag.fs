namespace GeneSort.Model.Sorting

open FSharp.UMX
open System


type splitJoin =
     | First_First
     | First_Second
     | Second_First
     | Second_Second

module SplitJoin =
    let toString (prefixOrSuffix: splitJoin) : string =
        match prefixOrSuffix with
        | First_First -> "First_First"
        | First_Second -> "First_Second"
        | Second_First -> "Second_First"
        | Second_Second -> "Second_Second"

    let fromString (str: string) : splitJoin =
        match str with
        | "First_First" -> First_First
        | "First_Second" -> First_Second
        | "Second_First" -> Second_First
        | "Second_Second" -> Second_Second
        | _ -> failwithf "Invalid prefixOrSuffix value: %s" str

// used to track how a sorter was made from a sortingModel
type modelTag =
     | Single
     | SplitPair of splitJoin


module ModelTag =
    let toString (modelTag: modelTag) : string =
        match modelTag with
        | Single -> "Single"
        | SplitPair sj -> sprintf "SplitPair(%s)" (SplitJoin.toString sj)

    let fromString (str: string) : modelTag =
        if str = "Single" then
            Single
        else if str.StartsWith("SplitPair(") && str.EndsWith(")") then
            let inner = str.Substring("SplitPair(".Length, str.Length - "SplitPair(".Length - 1)
            let splitJoin = SplitJoin.fromString inner
            SplitPair splitJoin
        else
            failwithf "Invalid modelTag format: %s" str

            
type sortingModelParentId = Guid<sortingModelId>

// used to track a sorter back to it's parent sortingModel, and it gives it's position 
// within it's family
type sortingModelTag = sortingModelParentId * modelTag

module SortingModelTag =

    let create (id: Guid) (tag: modelTag) : sortingModelTag =
        (id |> UMX.tag<sortingModelId>, tag)

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
            failwithf "Invalid sortingModelTag format: %s" str
        let modelId = Guid.Parse(parts.[0]) |> UMX.tag<sortingModelId>
        let tag = ModelTag.fromString parts.[1]
        (modelId, tag)