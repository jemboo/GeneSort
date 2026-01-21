namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Sorting
open GeneSort.Core


[<MessagePackObject>]
type MsasORandGenDto = {
    [<Key(0)>] Id: Guid
    [<Key(1)>] RngType: rngType
    [<Key(2)>] SortingWidth: int
    [<Key(3)>] MaxOrbit: int
}

module MsasORandGenDto =

    let fromDomain (msas: MsasORandGen) : MsasORandGenDto =
        { Id = %msas.Id; RngType = msas.RngType; SortingWidth = int msas.SortingWidth; MaxOrbit = msas.MaxOrbit }

    let toDomain (dto: MsasORandGenDto) : MsasORandGen =
        if dto.SortingWidth < 0 then
            invalidArg "SortingWidth" "Sorting width must be non-negative."
        if dto.MaxOrbit < 0 then
            invalidArg "MaxOrbit" "Max orbit must be non-negative."
        try
            MsasORandGen.create dto.RngType (UMX.tag<sortingWidth> dto.SortingWidth) dto.MaxOrbit
        with
        | :? ArgumentException -> invalidArg "RngType" $"Invalid RngType: {dto.RngType}."
