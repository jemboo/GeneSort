namespace GeneSort.Model.Mp.Sorting

//open System
//open GeneSort.Model.Sorting
//open MessagePack
//open MessagePack.Resolvers
//open MessagePack.FSharp
//open FSharp.UMX
//open GeneSort.Sorting
//open GeneSort.Model.Mp.Sorter


//[<MessagePackObject>]
//type sortingModelSetMakerDto = 
//    { 
//        [<Key(0)>] Id: Guid
//        [<Key(1)>] SorterModelMakerDto: SorterModelMakerDto
//        [<Key(2)>] FirstIndex: int
//        [<Key(3)>] Count: int 
//    }

//module SorterModelSetMakerDto =

//    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
//    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

//    let toDomain (dto: sortingModelSetMakerDto) : sortingModelSetMaker =
//        if dto.Count <= 0 then
//            failwith "Count must be greater than 0"
//        if dto.FirstIndex < 0 then
//            failwith "FirstIndex must be non-negative"
//        let sorterModelMaker = SorterModelMakerDto.fromSorterModelMakerDto dto.SorterModelMaker

//        sortingModelSetMaker.create
//            sorterModelMaker
//            (dto.FirstIndex |> UMX.tag<sorterCount>)
//            (dto.Count |> UMX.tag<sorterCount>)

//    let fromDomain (domain: sorterModelSetMaker) : sorterModelSetMakerDto =
//        { Id = %domain.Id
//          SorterModelMakerDto = SorterModelMakerDto.toSorterModelMakerDto domain.SorterModelMaker
//          FirstIndex = %domain.FirstIndex
//          Count = %domain.Count }