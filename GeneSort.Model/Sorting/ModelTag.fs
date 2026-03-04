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

// used to track how a sorter was made from a sorting
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

            
type sortingParentId = Guid<sortingId>

// used to track a sorter back to it's parent sorting, and it gives it's position 
// within it's family
type sortingTag = sortingParentId * modelTag

module SortingTag =

    let create (id: Guid<sortingId>) (tag: modelTag) : sortingTag =
        (id, tag)

    let getSortingParentId (sorterModelTag: sortingTag) : sortingParentId =
        let (modelId, _) = sorterModelTag
        modelId

    let getModelTag (sorterModelTag: sortingTag) : modelTag =
        let (_, tag) = sorterModelTag
        tag

    let toString (sorterModelTag: sortingTag) : string =
        let (modelId, tag) = sorterModelTag
        sprintf "%s\t%s" ((%modelId).ToString()) (ModelTag.toString tag)

    let fromString (str: string) : sortingTag =
        let parts = str.Split('\t')
        if parts.Length <> 2 then
            failwithf "Invalid sortingTag format: %s" str
        let modelId = Guid.Parse(parts.[0]) |> UMX.tag<sortingId>
        let tag = ModelTag.fromString parts.[1]
        (modelId, tag)


type sortingMutationSetTag = Guid<sortingMutationSegmentId> * sortingTag

module SortingMutationSetTag =
    let create (id: Guid<sortingMutationSegmentId>) (tag: sortingTag) : sortingMutationSetTag =
        (id, tag)
    let getMutationSegmentId (tag: sortingMutationSetTag) : Guid<sortingMutationSegmentId> =
        let (id, _) = tag
        id
    let getSortingTag (tag: sortingMutationSetTag) : sortingTag =
        let (_, sortingTag) = tag
        sortingTag
    let getSortingParentId (tag: sortingMutationSetTag) : sortingParentId =
        tag |> getSortingTag |> SortingTag.getSortingParentId
    let getModelTag (tag: sortingMutationSetTag) : modelTag =
        tag |> getSortingTag |> SortingTag.getModelTag
    let toString (tag: sortingMutationSetTag) : string =
        let (id, sortingTag) = tag
        sprintf "%s\t%s" ((%id).ToString()) (SortingTag.toString sortingTag)
    let fromString (str: string) : sortingMutationSetTag =
        let parts = str.Split('\t', 3)
        if parts.Length <> 3 then
            failwithf "Invalid sortingMutationSetTag format: %s" str
        let id = Guid.Parse(parts.[0]) |> UMX.tag<sortingMutationSegmentId>
        let sortingTag = SortingTag.fromString (sprintf "%s\t%s" parts.[1] parts.[2])
        (id, sortingTag)