
namespace GeneSort.Model.Mp.Sorter.Uf4

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorter
open GeneSort.Core.Mp.TwoOrbitUnfolder
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter

[<MessagePackObject>]
type Msuf4DTO =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] SortingWidth: int
      [<Key(2)>] TwoOrbitUnfolder4s: TwoOrbitUf4DTO array }
    
    static member Create(id: Guid, sortingWidth: int, twoOrbitUnfolder4s: TwoOrbitUf4DTO array) : Result<Msuf4DTO, string> =
        if isNull twoOrbitUnfolder4s then
            Error "TwoOrbitUnfolder4s array cannot be null"
        else if twoOrbitUnfolder4s.Length < 1 then
            Error $"Must have at least 1 TwoOrbitUnfolder4, got {twoOrbitUnfolder4s.Length}"
        else if sortingWidth < 1 then
            Error $"SortingWidth must be at least 1, got {sortingWidth}"
        else
            try
                if twoOrbitUnfolder4s |> Array.exists (fun tou -> (tou |> TwoOrbitUf4DTO.getOrder) <> sortingWidth) then
                    Error $"All TwoOrbitUnfolder4 must have order {sortingWidth}"
                else
                    Ok { Id = id
                         SortingWidth = sortingWidth
                         TwoOrbitUnfolder4s = twoOrbitUnfolder4s }
            with
            | :? ArgumentException as ex ->
                Error ex.Message

module Msuf4DTO =
    type Msuf4DTOError =
        | NullTwoOrbitUnfolder4sArray of string
        | EmptyTwoOrbitUnfolder4sArray of string
        | InvalidSortingWidth of string
        | MismatchedTwoOrbitUnfolder4Order of string
        | TwoOrbitUnfolder4ConversionError of TwoOrbitUf4DTO.TwoOrbitUf4DTOError

    let toMsuf4DTO (msuf4: Msuf4) : Msuf4DTO =
        { Id = %msuf4.Id
          SortingWidth = %msuf4.SortingWidth
          TwoOrbitUnfolder4s = msuf4.TwoOrbitUnfolder4s |> Array.map TwoOrbitUf4DTO.toTwoOrbitUnfolder4DTO }

    let toMsuf4 (dto: Msuf4DTO)   = // : Result<Msuf4, Msuf4DTOError> =
        let twoOrbitUnfolder4sResult = 
            dto.TwoOrbitUnfolder4s 
            |> Array.map TwoOrbitUf4DTO.toTwoOrbitUnfolder4
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
                    Msuf4.create
                        (UMX.tag<sorterModelID> dto.Id)
                        (UMX.tag<sortingWidth> dto.SortingWidth)
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