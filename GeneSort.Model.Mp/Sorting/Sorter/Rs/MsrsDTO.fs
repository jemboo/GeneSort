
namespace GeneSort.Model.Mp.Sorter.Rs
open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorter.Rs
open MessagePack
open GeneSort.Core.Mp
open GeneSort.Model.Sorter

[<MessagePackObject; Struct>]
type msrsDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] Width: int
      [<Key(2)>] Perm_Rss: Perm_RsDto array }
    
    static member Create(id: Guid, width: int, permRss: Perm_RsDto array) : Result<msrsDto, string> =
        if isNull permRss then
            Error "Perm_Rss array cannot be null"
        else if permRss.Length < 1 then
            Error $"Must have at least 1 Perm_Rs, got {permRss.Length}"
        else if width < 1 then
            Error $"Width must be at least 1, got {width}"
        else if permRss |> Array.exists (fun prs -> prs.Perm_Si.Permutation.Array.Length <> width) then
            Error $"All Perm_Rs must have order equal to width {width}"
        else
            Ok { Id = id
                 Width = width
                 Perm_Rss = permRss }

module MsrsDto =

    type MsrsDtoError =
        | NullPermRssArray of string
        | EmptyPermRssArray of string
        | InvalidWidth of string
        | MismatchedPermRsOrder of string
        | PermRsConversionError of Perm_RsDto.Perm_RsDtoError

    let toMsrsDto (msrs: msrs) : msrsDto =
        { Id = %msrs.Id
          Width = %msrs.SortingWidth
          Perm_Rss = msrs.Perm_Rss |> Array.map Perm_RsDto.toPerm_RsDto }

    let toMsrs (dto: msrsDto) : Result<msrs, MsrsDtoError> =
        let permRssResult = 
            dto.Perm_Rss 
            |> Array.map Perm_RsDto.toPerm_Rs
            |> Array.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok permRs -> Ok (Array.append arr [|permRs|])
                | Ok _, Error e -> Error (PermRsConversionError e)
                | Error e, _ -> Error e
            ) (Ok [||])
        
        match permRssResult with
        | Error e -> Error e
        | Ok permRss ->
            try
                let msrs = msrs.create
                                (UMX.tag<sortingModelID> dto.Id)
                                (UMX.tag<sortingWidth> dto.Width)
                                permRss
                Ok msrs
            with
            | :? ArgumentException as ex when ex.Message.Contains("Perm_Rs") && ex.Message.Contains("at least 1") ->
                Error (EmptyPermRssArray ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("Width") ->
                Error (InvalidWidth ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("order") ->
                Error (MismatchedPermRsOrder ex.Message)
            | ex ->
                Error (InvalidWidth ex.Message) // Fallback for unexpected errors