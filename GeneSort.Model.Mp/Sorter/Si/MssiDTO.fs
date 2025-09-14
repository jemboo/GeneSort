namespace GeneSort.Model.Mp.Sorter.Si

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Si
open MessagePack
open GeneSort.Core.Mp

[<MessagePackObject; Struct>]
type mssiDto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] sortingWidth: int
      [<Key(2)>] permSiDtos: permSiDto array }

module MssiDto =
    type MssiDtoError =
        | InvalidPermSiCount of string
        | InvalidWidth of string

    let toMssiDto (mssi: Mssi) : mssiDto =
        let mssiDtos = mssi.Perm_Sis |> Array.map PermSiDto.fromDomain
        { id = %mssi.Id
          sortingWidth = %mssi.SortingWidth
          permSiDtos = mssiDtos }

    let toMssi (mssiDto: mssiDto) : Result<Mssi, MssiDtoError> =
        try
            let perm_SisR = mssiDto.permSiDtos |> Array.map (PermSiDto.toDomain)
            let perm_Sis =
                perm_SisR 
                |> Array.map (function
                    | Ok ps -> ps
                    | Error e -> raise (ArgumentException(sprintf "Error converting Perm_SiDto: %A" e)))
            let mssi = GeneSort.Model.Sorter.Si.Mssi.create
                            (UMX.tag<sorterModelID> mssiDto.id)
                            (UMX.tag<sortingWidth> mssiDto.sortingWidth)
                            perm_Sis
            Ok mssi
        with
        | :? ArgumentException as ex when ex.Message.Contains("Perm_Si") ->
            Error (InvalidPermSiCount ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("Width") ->
            Error (InvalidWidth ex.Message)
        | ex ->
            Error (InvalidPermSiCount ex.Message) // Fallback for unexpected errors