
namespace GeneSort.Model.Mp.Sorting.Sorter.Ce

open System
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting.Sorter.Ce
open GeneSort.Model.Sorting
open GeneSort.Sorting

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

module MsceDto =
    type MsceDtoError =
        | InvalidCeCodesLength of string
        | InvalidSortingWidth of string

    let fromDomain (msce: msce) : msceDto =
        { Id = %msce.Id
          sortingWidth = %msce.SortingWidth
          ceCodes = msce.CeCodes }

    let toDomain (msceDto: msceDto) : msce =
            GeneSort.Model.Sorting.Sorter.Ce.msce.create
                (UMX.tag<sorterModelId> msceDto.Id)
                (UMX.tag<sortingWidth> msceDto.sortingWidth)
                msceDto.ceCodes
