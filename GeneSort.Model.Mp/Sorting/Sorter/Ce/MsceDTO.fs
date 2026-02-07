
namespace GeneSort.Model.Mp.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorter.Ce

open MessagePack
open GeneSort.Model.Sorter

[<MessagePackObject; Struct>]
type msceDto =
    { [<Key(0)>] Id: Guid
      [<Key(1)>] sortingWidth: int
      [<Key(2)>] ceCodes: int array }
    
    static member Create(id: Guid, sortingWidth: int, ceCodes: int array) : msceDto =
        if isNull ceCodes || ceCodes.Length < 1 then
            invalidArg "ceCodes" "Must be at least 1 Ce"
        if sortingWidth < 1 then
            invalidArg "sortingWidth" "SortingWidth must be at least 1"
        
        { Id = id; sortingWidth = sortingWidth; ceCodes = ceCodes }
    
   // member this.ceLength = this.CeCodes.Length


module MsceDto =
    type MsceDtoError =
        | InvalidCeCodesLength of string
        | InvalidSortingWidth of string

    let toMsceDto (msce: Msce) : msceDto =
        { Id = %msce.Id
          sortingWidth = %msce.SortingWidth
          ceCodes = msce.CeCodes }

    let toMsce (msceDto: msceDto) : Result<Msce, MsceDtoError> =
        try
            let msce = GeneSort.Model.Sorter.Ce.Msce.create
                            (UMX.tag<sortingModelID> msceDto.Id)
                            (UMX.tag<sortingWidth> msceDto.sortingWidth)
                            msceDto.ceCodes
            Ok msce
        with
        | :? ArgumentException as ex when ex.Message.Contains("Ce") ->
            Error (InvalidCeCodesLength ex.Message)
        | :? ArgumentException as ex when ex.Message.Contains("SortingWidth") ->
            Error (InvalidSortingWidth ex.Message)
        | ex ->
            Error (InvalidCeCodesLength ex.Message) // Fallback for unexpected errors
