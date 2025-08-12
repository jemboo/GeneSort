namespace GeneSort.Model.Mp.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter.Si
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type MssiRandGenDto = 
    { [<Key(0)>] SortingWidth: int
      [<Key(1)>] RngType: rngType
      [<Key(2)>] StageCount: int }

module MssiRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMssiRandGenDto (mssiRandGen: MssiRandGen) : MssiRandGenDto =
        { SortingWidth = %mssiRandGen.SortingWidth
          RngType = mssiRandGen.RngType
          StageCount = %mssiRandGen.StageCount }

    let fromMssiRandGenDto (dto: MssiRandGenDto) : Result<MssiRandGen, string> =
        try
            if dto.SortingWidth < 2 then
                Error "SortingWidth must be at least 2"
            else if dto.StageCount < 1 then
                Error "StageCount must be at least 1"
            else
                let mssiRandGen = 
                    MssiRandGen.create
                        (dto.RngType)
                        (UMX.tag<sortingWidth> dto.SortingWidth)
                        (UMX.tag<stageCount> dto.StageCount)
                Ok mssiRandGen
        with
        | ex -> Error ex.Message