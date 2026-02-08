
namespace GeneSort.Model.Mp.Sorter.Ce

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp


[<MessagePackObject>]
type msceRandGenDto = 
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngType: rngType
      [<Key(2)>] ceLength: int
      [<Key(3)>] excludeSelfCe: bool }

module MsceRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsceRandGenDto (msceRandGen: MsceRandGen) : msceRandGenDto =
        { sortingWidth = %msceRandGen.SortingWidth
          ceLength = %msceRandGen.CeLength
          rngType = msceRandGen.RngType
          excludeSelfCe = msceRandGen.ExcludeSelfCe }

    let fromMsceRandGenDto (dto: msceRandGenDto) : MsceRandGen =
        if dto.sortingWidth < 1 then
            failwith "SortingWidth must be at least 1"
        if dto.ceLength < 1 then
            failwith "CeCount must be at least 1"
        if dto.excludeSelfCe && dto.sortingWidth < 2 then
            failwith "SortingWidth must be at least 2 when ExcludeSelfCe is true"
        MsceRandGen.create (dto.rngType) (UMX.tag<sortingWidth> dto.sortingWidth) (dto.excludeSelfCe) (UMX.tag<ceLength> dto.ceLength)


