namespace GeneSort.Model.Mp.Sorter.Rs

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Mp.Sorter.Rs
open GeneSort.Model.Sorter.Rs
open GeneSort.Core.Mp.RatesAndOps
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Model.Sorter.Si
open GeneSort.Core.Mp

[<MessagePackObject>]
type MsrsRandMutateDto = 
    { [<Key(0)>] Msrs: MsrsDTO
      [<Key(1)>] RngType: rngType
      [<Key(2)>] OpsActionRatesArray: OpsActionRatesArrayDto }

module MsrsRandMutateDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toMsrsRandMutateDto (msrsRandMutate: MsrsRandMutate) : MsrsRandMutateDto =
        { Msrs = MsrsDTO.toMsrsDTO msrsRandMutate.Msrs
          RngType = msrsRandMutate.RngType
          OpsActionRatesArray = OpsActionRatesArrayDto.fromOpsActionRatesArray msrsRandMutate.OpsActionRates }

    let fromMsrsRandMutateDto (dto: MsrsRandMutateDto) : MsrsRandMutate =
        try
            let msrsResult = MsrsDTO.toMsrs dto.Msrs
            match msrsResult with
            | Ok msrs ->
                if %msrs.StageCount <> dto.OpsActionRatesArray.Rates.Length then
                    failwith $"StageCount ({%msrs.StageCount}) must match OpsActionRatesArray length ({dto.OpsActionRatesArray.Rates.Length})"
                MsrsRandMutate.create
                    (dto.RngType)
                    msrs
                    (OpsActionRatesArrayDto.toOpsActionRatesArray dto.OpsActionRatesArray)
            | Error err ->
                let msg = match err with
                          | MsrsDTO.NullPermRssArray m -> m
                          | MsrsDTO.EmptyPermRssArray m -> m
                          | MsrsDTO.InvalidWidth m -> m
                          | MsrsDTO.MismatchedPermRsOrder m -> m
                          | MsrsDTO.PermRsConversionError e ->
                              match e with
                              | Perm_RsDTO.Perm_RsDTOError.OrderTooSmall m -> m
                              | Perm_RsDTO.Perm_RsDTOError.OrderNotDivisibleByTwo m -> m
                              | Perm_RsDTO.Perm_RsDTOError.NotReflectionSymmetric m -> m
                              | Perm_RsDTO.Perm_RsDTOError.PermSiConversionError e' ->
                                  match e' with
                                  | Perm_SiDTO.Perm_SiDTOError.EmptyArray m -> m
                                  | Perm_SiDTO.Perm_SiDTOError.InvalidPermutation m -> m
                                  | Perm_SiDTO.Perm_SiDTOError.NotSelfInverse m -> m
                                  | Perm_SiDTO.Perm_SiDTOError.NullArray m -> m
                                  | Perm_SiDTO.Perm_SiDTOError.PermutationConversionError e'' ->
                                      match e'' with
                                      | PermutationDTO.PermutationDTOError.EmptyArray m -> m
                                      | PermutationDTO.PermutationDTOError.InvalidPermutation m -> m
                                      | PermutationDTO.PermutationDTOError.NullArray m -> m
                failwith $"Failed to convert MsrsDTO: {msg}"
        with
        | ex -> failwith $"Failed to convert MsrsRandMutateDto: {ex.Message}"