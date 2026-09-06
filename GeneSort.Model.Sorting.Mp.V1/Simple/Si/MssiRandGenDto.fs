
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Core.Mp

type mssiRandGenDto = 
    { sortingWidth: int
      rngFactoryDto: rngFactoryDto
      stageLength: int }

module MssiRandGenDto =

    let fromDomain (mssiRandGen: mssiRandGen) : mssiRandGenDto =
        { sortingWidth = %mssiRandGen.SortingWidth
          rngFactoryDto = mssiRandGen.RngFactory |> RngFactoryDto.fromDomain
          stageLength = %mssiRandGen.StageLength }

    let toDomain (dto: mssiRandGenDto) : Result<mssiRandGen, string> =
        try
            if dto.sortingWidth < 2 then
                Error "SortingWidth must be at least 2"
            else if dto.stageLength < 1 then
                Error "StageLength must be at least 1"
            else
                let mssiRandGen = 
                    mssiRandGen.create
                        (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                        (UMX.tag<sortingWidth> dto.sortingWidth)
                        (UMX.tag<stageLength> dto.stageLength)
                Ok mssiRandGen
        with
        | ex -> Error ex.Message
