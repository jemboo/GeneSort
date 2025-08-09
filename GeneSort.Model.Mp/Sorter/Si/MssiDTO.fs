namespace GeneSort.Model.Mp.Sorter.Si

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sorter.Si
open MessagePack

[<MessagePackObject; Struct>]
type MssiDTO =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] Width: int
      [<Key(2)>] Perm_Sis: Perm_Si array }
    
    static member Create(id: Guid, width: int, perm_Sis: Perm_Si array) : Result<MssiDTO, string> =
        if isNull perm_Sis || perm_Sis.Length < 1 then
            Error "Must have at least 1 Perm_Si"
        else if width < 1 then
            Error "Width must be at least 1"
        else
            Ok { Id = id; Width = width; Perm_Sis = perm_Sis }
    
    member this.StageCount = this.Perm_Sis.Length

module MssiDTO =
    type MssiDTOError =
        | InvalidPermSiCount of string
        | InvalidWidth of string

    let toMssiDTO (mssi: Mssi) : MssiDTO =
        { Id = %mssi.Id
          Width = %mssi.SortingWidth
          Perm_Sis = mssi.Perm_Sis }

    let toMssi (mssiDTO: MssiDTO) : Result<Mssi, MssiDTOError> =
        try
            let mssi = GeneSort.Model.Sorter.Si.Mssi.create
                            (UMX.tag<sorterModelID> mssiDTO.Id)
                            (UMX.tag<sortingWidth> mssiDTO.Width)
                            mssiDTO.Perm_Sis
            Ok mssi
        with
        | :? ArgumentException as ex when ex.Message.Contains("Perm_Si") ->
            Error (InvalidPermSiCount ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("Width") ->
            Error (InvalidWidth ex.Message)
        | ex ->
            Error (InvalidPermSiCount ex.Message) // Fallback for unexpected errors