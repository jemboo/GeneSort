namespace GeneSort.Model.Mp.Sorting.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Sorting.Sorter.Si
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type mssiRandMutateDto = 
    { [<Key(0)>] mssiDto: mssiDto
      [<Key(1)>] rngType: rngType
      [<Key(2)>] opActionRatesArrayDto: opActionRatesArrayDto }

module MssiRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let fromDomain (mssiRandMutate: mssiRandMutate) : mssiRandMutateDto =
        { mssiDto = MssiDto.fromDomain mssiRandMutate.Mssi
          rngType = mssiRandMutate.RngType
          opActionRatesArrayDto = OpActionRatesArrayDto.fromDomain mssiRandMutate.OpActionRates }

    let toDomain (dto: mssiRandMutateDto) : Result<mssiRandMutate, string> =
        try
            let mssiResult = MssiDto.toDomain dto.mssiDto
            match mssiResult with
            | Ok mssi ->
                if %mssi.StageLength <> (OpActionRatesArrayDto.toDomain dto.opActionRatesArrayDto).Length then
                    Error "StageLength must match OpActionRatesArray.Length"
                else
                    let mssiRandMutate = 
                        mssiRandMutate.create
                            (dto.rngType)
                            (OpActionRatesArrayDto.toDomain dto.opActionRatesArrayDto)
                            mssi
                    Ok mssiRandMutate
            | Error err ->
                Error (match err with
                       | MssiDto.InvalidPermSiCount msg -> msg
                       | MssiDto.InvalidWidth msg -> msg)
        with
        | ex -> Error ex.Message