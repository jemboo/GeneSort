namespace GeneSort.Model.Mp.Sorter.Ce

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorter.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps

[<MessagePackObject>]
type MsceRandMutateDto = 
    { [<Key(0)>] Msce: msceDto
      [<Key(1)>] RngType: rngType
      [<Key(2)>] IndelRatesArray: IndelRatesArrayDto
      [<Key(3)>] ExcludeSelfCe: bool }

module MsceRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsceRandMutateDto (msceRandMutate: MsceRandMutate) : MsceRandMutateDto =
        { Msce = MsceDto.toMsceDto msceRandMutate.Msce
          RngType = msceRandMutate.RngType
          IndelRatesArray = IndelRatesArrayDto.fromDomain msceRandMutate.IndelRatesArray
          ExcludeSelfCe = msceRandMutate.ExcludeSelfCe }

    let fromMsceRandMutateDto (dto: MsceRandMutateDto) : Result<MsceRandMutate, string> =
        try
            let msceResult = MsceDto.toMsce dto.Msce
            match msceResult with
            | Ok msce ->
                if %msce.CeLength <> (IndelRatesArrayDto.toDomain dto.IndelRatesArray).Length then
                    Error "CeCount must match IndelRatesArray.Length"
                else
                    let msceRandMutate = 
                        MsceRandMutate.create
                            (dto.RngType)
                            (IndelRatesArrayDto.toDomain dto.IndelRatesArray)
                            (dto.ExcludeSelfCe)
                            msce
                    Ok msceRandMutate
            | Error err ->
                Error (match err with
                       | MsceDto.InvalidCeCodesLength msg -> msg
                       | MsceDto.InvalidSortingWidth msg -> msg)
        with
        | ex -> Error ex.Message