
namespace GeneSort.Model.Mp.Sorting.Sorter.Ce

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.Sorter.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp


[<MessagePackObject>]
type msceRandGenDto = 
    { [<Key(0)>] sortingWidth: int
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] ceLength: int
      [<Key(3)>] excludeSelfCe: bool }

module MsceRandGenDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (msceRandGen: msceRandGen) : msceRandGenDto =
        { sortingWidth = %msceRandGen.SortingWidth
          ceLength = %msceRandGen.CeLength
          rngFactoryDto = msceRandGen.RngFactory |> RngFactoryDto.fromDomain
          excludeSelfCe = msceRandGen.ExcludeSelfCe }

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
                    (dto.excludeSelfCe) 
                    (UMX.tag<ceLength> dto.ceLength)


