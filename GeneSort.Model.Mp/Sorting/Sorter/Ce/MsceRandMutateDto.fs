namespace GeneSort.Model.Mp.Sorting.Sorter.Ce
open FSharp.UMX
open GeneSort.Model.Sorting.Sorter.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

[<MessagePackObject>]
type msceRandMutateDto = 
    { [<Key(0)>] Msce: msceDto
      [<Key(1)>] rngFactoryDto: rngFactoryDto
      [<Key(2)>] IndelRatesArray: IndelRatesArrayDto
      [<Key(3)>] ExcludeSelfCe: bool }

module MsceRandMutateDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    
    let fromDomain (msceRandMutate: msceRandMutate) : msceRandMutateDto =
        { Msce = MsceDto.fromDomain msceRandMutate.Msce
          rngFactoryDto = msceRandMutate.RngFactory |> RngFactoryDto.fromDomain
          IndelRatesArray = IndelRatesArrayDto.fromDomain msceRandMutate.IndelRatesArray
          ExcludeSelfCe = msceRandMutate.ExcludeSelfCe }
    
    let toDomain (dto: msceRandMutateDto) : msceRandMutate =
        match MsceDto.toDomain dto.Msce with
        | Ok msce ->
            let indelRatesArray = IndelRatesArrayDto.toDomain dto.IndelRatesArray
        
            if %msce.CeLength <> indelRatesArray.Length then
                failwith "CeCount must match IndelRatesArray.Length"
        
            msceRandMutate.create
                (dto.rngFactoryDto |> RngFactoryDto.toDomain)
                indelRatesArray
                dto.ExcludeSelfCe
                msce
        | Error err ->
            failwith (match err with
                      | MsceDto.InvalidCeCodesLength msg -> msg
                      | MsceDto.InvalidSortingWidth msg -> msg)