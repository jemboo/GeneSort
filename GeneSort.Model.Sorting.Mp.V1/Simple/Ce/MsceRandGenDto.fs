namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Core.Mp

type msceRandGenDto = 
    { sortingWidth: int
      rngFactoryDto: rngFactoryDto
      ceLength: int
      excludeSelfCe: bool }

module MsceRandGenDto =

    let fromDomain (msceRandGen: msceRandGen) : msceRandGenDto =
        { sortingWidth = %msceRandGen.SortingWidth
          ceLength = %msceRandGen.CeLength
          rngFactoryDto = msceRandGen.RngFactory |> RngFactoryDto.fromDomain
          excludeSelfCe = %msceRandGen.ExcludeSelfCe }

    let toDomain (dto: msceRandGenDto) : msceRandGen =
        if dto.sortingWidth < 1 then
            failwith "SortingWidth must be at least 1"
        if dto.ceLength < 1 then
            failwith "CeCount must be at least 1"
        if dto.excludeSelfCe && dto.sortingWidth < 2 then
            failwith "SortingWidth must be at least 2 when ExcludeSelfCe is true"
        msceRandGen.create 
                    (dto.rngFactoryDto |> RngFactoryDto.toDomain) 
                    (UMX.tag<sortingWidth> dto.sortingWidth) 
                    (dto.excludeSelfCe |> UMX.tag) 
                    (UMX.tag<ceLength> dto.ceLength)

