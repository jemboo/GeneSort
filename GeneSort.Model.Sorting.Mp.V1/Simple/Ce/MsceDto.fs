namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce

open System
open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Sorting

type msceDto =
    { id: Guid
      sortingWidth: int
      ceCodes: int array }
    
    static member Create(id: Guid, sortingWidth: int, ceCodes: int array) : msceDto =
        if isNull ceCodes || ceCodes.Length < 1 then
            invalidArg "ceCodes" "Must be at least 1 Ce"
        if sortingWidth < 1 then
            invalidArg "sortingWidth" "SortingWidth must be at least 1"
        
        { id = id; sortingWidth = sortingWidth; ceCodes = ceCodes }

module MsceDto =

    let fromDomain (msce: msce) : msceDto =
        { id = %msce.Id
          sortingWidth = %msce.SortingWidth
          ceCodes = msce.CeCodes }

    let toDomain (msceDto: msceDto) : msce =
        msce.create
            (UMX.tag<sorterModelId> msceDto.id)
            (UMX.tag<sortingWidth> msceDto.sortingWidth)
            msceDto.ceCodes
