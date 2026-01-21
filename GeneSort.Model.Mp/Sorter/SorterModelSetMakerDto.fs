namespace GeneSort.Model.Mp.Sorter

open System
open GeneSort.Model.Sorter
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open FSharp.UMX
open GeneSort.Sorting


[<MessagePackObject>]
type sorterModelSetMakerDto = 
    { 
        [<Key(0)>] Id: Guid
        [<Key(1)>] SorterModelMaker: SorterModelMakerDto
        [<Key(2)>] FirstIndex: int
        [<Key(3)>] Count: int 
    }

module SorterModelSetMakerDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toDomain (dto: sorterModelSetMakerDto) : sorterModelSetMaker =
        if dto.Count <= 0 then
            failwith "Count must be greater than 0"
        if dto.FirstIndex < 0 then
            failwith "FirstIndex must be non-negative"
        let sorterModelMaker = SorterModelMakerDto.fromSorterModelMakerDto dto.SorterModelMaker

        sorterModelSetMaker.create
            sorterModelMaker
            (dto.FirstIndex |> UMX.tag<sorterCount>)
            (dto.Count |> UMX.tag<sorterCount>)

    let fromDomain (domain: sorterModelSetMaker) : sorterModelSetMakerDto =
        { Id = %domain.Id
          SorterModelMaker = SorterModelMakerDto.toSorterModelMakerDto domain.SorterModelMaker
          FirstIndex = %domain.FirstIndex
          Count = %domain.Count }