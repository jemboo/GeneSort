namespace GeneSort.Model.Mp.Sorter.Rs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Model.Mp.Sorter.Rs
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorter.Si
open GeneSort.Core.Mp

[<MessagePackObject>]
type MsrsRandMutateDto = 
    { [<Key(0)>] Msrs: MsrsDto
      [<Key(1)>] RngType: rngType
      [<Key(2)>] OpsActionRatesArray: OpsActionRatesArrayDto }

module MsrsRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsrsRandMutateDto (msrsRandMutate: MsrsRandMutate) : MsrsRandMutateDto =
        { Msrs = MsrsDto.toMsrsDto msrsRandMutate.Msrs
          RngType = msrsRandMutate.RngType
          OpsActionRatesArray = OpsActionRatesArrayDto.toDomain msrsRandMutate.OpsActionRates }

    let fromMsrsRandMutateDto (dto: MsrsRandMutateDto) : MsrsRandMutate =
        try
            let msrsResult = MsrsDto.toMsrs dto.Msrs
            match msrsResult with
            | Ok msrs ->
                if %msrs.StageCount <> dto.OpsActionRatesArray.Rates.Length then
                    failwith $"StageCount ({%msrs.StageCount}) must match OpsActionRatesArray length ({dto.OpsActionRatesArray.Rates.Length})"
                MsrsRandMutate.create
                    (dto.RngType)
                    msrs
                    (OpsActionRatesArrayDto.fromDomain dto.OpsActionRatesArray)
            | Error err ->
                let msg = match err with
                          | MsrsDto.NullPermRssArray m -> m
                          | MsrsDto.EmptyPermRssArray m -> m
                          | MsrsDto.InvalidWidth m -> m
                          | MsrsDto.MismatchedPermRsOrder m -> m
                          | MsrsDto.PermRsConversionError e ->
                              match e with
                              | Perm_RsDto.Perm_RsDtoError.OrderTooSmall m -> m
                              | Perm_RsDto.Perm_RsDtoError.OrderNotDivisibleByTwo m -> m
                              | Perm_RsDto.Perm_RsDtoError.NotReflectionSymmetric m -> m
                              | Perm_RsDto.Perm_RsDtoError.PermSiConversionError e' ->
                                  match e' with
                                  | Perm_SiDto.Perm_SiDtoError.EmptyArray m -> m
                                  | Perm_SiDto.Perm_SiDtoError.InvalidPermutation m -> m
                                  | Perm_SiDto.Perm_SiDtoError.NotSelfInverse m -> m
                                  | Perm_SiDto.Perm_SiDtoError.NullArray m -> m
                                  | Perm_SiDto.Perm_SiDtoError.PermutationConversionError e'' ->
                                      match e'' with
                                      | PermutationDto.PermutationDtoError.EmptyArray m -> m
                                      | PermutationDto.PermutationDtoError.InvalidPermutation m -> m
                                      | PermutationDto.PermutationDtoError.NullArray m -> m
                failwith $"Failed to convert MsrsDto: {msg}"
        with
        | ex -> failwith $"Failed to convert MsrsRandMutateDto: {ex.Message}"