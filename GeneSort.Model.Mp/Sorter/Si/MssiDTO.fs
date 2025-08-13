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
type MssiDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] Width: int
      [<Key(2)>] Perm_SiDtos: Perm_SiDto array }
    
    //static member Create(id: Guid, width: int, perm_Sis: Perm_Si array) : Result<MssiDto, string> =
    //    if isNull perm_Sis || perm_Sis.Length < 1 then
    //        Error "Must have at least 1 Perm_Si"
    //    else if width < 1 then
    //        Error "Width must be at least 1"
    //    else
    //        Ok { Id = id; Width = width; Perm_SiDtos = perm_Sis }
    
    //member this.StageCount = this.Perm_Sis.Length

module MssiDto =
    type MssiDtoError =
        | InvalidPermSiCount of string
        | InvalidWidth of string

    let toMssiDto (mssi: Mssi) : MssiDto =
        let mssiDtos = mssi.Perm_Sis |> Array.map Perm_SiDto.toPerm_SiDto
        { Id = %mssi.Id
          Width = %mssi.SortingWidth
          Perm_SiDtos = mssiDtos }

    let toMssi (mssiDto: MssiDto) : Result<Mssi, MssiDtoError> =
        try
            let perm_SisR = mssiDto.Perm_SiDtos |> Array.map (Perm_SiDto.toPerm_Si)
            let perm_Sis =
                perm_SisR 
                |> Array.map (function
                    | Ok ps -> ps
                    | Error e -> raise (ArgumentException(sprintf "Error converting Perm_SiDto: %A" e)))
            let mssi = GeneSort.Model.Sorter.Si.Mssi.create
                            (UMX.tag<sorterModelID> mssiDto.Id)
                            (UMX.tag<sortingWidth> mssiDto.Width)
                            perm_Sis
            Ok mssi
        with
        | :? ArgumentException as ex when ex.Message.Contains("Perm_Si") ->
            Error (InvalidPermSiCount ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("Width") ->
            Error (InvalidWidth ex.Message)
        | ex ->
            Error (InvalidPermSiCount ex.Message) // Fallback for unexpected errors