namespace GeneSort.Model.Mp.Sorter.Si

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Mp.Sorter.Si
open GeneSort.Model.Sorter.Si
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp

[<MessagePackObject>]
type MssiRandMutateDto = 
    { [<Key(0)>] Mssi: MssiDto
      [<Key(1)>] RngType: rngType
      [<Key(2)>] OpActionRatesArray: OpActionRatesArrayDto }

module MssiRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMssiRandMutateDto (mssiRandMutate: MssiRandMutate) : MssiRandMutateDto =
        { Mssi = MssiDto.toMssiDto mssiRandMutate.Mssi
          RngType = mssiRandMutate.RngType
          OpActionRatesArray = OpActionRatesArrayDto.fromDomain mssiRandMutate.OpActionRates }

    let fromMssiRandMutateDto (dto: MssiRandMutateDto) : Result<MssiRandMutate, string> =
        try
            let mssiResult = MssiDto.toMssi dto.Mssi
            match mssiResult with
            | Ok mssi ->
                if %mssi.StageCount <> (OpActionRatesArrayDto.fromDomain dto.OpActionRatesArray).Length then
                    Error "StageCount must match OpActionRatesArray.Length"
                else
                    let mssiRandMutate = 
                        MssiRandMutate.create
                            (dto.RngType)
                            mssi
                            (OpActionRatesArrayDto.fromDomain dto.OpActionRatesArray)
                    Ok mssiRandMutate
            | Error err ->
                Error (match err with
                       | MssiDto.InvalidPermSiCount msg -> msg
                       | MssiDto.InvalidWidth msg -> msg)
        with
        | ex -> Error ex.Message