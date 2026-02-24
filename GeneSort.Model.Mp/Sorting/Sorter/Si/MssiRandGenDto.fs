namespace GeneSort.Model.Mp.Sorting.Sorter.Si

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Si
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp

[<MessagePackObject>]
type mssiRandGenDto = 
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] stageLength: int }

module MssiRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

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