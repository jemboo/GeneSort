namespace GeneSort.Model.Mp.Sorting

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open MessagePack

[<MessagePackObject>]
type splitJoinDto =
    | First_First = 0
    | First_Second = 1
    | Second_First = 2
    | Second_Second = 3

module SplitJoinDto =
    let toDto (sj: splitJoin) : splitJoinDto =
        match sj with
        | First_First -> splitJoinDto.First_First
        | First_Second -> splitJoinDto.First_Second
        | Second_First -> splitJoinDto.Second_First
        | Second_Second -> splitJoinDto.Second_Second
    
    let fromDto (dto: splitJoinDto) : splitJoin =
        match dto with
        | splitJoinDto.First_First -> First_First
        | splitJoinDto.First_Second -> First_Second
        | splitJoinDto.Second_First -> Second_First
        | splitJoinDto.Second_Second -> Second_Second
        | _ -> failwith "Invalid splitJoinDto value"

[<MessagePackObject>]
type modelTagDto =
    { [<Key(0)>] Tag: string
      [<Key(1)>] SplitJoin: splitJoinDto option }

module ModelTagDto =
    let toDto (tag: modelTag) : modelTagDto =
        match tag with
        | Single -> 
            { Tag = "Single"; SplitJoin = None }
        | SplitPair sj -> 
            { Tag = "SplitPair"
              SplitJoin = Some (SplitJoinDto.toDto sj) }
    
    let fromDto (dto: modelTagDto) : modelTag =
        match dto.Tag with
        | "Single" -> Single
        | "SplitPair" ->
            match dto.SplitJoin with
            | Some sj -> SplitPair (SplitJoinDto.fromDto sj)
            | None -> failwith "SplitPair requires a SplitJoin value"
        | _ -> failwithf "Invalid modelTag: %s" dto.Tag


[<MessagePackObject>]
type sortingModelTagDto =
    { [<Key(0)>] ParentId: string
      [<Key(1)>] ModelTag: modelTagDto }

module SortingModelTagDto =
    let toDto (tag: sortingModelTag) : sortingModelTagDto =
        let (parentId, modelTag) = tag
        { ParentId = (%parentId).ToString()
          ModelTag = ModelTagDto.toDto modelTag }
    
    let fromDto (dto: sortingModelTagDto) : sortingModelTag =
        let parentId = Guid.Parse(dto.ParentId) |> UMX.tag<sortingModelId>
        let modelTag = ModelTagDto.fromDto dto.ModelTag
        (parentId, modelTag)

[<MessagePackObject>]
type parentSortingModelTagDto =
    { [<Key(0)>] GrandParentId: string
      [<Key(1)>] ParentTag: sortingModelTagDto }

module ParentSortingModelTagDto =
    let toDto (tag: parentSortingModelTag) : parentSortingModelTagDto =
        let (grandParentId, parentTag) = tag
        { GrandParentId = (%grandParentId).ToString()
          ParentTag = SortingModelTagDto.toDto parentTag }
    
    let fromDto (dto: parentSortingModelTagDto) : parentSortingModelTag =
        let grandParentId = Guid.Parse(dto.GrandParentId) |> UMX.tag<sortingModelId>
        let parentTag = SortingModelTagDto.fromDto dto.ParentTag
        (grandParentId, parentTag)