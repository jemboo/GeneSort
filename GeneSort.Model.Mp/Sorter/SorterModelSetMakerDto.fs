namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
open GeneSort.Model.Mp.Sorter.Ce
open GeneSort.Model.Mp.Sorter.Si
open GeneSort.Model.Mp.Sorter.Rs
open GeneSort.Model.Mp.Sorter.Uf4
open GeneSort.Model.Mp.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open FSharp.UMX


[<MessagePackObject>]
type SorterModelSetMakerDto = 
    { 
        [<Key(0)>] Id: Guid
        [<Key(1)>] SorterModelMaker: SorterModelMakerDto
        [<Key(2)>] FirstIndex: int
        [<Key(3)>] Count: int 
    }

module SorterModelSetMakerDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toDomain (dto: SorterModelSetMakerDto) : SorterModelSetMaker =
        if dto.Count <= 0 then
            failwith "Count must be greater than 0"
        if dto.FirstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sorterModelMaker = SorterModelMakerDto.fromSorterModelMakerDto dto.SorterModelMaker

        SorterModelSetMaker.create
            sorterModelMaker
            dto.FirstIndex
            dto.Count

    let fromDomain (domain: SorterModelSetMaker) : SorterModelSetMakerDto =
        { Id = %domain.Id
          SorterModelMaker = SorterModelMakerDto.toSorterModelMakerDto domain.SorterModelMaker
          FirstIndex = domain.FirstIndex
          Count = domain.Count }