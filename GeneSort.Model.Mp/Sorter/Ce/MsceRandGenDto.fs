
namespace GeneSort.Model.Mp.Sorter.Ce

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp


[<MessagePackObject>]
type MsceRandGenDto = 
    { [<Key(0)>] SortingWidth: int
      [<Key(1)>] RngType: rngType
      [<Key(2)>] CeCount: int
      [<Key(3)>] ExcludeSelfCe: bool }

module MsceRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsceRandGenDto (msceRandGen: MsceRandGen) : MsceRandGenDto =
        { SortingWidth = %msceRandGen.SortingWidth
          CeCount = %msceRandGen.CeCount
          RngType = msceRandGen.RngType
          ExcludeSelfCe = msceRandGen.ExcludeSelfCe }

    let fromMsceRandGenDto (dto: MsceRandGenDto) : MsceRandGen =
        if dto.SortingWidth < 1 then
            failwith "SortingWidth must be at least 1"
        if dto.CeCount < 1 then
            failwith "CeCount must be at least 1"
        if dto.ExcludeSelfCe && dto.SortingWidth < 2 then
            failwith "SortingWidth must be at least 2 when ExcludeSelfCe is true"
        MsceRandGen.create (dto.RngType) (UMX.tag<sortingWidth> dto.SortingWidth) (dto.ExcludeSelfCe) (UMX.tag<ceCount> dto.CeCount)


