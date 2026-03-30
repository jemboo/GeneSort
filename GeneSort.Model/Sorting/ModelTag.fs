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



// used to relate a sorter with the sorting that generates it.
type modelTag =
     | Single
     | SplitPair of splitJoin


module ModelTag =

    let allSingleTags : modelTag [] = [| 
                modelTag.Single 
            |]

    let allSplitJoinTags : modelTag [] = [| 
            modelTag.SplitPair splitJoin.First_First; 
            modelTag.SplitPair  splitJoin.First_Second; 
            modelTag.SplitPair  splitJoin.Second_First; 
            modelTag.SplitPair  splitJoin.Second_Second
            |]

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

           


// used to relate a sorter with the item in a collection of sortings that generates it.
[<Struct; StructuralEquality; NoComparison>]
type modelSetTag =
    private {
        sortingId: Guid<sortingId>
        modelTag:  modelTag
    }

    static member create (id: Guid<sortingId>) (tag: modelTag) =
        { sortingId = id; modelTag = tag }

    member this.SortingId with get() = this.sortingId
    member this.ModelTag  with get() = this.modelTag


module ModelSetTag =

    let create (id: Guid<sortingId>) (tag: modelTag) : modelSetTag =
        modelSetTag.create id tag

    let getSortingParentId (t: modelSetTag) : Guid<sortingId> =
        t.SortingId

    let getModelTag (t: modelSetTag) : modelTag =
        t.ModelTag

    let toString (t: modelSetTag) : string =
        sprintf "%s\t%s" ((%t.SortingId).ToString()) (ModelTag.toString t.ModelTag)

    let fromString (str: string) : modelSetTag =
        let parts = str.Split('\t')
        if parts.Length <> 2 then
            failwithf "Invalid modelSetTag format: %s" str
        let id  = Guid.Parse(parts.[0]) |> UMX.tag<sortingId>
        let tag = ModelTag.fromString parts.[1]
        modelSetTag.create id tag


type sortingMutationSetTag = Guid<sortingMutationSegmentId> * modelSetTag

module SortingMutationSetTag =
    let create (id: Guid<sortingMutationSegmentId>) (tag: modelSetTag) : sortingMutationSetTag =
        (id, tag)
    let getMutationSegmentId (tag: sortingMutationSetTag) : Guid<sortingMutationSegmentId> =
        let (id, _) = tag
        id
    let getSortingTag (tag: sortingMutationSetTag) : modelSetTag =
        let (_, sortingTag) = tag
        sortingTag
    let getSortingParentId (tag: sortingMutationSetTag) : Guid<sortingId> =
        tag |> getSortingTag |> ModelSetTag.getSortingParentId
    let getModelTag (tag: sortingMutationSetTag) : modelTag =
        tag |> getSortingTag |> ModelSetTag.getModelTag
    let toString (tag: sortingMutationSetTag) : string =
        let (id, sortingTag) = tag
        sprintf "%s\t%s" ((%id).ToString()) (ModelSetTag.toString sortingTag)
    let fromString (str: string) : sortingMutationSetTag =
        let parts = str.Split('\t', 3)
        if parts.Length <> 3 then
            failwithf "Invalid sortingMutationSetTag format: %s" str
        let id = Guid.Parse(parts.[0]) |> UMX.tag<sortingMutationSegmentId>
        let sortingTag = ModelSetTag.fromString (sprintf "%s\t%s" parts.[1] parts.[2])
        (id, sortingTag)