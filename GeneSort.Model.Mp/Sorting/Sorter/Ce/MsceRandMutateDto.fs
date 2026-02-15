namespace GeneSort.Model.Mp.Sorting.Sorter.Ce
open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps

[<MessagePackObject>]
type msceRandMutateDto = 
    { [<Key(0)>] Msce: msceDto
      [<Key(1)>] RngType: rngType
      [<Key(2)>] IndelRatesArray: IndelRatesArrayDto
      [<Key(3)>] ExcludeSelfCe: bool }

module MsceRandMutateDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    
    let toMsceRandMutateDto (msceRandMutate: msceRandMutate) : msceRandMutateDto =
        { Msce = MsceDto.toMsceDto msceRandMutate.Msce
          RngType = msceRandMutate.RngType
          IndelRatesArray = IndelRatesArrayDto.fromDomain msceRandMutate.IndelRatesArray
          ExcludeSelfCe = msceRandMutate.ExcludeSelfCe }
    
    let fromMsceRandMutateDto (dto: msceRandMutateDto) : msceRandMutate =
        match MsceDto.toMsce dto.Msce with
        | Ok msce ->
            let indelRatesArray = IndelRatesArrayDto.toDomain dto.IndelRatesArray
        
            if %msce.CeLength <> indelRatesArray.Length then
                failwith "CeCount must match IndelRatesArray.Length"
        
            msceRandMutate.create
                dto.RngType
                indelRatesArray
                dto.ExcludeSelfCe
                msce
        | Error err ->
            failwith (match err with
                      | MsceDto.InvalidCeCodesLength msg -> msg
                      | MsceDto.InvalidSortingWidth msg -> msg)