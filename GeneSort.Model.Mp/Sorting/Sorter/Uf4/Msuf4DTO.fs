
namespace GeneSort.Model.Mp.Sorting.Sorter.Uf4

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.Core.Mp.TwoOrbitUnfolder
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting

[<MessagePackObject>]
type msuf4Dto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] sortingWidth: int
      [<Key(2)>] twoOrbitUf4Dtos: TwoOrbitUf4Dto array }
    
    static member Create(id: Guid, sortingWidth: int, twoOrbitUnfolder4s: TwoOrbitUf4Dto array) : Result<msuf4Dto, string> =
        if isNull twoOrbitUnfolder4s then
            Error "TwoOrbitUnfolder4s array cannot be null"
        else if twoOrbitUnfolder4s.Length < 1 then
            Error $"Must have at least 1 TwoOrbitUnfolder4, got {twoOrbitUnfolder4s.Length}"
        else if sortingWidth < 1 then
            Error $"SortingWidth must be at least 1, got {sortingWidth}"
        else
            try
                if twoOrbitUnfolder4s |> Array.exists (fun tou -> (tou |> TwoOrbitUf4Dto.getOrder) <> sortingWidth) then
                    Error $"All TwoOrbitUnfolder4 must have order {sortingWidth}"
                else
                    Ok { id = id
                         sortingWidth = sortingWidth
                         twoOrbitUf4Dtos = twoOrbitUnfolder4s }
            with
            | :? ArgumentException as ex ->
                Error ex.Message

module Msuf4Dto =
    type Msuf4DtoError =
        | NullTwoOrbitUnfolder4sArray of string
        | EmptyTwoOrbitUnfolder4sArray of string
        | InvalidSortingWidth of string
        | MismatchedTwoOrbitUnfolder4Order of string
        | TwoOrbitUnfolder4ConversionError of TwoOrbitUf4Dto.TwoOrbitUf4DtoError

    let toMsuf4Dto (msuf4: msuf4) : msuf4Dto =
        { id = %msuf4.Id
          sortingWidth = %msuf4.SortingWidth
          twoOrbitUf4Dtos = msuf4.TwoOrbitUnfolder4s |> Array.map TwoOrbitUf4Dto.fromDomain }

    let fromMsuf4Dto (dto: msuf4Dto)   = // : Result<Msuf4, Msuf4DtoError> =
        let twoOrbitUnfolder4sResult = 
            dto.twoOrbitUf4Dtos 
            |> Array.map TwoOrbitUf4Dto.toDomain
            |> Array.fold (fun acc res ->
                match acc, res with
                | Ok arr, Ok tou -> Ok (Array.append arr [|tou|])
                | Ok _, Error e -> Error (TwoOrbitUnfolder4ConversionError e)
                | Error e, _ -> Error e
            ) (Ok [||])
        match twoOrbitUnfolder4sResult with
        | Error e -> Error e
        | Ok twoOrbitUnfolder4s ->
            try
                let msuf4 = 
                    msuf4.create
                        (UMX.tag<sorterModelID> dto.id)
                        (UMX.tag<sortingWidth> dto.sortingWidth)
                        twoOrbitUnfolder4s
                Ok msuf4
            with
            | :? ArgumentException as ex when ex.Message.Contains("TwoOrbitUnfolder4") && ex.Message.Contains("at least 1") ->
                Error (EmptyTwoOrbitUnfolder4sArray ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("SortingWidth") ->
                Error (InvalidSortingWidth ex.Message)
            | :? ArgumentException as ex when ex.Message.Contains("order") ->
                Error (MismatchedTwoOrbitUnfolder4Order ex.Message)
            | ex ->
                Error (InvalidSortingWidth ex.Message)