namespace GeneSort.Model.Mp.Sorter

open System
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorter
open GeneSort.Model.Sorting

[<MessagePackObject>]
type sortingModelSingleDto = {
    [<Key(0)>]
    Id: Guid
    [<Key(1)>]
    SorterModelDto: sorterModelDto
}

module SortingModelSingleDto =
    
    let toSortingModelSingleDto (model: sortingModelSingle) : sortingModelSingleDto =
        {
            Id = %model.Id
            SorterModelDto = SorterModelDto.toSorterModelDto model.SorterModel
        }
    
    let fromSortingModelSingleDto (dto: sortingModelSingleDto) : sortingModelSingle =
        sortingModelSingle.create
            (UMX.tag<sortingModelID> dto.Id)
            (SorterModelDto.fromSorterModelDto dto.SorterModelDto)
