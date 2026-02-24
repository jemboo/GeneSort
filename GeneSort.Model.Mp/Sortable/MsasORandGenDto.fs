namespace GeneSort.Model.Mp.Sortable

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Sorting
open GeneSort.Core.Mp


[<MessagePackObject>]
type msasORandGenDto = {
    [<Key(0)>] id: Guid
    [<Key(1)>] rngFactoryDto: rngFactoryDto
    [<Key(2)>] sortingWidth: int
    [<Key(3)>] maxOrbit: int
}

module MsasORandGenDto =

    let fromDomain (msas: MsasORandGen) : msasORandGenDto =
        { id = %msas.Id; rngFactoryDto = msas.RngFactory |> RngFactoryDto.fromDomain; sortingWidth = int msas.SortingWidth; maxOrbit = msas.MaxOrbit }

    let toDomain (dto: msasORandGenDto) : MsasORandGen =
        if dto.sortingWidth < 0 then
            invalidArg "SortingWidth" "Sorting width must be non-negative."
        if dto.maxOrbit < 0 then
            invalidArg "MaxOrbit" "Max orbit must be non-negative."
        try
            let rngFactory = RngFactoryDto.toDomain dto.rngFactoryDto
            MsasORandGen.create rngFactory (UMX.tag<sortingWidth> dto.sortingWidth) dto.maxOrbit
        with
        | :? ArgumentException -> invalidArg "RngType" $"Invalid RngType: {dto.rngFactoryDto}."
