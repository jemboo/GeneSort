namespace GeneSort.Model.Mp.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter.Si
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type mssiRandGenDto = 
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngType: rngType
      [<Key(2)>] stageLength: int }

module MssiRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMssiRandGenDto (mssiRandGen: MssiRandGen) : mssiRandGenDto =
        { sortingWidth = %mssiRandGen.SortingWidth
          rngType = mssiRandGen.RngType
          stageLength = %mssiRandGen.StageCount }

    let fromMssiRandGenDto (dto: mssiRandGenDto) : Result<MssiRandGen, string> =
        try
            if dto.sortingWidth < 2 then
                Error "SortingWidth must be at least 2"
            else if dto.stageLength < 1 then
                Error "StageLength must be at least 1"
            else
                let mssiRandGen = 
                    MssiRandGen.create
                        (dto.rngType)
                        (UMX.tag<sortingWidth> dto.sortingWidth)
                        (UMX.tag<stageLength> dto.stageLength)
                Ok mssiRandGen
        with
        | ex -> Error ex.Message