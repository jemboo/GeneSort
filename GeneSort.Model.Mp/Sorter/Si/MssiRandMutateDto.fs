namespace GeneSort.Model.Mp.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open GeneSort.Component
open GeneSort.Model.Mp.Sorter.Si
open GeneSort.Model.Sorter.Si
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type mssiRandMutateDto = 
    { [<Key(0)>] mssiDto: mssiDto
      [<Key(1)>] rngType: rngType
      [<Key(2)>] opActionRatesArrayDto: OpActionRatesArrayDto }

module MssiRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMssiRandMutateDto (mssiRandMutate: MssiRandMutate) : mssiRandMutateDto =
        { mssiDto = MssiDto.toMssiDto mssiRandMutate.Mssi
          rngType = mssiRandMutate.RngType
          opActionRatesArrayDto = OpActionRatesArrayDto.fromDomain mssiRandMutate.OpActionRates }

    let fromMssiRandMutateDto (dto: mssiRandMutateDto) : Result<MssiRandMutate, string> =
        try
            let mssiResult = MssiDto.toMssi dto.mssiDto
            match mssiResult with
            | Ok mssi ->
                if %mssi.StageLength <> (OpActionRatesArrayDto.toDomain dto.opActionRatesArrayDto).Length then
                    Error "StageLength must match OpActionRatesArray.Length"
                else
                    let mssiRandMutate = 
                        MssiRandMutate.create
                            (dto.rngType)
                            mssi
                            (OpActionRatesArrayDto.toDomain dto.opActionRatesArrayDto)
                    Ok mssiRandMutate
            | Error err ->
                Error (match err with
                       | MssiDto.InvalidPermSiCount msg -> msg
                       | MssiDto.InvalidWidth msg -> msg)
        with
        | ex -> Error ex.Message