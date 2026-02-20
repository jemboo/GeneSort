namespace GeneSort.Model.Mp.Sorting

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open MessagePack

[<MessagePackObject>]
type prefixOrSuffixDto =
    | Prefix = 0
    | Suffix = 1

module PrefixOrSuffixDto =
    let toDto (pos: prefixOrSuffix) : prefixOrSuffixDto =
        match pos with
        | Prefix -> prefixOrSuffixDto.Prefix
        | Suffix -> prefixOrSuffixDto.Suffix
    
    let fromDto (dto: prefixOrSuffixDto) : prefixOrSuffix =
        match dto with
        | prefixOrSuffixDto.Prefix -> Prefix
        | prefixOrSuffixDto.Suffix -> Suffix
        | _ -> failwith "Invalid prefixOrSuffixDto value"

[<MessagePackObject>]
type modelTagDto =
    { [<Key(0)>] Tag: string
      [<Key(1)>] Prefix: prefixOrSuffixDto option
      [<Key(2)>] Suffix: prefixOrSuffixDto option }

module ModelTagDto =
    let toDto (tag: modelTag) : modelTagDto =
        match tag with
        | Single -> 
            { Tag = "Single"; Prefix = None; Suffix = None }
        | SplitPair (p, s) -> 
            { Tag = "SplitPair"
              Prefix = Some (PrefixOrSuffixDto.toDto p)
              Suffix = Some (PrefixOrSuffixDto.toDto s) }
    
    let fromDto (dto: modelTagDto) : modelTag =
        match dto.Tag with
        | "Single" -> Single
        | "SplitPair" ->
            match dto.Prefix, dto.Suffix with
            | Some p, Some s -> 
                SplitPair (PrefixOrSuffixDto.fromDto p, PrefixOrSuffixDto.fromDto s)
            | _ -> failwith "SplitPair requires both Prefix and Suffix"
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
        let parentId = Guid.Parse(dto.ParentId) |> UMX.tag<sortingModelID>
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
        let grandParentId = Guid.Parse(dto.GrandParentId) |> UMX.tag<sortingModelID>
        let parentTag = SortingModelTagDto.fromDto dto.ParentTag
        (grandParentId, parentTag)