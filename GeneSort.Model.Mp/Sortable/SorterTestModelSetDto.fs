namespace GeneSort.Model.Mp.Sortable

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Core.Mp
open GeneSort.Sorter


[<MessagePackObject>]
type SorterTestModelSetDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] SorterTestModels: SorterTestModelDto[]
}

module SorterTestModelSetDto =

    let toDtoMsasORandGen (msas: MsasORandGen) : MsasORandGenDto =
        { Id = %msas.Id; RngType = msas.RngType; SortingWidth = int msas.SortingWidth; MaxOrbit = msas.MaxOrbit }

    let fromDtoMsasORandGen (dto: MsasORandGenDto) : MsasORandGen =
        if dto.SortingWidth < 0 then
            invalidArg "SortingWidth" "Sorting width must be non-negative."
        if dto.MaxOrbit < 0 then
            invalidArg "MaxOrbit" "Max orbit must be non-negative."
        MsasORandGen.create dto.RngType (UMX.tag<sortingWidth> dto.SortingWidth) dto.MaxOrbit