
namespace GeneSort.Model.Mp.Sorting.Sorter.Uf6

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.Core.Mp.TwoOrbitUnfolder
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Model.Sorting

[<MessagePackObject>]
type msuf6Dto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] sortingWidth: int
      [<Key(2)>] twoOrbitUf6Dtos: TwoOrbitUf6Dto array }
    
    static member Create(id: Guid, sortingWidth: int, twoOrbitUnfolder6s: TwoOrbitUf6Dto array) : Result<msuf6Dto, string> =
        if isNull twoOrbitUnfolder6s then
            Error "TwoOrbitUnfolder6s array cannot be null"
        else if twoOrbitUnfolder6s.Length < 1 then
            Error $"Must have at least 1 TwoOrbitUnfolder6, got {twoOrbitUnfolder6s.Length}"
        else if sortingWidth < 1 then
            Error $"SortingWidth must be at least 1, got {sortingWidth}"
        else
            try
                if twoOrbitUnfolder6s |> Array.exists (fun tou -> (tou |> TwoOrbitUf6Dto.getOrder) <> sortingWidth) then
                    Error $"All TwoOrbitUnfolder6 must have order {sortingWidth}"
                else
                    Ok { id = id
                         sortingWidth = sortingWidth
                         twoOrbitUf6Dtos = twoOrbitUnfolder6s }
            with
            | :? ArgumentException as ex ->
                Error ex.Message

module Msuf6Dto =
    type Msuf6DtoError =
        | NullTwoOrbitUnfolder6sArray of string
        | EmptyTwoOrbitUnfolder6sArray of string
        | InvalidSortingWidth of string
        | MismatchedTwoOrbitUnfolder6Order of string
        | TwoOrbitUnfolder6ConversionError of TwoOrbitUf6Dto.TwoOrbitUf6DtoError

    let fromDomain (msuf6: msuf6) : msuf6Dto =
        { id = %msuf6.Id
          sortingWidth = %msuf6.SortingWidth
          twoOrbitUf6Dtos = msuf6.TwoOrbitUnfolder6s |> Array.map TwoOrbitUf6Dto.fromDomain }

    let toDomain (dto: msuf6Dto) : Result<msuf6, Msuf6DtoError> =
        let twoOrbitUnfolder6sResult = 
            dto.twoOrbitUf6Dtos 
            |> Array.map TwoOrbitUf6Dto.toDomain
            |> Array.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok tou -> Ok (Array.append arr [|tou|])
                | Ok _, Error e -> Error (TwoOrbitUnfolder6ConversionError e)
                | Error e, _ -> Error e
            ) (Ok [||])
        match twoOrbitUnfolder6sResult with
        | Error e -> Error e
        | Ok twoOrbitUnfolder6s ->
            try
                let msuf6 = 
                    msuf6.create
                        (UMX.tag<sorterModelId> dto.id)
                        (UMX.tag<sortingWidth> dto.sortingWidth)
                        twoOrbitUnfolder6s
                Ok msuf6
            with
            | :? ArgumentException as ex when ex.Message.Contains("TwoOrbitUnfolder6") && ex.Message.Contains("at least 1") ->
                Error (EmptyTwoOrbitUnfolder6sArray ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("SortingWidth") ->
                Error (InvalidSortingWidth ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("order") ->
                Error (MismatchedTwoOrbitUnfolder6Order ex.Message)
            | ex ->
                Error (InvalidSortingWidth ex.Message)