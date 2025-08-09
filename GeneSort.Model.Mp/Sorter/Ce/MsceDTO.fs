
namespace GeneSort.Model.Mp.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Model.Sorter.Ce

open MessagePack
open GeneSort.Model.Sorter

[<MessagePackObject; Struct>]
type MsceDTO =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] SortingWidth: int
      [<Key(2)>] CeCodes: int array }
    
    static member Create(id: Guid, sortingWidth: int, ceCodes: int array) : MsceDTO =
        if isNull ceCodes || ceCodes.Length < 1 then
            invalidArg "ceCodes" "Must be at least 1 Ce"
        if sortingWidth < 1 then
            invalidArg "sortingWidth" "SortingWidth must be at least 1"
        
        { Id = id; SortingWidth = sortingWidth; CeCodes = ceCodes }
    
    member this.CeCount = this.CeCodes.Length


module MsceDTO =
    type MsceDTOError =
        | InvalidCeCodesLength of string
        | InvalidSortingWidth of string

    let toMsceDTO (msce: Msce) : MsceDTO =
        { Id = %msce.Id
          SortingWidth = %msce.SortingWidth
          CeCodes = msce.CeCodes }

    let toMsce (msceDTO: MsceDTO) : Result<Msce, MsceDTOError> =
        try
            let msce = GeneSort.Model.Sorter.Ce.Msce.create
                            (UMX.tag<sorterModelID> msceDTO.Id)
                            (UMX.tag<sortingWidth> msceDTO.SortingWidth)
                            msceDTO.CeCodes
            Ok msce
        with
        | :? ArgumentException as ex when ex.Message.Contains("Ce") ->
            Error (InvalidCeCodesLength ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("SortingWidth") ->
            Error (InvalidSortingWidth ex.Message)
        | ex ->
            Error (InvalidCeCodesLength ex.Message) // Fallback for unexpected errors
