﻿namespace GeneSort.Model.Mp.Sorter.Rs

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
type msrsRandMutateDto = 
    { [<Key(0)>] msrsDto: msrsDto
      [<Key(1)>] rngType: rngType
      [<Key(2)>] opsActionRatesArray: opsActionRatesArrayDto }

module MsrsRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsrsRandMutateDto (msrsRandMutate: msrsRandMutate) : msrsRandMutateDto =
        { msrsDto = MsrsDto.toMsrsDto msrsRandMutate.Msrs
          rngType = msrsRandMutate.RngType
          opsActionRatesArray = OpsActionRatesArrayDto.fromDomain msrsRandMutate.OpsActionRates }

    let fromMsrsRandMutateDto (dto: msrsRandMutateDto) : msrsRandMutate =
        try
            let msrsResult = MsrsDto.toMsrs dto.msrsDto
            match msrsResult with
            | Ok msrs ->
                if %msrs.StageLength <> dto.opsActionRatesArray.Rates.Length then
                    failwith $"StageLength ({%msrs.StageLength}) must match OpsActionRatesArray length ({dto.opsActionRatesArray.Rates.Length})"
                msrsRandMutate.create
                    (dto.rngType)
                    msrs
                    (OpsActionRatesArrayDto.toDomain dto.opsActionRatesArray)
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
                                  | PermSiDto.Perm_SiDtoError.EmptyArray m -> m
                                  | PermSiDto.Perm_SiDtoError.InvalidPermutation m -> m
                                  | PermSiDto.Perm_SiDtoError.NotSelfInverse m -> m
                                  | PermSiDto.Perm_SiDtoError.NullArray m -> m
                                  | PermSiDto.Perm_SiDtoError.PermutationConversionError e'' ->
                                      match e'' with
                                      | PermutationDto.PermutationDtoError.EmptyArray m -> m
                                      | PermutationDto.PermutationDtoError.InvalidPermutation m -> m
                                      | PermutationDto.PermutationDtoError.NullArray m -> m
                failwith $"Failed to convert MsrsDto: {msg}"
        with
        | ex -> failwith $"Failed to convert MsrsRandMutateDto: {ex.Message}"