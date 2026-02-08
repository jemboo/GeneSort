namespace GeneSort.Model.Mp.Sorter

open System
open FSharp.UMX
open MessagePack
open GeneSort.Model.Sorting

[<MessagePackObject>]
type sortingModelPairDto = {
    [<Key(0)>]
    Id: Guid
    [<Key(1)>]
    SorterPairModelDto: sorterPairModelDto
}

module SortingModelPairDto =
    
    let toSortingModelPairDto (model: sortingModelPair) : sortingModelPairDto =
        {
            Id = %model.Id
            SorterPairModelDto = SorterPairModelDto.toSorterModelDto model.SorterPairModel
        }
    
    let fromSortingModelPairDto (dto: sortingModelPairDto) : sortingModelPair =
        sortingModelPair.create
            (UMX.tag<sortingModelID> dto.Id)
            (SorterPairModelDto.fromSorterModelDto dto.SorterPairModelDto)
