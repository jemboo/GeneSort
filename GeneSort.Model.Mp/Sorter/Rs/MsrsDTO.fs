
namespace GeneSort.Model.Mp.Sorter.Rs
open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Model.Sorter.Rs
open MessagePack
open GeneSort.Core.Mp
open GeneSort.Model.Sorter

[<MessagePackObject; Struct>]
type MsrsDTO =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] Width: int
      [<Key(2)>] Perm_Rss: Perm_RsDTO array }
    
    static member Create(id: Guid, width: int, permRss: Perm_RsDTO array) : Result<MsrsDTO, string> =
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

module MsrsDTO =

    type MsrsDTOError =
        | NullPermRssArray of string
        | EmptyPermRssArray of string
        | InvalidWidth of string
        | MismatchedPermRsOrder of string
        | PermRsConversionError of Perm_RsDTO.Perm_RsDTOError

    let toMsrsDTO (msrs: Msrs) : MsrsDTO =
        { Id = %msrs.Id
          Width = %msrs.SortingWidth
          Perm_Rss = msrs.Perm_Rss |> Array.map Perm_RsDTO.toPerm_RsDTO }

    let toMsrs (dto: MsrsDTO) : Result<Msrs, MsrsDTOError> =
        let permRssResult = 
            dto.Perm_Rss 
            |> Array.map Perm_RsDTO.toPerm_Rs
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
                let msrs = Msrs.create
                                (UMX.tag<sorterModelID> dto.Id)
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