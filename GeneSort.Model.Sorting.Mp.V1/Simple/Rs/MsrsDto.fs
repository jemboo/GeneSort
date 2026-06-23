namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Rs

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Core.Mp
open GeneSort.Model.Sorting.V1

[<MessagePackObject; Struct>]
type msrsDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] Width: int
      [<Key(2)>] Perm_Rss: permRsDto array }
    
    static member Create(id: Guid, width: int, permRss: permRsDto array) : msrsDto =
        if isNull permRss then
            raise (ArgumentNullException(nameof permRss, "Perm_Rss array cannot be null"))
        elif permRss.Length < 1 then
            raise (ArgumentException($"Must have at least 1 Perm_Rs, got {permRss.Length}", nameof permRss))
        elif width < 1 then
            raise (ArgumentException($"Width must be at least 1, got {width}", nameof width))
        elif permRss |> Array.exists (fun prs -> prs.permSiDto.permutationDto.intArray.Length <> width) then
            raise (ArgumentException($"All Perm_Rs must have order equal to width {width}", nameof permRss))
        else
            { Id = id
              Width = width
              Perm_Rss = permRss }

module MsrsDto =

    let fromDomain (msrs: msrs) : msrsDto =
        { Id = %msrs.Id
          Width = %msrs.SortingWidth
          Perm_Rss = msrs.Perm_Rss |> Array.map PermRsDto.toPerm_RsDto }

    let toDomain (dto: msrsDto) : msrs =
        let permRss = 
            dto.Perm_Rss 
            |> Array.map PermRsDto.toPerm_Rs
        
        msrs.create
            (UMX.tag<sorterModelId> dto.Id)
            (UMX.tag<sortingWidth> dto.Width)
            permRss