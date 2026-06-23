namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Si
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

    let fromDomain (mssi: mssi) : mssiDto =
        let mssiDtos = mssi.Perm_Sis |> Array.map PermSiDto.fromDomain
        { id = %mssi.Id
          sortingWidth = %mssi.SortingWidth
          permSiDtos = mssiDtos }

    let toDomain (mssiDto: mssiDto) : Result<mssi, MssiDtoError> =
        try
            let perm_SisR = mssiDto.permSiDtos |> Array.map (PermSiDto.toDomain)
            let perm_Sis =
                perm_SisR 
                |> Array.map (function
                    | Ok ps -> ps
                    | Error e -> raise (ArgumentException(sprintf "Error converting Perm_SiDto: %A" e)))
            let mssi = mssi.create
                            (UMX.tag<sorterModelId> mssiDto.id)
                            (UMX.tag<sortingWidth> mssiDto.sortingWidth)
                            perm_Sis
            Ok mssi
        with
        | :? ArgumentException as ex when ex.Message.Contains("Perm_Si") ->
            Error (InvalidPermSiCount ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("Width") ->
            Error (InvalidWidth ex.Message)
        | ex ->
            Error (InvalidPermSiCount ex.Message)